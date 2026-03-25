using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ITokenValidator, MyTokenValidator>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandling();

app.UseMiddleware<TokenValidationMiddleware>();

app.UseRequestResponseLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// In-memory storage using Dictionary
var users = new Dictionary<int, User>{
    { 1, new User { UserName = "Alice", UserEmail = "alice@example.com", UserAge = 30 } },
    { 2, new User { UserName = "Bob", UserEmail = "bob@example.com", UserAge = 25 } }
};
int nextId = users.Keys.Any() ? users.Keys.Max() + 1 : 1;

app.MapGet("/secure", (HttpContext ctx) =>
{
    var user = ctx.Items["User"];
    return Results.Ok(new { message = "Hello!", user });
});

// GET all users
app.MapGet("/users", () => users.Values);

// GET user by ID
app.MapGet("/users/{id:int}", (int id) =>
    users.TryGetValue(id, out var user) ? Results.Ok(user) : Results.NotFound());

// POST new user
app.MapPost("/users", (User user) =>
{
    var id = nextId++;
    var newUser = new User { UserName = user.UserName, UserEmail = user.UserEmail, UserAge = user.UserAge };
    users[id] = newUser;
    return Results.Created($"/users/{id}", newUser);
});

// PUT update user
app.MapPut("/users/{id:int}", (int id, User user) =>
{
    if (!users.ContainsKey(id)) return Results.NotFound();
    var updatedUser = new User { UserName = user.UserName, UserEmail = user.UserEmail, UserAge = user.UserAge };
    users[id] = updatedUser;
    return Results.Ok(updatedUser);
});

// DELETE user
app.MapDelete("/users/{id:int}", (int id) =>
    users.Remove(id) ? Results.NoContent() : Results.NotFound());

app.Run();
public class User
{ 
    required public string UserName { get; set; }

    required public string UserEmail { get; set; }
    public int UserAge { get; set; }
    public string Role { get; set; } = "User";
}

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // Log incoming request
        _logger.LogInformation(
            "Incoming Request: {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        await _next(context);

        // Log outgoing response
        _logger.LogInformation(
            "Outgoing Response: {StatusCode}",
            context.Response.StatusCode);
    }
}


public static class RequestResponseLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "Internal server error."
            };

            var json = JsonSerializer.Serialize(errorResponse);

            await context.Response.WriteAsync(json);
        }
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenValidator tokenValidator)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing or invalid token");
            return;
        }

        var token = authHeader.Substring("Bearer ".Length);

        var user = await tokenValidator.ValidateAsync(token);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid token");
        }

        // Attach user info to HttpContext for downstream usage
        context.Items["User"] = user;

        await _next(context);
    }
}

public interface ITokenValidator
{
    Task<User?> ValidateAsync(string token);
}

public class MyTokenValidator : ITokenValidator
{
    public Task<User?> ValidateAsync(string token)
    {
        // Replace with real JWT or custom token validation
        if (token == "valid-token-123")
        {
            return Task.FromResult<User?>(new User { UserName = "Lechu", UserEmail = "lechu@example.com", Role = "Admin" });
        }

        return Task.FromResult<User?>(null);
    }
}
