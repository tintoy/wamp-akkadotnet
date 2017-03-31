using Akka.Actor;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Wamp.Tests
{
	/// <summary>
	///		Tests for the WAMP integration extension for Akka actor systems.
	/// </summary>
	public class WampExtensionTests
	{
		/// <summary>
		///		Create a new test suite for the WAMP integration extension.
		/// </summary>
		/// <param name="output">
		///		The test output.
		/// </param>
		public WampExtensionTests(ITestOutputHelper output)
		{
			if (output == null)
				throw new System.ArgumentNullException(nameof(output));

			Output = output;
		}

		/// <summary>
		///		Output for the current test run.
		/// </summary>
		public ITestOutputHelper Output
		{
			get;
		}

		/// <summary>
		///		Verify that the WAMP integration extension can be retrieved for an Akka actor system.
		/// </summary>
		[Fact]
		public void Can_get_extension_from_actor_system()
		{
			using (ActorSystem system = ActorSystem.Create("Test", TestConfigurations.Default))
			{
				WampApi WampApi = system.Wamp();
				Assert.NotNull(WampApi);

				system.Terminate();
				system.WhenTerminated.Wait();
			}
		}

		/// <summary>
		///		Verify that the WAMP integration extension is a singleton if retrieved multiple times from the same actor system.
		/// </summary>
		[Fact]
		public void Extension_is_singleton_within_actor_system()
		{
			using (ActorSystem system = ActorSystem.Create("Test", TestConfigurations.Default))
			{
				WampApi WampApi1 = system.Wamp();
				Assert.NotNull(WampApi1);

				WampApi WampApi2 = system.Wamp();
				Assert.NotNull(WampApi2);

				Assert.Same(WampApi1, WampApi2);

				system.Terminate();
				system.WhenTerminated.Wait();
			}
		}

		/// <summary>
		///		Verify that the WAMP integration extension is not a singleton across multiple Akka actor systems.
		/// </summary>
		[Fact]
		public void Extension_is_not_singleton_across_actor_systems()
		{
			using (ActorSystem system1 = ActorSystem.Create("Test1", TestConfigurations.Default))
			{
				WampApi WampApi1 = system1.Wamp();
				Assert.NotNull(WampApi1);

				using (ActorSystem system2 = ActorSystem.Create("Test2", TestConfigurations.Default))
				{
					WampApi WampApi2 = system2.Wamp();
					Assert.NotNull(WampApi2);

					Assert.NotSame(WampApi1, WampApi2);

					system2.Terminate();
					system2.WhenTerminated.Wait();
				}

				system1.Terminate();
				system1.WhenTerminated.Wait();
			}
		}
	}
}
