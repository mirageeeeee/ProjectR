using HarmonyLib;
using Lidgren.Network;
using ProjectM;
using ProjectM.Auth;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Terrain;
using RPGMods.Systems;
using RPGMods.Utils;
using Stunlock.Network;
using System;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Wetstone.API;

namespace RPGMods.Hooks
{
    //[HarmonyPatch(typeof(LoadPersistenceSystemV2), nameof(LoadPersistenceSystemV2.SetLoadState))]
    //public class PersistenceSystem_Patch
    //{
    //    public static void Prefix(ServerStartupState.State loadState, LoadPersistenceSystemV2 __instance)
    //    {
    //        if (loadState == ServerStartupState.State.SuccessfulStartup)
    //        {
    //            Plugin.Initialize();
    //        }
    //    }
    //}

    //[HarmonyPatch(typeof(SettingsManager), nameof(SettingsManager.VerifyServerGameSettings))]
    //public class ServerGameSetting_Patch
    //{
    //    private static bool isInitialized = false;
    //    public static void Postfix()
    //    {
    //        if (isInitialized == false)
    //        {
    //            Plugin.Initialize();
    //            isInitialized = true;
    //        }
    //    }
    //}


    [HarmonyPatch(typeof(VBloodSystem), nameof(VBloodSystem.UnlockProgression))]
    public class UnlockProgressionPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
              EntityManager entityManager,
              GameDataSystem gameDataSystem,
              ProgressionUtility.UpdateUnlockedJobData progressionJobData,
              PrefabGUID vBloodUnit,
              Entity userEntity,
              EntityCommandBufferSafe commandBuffer,
              NativeHashMap<PrefabGUID, Entity> prefabMapping,
              Entity progressionEntity,
              bool logOnDuplicate = true)
        {
            var user = entityManager.GetComponentData<User>(userEntity);

            var guid = new PrefabGUID(-910296704);
            if (vBloodUnit == guid)
            {
                var userData = entityManager.GetComponentData<User>(userEntity);
                bool isNewVampire = userData.CharacterName.IsEmpty;

                TaskRunner.Start(world =>
                {
                    var entity = entityManager.CreateEntity(
                        ComponentType.ReadWrite<FromCharacter>(),
                        ComponentType.ReadWrite<PlayerTeleportDebugEvent>()
                    );

                    entityManager.SetComponentData<FromCharacter>(entity, new()
                    {
                        User = userEntity,
                        Character = userData.LocalCharacter._Entity
                    });

                    entityManager.SetComponentData<PlayerTeleportDebugEvent>(entity, new()
                    {
                        Position = new float2(-1062.5f, -2797.35f),
                        Target = PlayerTeleportDebugEvent.TeleportTarget.Self
                    });

                    return null;
                }, HighPriority: true, runNow: true, false, System.TimeSpan.FromSeconds(7));

                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(-850142339), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(1322545846), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(1887724512), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(-2044057823), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(-126076280), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(-2053917766), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(-774462329), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(1389040540), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(488592933), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(-556769032), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(1292986377), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(1634690081), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(-227965303), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(1380368392), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(-296161379), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(-175650376), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(82446940), 1);
                Helper.AddItemToInventoryWithoutChat(entityManager, userData.LocalCharacter._Entity, new PrefabGUID(-674860200), 1);
            }
        }
    }

    [HarmonyPatch(typeof(HandleGameplayEventsSystem), nameof(HandleGameplayEventsSystem.OnUpdate))]
    public class InitializationPatch
    {
        [HarmonyPostfix]
        public static void RPGMods_Initialize_Method()
        {
            Plugin.Initialize();
            Plugin.harmony.Unpatch(typeof(HandleGameplayEventsSystem).GetMethod("OnUpdate"), typeof(InitializationPatch).GetMethod("RPGMods_Initialize_Method"));
        }
    }

    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.Start))]
    public static class GameBootstrap_Patch
    {
        public static void Postfix()
        {
            Plugin.Initialize();
        }
    }

    [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.OnApplicationQuit))]
    public static class GameBootstrapQuit_Patch
    {
        public static void Prefix()
        {
            AutoSaveSystem.SaveDatabase();
        }
    }

    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    public static class OnUserConnected_Patch
    {
        public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            try
            {
                var em = __instance.EntityManager;
                var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
                var serverClient = __instance._ApprovedUsersLookup[userIndex];
                var userEntity = serverClient.UserEntity;
                var userData = __instance.EntityManager.GetComponentData<User>(userEntity);
                bool isNewVampire = userData.CharacterName.IsEmpty;

                //ServerChatUtils.SendSystemMessageToAllClients(__instance.EntityManager, $"Login user - {userData.LocalCharacter} - {userData.LocalCharacter._Entity} - {userEntity}");

                TaskRunner.Start(world =>
                {
                    var entity = em.CreateEntity(
                        ComponentType.ReadWrite<FromCharacter>(),
                        ComponentType.ReadWrite<PlayerTeleportDebugEvent>()
                    );

                    em.SetComponentData<FromCharacter>(entity, new() 
                    { 
                        User = userEntity, 
                        Character = userData.LocalCharacter._Entity 
                    });

                    em.SetComponentData<PlayerTeleportDebugEvent>(entity, new()
                    {
                        Position = new float2(-1062.5f, -2797.35f),
                        Target = PlayerTeleportDebugEvent.TeleportTarget.Self
                    });

                    return null;
                }, HighPriority: true, runNow: true);

                if (!isNewVampire)
                {
                    if (PvPSystem.isHonorSystemEnabled)
                    {
                        if (PvPSystem.isHonorTitleEnabled) Helper.RenamePlayer(userEntity, userData.LocalCharacter._Entity, userData.CharacterName);

                        Database.PvPStats.TryGetValue(userData.PlatformId, out var pvpStats);
                        Database.SiegeState.TryGetValue(userData.PlatformId, out var siegeState);

                        if (pvpStats.Reputation <= -1000)
                        {
                            PvPSystem.HostileON(userData.PlatformId, userData.LocalCharacter._Entity, userEntity);
                        }
                        else
                        {
                            if (!siegeState.IsSiegeOn)
                            {
                                PvPSystem.HostileOFF(userData.PlatformId, userData.LocalCharacter._Entity);
                            }
                        }
                    }
                    else
                    {
                        var playerName = userData.CharacterName.ToString();
                        Helper.UpdatePlayerCache(userEntity, playerName, playerName);
                    }
                }
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
    public static class OnUserDisconnected_Patch
    {
        private static void Prefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId, ConnectionStatusChangeReason connectionStatusReason, string extraData)
        {
            try
            {
                var em = __instance.EntityManager;
                var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
                var serverClient = __instance._ApprovedUsersLookup[userIndex];
                var userEntity = serverClient.UserEntity;
                var userData = __instance.EntityManager.GetComponentData<User>(serverClient.UserEntity);
                bool isNewVampire = userData.CharacterName.IsEmpty;

                TaskRunner.Start(world =>
                {
                    var entity = em.CreateEntity(
                        ComponentType.ReadWrite<FromCharacter>(),
                        ComponentType.ReadWrite<PlayerTeleportDebugEvent>()
                    );

                    em.SetComponentData<FromCharacter>(entity, new()
                    {
                        User = userEntity,
                        Character = userData.LocalCharacter._Entity
                    });

                    em.SetComponentData<PlayerTeleportDebugEvent>(entity, new()
                    {
                        Position = new float2(-1507f, 0.02f),
                        Target = PlayerTeleportDebugEvent.TeleportTarget.Self
                    });

                    return null;
                }, HighPriority: true, runNow: true);

                if (!isNewVampire)
                {
                    var playerName = userData.CharacterName.ToString();
                    Helper.UpdatePlayerCache(serverClient.UserEntity, playerName, playerName, true);
                }
            }
            catch { };
        }
    }
}