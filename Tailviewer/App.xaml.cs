using System;
using System.IO;
using System.Reflection;

namespace Tailviewer
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		public App()
		{
			AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
		}

		private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
		{
			var name = new AssemblyName(args.Name);
			var fileName = name.Name;

			var resource = string.Format("ThirdParty.{0}.dll", fileName);
			Assembly curAsm = Assembly.GetExecutingAssembly();
			using (Stream stream = curAsm.GetManifestResourceStream(resource))
			{
				if (stream == null)
					return null;

				using (var reader = new StreamReader(stream))
				{
					var data = reader.ReadToEnd();
					return Assembly.Load(data);
				}
			}
		}
	}
}
