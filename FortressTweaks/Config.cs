/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/02/2022
 * Time: 6:01 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace ReikaKalseki.FortressTweaks
{
	public class Config
	{
		private static readonly string FILENAME = "FortressTweaks_Config.xml";
		private readonly Dictionary<string, string> data = new Dictionary<string, string>();
		
		public Config()
		{
			
		}
		
		public void load() {
			string folder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string path = System.IO.Path.Combine(folder, FILENAME);
			if (System.IO.File.Exists(path))
			{
				Util.log("Loading config file at "+path);
				try
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(path);
					Util.log("Config successfully loaded.");
				}
				catch (Exception ex)
				{
					Util.log("Config failed to load: "+ex.ToString());
				}
			}
			else {
				Util.log("Config file does not exist at "+path+"; generating.");
				try
				{
					XmlDocument doc = new XmlDocument();
					XmlElement root = doc.CreateElement("Settings");
					doc.AppendChild(root);
					foreach (ConfigEntries key in Enum.GetValues(typeof(ConfigEntries))) {
						XmlElement node = doc.CreateElement(Enum.GetName(typeof(ConfigEntries), key));
						ConfigEntry e = getEntry(key);
						node.InnerText = e.formatValue(e.defaultValue);
						root.AppendChild(node);
					}
					doc.Save(path);
					Util.log("Default config successfully generated.");
				}
				catch (Exception ex)
				{
					Util.log("Config failed to generate: "+ex.ToString());
				}
			}
		}
		
		private string getString(string key) {
			return data.ContainsKey(key) ? data[key] : null;
		}
		
		public string getString(ConfigEntries key) {
			return getString(Enum.GetName(typeof(ConfigEntries), key));
		}
		
		public bool getBoolean(ConfigEntries key) {
			string ret = getString(key);
			return string.IsNullOrEmpty(ret) ? getEntry(key).defaultValue > 0 : ret.ToLowerInvariant() == "true";
		}
		
		public int getInt(ConfigEntries key) {
			string ret = getString(key);
			return string.IsNullOrEmpty(ret) ? (int)getEntry(key).defaultValue : Int32.Parse(ret);
		}
		
		public float getFloat(ConfigEntries key) {
			string ret = getString(key);
			return string.IsNullOrEmpty(ret) ? getEntry(key).defaultValue : float.Parse(ret);
		}
		
		public ConfigEntry getEntry(ConfigEntries key) {
			MemberInfo info = typeof(ConfigEntries).GetField(Enum.GetName(typeof(ConfigEntries), key));
			return (ConfigEntry)Attribute.GetCustomAttribute(info, typeof(ConfigEntry));
		}
		
		public enum ConfigEntries {
			[ConfigEntry(typeof(int), 40, 1, 5000, 5)]FREIGHT_SPEED,
			[ConfigEntry(typeof(int), 10, 0, 10000, 100)]HOF_CACHE,
			[ConfigEntry(true)]MAGMABORE,
			[ConfigEntry(true)]GAS_SPEED,
			[ConfigEntry(true)]GEO_PIPE_PASS,
			[ConfigEntry(true)]GRAPPLE_COOLDOWN,
			[ConfigEntry(typeof(float), 1, 0, 10, 0)]PSB_SHARE,
			[ConfigEntry(true)]OET,
			[ConfigEntry(typeof(int), 2000000, 1, 100000000, 100000000)]OET_WEAK_COST,
			[ConfigEntry(true)]AIRLOCK,
			[ConfigEntry(typeof(float), 1, 0, 1, 0.5F)]MATTERMITTER_RANGE_FACTOR,
			[ConfigEntry(typeof(int), 16, 0, 64, 0)]MATTERMITTER_RANGE_DROP,
			[ConfigEntry(typeof(int), 96, 16, 256, 31)]CASTING_PIPE,
			[ConfigEntry(true)]FALCOR_SKY,
			[ConfigEntry(false)]ITEM_DESPAWN,
			[ConfigEntry(true)]WORM_REVEAL,
			[ConfigEntry(typeof(int), 8192, 128, 65536, 250)]INDUCTION_CAP,
			[ConfigEntry(typeof(int), 16384, 1024, 65536, 10000)]CONDUIT_SPEED,
			[ConfigEntry(false)]FREIGHT_BASIC_BOOST,
			[ConfigEntry(typeof(float), 0, 0, 4, 0)]GAC_RAMP,
			[ConfigEntry(typeof(float), 5, 4, 8, 4)]FORCED_INDUCTION_4_SPEED,
			[ConfigEntry(typeof(float), 6, 4, 8, 4)]FORCED_INDUCTION_5_SPEED,
			[ConfigEntry(typeof(int), 4, 1, 128, 8)]FI_5_COST4,
			[ConfigEntry(false)]CHEAP_ARC,
			[ConfigEntry(true)]EARLIER_V3_GUN,
		}
		
		public class ConfigEntry : Attribute {
			
			public readonly Type type;
			public readonly float minValue;
			public readonly float maxValue;
			public readonly float defaultValue;
			public readonly float vanillaValue;
			
			public ConfigEntry(bool flag) : this(typeof(bool), flag ? 1 : 0, 0, 1, 0) {
				
			}
			
			public ConfigEntry(Type t, float def, float v) : this(t, def, float.MinValue, float.MaxValue, v) {
				
			}
			
			public ConfigEntry(Type t, float def, float min, float max, float v) {
				type = t;
				defaultValue = def;
				minValue = min;
				maxValue = max;
				vanillaValue = v;
			}
			
			protected bool validate(ref float val) {
				bool flag = true;
				if (val < minValue) {
					val = minValue;
					flag = false;
				}
				else if (val > maxValue) {
					val = maxValue;
					flag = false;
				}
				return flag;
			}
			
			public string formatValue(float value) {
				if (type == typeof(bool)) {
					return (value > 0).ToString();
				}
				else if (type == typeof(int) || type == typeof(uint) || type == typeof(byte) || type == typeof(long) || type == typeof(ulong)) {
					return ((int)(value)).ToString();
				}
				return value.ToString();
			}
			
		}
	}
}
