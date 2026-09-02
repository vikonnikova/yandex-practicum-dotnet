# Event Booking

Система бронирования билетов.

## Состав системы

| Сервис       | Назначение                                  | HTTP (http)             | HTTPS                    | База (Postgres) | Контейнер в Docker     |
|--------------|---------------------------------------------|-------------------------|--------------------------|-----------------|------------------------|
| **Auth**     | Регистрация, вход, пользователи, выдача JWT | `http://localhost:5125` | `https://localhost:7090` | `users`         | `eventapi-users-db`    |
| **Events**   | CRUD событий, учёт свободных мест           | `http://localhost:5130` | `https://localhost:7262` | `events`        | `eventapi-events-db`   |
| **Bookings** | Заявки на бронь, подтверждение в фоне       | `http://localhost:5063` | `https://localhost:7275` | `bookings`      | `eventapi-bookings-db` |

Дополнительно:

| Компонент     | Порт на хосте          | Назначение                           |
|---------------|------------------------|--------------------------------------|
| **Kafka**     | `9092`                 | Топик `booking-confirmed`            |
| **Zookeeper** | `2181` (внутри Docker) | Координация брокера                  |
| **Kafka UI**  | `8082`                 | Визуальный веб-интерфейс брокера     |

Строки подключения в `appsettings.Development.json` смотрят на `localhost:5432` и разные имена БД (`users` / `events` / `bookings`). Kafka: `localhost:9092`. Consumer group Events: `events-service-group`.

## Поток BookingConfirmed

Контракт `BookingConfirmedEvent` (`src/Shared/Contracts`): `BookingId`, `EventId`, `UserId`, `SeatsCount` (сейчас всегда `1`), `ConfirmedAt`. Топик: **`booking-confirmed`**.

1. Пользователь создаёт заявку: `POST /bookings/{eventId}` в **Bookings**. Бронь сохраняется со статусом **Pending**.
2. Фоновый `BookingBackgroundService` периодически забирает pending-брони, ждёт ~10 секунд, вызывает `Confirm` и `KafkaPublisher.PublishBookingConfirmedAsync`.
3. При старте сервиса **Events** `KafkaTopicInitializer` создаёт топик при необходимости. `BookingConfirmedConsumer` (группа `events-service-group`) читает сообщения.
4. **Events** ищет событие по `EventId`, вызывает `TryReserveSeats(SeatsCount)` и сохраняет уменьшение `AvailableSeats`. Нет события или нет мест — ошибка в лог, сообщение пропускается.

## ⚙️ Архитектура

Система поделена на три сервиса:
1) Auth.Service - регистрация, вход и выдача JWT-токена.
2) Events.Service - управление событиями (CRUD) и учёт доступных мест.
3) Bookings.Service - создание и отмена броней.

Общие контракты лежат в `src/Shared`.

Архитектура каждого сервиса основана на принципах **Clean Architecture**:
- в основе лежит слой **Domain**, который содержит изолированную бизнес-логику, сущности и бизнес-правила;
- слой **Application** оркеструет эти бизнес-правила, реализуя пользовательские сценарии (Use Cases) и отвечая за логику приложения;
- **Infrastructure** отвечает за взаимодействие с внешними системами, включая базы данных, кэширование и сторонние сервисы;
- слой **Api** (или **Presentation**) является точкой входа в приложение, принимая HTTP-запросы и возвращая ответы пользователю.

Зависимости в проекте направлены строго внутрь (Domain → Application → Infrastructure → Api), что гарантирует независимость и легкую тестируемость ядра системы.

### Сущности доменного слоя

#### Auth.Service
User (Пользователь)

| Поле           | Тип      | Описание                                                              |
|----------------|----------|-----------------------------------------------------------------------|
| Id             | Guid     | Идентификатор                                                         |
| Login          | string   | Логин                                                                 |
| PasswordHash   | string   | Хэш пароля                                                            |
| Role           | UserRole | Роль пользователя: Admin (администратор), User (обычный пользователь) |

#### Events.Service
Event (Событие)

| Поле           | Тип      | Описание                                   |
|----------------|----------|--------------------------------------------|
| Id             | Guid     | Идентификатор                              |
| Title          | string   | Наименование                               |
| Description    | string   | Описание                                   |
| StartAt        | DateTime | Дата начала                                |
| EndAt          | DateTime | Дата окончания                             |
| TotalSeats     | int      | Общее количество мест                      |
| AvailableSeats | int      | Доступное количество мест для бронирования |

#### Bookings.Service
Booking (Бронь)

| Поле        | Тип           | Описание                                                                                                                        |
|-------------|---------------|---------------------------------------------------------------------------------------------------------------------------------|
| Id          | Guid          | Идентификатор брони                                                                                                             |
| EventId     | Guid          | Идентификатор события                                                                                                           |
| UserId      | Guid          | Идентификатор пользователя                                                                                                      |
| Status      | BookingStatus | Статус бронирования: Pending (создана, ожидает обработки), Confirmed (подтверждена), Rejected (отклонена), Cancelled (отменена) |
| CreatedAt   | DateTime      | Дата и время создания брони                                                                                                     |
| ProcessedAt | DateTime      | Дата и время обработки брони                                                                                                    |

В проекте используется фоновый сервис (BackgroundService) для асинхронного выполнения длительных задач, дабы не блокировать работу пользователей.

Для корректной обработки параллельных запросов используются примитивы синхронизации ***lock*** и ***semaphoreSlim***.

Схема базы данных управляется с помощью **Entity Framework Core**. Все изменения моделей отображаются в коде миграций и применяются к БД автоматически.

### Сценарии

Овербукинг (конфликт мест):

- Создаёте событие с TotalSeats = 10 через POST /events.

- Успешно бронируете 10 мест через POST /events/{id}/book (например, 10 разных пользователей).

- Пытаетесь забронировать 11-е место.

- Система возвращает ответ 409 Conflict с сообщением о том, что свободных мест нет.

Ожидаемый результат: бронь не создаётся, количество AvailableSeats остаётся равным 0.

⚙️ Обработка заявок на бронирование выполняется асинхронно в фоновом сервисе.
Новые бронирования создаются со статусом **Ожидают обработки** и после обработки в фоновом сервисе подтверждаются или отклоняются.

⚙️ При реализации механизма бронирования были использованы примитивы синхронизации ***lock*** и ***semaphoreSlim*** для корректной обработки параллельных запросов пользователей.

### 🚀 Запуск проекта

Нужны .NET 10 SDK, Docker.

```bash
# инфраструктура: Kafka, Zookeeper, Postgres
cd devops
docker compose up -d
```

Дождитесь healthy у Kafka и БД (`docker compose ps`).

Из корня репозитория (три терминала):

```bash
cd YandexPracticum

dotnet run --project src/Auth.Service/Auth.Api/Auth.Api.csproj
dotnet run --project src/Events.Service/Events.Api/Events.Api.csproj
dotnet run --project src/Bookings.Service/Bookings.Api/Bookings.Api.csproj
```

#### Swagger (Development):

- Auth: http://localhost:5125/swagger
- Events: http://localhost:5130/swagger
- Bookings: http://localhost:5063/swagger

Секрет JWT — appsettings.Development.json (Jwt:SecretKey), в проде — переменная Jwt__SecretKey.

#### JWT в Swagger
- POST /auth/login в Auth (логин и пароль).
- Скопируйте токен из ответа.
- Authorize → Bearer <токен> → Authorize.

### Миграции

Применяются при старте API. Создать новую:

```bash
dotnet ef migrations add <Имя> --project src/Auth.Service/Auth.Infrastructure --startup-project src/Auth.Service/Auth.Api
dotnet ef migrations add <Имя> --project src/Events.Service/Events.Infrastructure --startup-project src/Events.Service/Events.Api
dotnet ef migrations add <Имя> --project src/Bookings.Service/Bookings.Infrastructure --startup-project src/Bookings.Service/Bookings.Api
```

### ✅ Тестирование

```bash
# запуск тестов
dotnet test
```

⚠️Интеграционные тесты поднимают Postgres и Kafka через **Testcontainers**. Нужен запущенный **Docker**.

## 📖 Документация API

### API Endpoints

### 🧩 Auth (`:5125`)

| Метод | Путь             | Описание        | Статусы            |
|-------|------------------|-----------------|--------------------|
| POST  | /auth/register   | Регистрация     | 204, 400, 409      |
| POST  | /auth/login      | Вход            | 200, 400, 401      |
| GET   | /users/{id}      | Пользователь    | 200, 401, 403, 404 |

### 🧩 Events (`:5130`)

| Метод  | Путь           | Описание     | Статусы                 |
|--------|----------------|--------------|-------------------------|
| GET    | /events        | Список       | 200, 401                |
| GET    | /events/{id}   | По id        | 200, 401, 404           |
| POST   | /events        | Создать      | 201, 400, 401, 403      |
| PUT    | /events/{id}   | Обновить     | 204, 400, 401, 403, 404 |
| DELETE | /events/{id}   | Удалить      | 200, 401, 403, 404      |

Параметры Get /events запроса (Query Parameters)

| Параметр   | Тип        | Обязательный | По умолчанию | Описание                           |
|:-----------|:-----------|:-------------|:-------------|:-----------------------------------|
| `title`    | `string`   | Нет          | —            | Фильтр по наименованию события.    |
| `from`     | `DateTime` | Нет          | —            | Фильтр по дате начала события .    |
| `to`       | `DateTime` | Нет          | —            | Фильтр по дате завершения события. |
| `page`     | `integer`  | Нет          | `1`          | Номер текущей страницы.            |
| `pageSize` | `integer`  | Нет          | `10`         | Количество элементов на странице.  |

### 🧩 Bookings (`:5063`)

| Метод  | Путь                | Описание              | Статусы                 |
|--------|---------------------|-----------------------|-------------------------|
| POST   | /bookings/{eventId} | Заявка на бронь       | 202, 401, 404, 409      |
| GET    | /bookings/{id}      | Статус брони          | 200, 401, 403, 404      |
| DELETE | /bookings/{id}      | Отмена                | 200, 401, 403, 404      |

### ❌ Обработка ошибок (Error Responses)

В случае неуспешного запроса API возвращает стандартный формат ошибок **Problem Details (RFC 7807)**.

### Общая структура ошибки (JSON)

```json
{
  "type": "https://ietf.org",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00",
  "errors": {
    "Title": [
      "Наименование события обязательно для заполнения."
    ],
    "StartAt": [
      "Дата начала события обязательна для заполнения."
    ],
    "EndAt": [
      "Дата окончания события обязательна для заполнения."
    ]
  }
}
```

### Коды ответов при ошибках

| Код                  | Причина                               | Пример сценария                                                  |
|:---------------------|:--------------------------------------|:-----------------------------------------------------------------|
| `400 Bad Request`    | Ошибка валидации параметров.          | Не передан обязательный параметр.                                |
| `401 Unauthorized`   | Не аутентифицирован.                  | Пользователь пытается забронировать событие без входа в систему. |
| `403 Forbidden`      | Доступ запрещен.                      | Пользователь пытается отменить чужое бронирование.               |
| `404 Not Found`      | Ресурс не найден.                     | Запрос несуществующего события.                                  |
| `409 Conflict`       | Отсутствие доступных мест на событии. | Попытка забронировать места при отсутствии доступных.            |
| `500 Internal Error` | Внутренняя ошибка сервера.            | Непредвиденный сбой в базе данных или коде.                      |

## 🛠 Технологии
.NET 10, ASP.NET Core, EF Core, PostgreSQL, Kafka (Confluent), JWT, Swagger, Testcontainers, xUnit.