using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay;
using RPGMods.Systems;

namespace RPGMods.Hooks
{
    //-- Can Intercept Entity Spawn Here! Nice!
    //var GUID = Helper.GetPrefabGUID(entity);
    //var Name = Helper.GetPrefabName(GUID);
    //Plugin.Logger.LogWarning($"{entity} - {Name}");
    //foreach (var t in __instance.EntityManager.GetComponentTypes(entity))
    //{
    //    Plugin.Logger.LogWarning($"--{t}");
    //}

    [HarmonyPatch(typeof(EntityMetadataSystem), nameof(EntityMetadataSystem.OnUpdate))]
    public class EntityMetadataSystem_Patch
    {
        public static void Prefix(EntityMetadataSystem __instance)
        {

        }
    }
}