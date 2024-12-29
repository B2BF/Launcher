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
        private static readonly string[] requiredFiles = new string[3] { "Audio.con", "General.con", "ServerSettings.con" };

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
                profileNames.Add(AccountInfo.Username);
                var profilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Battlefield 2", "Profiles", highestProfileId.ToString().PadLeft(4, '0'));
                Directory.CreateDirectory(profilePath);

                var sb = new StringBuilder();
                sb.AppendLine("LocalProfile.setName \"" + AccountInfo.Username + "\"");
                sb.AppendLine("LocalProfile.setNick \"" + AccountInfo.Username + "\"");
                sb.AppendLine("LocalProfile.setGamespyNick \"" + AccountInfo.Username + "\"");
                sb.AppendLine("LocalProfile.setEmail \"private@b2bf.net\"");

                File.WriteAllText(Path.Combine(profilePath, "Profile.con"), sb.ToString());

                var defaultProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Battlefield 2", "Profiles", "Default");

                foreach (var requiredFile in requiredFiles)
                {
                    if (File.Exists(Path.Combine(defaultProfilePath, requiredFile)) && !File.Exists(Path.Combine(profilePath, requiredFile)))
                        File.Copy(Path.Combine(defaultProfilePath, requiredFile), Path.Combine(profilePath, requiredFile));
                    else if ((!hasDefaultProfile || !File.Exists(Path.Combine(defaultProfilePath, requiredFile))) && !File.Exists(Path.Combine(profilePath, requiredFile)))
                        File.Create(Path.Combine(profilePath, requiredFile));
                }

                File.WriteAllText(Path.Combine(profilePath, "Controls.con"),
                    """
                    ControlMap.create InfantryPlayerInputControlMap
                    ControlMap.addKeysToAxisMapping c_PIYaw IDFKeyboard IDKey_D IDKey_A 0
                    ControlMap.addKeysToAxisMapping c_PIThrottle IDFKeyboard IDKey_W IDKey_S 0
                    ControlMap.addButtonToTriggerMapping c_PIFire IDFMouse IDButton_0 0 0
                    ControlMap.addButtonToTriggerMapping c_PIFire IDFFalcon IDButton_0 0 1
                    ControlMap.addKeyToTriggerMapping c_PIAction IDFKeyboard IDKey_Space 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIAltSprint IDFKeyboard IDKey_W 1000 0
                    ControlMap.addKeyToTriggerMapping c_PISprint IDFKeyboard IDKey_LeftShift 0 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect1 IDFKeyboard IDKey_1 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect2 IDFKeyboard IDKey_2 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect3 IDFKeyboard IDKey_3 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect4 IDFKeyboard IDKey_4 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect5 IDFKeyboard IDKey_5 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect6 IDFKeyboard IDKey_6 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect7 IDFKeyboard IDKey_7 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect8 IDFKeyboard IDKey_8 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect9 IDFKeyboard IDKey_9 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect1 IDFKeyboard IDKey_F1 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect2 IDFKeyboard IDKey_F2 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect3 IDFKeyboard IDKey_F3 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect4 IDFKeyboard IDKey_F4 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect5 IDFKeyboard IDKey_F5 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect6 IDFKeyboard IDKey_F6 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect7 IDFKeyboard IDKey_F7 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect8 IDFKeyboard IDKey_F8 10000 0
                    ControlMap.addButtonToTriggerMapping c_PIAltFire IDFMouse IDButton_1 0 0
                    ControlMap.addButtonToTriggerMapping c_PIAltFire IDFFalcon IDButton_3 0 1
                    ControlMap.addKeyToTriggerMapping c_PIDrop IDFKeyboard IDKey_G 10000 0
                    ControlMap.addKeyToTriggerMapping c_PILie IDFKeyboard IDKey_Z 0 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode1 IDFKeyboard IDKey_F9 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode2 IDFKeyboard IDKey_F10 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode3 IDFKeyboard IDKey_F11 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode4 IDFKeyboard IDKey_F12 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIToggleWeapon IDFKeyboard IDKey_F 10000 0
                    ControlMap.addButtonToTriggerMapping c_PIToggleWeapon IDFFalcon IDButton_2 10000 1

                    ControlMap.create LandPlayerInputControlMap
                    ControlMap.addKeysToAxisMapping c_PIYaw IDFKeyboard IDKey_D IDKey_A 0
                    ControlMap.addKeysToAxisMapping c_PIThrottle IDFKeyboard IDKey_W IDKey_S 0
                    ControlMap.addButtonToTriggerMapping c_PIFire IDFMouse IDButton_0 0 0
                    ControlMap.addButtonToTriggerMapping c_PIFire IDFFalcon IDButton_0 0 1
                    ControlMap.addKeyToTriggerMapping c_PISprint IDFKeyboard IDKey_LeftShift 0 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect1 IDFKeyboard IDKey_1 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect2 IDFKeyboard IDKey_2 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect3 IDFKeyboard IDKey_3 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect4 IDFKeyboard IDKey_4 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect5 IDFKeyboard IDKey_5 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect6 IDFKeyboard IDKey_6 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect7 IDFKeyboard IDKey_7 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect8 IDFKeyboard IDKey_8 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect9 IDFKeyboard IDKey_9 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect1 IDFKeyboard IDKey_F1 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect2 IDFKeyboard IDKey_F2 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect3 IDFKeyboard IDKey_F3 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect4 IDFKeyboard IDKey_F4 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect5 IDFKeyboard IDKey_F5 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect6 IDFKeyboard IDKey_F6 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect7 IDFKeyboard IDKey_F7 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect8 IDFKeyboard IDKey_F8 10000 0
                    ControlMap.addButtonToTriggerMapping c_PIAltFire IDFMouse IDButton_1 0 0
                    ControlMap.addButtonToTriggerMapping c_PIAltFire IDFFalcon IDButton_3 0 1
                    ControlMap.addKeyToTriggerMapping c_PICameraMode1 IDFKeyboard IDKey_F9 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode2 IDFKeyboard IDKey_F10 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode3 IDFKeyboard IDKey_F11 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode4 IDFKeyboard IDKey_F12 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIToggleWeapon IDFKeyboard IDKey_F 10000 0
                    ControlMap.addButtonToTriggerMapping c_PIToggleWeapon IDFFalcon IDButton_2 10000 1
                    ControlMap.addKeyToTriggerMapping c_PIFlareFire IDFKeyboard IDKey_X 0 0

                    ControlMap.create AirPlayerInputControlMap
                    ControlMap.addKeysToAxisMapping c_PIYaw IDFKeyboard IDKey_D IDKey_A 0
                    ControlMap.addAxisToAxisMapping c_PIPitch IDFMouse IDAxis_1 0 0
                    ControlMap.addAxisToAxisMapping c_PIPitch IDFFalcon IDAxis_2 0 1
                    ControlMap.addAxisToAxisMapping c_PIRoll IDFMouse IDAxis_0 0 0
                    ControlMap.addAxisToAxisMapping c_PIRoll IDFFalcon IDAxis_0 0 1
                    ControlMap.addKeysToAxisMapping c_PIThrottle IDFKeyboard IDKey_W IDKey_S 0
                    ControlMap.addButtonToTriggerMapping c_PIFire IDFMouse IDButton_0 0 0
                    ControlMap.addButtonToTriggerMapping c_PIFire IDFFalcon IDButton_0 0 1
                    ControlMap.addKeyToTriggerMapping c_PIMouseLook IDFKeyboard IDKey_LeftCtrl 0 0
                    ControlMap.addKeyToTriggerMapping c_PIAltSprint IDFKeyboard IDKey_W 1000 0
                    ControlMap.addKeyToTriggerMapping c_PISprint IDFKeyboard IDKey_LeftShift 0 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect1 IDFKeyboard IDKey_1 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect2 IDFKeyboard IDKey_2 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect3 IDFKeyboard IDKey_3 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect4 IDFKeyboard IDKey_4 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect5 IDFKeyboard IDKey_5 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect6 IDFKeyboard IDKey_6 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect7 IDFKeyboard IDKey_7 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect8 IDFKeyboard IDKey_8 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect9 IDFKeyboard IDKey_9 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect1 IDFKeyboard IDKey_F1 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect2 IDFKeyboard IDKey_F2 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect3 IDFKeyboard IDKey_F3 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect4 IDFKeyboard IDKey_F4 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect5 IDFKeyboard IDKey_F5 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect6 IDFKeyboard IDKey_F6 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect7 IDFKeyboard IDKey_F7 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect8 IDFKeyboard IDKey_F8 10000 0
                    ControlMap.addButtonToTriggerMapping c_PIAltFire IDFMouse IDButton_1 0 0
                    ControlMap.addButtonToTriggerMapping c_PIAltFire IDFFalcon IDButton_3 0 1
                    ControlMap.addKeyToTriggerMapping c_PICameraMode1 IDFKeyboard IDKey_F9 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode2 IDFKeyboard IDKey_F10 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode3 IDFKeyboard IDKey_F11 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode4 IDFKeyboard IDKey_F12 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIToggleWeapon IDFKeyboard IDKey_F 10000 0
                    ControlMap.addButtonToTriggerMapping c_PIToggleWeapon IDFFalcon IDButton_2 10000 1
                    ControlMap.addKeyToTriggerMapping c_PIFlareFire IDFKeyboard IDKey_X 0 0
                    ControlMap.mouseSensitivity 1.7

                    ControlMap.create HelicopterPlayerInputControlMap
                    ControlMap.addKeysToAxisMapping c_PIYaw IDFKeyboard IDKey_D IDKey_A 0
                    ControlMap.addAxisToAxisMapping c_PIPitch IDFMouse IDAxis_1 0 0
                    ControlMap.addAxisToAxisMapping c_PIPitch IDFFalcon IDAxis_2 0 1
                    ControlMap.addAxisToAxisMapping c_PIRoll IDFMouse IDAxis_0 0 0
                    ControlMap.addAxisToAxisMapping c_PIRoll IDFFalcon IDAxis_0 0 1
                    ControlMap.addKeysToAxisMapping c_PIThrottle IDFKeyboard IDKey_W IDKey_S 0
                    ControlMap.addButtonToTriggerMapping c_PIFire IDFMouse IDButton_0 0 0
                    ControlMap.addButtonToTriggerMapping c_PIFire IDFFalcon IDButton_0 0 1
                    ControlMap.addKeyToTriggerMapping c_PIMouseLook IDFKeyboard IDKey_LeftCtrl 0 0
                    ControlMap.addKeyToTriggerMapping c_PIAltSprint IDFKeyboard IDKey_W 1000 0
                    ControlMap.addKeyToTriggerMapping c_PISprint IDFKeyboard IDKey_LeftShift 0 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect1 IDFKeyboard IDKey_1 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect2 IDFKeyboard IDKey_2 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect3 IDFKeyboard IDKey_3 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect4 IDFKeyboard IDKey_4 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect5 IDFKeyboard IDKey_5 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect6 IDFKeyboard IDKey_6 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect7 IDFKeyboard IDKey_7 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect8 IDFKeyboard IDKey_8 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect9 IDFKeyboard IDKey_9 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect1 IDFKeyboard IDKey_F1 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect2 IDFKeyboard IDKey_F2 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect3 IDFKeyboard IDKey_F3 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect4 IDFKeyboard IDKey_F4 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect5 IDFKeyboard IDKey_F5 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect6 IDFKeyboard IDKey_F6 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect7 IDFKeyboard IDKey_F7 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect8 IDFKeyboard IDKey_F8 10000 0
                    ControlMap.addButtonToTriggerMapping c_PIAltFire IDFMouse IDButton_1 0 0
                    ControlMap.addButtonToTriggerMapping c_PIAltFire IDFFalcon IDButton_3 0 1
                    ControlMap.addKeyToTriggerMapping c_PICameraMode1 IDFKeyboard IDKey_F9 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode2 IDFKeyboard IDKey_F10 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode3 IDFKeyboard IDKey_F11 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode4 IDFKeyboard IDKey_F12 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIToggleWeapon IDFKeyboard IDKey_F 10000 0
                    ControlMap.addButtonToTriggerMapping c_PIToggleWeapon IDFFalcon IDButton_2 10000 1
                    ControlMap.addKeyToTriggerMapping c_PIFlareFire IDFKeyboard IDKey_X 0 0
                    ControlMap.mouseSensitivity 3

                    ControlMap.create SeaPlayerInputControlMap
                    ControlMap.addKeysToAxisMapping c_PIYaw IDFKeyboard IDKey_D IDKey_A 0
                    ControlMap.addKeysToAxisMapping c_PIPitch IDFKeyboard IDKey_ArrowUp IDKey_ArrowDown 0
                    ControlMap.addKeysToAxisMapping c_PIThrottle IDFKeyboard IDKey_W IDKey_S 0
                    ControlMap.addButtonToTriggerMapping c_PIFire IDFMouse IDButton_0 0 0
                    ControlMap.addButtonToTriggerMapping c_PIFire IDFFalcon IDButton_0 0 1
                    ControlMap.addKeyToTriggerMapping c_PISprint IDFKeyboard IDKey_LeftShift 0 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect1 IDFKeyboard IDKey_1 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect2 IDFKeyboard IDKey_2 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect3 IDFKeyboard IDKey_3 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect4 IDFKeyboard IDKey_4 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect5 IDFKeyboard IDKey_5 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect6 IDFKeyboard IDKey_6 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect7 IDFKeyboard IDKey_7 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect8 IDFKeyboard IDKey_8 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIWeaponSelect9 IDFKeyboard IDKey_9 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect1 IDFKeyboard IDKey_F1 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect2 IDFKeyboard IDKey_F2 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect3 IDFKeyboard IDKey_F3 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect4 IDFKeyboard IDKey_F4 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect5 IDFKeyboard IDKey_F5 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect6 IDFKeyboard IDKey_F6 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect7 IDFKeyboard IDKey_F7 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIPositionSelect8 IDFKeyboard IDKey_F8 10000 0
                    ControlMap.addButtonToTriggerMapping c_PIAltFire IDFMouse IDButton_1 0 0
                    ControlMap.addButtonToTriggerMapping c_PIAltFire IDFFalcon IDButton_3 0 1
                    ControlMap.addKeyToTriggerMapping c_PICameraMode1 IDFKeyboard IDKey_F9 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode2 IDFKeyboard IDKey_F10 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode3 IDFKeyboard IDKey_F11 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICameraMode4 IDFKeyboard IDKey_F12 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIToggleWeapon IDFKeyboard IDKey_F 10000 0
                    ControlMap.addButtonToTriggerMapping c_PIToggleWeapon IDFFalcon IDButton_2 10000 1

                    ControlMap.create defaultGameControlMap
                    ControlMap.addAxisToAxisMapping c_GIMouseLookX IDFFalcon IDAxis_0 0 0
                    ControlMap.addAxisToAxisMapping c_GIMouseLookX IDFMouse IDAxis_0 0 1
                    ControlMap.addAxisToAxisMapping c_GIMouseLookY IDFFalcon IDAxis_1 0 0
                    ControlMap.addAxisToAxisMapping c_GIMouseLookY IDFMouse IDAxis_1 0 1
                    ControlMap.addKeyToTriggerMapping c_GIMenu IDFKeyboard IDKey_Escape 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIToggleConsole IDFKeyboard IDKey_Grave 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIToggleConsole IDFKeyboard IDKey_End 10000 1
                    ControlMap.addKeyToTriggerMapping c_GIEscape IDFKeyboard IDKey_Escape 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIUp IDFKeyboard IDKey_ArrowUp 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIDown IDFKeyboard IDKey_ArrowDown 10000 0
                    ControlMap.addKeyToTriggerMapping c_GILeft IDFKeyboard IDKey_ArrowLeft 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIRight IDFKeyboard IDKey_ArrowRight 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIPageUp IDFKeyboard IDKey_PageUp 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIPageDown IDFKeyboard IDKey_PageDown 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIRightShift IDFKeyboard IDKey_RightShift 0 0
                    ControlMap.addKeyToTriggerMapping c_GILeftShift IDFKeyboard IDKey_LeftShift 0 0
                    ControlMap.addKeyToTriggerMapping c_GILeftCtrl IDFKeyboard IDKey_LeftCtrl 0 0
                    ControlMap.addKeyToTriggerMapping c_GIRightCtrl IDFKeyboard IDKey_RightCtrl 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIRightAlt IDFKeyboard IDKey_RightAlt 0 0
                    ControlMap.addButtonToTriggerMapping c_GIOk IDFFalcon IDButton_0 0 0
                    ControlMap.addButtonToTriggerMapping c_GIOk IDFMouse IDButton_0 0 1
                    ControlMap.addButtonToTriggerMapping c_GIAltOk IDFFalcon IDButton_3 0 0
                    ControlMap.addButtonToTriggerMapping c_GIAltOk IDFMouse IDButton_1 0 1
                    ControlMap.addKeyToTriggerMapping c_GIScreenShot IDFKeyboard IDKey_PrintScreen 10000 0
                    ControlMap.addKeyToTriggerMapping c_GITogglePause IDFKeyboard IDKey_P 10000 0
                    ControlMap.addKeyToTriggerMapping c_GISayAll IDFKeyboard IDKey_J 10000 0
                    ControlMap.addKeyToTriggerMapping c_GISayTeam IDFKeyboard IDKey_K 10000 0
                    ControlMap.addKeyToTriggerMapping c_GISaySquad IDFKeyboard IDKey_L 10000 0
                    ControlMap.addAxisToTriggerMapping c_GIMouseWheelUp -1 IDFFalcon IDAxis_2 0
                    ControlMap.addAxisToTriggerMapping -1 c_GIMouseWheelDown IDFFalcon IDAxis_2 0
                    ControlMap.addKeyToTriggerMapping c_GITab IDFKeyboard IDKey_Tab 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIEnter IDFKeyboard IDKey_Enter 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIDelete IDFKeyboard IDKey_Delete 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIBack IDFKeyboard IDKey_Backspace 10000 0
                    ControlMap.addKeyToTriggerMapping c_GITacticalComm IDFKeyboard IDKey_T 0 0
                    ControlMap.addKeyToTriggerMapping c_GIRadioComm IDFKeyboard IDKey_Q 0 0
                    ControlMap.addButtonToTriggerMapping c_GIRadioComm IDFFalcon IDButton_1 0 1
                    ControlMap.addKeyToTriggerMapping c_GIMapSize IDFKeyboard IDKey_M 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIMapZoom IDFKeyboard IDKey_N 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIMenuSelect0 IDFKeyboard IDKey_0 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIMenuSelect1 IDFKeyboard IDKey_1 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIMenuSelect2 IDFKeyboard IDKey_2 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIMenuSelect3 IDFKeyboard IDKey_3 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIMenuSelect4 IDFKeyboard IDKey_4 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIMenuSelect5 IDFKeyboard IDKey_5 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIMenuSelect6 IDFKeyboard IDKey_6 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIMenuSelect7 IDFKeyboard IDKey_7 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIMenuSelect8 IDFKeyboard IDKey_8 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIMenuSelect9 IDFKeyboard IDKey_9 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIYes IDFKeyboard IDKey_PageUp 10000 0
                    ControlMap.addKeyToTriggerMapping c_GINo IDFKeyboard IDKey_PageDown 10000 0
                    ControlMap.addKeyToTriggerMapping c_GICreateSquad IDFKeyboard IDKey_Insert 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIToggleFreeCamera IDFKeyboard IDKey_0 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIHotRankUp IDFKeyboard IDKey_Add 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIShowScoreboard IDFKeyboard IDKey_Tab 0 0
                    ControlMap.addKeyToTriggerMapping c_GIVoipUseLeaderChannel IDFKeyboard IDKey_V 0 0
                    ControlMap.addKeyToTriggerMapping c_GI3dMap IDFKeyboard IDKey_LeftAlt 10000 0
                    ControlMap.addKeyToTriggerMapping c_GISelectAll IDFKeyboard IDKey_A 10000 0
                    ControlMap.addKeyToTriggerMapping c_GIDeselectAll IDFKeyboard IDKey_D 10000 0
                    ControlMap.addKeyToTriggerMapping c_GILeader IDFKeyboard IDKey_Capital 10000 0
                    ControlMap.addKeyToTriggerMapping c_GILeader IDFKeyboard IDKey_Home 10000 1
                    ControlMap.addKeyToTriggerMapping c_GIVoipPushToTalk IDFKeyboard IDKey_B 0 0

                    ControlMap.create defaultPlayerInputControlMap
                    ControlMap.addAxisToAxisMapping c_PIMouseLookX IDFFalcon IDAxis_0 0 0
                    ControlMap.addAxisToAxisMapping c_PIMouseLookX IDFMouse IDAxis_0 0 1
                    ControlMap.addAxisToAxisMapping c_PIMouseLookY IDFFalcon IDAxis_1 0 0
                    ControlMap.addAxisToAxisMapping c_PIMouseLookY IDFMouse IDAxis_1 0 1
                    ControlMap.addKeyToTriggerMapping c_PIUse IDFKeyboard IDKey_E 0 0
                    ControlMap.addKeyToTriggerMapping c_PIReload IDFKeyboard IDKey_R 10000 0
                    ControlMap.addKeyToTriggerMapping c_PIToggleCameraMode IDFKeyboard IDKey_C 10000 0
                    ControlMap.addKeyToTriggerMapping c_PICrouch IDFKeyboard IDKey_LeftCtrl 0 0
                    ControlMap.addAxisToTriggerMapping c_PINextItem -1 IDFMouse IDAxis_2 0
                    ControlMap.addAxisToTriggerMapping -1 c_PIPrevItem IDFMouse IDAxis_2 0
                    """);
            }
        }
    }
}