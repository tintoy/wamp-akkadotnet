using Akka.Actor;
using Akka.Event;
using Akka.TestKit;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

using TestKitBase = Akka.TestKit.Xunit2.TestKit;

namespace Akka.Wamp.Tests
{
    using Actors;
    using Messages;

	/// <summary>
	///		Tests for the WAMP server manager support.
	/// </summary>
	public class ServerManagerTests
		: ActorTestBase
    {
        /// <summary>
		///		Create a new test suite for Rx subjects.
		/// </summary>
		/// <param name="output">
		///		The test output facility.
		/// </param>
		public ServerManagerTests(ITestOutputHelper output)
			: base(output)
		{
		}

        /// <summary>
        ///     Verify that we can create an instance of the <see cref="ServerManager"/>.
        /// </summary>
        [Fact]
        public void Can_create_server_manager()
        {
            SubscribeToErrors();

            IActorRef serverManager = ActorOf(
                WampServerManager.Create(new Uri("ws://localhost:19929")),
                name: "TestServerManager"
            );
            ExpectNoMsg(
                TimeSpan.FromMilliseconds(500)
            );
        }

        /// <summary>
        ///     Verify that we can start the underlying server for a <see cref="ServerManager"/>.
        /// </summary>
        [Fact]
        public async Task Can_start_server_manager()
        {
            SubscribeToErrors();

            IActorRef serverManager = ActorOf(
                WampServerManager.Create(new Uri("ws://localhost:19929")),
                name: "TestServerManager"
            );
            ExpectNoMsg();

            // AF: This test is ugly - the actor should return Status.Success or Status.Failure (Status.Failure, not Failure).
            // This enables the use of Ask<Status>(Start.Instance).

            object response = await serverManager.Ask(Start.Instance,
                timeout: TimeSpan.FromSeconds(5)
            );
            ExpectNoMsg(); // Should be no Error events.

            if (response is Failure failureResponse)
                Output.WriteLine(failureResponse.Exception.ToString());

            Assert.IsType<Status.Success>(response);
        }
    }
}