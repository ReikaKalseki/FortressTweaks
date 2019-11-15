using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System;
using System.Collections.Generic;

namespace ReikaKalseki.FortressTweaks {

	public class BasinInterfaceWrapper : StorageMachineInterface {
		
		private readonly ContinuousCastingBasin basin;
		
		public int TotalCapacity {set;get;}
		public int UsedCapacity {set;get;}
		public int RemainingCapacity {set;get;}
		public bool InventoryExtractionPermitted {set;get;}
		
		internal BasinInterfaceWrapper(ContinuousCastingBasin te) {
			basin = te;
			
			TotalCapacity = 10000;
			InventoryExtractionPermitted = true;
			UsedCapacity = 0;
			for (int i = 0; i < basin.mItemsStored.Length; i++) {
				if (basin.mItemsStored[i] != null)
					UsedCapacity += basin.mItemsStored[i].GetAmount();
			}
			RemainingCapacity = TotalCapacity-UsedCapacity;
		}
		
		public eHopperPermissions GetPermissions() {
			return eHopperPermissions.RemoveOnly;
		}
	
		public bool IsEmpty() {
			for (int i = 0; i < basin.mItemsStored.Length; i++) {
				if (basin.mItemsStored[i] != null)
					return false;
			}
			return true;
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
			for (int i = 0; i < basin.mItemsStored.Length; i++) {
				if (basin.mItemsStored[i] != null && basin.mItemsStored[i].GetAmount() > 0) {
					//Debug.Log("Belt meta "+((ConveyorEntity)options.SourceEntity).mValue+" is requesting "+options.ExemplarBlockID+"/"+options.ExemplarItemID+" amt "+options.MinimumAmount+"/"+options.MaximumAmount);
					results.Item = ItemManager.CloneItem(basin.mItemsStored[i], options.MinimumAmount);
					results.Amount = options.MinimumAmount;
					/*
					Debug.Log("Stored item is:");
					Debug.Log(basin.mItemsStored[i].GetType());
					Debug.Log(basin.mItemsStored[i].IsStack());
					Debug.Log(basin.mItemsStored[i].mType);
					Debug.Log(basin.mItemsStored[i].GetAmount());
					Debug.Log(basin.mItemsStored[i].GetName());
					*/
					basin.mItemsStored[i].DecrementStack(options.MinimumAmount);
					if (basin.mItemsStored[i].GetAmount() <= 0)
						basin.mItemsStored[i] = null;
					return true;
				}
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
