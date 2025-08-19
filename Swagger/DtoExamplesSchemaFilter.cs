using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserManagement.Dtos;

namespace UserManagement.Swagger;

public sealed class DtoExamplesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(UserCreateDto))
        {
            schema.Example = new OpenApiObject
            {
                ["firstName"] = new OpenApiString("John"),
                ["lastName"] = new OpenApiString("Doe"),
                ["email"] = new OpenApiString("user@example.com"),
                ["dateOfBirth"] = new OpenApiString("1990-01-01")
            };
        }
        else if (context.Type == typeof(UserUpdateDto))
        {
            schema.Example = new OpenApiObject
            {
                ["firstName"] = new OpenApiString("Jane"),
                ["lastName"] = new OpenApiString("Smith"),
                ["email"] = new OpenApiString("jane.smith@example.com"),
                ["dateOfBirth"] = new OpenApiString("1988-05-20"),
                ["isActive"] = new OpenApiBoolean(true)
            };
        }
    }
}
