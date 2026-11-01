
using Assets.Scripts;
using Assets.Scripts.Serialization;
using HarmonyLib;
using System.Linq;
using static Assets.Scripts.Serialization.Settings;


namespace DeepCoreMods.InfiniteCharge
{
    [HarmonyPatch(typeof(KeyManager), "SetupKeyBindings")]
    internal static class KeybindingStartupPatch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            StationeersKeybind.Register();
        }
    }


    [HarmonyPatch(typeof(Settings), "LoadSettings")]
    internal static class SettingsLoadedPatch
    {
        internal static bool HasLoadedSettings { get; private set; }

        [HarmonyPostfix]
        private static void Postfix()
        {
            StationeersKeybind.Register();

            HasLoadedSettings = true;

            Plugin.Log.LogInfo(
                "Infinite Charge settings loaded successfully.");
        }
    }
    [HarmonyPatch(typeof(Settings), "SaveSettings")]
    internal static class EarlySettingsSavePatch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            if (SettingsLoadedPatch.HasLoadedSettings)
                return;

            try
            {
                KeyItem activeKey =
                    StationeersKeybind.ToggleKeyItem;

                if (activeKey == null)
                    return;

                SettingData savedSettings = SettingData.Load();

                if (savedSettings?.KeyList == null)
                    return;

                var savedKey = savedSettings.KeyList
                    .FirstOrDefault(k =>
                        k != null &&
                        k.Name == StationeersKeybind.KeyName);

                if (savedKey == null)
                    return;

                activeKey.Key = savedKey.Key;

                Plugin.Log.LogInfo(
                    "Protected Infinite Charge key before early save: " +
                    activeKey.Key);
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogWarning(
                    "Early keybinding protection failed: " + ex);
            }
        }
    }

}
