/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 22/02/2022
 * Time: 6:57 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

using UnityEngine;

namespace ReikaKalseki.FortressTweaks
{
	/// <summary>
	/// Description of MusicReplacement.
	/// </summary>
	public class MusicReplacement
	{	    
		private static AudioMusicManager.eMusicSource currentPlaying;
		private static long lastPlayStart;
		
	    public static AudioMusicManager.eMusicSource getMusicCategory(AudioMusicManager audio) {
			
			if (!GameState.PlayerSpawnedAndHadUpdates)
				return AudioMusicManager.eMusicSource.eSilent;
	
	        if (WorldScript.instance.localPlayerInstance.CurrentRoomID != -1)
	            return AudioMusicManager.eMusicSource.eGenericRoom;//innaroom. Todo, non generic room ambiences?
	
	        long lnX = WorldScript.instance.localPlayerInstance.mWorldX - CentralPowerHub.mCreepX;
	        long lnY = WorldScript.instance.localPlayerInstance.mWorldY - CentralPowerHub.mCreepY;
	        long lnZ = WorldScript.instance.localPlayerInstance.mWorldZ - CentralPowerHub.mCreepZ;
	
	        double distSq = lnX*lnX+lnY*lnY+lnZ*lnZ;
	        if (distSq <= 1024) {
	        	return AudioMusicManager.eMusicSource.eHiveMind;
	        }
	
	        lnX = WorldScript.instance.localPlayerInstance.mWorldX - WorldScript.mDefaultOffset;
	        lnY = WorldScript.instance.localPlayerInstance.mWorldY - WorldScript.mDefaultOffset;
	        lnZ = WorldScript.instance.localPlayerInstance.mWorldZ - WorldScript.mDefaultOffset;
	
	        if (lnY > -32 && MobSpawnManager.mbSurfaceAttacksActive) {
	            //calculate which hivemind is closest
	            if (lnX < 0)
	            	lnX = -MobSpawnManager.OverMindOffset;
	            if (lnZ < 0)
	            	lnZ = -MobSpawnManager.OverMindOffset;
	
	            if (lnX >= 0)
	            	lnX = MobSpawnManager.OverMindOffset;
	            if (lnZ >= 0)
	            	lnZ = MobSpawnManager.OverMindOffset;
	
	            lnX += WorldScript.mDefaultOffset;
	            lnZ += WorldScript.mDefaultOffset;
	
	            //Calculate vector to closest
	            lnX = WorldScript.instance.localPlayerInstance.mWorldX - lnX;
	            lnY = 0;
	            lnZ = WorldScript.instance.localPlayerInstance.mWorldZ - lnZ;
	        	distSq = lnX*lnX+lnY*lnY+lnZ*lnZ;
	
	            if (distSq <= 9216) {
	            	return AudioMusicManager.eMusicSource.eOvermind;
	            }
	        }
	
	        //Not close to something that requires specific effects; check depth
	
	        if (SurvivalFogManager.GlobalDepth > -32) {
	            //Room -> outdoors == silent
	            //Anything else -> Outdoors == surface music	
	            if (audio.mPreviousMusic == AudioMusicManager.eMusicSource.eGenericRoom)
	                return AudioMusicManager.eMusicSource.eSilent;
	            else
	                return AudioMusicManager.eMusicSource.eSurface;
	        } //TODO add cooldown for a given track
	        else if (SurvivalFogManager.GlobalDepth > BiomeLayer.CavernColdCeiling) {
	        	return AudioMusicManager.eMusicSource.eUpperCave;
	        }
	        else if (SurvivalFogManager.GlobalDepth > BiomeLayer.CavernColdFloor) {
	            if (CCCCC.ActiveAndWorking && false)
	                return AudioMusicManager.eMusicSource.eColdCavern_FF;
	            else
	                return AudioMusicManager.eMusicSource.eColdCavern;
	        }	        
	        else if (SurvivalFogManager.GlobalDepth > BiomeLayer.CavernToxicCeiling) {
	        	return AudioMusicManager.eMusicSource.eLowerCave;
	        }	        
	        else if (SurvivalFogManager.GlobalDepth > BiomeLayer.CavernToxicFloor) {
	        	return AudioMusicManager.eMusicSource.eToxic;
	        }	        
	        else if (SurvivalFogManager.GlobalDepth > BiomeLayer.CavernMagmaCeiling) {
	        	return AudioMusicManager.eMusicSource.eUnderToxic;
	        }
	        else if (SurvivalFogManager.GlobalDepth > BiomeLayer.CavernMagmaFloor) {
	        	return AudioMusicManager.eMusicSource.eMagma;
	        }
	        //TODO custom music for deep?
	        return AudioMusicManager.eMusicSource.eSilent;
		}
		
	}
}
