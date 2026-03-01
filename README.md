# BookHub

API for a book review and reading list application built with .NET 8 and C#.

This project provides RESTful endpoints for managing books, authors, genres, user accounts, reading lists, and reviews. It uses Entity Framework Core with a PostgreSQL database and follows a clean architecture with controllers, data models, DTOs, and services.

## 🚀 Features

- **User Authentication**: Register and log in with JWT-based authentication (via `AccountController`).
- **Books Management**: CRUD operations for books and book details (`BooksController`).
- **Authors & Genres**: Endpoints to manage authors and genres (`AuthorsController`, `GenresController`).
- **Reading Lists**: Create and maintain personal reading lists with items (`ReadingListsController`).
- **Reviews**: Users can post and retrieve book reviews (`ReviewsController`).
- **Email Service**: Simple email settings and service for notifications (`EmailService`).
- **Localization**: Includes multiple culture resources (see `bin/Debug/net8.0` locales).

## 🏗️ Project Structure

```
Controllers/          # API controllers for each resource
Data/                 # Entity Framework DbContext and migrations
DTOs/                 # Data transfer objects used by the API
Models/               # Entity models corresponding to database tables
Services/             # Business logic services (e.g., email)
Properties/           # Launch settings and configuration
appsettings*.json     # Configuration files for different environments
```

## 📦 Dependencies

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL (configured via Migrations folder)

## 🧩 Design Overview

The API follows a traditional layered architecture:
1. **Controllers** handle HTTP requests and map them to services.
2. **DTOs** provide flattened shapes for incoming/outgoing JSON.
3. **Data Layer** (`ApplicationDbContext`) defines EF Core models and migrations.
4. **Services** encapsulate reusable logic (e.g. email sending).
5. **Configuration** is managed through `appsettings.json` files and environment variables.

Authentication and authorization are implemented via ASP.NET Core Identity with JWT tokens. The `ApplicationUser` model extends `IdentityUser` to include application-specific fields.

## ⚙️ Running the Application Locally

1. Ensure you have .NET 8 SDK installed.
2. Update `appsettings.json` with your PostgreSQL connection string.
3. Apply migrations:
   ```bash
   dotnet ef database update
   ```
4. Run the project:
   ```bash
   dotnet run
   ```
5. The API will be available at `https://localhost:5001` (or as configured in `launchSettings.json`).

## 🌐 Deployment

The API is deployed on **Render** and is publicly accessible:

[https://bookhub-7cs4.onrender.com](https://bookhub-7cs4.onrender.com)

Feel free to explore the live endpoints directly or use it for testing without running the server locally.

## 📝 Postman Collection

A Postman collection is included: `BookHub_Postman_Collection.json` for testing endpoints.


