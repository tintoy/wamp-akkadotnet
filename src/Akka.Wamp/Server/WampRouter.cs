using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WampSharp.AspNetCore.WebSockets.Server;
using WampSharp.Binding;
using WampSharp.V2;
using WampSharp.V2.Realm;

namespace Akka.Wamp.Server
{
    /// <summary>
    ///     A WAMP router (based on ASP.NET Core).
    /// </summary>
    /// <remarks>
    ///     This ugly workaround is required, for now, in order to support .NET Core (WampSharp.Default is missing a WebSockets server for Core unless you're using ASP.NET Core).
    /// </remarks>
    sealed class WampRouter
        : IDisposable
    {
        /// <summary>
        ///     Has the <see cref="WampRouter"/> been disposed?
        /// </summary>
        bool _isDisposed;

        /// <summary>
        ///     Create a new WebSocket server.
        /// </summary>
        /// <param name="baseAddress">
        ///     The server base address.
        /// </param>
        public WampRouter(Uri baseAddress)
        {
            if (baseAddress == null)
                throw new ArgumentNullException(nameof(baseAddress));
            
            BaseAddress = baseAddress;
        }

        /// <summary>
        ///     Dispose of resources being used by the <see cref="WampRouter"/>.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            if (IsRunning)
                Stop();

            _isDisposed = true;
        }

        /// <summary>
        ///     Check if the <see cref="WampRouter"/> has been disposed.
        /// </summary>
        void CheckDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(WampRouter));
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
        ///     Is the WAMP router running?
        /// </summary>
        public bool IsRunning => WampHost != null && WebHost != null;

        /// <summary>
        ///     Start the WAMP router.
        /// </summary>
        public void Start()
        {
            if (IsRunning)
                throw new InvalidOperationException("The WAMP router is already running.");

            WampHost = new WampHost();
            WebHost = CreateWebHost(WampHost);

            WampHost.Open();
            WebHost.Start();
        }

        /// <summary>
        ///     Stop the WAMP router.
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
                throw new InvalidOperationException("The WAMP router is not running.");

            if (WampHost != null)
            {
                WampHost.Dispose();
                WampHost = null;
            }

            if (WebHost != null)
            {
                WebHost.Dispose();
                WebHost = null;
            }
        }

        /// <summary>
        ///     Get a reference to the host-side API for the specified WAMP realm.
        /// </summary>
        /// <param name="name">
        ///     The name of the target WAMP realm.
        /// </param>
        /// <returns>
        ///     An <see cref="IWampHostedRealm"/> that can be used to interact with the realm.
        /// </returns>
        public IWampHostedRealm GetRealm(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Argument cannot be null, empty, or entirely composed of whitespace: 'name'.", nameof(name));
            
            CheckDisposed();
            EnsureRunning();

            return WampHost.RealmContainer.GetRealmByName(name);
        }

        /// <summary>
        ///     Ensure that the WAMP router is running.
        /// </summary>
        void EnsureRunning()
        {
            if (!IsRunning)
                throw new InvalidOperationException("The WAMP router is not running.");
        }

        /// <summary>
        ///     Create an ASP.NET Core web host for the specified <see cref="IWampHost"/>.
        /// </summary>
        /// <param name="wampHost">
        ///     The WAMP host.
        /// </param>
        /// <returns>
        ///     The configured <see cref="IWebHost"/>.
        /// </returns>
        IWebHost CreateWebHost(IWampHost wampHost)
        {
            if (wampHost == null)
                throw new ArgumentNullException(nameof(wampHost));

            return new WebHostBuilder()
                .UseUrls(
                    BaseAddress
                        .GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)
                        .Replace("localhost", "+")
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

                        wampHost.RegisterTransport(
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