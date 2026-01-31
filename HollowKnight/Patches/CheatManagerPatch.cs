using HarmonyLib;
using System;

namespace HollowKnight.Patches
{
    /// <summary>
    /// Harmony patch to enable CheatManager by making IsCheatsEnabled always return true.
    /// This allows the game to naturally create CheatManager on startup.
    /// </summary>
    [HarmonyPatch(typeof(CheatManager), "get_IsCheatsEnabled")]
    public static class CheatManager_IsCheatsEnabled_Patch
    {
        static bool Prefix(ref bool __result)
        {
            // Always enable cheats
            __result = true;
            return false; // Skip original method
        }
    }
}

