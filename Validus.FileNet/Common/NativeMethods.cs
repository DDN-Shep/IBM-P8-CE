using System;
using System.Runtime.InteropServices;

namespace Validus.FileNet
{
	internal static class NativeMethods
	{
		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool LogonUser(string username,
		                                      string domain,
		                                      string password,
		                                      int logonType,
		                                      int logonProvider,
		                                      out IntPtr token);
	}
}