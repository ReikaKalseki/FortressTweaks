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
						if (cp.opcode == OpCodes.Ldc_R4) {
							FieldInfo look = InstructionHandlers.convertFieldOperand("SurvivalGrapplingHook", "mrGrappleDebounce");
							if (ci.operand == look) {
								FileLog.Log("Found match at pos "+InstructionHandlers.toString(codes, i));
								CodeInstruction call = InstructionHandlers.createMethodCall("ReikaKalseki.FortressTweaks.FortressTweaksMod", "getGrappleCooldown", false, new Type[]{typeof(float)});
								codes.Insert(i, call);
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
						codes.Insert(i+1, InstructionHandlers.createMethodCall("ReikaKalseki.FortressTweaks.FortressTweaksMod", "getSharedPSBPower", false, new Type[]{typeof(PowerStorageBlock), typeof(PowerStorageBlock), typeof(float)}));
						codes.Insert(i+1, new CodeInstruction(OpCodes.Ldloc_S, 6)); //original amt
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
