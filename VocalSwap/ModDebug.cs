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

public static partial class ModDebug
{
	public static void TestSound(string soundKey)
	{
		PlayerVocalSFX.SoundEffectType effectType;
		SFX_Instance sfx_Instance = null;

		// Handle damage variants
		if (soundKey == GetSoundKey(SoundEffectType.Damage, DamageVariantTypes.Small))
		{
			effectType = SoundEffectType.Damage;
			sfx_Instance = PlayerVocalSFX.Instance.vocalBank.smallDamageVocals;
		}
		else if (soundKey == GetSoundKey(SoundEffectType.Damage, DamageVariantTypes.Medium))
		{
			effectType = SoundEffectType.Damage;
			sfx_Instance = PlayerVocalSFX.Instance.vocalBank.mediumDamageVocals;
		}
		else if (soundKey == GetSoundKey(SoundEffectType.Damage, DamageVariantTypes.Big))
		{
			effectType = SoundEffectType.Damage;
			sfx_Instance = PlayerVocalSFX.Instance.vocalBank.bigDamageVocals;
		}
		else if (soundKey == GetSoundKey(SoundEffectType.Damage, DamageVariantTypes.Death))
		{
			effectType = SoundEffectType.Damage;
			sfx_Instance = PlayerVocalSFX.Instance.vocalBank.deathDamageVocals;
		}
		else if (soundKey == GetSoundKey(SoundEffectType.EnterShop, ItemVariantTypes.Common))
		{
			effectType = SoundEffectType.EnterShop;
			sfx_Instance = PlayerVocalSFX.Instance.vocalBank.buyCommonItemVocals;
		}
		else if (soundKey == GetSoundKey(SoundEffectType.EnterShop, ItemVariantTypes.Rare))
		{
			effectType = SoundEffectType.EnterShop;
			sfx_Instance = PlayerVocalSFX.Instance.vocalBank.buyRareItemVocals;
		}
		else if (soundKey == GetSoundKey(SoundEffectType.EnterShop, ItemVariantTypes.Epic))
		{
			effectType = SoundEffectType.EnterShop;
			sfx_Instance = PlayerVocalSFX.Instance.vocalBank.buyEpicItemVocals;
		}
		else if (soundKey == GetSoundKey(SoundEffectType.EnterShop, ItemVariantTypes.Legendary))
		{
			effectType = SoundEffectType.EnterShop;
			sfx_Instance = PlayerVocalSFX.Instance.vocalBank.buyLegendaryItemVocals;
		}
		else
		{
			// Try parse normally
			if (!Enum.TryParse(soundKey, out effectType))
			{
				Debug.LogWarning($"[VocalSwap] Unknown sound key '{soundKey}' passed to TestSound");
				return;
			}
		}
		var instance = PlayerVocalSFX.Instance;
		instance.lastpriority = PlayerVocalSFX.Priority.High;

		// HACK: We're hijacking enter shop for 
		if (effectType == SoundEffectType.Damage || effectType == SoundEffectType.EnterShop)
		{
			if (sfx_Instance == null)
			{
				return;
			}

			var methodPlay = typeof(PlayerVocalSFX).GetMethod("Play", BindingFlags.Instance | BindingFlags.NonPublic);
			if (methodPlay == null)
			{
				Debug.LogError("[VocalSwap] Failed to find internal method 'methodPlayDamageVocal'");
				return;
			}
			methodPlay.Invoke(instance, new object[] { sfx_Instance, PlayerVocalSFX.Priority.Top, 0.0f });
			return;
		}

		// Use reflection to call internal PlayVocal
		var method = typeof(PlayerVocalSFX).GetMethod("PlayVocal", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method == null)
		{
			Debug.LogError("[VocalSwap] Failed to find internal method 'PlayVocal'");
			return;
		}

		method.Invoke(instance, new object[] { effectType, PlayerVocalSFX.Priority.Top, 0.0f });
	}
}

public class ModDebugUpdater : MonoBehaviour
{
	public static ModDebugUpdater instance;

	public readonly static HashSet<string> brokenSoundKeys = new HashSet<string>();

	void Awake()
	{
		instance = this;

		brokenSoundKeys.Add(GetSoundKey(SoundEffectType.WonEncounter));
		brokenSoundKeys.Add(GetSoundKey(SoundEffectType.LostEncounter));
		brokenSoundKeys.Add(GetSoundKey(SoundEffectType.WinRun));
		brokenSoundKeys.Add(GetSoundKey(SoundEffectType.LoseRun));

		if (GameHandler.Instance?.SettingsHandler == null)
		{
			this.enabled = false;
			return;
		}

		var setting = GameHandler.Instance.SettingsHandler.GetSetting<EnableDebugModeSetting>();

		if (setting == null)
		{
			this.enabled = false;
			return;
		}

		if (!setting.Value)
		{
			this.enabled = false;
			return;
		}
	}

	void OnGUI()
	{
		bool shouldShowUI = isLookingAtModSettings;
		if (!shouldShowUI)
			shouldShowUI = isOnMainMenu && !isOnMainMenuSettings;

		if (!shouldShowUI)
			return;

		const int columns = 5;
		const int buttonWidth = 200;
		const int buttonHeight = 30;
		const int padding = 10;
		const int spacingBetweenCharacterAndGrid = 20;
		const int characterButtonHeight = 30;

		var soundKeys = GetAllSoundKeys();
		int totalKeys = soundKeys.Count;
		int rowCount = Mathf.CeilToInt(totalKeys / (float)columns);

		int gridWidth = columns * (buttonWidth + padding);
		int gridHeight = rowCount * (buttonHeight + padding);
		int characterRowHeight = characterButtonHeight + padding;

		int totalHeight = characterRowHeight + spacingBetweenCharacterAndGrid + gridHeight;
		int startX = (Screen.width - gridWidth) / 2;
		int startY = Screen.height - totalHeight - 150; // Anchored to bottom (with small margin)

		GUI.BeginGroup(new Rect(startX, startY, gridWidth, totalHeight));

		// Character select buttons (drawn last — at the top)
		for (int i = 0; i < voicePacks.Count; i++)
		{
			var pack = voicePacks[i];
			float x = i * (buttonWidth + padding);
			float y = 0; // top of the group

			if (GUI.Button(new Rect(x, y, buttonWidth, characterButtonHeight), pack.characterName))
			{
				ApplyVoicePack(pack);
			}
		}

		// Sound key buttons (drawn starting from bottom and going up)
		int index = 0;
		for (int i = 0; i < soundKeys.Count; i++)
		{
			if (brokenSoundKeys.Contains(soundKeys[i]))
				continue;

			int row = index / columns;
			int col = index % columns;

			float x = col * (buttonWidth + padding);
			float y = characterRowHeight + spacingBetweenCharacterAndGrid + row * (buttonHeight + padding);

			if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), soundKeys[i]))
			{
				ModDebug.TestSound(soundKeys[i]);
			}
			index++;
		}

		GUI.EndGroup();
	}
}