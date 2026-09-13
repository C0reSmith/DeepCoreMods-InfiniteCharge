using System;
using System.Runtime.InteropServices;
using System.Threading;
using Assets.Scripts.Networks;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
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

            _running = true;

            _workerThread =
                new Thread(WorkerLoop);

            _workerThread.IsBackground = true;

            _workerThread.Name =
                "DeepCoreMods.InfiniteCharge.Worker";

            _workerThread.Start();

            Plugin.Log.LogInfo(
                "Infinite Charge worker thread started.");
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

            while (_running)
            {
                try
                {
                    CheckToggleKey();

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
        // Toggle Key
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
                _infiniteChargeEnabled =
                    !_infiniteChargeEnabled;

                Plugin.Log.LogInfo(
                    $"Infinite Charge toggled " +
                    $"{(_infiniteChargeEnabled ? "ON" : "OFF")} " +
                    $"by {assignedKey}.");
            }

            _toggleKeyWasDown = keyDown;
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