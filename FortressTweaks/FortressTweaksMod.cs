using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using System.Reflection.Emit;
using Harmony;
using ReikaKalseki.FortressTweaks;
using ReikaKalseki.FortressCore;

namespace ReikaKalseki.FortressTweaks
{
  public class FortressTweaksMod : FCoreMod
  {
    public const string MOD_KEY = "ReikaKalseki.FortressTweaks";
    public const string CUBE_KEY = "ReikaKalseki.FortressTweaks_Key";
    
    private static Config<FTConfig.ConfigEntries> config;
    
    public FortressTweaksMod() : base("FortressTweaks") {
    	config = new Config<FTConfig.ConfigEntries>(this);
    }
    
    public static Config<FTConfig.ConfigEntries> getConfig() {
    	return config;
    }

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
        
        int all = 0;
        int failed = 0;
        List<string> success = new List<string>();
        foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
        	all++;
        	try {
        		FUtil.log("Testing assembly "+a.FullName);
        		FUtil.log("Location @ "+a.Location);
        		int amt = a.GetTypes().Length;
        		FUtil.log("Found "+amt+" classes.");
        		success.Add(a.Location);
        	}
        	catch (Exception e) {
        		FUtil.log("Threw exception on access: "+e);
        		failed++;
        	}
        }
        FUtil.log("Failed to parse "+failed+"/"+all+" assemblies");
        FUtil.log("Successes:\n"+string.Join("\n", success.ToArray()));
        
        config.load();
        
        runHarmony();
        
        Type t = InstructionHandlers.getTypeBySimpleName("DiNSCustomT4Turret");
		if (t != null) {
			InstructionHandlers.patchMethod(harmony, t, "TrackTarget", modDLL, codes => {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, 37800F);
				codes[idx].operand = config.getFloat(FTConfig.ConfigEntries.MK4_TURRET_PPS);
			});
        	FUtil.log("Patch to "+t.Name+" complete.");
		}
        
        CubeHelper.mabIsCubeTypeGlass[eCubeTypes.EnergyGrommet] = true;
        CubeHelper.mabIsCubeTypeGlass[eCubeTypes.LogisticsGrommet] = true;
        //CubeHelper.mabIsCubeTypeGlass[eCubeTypes.HeatConductingPipe] = true;
        //CubeHelper.mabIsCubeTypeGlass[eCubeTypes.CastingPipe] = true;
        //CubeHelper.mabIsCubeTypeGlass[eCubeTypes.T4_GenericPipe] = true;
        
        FreightCartMob.OreFreighterWithdrawalPerTick = config.getInt(FTConfig.ConfigEntries.FREIGHT_SPEED); //base is 5, barely better than minecarts; was 25 here until 2022
        
        T3_FuelCompressor.MAX_HIGHOCTANE = config.getInt(FTConfig.ConfigEntries.HOF_CACHE);
        
        applyRecipeChanges();
        
        return registrationData;
    }
    
    private void applyRecipeChanges() {
    	foreach (CraftData rec in RecipeUtil.getRecipesFor("ForcedInductionMK5")) {
        	RecipeUtil.modifyIngredientCount(rec, "ForcedInductionMK4", (uint)config.getInt(FTConfig.ConfigEntries.FI_5_COST4)); //was 8
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.CHEAP_ARC)) {
	        foreach (CraftData rec in RecipeUtil.getRecipesFor("ForcedInductionMK6")) { //ARC upgrade
	        	RecipeUtil.modifyIngredientCount(rec, "AlloyedMachineBlock", 16); //was 512
	        	RecipeUtil.modifyIngredientCount(rec, "PowerBoosterMK5", 5); //was 2
	        }
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.CHEAP_INDUCTION)) {
	        foreach (CraftData rec in RecipeUtil.getRecipesFor("InductionChargerPlacement")) {
	        	RecipeUtil.removeIngredient(rec, "PowerStorageMK2");
	        	//RecipeUtil.modifyIngredientCount(rec, "PowerBoosterMK2", 5); //was 5
    			RecipeUtil.addIngredient(rec, "PowerStorageMK1", 5);
	        }
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.CHEAP_TRICKY_OT)) {
    		CraftData rec = RecipeUtil.getRecipeByKey("Tricky.1000SlotHopperx");
    		if (rec != null) {
    			CraftCost ing = RecipeUtil.removeIngredient(rec, "ImbuedMachineBlock");
    			if (ing != null)
    				RecipeUtil.addIngredient(rec, "PlasticPellet", ing.Amount*5);
    		}
    		rec = RecipeUtil.getRecipeByKey("Tricky.2000SlotHopperx");
    		if (rec != null) {
    			CraftCost ing = RecipeUtil.removeIngredient(rec, "ImbuedMachineBlock");
    			if (ing != null)
    				RecipeUtil.addIngredient(rec, "PlasticPellet", ing.Amount*10);
    		}
        }
    	
    	foreach (CraftData rec in RecipeUtil.getRecipesFor("ThreatReducer")) {
    		RecipeUtil.removeIngredient(rec, "CopperBar");
    		RecipeUtil.removeIngredient(rec, "FortifiedPCB");
    		RecipeUtil.addIngredient(rec, "BasicPCB", 6);
    		RecipeUtil.addIngredient(rec, "IronGear", 4);
    		RecipeUtil.addIngredient(rec, "LightweightMachineHousing", 1);
	    }
        
    	int hopp = config.getInt(FTConfig.ConfigEntries.HOPPER_COST);
        if (hopp != 10) { //skip doing if vanilla
	        foreach (CraftData rec in RecipeUtil.getRecipesFor("StorageHopper")) {
    			RecipeUtil.modifyIngredientCount(rec, "IronGear", (uint)hopp);
	        }
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.EARLIER_V3_GUN)) {
	        foreach (CraftData rec in RecipeUtil.getRecipesFor("BuildGunV3")) {
	        	RecipeUtil.modifyIngredientCount(rec, "MolybdenumBar", 256); //was 1024
	        	RecipeUtil.modifyIngredientCount(rec, "ChromiumBar", 256); //was 1024
	        	RecipeUtil.removeResearch(rec, "T4_MagmaBore");
	        	RecipeUtil.addResearch(rec, "T4_drills_2");
	        }
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.CHEAPER_MK4_TURRET)) {
	        foreach (CraftData rec in RecipeUtil.getRecipesFor("T4EnergyTurretPlacement")) {
	        	RecipeUtil.modifyIngredientCount(rec, "ExceptionalOrganicLens", 1); //was 2
	        }
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.AMPULEUNDO)) {
        	for (int i = 1; i <= 5; i++) {
        		CraftData rec = RecipeUtil.addRecipe("AmpuleUncraft"+i, "Seed_UnderGrump", "consumables", 8*i);
        		rec.Tier = 1;
        		rec.CanCraftAnywhere = true;
        		rec.Description = "Uncrafting MK"+i+" ampules.";
        		RecipeUtil.addIngredient(rec, "AmpuleMK"+i, 1);
        		RecipeUtil.addResearch(rec, "Rooms_Herb");
        	}
        }
        
    	int braincost = config.getInt(FTConfig.ConfigEntries.HIVE_BRAIN);
        if (braincost > 0) {
        	CraftData rec = RecipeUtil.addRecipe("RecombinedBrain", "HiveBrainMatter", "CraftingIngredient", 1);
        	rec.CanCraftAnywhere = true;
        	RecipeUtil.addIngredient(rec, "RecombinedOrganicMatter", (uint)braincost);
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.T2LIFT)) {
        	CraftData rec = RecipeUtil.getRecipeByKey("CargoLiftImproved");
        	RecipeUtil.addIngredient(rec, "AlloyedPCB", 10);
        	RecipeUtil.removeIngredient(rec, "UltimatePCB");
        	rec.Hint = "Can be upgraded again.";
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.GEOHEIM)) {
        	CraftData template = RecipeUtil.getRecipeByKey("GeothermalGeneratorPlacementMan");
			CraftData rec = RecipeUtil.copyRecipe(template);
			rec.Key = "ReikaKalseki.GeothermalGeneratorPlacementMan";
			RecipeUtil.addRecipe(rec);
        	RecipeUtil.removeIngredient(rec, "ChromedMachineBlock");
        	CraftCost ing = RecipeUtil.removeIngredient(rec, "MagneticMachineBlock");
        	RecipeUtil.addIngredient(rec, "HiemalMachineBlock", ing.Amount*9/10);
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.PODUNCRAFT)) {
			string folder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string path = System.IO.Path.Combine(folder, "Xml/PodUncrafting.xml");
        	List<OverrideCraftData> li = (List<OverrideCraftData>)XMLParser.ReadXML(path, typeof(List<OverrideCraftData>));
        	foreach (OverrideCraftData data in li) {
        		CraftData rec = data.ToStandardFormat();
				RecipeUtil.addRecipe(rec);
        		FUtil.log("Adding pod uncrafting recipe "+RecipeUtil.recipeToString(rec));
        	}
        }
    }
    
    public static float getMagmaborePowerCost(float orig, T4_MagmaBore bore) {
    	if (!config.getBoolean(FTConfig.ConfigEntries.MAGMABORE))
    		return orig;
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
    	if (!config.getBoolean(FTConfig.ConfigEntries.GAS_SPEED))
    		return orig;
    	float f = filter.mrCurrentPower/filter.mrMaxPower;
    	if (f < 0.5) {
    		return orig;
    	}
    	float ratio = Math.Min(4F, (f-0.5F)*2*4.1F);
    	return ratio*orig;
    }
    
    public static int getProducedGas(int orig, T4_ParticleFilter filter) {
    	if (!config.getBoolean(FTConfig.ConfigEntries.GAS_SPEED))
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
    	return ID == eCubeTypes.Magma || ID == eCubeTypes.MagmaFluid || (config.getBoolean(FTConfig.ConfigEntries.GEO_PIPE_PASS) && (ID == eCubeTypes.Magmacite || (CubeHelper.IsOre(ID) && gen.mShaftEndY < -1000)));
    }
    
    public static float getGrappleCooldown(float orig) {
    	return PlayerInventory.mbPlayerHasMK3BuildGun && config.getBoolean(FTConfig.ConfigEntries.GRAPPLE_COOLDOWN) ? Math.Min(orig, 0.2F) : orig;
    }
    
    public static bool isCubeGlassForRoom(ushort blockID, RoomController rc) {
    	if (blockID == eCubeTypes.EnergyGrommet || blockID == eCubeTypes.LogisticsGrommet)
    		return false;
    	return CubeHelper.IsCubeGlass((int)blockID);
    }
    
    public static float getSharedPSBPower(float orig, PowerStorageBlock from, PowerStorageBlock to) {
    	float max = Math.Min(from.mrCurrentPower, to.mrPowerSpareCapacity);
    	float buff = from.mrMaxPower/to.mrMaxPower; //eg 1500/200 = 7.5x for blue/green, 5000/1500 = 3.33x purple/blue, 5000/200 = 25x purple/green
    	buff *= config.getFloat(FTConfig.ConfigEntries.PSB_SHARE);
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
    	if (!MobSpawnManager.mbSurfaceAttacksActive && config.getBoolean(FTConfig.ConfigEntries.OET)) {
    		OrbitalEnergyTransmitter.mrMaxPower = Math.Min(OrbitalEnergyTransmitter.mrMaxPower, config.getInt(FTConfig.ConfigEntries.OET_WEAK_COST));
    	}
    }
    
    public static bool deleteOET(WorldScript world, Segment segment, long x, long y, long z, ushort leType)
    {
    	if (!MobSpawnManager.mbSurfaceAttacksActive && config.getBoolean(FTConfig.ConfigEntries.OET)) {
    		return true;
    	}
        return world.BuildFromEntity(segment,x,y,z,leType);
    }
    
    public static void guaranteeAirlock(Room_Airlock air) {
    	if (air.LinkedAirLock == null && config.getBoolean(FTConfig.ConfigEntries.AIRLOCK)) {
    		air.LinkedAirLock = air;
    		//Debug.Log("Airlock not found @ "+air.mnX+" / "+air.mnY+" / "+air.mnZ+", linking to self");
    	}
    }
    
    public static Room_Airlock guaranteeAirlockDuringCheck(Room_Airlock air) {
    	if (config.getBoolean(FTConfig.ConfigEntries.AIRLOCK))
	    	guaranteeAirlock(air);
		return air.LinkedAirLock;
    }
    
    public static void setMMRange(MatterMover mm) {
    	int tier = mm.mValue;
    	int drop = config.getInt(FTConfig.ConfigEntries.MATTERMITTER_RANGE_DROP);
    	float scale = config.getFloat(FTConfig.ConfigEntries.MATTERMITTER_RANGE_FACTOR);
    	double fac = Math.Abs(scale-1) <= 0.01 ? 1 : Math.Pow(scale, tier);
    	int range = (int)(64*fac-tier*drop);
    	mm.mnMaxTransmitDistance = range;
    }
    
    public static float progressGACTimer(float prevTimerValue, float step, GenericAutoCrafterNew gac) {
    	float pf = gac.mrCurrentPower/gac.mrMaxPower;
    	float speed = gac.mrMaxPower <= 0 || float.IsInfinity(pf) || float.IsNaN(pf) ? 1 : Math.Max(1, (pf-0.5F)*4*config.getFloat(FTConfig.ConfigEntries.GAC_RAMP));
    	return Math.Max(0, prevTimerValue-step*speed);
    }
    
    public static bool doOETBlockEffects(WorldScript world, long x0, long y0, long z0, int size, int hardness) {
    	if (MobSpawnManager.mbSurfaceAttacksActive || !config.getBoolean(FTConfig.ConfigEntries.OET)) {
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
    
    public static float getHeadlightEffect() {
    	return config.getFloat(FTConfig.ConfigEntries.HEADLIGHT_MODULE_EFFECT);
    }

  }
}
