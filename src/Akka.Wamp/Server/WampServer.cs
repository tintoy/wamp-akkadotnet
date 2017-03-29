using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WampSharp.AspNetCore.WebSockets.Server;
using WampSharp.Binding;
using WampSharp.V2;

namespace Akka.Wamp.Server
{
    /// <summary>
    ///     A WAMP server (based on ASP.NET Core).
    /// </summary>
    /// <remarks>
    ///     This ugly workaround is required, for now, in order to support .NET Core (WampSharp.Default is missing a WebSockets server for Core unless you're using ASP.NET Core).
    /// </remarks>
    class WampServer
    {
        /// <summary>
        ///     Create a new WebSocket server.
        /// </summary>
        /// <param name="baseAddress">
        ///     The server base address.
        /// </param>
        public WampServer(Uri baseAddress)
        {
            if (baseAddress == null)
                throw new ArgumentNullException(nameof(baseAddress));
            
            BaseAddress = baseAddress;
        }

        /// <summary>
        ///     The server base address.
        /// </summary>
        public Uri BaseAddress { get; }

        /// <summary>
        ///     The underlying WAMP host.
        /// </summary>
        WampHost WampHost { get; set; }

        /// <summary>
        ///     The underlying ASP.NET Core web host.
        /// </summary>
        IWebHost WebHost { get; set; }

        /// <summary>
        ///     Start the WAMP server.
        /// </summary>
        public void Start()
        {
            // TODO: Guard (already started?)

            WampHost = new WampHost();
            WebHost = CreateWebHost();

            WampHost.Open();
            WebHost.Start();
        }

        IWebHost CreateWebHost()
        {
            return new WebHostBuilder()
                .UseUrls(
                    BaseAddress.ToString().Replace("localhost", "+")
                )
                .Configure(app =>
                {
                    app.UseDeveloperExceptionPage();

                    app.Map(BaseAddress.AbsolutePath, ws =>
                    {
                        ws.UseWebSockets(new WebSocketOptions
                        {
                            ReplaceFeature = true
                        });

                        WampHost.RegisterTransport(
                            new AspNetCoreWebSocketTransport(ws),
                            new JTokenJsonBinding(),
                            new JTokenMsgpackBinding()
                        );
                    });
                })
                .Build();
        }
    }
}