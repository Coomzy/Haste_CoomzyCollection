using Landfall.Haste;
using Landfall.Modding;
using UnityEngine;
using Zorro.Settings;
using HarmonyLib;
using System.Reflection;
using static VocalSwap.Main;
using static VocalSwap.Utility;
using System.Collections;
using UnityEngine.Networking;
using static UnityEngine.UI.Image;
using JetBrains.Annotations;
using static PlayerVocalSFX;

namespace VocalSwap;

public static partial class Utility
{
	public static List<string> GetAllSoundKeys()
	{
		var keys = new List<string>();

		foreach (SoundEffectType effectType in Enum.GetValues(typeof(SoundEffectType)))
		{
			// NB: Seems like they never implemented this
			if (effectType == SoundEffectType.EnterShop)
			{
				foreach (ItemVariantTypes variant in Enum.GetValues(typeof(ItemVariantTypes)))
				{
					if (variant == ItemVariantTypes.None)
						continue;
					keys.Add(GetSoundKey(effectType, variant));
				}
				continue;
			}

			if (effectType == SoundEffectType.Damage)
			{
				foreach (DamageVariantTypes variant in Enum.GetValues(typeof(DamageVariantTypes)))
				{
					if (variant == DamageVariantTypes.None)
						continue;
					keys.Add(GetSoundKey(effectType, variant));
				}

				continue; // skip adding "Damage" directly
			}

			keys.Add(effectType.ToString());
		}

		return keys;
	}

	public static string GetSoundKey(SoundEffectType type)
	{
		return type.ToString();
	}
	
	public static string GetSoundKey(SoundEffectType type, DamageVariantTypes variant)
	{
		if (type != SoundEffectType.Damage || variant == DamageVariantTypes.None)
			return GetSoundKey(type);

		return $"{type}{variant}";
	}
	
	public static string GetSoundKey(SoundEffectType type, ItemVariantTypes variant)
	{
		// HACK: Hijack 'EnterShop' because it's not used
		if (type != SoundEffectType.EnterShop || variant == ItemVariantTypes.None)
			return GetSoundKey(type);

		return $"ItemBuy{variant}";
	}

	public static SFX_Instance GetVocalInstance(string key, VocalBank vocalBank)
	{
		if (key == GetSoundKey(SoundEffectType.Running))
			return vocalBank.runningVocals;

		if (key == GetSoundKey(SoundEffectType.StartFastRun))
			return vocalBank.startFastRunVocals;

		if (key == GetSoundKey(SoundEffectType.Damage, DamageVariantTypes.Small))
			return vocalBank.smallDamageVocals;
		if (key == GetSoundKey(SoundEffectType.Damage, DamageVariantTypes.Medium))
			return vocalBank.mediumDamageVocals;
		if (key == GetSoundKey(SoundEffectType.Damage, DamageVariantTypes.Big))
			return vocalBank.bigDamageVocals;
		if (key == GetSoundKey(SoundEffectType.Damage, DamageVariantTypes.Death))
			return vocalBank.deathDamageVocals;

		if (key == GetSoundKey(SoundEffectType.EnterShop, ItemVariantTypes.Common))
			return vocalBank.buyCommonItemVocals;
		if (key == GetSoundKey(SoundEffectType.EnterShop, ItemVariantTypes.Rare))
			return vocalBank.buyRareItemVocals;
		if (key == GetSoundKey(SoundEffectType.EnterShop, ItemVariantTypes.Epic))
			return vocalBank.buyEpicItemVocals;
		if (key == GetSoundKey(SoundEffectType.EnterShop, ItemVariantTypes.Legendary))
			return vocalBank.buyLegendaryItemVocals;

		if (key == GetSoundKey(SoundEffectType.Death))
			return vocalBank.deathVocals;
		if (key == GetSoundKey(SoundEffectType.DeathTooSlow))
			return vocalBank.deathTooSlowVocals;
		if (key == GetSoundKey(SoundEffectType.DeathFallOut))
			return vocalBank.deathFallOutVocals;

		if (key == GetSoundKey(SoundEffectType.CloseCall))
			return vocalBank.closeCallVocals;
		if (key == GetSoundKey(SoundEffectType.EnterPortal))
			return vocalBank.enterPortalVocals;

		if (key == GetSoundKey(SoundEffectType.Landing))
			return vocalBank.landingVocals;
		if (key == GetSoundKey(SoundEffectType.PerfectLanding))
			return vocalBank.perfectLandingVocals;
		if (key == GetSoundKey(SoundEffectType.DiveLanding))
			return vocalBank.diveLandingVocals;

		if (key == GetSoundKey(SoundEffectType.Jumping))
			return vocalBank.jumpingVocals;
		if (key == GetSoundKey(SoundEffectType.Dive))
			return vocalBank.diveVocals;
		if (key == GetSoundKey(SoundEffectType.BigJumping))
			return vocalBank.bigJumpingVocals;

		if (key == GetSoundKey(SoundEffectType.EnterShard))
			return vocalBank.enterShardVocals;
		if (key == GetSoundKey(SoundEffectType.ExitShard))
			return vocalBank.exitShardVocals;
		if (key == GetSoundKey(SoundEffectType.EnterRest))
			return vocalBank.enterRestVocals;

		if (key == GetSoundKey(SoundEffectType.Hello))
			return vocalBank.helloVocals;
		if (key == GetSoundKey(SoundEffectType.Goodbye))
			return vocalBank.goodByeVocals;
		if (key == GetSoundKey(SoundEffectType.NoAfford))
			return vocalBank.noAffordVocals;

		if (key == GetSoundKey(SoundEffectType.FoundEncounter))
			return vocalBank.foundEncounterVocals;
		if (key == GetSoundKey(SoundEffectType.FoundChallenge))
			return vocalBank.foundChallengeVocals;

		if (key == GetSoundKey(SoundEffectType.WonEncounter))
			return vocalBank.wonEncounterVocals;
		if (key == GetSoundKey(SoundEffectType.LostEncounter))
			return vocalBank.failEncounterVocals;
		if (key == GetSoundKey(SoundEffectType.AlmostWonEncounter))
			return vocalBank.almostWinEncounterVocals;

		if (key == GetSoundKey(SoundEffectType.WinRun))
			return vocalBank.WinRunVocals;
		if (key == GetSoundKey(SoundEffectType.LoseRun))
			return vocalBank.loseRunVocals;

		// Unknown key fallback
		Debug.LogError($"[VocalSwap] Unknown Sound Key '{key}', please report this to the mod developer.");
		return null;
	}

	public static VocalBank CloneVocalBank(VocalBank vocalBank)
	{
		var clone = ScriptableObject.Instantiate(vocalBank);

		// Deep clone each referenced SFX_Instance
		clone.runningVocals = ScriptableObject.Instantiate(clone.runningVocals);
		clone.startFastRunVocals = ScriptableObject.Instantiate(clone.startFastRunVocals);
		clone.smallDamageVocals = ScriptableObject.Instantiate(clone.smallDamageVocals);
		clone.mediumDamageVocals = ScriptableObject.Instantiate(clone.mediumDamageVocals);
		clone.bigDamageVocals = ScriptableObject.Instantiate(clone.bigDamageVocals);
		clone.deathDamageVocals = ScriptableObject.Instantiate(clone.deathDamageVocals);
		clone.buyCommonItemVocals = ScriptableObject.Instantiate(clone.buyCommonItemVocals);
		clone.buyRareItemVocals = ScriptableObject.Instantiate(clone.buyRareItemVocals);
		clone.buyEpicItemVocals = ScriptableObject.Instantiate(clone.buyEpicItemVocals);
		clone.buyLegendaryItemVocals = ScriptableObject.Instantiate(clone.buyLegendaryItemVocals);
		clone.deathVocals = ScriptableObject.Instantiate(clone.deathVocals);
		clone.deathTooSlowVocals = ScriptableObject.Instantiate(clone.deathTooSlowVocals);
		clone.deathFallOutVocals = ScriptableObject.Instantiate(clone.deathFallOutVocals);
		clone.closeCallVocals = ScriptableObject.Instantiate(clone.closeCallVocals);
		clone.enterPortalVocals = ScriptableObject.Instantiate(clone.enterPortalVocals);
		clone.landingVocals = ScriptableObject.Instantiate(clone.landingVocals);
		clone.perfectLandingVocals = ScriptableObject.Instantiate(clone.perfectLandingVocals);
		clone.diveLandingVocals = ScriptableObject.Instantiate(clone.diveLandingVocals);
		clone.jumpingVocals = ScriptableObject.Instantiate(clone.jumpingVocals);
		clone.diveVocals = ScriptableObject.Instantiate(clone.diveVocals);
		clone.bigJumpingVocals = ScriptableObject.Instantiate(clone.bigJumpingVocals);
		clone.enterShardVocals = ScriptableObject.Instantiate(clone.enterShardVocals);
		clone.exitShardVocals = ScriptableObject.Instantiate(clone.exitShardVocals);
		clone.enterRestVocals = ScriptableObject.Instantiate(clone.enterRestVocals);
		clone.helloVocals = ScriptableObject.Instantiate(clone.helloVocals);
		clone.goodByeVocals = ScriptableObject.Instantiate(clone.goodByeVocals);
		clone.noAffordVocals = ScriptableObject.Instantiate(clone.noAffordVocals);
		clone.foundEncounterVocals = ScriptableObject.Instantiate(clone.foundEncounterVocals);
		clone.foundChallengeVocals = ScriptableObject.Instantiate(clone.foundChallengeVocals);
		clone.wonEncounterVocals = ScriptableObject.Instantiate(clone.wonEncounterVocals);
		clone.failEncounterVocals = ScriptableObject.Instantiate(clone.failEncounterVocals);
		clone.almostWinEncounterVocals = ScriptableObject.Instantiate(clone.almostWinEncounterVocals);
		clone.WinRunVocals = ScriptableObject.Instantiate(clone.WinRunVocals);
		clone.loseRunVocals = ScriptableObject.Instantiate(clone.loseRunVocals);

		return clone;
	}
}