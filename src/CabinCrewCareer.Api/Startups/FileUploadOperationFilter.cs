using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace CabinCrewCareer.Api.Startups;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if endpoint consumes multipart/form-data
        var consumesFormData = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<Microsoft.AspNetCore.Mvc.ConsumesAttribute>()
            .Any(attr => attr.ContentTypes.Contains("multipart/form-data"));

        if (!consumesFormData)
            return;

        // Get all parameters from form
        var formParameters = context.ApiDescription.ParameterDescriptions
            .Where(p => p.Source.Id == "Form")
            .ToList();

        if (!formParameters.Any())
            return;

        // Build request body schema
        var properties = new Dictionary<string, OpenApiSchema>();
        var requiredFields = new HashSet<string>();

        foreach (var param in formParameters)
        {
            // Check if parameter type is IFormFile
            if (param.Type == typeof(IFormFile))
            {
                properties[param.Name] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
                
                if (param.IsRequired)
                    requiredFields.Add(param.Name);
            }
            // Check if parameter is a complex type that might contain IFormFile
            else if (param.Type.IsClass && param.Type != typeof(string))
            {
                // Get all properties of the model
                var modelProperties = param.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                
                foreach (var prop in modelProperties)
                {
                    if (prop.PropertyType == typeof(IFormFile))
                    {
                        properties[prop.Name] = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        };
                    }
                    else
                    {
                        properties[prop.Name] = new OpenApiSchema
                        {
                            Type = GetSchemaType(prop.PropertyType)
                        };
                    }
                    
                    // Check if property is required (not nullable)
                    if (!IsNullable(prop.PropertyType))
                        requiredFields.Add(prop.Name);
                }
            }
            else
            {
                properties[param.Name] = new OpenApiSchema
                {
                    Type = GetSchemaType(param.Type)
                };
                
                if (param.IsRequired)
                    requiredFields.Add(param.Name);
            }
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = properties,
                        Required = requiredFields
                    }
                }
            }
        };

        // Clear parameters (they're now in request body)
        var paramsToRemove = operation.Parameters
            .Where(p => formParameters.Any(fp => fp.Name == p.Name))
            .ToList();

        foreach (var param in paramsToRemove)
        {
            operation.Parameters.Remove(param);
        }
    }

    private static bool IsNullable(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null || 
               !type.IsValueType;
    }

    private static string GetSchemaType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        
        if (underlyingType == typeof(int) || underlyingType == typeof(long))
            return "integer";
        
        if (underlyingType == typeof(bool))
            return "boolean";
        
        if (underlyingType == typeof(double) || underlyingType == typeof(float) || underlyingType == typeof(decimal))
            return "number";
        
        return "string";
    }
}
