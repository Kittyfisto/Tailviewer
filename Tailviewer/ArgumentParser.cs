namespace Tailviewer
{
	public static class ArgumentParser
	{
		public static Arguments TryParse(string[] args)
		{
			var arguments = new Arguments();
			if (args?.Length > 0)
			{
				arguments.FilesToOpen = new[] {args[0]};
			}
			else
			{
				arguments.FilesToOpen = new string[0];
			}
			return arguments;
		}

		public sealed class Arguments
		{
			public string[] FilesToOpen;
		}
	}
}