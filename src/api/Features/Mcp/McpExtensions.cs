using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Achai.Api.Features.Mcp;

public static class McpExtensions
{
    public const string Path = "/mcp";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    public static IServiceCollection AddAchaiMcpServer(this IServiceCollection services)
    {
        services.AddMcpServer(options => options.ServerInfo = new() { Name = "achai-api", Version = "1.0.0" })
            .WithHttpTransport(options => options.Stateless = true)
            .WithTools([typeof(AddressTools)], Json);

        return services;
    }
}
