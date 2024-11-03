using System.Xml.XPath;
using System.Xml;
using System.Reflection;
using Sentry;

namespace B2BF.Common.Data
{
    public static class Settings
    {
        private static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "B2BF", "settings.xml");
        public static String RememberMeContainer
        {
            get
            {
                return ReadValueSafe("RememberMeContainer", "");
            }
            set
            {
                WriteValue("RememberMeContainer", value.ToString());
            }
        }
        /*public static GameType LastSelectedGame
        {
            get
            {
                return (GameType)Enum.Parse(typeof(GameType), ReadValueSafe("LastSelectedGame", "PhoenixHeroes"));
            }
            set
            {
                WriteValue("LastSelectedGame", Convert.ToString(value));
            }
        }*/
        public static string Language
        {
            get
            {
                return ReadValueSafe("Language", "English");
            }
            set
            {
                WriteValue("Language", value.ToString());
            }
        }
        public static Boolean Fullscreen
        {
            get
            {
                return Boolean.Parse(ReadValueSafe("Fullscreen", "false"));
            }
            set
            {
                WriteValue("Fullscreen", value.ToString());
            }
        }
        public static Boolean Restart
        {
            get
            {
                return Boolean.Parse(ReadValueSafe("Restart", "false"));
            }
            set
            {
                WriteValue("Restart", value.ToString());
            }
        }
        public static string Mod
        {
            get
            {
                return ReadValueSafe("Mod", "bf2");
            }
            set
            {
                WriteValue("Mod", value.ToString());
            }
        }
        public static Boolean InstallFinished
        {
            get
            {
                return Boolean.Parse(ReadValueSafe("InstallFinished", "false"));
            }
            set
            {
                WriteValue("InstallFinished", value.ToString());
            }
        }
        public static string GamePath
        {
            get
            {
                return ReadValue("GamePath");
                //return Path.Combine((IntPtr.Size == 8) ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Phoenix Network");
            }
            set
            {
                WriteValue("GamePath", value);
            }
        }
        public static string BF2GamePath
        {
            get
            {
                if (File.Exists(Path.Combine(GamePath, "BF2.exe")) || File.Exists(Path.Combine(GamePath, "bf2.exe")))
                    return GamePath;

                return Path.Combine(GamePath, "Battlefield2");
            }
        }
        public static Version GetGameVersion(string Game)
        {
            if (Game == "Battlefield2")
            {
				if (!File.Exists(Path.Combine(BF2GamePath, "version.txt")))
					return null;
				var value2 = File.ReadAllText(Path.Combine(BF2GamePath, "version.txt"));
				Version v2;
				if (!Version.TryParse(value2, out v2))
					return null;
				return v2;
			}

            if (!File.Exists(Path.Combine(GamePath, Game, "version.txt")))
                return null;
            var value = File.ReadAllText(Path.Combine(GamePath, Game, "version.txt"));
            Version v;
            if (!Version.TryParse(value, out v))
                return null;
            return v;
        }
        private static string ReadValue(string pstrValueToRead)
        {
            try
            {
                XPathDocument doc = new XPathDocument(SettingsPath);
                XPathNavigator nav = doc.CreateNavigator();
                XPathExpression expr = nav.Compile(@"/settings/" + pstrValueToRead);
                XPathNodeIterator iterator = nav.Select(expr);
                while (iterator.MoveNext())
                {
                    return iterator.Current.Value;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ReadValueSafe(string pstrValueToRead, string defaultValue = "")
        {
            string value = ReadValue(pstrValueToRead);
            return (!string.IsNullOrEmpty(value)) ? value : (!string.IsNullOrEmpty(defaultValue)) ? defaultValue : null;
        }

        private static void WriteValue(string pstrValueToRead, string pstrValueToWrite)
        {
            try
            {
                XmlDocument doc = new XmlDocument();

                if (File.Exists(SettingsPath))
                {
                    using (var reader = new XmlTextReader(SettingsPath))
                    {
                        doc.Load(reader);
                    }
                }
                else
                {
                    var dir = Path.GetDirectoryName(SettingsPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    doc.AppendChild(doc.CreateElement("settings"));
                }

                XmlElement root = doc.DocumentElement;
                XmlNode oldNode = root.SelectSingleNode(@"/settings/" + pstrValueToRead);
                if (oldNode == null) // create if not exist
                {
                    oldNode = doc.SelectSingleNode("settings");
                    oldNode.AppendChild(doc.CreateElement(pstrValueToRead)).InnerText = pstrValueToWrite;
                    doc.Save(SettingsPath);
                    return;
                }
                oldNode.InnerText = pstrValueToWrite;
                doc.Save(SettingsPath);
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            }
        }
    }
}