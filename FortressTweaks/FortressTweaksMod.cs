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
    
    private static readonly Dictionary<ushort, PasteCostCache> pasteCostCache = new Dictionary<ushort, PasteCostCache>();
    
    public FortressTweaksMod() : base("FortressTweaks") {
    	config = new Config<FTConfig.ConfigEntries>(this);
    }
    
    public static Config<FTConfig.ConfigEntries> getConfig() {
    	return config;
    }

	public override ModItemActionResults PerformItemAction(ModItemActionParameters parameters) {
    	if (parameters.ItemToUse.mnItemID == ItemEntry.mEntriesByKey["ReikaKalseki.BlockCopier"].ItemID) {
    		parameters.Consume = false;
    		LocalPlayerScript ep = parameters.Consumer.mLocalPlayer;
    		PlayerBlockPicker pbp = ep.mPlayerBlockPicker;
    		ushort id = pbp.selectBlockType;
    		ushort meta = pbp.selectBlockValue;
    		WorldUtil.buildBlockAtLook(parameters.Consumer.mLocalPlayer, parameters.ItemToUse, id, meta);
    		ARTHERPetSurvival.instance.SetARTHERReadoutText("Replicated ["+id+"/"+meta+"] ("+FUtil.getBlockName(id, meta)+") into "+Coordinate.fromLook(ep), 10, false, true);
    		ModItemActionResults res = new ModItemActionResults();
    		AudioHUDManager.instance.Build(WorldScript.instance.mPlayerFrustrum.GetCoordsToUnity(pbp.selectFaceX, pbp.selectFaceY, pbp.selectFaceZ) + WorldHelper.DefaultBlockOffset);
    		res.Consume = false;
    		return res;
    	}
		return null;
	}

    protected override void loadMod(ModRegistrationData registrationData) {
    	validateDLLs();
        
        config.load();
        
        PersistentData.load(Path.Combine(Path.GetDirectoryName(modDLL.Location), "persistent.dat"));
        
        runHarmony();
        
        Type t = InstructionHandlers.getTypeBySimpleName("DiNSCustomT4Turret");
		if (t != null) {
			InstructionHandlers.patchMethod(harmony, t, "TrackTarget", modDLL, codes => {
				int idx = InstructionHandlers.getInstruction(codes, 0, 0, OpCodes.Ldc_R4, 37800F);
				codes[idx].operand = config.getFloat(FTConfig.ConfigEntries.MK4_TURRET_PPS);
			});
        	FUtil.log("Patch to "+t.Name+" complete.");
		}
        
        //makes them drop as paste
        for (int i = eCubeTypes.CanvasYellow; i < eCubeTypes.CanvasBlack; i++) {
	        CubeHelper.maCategory[i] = MaterialCategories.Decoration;
	        TerrainData.mEntries[i].Category = MaterialCategories.Decoration;
        }
        for (int i = eCubeTypes.CanvasPlum; i < eCubeTypes.CanvasMaroon; i++) {
	        CubeHelper.maCategory[i] = MaterialCategories.Decoration;
	        TerrainData.mEntries[i].Category = MaterialCategories.Decoration;
        }
        
        //fix snow invisible bottom
        TerrainData.mEntries[21].BottomTexture = 12; //was 2, typo?
        
        foreach (List<CraftData> li in CraftData.mRecipesForSet.Values) {
        	foreach (CraftData rec in li) {
        		if (rec.Costs.Count == 1 && rec.Costs[0].Key == "ConstructionPaste" && rec.CraftableCubeType > 1) {
        			PasteCostCache cc;
        			int amt = (int)rec.Costs[0].Amount;
        			if (!pasteCostCache.TryGetValue(rec.CraftableCubeType, out cc)) {
        				cc = new PasteCostCache(rec.CraftableCubeType);
        				pasteCostCache[rec.CraftableCubeType] = cc;
        				cc.anyCost = amt;
        			}
        			cc.costs[rec.CraftableCubeValue] = amt;
        			FUtil.log("Caching paste cost of '"+rec.Key+"' (makes "+TerrainData.mEntries[rec.CraftableCubeType].Name+") ID["+rec.CraftableCubeType+"/"+rec.CraftableCubeValue+"] - x"+rec.Costs[0].Amount);
        		}
        	}
        }
        
        Dictionary<string, TerrainDataValueEntry> placers = TerrainData.mEntries[eCubeTypes.MachinePlacementBlock].ValuesByKey;
        try {
	        TerrainDataValueEntry[] trencherDrills = new TerrainDataValueEntry[]{
		        placers["TrencherDrillPlacement"],
		        placers["TrencherDrillPlacementMk2"],
		        placers["TrencherDrillPlacementMk3"]
	        };
	        string[] trencherDrillIcons = trencherDrills.Select(e => e.IconName).ToArray();
	        for (int i = 0; i < 3; i++) {
	        	int idx = getTrencherVisual(i);
	        	trencherDrills[i].IconName = trencherDrillIcons[idx];
	        	FUtil.log("Trencher tier "+(i+1)+" set to use icon "+(idx+1)+" ("+trencherDrills[i].IconName+")");
	        }
        }
        catch (Exception e) {
        	FUtil.log("Could not set trencher drill icons: "+e.ToString());
        	FUtil.log("MB Placer values:\n"+string.Join("\n", placers.Select(kvp => kvp.Key+"=="+kvp.Value.terrainDataValueToString()).ToArray()));
        }
        
        CubeHelper.mabIsCubeTypeGlass[eCubeTypes.EnergyGrommet] = true;
        CubeHelper.mabIsCubeTypeGlass[eCubeTypes.LogisticsGrommet] = true;
        //CubeHelper.mabIsCubeTypeGlass[eCubeTypes.HeatConductingPipe] = true;
        //CubeHelper.mabIsCubeTypeGlass[eCubeTypes.CastingPipe] = true;
        //CubeHelper.mabIsCubeTypeGlass[eCubeTypes.T4_GenericPipe] = true;
        
        FreightCartMob.OreFreighterWithdrawalPerTick = config.getInt(FTConfig.ConfigEntries.FREIGHT_SPEED); //base is 5, barely better than minecarts; was 25 here until 2022
        
        T3_FuelCompressor.MAX_HIGHOCTANE = config.getInt(FTConfig.ConfigEntries.HOF_CACHE);
        T3_FuelCompressor.COAL_NEEDED = config.getInt(FTConfig.ConfigEntries.HOF_COAL);
        
        applyRecipeChanges();
    }
    
    private void validateDLLs() {
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
    }
    
    private void applyRecipeChanges() {
    	foreach (CraftData rec in RecipeUtil.getRecipesFor("ForcedInductionMK5")) {
        	rec.modifyIngredientCount("ForcedInductionMK4", (uint)config.getInt(FTConfig.ConfigEntries.FI_5_COST4)); //was 8
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.CHEAP_ARC)) {
    		uint n = 2;
	        foreach (CraftData rec in RecipeUtil.getRecipesFor("ForcedInductionMK6")) { //ARC upgrade
	        	rec.modifyIngredientCount("AlloyedMachineBlock", 16*n); //was 512
	        	rec.modifyIngredientCount("PowerBoosterMK5", 5*n); //was 2
	        	rec.CraftedAmount *= 2;
	        }
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.CHEAP_INDUCTION)) {
	        foreach (CraftData rec in RecipeUtil.getRecipesFor("InductionChargerPlacement")) {
	        	rec.removeIngredient("PowerStorageMK2");
	        	//rec.modifyIngredientCount("PowerBoosterMK2", 5); //was 5
    			rec.addIngredient("PowerStorageMK1", 5);
	        }
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.EARLIER_TRICKY_OT)) {
    		CraftData rec = RecipeUtil.getRecipeByKey("Tricky.1000SlotHopperx");
    		if (rec != null)
    			rec.replaceIngredient("ImbuedMachineBlock", "PlasticPellet", 5);
    		rec = RecipeUtil.getRecipeByKey("Tricky.2000SlotHopperx");
    		if (rec != null)
    			rec.replaceIngredient("ImbuedMachineBlock", "PlasticPellet", 10);
    		rec = RecipeUtil.getRecipeByKey("Tricky.2000SlotHopperx");
    		if (rec != null)
    			rec.replaceIngredient("ImbuedMachineBlock", "PlasticPellet", 10);
    		rec = RecipeUtil.getRecipeByKey("Tricky.3000SlotHopperx");
    		if (rec != null) {
    			bool integrated = ItemEntry.mEntriesByKey["ReikaKalseki.ChromiumPCB"] != null;
    			rec.replaceIngredient("ChromedMachineBlock", integrated ? "ReikaKalseki.ChromiumPCB" : "ChromiumBar", 2);
    			rec.replaceIngredient("MagneticMachineBlock", integrated ? "ReikaKalseki.MolybdenumPCB" : "MolybdenumBar", 2);
    			rec.removeIngredient("CompressedSulphur");
    			if (rec.removeIngredient("UltimatePCB") != null)
    				rec.addItemPerN("UltimatePCB", 2);
    		}
    		rec = RecipeUtil.getRecipeByKey("Tricky.5000SlotHopperx");
    		if (rec != null) {
    			rec.replaceIngredient("CompressedSulphur", "CompressedFreon", 5/3F); //from 30 to 50
    		}
        }
    	
    	foreach (CraftData rec in RecipeUtil.getRecipesFor("ThreatReducer")) {
    		rec.removeIngredient("CopperBar");
    		rec.removeIngredient("FortifiedPCB");
    		rec.addIngredient("BasicPCB", 6);
    		rec.addIngredient("IronGear", 4);
    		rec.addIngredient("LightweightMachineHousing", 1);
	    }
    	
    	RecipeUtil.addUncrafting("laser_mk3_new", "Uncrafting LPT3");
    	RecipeUtil.addUncrafting("PSB_mk3_Alt", "Uncrafting PSB3", "AlloyedMachineBlock");
    	RecipeUtil.addUncrafting("PSB mk4", "Uncrafting PSB4");
    	RecipeUtil.addUncrafting("PSB mk5", "Uncrafting PSB5");
    	
    	RecipeUtil.addUncrafting("forced induction6", "Uncrafting Arc Smelter", "ForcedInductionMK5");
    	
    	foreach (CraftData rec in RecipeUtil.getRecipesFor("ThreatReducer")) {
    		rec.Costs.Clear();
    		rec.addIngredient("BasicPCB", 6);
    		rec.addIngredient("IronGear", 4);
    		rec.addIngredient("LightweightMachineHousing", 1);
	    }
        
    	int hopp = config.getInt(FTConfig.ConfigEntries.HOPPER_COST);
        if (hopp != 10) { //skip doing if vanilla
	        foreach (CraftData rec in RecipeUtil.getRecipesFor("StorageHopper")) {
    			rec.modifyIngredientCount("IronGear", (uint)hopp);
	        }
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.EARLIER_V3_GUN) || config.getBoolean(FTConfig.ConfigEntries.BEGIN_FF_V3_GUN)) {
	        foreach (CraftData rec in RecipeUtil.getRecipesFor("BuildGunV3")) {
	        	rec.modifyIngredientCount("MolybdenumBar", 256); //was 1024
	        	rec.modifyIngredientCount("ChromiumBar", 256); //was 1024
	        	rec.removeResearch("T4_MagmaBore");
	        	rec.addResearch(config.getBoolean(FTConfig.ConfigEntries.BEGIN_FF_V3_GUN) ? "T4 ore extracting" : "T4_drills_2");
	        }
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.EARLIER_CONDUIT)) {
	        foreach (CraftData rec in RecipeUtil.getRecipesFor("ConduitPlacement")) { //includes the T4 turret to conduit one added by this mod
	        	rec.removeResearch("T4_GTPower");
	        	rec.addResearch("T4 ore extracting");
	        }
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.CHEAPER_MK4_TURRET)) {
	        foreach (CraftData rec in RecipeUtil.getRecipesFor("T4EnergyTurretPlacement")) {
	        	rec.modifyIngredientCount("ExceptionalOrganicLens", 1); //was 2
	        }
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.AMPULEUNDO)) {
        	for (int i = 1; i <= 5; i++) {
    			RecipeUtil.addRecipe("AmpuleUncraft"+i, "Seed_UnderGrump", "consumables", 8*i, init: rec => {
	        		rec.Tier = 1;
	        		rec.CanCraftAnywhere = true;
	        		rec.Description = "Uncrafting MK"+i+" ampules.";
	        		rec.addIngredient("AmpuleMK"+i, 1);
	        		rec.addResearch("Rooms_Herb");
				});
        	}
        }
        
    	int braincost = config.getInt(FTConfig.ConfigEntries.HIVE_BRAIN);
        if (braincost > 0) {
    		RecipeUtil.addRecipe("RecombinedBrain", "HiveBrainMatter", "craftingingredient", 1, init: rec => {
        		rec.addIngredient("RecombinedOrganicMatter", (uint)braincost);
        		rec.ResearchRequirements.Add("Cyrogenics Research"); //yes it is mispelled
    			rec.CanCraftAnywhere = true;
    			rec.ResearchCost = config.getInt(FTConfig.ConfigEntries.HIVE_BRAIN_CRAFT_RPOINTS);
    		});
        }
    	ItemEntry.mEntriesByKey["HiveBrainMatter"].DecomposeValue = config.getInt(FTConfig.ConfigEntries.HIVE_BRAIN_RPOINTS);
        
        if (config.getBoolean(FTConfig.ConfigEntries.T2LIFT)) {
        	CraftData rec = RecipeUtil.getRecipeByKey("CargoLiftImproved");
        	rec.addIngredient("AlloyedPCB", 10);
        	rec.removeIngredient("UltimatePCB");
        	rec.Hint = "Can be upgraded again.";
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.GEOHEIM)) {
        	CraftData template = RecipeUtil.getRecipeByKey("GeothermalGeneratorPlacementMan");
			CraftData rec = RecipeUtil.copyRecipe(template);
			rec.Key = "ReikaKalseki.GeothermalGeneratorPlacementMan";
			RecipeUtil.addRecipe(rec);
        	rec.removeIngredient("ChromedMachineBlock");
        	rec.replaceIngredient("MagneticMachineBlock", "HiemalMachineBlock", 0.9F);
        }
        
        if (config.getBoolean(FTConfig.ConfigEntries.PODUNCRAFT)) {
			string folder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string path = Path.Combine(folder, "Xml/PodUncrafting.xml");
        	List<OverrideCraftData> li = (List<OverrideCraftData>)XMLParser.ReadXML(path, typeof(List<OverrideCraftData>));
        	foreach (OverrideCraftData data in li) {
        		CraftData rec = data.ToStandardFormat();
				RecipeUtil.addRecipe(rec);
        		FUtil.log("Adding pod uncrafting recipe "+RecipeUtil.recipeToString(rec));
        	}
        }
    	
    	if (TerrainData.mEntriesByKey.ContainsKey("Innominate.BoringCompany")) {
    		Action<CraftData> setup = (rec) => {
    			rec.CanCraftAnywhere = true;
    		};
    		CraftData r1 = RecipeUtil.addRecipe("BorerStairToSquare", "Innominate.TunnelBoreSquare", "mining", init: setup);
    		CraftData r2 = RecipeUtil.addRecipe("BorerSquareToStair", "Innominate.TunnelBore", "mining", init: setup);
    		RecipeUtil.addIngredient(r1, "Innominate.TunnelBore", 1);
    		RecipeUtil.addIngredient(r2, "Innominate.TunnelBoreSquare", 1);
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
    
    public static float getHRGSpeed(float incr, T4_Grinder grinder) {
    	if (!config.getBoolean(FTConfig.ConfigEntries.HRG_SPEED))
    		return incr;
    	float f = grinder.mrCurrentPower/grinder.mrMaxPower;
    	float boost = 0;
    	const float maxBoost = 4F; //goes from 10 min to 2.5
    	if (f >= 0.5F) {
    		boost = (f-0.5F)*2F*maxBoost;
    		incr *= 1+boost;
    	}
    	float pps = config.getFloat(FTConfig.ConfigEntries.HRG_PPS);
    	if (pps > 512) {
	    	grinder.mrMaxPower = Mathf.Max(grinder.mrMaxPower, pps*8);
	    	grinder.mrCurrentPower -= Mathf.Lerp(boost/maxBoost, 0, pps-512); //subtract extra power, linearly
    	}
    	return incr;
    }
    
    public static float getFuelCompressorCycleTime(T3_FuelCompressor com, int coal, int hecf) {
    	if (!config.getBoolean(FTConfig.ConfigEntries.FUELCOM_SPEED))
    		return T3_FuelCompressor.WORK_TIME;
    	float fPwr = com.mrCurrentPower/com.mrMaxPower;
    	float fCoal = coal/(float)T3_FuelCompressor.MAX_COAL;
    	float fFuel = hecf/(float)T3_FuelCompressor.MAX_HEFC;
    	float bonus = Mathf.Clamp01(fPwr*0.6F+fCoal*0.3F+fFuel*0.1F)*0.8F;
    	return T3_FuelCompressor.WORK_TIME*(1-bonus);
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
    	//FUtil.log("Setting MM "+tier+" range to "+range+", 64*"+fac.ToString("0.00")+"-"+(tier*drop));
    	mm.mnMaxTransmitDistance = range;
    }
    
    public static float getMMTransferCost(float val, MatterMover mov) {
    	if (!config.getBoolean(FTConfig.ConfigEntries.MATTERMITTER_COST_SCALE))
    		return val;
    	float f = mov.mnBeamStrokeLength/(float)mov.mnMaxTransmitDistance;
    	return (int)Mathf.Max(1, val*f);
    }
    
    public static float progressGACTimer(float prevTimerValue, float step, GenericAutoCrafterNew gac) {
    	float pf = gac.mrCurrentPower/gac.mrMaxPower;
    	float speed = gac.mrMaxPower <= 0 || float.IsInfinity(pf) || float.IsNaN(pf) ? 1 : Math.Max(1, (pf-0.5F)*4*config.getFloat(FTConfig.ConfigEntries.GAC_RAMP));
    	return Math.Max(0, prevTimerValue-step*speed);
    }
    
    public static float getHeadlightEffect() {
    	return config.getFloat(FTConfig.ConfigEntries.HEADLIGHT_MODULE_EFFECT);
    }
    
    public static float loadHeadlightSettings(SurvivalPowerPanel panel) {
    	float ret = PersistentData.getValue<float>(PersistentData.Values.HEADLIGHT);
    	FUtil.log("Fetching persistent headlight value "+ret);
    	
    	//update UI
		panel.Headlight_Low.SetActive(false);
		panel.Headlight_Med.SetActive(false);
		panel.Headlight_Hi.SetActive(false);
		panel.HeadlightOn.SetActive(true);
		if (ret < 15f)
			panel.HeadlightOn.SetActive(false);
		if (ret >= 15f)
			panel.Headlight_Low.SetActive(true);
		if (ret >= 50f)
			panel.Headlight_Med.SetActive(true);
		if (ret >= 150f)
			panel.Headlight_Hi.SetActive(true);
    	return ret;
    }
    
    public static void cycleHeadlightSettings(SurvivalPowerPanel panel, float setting) {
    	//FUtil.log("Updating headlight to "+setting);
    	PersistentData.setValue(PersistentData.Values.HEADLIGHT, setting);
    }
    
    public static void onVatLinked(RefineryController c, RefineryReactorVat vat) {
    	FUtil.log("Linked refinery controller "+new Coordinate(c)+" to vat "+new Coordinate(vat)+", list = ["+string.Join(", ", c.mConnectedVats.Select(v => new Coordinate(v).ToString()).ToArray())+"]");
    }
    
    public static int getPasteDropCount(ushort id, ushort value) {
    	int ret = 1;
    	PasteCostCache cache;
    	if (pasteCostCache.TryGetValue(id, out cache)) {
    		cache.costs.TryGetValue(value, out ret);
    	}
    	if (ret <= 0) {
    		FUtil.log("Fetched invalid paste cost for block ID "+id+"/"+value+" ("+FUtil.getBlockName(id, value)+"): "+ret+"; attempting fallback value "+cache.anyCost);
    		ret = cache.anyCost > 0 ? cache.anyCost : 1;
    	}
    	return ret;
    }
    
    class PasteCostCache {
    	
    	public readonly ushort blockID;
    	
    	internal int anyCost;
    	
    	internal readonly Dictionary<ushort, int> costs = new Dictionary<ushort, int>();
    	
    	internal PasteCostCache(ushort id) {
    		blockID = id;
    	}
    	
    }
    
    public static void debugBlastFurnace(BlastFurnace bf, int[][] recipeCounters) {
    	FUtil.log("Blast furnace recipe search @ RF="+DifficultySettings.mrResourcesFactor);
    	List<CraftData> recipesForSet = CraftData.GetRecipesForSet("BlastFurnace");
    	for(int i = 0; i < recipeCounters.Length; i++) {
    		FUtil.log("Recipe "+i+": "+recipesForSet[i].recipeToString()+" with cost "+recipesForSet[i].Costs[0].ingredientToString());
    		FUtil.log("Recipe counter "+i+": ["+string.Join(", ", recipeCounters[i].Select(amt => amt.ToString()).ToArray())+"]");
    	}
    	FUtil.log("Current recipe: "+(bf.mCurrentRecipe == null ? "null" : bf.mCurrentRecipe.Key));
    	FUtil.log("Hoppers:\n"+string.Join("\n", bf.mAttachedHoppers.Select(hop => hop.GetType().Name+" @ "+(new Coordinate((SegmentEntity)hop).ToString())).ToArray()));
    }
    
    public static void spawnTrencherGO(MBOreExtractorDrill drill) {
		if (drill.mbIsCenter && !drill.DoNotSpawn)
    		FUtil.setMachineModel(drill, getTrencherModel(drill));
    }
    
    public static SpawnableObjectEnum getTrencherModel(MBOreExtractorDrill drill) {
    	int mdl = getTrencherVisual(drill.mnMark);
    	FUtil.log("Trencher @ "+FUtil.machineToString(drill)+" tier "+(drill.mnMark+1)+" is using model #"+mdl+" from config");
    	switch(mdl+1) {
    		case 1:
    		default:
    			return SpawnableObjectEnum.MB_Ore_Extractor_Drill;
    		case 2:
    			return SpawnableObjectEnum.MB_Ore_Extractor_DrillMk2;
    		case 3:
    			return SpawnableObjectEnum.MB_Ore_Extractor_DrillMk3;
    	}
    }
     //tier is 0-2, returns 0-2
    public static int getTrencherVisual(int tier) {
    	FTConfig.ConfigEntries option;
    	tier += 1;
    	switch(tier) {
    		case 1:
    		default:
    			option = FTConfig.ConfigEntries.TRENCHER_1_MODEL;
    			break;
    		case 2:
    			option = FTConfig.ConfigEntries.TRENCHER_2_MODEL;
    			break;
    		case 3:
    			option = FTConfig.ConfigEntries.TRENCHER_3_MODEL;
    			break;
    	}
    	return config.getInt(option)-1;
    }
    /*
    public static float getLiftCheckPPS(float orig) {
    	FUtil.log("tied to consume lift check pps: "+orig);
    	return 100;//config.getFloat(FTConfig.ConfigEntries.LIFT_CHECK_PPS);
    }*/

  }
}
