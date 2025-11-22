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

        // Get method parameters to check for [FromForm] attribute
        var methodInfo = context.MethodInfo;
        var methodParameters = methodInfo.GetParameters();
        
        // Check if any parameter has [FromForm] with a complex type
        var fromFormComplexParam = methodParameters
            .FirstOrDefault(p => 
                p.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.FromFormAttribute), false).Any() &&
                p.ParameterType.IsClass && 
                p.ParameterType != typeof(string) &&
                !typeof(IFormFile).IsAssignableFrom(p.ParameterType));

        // If we have a complex [FromForm] parameter, process it directly
        if (fromFormComplexParam != null)
        {
            ProcessComplexFormParameter(operation, fromFormComplexParam.ParameterType);
            return;
        }

        // Otherwise, get all parameters from form (for individual parameters)
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
                    var propName = prop.Name;
                    
                    // Handle IFormFile
                    if (prop.PropertyType == typeof(IFormFile))
                    {
                        properties[propName] = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        };
                    }
                    // Handle List<IFormFile> or IEnumerable<IFormFile>
                    else if (IsFileCollection(prop.PropertyType))
                    {
                        properties[propName] = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary"
                            }
                        };
                    }
                    // Handle List<T> for complex types
                    else if (IsGenericList(prop.PropertyType))
                    {
                        var itemType = prop.PropertyType.GetGenericArguments()[0];
                        
                        // If it's a simple type
                        if (IsSimpleType(itemType))
                        {
                            properties[propName] = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema
                                {
                                    Type = GetSchemaType(itemType)
                                }
                            };
                        }
                        // If it's a complex type, create nested object schema
                        else
                        {
                            var itemProperties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                            var itemSchema = new Dictionary<string, OpenApiSchema>();
                            
                            foreach (var itemProp in itemProperties)
                            {
                                itemSchema[itemProp.Name] = new OpenApiSchema
                                {
                                    Type = GetSchemaType(itemProp.PropertyType)
                                };
                            }
                            
                            properties[propName] = new OpenApiSchema
                            {
                                Type = "array",
                                Items = new OpenApiSchema
                                {
                                    Type = "object",
                                    Properties = itemSchema
                                }
                            };
                        }
                    }
                    else
                    {
                        properties[propName] = new OpenApiSchema
                        {
                            Type = GetSchemaType(prop.PropertyType)
                        };
                    }
                    
                    // Check if property is required (not nullable)
                    if (!IsNullable(prop.PropertyType))
                        requiredFields.Add(propName);
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

    private void ProcessComplexFormParameter(OpenApiOperation operation, Type parameterType)
    {
        var properties = new Dictionary<string, OpenApiSchema>();
        var requiredFields = new HashSet<string>();

        // Get all properties of the model
        var modelProperties = parameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in modelProperties)
        {
            var propName = prop.Name;

            // Handle IFormFile
            if (prop.PropertyType == typeof(IFormFile))
            {
                properties[propName] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
            }
            // Handle List<IFormFile> or IEnumerable<IFormFile>
            else if (IsFileCollection(prop.PropertyType))
            {
                properties[propName] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                };
            }
            // Handle List<T> for complex types
            else if (IsGenericList(prop.PropertyType))
            {
                var itemType = prop.PropertyType.GetGenericArguments()[0];

                // If it's a simple type
                if (IsSimpleType(itemType))
                {
                    properties[propName] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Type = GetSchemaType(itemType)
                        }
                    };
                }
                // If it's a complex type, create nested object schema
                else
                {
                    var itemProperties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    var itemSchema = new Dictionary<string, OpenApiSchema>();

                    foreach (var itemProp in itemProperties)
                    {
                        itemSchema[itemProp.Name] = new OpenApiSchema
                        {
                            Type = GetSchemaType(itemProp.PropertyType)
                        };
                    }

                    properties[propName] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = itemSchema
                        }
                    };
                }
            }
            else
            {
                properties[propName] = new OpenApiSchema
                {
                    Type = GetSchemaType(prop.PropertyType)
                };
            }

            // Check if property is required (not nullable)
            if (!IsNullable(prop.PropertyType))
                requiredFields.Add(propName);
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

        // Clear all parameters
        operation.Parameters.Clear();
    }

    private static bool IsFileCollection(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var genericTypeDef = type.GetGenericTypeDefinition();
        if (genericTypeDef != typeof(List<>) && 
            genericTypeDef != typeof(IList<>) &&
            genericTypeDef != typeof(ICollection<>) &&
            genericTypeDef != typeof(IEnumerable<>))
            return false;

        var itemType = type.GetGenericArguments()[0];
        return itemType == typeof(IFormFile);
    }

    private static bool IsGenericList(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var genericTypeDef = type.GetGenericTypeDefinition();
        return genericTypeDef == typeof(List<>) ||
               genericTypeDef == typeof(IList<>) ||
               genericTypeDef == typeof(ICollection<>) ||
               genericTypeDef == typeof(IEnumerable<>);
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive ||
               type.IsEnum ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               type == typeof(Guid);
    }

    private static bool IsNullable(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null || 
               !type.IsValueType;
    }

    private static string GetSchemaType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        
        if (underlyingType == typeof(int) || underlyingType == typeof(long) || 
            underlyingType == typeof(short) || underlyingType == typeof(byte))
            return "integer";
        
        if (underlyingType == typeof(bool))
            return "boolean";
        
        if (underlyingType == typeof(double) || underlyingType == typeof(float) || 
            underlyingType == typeof(decimal))
            return "number";
        
        if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset))
            return "string";
        
        return "string";
    }
}
