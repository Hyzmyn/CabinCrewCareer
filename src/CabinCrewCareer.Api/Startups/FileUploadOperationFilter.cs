using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CabinCrewCareer.Api.Startups;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var formFileParameters = context.ApiDescription.ParameterDescriptions
            .Where(p => p.Type == typeof(IFormFile) || 
                       (Nullable.GetUnderlyingType(p.Type) == null && 
                        p.Type.IsGenericType && 
                        p.Type.GetGenericTypeDefinition() == typeof(Nullable<>) == false &&
                        p.Type == typeof(IFormFile)))
            .ToList();

        // Check if any parameter is IFormFile by name convention or model property
        var hasFileParameter = context.ApiDescription.ParameterDescriptions
            .Any(p => p.Type == typeof(IFormFile) || p.Name.Contains("file", StringComparison.OrdinalIgnoreCase));

        if (!hasFileParameter)
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content =
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = context.ApiDescription.ParameterDescriptions
                            .ToDictionary(
                                p => p.Name,
                                p => IsFileParameter(p.Type, p.Name)
                                    ? new OpenApiSchema { Type = "string", Format = "binary" }
                                    : new OpenApiSchema { Type = GetSchemaType(p.Type) }
                            ),
                        Required = context.ApiDescription.ParameterDescriptions
                            .Where(p => p.IsRequired)
                            .Select(p => p.Name)
                            .ToHashSet()
                    }
                }
            }
        };

        // Remove all parameters since they're now in request body
        operation.Parameters.Clear();
    }

    private static bool IsFileParameter(Type type, string name)
    {
        return type == typeof(IFormFile) || 
               name.Contains("file", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetSchemaType(Type type)
    {
        if (type == typeof(int) || type == typeof(int?)) return "integer";
        if (type == typeof(long) || type == typeof(long?)) return "integer";
        if (type == typeof(bool) || type == typeof(bool?)) return "boolean";
        if (type == typeof(double) || type == typeof(double?) || 
            type == typeof(float) || type == typeof(float?) ||
            type == typeof(decimal) || type == typeof(decimal?)) return "number";
        return "string";
    }
}
