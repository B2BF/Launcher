using B2BF.Common.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B2BF.Launcher.Helpers
{
    public static class ProfileHelper
    {
        private static int highestProfileId = 0;
        private static bool hasDefaultProfile = false;
        private static List<string> profileNames = new();

        static ProfileHelper()
        {
            var profilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Battlefield 2", "Profiles");
            if (!Directory.Exists(profilesPath))
            {
                Directory.CreateDirectory(profilesPath);
            }

            var profiles = Directory.GetDirectories(profilesPath);
            foreach (var profile in profiles)
            {
                var profileIdStr = profile.Replace(profilesPath + "\\", "");
                if (profileIdStr == "Default")
                {
                    hasDefaultProfile = true;
                    continue;
                }

                if (!int.TryParse(profileIdStr, out var profileId)) continue;

                if (highestProfileId < profileId) highestProfileId = profileId;

                var profileCon = Path.Combine(profilesPath, profileIdStr, "Profile.con");
                if (File.Exists(profileCon))
                {
                    var profileConLines = File.ReadAllLines(profileCon);
                    foreach (var line in profileConLines)
                    {
                        if (line.StartsWith("LocalProfile.setGamespyNick"))
                        {
                            var gameSpyNickQuoted = line.Split(' ')[1];
                            var gameSpyNick = gameSpyNickQuoted.Substring(1, gameSpyNickQuoted.Length - 2);

                            profileNames.Add(gameSpyNick);
                        }
                    }
                }
            }
        }

        public static void CreateProfileIfNotExists()
        {
            if (!profileNames.Contains(AccountInfo.Username, StringComparer.Ordinal))
            {
                highestProfileId++;
                var profilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Battlefield 2", "Profiles", highestProfileId.ToString().PadLeft(4, '0'));
                Directory.CreateDirectory(profilePath);

                var sb = new StringBuilder();
                sb.AppendLine("LocalProfile.setName \"" + AccountInfo.Username + "\"");
                sb.AppendLine("LocalProfile.setNick \"" + AccountInfo.Username + "\"");
                sb.AppendLine("LocalProfile.setGamespyNick \"" + AccountInfo.Username + "\"");
                sb.AppendLine("LocalProfile.setEmail \"private@b2bf.net\"");

                File.WriteAllText(Path.Combine(profilePath, "Profile.con"), sb.ToString());

                if (hasDefaultProfile)
                {
                    var defaultProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Battlefield 2", "Profiles", "Default");

                    if (File.Exists(Path.Combine(defaultProfilePath, "Audio.con")) && !File.Exists(Path.Combine(profilePath, "Audio.con")))
                        File.Copy(Path.Combine(defaultProfilePath, "Audio.con"), Path.Combine(profilePath, "Audio.con"));
                    if (File.Exists(Path.Combine(defaultProfilePath, "General.con")) && !File.Exists(Path.Combine(profilePath, "General.con")))
                        File.Copy(Path.Combine(defaultProfilePath, "General.con"), Path.Combine(profilePath, "General.con"));
                    if (File.Exists(Path.Combine(defaultProfilePath, "ServerSettings.con")) && !File.Exists(Path.Combine(profilePath, "ServerSettings.con")))
                        File.Copy(Path.Combine(defaultProfilePath, "ServerSettings.con"), Path.Combine(profilePath, "ServerSettings.con"));
                }
                else
                {
                    if (!File.Exists(Path.Combine(profilePath, "Audio.con")))
                        File.Create(Path.Combine(profilePath, "Audio.con"));
                    if (!File.Exists(Path.Combine(profilePath, "General.con")))
                        File.Create(Path.Combine(profilePath, "General.con"));
                    if (!File.Exists(Path.Combine(profilePath, "ServerSettings.con")))
                        File.Create(Path.Combine(profilePath, "ServerSettings.con"));
                }
            }
        }
    }
}