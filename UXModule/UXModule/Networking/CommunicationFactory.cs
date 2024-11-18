using Microsoft.Extensions.Hosting;
using Networking.Communication;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Networking.GrpcServices;

public static class CommunicationFactory
{
    private static readonly CommunicatorClient s_communicatorClient = new();
    private static readonly CommunicatorServer s_communicatorServer = new();
    private static ClientServices s_grpcClientServices;
    private static ServerServices s_grpcServerServices;

    private static IHost s_grpcHost; // Global static host variable
    private static readonly object s_lock = new(); // Lock for thread safety

    /// <summary>
    /// Factory function to get the communicator.
    /// </summary>
    /// <param name="isClientSide">
    /// Boolean telling if it is client side or server side.
    /// </param>
    /// <param name="isGrpc">Specifies if gRPC is enabled.</param>
    /// <returns>The communicator singleton instance.</returns>
    public static ICommunicator GetCommunicator(bool isClientSide = true, bool isGrpc = false)
    {
        Trace.WriteLine("[Networking] CommunicationFactory.GetCommunicator() function called with isClientSide: " + isClientSide);

        if (isGrpc)
        {
            // Ensure the host is initialized only once
            if (s_grpcHost == null)
            {
                lock (s_lock)
                {
                    if (s_grpcHost == null)
                    {
                        int port = isClientSide ? 7009 : 7000;
                        s_grpcHost = Host.CreateDefaultBuilder()
                            .ConfigureWebHostDefaults(webBuilder =>
                            {
                                webBuilder.ConfigureServices(services =>
                                {
                                    services.AddGrpc();
                                    services.AddSingleton<ServerServices>();
                                    services.AddSingleton<ClientServices>();
                                });

                                webBuilder.ConfigureKestrel(options =>
                                {
                                    options.ListenAnyIP(port, listenOptions =>
                                    {
                                        listenOptions.Protocols = HttpProtocols.Http2; // Enable HTTP/2
                                    });
                                });

                                webBuilder.Configure(app =>
                                {
                                    app.UseRouting();
                                    app.UseEndpoints(endpoints =>
                                    {
                                        endpoints.MapGrpcService<ServerServices>();
                                        endpoints.MapGrpcService<ClientServices>();
                                        endpoints.MapGet("/", () => "server running");
                                    });
                                });

                                webBuilder.UseUrls($"http://0.0.0.0:{port}");
                            })
                            .Build();

                        // Start the host in a background thread
                        s_grpcHost.Start();

                        // Initialize server services
                        using IServiceScope scope = s_grpcHost.Services.CreateScope();
                        s_grpcServerServices = scope.ServiceProvider.GetRequiredService<ServerServices>();
                        s_grpcClientServices = scope.ServiceProvider.GetRequiredService<ClientServices>();
                    }
                }
            }

            // Return the appropriate instance
            return isClientSide ? s_grpcClientServices : s_grpcServerServices;
        }
        else
        {
            // Return non-gRPC instances
            return isClientSide ? s_communicatorClient : s_communicatorServer;
        }
    }
}
