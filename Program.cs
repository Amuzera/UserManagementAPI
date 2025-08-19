using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;


// App namespaces
using UserManagement.Common;
using UserManagement.Data;
using UserManagement.Dtos;
using UserManagement.Middleware;
using UserManagement.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new() { Title = "UserManagementAPI", Version = "v1" });

    var bearerScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Paste your JWT here (no 'Bearer ' prefix).",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", bearerScheme);

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        [bearerScheme] = Array.Empty<string>()
    });
    c.SchemaFilter<UserManagement.Swagger.DtoExamplesSchemaFilter>();
});



// EF InMemory
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("UserDb"));

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-super-secret-key-min-32-chars-1234567890!!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "UserManagementAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "UserManagementAPIClients";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// --- Build ---
var app = builder.Build();

// --- Swagger ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ======= MIDDLEWARE ========
// 1) Error handling
app.UseMiddleware<GlobalExceptionMiddleware>();
// 2) Authentication/Authorization
app.UseAuthentication();
app.UseAuthorization();
// 3) Logging (request/response)
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Dev token
app.MapPost("/auth/token", (AuthRequest? req) =>
{

    // null/body guard
    if (req is null)
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            { "body", new[] { "Request body is required." } }
        });

    if (string.IsNullOrWhiteSpace(req.Username))
        return Results.BadRequest("Username required.");

    var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, req.Username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: creds);

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { access_token = jwt, token_type = "Bearer" });
}).AllowAnonymous();


// ======= SECURED API =======
var users = app.MapGroup("/api/users").RequireAuthorization();

// GET /api/users?search=&page=1&pageSize=10
users.MapGet("/", async (AppDbContext db, CancellationToken ct, string? search, int page = 1, int pageSize = 10) =>
{
    page = page < 1 ? 1 : page;
    pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

    var query = db.Users.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(search))
    {
        search = search.Trim();
        query = query.Where(u =>
            u.FirstName.Contains(search) ||
            u.LastName.Contains(search) ||
            u.Email.Contains(search));
    }

    var total = await query.CountAsync(ct);
    var items = await query
        .OrderBy(u => u.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(u => new UserDto(u.Id, u.FirstName, u.LastName, u.Email, u.DateOfBirth, u.IsActive))
        .ToListAsync(ct);

    return Results.Ok(new PagedResult<UserDto>(items, total, page, pageSize));
});

// GET /api/users/{id}
users.MapGet("/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
{
    var user = await db.Users.AsNoTracking()
        .Where(u => u.Id == id)
        .Select(u => new UserDto(u.Id, u.FirstName, u.LastName, u.Email, u.DateOfBirth, u.IsActive))
        .FirstOrDefaultAsync(ct);

    return user is null
        ? Results.Problem($"User {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found")
        : Results.Ok(user);
});

// POST /api/users
users.MapPost("/", async (UserCreateDto dto, AppDbContext db, CancellationToken ct) =>
{
    if (dto is null)
        return Results.BadRequest("Request body is required.");

    var errors = ValidationHelper.Validate(dto);
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    var user = new User
    {
        FirstName = dto.FirstName.Trim(),
        LastName = dto.LastName.Trim(),
        Email = dto.Email.Trim(),
        DateOfBirth = dto.DateOfBirth,
        IsActive = true
    };

    db.Users.Add(user);
    await db.SaveChangesAsync(ct);

    var result = new UserDto(user.Id, user.FirstName, user.LastName, user.Email, user.DateOfBirth, user.IsActive);
    return Results.Created($"/api/users/{user.Id}", result);
});


// PUT /api/users/{id}
users.MapPut("/{id:int}", async (int id, UserUpdateDto dto, AppDbContext db, CancellationToken ct) =>
{
    if (dto is null) return Results.ValidationProblem(
        new Dictionary<string, string[]>
        {
            { "body", new[] { "Request body is required." } }
        }
    );

    var user = await db.Users.FindAsync(new object[] { id }, ct);
    if (user is null)
        return Results.Problem($"User {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

    // ✅ Run the same attribute-based validation used by POST
    var errors = ValidationHelper.Validate(dto);
    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    // Optional: still block whitespace-only names (trim first)
    var first = dto.FirstName?.Trim();
    var last = dto.LastName?.Trim();
    if (first is not null && string.IsNullOrWhiteSpace(first))
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            { "FirstName", new[] { "FirstName cannot be empty." } }
        });
    if (last is not null && string.IsNullOrWhiteSpace(last))
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            { "LastName", new[] { "LastName cannot be empty." } }
        });

    // Save trimmed values
    if (first is not null) user.FirstName = first;
    if (last is not null) user.LastName = last;
    if (dto.Email is not null) user.Email = dto.Email.Trim();
    if (dto.DateOfBirth.HasValue) user.DateOfBirth = dto.DateOfBirth;
    if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;

    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});



// DELETE /api/users/{id}
users.MapDelete("/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
{
    var user = await db.Users.FindAsync(new object[] { id }, ct);
    if (user is null)
        return Results.Problem($"User {id} not found.", statusCode: StatusCodes.Status404NotFound, title: "Not Found");

    db.Users.Remove(user);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

// Debug route to force an exception
if (app.Environment.IsDevelopment())
{
    app.MapGet("/boom", () => { throw new Exception("Kaboom!"); })
       .ExcludeFromDescription();
}

// Seed dev data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!await db.Users.AnyAsync())
    {
        db.Users.AddRange(
            new User { FirstName = "Alan", LastName = "Z", Email = "alan@example.com" },
            new User { FirstName = "David", LastName = "S", Email = "david@example.com" }
        );
        await db.SaveChangesAsync();
    }
}

app.Run();
