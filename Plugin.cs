using NetScriptFramework.SkyrimSE;
using NetScriptFramework.Tools;
using NetScriptFramework;
using System.Reflection;
using System.Linq;
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
        //static uint? lastGauntlets;
        bool nonone = false;
        BGSPerk foSPerk;
        BGSPerk FistsOfSteelPerk => foSPerk ?? (foSPerk = TESForm.LookupFormById(0x00058F6Eu) as BGSPerk);
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
                if (PlayerCharacter.Instance is PlayerCharacter player && player.IsValid && HeavyArmorKeyword is BGSKeyword heavyArmorKeyword) {// && FistsOfSteelPerk is BGSPerk && player.HasPerk(FistsOfSteelPerk)
                    var gauntlets = player.GetEquippedArmorInSlot(EquipSlots.Hands);
                    float value = 0;
                    if (!(gauntlets is null || !gauntlets.HasKeyword(heavyArmorKeyword))) {
                        float skillScaling = 1f;
                        if (SettingsInstance.UseSkill) skillScaling += 0.4f * player.GetActorValue(ActorValueIndices.HeavyArmor) / 100f;
                        float perkScaling = 1f;
                        if (SettingsInstance.UsePerks) {
                            if (player.HasPerk(GetHeavyArmorPerk(4))) {
                                perkScaling += 1f;
                            } else if (player.HasPerk(GetHeavyArmorPerk(3))) {
                                perkScaling += 0.8f;
                            } else if (player.HasPerk(GetHeavyArmorPerk(2))) {
                                perkScaling += 0.6f;
                            } else if (player.HasPerk(GetHeavyArmorPerk(1))) {
                                perkScaling += 0.4f;
                            } else if (player.HasPerk(GetHeavyArmorPerk(0))) {
                                perkScaling += 0.2f;
                            }
                        }
                        float qualityScaling = 0f;
                        if (SettingsInstance.UseQuality) {
                            try {
								if (!(player?.Inventory?.Objects is null)) {
                                    float potentialHealth = 0;
                                    var matches = player.Inventory.Objects.Where(
                                        v0 => !(v0?.ExtraData is null) && v0?.Template?.FormId == gauntlets.FormId && v0.ExtraData.Any(
                                            v1 => {
												if (!(v1 is null) && v1.HasExtraDataByType(ExtraDataTypes.Worn)) {
													if (v1.HasExtraDataByType(ExtraDataTypes.Health)) {
                                                        potentialHealth = (v1.GetExtraDataByType(ExtraDataTypes.Health) as ExtraHealth)?.Health??0;
													}
                                                    return true;
												}
                                                return false;
                                            }
                                        )
                                    );
                                    if (matches.Count() == 1) {
                                        if(potentialHealth > 1) qualityScaling = 1.8f * ((potentialHealth - 1) * 10) - 0.8f;
                                    } else if(!nonone) {
                                        Utils.Log($"Owo? da pwayew is wearing their gauntlets {matches.Count()} times? Che cazzo?");
                                        nonone = true;
                                    }
                                }

                            } catch (Exception ex) {
                                Utils.Log(ex.ToString());
							}
						}
                        value = (float)Math.Ceiling((gauntlets.ArmorRating + qualityScaling) * skillScaling) * perkScaling;
                    }
                    foreach (var item in player.ActiveEffects) if(!(item?.IsInactive??true) && item?.BaseEffect?.PrimaryActorValue == ActorValueIndices.UnarmedDamage && (item.Item.Name.Equals("Fists of Steel"))) {
                        if (Math.Abs(value - item.Magnitude) > float.Epsilon) {
                            Utils.Log($"changed from {item.Magnitude} to {value} ({gauntlets?.Text?.Text??"null"})");
                        }
						player.ModActorValue(ActorValueIndices.UnarmedDamage, value - item.Magnitude);
                        Memory.WriteFloat(item.Address + 0x78, value);
                    }
					/*if (gauntlets?.FormId != lastGauntlets) {
                        Utils.Log(lastGauntlets + "->" + gauntlets?.FormId);
                        lastGauntlets = gauntlets?.FormId;
                        //(player.ExtraDataList.GetExtraDataByType(ExtraDataTypes.ContainerChanges) as ExtraContainerChanges).;
                        if (lastGauntlets.HasValue) {
                            //ptr = gauntlets.Cast<TESObjectREFR>();
                            //Utils.Log(ptr.ToString());
                            //Utils.Log(string.Join(", ", player.Inventory.Objects.Select(v=>$"[{v?.TypeInfo?.Info?.Name}:{string.Join(", ", v?.ExtraData?.Select(v2=>"{"+v2.TypeInfo?.Info?.Name + ":" + (v2?.GetExtraDataByType(ExtraDataTypes.Health) as ExtraHealth)?.Health+"}")??new string[0])}]")));
							foreach (var item in player.Inventory.Objects) {
								try {
                                    Utils.Log((item?.TypeInfo?.Info?.Name ?? "null") + ";" + (item?.Template?.Name ?? "null") + ":");
                                } catch (Exception) { }
                                if (item?.ExtraData is null) continue;
                                foreach (var item1 in item.ExtraData) {
                                    if (item1 is null) continue;
                                    try {
                                        Utils.Log((item1?.TypeInfo?.Info?.Name ?? "null") + ":");
                                        LogExtraData(item1);
								    } catch (Exception) {}
                                }
							}
                        }
                    }*/
                }
            });
            Utils.Log($"Loaded FixOfSteel, OwO");
            return true;
        }
        static void LogExtraData(BSExtraDataList dataList) {
            foreach (ExtraDataTypes item in Enum.GetValues(typeof(ExtraDataTypes))) {
                try {
                    if (dataList.HasExtraDataByType(item)) {
                        BSExtraData extraData = dataList.GetExtraDataByType(item);
                        if (extraData is null) continue;
					    if (extraData is ExtraHealth extraHealth) {
                            Utils.Log("health: " + extraHealth.Health);
                        } else if (extraData is ExtraWorn extraWorn) {
                            Utils.Log("worn: " + extraWorn);
                        }else if (extraData is ExtraObjectHealth extraObjectHealth) {
                            Utils.Log("object health: " + extraObjectHealth.Health);
                        } else if (extraData is ExtraWornLeft extraWornLeft) {
                            Utils.Log("worn: " + extraWornLeft);
                        } else {
                            Utils.Log(extraData.ToString());
                        }
                    }
				} catch (Exception) {}
            }
        }
        static string ToHex(byte val) {
            string o = "";
            if (((val >> 4) & 15) > 9) {
                o += (char)(55 + ((val >> 4) & 15));
            } else {
                o += (char)(48 + ((val >> 4) & 15));
            }
            if ((val & 15) > 9) {
                o += (char)(55 + (val & 15));
            } else {
                o += (char)(48 + (val & 15));
            }
            return o;
        }
    }
}
