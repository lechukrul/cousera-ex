# UserManagementApi

A simple ASP.NET Core Web API for managing users, built with minimal APIs and in-memory storage.

## Features

- **User Management**: Full CRUD operations for users including create, read, update, and delete.
- **Authentication**: Token-based authentication using Bearer tokens. Includes a secure endpoint that requires valid authentication.
- **Middleware**:
  - Exception handling middleware to catch and log unhandled exceptions.
  - Request/response logging middleware to log incoming requests and outgoing responses.
  - Token validation middleware to authenticate requests.
- **In-Memory Storage**: Uses a Dictionary for storing user data (not persistent across restarts).
- **OpenAPI Support**: Generates OpenAPI documentation in development mode.

## API Endpoints

- `GET /users` - Retrieve all users
- `GET /users/{id}` - Retrieve a user by ID
- `POST /users` - Create a new user
- `PUT /users/{id}` - Update an existing user
- `DELETE /users/{id}` - Delete a user by ID
- `GET /secure` - Secure endpoint requiring authentication

## Prerequisites

- .NET 10.0 or later
- Visual Studio or .NET CLI

## Running the Application

1. Navigate to the project directory: `cd UserMenagementApi`
2. Restore dependencies: `dotnet restore`
3. Run the application: `dotnet run`
4. The API will be available at `https://localhost:5001` (or as configured in launchSettings.json)

## Testing

Use the provided `UserMenagementApi.http` file for testing endpoints with tools like REST Client in VS Code.

For the secure endpoint, include the header: `Authorization: Bearer valid-token-123`

## Project Structure

- `Program.cs` - Main application file with API routes and middleware
- `appsettings.json` - Configuration files
- `UserMenagementApi.csproj` - Project file
- `UserMenagementApi.http` - HTTP test file

## Notes

- This is a demo application using in-memory storage. For production, replace with a proper database.
- Token validation is simplified for demonstration; implement proper JWT or OAuth in production.