﻿using NetScriptFramework.SkyrimSE;
using NetScriptFramework.Tools;
using NetScriptFramework;
using System.Reflection;
using System.Linq;

namespace FixOfSteel {
    public class Plugin : NetScriptFramework.Plugin {
        public override string Key => "fix.of.steel";
        public static string PluginName => "Fix Of Steel";
        public override string Name => PluginName;
        public override int Version => 1;

        public override string Author => "tyfyter";
        public override string Website => "https://tyfyter.github.io/";

        public override int RequiredFrameworkVersion => 9;
        public override int RequiredLibraryVersion => 13;
        BGSKeyword hAKwd;
        BGSKeyword HeavyArmorKeyword => hAKwd ?? (hAKwd = TESForm.LookupFormById(0x6BBD2u) as BGSKeyword);
        protected override bool Initialize(bool loadedAny) {
            if (!loadedAny) {
                Utils.Log("Did not load FixOfSteel");
                return false;
            }
            Events.OnFrame.Register((e) => {
                NetScriptFramework.SkyrimSE.Main instance = NetScriptFramework.SkyrimSE.Main.Instance;
				if (instance is null || instance.IsGamePaused || !HeavyArmorKeyword.IsValid) return;
                if (HeavyArmorKeyword is null || !HeavyArmorKeyword.IsValid) return;
                if (PlayerCharacter.Instance is PlayerCharacter player && player.IsValid && HeavyArmorKeyword is BGSKeyword heavyArmorKeyword) {
                    var gauntlets = player.GetEquippedArmorInSlot(EquipSlots.Hands);
                    float value = 0;
                    if (!(gauntlets is null || !gauntlets.HasKeyword(heavyArmorKeyword))) {
                        value = gauntlets.ArmorRating * (1 + 0.4f * player.GetActorValue(ActorValueIndices.HeavyArmor) / 100f);
                    }
                    foreach (var item in player.ActiveEffects) if(!(item?.IsInactive??true) && item?.BaseEffect?.PrimaryActorValue == ActorValueIndices.UnarmedDamage && (item.Item.Name.Equals("Fists of Steel"))) {
                        Memory.WriteFloat(item.Address + 0x78, value);
                    }
                }
            });
            Utils.Log($"Loaded FixOfSteel, OwO");
            return true;
        }
    }
}
