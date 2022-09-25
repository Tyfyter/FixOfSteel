using NetScriptFramework.SkyrimSE;
using NetScriptFramework.Tools;
using NetScriptFramework;
using System.Reflection;
using System.Linq;
using System;

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
        public static Settings SettingsInstance {
            get;
            private set;
        }
        BGSKeyword hAKwd;
        BGSKeyword HeavyArmorKeyword => hAKwd ?? (hAKwd = TESForm.LookupFormById(0x6BBD2u) as BGSKeyword);
        BGSPerk[] hAPks;
        BGSPerk GetHeavyArmorPerk(int index) {
            if (hAPks?.Length != 5) hAPks = new BGSPerk[5];
			switch (index) {
                default:
                return hAPks[0] ?? (hAPks[0] = TESForm.LookupFormById(0x000BCD2Au) as BGSPerk);
                case 1:
                return hAPks[1] ?? (hAPks[1] = TESForm.LookupFormById(0x0007935Eu) as BGSPerk);
                case 2:
                return hAPks[2] ?? (hAPks[2] = TESForm.LookupFormById(0x00079361u) as BGSPerk);
                case 3:
                return hAPks[3] ?? (hAPks[3] = TESForm.LookupFormById(0x00079362u) as BGSPerk);
                case 4:
                return hAPks[4] ?? (hAPks[4] = TESForm.LookupFormById(0x00079374u) as BGSPerk);
            }
        }
        protected override bool Initialize(bool loadedAny) {
            SettingsInstance = new Settings();
            SettingsInstance.Load();
            Events.OnFrame.Register((e) => {
                NetScriptFramework.SkyrimSE.Main instance = NetScriptFramework.SkyrimSE.Main.Instance;
				if (instance is null || instance.IsGamePaused || !HeavyArmorKeyword.IsValid) return;
                if (HeavyArmorKeyword is null || !HeavyArmorKeyword.IsValid) return;
                if (PlayerCharacter.Instance is PlayerCharacter player && player.IsValid && HeavyArmorKeyword is BGSKeyword heavyArmorKeyword) {
                    var gauntlets = player.GetEquippedArmorInSlot(EquipSlots.Hands);
                    float value = 0;
                    float skillScaling = 1f;
                    if(SettingsInstance.UseSkill) skillScaling += 0.4f * player.GetActorValue(ActorValueIndices.HeavyArmor) / 100f;
                    float perkScaling = 1f;
                    if (SettingsInstance.UsePerks) {
						if (player.HasPerk(GetHeavyArmorPerk(4))) {
                            perkScaling += 1f;
                        }else if (player.HasPerk(GetHeavyArmorPerk(3))) {
                            perkScaling += 0.8f;
                        } else if (player.HasPerk(GetHeavyArmorPerk(2))) {
                            perkScaling += 0.6f;
                        } else if (player.HasPerk(GetHeavyArmorPerk(1))) {
                            perkScaling += 0.4f;
                        } else if (player.HasPerk(GetHeavyArmorPerk(0))) {
                            perkScaling += 0.2f;
                        }
                    }
                    if (!(gauntlets is null || !gauntlets.HasKeyword(heavyArmorKeyword))) {
                        value = (float)Math.Ceiling(gauntlets.ArmorRating * skillScaling) * perkScaling;
                    }
                    foreach (var item in player.ActiveEffects) if(!(item?.IsInactive??true) && item?.BaseEffect?.PrimaryActorValue == ActorValueIndices.UnarmedDamage && (item.Item.Name.Equals("Fists of Steel"))) {
                        player.ModActorValue(ActorValueIndices.UnarmedDamage, value - item.Magnitude);
                        Memory.WriteFloat(item.Address + 0x78, value);
                    }
                }
            });
            Utils.Log($"Loaded FixOfSteel, OwO");
            return true;
        }
    }
}
