using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyLife.Services.Functions;
public class AzureStaticWebAppMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext != null && httpContext.Request.Headers.TryGetValue("x-ms-client-principal", out var header))
        {
            if (header[0] is string data)
            {
                var decoded = Convert.FromBase64String(data);
                var json = Encoding.UTF8.GetString(decoded);
                ClientPrincipal? clientPrincipal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (clientPrincipal is not null)
                {
                    clientPrincipal.UserRoles = clientPrincipal.UserRoles?.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase) ?? Enumerable.Empty<string>();

                    if (clientPrincipal.UserRoles?.Any() == true)
                    {
                        var identity = new ClaimsIdentity(clientPrincipal.IdentityProvider);
                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, clientPrincipal.UserId));
                        identity.AddClaim(new Claim(ClaimTypes.Name, clientPrincipal.UserDetails));
                        identity.AddClaims(clientPrincipal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));

                        httpContext.User = new(identity);
                        context.Items.Add("User", httpContext.User);
                    }
                }
            }
        }

        await next(context);
    }
}

file class ClientPrincipal
{
    public string IdentityProvider { get; set; } = "";
    public string UserId { get; set; } = "";
    public string UserDetails { get; set; } = "";
    public IEnumerable<string> UserRoles { get; set; } = Enumerable.Empty<string>();
}

public static class AzureStaticWebAppMiddlewareExtensions
{
    public static IFunctionsWorkerApplicationBuilder UseAzureStaticWebAppMiddleware(this IFunctionsWorkerApplicationBuilder builder)
    {
        return builder.UseMiddleware<AzureStaticWebAppMiddleware>();
    }
}
