# Warehouse API

## Обзор

RESTful API для упрощенной системы управления складом.

## Технологии

-   **.NET 9**
-   **ASP.NET Core:** Веб-фреймворк.
-   **Entity Framework Core:** ORM для доступа к данным.
-   **PostgreSQL:** Реляционная база данных.
-   **Docker и Docker Compose:** Для контейнеризации и управления окружением.
-   **Swagger (OpenAPI):** Для документирования API.
-   **AutoMapper:** Для маппинга объектов.
-   **FluentValidation:** Для валидации запросов.

## Настройка

1.  **Необходимые компоненты:**
    *   .NET 9 SDK
    *   Docker Desktop

2.  **Конфигурация:**
    Строка подключения находится в файле `appsettings.json`. По умолчанию она настроена для подключения к экземпляру PostgreSQL, запущенному через `docker-compose`.

    ```json
    "ConnectionStrings": {
      "WarehouseContext": "Host=localhost;Port=5432;Database=warehouse;Username=postgres;Password=postgres"
    }
    ```

## Запуск приложения

1.  **Запуск базы данных:**
    Выполните следующую команду в корне проекта, чтобы запустить базу данных PostgreSQL с помощью Docker Compose:
    ```bash
    docker-compose up -d
    ```

2.  **Запуск API:**
    Выполните следующую команду из корневого каталога проекта:
    ```bash
    dotnet run
    ```
    API будет доступен по адресу `http://localhost:5233`.

## Документация API

После запуска приложения документация Swagger UI будет доступна по адресу:

[http://localhost:5233/swagger/index.html](http://localhost:5233/swagger/index.html)
