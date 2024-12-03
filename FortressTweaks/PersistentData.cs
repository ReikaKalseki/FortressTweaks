/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 22/02/2022
 * Time: 6:57 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using ReikaKalseki.FortressCore;

namespace ReikaKalseki.FortressTweaks {
	public class PersistentData {
		
		private static readonly object[] values = new object[Enum.GetValues(typeof(Values)).Length];
		
		private static string currentFile;
		
		public static void load(string file) {
			currentFile = file;
			FUtil.log("Loading persistent data from " + currentFile);
			if (!File.Exists(currentFile)) {
				FUtil.log("Cannot load, file does not exist.");
				return;
			}
			XmlDocument doc = new XmlDocument();
			doc.Load(currentFile);
			foreach (XmlNode e in doc.DocumentElement.ChildNodes) {
				if (!(e is XmlElement))
					continue;
				string name = e.Name;
				try {
					Values key = (Values)Enum.Parse(typeof(Values), name);
					values[(int)key] = parseValue((XmlElement)e);
				}
				catch (ArgumentException ex) {
					FUtil.log("Persistent data entry " + name + " did not find a corresponding value mapping. Skipping.");
				}
				catch (Exception ex) {
					FUtil.log("Persistent data entry " + name + " failed to load: " + ex.ToString());
				}
			}
		}
		
		public static V getValue<V>(Values v) {
			return (V)values[(int)v];
		}
		
		public static void setValue(Values v, object current) {
			int idx = (int)v;
			object has = values[idx];
			if (changed(has, current)) {
				values[idx] = current;
				save();
				FUtil.log("Persistent value " + v.ToString() + " changed, from " + (has == null ? "null" : has) + " to " + (current == null ? "null" : current) + ", saving");
			}
		}
		
		private static bool changed(object v1, object v2) {
			if (v1 == v2)
				return false;
			else if (v1 == null || v2 == null)
				return true;
			else if (v1 is string || v1 is bool || v1 is int || v1 is uint || v1 is short || v1 is ushort || v1 is byte || v1 is long || v1 is ulong)
				return v1 != v2;
			else if (v1 is float)
				return !Mathf.Approximately((float)v1, (float)v2);
			else if (v1 is double)
				return Math.Abs((double)v1 - (double)v2) > 0.0001;
			return false;
		}
		
		public static void save() {
			if (string.IsNullOrEmpty(currentFile)) {
				FUtil.log("Could not save persistent data, file is null");
				return;
			}
			FUtil.log("Saving persistent data to " + currentFile);
			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement("Values");
			doc.AppendChild(root);
			foreach (Values v in Enum.GetValues(typeof(Values))) {
				XmlElement e = doc.CreateElement(v.ToString());
				saveValue(values[(int)v], e);
				root.AppendChild(e);
			}
			doc.Save(currentFile);
		}
		
		public static object parseValue(XmlElement e) {
			string type = e.GetAttribute("type").ToLowerInvariant();
			if (string.IsNullOrEmpty(type))
				return null;
			string val = e.GetAttribute("value");
			switch (type) {
				case "string":
					return val;
				case "bool":
					return bool.Parse(val);
				case "int":
				case "int32":
					return int.Parse(val);
				case "uint":
				case "uint32":
					return uint.Parse(val);
				case "byte":
				case "int8":
					return byte.Parse(val);
				case "short":
				case "int16":
					return short.Parse(val);
				case "ushort":
				case "uint16":
					return ushort.Parse(val);
				case "long":
				case "int64":
					return long.Parse(val);
				case "ulong":
				case "uint64":
					return ulong.Parse(val);
				case "float":
				case "single":
					return float.Parse(val);
				case "double":
					return double.Parse(val);
			}
			FUtil.log("Could not parse value '"+val+"' for type "+type);
			return null;
		}
			
		public static void saveValue(object value, XmlElement e) {
			if (value == null) {
				e.InnerText = "null";
				return;
			}
			Type type = value.GetType();
			XmlAttribute attr = e.OwnerDocument.CreateAttribute("type");
			attr.Value = type.Name;
			e.Attributes.Append(attr);
			string str = "";
			if (value is string)
				str = (string)value;
			else if (value is bool || value is int || value is uint || value is byte || value is short || value is ushort || value is long || value is ulong)
				str = value.ToString();
			else if (value is float)
				str = ((float)value).ToString("0.00000000");
			else if (value is double)
				str = ((double)value).ToString("0.0000000000000000");
			else
				str = "Unformattable type";
			attr = e.OwnerDocument.CreateAttribute("value");
			attr.Value = str;
			e.Attributes.Append(attr);
		}
	
		public enum Values {
			HEADLIGHT,
		}
				
	}
}
