using System.Reflection;
using Harmony;

namespace WpfUnit
{
	internal static class AssemblySetup
	{
		static AssemblySetup()
		{
			var harmony = HarmonyInstance.Create("com.github.wpfunit");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		public static void EnsureIsPatched()
		{
			
		}
	}
}