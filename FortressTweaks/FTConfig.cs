using System;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml;
using ReikaKalseki.FortressCore;

namespace ReikaKalseki.FortressTweaks
{
	public class FTConfig
	{		
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
			[ConfigEntry("Make outer airlocks optional", false)]AIRLOCK,//*
			[ConfigEntry("Mattermitter range reduction factor by tier", typeof(float), 0.5F, 0, 1, 0.5F)]MATTERMITTER_RANGE_FACTOR,
			[ConfigEntry("Mattermitter range flat reduction by tier", typeof(int), 0, 0, 64, 0)]MATTERMITTER_RANGE_DROP,
			//*/
			//[ConfigEntry("Mattermitter ranges by tier", typeof(int), 0, 0, 64, 0)]MATTERMITTER_RANGES,
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
			[ConfigEntry("Keep pre-FF CC music", true)]FF_MUSIC,
			[ConfigEntry("Hive Brain Cost From Recombined Matter (Zero To Disable)", typeof(int), 60, 0, 32767, 0)]HIVE_BRAIN,
			[ConfigEntry("Enable Ampule Uncrafting To Seeds", true)]AMPULEUNDO,
			[ConfigEntry("Enable Heimal Geothermal Crafting", true)]GEOHEIM,
			[ConfigEntry("Make T2 cargo lift cheaper", true)]T2LIFT,
			[ConfigEntry("Allow uncrafting of research pods to basic material items", false)]PODUNCRAFT,
			[ConfigEntry("Disable slime ore drops", false)]SLIMEORE,
			[ConfigEntry("Item Magnet Power Cost", typeof(float), 0.1F, 0, 10, 0)]MAGNET_COST,
			[ConfigEntry("Night Vision Power Cost", typeof(float), 0.4F, 0, 10, 0)]NV_COST,
			[ConfigEntry("Night Vision Lighting Strength", typeof(float), 0.3F, 0, 1, 0)]NV_STRENGTH,
			[ConfigEntry("Spring Boots Damage Reduction Power Cost (Minimum)", typeof(float), 16, 0, 512, 0)]FALL_BOOT_COST_MIN,
			[ConfigEntry("Spring Boots Damage Reduction Power Cost (Maximum)", typeof(float), 64, 0, 512, 0)]FALL_BOOT_COST_MAX,
			[ConfigEntry("Hopper Gear Cost", typeof(int), 8, 1, 100, 10)]HOPPER_COST,
		}
	}
}
