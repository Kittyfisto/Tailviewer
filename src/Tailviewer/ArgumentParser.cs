namespace Tailviewer
{
	public static class ArgumentParser
	{
		public static Arguments TryParse(string[] args)
		{
			var arguments = new Arguments();
			if (args?.Length > 0)
			{
				arguments.FileToOpen = args[0];
			}
			else
			{
				arguments.FileToOpen = null;
			}
			return arguments;
		}

		public sealed class Arguments
		{
			public string FileToOpen;
		}
	}
}