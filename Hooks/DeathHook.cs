using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using RPGMods.Commands;
using RPGMods.Systems;
using RPGMods.Utils;
using Unity.Collections;
using Unity.Entities;

namespace RPGMods.Hooks
{
    [HarmonyPatch]
    public class DeathEventListenerSystem_Patch
    {
        [HarmonyPatch(typeof(DeathEventListenerSystem), "OnUpdate")]
        [HarmonyPostfix]
        public static void Postfix(DeathEventListenerSystem __instance)
        {
            if (__instance._DeathEventQuery != null)
            {
                NativeArray<DeathEvent> deathEvents = __instance._DeathEventQuery.ToComponentDataArray<DeathEvent>(Allocator.Temp);
                foreach (DeathEvent ev in deathEvents)
                {
                    var em = __instance.EntityManager;

                    //-- Player Creature Kill Tracking
                    if (em.HasComponent<PlayerCharacter>(ev.Killer) && em.HasComponent<Movement>(ev.Died))
                    {
                        if (PvPSystem.isHonorSystemEnabled) PvPSystem.MobKillMonitor(ev.Killer, ev.Died);

                        /*
                        Blood bloodData = em.GetComponentData<Blood>(ev.Killer);
                            PlayerCharacter p = em.GetComponentData<PlayerCharacter>(ev.Killer);
                            bloodData = em.GetComponentData<Blood>(p.UserEntity.GetEntityOnServer());
                        Output.SendLore(ev.Killer, $"Bloodtype -<color=#fffffffe>{bloodData.BloodType} - {bloodData.Quality}%</color>");*/
                    }
                    //-- ----------------------

                    //-- Auto Respawn & HunterHunted System Begin
                    if (em.HasComponent<PlayerCharacter>(ev.Died))
                    {
                        PlayerCharacter player = em.GetComponentData<PlayerCharacter>(ev.Died);
                        Entity userEntity = player.UserEntity._Entity;
                        User user = em.GetComponentData<User>(userEntity);

                        //-- Check for AutoRespawn
                        if (user.IsConnected)
                        {
                            Utils.RespawnCharacter.Respawn(ev.Died, player, userEntity);
                        }

                        Entity PlayerCharacter = user.LocalCharacter._Entity;
                        string CharName = user.CharacterName.ToString();
                        EntityManager entityManager = Plugin.Server.EntityManager;

                        var UserIndex = user.Index;
                        var component = em.GetComponentData<ProjectM.Health>(PlayerCharacter);
                        int Value = 100;
                        
                        float restore_hp = ((component.MaxHealth / 100) * Value) - component.Value;

                        var HealthEvent = new ChangeHealthDebugEvent()
                        {
                            Amount = (int)restore_hp
                        };
                        Plugin.Server.GetExistingSystem<DebugEventsSystem>().ChangeHealthEvent(UserIndex, ref HealthEvent);

                        var AbilityBuffer = entityManager.GetBuffer<AbilityGroupSlotBuffer>(PlayerCharacter);
                        foreach (var ability in AbilityBuffer)
                        {
                            var AbilitySlot = ability.GroupSlotEntity._Entity;
                            var ActiveAbility = entityManager.GetComponentData<AbilityGroupSlot>(AbilitySlot);
                            var ActiveAbility_Entity = ActiveAbility.StateEntity._Entity;

                            var b = Helper.GetPrefabGUID(ActiveAbility_Entity);
                            if (b.GuidHash == 0) continue;

                            var AbilityStateBuffer = entityManager.GetBuffer<AbilityStateBuffer>(ActiveAbility_Entity);
                            foreach (var state in AbilityStateBuffer)
                            {
                                var abilityState = state.StateEntity._Entity;
                                var abilityCooldownState = entityManager.GetComponentData<AbilityCooldownState>(abilityState);
                                abilityCooldownState.CooldownEndTime = 0;
                                entityManager.SetComponentData(abilityState, abilityCooldownState);
                            }
                        }



                        //-- ---------------------
                    }
                    //-- ----------------------------------------
                }
            }
        }
    }
}