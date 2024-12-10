using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System;
using System.Reflection;
using System.Collections.Generic;

namespace ReikaKalseki.FortressTweaks {

	public class BottlerInterfaceWrapper : StorageMachineInterface {
		
		private static readonly FieldInfo gas = typeof(T4_GasBottler).GetField("StoredGas", BindingFlags.Instance | BindingFlags.NonPublic);
		
		private readonly T4_GasBottler bottler;
		
		public int TotalCapacity {set;get;}
		public int UsedCapacity {set;get;}
		public int RemainingCapacity {set;get;}
		public bool InventoryExtractionPermitted {set;get;}
		
		internal BottlerInterfaceWrapper(T4_GasBottler te) {
			bottler = te;
			
			TotalCapacity = 10000;
			InventoryExtractionPermitted = true;
			UsedCapacity = 0;
			ItemBase ib = getGas();
			if (ib != null)
				UsedCapacity += ib.GetAmount();
			
			RemainingCapacity = TotalCapacity-UsedCapacity;
		}
		
		public eHopperPermissions GetPermissions() {
			return eHopperPermissions.RemoveOnly;
		}
		
		private ItemBase getGas() {
			return (ItemBase)gas.GetValue(bottler);
		}
	
		public bool IsEmpty() {
			return getGas() == null;
		}
	
		public bool IsFull() {
			return false;
		}
	
		public bool IsNotEmpty() {
			return !IsEmpty();
		}
	
		public bool IsNotFull() {
			return !IsFull();
		}
	
		public bool TryInsert(InventoryInsertionOptions options, ref InventoryInsertionResults results) {return false;}
	
		public bool TryInsert(StorageUserInterface sourceEntity, ItemBase item) {return false;}
	
		public bool TryInsert(StorageUserInterface sourceEntity, ushort cube, ushort value, int amount) {return false;}
	
		public int TryPartialInsert(StorageUserInterface sourceEntity, ref ItemBase item, bool alwaysCloneItem, bool updateSourceItem) {return 0;}
	
		public int TryPartialInsert(StorageUserInterface sourceEntity, ushort cube, ushort value, int amount) {return 0;}
	
		public bool TryExtract(InventoryExtractionOptions options, ref InventoryExtractionResults results) { //THIS IS THE ONE THAT CONVEYORS CALL
			results.Item = null;
			results.Amount = 0;
			ItemBase ib = getGas();
			if (ib != null && ib.GetAmount() > 0) {
				results.Item = ItemManager.CloneItem(ib, options.MinimumAmount);
				results.Amount = options.MinimumAmount;
				ib.DecrementStack(options.MinimumAmount);
				//nulling is not necessary - the machine stacks cleanly if (ib.GetAmount() <= 0)
				//	bottler.mItemsStored[i] = null;
				return true;
			}
			return false;
		}
	
		public bool TryExtractCubes(StorageUserInterface sourceEntity, ushort cube, ushort value, int amount) {return false;}
	
		public bool TryExtractItems(StorageUserInterface sourceEntity, int itemId, int amount, out ItemBase item) {item = null; return false;}
	
		public bool TryExtractItems(StorageUserInterface sourceEntity, int itemId, int amount) {return false;}
	
		public bool TryExtractItemsOrCubes(StorageUserInterface sourceEntity, int itemId, ushort cube, ushort value, int amount, out ItemBase item) {item = null; return false;}
	
		public bool TryExtractItemsOrCubes(StorageUserInterface sourceEntity, int itemId, ushort cube, ushort value, int amount) {return false;}
	
		public bool TryExtractAny(StorageUserInterface sourceEntity, int amount, out ItemBase item) {item = null; return false;}
	
		public int TryPartialExtractCubes(StorageUserInterface sourceEntity, ushort cube, ushort value, int amount) {return 0;}
	
		public int TryPartialExtractItems(StorageUserInterface sourceEntity, int itemId, int amount, out ItemBase item) {item = null; return 0;}
	
		public int TryPartialExtractItems(StorageUserInterface sourceEntity, int itemId, int amount) {return 0;}
	
		public int TryPartialExtractItemsOrCubes(StorageUserInterface sourceEntity, int itemId, ushort cube, ushort value, int amount, out ItemBase item) {item = null; return 0;}
	
		public int TryPartialExtractItemsOrCubes(StorageUserInterface sourceEntity, int itemId, ushort cube, ushort value, int amount) {return 0;}
	
		public int CountItems(InventoryExtractionOptions options) {return 0;}
	
		public int CountItems(int itemId, ushort cube, ushort value) {return 0;}
	
		public int CountItems(int itemId) {return 0;}
	
		public int CountCubes(ushort cube, ushort value) {return 0;}
	
		public int UnloadToList(List<ItemBase> cargoList, int amountToExtract) {return 0;}
		
		public void IterateContents(IterateItem itemFunc, object state) {}
	}
}
