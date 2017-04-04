using Akka.Actor;
using Akka.Configuration;
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
    /// <summary>
	///		The base class for actor-oriented test suites.
	/// </summary>
	public abstract class ActorTestBase
		: TestKitBase
    {
		/// <summary>
		/// 	Create a new actor-oriented test suite.
		/// </summary>
		/// <param name="output">
		/// 	The output facility for the current test.
		/// </param>
		protected ActorTestBase(ITestOutputHelper output)
			: this(TestConfigurations.Default.WithFallback(DefaultConfig), output)
		{

		}
		
		/// <summary>
		/// 	Create a new actor-oriented test suite.
		/// </summary>
		/// <param name="config">
		/// 	The Akka.NET configuration for the current test.
		/// </param>
		/// <param name="output">
		/// 	The output facility for the current test.
		/// </param>
		protected ActorTestBase(Config config, ITestOutputHelper output)
			: base(config, output: output)
		{
			if (output == null)
				throw new ArgumentNullException(nameof(output));
			
			Output = output;
		}

        /// <summary>
		///		Output for the current test run.
		/// </summary>
		protected ITestOutputHelper Output { get; }

		/// <summary>
		/// 	Subscribe the test actor to all <see cref="Error"/> messages from the system event stream.
		/// </summary>
		/// <remarks>
		/// 	Use this in combination with <see cref="ExpectNoMsg"/> to ensure no unexpected error messages are swallowed.
		/// </remarks>
		protected virtual void SubscribeToErrors()
		{
			Sys.EventStream.Subscribe(TestActor,
                typeof(Error)
            );
		}

		/// <summary>
		/// 	Unsubscribe the test actor from all <see cref="Error"/> messages from the system event stream.
		/// </summary>
		/// <remarks>
		/// 	Use this in combination with <see cref="ExpectNoMsg"/> to ensure no unexpected error messages are swallowed.
		/// </remarks>
		protected virtual void UnsubscribeFromErrors()
		{
			Sys.EventStream.Unsubscribe(TestActor,
                typeof(Error)
            );
		}
    }
}
