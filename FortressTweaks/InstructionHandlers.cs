﻿using System;
using System.IO;    //For data read/write methods
using System.Collections;   //Working with Lists and Collections
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.

namespace ReikaKalseki.FortressTweaks
{
	public class InstructionHandlers
	{
		public InstructionHandlers() {
			
		}
		
		internal static long getIntFromOpcode(CodeInstruction ci) {
			switch (ci.opcode.Name) {
				case "ldc.i4.m1":
				return -1;
				case "ldc.i4.0":
				return 0;
				case "ldc.i4.1":
				return 1;
				case "ldc.i4.2":
				return 2;
				case "ldc.i4.3":
				return 3;
				case "ldc.i4.4":
				return 4;
				case "ldc.i4.5":
				return 5;
				case "ldc.i4.6":
				return 6;
				case "ldc.i4.7":
				return 7;
				case "ldc.i4.8":
				return 8;
				case "ldc.i4.s":
				return (int)((sbyte)ci.operand);
				case "ldc.i4":
				return (int)ci.operand;
				case "ldc.i8":
				return (long)ci.operand;
			default:
				return Int64.MaxValue;
			}
		}
		
		internal static void nullInstructions(List<CodeInstruction> li, int begin, int end) {
			for (int i = begin; i <= end; i++) {
				CodeInstruction insn = li[i];
				insn.opcode = OpCodes.Nop;
				insn.operand = null;
			}
		}
		
		internal static CodeInstruction createMethodCall(string owner, string name, bool instance, params string[] args) {
			return new CodeInstruction(OpCodes.Call, convertMethodOperand(owner, name, instance, args));
		}
		
		internal static CodeInstruction createMethodCall(string owner, string name, bool instance, params Type[] args) {
			return new CodeInstruction(OpCodes.Call, convertMethodOperand(owner, name, instance, args));
		}
		
		internal static MethodInfo convertMethodOperand(string owner, string name, bool instance, params string[] args) {
			Type[] types = new Type[args.Length];
			for (int i = 0; i < args.Length; i++) {
				types[i] = AccessTools.TypeByName(args[i]);
			}
			return convertMethodOperand(owner, name, instance, types);
		}
		
		internal static MethodInfo convertMethodOperand(string owner, string name, bool instance, params Type[] args) {
			MethodInfo ret = AccessTools.Method(AccessTools.TypeByName(owner), name, args);
			//ret.IsStatic = !instance;
			return ret;
		}
		
		internal static FieldInfo convertFieldOperand(string owner, string name) {
			return AccessTools.Field(AccessTools.TypeByName(owner), name);
		}
		
		internal static int getInstruction(List<CodeInstruction> li, int start, int index, OpCode opcode, params object[] args) {
			int count = 0;
			for (int i = start; i < li.Count; i++) {
				CodeInstruction insn = li[i];
				if (insn.opcode == opcode) {
					if (match(insn, args)) {
						if (count == index)
							return i;
						else
							count++;
					}
				}
			}
			return -1;
		}
		
		internal static int getFirstOpcode(List<CodeInstruction> li, int index, OpCode opcode) {
			for (int i = index; i < li.Count; i++) {
				CodeInstruction insn = li[i];
				if (insn.opcode == opcode) {
					return i;
				}
			}
			return -1;
		}
		
		internal static int getLastOpcodeBefore(List<CodeInstruction> li, int before, OpCode opcode) {
			if (before > li.Count)
				before = li.Count;
			for (int i = before-1; i >= 0; i--) {
				CodeInstruction insn = li[i];
				if (insn.opcode == opcode) {
					return i;
				}
			}
			return -1;
		}
		
		internal static int getLastInstructionBefore(List<CodeInstruction> li, int before, OpCode opcode, params object[] args) {
			for (int i = before-1; i >= 0; i--) {
				CodeInstruction insn = li[i];
				if (insn.opcode == opcode) {
					if (match(insn, args)) {
						return i;
					}
				}
			}
			return -1;
		}
		
		internal static bool match(CodeInstruction a, CodeInstruction b) {
			return a.opcode == b.opcode && a.operand == b.operand;
		}
		
		internal static bool match(CodeInstruction insn, params object[] args) {
			//FileLog.Log("Comparing "+insn.operand.GetType()+" "+insn.operand.ToString()+" against seek of "+String.Join(",", args.Select(p=>p.ToString()).ToArray()));
			if (insn.opcode == OpCodes.Call || insn.opcode == OpCodes.Callvirt) { //string class, string name, bool instance, Type[] args
				MethodInfo info = convertMethodOperand((string)args[0], (string)args[1], (bool)args[2], (Type[])args[3]);
				return insn.operand == info;
			}
			else if (insn.opcode == OpCodes.Isinst || insn.opcode == OpCodes.Newobj) { //string class
				return insn.operand == AccessTools.TypeByName((string)args[0]);
			}
			else if (insn.opcode == OpCodes.Ldfld || insn.opcode == OpCodes.Stfld || insn.opcode == OpCodes.Ldsfld || insn.opcode == OpCodes.Stsfld) { //string class, string name
				FieldInfo info = convertFieldOperand((string)args[0], (string)args[1]);
				return insn.operand == info;
			}
			else if (insn.opcode == OpCodes.Ldarg) { //int pos
				return insn.operand == args[0];
			}/*
			else if (insn.opcode == OpCodes.Ldc_I4 || insn.opcode == OpCodes.Ldc_R4 || insn.opcode == OpCodes.Ldc_I8 || insn.opcode == OpCodes.Ldc_R8) { //ldc
				return insn.operand == args[0];
			}*/
			return true;
		}
		
		internal static string toString(List<CodeInstruction> li) {
			return String.Join("\n", li.Select(p=>toString(p)).ToArray());
		}
		
		internal static string toString(List<CodeInstruction> li, int idx) {
			return "#"+idx+" = "+toString(li[idx]);
		}
		
		internal static string toString(CodeInstruction ci) {
			return ci.opcode.Name+" "+(ci.operand != null ? ci.operand.ToString() : "");
		}
	}
}
