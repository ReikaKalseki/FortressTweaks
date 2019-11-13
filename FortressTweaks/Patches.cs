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
				Lib.seekAndPatch(codes);
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
				Lib.seekAndPatch(codes);
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
	
	static class Lib {
		
		internal static void seekAndPatch(List<CodeInstruction> codes) {
			for (int i = 0; i < codes.Count; i++) {
				CodeInstruction ci = codes[i];
				if (ci.opcode == OpCodes.Call) {
					MethodInfo look = InstructionHandlers.convertMethodOperand("CubeHelper", "IsCubeSelectable", false, new Type[]{typeof(int)});
					if (ci.operand == look) {
						FileLog.Log("Found match at pos "+InstructionHandlers.toString(codes, i));
						patch(codes, i);
					}
				}
			}
		}
		
		private static void patch(List<CodeInstruction> codes, int loc) {
			int nextCall = InstructionHandlers.getLastInstructionBefore(codes, loc, OpCodes.Callvirt, "Segment", "GetCubeDataNoChecking", false, new Type[]{typeof(int), typeof(int), typeof(int), typeof(ushort).MakeByRefType(), typeof(CubeData).MakeByRefType()});
			FileLog.Log("Found ref "+InstructionHandlers.toString(codes, nextCall));
			object val = codes[nextCall-1].operand;
			codes[loc].operand = InstructionHandlers.convertMethodOperand("ReikaKalseki.FortressTweaks.FortressTweaksMod", "canCubeBeMouseClicked", false, new Type[]{typeof(ushort), typeof(CubeData).MakeByRefType()});
			CodeInstruction ci = new CodeInstruction(OpCodes.Ldloca_S, val);
			FileLog.Log("Injecting "+InstructionHandlers.toString(ci));
			codes.Insert(loc, ci);
		}
		
	}
}
