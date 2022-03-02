using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using System.Threading;
using Harmony;
using ReikaKalseki.FortressTweaks;

namespace ReikaKalseki.FortressTweaks
{
  public class FortressTweaksMod : FortressCraftMod
  {
    public const string MOD_KEY = "ReikaKalseki.FortressTweaks";
    public const string CUBE_KEY = "ReikaKalseki.FortressTweaks_Key";
    
    public static readonly Config config = new Config();
    
    public static bool useMagmaMusicInFF = false;//CCCCC.ActiveAndWorking;

    public override ModRegistrationData Register()
    {
        ModRegistrationData registrationData = new ModRegistrationData();
        //registrationData.RegisterEntityHandler(MOD_KEY);
        /*
        TerrainDataEntry entry;
        TerrainDataValueEntry valueEntry;
        TerrainData.GetCubeByKey(CUBE_KEY, out entry, out valueEntry);
        if (entry != null)
          ModCubeType = entry.CubeType;
         */        
        
        config.load();
        
        var harmony = HarmonyInstance.Create("ReikaKalseki.FortressTweaks");
        HarmonyInstance.DEBUG = true;
        FileLog.Log("Ran mod register, started harmony (harmony log)");
        Util.log("Ran mod register, started harmony");
        
        try {
			harmony.PatchAll();
        }
        catch (Exception e) {
			FileLog.Log("Caught exception when running patcher!");
			FileLog.Log(e.Message);
			FileLog.Log(e.StackTrace);
			FileLog.Log(e.ToString());
        }        
        
        CubeHelper.mabIsCubeTypeGlass[eCubeTypes.EnergyGrommet] = true;
        CubeHelper.mabIsCubeTypeGlass[eCubeTypes.LogisticsGrommet] = true;
        //CubeHelper.mabIsCubeTypeGlass[eCubeTypes.HeatConductingPipe] = true;
        //CubeHelper.mabIsCubeTypeGlass[eCubeTypes.CastingPipe] = true;
        //CubeHelper.mabIsCubeTypeGlass[eCubeTypes.T4_GenericPipe] = true;
        
        FreightCartMob.OreFreighterWithdrawalPerTick = 40; //base is 5, barely better than minecarts; was 25 here until 2022
        
        T3_FuelCompressor.MAX_HIGHOCTANE = 10;
        
        foreach (CraftData rec in Util.getRecipesFor("ForcedInductionMK5")) {
        	Util.modifyIngredientCount(rec, "ForcedInductionMK4", 4); //was 8
        }
        
        foreach (CraftData rec in Util.getRecipesFor("ForcedInductionMK6")) { //ARC upgrade
        	Util.modifyIngredientCount(rec, "AlloyedMachineBlock", 16); //was 512
        	Util.modifyIngredientCount(rec, "PowerBoosterMK5", 5); //was 2
        }
        
        foreach (CraftData rec in Util.getRecipesFor("BuildGunV3")) {
        	Util.modifyIngredientCount(rec, "MolybdenumBar", 256); //was 1024
        	Util.modifyIngredientCount(rec, "ChromiumBar", 256); //was 1024
        	Util.removeResearch(rec, "T4_MagmaBore");
        	Util.addResearch(rec, "T4_drills_2");
        }
        
        return registrationData;
    }
    
    public static float getMagmaborePowerCost(float orig, T4_MagmaBore bore) {
    	float ret = orig;
    	if (DifficultySettings.mbEasyPower)
    		ret *= 0.8F;
    	if (DifficultySettings.mbCasualResource)
    		ret *= 0.5F;
    	else if (DifficultySettings.mbEasyResources)
    		ret *= 0.75F;
    	if (DifficultySettings.mbRushMode)
    		ret *= 0.5F;
    	return ret;
    }
    
    public static float getCompressorSpeed(float orig, T4_ParticleCompressor filter) {
    	if (!DifficultySettings.mbCasualResource)
    		return orig;
    	float f = filter.mrCurrentPower/filter.mrMaxPower;
    	if (f < 0.5) {
    		return orig;
    	}
    	float ratio = Math.Min(4F, (f-0.5F)*2*4.1F);
    	return ratio*orig;
    }
    
    public static int getProducedGas(int orig, T4_ParticleFilter filter) {
    	if (!DifficultySettings.mbCasualResource)
    		return orig;
    	float f = filter.mrCurrentPower/filter.mrMaxPower;
    	if (f < 0.5) {
    		return orig;
    	}
    	float ratio = Math.Min(4, (f-0.5F)*2*4.1F);
    	float power = Math.Min(filter.mrCurrentPower, Math.Max(0, (ratio-1)*10)); //extra 10PPS for each 1x increase
    	filter.mrCurrentPower -= power; 
    	return (int)(orig*ratio);
    }
    
    public static bool isCubeGeoPassable(ushort ID, GeothermalGenerator gen) {
    	return ID == eCubeTypes.Magma || ID == eCubeTypes.MagmaFluid || ID == eCubeTypes.Magmacite || (CubeHelper.IsOre(ID) && gen.mShaftEndY < -1050);
    }
    
    public static float getGrappleCooldown(float orig) {
    	return PlayerInventory.mbPlayerHasMK3BuildGun ? Math.Min(orig, 0.2F) : orig;
    }
    
    public static bool isCubeGlassForRoom(ushort blockID, RoomController rc) {
    	if (blockID == eCubeTypes.EnergyGrommet || blockID == eCubeTypes.LogisticsGrommet)
    		return false;
    	return CubeHelper.IsCubeGlass((int)blockID);
    }
    
    public static float getSharedPSBPower(float orig, PowerStorageBlock from, PowerStorageBlock to) {
    	float max = Math.Min(from.mrCurrentPower, to.mrPowerSpareCapacity);
    	float buff = from.mrMaxPower/to.mrMaxPower; //eg 1500/200 = 7.5x for blue/green, 5000/1500 = 3.33x purple/blue, 5000/200 = 25x purple/green
    	return Math.Min(max*0.8F, (float)Math.Round(orig*Math.Max(1, buff/2.5F))); //max flow rate: blue/green from 40 to 120, purple/blue from 300 to 400, purple/green from 40 to 160
    }
    
    public static StorageMachineInterface getStorageHandlerForEntityForBelt(Segment s, long x, long y, long z, ConveyorEntity belt) {
    	SegmentEntity ret = s.SearchEntity(x, y, z);
    	if (belt.mValue == 15) {
    		//Debug.Log("Motor belt at "+new Coordinate(belt)+" is pulling from a "+ret+" at "+new Coordinate(ret));
    	}
    	if (ret is ContinuousCastingBasin && belt.mValue == 15) { //motor belt
    		ContinuousCastingBasin ccb = ret as ContinuousCastingBasin;
    		ccb = ccb.GetCenter();
    		return ccb != null ? new BasinInterfaceWrapper(ccb) : null;
    	}
    	else {
    		return ret as StorageMachineInterface;
    	}
    }
    
    public static bool canCubeBeMouseClicked(ushort ID, ref CubeData data) { //hydroponics bays are the same ID as canopies
    	if (ID == eCubeTypes.TNTButNowGrass) {
    		return false;
    	}
    	if (ID == eCubeTypes.Hydroponics && data.mValue == 1) {
    		return false;
    	}
    	if (CubeHelper.IsCustom(ID) || CubeHelper.HasObject(ID) || CubeHelper.HasEmbeddedObject(ID))
     		return true;
		return CubeHelper.IsCubeSolid(ID);
    }
    
    public static void updateOETRequiredCharge() {
    	if (!MobSpawnManager.mbSurfaceAttacksActive) {
    		OrbitalEnergyTransmitter.mrMaxPower = Math.Min(OrbitalEnergyTransmitter.mrMaxPower, 2000000);
    	}
    }
    
    public static bool deleteOET(WorldScript world, Segment segment, long x, long y, long z, ushort leType)
    {
    	if (!MobSpawnManager.mbSurfaceAttacksActive) {
    		return true;
    	}
        return world.BuildFromEntity(segment,x,y,z,leType);
    }
    
    public static void guaranteeAirlock(Room_Airlock air) {
    	if (air.LinkedAirLock == null) {
    		air.LinkedAirLock = air;
    		Debug.Log("Airlock not found @ "+air.mnX+" / "+air.mnY+" / "+air.mnZ+", linking to self");
    	}
    }
    
    public static Room_Airlock guaranteeAirlockDuringCheck(Room_Airlock air) {
    	guaranteeAirlock(air);
    	return air.LinkedAirLock;
    }
    
    public static void setMMRange(MatterMover mm) {
    	int tier = mm.mValue;
    	int drop = 16;
    	float scale = 1;
    	double fac = scale == 1 ? 1 : Math.Pow(scale, tier);
    	int range = (int)(64*fac-tier*drop);
    	mm.mnMaxTransmitDistance = range;
    }
    
    public static float progressGACTimer(float prevTimerValue, float step, GenericAutoCrafterNew gac) {
    	float pf = gac.mrCurrentPower/gac.mrMaxPower;
    	float speed = gac.mrMaxPower <= 0 || float.IsInfinity(pf) || float.IsNaN(pf) ? 1 : Math.Max(1, (pf-0.5F)*4);
    	return Math.Max(0, prevTimerValue-step*speed);
    }
    
    public static bool doOETBlockEffects(WorldScript world, long x0, long y0, long z0, int size, int hardness) {
    	if (MobSpawnManager.mbSurfaceAttacksActive) {
    	//	WorldScript.instance.Explode(x0, y0, z0, size, hardness);
    		return WorldScript.instance.SafeExplode(x0, y0, z0, size);
    	}
    	else {
    		clearSoftResin(x0, y0, z0, size);
    		killWorms(x0, y0, z0, size);
    		return true;
    	}
    }
    
    private static void killWorms(long x0, long y0, long z0, int size) {
		int count = MobManager.instance.mActiveMobs.Count;
		for (int index = 0; index < count; index++) {
			MobEntity e = MobManager.instance.mActiveMobs[index];
			if (e != null && e.mType == MobType.WormBoss && e.mnHealth > 0) {
				Vector3 vec = Vector3.zero;
				vec.x = (float) (e.mnX - x0);
				vec.y = (float) (e.mnY - y0);
				vec.z = (float) (e.mnZ - z0);
				if (vec.magnitude <= size*1.25) {
					e.TakeDamage(Int32.MaxValue); //DIE DIE DIE DIE DIE
					FloatingCombatTextManager.instance.QueueText(e.mnX, e.mnY + 4L, e.mnZ, 1.5f, "Worm Killed!", Color.magenta, 2F, 4096F);
				}
			}
		}
    }
    
    private static void clearSoftResin(long x0, long y0, long z0, int size) {
    	size = (int)(size*2.5); //up to 120
    	int sizey = size/3; //up to 40
		int maxrSq = size + 1;
		maxrSq *= maxrSq;
		HashSet<Segment> hashSet = new HashSet<Segment>();
		try {
			for (int i = -size; i <= size; i++) {
				for (int j = -size; j <= sizey; j++) {
					for (int k = -size; k <= size; k++) {
						Vector3 vector = new Vector3((float)j, (float)i, (float)k);
						int num4 = (int)vector.sqrMagnitude;
						if (num4 < maxrSq) {
							long x = x0 + (long)j;
							long y = y0 + (long)i;
							long z = z0 + (long)k;
							Segment segment = WorldScript.instance.GetSegment(x, y, z);
							if (segment != null && segment.mbInitialGenerationComplete && !segment.mbDestroyed) {
								if (!segment.mbIsEmpty) {
									if (!hashSet.Contains(segment)) {
										hashSet.Add(segment);
										segment.BeginProcessing();
									}
									ushort cube = segment.GetCube(x, y, z);
									if (cube == eCubeTypes.Giger) {
										if (WorldScript.instance.BuildFromEntity(segment, x, y, z, eCubeTypes.Air, global::TerrainData.DefaultAirValue)) {
											DroppedItemData stack = ItemManager.DropNewCubeStack(eCubeTypes.Giger, 0, 1, x, y, z, Vector3.zero);
										}
									}
								}
							}
						}
					}
				}
			}
		}
		finally {
			foreach (Segment current in hashSet) {
				if (current.mbHasFluid) {
					current.FluidSleepTicks = 1;
				}
				current.EndProcessing();
			}
			WorldScript.instance.mNodeWorkerThread.KickNodeWorkerThread();
		}
    }

  }
}
