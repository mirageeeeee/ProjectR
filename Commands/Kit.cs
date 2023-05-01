using Il2CppSystem;
using ProjectM;
using RPGMods.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RPGMods.Commands
{
    [Command("kit", "kit <Name>", "Gives you a previously specified set of items.")]
    public static class Kit
    {
        private static List<ItemKit> kits;

        public static void Initialize(Context ctx)
        {
            if (ctx.Args.Length < 1)
            {
                Helper.AddItemToInventory(ctx, new PrefabGUID(-850142339), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(1322545846), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(1887724512), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(-2044057823), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(-126076280), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(-2053917766), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(-774462329), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(1389040540), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(488592933), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(-556769032), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(1292986377), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(1634690081), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(-227965303), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(1380368392), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(-296161379), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(-175650376), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(82446940), 1);
                Helper.AddItemToInventory(ctx, new PrefabGUID(-674860200), 1);
                return;
            }

            string name = string.Join(' ', ctx.Args);

            try
            {
                ItemKit kit = kits.First(x => x.Name.ToLower() == name.ToLower());
                foreach (var guid in kit.PrefabGUIDs)
                {
                    Helper.AddItemToInventory(ctx, new PrefabGUID(guid.Key), guid.Value);
                }
            }
            catch
            {
                Output.SendSystemMessage(ctx, $"Kit doesn't exist.");
                return;
            }
        }

        public static void LoadKits()
        {
            if (!File.Exists("BepInEx/config/RPGMods/kits.json"))
            {
                var stream = File.Create("BepInEx/config/RPGMods/kits.json");
                stream.Dispose();
            }
            string json = File.ReadAllText("BepInEx/config/RPGMods/kits.json");
            try
            {
                kits = JsonSerializer.Deserialize<List<ItemKit>>(json);
                Plugin.Logger.LogWarning("Kits DB Populated.");
            }
            catch
            {
                kits = new List<ItemKit>();
                Plugin.Logger.LogWarning("Kits DB Created.");
            }
        }

        public static void SaveKits()
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                IncludeFields = true
            };
            File.WriteAllText("BepInEx/config/RPGMods/kits.json", JsonSerializer.Serialize(kits, options));
        }
    }
}
