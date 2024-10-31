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