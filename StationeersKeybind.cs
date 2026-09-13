using Assets.Scripts;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DeepCoreMods.InfiniteCharge
{
    internal static class StationeersKeybind
    {
        public static KeyItem ToggleKeyItem { get; private set; }

        public const string GroupName =
            "DeepCore Mods - Infinite Charge";

        public const string KeyName =
            "Toggle Infinite Charge";

        public static void Register()
        {
            try
            {
                Plugin.Log.LogInfo(
                    "Registering Stationeers keybinding...");

                if (KeyManager.KeyItemLookup == null ||
                    KeyManager.AllKeys == null)
                {
                    Plugin.Log.LogWarning(
                        "KeyManager collections are not available yet.");

                    return;
                }

                ControlsGroup modGroup =
                    ControlsGroup.AllControlGroups.Find(
                        g => g.Name == GroupName);

                if (modGroup == null)
                {
                    modGroup =
                        new ControlsGroup(GroupName);

                    Plugin.Log.LogInfo(
                        $"Created ControlsGroup '{GroupName}'.");
                }

                KeyItem toggleKey;

                if (!KeyManager.KeyItemLookup.TryGetValue(
                        KeyName,
                        out toggleKey))
                {
                    toggleKey =
                        new KeyItem(
                            KeyName,
                            KeyCode.F7,
                            false);

                    KeyManager.KeyItemLookup[KeyName] =
                        toggleKey;

                    if (!KeyManager.AllKeys.Contains(toggleKey))
                    {
                        KeyManager.AllKeys.Add(toggleKey);
                    }

                    Plugin.Log.LogInfo(
                        "Created Infinite Charge KeyItem.");
                }

                ToggleKeyItem =
                    toggleKey;

                if (!modGroup.KeyItems.Contains(toggleKey))
                {
                    modGroup.KeyItems.Add(toggleKey);
                }

                FieldInfo lookupField =
                    typeof(KeyManager).GetField(
                        "_controlsGroupLookup",
                        BindingFlags.Static |
                        BindingFlags.NonPublic);

                if (lookupField != null)
                {
                    Dictionary<string, ControlsGroup> lookup =
                        lookupField.GetValue(null)
                        as Dictionary<string, ControlsGroup>;

                    if (lookup != null)
                    {
                        lookup[KeyName] =
                            modGroup;
                    }
                }

                ControlsAssignment.RefreshState();

                Plugin.Log.LogInfo(
                    $"Stationeers keybinding registration complete. "
                    );
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(
                    "Stationeers keybinding registration failed: " +
                    ex);
            }
        }
    }
}