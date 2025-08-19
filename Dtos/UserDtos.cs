using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace UserManagement.Dtos;

// READ model used in responses and queries
public record UserDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    DateOnly? DateOfBirth,
    bool IsActive
);

// CREATE payload
public record UserCreateDto(
    [property: Required, MinLength(2)]
    [property: RegularExpression(@"^(?=.*\p{L})[\p{L}\p{M}\s'\-]+$",
    ErrorMessage = "Must contain letters only (letters, spaces, hyphens, apostrophes).")]
    [property: SwaggerSchema(Description = "User's first name")]
    string FirstName,

    [property: Required, MinLength(2)]
    [property: RegularExpression(@"^(?=.*\p{L})[\p{L}\p{M}\s'\-]+$",
    ErrorMessage = "Must contain letters only (letters, spaces, hyphens, apostrophes).")]
    [property: SwaggerSchema(Description = "User's last name")]
    string LastName,

    [property: Required, EmailAddress]
    [property: SwaggerSchema(Description = "Email address")]
    string Email,

    [property: SwaggerSchema(Description = "YYYY-MM-DD")]
    DateOnly? DateOfBirth
);

// UPDATE payload
public record UserUpdateDto(
    [property: SwaggerSchema(Description = "User's first name")]
    [property: RegularExpression(@"^[\p{L}\p{M}\s'\-]+$", ErrorMessage = "FirstName must contain letters only.")]
    string? FirstName,

    [property: SwaggerSchema(Description = "User's last name")]
    [property: RegularExpression(@"^[\p{L}\p{M}\s'\-]+$", ErrorMessage = "LastName must contain letters only.")]
    string? LastName,

    [property: SwaggerSchema(Description = "Email address")]
    [property: EmailAddress]
    string? Email,

    [property: SwaggerSchema(Description = "YYYY-MM-DD")]
    DateOnly? DateOfBirth,

    bool? IsActive
);
