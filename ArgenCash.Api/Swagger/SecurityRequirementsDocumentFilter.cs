using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ArgenCash.Api.Swagger;

public class SecurityRequirementsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(swaggerDoc);

        foreach (var pathItem in swaggerDoc.Paths.Values)
        {
            if (pathItem is null)
            {
                continue;
            }

            if (pathItem.Operations is null)
            {
                continue;
            }

            foreach (var operation in pathItem.Operations)
            {
                var operationPath = operation.Key;
                var target = operation.Value;

                if (target?.Tags is null)
                {
                    continue;
                }

                var isAnonymousAuthEndpoint = target.Tags.Any(tag => string.Equals(tag.Name, "Auth", StringComparison.OrdinalIgnoreCase))
                    && (operationPath == HttpMethod.Post)
                    && (target.OperationId?.Contains("Register", StringComparison.OrdinalIgnoreCase) == true
                        || target.OperationId?.Contains("Login", StringComparison.OrdinalIgnoreCase) == true);

                if (isAnonymousAuthEndpoint)
                {
                    continue;
                }

                target.Security ??= new List<OpenApiSecurityRequirement>();
                if (target.Security.Count == 0)
                {
                    target.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", null, null)] = new List<string>()
                    });
                }
            }
        }
    }
}
