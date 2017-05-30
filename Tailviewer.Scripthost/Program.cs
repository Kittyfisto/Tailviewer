using System;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace Tailviewer.Scripthost
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				ExecuteScript();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private static void ExecuteScript()
		{
			var script = @"int Add(int x, int y) {
 return x+y;
 }
 Add(1, 4)";
			//note: we block here, because we are in Main method, normally we could await as scripting APIs are async
			var result = CSharpScript.EvaluateAsync<int>(script).Result;

			//result is now 5
			Console.WriteLine(result);
		}
	}
}
