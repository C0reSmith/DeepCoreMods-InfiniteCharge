using System.Reflection;
using BepInEx;
using HarmonyLib;


namespace DeepCoreMods.InfiniteCharge
{
    [BepInPlugin(
        PluginGuid,
        PluginName,
        PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid =
            "DeepCoreMods.InfiniteCharge";

        public const string PluginName =
            "DeepCore Mods - Infinite Charge";

        public const string PluginVersion =
            "1.0.1";

        internal static BepInEx.Logging.ManualLogSource Log;
        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            // ----------------------------------------------------------
            // Startup
            // ----------------------------------------------------------

            Logger.LogInfo(
                $"{PluginName} v{PluginVersion} loading...");

            Logger.LogInfo(
                $"Plugin DLL: {Assembly.GetExecutingAssembly().Location}");

            // ----------------------------------------------------------
            // Register Harmony patches
            // ----------------------------------------------------------

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            Logger.LogInfo("Harmony patches registered.");

            // ----------------------------------------------------------
            // Register Stationeers keybinding
            // ----------------------------------------------------------

            StationeersKeybind.Register();

            // ----------------------------------------------------------
            // Start Infinite Charge worker
            // ----------------------------------------------------------

            InfiniteChargeTimer.Start();

            Logger.LogInfo(
                $"{PluginName} loaded.");
        }
    }
}