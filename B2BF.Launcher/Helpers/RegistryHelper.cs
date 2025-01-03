﻿using B2BF.Common.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2BF.Launcher.Helpers
{
	public class RegistryHelper
	{
		public static string? GetBattlefield2Installation()
		{
			try
			{
				RegistryKey? reg;
				if (!PlatformHelper.Is64Bit)
				{
					reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Electronic Arts\\EA Games\\Battlefield 2");
					if (reg == null)
					{
						throw new Exception();
					}
				}
				else
				{
					reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Electronic Arts\\EA Games\\Battlefield 2");
					if (reg == null)
					{
						throw new Exception();
					}
				}
				if (reg.GetValue("InstallDir") != null)
				{
					return Convert.ToString(reg.GetValue("InstallDir"));
				}
				return null;
			}
			catch (UnauthorizedAccessException)
			{
				//MessageBox.Show("Could not get the registry keys for Battlefield 2. Please start the launcher as administrator");
				return null;
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		public static void WriteBattlefield2Installation()
		{
            try
            {
                RegistryKey? reg;
                if (!PlatformHelper.Is64Bit)
                {
                    reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Electronic Arts\\EA Games\\Battlefield 2", true);
                    if (reg == null)
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Electronic Arts\\EA Games\\Battlefield 2", true);
                    if (reg == null)
                    {
                        throw new Exception();
                    }
                }
				reg.SetValue("InstallDir", Settings.BF2GamePath);
            }
            catch (UnauthorizedAccessException)
            {
                
            }
            catch (Exception ex)
            {
                
            }
        }

		public static void DisableBF2HubAutoPatching()
		{
            try
            {
                RegistryKey? reg = Registry.CurrentUser.OpenSubKey("SOFTWARE\\BF2Hub Systems\\BF2Hub Client", true);
                if (reg == null)
                {
                    throw new Exception();
                }

                reg.SetValue("hrpApplyOnStartup", 0);
                reg.SetValue("hrpInterval", 0);
            }
            catch (UnauthorizedAccessException)
            {

            }
            catch (Exception ex)
            {

            }
        }
	}
}