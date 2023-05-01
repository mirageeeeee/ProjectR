using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using RPGMods.Commands;
using RPGMods.Hooks;
using RPGMods.Systems;
using RPGMods.Utils;
using System.IO;
using System.Reflection;
using UnhollowerRuntimeLib;
using Unity.Entities;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;

#if WETSTONE
using Wetstone.API;
#endif

namespace RPGMods
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]

#if WETSTONE
    [BepInDependency("xyz.molenzwiebel.wetstone")]
    [Reloadable]
    public class Plugin : BasePlugin, IRunOnInitialized
#else
    public class Plugin : BasePlugin
#endif
    {
        public static Harmony harmony;

        private static ConfigEntry<string> Prefix;
        private static ConfigEntry<string> DisabledCommands;
        private static ConfigEntry<float> DelayedCommands;
        private static ConfigEntry<int> WaypointLimit;

        private static ConfigEntry<bool> EnableVIPSystem;
        private static ConfigEntry<bool> EnableVIPWhitelist;
        private static ConfigEntry<int> VIP_Permission;

        private static ConfigEntry<double> VIP_InCombat_ResYield;
        private static ConfigEntry<double> VIP_InCombat_DurabilityLoss;
        private static ConfigEntry<double> VIP_InCombat_MoveSpeed;
        private static ConfigEntry<double> VIP_InCombat_GarlicResistance;
        private static ConfigEntry<double> VIP_InCombat_SilverResistance;

        private static ConfigEntry<double> VIP_OutCombat_ResYield;
        private static ConfigEntry<double> VIP_OutCombat_DurabilityLoss;
        private static ConfigEntry<double> VIP_OutCombat_MoveSpeed;
        private static ConfigEntry<double> VIP_OutCombat_GarlicResistance;
        private static ConfigEntry<double> VIP_OutCombat_SilverResistance;

        private static ConfigEntry<bool> AnnouncePvPKills;
        private static ConfigEntry<bool> EnablePvPToggle;

        private static ConfigEntry<bool> EnablePvPLadder;
        private static ConfigEntry<int> PvPLadderLength;
        private static ConfigEntry<bool> HonorSortLadder;
        
        private static ConfigEntry<bool> EnablePvPPunish;
        private static ConfigEntry<bool> EnablePvPPunishAnnounce;
        private static ConfigEntry<bool> ExcludeOfflineKills;
        private static ConfigEntry<int> PunishLevelDiff;
        private static ConfigEntry<float> PunishDuration;
        private static ConfigEntry<int> PunishOffenseLimit;
        private static ConfigEntry<float> PunishOffenseCooldown;

        private static ConfigEntry<bool> EnableHonorSystem;
        private static ConfigEntry<bool> EnableHonorTitle;
        private static ConfigEntry<int> MaxHonorGainPerSpan;
        private static ConfigEntry<bool> EnableHonorBenefit;
        private static ConfigEntry<int> HonorSiegeDuration;
        private static ConfigEntry<bool> EnableHostileGlow;
        private static ConfigEntry<bool> UseProximityGlow;

        private static ConfigEntry<bool> EnableWorldDynamics;
        private static ConfigEntry<bool> WDGrowOnKill;


        public static bool isInitialized = false;

        public static ManualLogSource Logger;
        private static World _serverWorld;
        public static World Server
        {
            get
            {
                if (_serverWorld != null) return _serverWorld;

                _serverWorld = GetWorld("Server")
                    ?? throw new System.Exception("There is no Server world (yet). Did you install a server mod on the client?");
                return _serverWorld;
            }
        }

        public static bool IsServer => Application.productName == "VRisingServer";

        private static World GetWorld(string name)
        {
            foreach (var world in World.s_AllWorlds)
            {
                if (world.Name == name)
                {
                    return world;
                }
            }

            return null;
        }

        public void InitConfig()
        {
            Prefix = Config.Bind("Config", "Prefix", ".", "The prefix used for chat commands.");
            DelayedCommands = Config.Bind("Config", "Command Delay", 5f, "The number of seconds user need to wait out before sending another command.\n" +
                "Admin will always bypass this.");
            DisabledCommands = Config.Bind("Config", "Disabled Commands", "", "Enter command names to disable them, abbreviation are included automatically. Seperated by commas.\n" +
                "Ex.: save,godmode");
            WaypointLimit = Config.Bind("Config", "Waypoint Limit", 3, "Set a waypoint limit per user.");

            EnableVIPSystem = Config.Bind("VIP", "Enable VIP System", false, "Enable the VIP System.");
            EnableVIPWhitelist = Config.Bind("VIP", "Enable VIP Whitelist", false, "Enable the VIP user to ignore server capacity limit.");
            VIP_Permission = Config.Bind("VIP", "Minimum VIP Permission", 10, "The minimum permission level required for the user to be considered as VIP.");

            VIP_InCombat_DurabilityLoss = Config.Bind("VIP.InCombat", "Durability Loss Multiplier", 0.5, "Multiply durability loss when user is in combat. -1.0 to disable.\n" +
                "Does not affect durability loss on death.");
            VIP_InCombat_GarlicResistance = Config.Bind("VIP.InCombat", "Garlic Resistance Multiplier", -1.0, "Multiply garlic resistance when user is in combat. -1.0 to disable.");
            VIP_InCombat_SilverResistance = Config.Bind("VIP.InCombat", "Silver Resistance Multiplier", -1.0, "Multiply silver resistance when user is in combat. -1.0 to disable.");
            VIP_InCombat_MoveSpeed = Config.Bind("VIP.InCombat", "Move Speed Multiplier", -1.0, "Multiply move speed when user is in combat. -1.0 to disable.");
            VIP_InCombat_ResYield = Config.Bind("VIP.InCombat", "Resource Yield Multiplier", 2.0, "Multiply resource yield (not item drop) when user is in combat. -1.0 to disable.");

            VIP_OutCombat_DurabilityLoss = Config.Bind("VIP.OutCombat", "Durability Loss Multiplier", 0.5, "Multiply durability loss when user is out of combat. -1.0 to disable.\n" +
                "Does not affect durability loss on death.");
            VIP_OutCombat_GarlicResistance = Config.Bind("VIP.OutCombat", "Garlic Resistance Multiplier", 2.0, "Multiply garlic resistance when user is out of combat. -1.0 to disable.");
            VIP_OutCombat_SilverResistance = Config.Bind("VIP.OutCombat", "Silver Resistance Multiplier", 2.0, "Multiply silver resistance when user is out of combat. -1.0 to disable.");
            VIP_OutCombat_MoveSpeed = Config.Bind("VIP.OutCombat", "Move Speed Multiplier", 1.25, "Multiply move speed when user is out of combat. -1.0 to disable.");
            VIP_OutCombat_ResYield = Config.Bind("VIP.OutCombat", "Resource Yield Multiplier", 2.0, "Multiply resource yield (not item drop) when user is out of combat. -1.0 to disable.");

            AnnouncePvPKills = Config.Bind("PvP", "Announce PvP Kills", true, "Make a server wide announcement for all PvP kills.");
            EnableHonorSystem = Config.Bind("PvP", "Enable Honor System", false, "Enable the honor system.");
            EnableHonorTitle = Config.Bind("PvP", "Enable Honor Title", true, "When enabled, the system will append the title to their name.\nHonor system will leave the player name untouched if disabled.");
            MaxHonorGainPerSpan = Config.Bind("PvP", "Max Honor Gain/Hour", 250, "Maximum amount of honor points the player can gain per hour.");
            EnableHonorBenefit = Config.Bind("PvP", "Enable Honor Benefit & Penalties", true, "If disabled, the hostility state and custom siege system will be disabled.\n" +
                "All other bonus is also not applied.");
            HonorSiegeDuration = Config.Bind("PvP", "Custom Siege Duration", 180, "In minutes. Player will automatically exit siege mode after this many minutes has passed.\n" +
                "Siege mode cannot be exited while duration has not passed.");
            EnableHostileGlow = Config.Bind("PvP", "Enable Hostile Glow", true, "When set to true, hostile players will glow red.");
            UseProximityGlow = Config.Bind("PvP", "Enable Proximity Hostile Glow", true, "If enabled, hostile players will only glow when they are close to other online player.\n" +
                "If disabled, hostile players will always glow red.");
            EnablePvPLadder = Config.Bind("PvP", "Enable PvP Ladder", true, "Enables the PvP Ladder in the PvP command.");
            PvPLadderLength = Config.Bind("PvP", "Ladder Length", 10, "How many players should be displayed in the PvP Ladders.");
            HonorSortLadder = Config.Bind("PvP", "Sort PvP Ladder by Honor", true, "This will automatically be false if honor system is not enabled.");
            EnablePvPToggle = Config.Bind("PvP", "Enable PvP Toggle", false, "Enable/disable the pvp toggle feature in the pvp command.");

            EnablePvPPunish = Config.Bind("PvP", "Enable PvP Punishment", false, "Enables the punishment system for killing lower level player.");
            EnablePvPPunishAnnounce = Config.Bind("PvP", "Enable PvP Punish Announcement", true, "Announce all grief-kills that occured.");
            ExcludeOfflineKills = Config.Bind("PvP", "Exclude Offline Grief", true, "Do not punish the killer if the victim is offline.");
            PunishLevelDiff = Config.Bind("PvP", "Punish Level Difference", -10, "Only punish the killer if the victim level is this much lower.");
            PunishOffenseLimit = Config.Bind("PvP", "Offense Limit", 3, "Killer must make this many offense before the punishment debuff is applied.");
            PunishOffenseCooldown = Config.Bind("PvP", "Offense Cooldown", 300f, "Reset the offense counter after this many seconds has passed since last offense.");
            PunishDuration = Config.Bind("PvP", "Debuff Duration", 1800f, "Apply the punishment debuff for this amount of time.");

            EnableWorldDynamics = Config.Bind("World Dynamics", "Enable Faction Dynamics", true, "All other faction dynamics data & config is withing /RPGMods/Saves/factionstats.json file.");
            WDGrowOnKill = Config.Bind("World Dynamics", "Factions grow on kill", false, "Inverts the faction dynamic system, so that they grow stronger when killed and weaker over time.");

            if (!Directory.Exists("BepInEx/config/RPGMods")) Directory.CreateDirectory("BepInEx/config/RPGMods");
            if (!Directory.Exists("BepInEx/config/RPGMods/Saves")) Directory.CreateDirectory("BepInEx/config/RPGMods/Saves");

            if (!File.Exists("BepInEx/config/RPGMods/kits.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/kits.json");
                stream.Dispose();
            }
        }

        public override void Load()
        {
            InitConfig();
            Logger = Log;
            harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            TaskRunner.Initialize();

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        public override bool Unload()
        {
            AutoSaveSystem.SaveDatabase();
            Config.Clear();
            harmony.UnpatchSelf();

            TaskRunner.Destroy();

            return true;
        }

        public void OnGameInitialized()
        {
            Initialize();
        }

        public static void Initialize()
        {
            //-- Initialize System
            Helper.CreatePlayerCache();
            Helper.GetServerGameSettings(out Helper.SGS);
            Helper.GetServerGameManager(out Helper.SGM);
            Helper.GetUserActivityGridSystem(out Helper.UAGS);
            ProximityLoop.UpdateCache();
            PvPSystem.Interlocked.isSiegeOn = false;

            if (isInitialized) return;

            //-- Commands Related
            AutoSaveSystem.LoadDatabase();

            //-- Apply configs
            CommandHandler.Prefix = Prefix.Value;
            CommandHandler.DisabledCommands = DisabledCommands.Value;
            CommandHandler.delay_Cooldown = DelayedCommands.Value;
            Waypoint.WaypointLimit = WaypointLimit.Value;

            PermissionSystem.isVIPSystem = EnableVIPSystem.Value;
            PermissionSystem.isVIPWhitelist = EnableVIPWhitelist.Value;
            PermissionSystem.VIP_Permission = VIP_Permission.Value;

            PermissionSystem.VIP_InCombat_ResYield = VIP_InCombat_ResYield.Value;
            PermissionSystem.VIP_InCombat_DurabilityLoss = VIP_InCombat_DurabilityLoss.Value;
            PermissionSystem.VIP_InCombat_MoveSpeed = VIP_InCombat_MoveSpeed.Value;
            PermissionSystem.VIP_InCombat_GarlicResistance = VIP_InCombat_GarlicResistance.Value;
            PermissionSystem.VIP_InCombat_SilverResistance = VIP_InCombat_SilverResistance.Value;

            PermissionSystem.VIP_OutCombat_ResYield = VIP_OutCombat_ResYield.Value;
            PermissionSystem.VIP_OutCombat_DurabilityLoss = VIP_OutCombat_DurabilityLoss.Value;
            PermissionSystem.VIP_OutCombat_MoveSpeed = VIP_OutCombat_MoveSpeed.Value;
            PermissionSystem.VIP_OutCombat_GarlicResistance = VIP_OutCombat_GarlicResistance.Value;
            PermissionSystem.VIP_OutCombat_SilverResistance = VIP_OutCombat_SilverResistance.Value;


            PvPSystem.isPvPToggleEnabled = EnablePvPToggle.Value;
            PvPSystem.isAnnounceKills = AnnouncePvPKills.Value;

            PvPSystem.isHonorSystemEnabled = EnableHonorSystem.Value;
            PvPSystem.isHonorTitleEnabled = EnableHonorTitle.Value;
            PvPSystem.MaxHonorGainPerSpan = MaxHonorGainPerSpan.Value;
            PvPSystem.SiegeDuration = HonorSiegeDuration.Value;
            PvPSystem.isHonorBenefitEnabled = EnableHonorBenefit.Value;
            PvPSystem.isEnableHostileGlow = EnableHostileGlow.Value;
            PvPSystem.isUseProximityGlow = UseProximityGlow.Value;

            PvPSystem.isLadderEnabled = EnablePvPLadder.Value;
            PvPSystem.LadderLength = PvPLadderLength.Value;
            PvPSystem.isSortByHonor = HonorSortLadder.Value;
            
            PvPSystem.isPunishEnabled = EnablePvPPunish.Value;
            PvPSystem.isAnnounceGrief = EnablePvPPunishAnnounce.Value;
            PvPSystem.isExcludeOffline = ExcludeOfflineKills.Value;
            PvPSystem.PunishLevelDiff = PunishLevelDiff.Value;
            PvPSystem.PunishDuration = PunishDuration.Value;
            PvPSystem.OffenseLimit = PunishOffenseLimit.Value;
            PvPSystem.Offense_Cooldown = PunishOffenseCooldown.Value;

            isInitialized = true;
        }

        public static int[] parseIntArrayConifg(string data) {
            Plugin.Logger.LogInfo(">>>parsing int array: " + data);
            var match = Regex.Match(data, "([0-9]+)");
            List<int> list = new List<int>();
            while (match.Success) {
                try {
                    Plugin.Logger.LogInfo(">>>got int: " + match.Value);
                    int temp = int.Parse(match.Value, CultureInfo.InvariantCulture);
                    Plugin.Logger.LogInfo(">>>int parsed into: " + temp);
                    list.Add(temp);
                }
                catch {
                    Plugin.Logger.LogWarning("Error interperting integer value: " + match.ToString());
                }
                match = match.NextMatch();
            }
            Plugin.Logger.LogInfo(">>>done parsing int array");
            int[] result = list.ToArray();
            return result;
        }
        public static float[] parseFloatArrayConifg(string data) {
            Plugin.Logger.LogInfo(">>>parsing float array: " + data);
            var match = Regex.Match(data, "[-+]?[0-9]*\\.?[0-9]+");
            List<float> list = new List<float>();
            while (match.Success) {
                try {
                    Plugin.Logger.LogInfo(">>>got float: " + match.Value);
                    float temp = float.Parse(match.Value, CultureInfo.InvariantCulture);
                    Plugin.Logger.LogInfo(">>>float parsed into: " + temp);
                    list.Add(temp);
                }
                catch {
                    Plugin.Logger.LogWarning("Error interperting float value: " + match.ToString());
                }
                
                match = match.NextMatch();
            }

            Plugin.Logger.LogInfo(">>>done parsing float array");
            float[] result = list.ToArray();
            return result;
        }
    }
}
