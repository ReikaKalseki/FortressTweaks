/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 04/11/2019
 * Time: 11:28 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;    //For data read/write methods
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using ReikaKalseki.FortressCore;

namespace ReikaKalseki.FortressTweaks {
	
	[HarmonyPatch(typeof(PlayerBlockPicker))]
	[HarmonyPatch("BlockPicker")]
	public static class MouseTraceRedirect1 {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				Lib.seekAndPatchCubeSelect(codes);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(PlayerBlockPicker))]
	[HarmonyPatch("RefineSelection")]
	public static class MouseTraceRedirect2 {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				Lib.seekAndPatchCubeSelect(codes);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(ConveyorEntity))]
	[HarmonyPatch("LookForHopper")]
	public static class ConveyorItemExtraction {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int loc = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Callvirt, "Segment", "SearchEntity", true, new Type[]{typeof(long), typeof(long), typeof(long)});
				FileLog.Log("Found match at pos "+InstructionHandlers.toString(codes, loc));
				codes.RemoveAt(loc+1); //isinst
				codes[loc].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.FortressTweaks.FortressTweaksMod", "getStorageHandlerForEntityForBelt", false, new Type[]{typeof(Segment), typeof(long), typeof(long), typeof(long), typeof(ConveyorEntity)});
				codes.Insert(loc, new CodeInstruction(OpCodes.Ldarg_0));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(RoomController))]
	[HarmonyPatch("ScanWall")]
	public static class RoomGlassDetection {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int loc = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "CubeHelper", "IsCubeGlass", true, new Type[]{typeof(int)});
				FileLog.Log("Found match at pos "+InstructionHandlers.toString(codes, loc));
				codes[loc].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.FortressTweaks.FortressTweaksMod", "isCubeGlassForRoom", false, new Type[]{typeof(ushort), typeof(RoomController)});
				codes.Insert(loc, new CodeInstruction(OpCodes.Ldarg_0));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(SurvivalGrapplingHook))]
	[HarmonyPatch("Update")]
	public static class GrappleClimateMk3Bypass {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Stfld) {
						CodeInstruction cp = codes[i-1];
						if (cp.opcode == OpCodes.Ldc_R4 || (cp.opcode == OpCodes.Call && cp.operand is MethodInfo && ((MethodInfo)cp.operand).DeclaringType.BaseType == typeof(FortressCraftMod))) { //Mod compat
							FieldInfo look = cp.opcode == OpCodes.Call ? null : InstructionHandlers.convertFieldOperand("SurvivalGrapplingHook", "mrGrappleDebounce");
							if (ci.operand == look || cp.opcode == OpCodes.Call) {
								FileLog.Log("Found match at pos "+InstructionHandlers.toString(codes, i));
								CodeInstruction call = InstructionHandlers.createMethodCall("ReikaKalseki.FortressTweaks.FortressTweaksMod", "getGrappleCooldown", false, new Type[]{typeof(float)});
								codes.Insert(i, call);
								i += 2;
							}
						}
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(GeothermalGenerator))]
	[HarmonyPatch("AttemptDigShaft")]
	public static class GeoShaftDiggingPatch {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int loc = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Call, "TerrainData", "GetHardness", false, new Type[]{typeof(ushort), typeof(ushort)});
				loc = InstructionHandlers.getInstruction(codes, loc, 0, OpCodes.Ldc_I4);
				FileLog.Log("Found match at pos "+InstructionHandlers.toString(codes, loc));
				codes[loc] = InstructionHandlers.createMethodCall("ReikaKalseki.FortressTweaks.FortressTweaksMod", "isCubeGeoPassable", false, new Type[]{typeof(ushort), typeof(GeothermalGenerator)});
				codes[loc+1].opcode = OpCodes.Brfalse;
				codes.Insert(loc, new CodeInstruction(OpCodes.Ldarg_0));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(T4_ParticleFilter))]
	[HarmonyPatch("LowFrequencyUpdate")]
	public static class ParticleSpeedPatch {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int loc = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldsfld, "DifficultySettings", "mbCasualResource");
				loc = InstructionHandlers.getInstruction(codes, loc, 0, OpCodes.Add);
				FileLog.Log("Found match at pos "+InstructionHandlers.toString(codes, loc));
				List<CodeInstruction> inject = new List<CodeInstruction>();
				inject.Add(new CodeInstruction(OpCodes.Ldarg_0));
				inject.Add(InstructionHandlers.createMethodCall("ReikaKalseki.FortressTweaks.FortressTweaksMod", "getProducedGas", false, new Type[]{typeof(int), typeof(T4_ParticleFilter)}));
				codes.InsertRange(loc, inject);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(T4_ParticleCompressor))]
	[HarmonyPatch("LowFrequencyUpdate")]
	public static class CompressorSpeedPatch {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int loc = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldfld, "T4_ParticleCompressor", "CompressionTimer");
				loc = InstructionHandlers.getInstruction(codes, loc, 0, OpCodes.Add);
				FileLog.Log("Found match at pos "+InstructionHandlers.toString(codes, loc));
				List<CodeInstruction> inject = new List<CodeInstruction>();
				inject.Add(new CodeInstruction(OpCodes.Ldarg_0));
				inject.Add(InstructionHandlers.createMethodCall("ReikaKalseki.FortressTweaks.FortressTweaksMod", "getCompressorSpeed", false, new Type[]{typeof(float), typeof(T4_ParticleCompressor)}));
				codes.InsertRange(loc, inject);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(T4_MagmaBore))]
	[HarmonyPatch(MethodType.Constructor, new Type[] {typeof(Segment), typeof(long), typeof(long), typeof(long), typeof(ushort), typeof(byte), typeof(ushort), typeof(bool)})]
	public static class MagmaboreCostPatch {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int loc = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Stfld, "T4_MagmaBore", "TotalDrillPowerRequired");
				FileLog.Log("Found match at pos "+InstructionHandlers.toString(codes, loc));
				List<CodeInstruction> inject = new List<CodeInstruction>();
				inject.Add(new CodeInstruction(OpCodes.Ldarg_0));
				inject.Add(InstructionHandlers.createMethodCall("ReikaKalseki.FortressTweaks.FortressTweaksMod", "getMagmaborePowerCost", false, new Type[]{typeof(float), typeof(T4_MagmaBore)}));
				codes.InsertRange(loc, inject);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
				//FileLog.Log("Codes are "+InstructionHandlers.toString(codes));
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(FALCOR_Beacon))]
	[HarmonyPatch("UpdateClearance")]
	public static class FalcorSkyPatch {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			if (!FortressTweaksMod.getConfig().getBoolean(FTConfig.ConfigEntries.FALCOR_SKY))
				return codes.AsEnumerable();
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Stfld) {
						CodeInstruction cp = codes[i-1];
						FieldInfo look = InstructionHandlers.convertFieldOperand("FALCOR_Beacon", "mbSolarBlocked");
						if (ci.operand == look) {
							FileLog.Log("Found match at pos "+InstructionHandlers.toString(codes, i));
							cp.opcode = OpCodes.Ldc_I4_0;
						}
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(PowerStorageBlock))]
	[HarmonyPatch("TransferPowerToAdjacentItem")]
	public static class PSBSharePatch {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Div) {
						codes.Insert(i+1, InstructionHandlers.createMethodCall("ReikaKalseki.FortressTweaks.FortressTweaksMod", "getSharedPSBPower", false, new Type[]{typeof(float), typeof(PowerStorageBlock), typeof(PowerStorageBlock)}));
						//codes.Insert(i+1, new CodeInstruction(OpCodes.Ldloc_S, 6)); //original amt
						codes.Insert(i+1, new CodeInstruction(OpCodes.Ldloc_S, 5)); //other psb
						codes.Insert(i+1, new CodeInstruction(OpCodes.Ldarg_0)); //this
						break;
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(ItemManager))]
	[HarmonyPatch("UpdateItem")]
	public static class ItemDespawnPatch {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			if (!FortressTweaksMod.getConfig().getBoolean(FTConfig.ConfigEntries.ITEM_DESPAWN))
				return codes.AsEnumerable();
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Sub) {
						CodeInstruction next = codes[i+1];
						if (next.opcode == OpCodes.Stfld && ((FieldInfo)next.operand).Name == "mrLifeRemaining") {
							CodeInstruction prev = codes[i-1];
							if (prev.opcode == OpCodes.Ldsfld) { //IL_002e: ldsfld float32 LowFrequencyThread::mrPreviousUpdateTimeStep
								prev.opcode = OpCodes.Ldc_R4;
								prev.operand = 0F;
								FileLog.Log("Done patch A");
							}
							else if (prev.opcode == OpCodes.Ldc_R4) { //IL_005c: ldc.r4 1
								prev.operand = 0F;
								FileLog.Log("Done patch B");
								break;
							}
						}
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(TD_WaspMob))]
	[HarmonyPatch("DropLoot")]
	public static class TDMobDropGuarantee {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Ldsfld && ((FieldInfo)ci.operand).Name == "mnTotalItemsUpdated") {
						CodeInstruction next = codes[i+1];
						if (ci.opcode == OpCodes.Ldc_I4) {
							next.operand = Int32.MaxValue;
							break;
						}
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(MobSpawnManager))]
	[HarmonyPatch("UpdateBombardment")]
	public static class OrbitalStrikeIntercept {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Callvirt && ((MethodInfo)ci.operand).Name == "Explode") {
						ci.opcode = OpCodes.Call;
						ci.operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.FortressTweaks.FortressTweaksMod", "doOETBlockEffects", false, new Type[]{typeof(WorldScript), typeof(long), typeof(long), typeof(long), typeof(int), typeof(int)});
						break;
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(OrbitalEnergyTransmitter))]
	[HarmonyPatch("LowFrequencyUpdate")]
	public static class OETChargeHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				CodeInstruction call = InstructionHandlers.createMethodCall("ReikaKalseki.FortressTweaks.FortressTweaksMod", "updateOETRequiredCharge", false, new Type[0]);
				codes.Insert(0, call);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(OrbitalStrikeController))]
	[HarmonyPatch("LowFrequencyUpdate")]
	public static class OETCallHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int ret = InstructionHandlers.getLastOpcodeBefore(codes, codes.Count, OpCodes.Ret);
				int call = InstructionHandlers.getLastOpcodeBefore(codes, ret, OpCodes.Callvirt);
				codes[call].opcode = OpCodes.Call;
				codes[call].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.FortressTweaks.FortressTweaksMod", "deleteOET", false, new Type[]{typeof(WorldScript), typeof(Segment), typeof(long), typeof(long), typeof(long), typeof(ushort)});
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(Room_Airlock))]
	[HarmonyPatch("SearchForLink")]
	public static class AirlockSeekHook {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int ret = InstructionHandlers.getLastOpcodeBefore(codes, codes.Count, OpCodes.Ret);
				CodeInstruction call = InstructionHandlers.createMethodCall("ReikaKalseki.FortressTweaks.FortressTweaksMod", "guaranteeAirlock", false, new Type[]{typeof(Room_Airlock)});
				codes.Insert(ret, call);
				CodeInstruction ldself = new CodeInstruction(OpCodes.Ldarg_0);
				codes.Insert(ret, ldself);
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(RoomController))]
	[HarmonyPatch("ScanRoom")]
	public static class AirlockSeekHook2 {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Ldfld && ((FieldInfo)ci.operand).Name == "LinkedAirLock") {
						ci.opcode = OpCodes.Call;
						ci.operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.FortressTweaks.FortressTweaksMod", "guaranteeAirlockDuringCheck", false, new Type[]{typeof(Room_Airlock)});
						break;
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(WormBoss))]
	[HarmonyPatch(MethodType.Constructor, new Type[] {typeof(WormBoss.eBossType)})]
	public static class WormRevealer {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			if (!FortressTweaksMod.getConfig().getBoolean(FTConfig.ConfigEntries.WORM_REVEAL))
				return codes.AsEnumerable();
			try {
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Stfld && ((FieldInfo)ci.operand).Name == "mnHideDistance") {
						codes[i-1].opcode = OpCodes.Ldc_I4;
						codes[i-1].operand = 1024;
					}
					if (ci.opcode == OpCodes.Stfld && ((FieldInfo)ci.operand).Name == "mnEruptDistance") {
						codes[i-1].opcode = OpCodes.Ldc_I4;
						codes[i-1].operand = 512;
						break;
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(MatterMover))]
	[HarmonyPatch(MethodType.Constructor, new Type[] {typeof(Segment), typeof(long), typeof(long), typeof(long), typeof(ushort), typeof(byte), typeof(ushort)})]
	public static class MatterMitterRangeRescale {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Stfld && ((FieldInfo)ci.operand).Name == "mnMaxTransmitDistance") {
						CodeInstruction call = InstructionHandlers.createMethodCall("ReikaKalseki.FortressTweaks.FortressTweaksMod", "setMMRange", false, new Type[]{typeof(MatterMover)});
						codes.Insert(i+1, call);
						CodeInstruction ldself = new CodeInstruction(OpCodes.Ldarg_0);
						codes.Insert(i+1, ldself);
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(InductionCharger))]
	[HarmonyPatch(MethodType.Constructor, new Type[] {typeof(Segment), typeof(long), typeof(long), typeof(long), typeof(ushort), typeof(byte), typeof(ushort), typeof(bool)})]
	public static class InductionChargerUncap {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			int through = FortressTweaksMod.getConfig().getInt(FTConfig.ConfigEntries.INDUCTION_CAP);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Stfld) {
						FieldInfo fi = (FieldInfo)ci.operand;
						if (fi.Name == "mrMaxPower" || fi.Name == "mrMaxTransferRate" || fi.Name == "mrMaxTransferRateOut") {
							codes[i-1].operand = (float)through;
						}
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(T4_Conduit))]
	[HarmonyPatch(MethodType.Constructor, new Type[] {typeof(Segment), typeof(long), typeof(long), typeof(long), typeof(ushort), typeof(byte), typeof(ushort), typeof(bool)})]
	public static class ConduitCapacityBoost {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			int through = FortressTweaksMod.getConfig().getInt(FTConfig.ConfigEntries.CONDUIT_SPEED);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Stfld) {
						FieldInfo fi = (FieldInfo)ci.operand;
						if (fi.Name == "mrMaxPower" || fi.Name == "mrMaxTransferRate") {
							codes[i-1].operand = fi.Name == "mrMaxPower" ? through/4F : (float)through; //transfer +50% and storage 6.5x to fix the difficulty-based scaling
						}
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(FreightCartMob))]
	[HarmonyPatch("SetStatsFromType")]
	public static class NonOreFreightBoost {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			if (!FortressTweaksMod.getConfig().getBoolean(FTConfig.ConfigEntries.FREIGHT_BASIC_BOOST))
				return codes.AsEnumerable();
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Stfld) {
						FieldInfo fi = (FieldInfo)ci.operand;
						if (fi.Name == "mnMaxStorage") {
							long prev = InstructionHandlers.getIntFromOpcode(codes[i-1]);
							if (prev <= 100) {
								int put = (int)(prev*4);
								codes[i-1].operand = put < 127 ? (sbyte)put : put; //x4 like all the others got
								codes[i-1].opcode = put >= 127 ? OpCodes.Ldc_I4 : OpCodes.Ldc_I4_S;
							}
						}
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(FreightCartMob))]
	[HarmonyPatch("UpdateCartUnload")]
	public static class NonOreFreightBoost2 {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			if (!FortressTweaksMod.getConfig().getBoolean(FTConfig.ConfigEntries.FREIGHT_BASIC_BOOST))
				return codes.AsEnumerable();
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Callvirt) {
						MethodInfo fi = (MethodInfo)ci.operand;
						if (fi.Name == "RemoveAnySingle") {
							CodeInstruction prev = codes[i-1];
							if (prev.opcode == OpCodes.Ldc_I4_1) {
								prev.opcode = OpCodes.Ldc_I4_5;
							}
						}
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(FreightCartMob))]
	[HarmonyPatch("GetNextOffer")]
	public static class NonOreFreightBoost3 {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			if (!FortressTweaksMod.getConfig().getBoolean(FTConfig.ConfigEntries.FREIGHT_BASIC_BOOST))
				return codes.AsEnumerable();
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Stloc_S) {
						//Type type = ci.operand != null ? ci.operand.GetType() : null;
						//string name = type != null ? type.FullName : "null";
						//FileLog.Log("Type: "+name+" val = "+ci.operand);
						if (((LocalBuilder)ci.operand).LocalIndex == 4) {
							CodeInstruction prev = codes[i-1];
							if (prev.opcode == OpCodes.Ldc_I4_1) {
								prev.opcode = OpCodes.Ldc_I4_5;
							}
						}
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(BlastFurnace))]
	[HarmonyPatch("UpdateCheckBasin")]
	public static class CastingPipeRangeExtender {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Ldc_I4_S && ((sbyte)ci.operand) == 31) {
						ci.operand = FortressTweaksMod.getConfig().getInt(FTConfig.ConfigEntries.CASTING_PIPE);
						break;
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(GenericAutoCrafterNew))]
	[HarmonyPatch("LowFrequencyUpdate")]
	public static class GACRamp {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Stfld && ((FieldInfo)ci.operand).Name == "mrCraftingTimer") {
						CodeInstruction prev = codes[i-1];
						if (prev.opcode == OpCodes.Sub) {
							CodeInstruction call = InstructionHandlers.createMethodCall("ReikaKalseki.FortressTweaks.FortressTweaksMod", "progressGACTimer", false, new Type[]{typeof(float), typeof(float), typeof(GenericAutoCrafterNew)});
							codes.Insert(i, call);
							CodeInstruction ldself = new CodeInstruction(OpCodes.Ldarg_0);
							codes[i-1] = ldself;
							break;
						}
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(ForcedInduction))]
	[HarmonyPatch("LowFrequencyUpdate")]
	public static class InductionSpeedBoost {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				int idx = 0;
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Callvirt && ((MethodInfo)ci.operand).Name == "SetSmelterBurnRate") {
						CodeInstruction prev = codes[i-1];
						if (prev.opcode == OpCodes.Ldc_R4) {
							float speed = (float)prev.operand;
							if (Math.Abs(4-speed) <= 0.1) {
								prev.operand = FortressTweaksMod.getConfig().getFloat(idx == 2 ? FTConfig.ConfigEntries.FORCED_INDUCTION_5_SPEED : FTConfig.ConfigEntries.FORCED_INDUCTION_4_SPEED);//speed+idx;
								idx++; //after because it fires for mk3 at a speed of 4 too
							}
						}
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(Player))]
	[HarmonyPatch("LowFrequencyUpdate")]
	public static class PlayerCollectionBooster {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Callvirt && ((MethodInfo)ci.operand).Name == "UpdateCollection") {
						ci.opcode = OpCodes.Call;
						ci.operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.FortressTweaks.FortressTweaksMod", "doPlayerItemCollection", false, new Type[]{typeof(ItemManager), typeof(long), typeof(long), typeof(long), typeof(Vector3), typeof(float), typeof(float), typeof(float), typeof(int), typeof(Player)});
						CodeInstruction ldself = new CodeInstruction(OpCodes.Ldarg_0);
						codes.Insert(i, ldself);
						break;
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(AudioMusicManager))]
	[HarmonyPatch("GetCurrentMusicSource")]
	public static class MusicSelectionRewrite {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>();
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
				CodeInstruction call = InstructionHandlers.createMethodCall("ReikaKalseki.FortressTweaks.MusicReplacement", "getMusicCategory", false, new Type[]{typeof(AudioMusicManager)});
				codes.Add(call);
				codes.Add(new CodeInstruction(OpCodes.Ret));
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	/*
	[HarmonyPatch(typeof(AudioMusicManager))]
	[HarmonyPatch("GetCurrentMusicSource")]
	public static class DeadOvermindSoundDisable {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Ldc_I4_S && ((sbyte)ci.operand) == -32) {
						CodeInstruction ci2 = codes[i+2];
						if (ci2.opcode == OpCodes.Ble) {
							CodeInstruction call = InstructionHandlers.createMethodCall("ReikaKalseki.FortressTweaks.FortressTweaksMod", "checkOvermindMusic", false, new Type[0]);
							CodeInstruction skip = new CodeInstruction(OpCodes.Br_S, ci2.operand);
							codes.Insert(i+3, skip);
							codes.Insert(i+3, call);
						}
						break;
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	
	[HarmonyPatch(typeof(AudioMusicManager))]
	[HarmonyPatch("GetCurrentMusicSource")]
	public static class C5MusicSwitchDisable {
		
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
			try {
				FileLog.Log("Running patch "+MethodBase.GetCurrentMethod().DeclaringType);
				for (int i = 0; i < codes.Count; i++) {
					CodeInstruction ci = codes[i];
					if (ci.opcode == OpCodes.Ldsfld && ((FieldInfo)ci.operand).Name == "ActiveAndWorking") {
						ci.operand = InstructionHandlers.convertFieldOperand("ReikaKalseki.FortressTweaks.FortressTweaksMod", "useMagmaMusicInFF");
						break;
					}
				}
				FileLog.Log("Done patch "+MethodBase.GetCurrentMethod().DeclaringType);
			}
			catch (Exception e) {
				FileLog.Log("Caught exception when running patch "+MethodBase.GetCurrentMethod().DeclaringType+"!");
				FileLog.Log(e.Message);
				FileLog.Log(e.StackTrace);
				FileLog.Log(e.ToString());
			}
			return codes.AsEnumerable();
		}
	}
	*/
	static class Lib {
		
		internal static void seekAndPatchCubeSelect(List<CodeInstruction> codes) {
			for (int i = 0; i < codes.Count; i++) {
				CodeInstruction ci = codes[i];
				if (ci.opcode == OpCodes.Call) {
					MethodInfo look = InstructionHandlers.convertMethodOperand("CubeHelper", "IsCubeSelectable", false, new Type[]{typeof(int)});
					if (ci.operand == look) {
						FileLog.Log("Found match at pos "+InstructionHandlers.toString(codes, i));
						patchCubeSelect(codes, i);
					}
				}
			}
		}
		
		private static void patchCubeSelect(List<CodeInstruction> codes, int loc) {
			int nextCall = InstructionHandlers.getLastInstructionBefore(codes, loc, OpCodes.Callvirt, "Segment", "GetCubeDataNoChecking", true, new Type[]{typeof(int), typeof(int), typeof(int), typeof(ushort).MakeByRefType(), typeof(CubeData).MakeByRefType()});
			FileLog.Log("Found ref "+InstructionHandlers.toString(codes, nextCall));
			object val = codes[nextCall-1].operand;
			codes[loc].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.FortressTweaks.FortressTweaksMod", "canCubeBeMouseClicked", false, new Type[]{typeof(ushort), typeof(CubeData).MakeByRefType()});
			CodeInstruction ci = new CodeInstruction(OpCodes.Ldloca_S, val);
			FileLog.Log("Injecting "+InstructionHandlers.toString(ci));
			codes.Insert(loc, ci);
		}
		
	}
}
