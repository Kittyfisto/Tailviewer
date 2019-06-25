using System;
using System.Runtime.InteropServices;

namespace Tailviewer.BusinessLogic.FileExplorer
{
	static class NativeMethods
	{
		[DllImport("shell32.dll", ExactSpelling = true)]
		public static extern int SHOpenFolderAndSelectItems(
			 IntPtr pidlFolder,
			 uint cidl,
			 [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl,
			 uint dwFlags);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr ILCreateFromPath([MarshalAs(UnmanagedType.LPTStr)] string pszPath);

		public static void OpenFolderAndSelectFiles(string folder, params string[] filesToSelect)
		{
			IntPtr dir = ILCreateFromPath(folder);

			var filesToSelectIntPtrs = new IntPtr[filesToSelect.Length];
			for (int i = 0; i < filesToSelect.Length; i++)
			{
				filesToSelectIntPtrs[i] = ILCreateFromPath(filesToSelect[i]);
			}

			SHOpenFolderAndSelectItems(dir, (uint)filesToSelect.Length, filesToSelectIntPtrs, 0);
			ReleaseComObject(dir);
			ReleaseComObject(filesToSelectIntPtrs);
		}

		private static void ReleaseComObject(params object[] comObjs)
		{
			foreach (object obj in comObjs)
			{
				if (obj != null && Marshal.IsComObject(obj))
					Marshal.ReleaseComObject(obj);
			}
		}
	}
}