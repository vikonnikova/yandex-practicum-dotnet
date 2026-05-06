# Events API

REST API для управления событиями (создание, получение, обновление и удаление).

---

## 📌 Описание

Проект реализует базовый CRUD для работы с событиями:
- получение списка событий
- получение события по id
- создание события
- обновление события
- удаление события

⚠️ Данные хранятся **в памяти приложения (in-memory storage)** и не сохраняются после перезапуска сервиса.

---

## 🛠 Технологии

- .NET 10 / ASP.NET Core Web API
- C#
- REST API
- OpenAPI (встроенная поддержка .NET)
- Swagger UI (для тестирования API)
- HTTP client (.http файлы)

---

## 🚀 Запуск проекта

### Требования
- .NET SDK 10.0

### Запуск

Перейти в папку проекта:
- cd ../YandexPracticum

Собрать проект:
- dotnet build

Запустить приложение:
- dotnet run --project Events.Api

После запуска API будет доступен по адресу:
http://localhost:5130/api

---

## 📖 Документация API

### Swagger UI
http://localhost:5130/swagger

### OpenAPI JSON
http://localhost:5130/openapi/v1.json

### API Endpoints

- GET /events — получить список всех событий
- GET /events/{id} — получить событие по id
- POST /events — создать событие
- PUT /events/{id} — обновить событие
- DELETE /events/{id} — удалить событие