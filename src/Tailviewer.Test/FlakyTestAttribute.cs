using System;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace Tailviewer.Test
{
	/// <summary>
	///     This attribute marks a flaky test which should be retried on failure.
	///     This includes tests which fail not due to an assertion failure, but any random
	///     exception (including those which are thrown by FluentAssertion failures).
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class FlakyTestAttribute
		: Attribute
			, IRepeatTest
	{
		private readonly int _tryCount;

		public FlakyTestAttribute(int tryCount)
		{
			if (tryCount < 2)
				throw new ArgumentException("Running flaky tests less than twice does not make sense!");

			_tryCount = tryCount;
		}

		#region Implementation of ICommandWrapper

		public TestCommand Wrap(TestCommand command)
		{
			return new RetryCommand(command, _tryCount);
		}

		#endregion

		private sealed class RetryCommand
			: TestCommand
		{
			private readonly TestCommand _command;
			private readonly int _tryCount;

			public RetryCommand(TestCommand command, int tryCount)
				: base(command.Test)
			{
				_command = command;
				_tryCount = tryCount;
			}

			#region Overrides of TestCommand

			public override TestResult Execute(TestExecutionContext context)
			{
				for (int i = 0; i < _tryCount; ++i)
				{
					try
					{
						context.CurrentResult = _command.Execute(context);
						if (context.CurrentResult?.ResultState != ResultState.Failure)
							break;
					}
					catch (Exception)
					{
						if (i == _tryCount - 1)
							throw;
					}
				}

				return context.CurrentResult;
			}

			#endregion
		}
	}
}