using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using RPGMods.Utils;
using RPGMods.Systems;
using Stunlock.Network;
using Unity.Mathematics;

namespace RPGMods.Hooks
{
    [HarmonyPatch(typeof(HandleCreateCharacterEventSystem), nameof(HandleCreateCharacterEventSystem.TryIsNameValid))]
    public class HandleCreateCharacterEventSystem_Patch
    {
        public static void Postfix(HandleCreateCharacterEventSystem __instance, Entity userEntity, string characterNameString, ref CreateCharacterFailureReason invalidReason, ref bool __result)
        {
            /*if (__result)
            {
                __result = Helper.ValidateName(characterNameString, out invalidReason);

                var em = __instance.EntityManager;
                var user = __instance.EntityManager.GetComponentData<User>(userEntity);
                bool isNewVampire = user.CharacterName.IsEmpty;

                TaskRunner.Start(world =>
                {
                    var entity = em.CreateEntity(
                        ComponentType.ReadWrite<FromCharacter>(),
                        ComponentType.ReadWrite<PlayerTeleportDebugEvent>()
                    );

                    em.SetComponentData<FromCharacter>(entity, new()
                    {
                        User = userEntity,
                        Character = __instance.EntityManager.GetComponentData<User>(userEntity).LocalCharacter._Entity
                    });

                    em.SetComponentData<PlayerTeleportDebugEvent>(entity, new()
                    {
                        Position = new float2(-1062f, -2795f),
                        Target = PlayerTeleportDebugEvent.TeleportTarget.Self
                    });

                    return null;
                }, HighPriority: true, runNow: true, false, System.TimeSpan.FromSeconds(20));

                ServerChatUtils.SendSystemMessageToAllClients(em, $"New user - {characterNameString}");

            }*/

            if (PvPSystem.isHonorSystemEnabled)
            {
                if (__result)
                {
                    __result = Helper.ValidateName(characterNameString, out invalidReason);

                    var userData = __instance.EntityManager.GetComponentData<User>(userEntity);
                    characterNameString = "[" + PvPSystem.GetHonorTitle(0).Title + "]" + characterNameString;
                    userData.CharacterName = (FixedString64)characterNameString;
                    __instance.EntityManager.SetComponentData(userEntity, userData);

                    var playerData = new PlayerData(characterNameString, userData.PlatformId, true, userEntity, userData.LocalCharacter._Entity);

                    userData = __instance.EntityManager.GetComponentData<User>(userEntity);

                    Cache.NamePlayerCache[Helper.GetTrueName(characterNameString)] = playerData;
                    Cache.SteamPlayerCache[userData.PlatformId] = playerData;

                    //ServerChatUtils.SendSystemMessageToAllClients(__instance.EntityManager, $"New user - {userData.LocalCharacter} - {userData.LocalCharacter._Entity} - {userEntity}");
                }
            }
        }
    }
}
