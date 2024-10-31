using System.Diagnostics;
using System.Security;
using System.Security.Principal;

namespace B2BF.Launcher.Helpers
{
	public static class PlatformHelper
	{
		static PlatformHelper()
		{
			Win32NT = Environment.OSVersion.Platform == PlatformID.Win32NT;
			XpOrHigher = Win32NT && Environment.OSVersion.Version.Major >= 5;
			VistaOrHigher = Win32NT && Environment.OSVersion.Version.Major >= 6;
			SevenOrHigher = Win32NT && (Environment.OSVersion.Version >= new Version(6, 1));
			EightOrHigher = Win32NT && (Environment.OSVersion.Version >= new Version(6, 2, 9200));
			EightPointOneOrHigher = Win32NT && (Environment.OSVersion.Version >= new Version(6, 3));
			TenOrHigher = Win32NT && (Environment.OSVersion.Version >= new Version(10, 0));
			RunningOnMono = Type.GetType("Mono.Runtime") != null;
			Is64Bit = Environment.Is64BitOperatingSystem;
		}

		public static bool NeedAdmin()
		{
			string testFilePath = Path.Combine(Directory.GetCurrentDirectory(), "testfile.tmp");

			try
			{
				// Attempt to create a test file
				using (FileStream fs = File.Create(testFilePath))
				{
					Console.WriteLine("No admin permissions needed to write to the current directory.");
				}

				// Delete the test file if it was created
				File.Delete(testFilePath);

				return false;
			}
			catch (UnauthorizedAccessException)
			{
				Console.WriteLine("Admin permissions are needed to write to the current directory.");
			}
			catch (SecurityException)
			{
				Console.WriteLine("Admin permissions are needed to write to the current directory.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
			}

			return true;
		}

		public static bool IsRunningAsAdmin()
		{
			using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
			{
				WindowsPrincipal principal = new WindowsPrincipal(identity);
				return principal.IsInRole(WindowsBuiltInRole.Administrator);
			}
		}

		public static void RestartWithoutAdmin()
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = Application.ExecutablePath, // Path to the current executable
				UseShellExecute = true,
				Verb = "runasuser" // "runasuser" is an unofficial workaround for restarting without elevation
			};

			try
			{
				Process.Start(startInfo);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to restart without admin privileges: " + ex.Message);
			}
		}

		public static bool Is64Bit { get; }
		public static bool RunningOnMono { get; }
		public static bool Win32NT { get; }
		public static bool XpOrHigher { get; }
		public static bool VistaOrHigher { get; }
		public static bool SevenOrHigher { get; }
		public static bool EightOrHigher { get; }
		public static bool EightPointOneOrHigher { get; }
		public static bool TenOrHigher { get; }
	}
}