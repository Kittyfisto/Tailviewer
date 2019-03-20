using Tailviewer.BusinessLogic.Analysis;

namespace Tailviewer.Test.BusinessLogic
{
	public sealed class TestLogAnalyserConfiguration
		: ILogAnalyserConfiguration
	{
		#region Implementation of ISerializableType

		public void Serialize(IWriter writer)
		{}

		public void Deserialize(IReader reader)
		{}

		#endregion

		#region Implementation of ICloneable

		public object Clone()
		{
			return new TestLogAnalyserConfiguration();
		}

		public bool IsEquivalent(ILogAnalyserConfiguration other)
		{
			return other is TestLogAnalyserConfiguration;
		}

		#endregion
	}
}