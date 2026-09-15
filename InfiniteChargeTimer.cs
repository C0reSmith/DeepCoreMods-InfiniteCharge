using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Assets.Scripts.Networks;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using BepInEx;
using UnityEngine;

namespace DeepCoreMods.InfiniteCharge
{
    internal static class InfiniteChargeTimer
    {
        // --------------------------------------------------------------
        // Timing
        // --------------------------------------------------------------

        private const int InitialWorldLoadDelayMs = 15000;
        private const int BatteryUpdateIntervalMs = 250;
        private const int InputPollIntervalMs = 50;
        private const int StreamDeckPollIntervalMs = 250;

        // --------------------------------------------------------------
        // Worker Thread
        // --------------------------------------------------------------

        private static Thread _workerThread;
        private static volatile bool _running;

        // --------------------------------------------------------------
        // Infinite Charge State
        // --------------------------------------------------------------

        private static bool _infiniteChargeEnabled = true;
        private static bool _toggleKeyWasDown;

        // --------------------------------------------------------------
        // Stream Deck Control
        // --------------------------------------------------------------

        private const string ControlFileName =
            "DeepCoreMods.InfiniteCharge.control";

        private const string StatusFileName =
            "DeepCoreMods.InfiniteCharge.status";

        private static string _controlFilePath;
        private static string _statusFilePath;

        // --------------------------------------------------------------
        // Windows Keyboard Input
        // --------------------------------------------------------------

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        // --------------------------------------------------------------
        // Start
        // --------------------------------------------------------------

        public static void Start()
        {
            if (_running)
            {
                return;
            }

            // Build the Stream Deck control paths inside:
            //
            // BepInEx\config\
            //
            _controlFilePath =
                Path.Combine(
                    Paths.ConfigPath,
                    ControlFileName);

            _statusFilePath =
                Path.Combine(
                    Paths.ConfigPath,
                    StatusFileName);

            // Infinite Charge starts ON.
            _infiniteChargeEnabled = true;

            WriteStatusFile();

            _running = true;

            _workerThread =
                new Thread(WorkerLoop);

            _workerThread.IsBackground = true;

            _workerThread.Name =
                "DeepCoreMods.InfiniteCharge.Worker";

            _workerThread.Start();

            Plugin.Log.LogInfo(
                "Infinite Charge worker thread started.");

            Plugin.Log.LogInfo(
                $"Stream Deck control file: {_controlFilePath}");

            Plugin.Log.LogInfo(
                $"Stream Deck status file: {_statusFilePath}");
        }

        // --------------------------------------------------------------
        // Worker
        // --------------------------------------------------------------

        private static void WorkerLoop()
        {
            Plugin.Log.LogInfo(
                "Infinite Charge worker thread is running.");

            // Give Stationeers time to finish loading.
            Thread.Sleep(InitialWorldLoadDelayMs);

            DateTime nextBatteryUpdate =
                DateTime.UtcNow;

            DateTime nextStreamDeckCheck =
                DateTime.UtcNow;

            while (_running)
            {
                try
                {
                    // --------------------------------------------------
                    // Physical keyboard toggle
                    // --------------------------------------------------

                    CheckToggleKey();

                    // --------------------------------------------------
                    // Stream Deck control
                    // --------------------------------------------------

                    if (DateTime.UtcNow >= nextStreamDeckCheck)
                    {
                        nextStreamDeckCheck =
                            DateTime.UtcNow.AddMilliseconds(
                                StreamDeckPollIntervalMs);

                        CheckStreamDeckControl();
                    }

                    // --------------------------------------------------
                    // Battery charging
                    // --------------------------------------------------

                    if (DateTime.UtcNow >= nextBatteryUpdate)
                    {
                        nextBatteryUpdate =
                            DateTime.UtcNow.AddMilliseconds(
                                BatteryUpdateIntervalMs);

                        if (_infiniteChargeEnabled)
                        {
                            ForceBatteriesFull();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError(
                        $"Infinite Charge worker error: {ex}");
                }

                Thread.Sleep(InputPollIntervalMs);
            }

            Plugin.Log.LogInfo(
                "Infinite Charge worker thread stopped.");
        }

        // --------------------------------------------------------------
        // Physical Toggle Key
        // --------------------------------------------------------------

        private static void CheckToggleKey()
        {
            KeyCode assignedKey =
                KeyCode.F7;

            if (StationeersKeybind.ToggleKeyItem != null)
            {
                assignedKey =
                    StationeersKeybind.ToggleKeyItem.Key;
            }

            int virtualKey =
                KeyCodeToVirtualKey(assignedKey);

            if (virtualKey == 0)
            {
                return;
            }

            bool keyDown =
                (GetAsyncKeyState(virtualKey) & 0x8000) != 0;

            if (keyDown && !_toggleKeyWasDown)
            {
                SetInfiniteChargeState(
                    !_infiniteChargeEnabled,
                    assignedKey.ToString());
            }

            _toggleKeyWasDown = keyDown;
        }

        // --------------------------------------------------------------
        // Stream Deck Control
        // --------------------------------------------------------------

        private static void CheckStreamDeckControl()
        {
            if (string.IsNullOrWhiteSpace(_controlFilePath))
            {
                return;
            }

            if (!File.Exists(_controlFilePath))
            {
                return;
            }

            try
            {
                string command =
                    File.ReadAllText(_controlFilePath)
                        .Trim()
                        .ToUpperInvariant();

                // Delete the command file immediately after reading it.
                //
                // This prevents the same command being processed again
                // on the next polling cycle.
                File.Delete(_controlFilePath);

                switch (command)
                {
                    case "ON":

                        SetInfiniteChargeState(
                            true,
                            "Stream Deck");

                        break;

                    case "OFF":

                        SetInfiniteChargeState(
                            false,
                            "Stream Deck");

                        break;

                    case "TOGGLE":

                        SetInfiniteChargeState(
                            !_infiniteChargeEnabled,
                            "Stream Deck");

                        break;

                    default:

                        Plugin.Log.LogWarning(
                            $"Unknown Stream Deck command: {command}");

                        break;
                }
            }
            catch (IOException)
            {
                // Stream Deck may still be writing the file.
                //
                // Ignore this polling cycle and try again shortly.
            }
            catch (UnauthorizedAccessException)
            {
                Plugin.Log.LogWarning(
                    "Unable to access Infinite Charge Stream Deck control file.");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(
                    $"Stream Deck control error: {ex}");
            }
        }

        // --------------------------------------------------------------
        // Change Infinite Charge State
        // --------------------------------------------------------------

        private static void SetInfiniteChargeState(
            bool enabled,
            string source)
        {
            if (_infiniteChargeEnabled == enabled)
            {
                // Still refresh the status file in case another program
                // removed or altered it.
                WriteStatusFile();

                return;
            }

            _infiniteChargeEnabled =
                enabled;

            WriteStatusFile();

            Plugin.Log.LogInfo(
                $"Infinite Charge " +
                $"{(_infiniteChargeEnabled ? "ON" : "OFF")} " +
                $"by {source}.");
        }

        // --------------------------------------------------------------
        // Status File
        // --------------------------------------------------------------

        private static void WriteStatusFile()
        {
            if (string.IsNullOrWhiteSpace(_statusFilePath))
            {
                return;
            }

            try
            {
                File.WriteAllText(
                    _statusFilePath,
                    _infiniteChargeEnabled
                        ? "ON"
                        : "OFF");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning(
                    $"Unable to write Infinite Charge status file: {ex.Message}");
            }
        }

        // --------------------------------------------------------------
        // Battery Charging
        // --------------------------------------------------------------

        private static void ForceBatteriesFull()
        {
            ElectricityManager.AllPoweredThings.ForEach(
                powered =>
                {
                    Battery battery =
                        powered as Battery;

                    if (battery != null)
                    {
                        battery.PowerStored =
                            battery.PowerMaximum;

                        return;
                    }

                    BatteryCell batteryCell =
                        powered as BatteryCell;

                    if (batteryCell != null)
                    {
                        batteryCell.PowerStored =
                            batteryCell.PowerMaximum;
                    }
                });
        }

        // --------------------------------------------------------------
        // Stop
        // --------------------------------------------------------------

        public static void Stop()
        {
            _running = false;
        }

        // --------------------------------------------------------------
        // Unity KeyCode -> Windows Virtual Key
        // --------------------------------------------------------------

        private static int KeyCodeToVirtualKey(KeyCode key)
        {
            // Function keys F1-F12.
            if (key >= KeyCode.F1 &&
                key <= KeyCode.F12)
            {
                return 0x70 +
                    (key - KeyCode.F1);
            }

            // Letters A-Z.
            if (key >= KeyCode.A &&
                key <= KeyCode.Z)
            {
                return 0x41 +
                    (key - KeyCode.A);
            }

            // Number row 0-9.
            if (key >= KeyCode.Alpha0 &&
                key <= KeyCode.Alpha9)
            {
                return 0x30 +
                    (key - KeyCode.Alpha0);
            }

            switch (key)
            {
                case KeyCode.Space:
                    return 0x20;

                case KeyCode.Tab:
                    return 0x09;

                case KeyCode.Return:
                    return 0x0D;

                case KeyCode.Escape:
                    return 0x1B;

                case KeyCode.Backspace:
                    return 0x08;

                case KeyCode.Insert:
                    return 0x2D;

                case KeyCode.Delete:
                    return 0x2E;

                case KeyCode.Home:
                    return 0x24;

                case KeyCode.End:
                    return 0x23;

                case KeyCode.PageUp:
                    return 0x21;

                case KeyCode.PageDown:
                    return 0x22;

                case KeyCode.UpArrow:
                    return 0x26;

                case KeyCode.DownArrow:
                    return 0x28;

                case KeyCode.LeftArrow:
                    return 0x25;

                case KeyCode.RightArrow:
                    return 0x27;

                default:
                    return 0;
            }
        }
    }
}