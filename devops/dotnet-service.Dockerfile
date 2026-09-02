ARG PROJECT_FILE

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG PROJECT_FILE
WORKDIR /src
COPY . .
RUN dotnet restore ${PROJECT_FILE}
RUN dotnet publish ${PROJECT_FILE} -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet"]
