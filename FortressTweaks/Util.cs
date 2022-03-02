﻿using System;

using System.Collections.Generic;

using UnityEngine;

namespace ReikaKalseki.FortressTweaks
{
	public static class Util {
		
		public static void log(string s) {
			Debug.Log("FORTRESSTWEAKS: "+s);
		}
		
	    public static double py3d(double rawX, double rawY, double rawZ, double rawX2, double rawY2, double rawZ2) {
	    	double dx = rawX2-rawX;
	    	double dy = rawY2-rawY;
	    	double dz = rawZ2-rawZ;
	    	return Math.Sqrt(dx*dx+dy*dy+dz*dz);
	    }
		
		public static void addCommand(string key, string desc, CmdParameterType type, Action callback) {
			//Console.AddCommand(new ConsoleCommand(key, desc, type, this.gameObject, callback));
		}
		
		public static CraftData getRecipeByKey(string key, string cat = "Manufacturer") {
			foreach (CraftData recipe in CraftData.GetRecipesForSet(cat)) {
				if (recipe.Key == key) {
					return recipe;
				}
			}
			return null;
		}
		
		public static List<CraftData> getRecipesFor(string output, string cat = "Manufacturer") {
			List<CraftData> li = new List<CraftData>();
			foreach (CraftData recipe in CraftData.GetRecipesForSet(cat)) {
				if (recipe.CraftedKey == output) {
					li.Add(recipe);
				}
			}
			return li;
		}
		
		public static void modifyIngredientCount(CraftData rec, string item, uint newAmt) {
			foreach (CraftCost ing in rec.Costs) {
				if (ing.Key == item) {
					ing.Amount = newAmt;
					log("Changed amount of "+item+" to "+newAmt+" in recipe "+recipeToString(rec, true));
				}
			}
		}
		
		public static void removeIngredient(CraftData rec, string item) {
			for (int i = rec.Costs.Count-1; i >= 0; i--) {
				CraftCost ing = rec.Costs[i];
				if (ing.Key == item) {
					rec.Costs.RemoveAt(i);
					log("Removed "+item+" from recipe "+recipeToString(rec, true));
				}
			}
		}
		
		public static void addIngredient(CraftData rec, string item, uint amt) {
			CraftCost cost = new CraftCost();
			cost.Amount = amt;
			cost.Key = item;
			rec.Costs.Add(cost);
			log("Added "+amt+" of "+item+" to recipe "+recipeToString(rec, true));
			link(rec);
		}
		
		public static CraftData addRecipe(string id, string item, int amt = 1, string cat = "Manufacturer") {
			CraftData rec = new CraftData();
			rec.Category = cat;
			rec.Key = "ReikaKalseki."+id;
			rec.CraftedKey = item;
			rec.CraftedAmount = amt;
			CraftData.mRecipesForSet[cat].Add(rec);
			link(rec);
			log("Added new recipe "+recipeToString(rec, true, true));
			return rec;
		}
		
		private static void link(CraftData rec) {
			CraftData.LinkEntries(new List<CraftData>(new CraftData[]{rec}), rec.Category);
		}
		
		public static void removeResearch(CraftData rec, string key) {
			rec.ResearchRequirements.Remove(key);
        	ResearchDataEntry e = ResearchDataEntry.GetResearchDataEntry(key);
        	if (e != null) {
        		rec.ResearchRequirementEntries.Remove(e);
        		log("Removed research '"+key+"' from recipe "+recipeToString(rec, false, true));
        	}
		}
		
		public static void addResearch(CraftData rec, string key) {
			rec.ResearchRequirements.Add(key);
        	ResearchDataEntry e = ResearchDataEntry.GetResearchDataEntry(key);
        	if (e != null) {
        		rec.ResearchRequirementEntries.Add(e);
				log("Added research '"+key+"' to recipe "+recipeToString(rec, false, true));
        	}
		}
		
		public static string ingredientToString(CraftCost ing) {
			return ing.Key+" x "+ing.Amount+" ("+ing.Name+")";
		}
		
		public static string recipeToString(CraftData rec, bool fullIngredients = false, bool fullResearch = false) {
			string ret = "'"+rec.Category+"::"+rec.Key+"'="+rec.CraftedKey+"x"+rec.CraftedAmount+" from ";
			if (fullIngredients) {
				List<string> li = new List<string>();
				rec.Costs.ForEach(c => li.Add(ingredientToString(c)));
				ret += "I["+string.Join(", ", li.ToArray())+"]";
			}
			else {
				ret += rec.Costs.Count+" items";
			}
			ret += " & ";
			if (fullResearch) {
				ret += "T["+string.Join(", ", rec.ResearchRequirements.ToArray())+"]";
			}
			else {
				ret += rec.ResearchRequirements.Count+" techs";
			}
			return ret;
		}
		
	}
}
