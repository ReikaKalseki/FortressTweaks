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
using System.Linq;
using System.Xml;

namespace ReikaKalseki.FortressTweaks
{
	public class Config
	{
		private static readonly string FILENAME = "FortressTweaks_Config.xml";
		private readonly Dictionary<string, float> data = new Dictionary<string, float>();
		
		public Config()
		{
			
		}
		
		public void load() {
			string folder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string path = System.IO.Path.Combine(folder, System.Environment.UserName+"_"+FILENAME);
			if (System.IO.File.Exists(path))
			{
				Util.log("Loading config file at "+path);
				try
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(path);
					XmlElement root = (XmlElement)doc.GetElementsByTagName("Settings")[0];
					foreach (XmlNode e in root.ChildNodes) {
						if (!(e is XmlElement))
							continue;
						string name = e.Name;
						try
						{
							XmlElement val = (XmlElement)(e as XmlElement).GetElementsByTagName("value")[0];
							ConfigEntries key = (ConfigEntries)Enum.Parse(typeof(ConfigEntries), name);
							ConfigEntry entry = getEntry(key);
							float raw = entry.parse(val.InnerText);
							float get = raw;
							if (!entry.validate(ref get)) {
								Util.log("Chosen "+name+" value ("+raw+") was out of bounds, clamed to "+get);
							}
							data[name] = get;
						}
						catch (Exception ex)
						{
							Util.log("Config entry "+name+" failed to load: "+ex.ToString());
						}
					}
					string vals = string.Join(";", data.Select(x => x.Key + "=" + x.Value).ToArray());
					Util.log("Config successfully loaded: "+vals);
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
						createNode(doc, root, key);
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
			
		private void createNode(XmlDocument doc, XmlElement root, ConfigEntries key) {
			ConfigEntry e = getEntry(key);
			XmlElement node = doc.CreateElement(Enum.GetName(typeof(ConfigEntries), key));
			
			XmlComment com = doc.CreateComment(e.desc);
			
			XmlElement val = doc.CreateElement("value");
			val.InnerText = e.formatValue(e.defaultValue);
			node.AppendChild(val);
			
			XmlElement def = doc.CreateElement("defaultValue");
			def.InnerText = e.formatValue(e.defaultValue);
			node.AppendChild(def);
			XmlElement van = doc.CreateElement("vanillaValue");
			van.InnerText = e.formatValue(e.vanillaValue);
			node.AppendChild(van);
			
			//XmlElement desc = doc.CreateElement("description");
			//desc.InnerText = e.desc;
			//node.AppendChild(desc);
			
			if (e.type != typeof(bool)) {
				XmlElement min = doc.CreateElement("minimumValue");
				min.InnerText = e.formatValue(e.minValue);
				node.AppendChild(min);
				XmlElement max = doc.CreateElement("maximumValue");
				max.InnerText = e.formatValue(e.maxValue);
				node.AppendChild(max);
			}
			root.AppendChild(com);
			root.AppendChild(node);
		}
		
		private float getValue(string key) {
			return data.ContainsKey(key) ? data[key] : 0;
		}
		
		public bool getBoolean(ConfigEntries key) {
			float ret = getFloat(key);
			return ret > 0.001;
		}
		
		public int getInt(ConfigEntries key) {
			float ret = getFloat(key);
			return (int)Math.Floor(ret);
		}
		
		public float getFloat(ConfigEntries key) {
			return getValue(Enum.GetName(typeof(ConfigEntries), key));
		}
		
		public ConfigEntry getEntry(ConfigEntries key) {
			MemberInfo info = typeof(ConfigEntries).GetField(Enum.GetName(typeof(ConfigEntries), key));
			return (ConfigEntry)Attribute.GetCustomAttribute(info, typeof(ConfigEntry));
		}
		
		public enum ConfigEntries {
			[ConfigEntry("Ore Freighter Unload Speed", typeof(int), 40, 1, 5000, 5)]FREIGHT_SPEED,
			[ConfigEntry("Fuel Compressor Output Buffer Limit", typeof(int), 10, 0, 10000, 100)]HOF_CACHE,
			[ConfigEntry("Scale MagmaBore drill cost with difficulty", true)]MAGMABORE,
			[ConfigEntry("Boost gas/particle systems when power-rich", true)]GAS_SPEED,
			[ConfigEntry("Allow geo pipe to pass through T5 ores", true)]GEO_PIPE_PASS,
			[ConfigEntry("Allow Mk3 Build Gun full grapple functionality in all caverns", true)]GRAPPLE_COOLDOWN,
			[ConfigEntry("PSB sharing boost from large to small", typeof(float), 1, 0, 10, 0)]PSB_SHARE,
			[ConfigEntry("Enable post-overmind anti-worm OET strikes", true)]OET,
			[ConfigEntry("Low-power anti-worm OET strike power cost", typeof(int), 2000000, 1, 100000000, 100000000)]OET_WEAK_COST,
			[ConfigEntry("Make outer airlocks optional", false)]AIRLOCK,
			[ConfigEntry("Mattermitter range reduction factor by tier", typeof(float), 0.5F, 0, 1, 0.5F)]MATTERMITTER_RANGE_FACTOR,
			[ConfigEntry("Mattermitter range flat reduction by tier", typeof(int), 0, 0, 64, 0)]MATTERMITTER_RANGE_DROP,
			[ConfigEntry("Casting pipe max range", typeof(int), 96, 16, 256, 31)]CASTING_PIPE,
			[ConfigEntry("Remove FALCOR beacon sky access requirement", true)]FALCOR_SKY,
			[ConfigEntry("Disable item despawning", false)]ITEM_DESPAWN,
			[ConfigEntry("Force worms to reveal at all distances", true)]WORM_REVEAL,
			[ConfigEntry("Induction charger PPS cap", typeof(int), 8192, 128, 65536, 250)]INDUCTION_CAP,
			[ConfigEntry("Conduit PPS cap", typeof(int), 16384, 1024, 65536, 10000)]CONDUIT_SPEED,
			[ConfigEntry("Boost basic carts' capacity when used as freight", false)]FREIGHT_BASIC_BOOST,
			[ConfigEntry("GAC power-rich speed bonus factor", typeof(float), 0, 0, 4, 0)]GAC_RAMP,
			[ConfigEntry("Forced Induction Mk4 smelting speed", typeof(float), 5, 4, 8, 4)]FORCED_INDUCTION_4_SPEED,
			[ConfigEntry("Forced Induction Mk5 smelting speed", typeof(float), 6, 4, 8, 4)]FORCED_INDUCTION_5_SPEED,
			[ConfigEntry("Forced Induction Mk5 cost in Mk4s", typeof(int), 4, 1, 128, 8)]FI_5_COST4,
			[ConfigEntry("Reduce ARC Smelter Upgrade material cost", false)]CHEAP_ARC,
			[ConfigEntry("Move V3 build gun to early FF", true)]EARLIER_V3_GUN,
		}
		
		public class ConfigEntry : Attribute {
			
			public readonly string desc;
			public readonly Type type;
			public readonly float minValue;
			public readonly float maxValue;
			public readonly float defaultValue;
			public readonly float vanillaValue;
			
			public ConfigEntry(string d, bool flag) : this(d, typeof(bool), flag ? 1 : 0, 0, 1, 0) {
				
			}
			
			public ConfigEntry(string d, Type t, float def, float v) : this(d, t, def, float.MinValue, float.MaxValue, v) {
				
			}
			
			public ConfigEntry(string d, Type t, float def, float min, float max, float v) {
				desc = d;
				type = t;
				defaultValue = def;
				minValue = min;
				maxValue = max;
				vanillaValue = v;
			}
			
			public bool validate(ref float val) {
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
			
			public float parse(string text) {
				if (type == typeof(bool)) {
					return text.ToLowerInvariant() == "true" ? 1 : 0;
				}
				return float.Parse(text);
			}
			
			public string formatValue(float value) {
				if (type == typeof(bool)) {
					return (value > 0).ToString();
				}
				else if (type == typeof(int) || type == typeof(uint) || type == typeof(byte) || type == typeof(long) || type == typeof(ulong)) {
					return ((int)(value)).ToString();
				}
				return value.ToString("0.00");
			}
			
		}
	}
}
