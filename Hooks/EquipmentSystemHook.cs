using HarmonyLib;
using ProjectM.Gameplay.Systems;
using Unity.Entities;
using Unity.Collections;
using ProjectM.Network;
using ProjectM;
using RPGMods.Systems;
using RPGMods.Utils;
using System;

namespace RPGMods.Hooks
{

    [HarmonyPatch(typeof(ArmorLevelSystem_Spawn), nameof(ArmorLevelSystem_Spawn.OnUpdate))]
    public class ArmorLevelSystem_Spawn_Patch
    {
        private static void Prefix(ArmorLevelSystem_Spawn __instance)
        {
            if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;
        }

        private static void Postfix(ArmorLevelSystem_Spawn __instance)
        {
            if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        }
    }

    [HarmonyPatch(typeof(WeaponLevelSystem_Spawn), nameof(WeaponLevelSystem_Spawn.OnUpdate))]
    public class WeaponLevelSystem_Spawn_Patch
    {
        private static void Prefix(WeaponLevelSystem_Spawn __instance)
        {
            if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;
        }

        private static void Postfix(WeaponLevelSystem_Spawn __instance)
        {
            if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        }
    }

    [HarmonyPatch(typeof(SpellLevelSystem_Spawn), nameof(SpellLevelSystem_Spawn.OnUpdate))]
    public class SpellLevelSystem_Spawn_Patch
    {
        private static void Prefix(SpellLevelSystem_Spawn __instance)
        {
            if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        }

        private static void Postfix(SpellLevelSystem_Spawn __instance)
        {
            if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        }
    }

    [HarmonyPatch(typeof(SpellLevelSystem_Destroy), nameof(SpellLevelSystem_Destroy.OnUpdate))]
    public class SpellLevelSystem_Destroy_Patch
    {
        private static void Prefix(SpellLevelSystem_Destroy __instance)
        {
            if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        }

        private static void Postfix(SpellLevelSystem_Destroy __instance)
        {
            if (__instance.__OnUpdate_LambdaJob0_entityQuery == null) return;

        }
    }
}