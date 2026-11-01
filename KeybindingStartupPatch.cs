
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
        [HarmonyPostfix]
        private static void Postfix()
        {
            Plugin.Log.LogInfo(
                "Stationeers LoadSettings completed.");

            Plugin.Log.LogInfo(
                "Stationeers settings path: " +
                SettingData.Path);

            StationeersKeybind.Register();

            if (StationeersKeybind.ToggleKeyItem != null)
            {
                Plugin.Log.LogInfo(
                    "Infinite Charge key after settings load: " +
                    StationeersKeybind.ToggleKeyItem.Key);
            }

            var savedKey = Settings.CurrentData?.KeyList?
    .FirstOrDefault(k =>
        k != null &&
        k.Name == StationeersKeybind.KeyName);

            if (savedKey != null)
            {
                Plugin.Log.LogInfo(
                    "Infinite Charge key in saved settings: " +
                    savedKey.Key);

                Plugin.Log.LogInfo(
                    "Saved key and active key are same object: " +
                    object.ReferenceEquals(
                        savedKey,
                        StationeersKeybind.ToggleKeyItem));
            }
            else
            {
                Plugin.Log.LogWarning(
                    "Infinite Charge key not found in saved settings.");
            }
        }
    }
    [HarmonyPatch(typeof(SettingData), "Load")]
    internal static class SettingDataLoadPatch
    {
        [HarmonyPostfix]
        private static void Postfix(SettingData __result)
        {
            var savedKey = __result?.KeyList?
                .FirstOrDefault(k =>
                    k != null &&
                    k.Name == StationeersKeybind.KeyName);

            Plugin.Log.LogInfo(
                "Infinite Charge key immediately after XML load: " +
                (savedKey != null
                    ? savedKey.Key.ToString()
                    : "NOT FOUND"));
        }
    }
}
