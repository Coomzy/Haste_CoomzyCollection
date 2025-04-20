using Landfall.Haste;
using Landfall.Modding;
using UnityEngine;
using Zorro.Settings;
using HarmonyLib;
using System.Reflection;
using System.Collections;
using UnityEngine.Networking;
using static UnityEngine.UI.Image;
using JetBrains.Annotations;
using static PlayerVocalSFX;

using static VocalSwap.Main;
using static VocalSwap.Utility;

namespace VocalSwap;

[LandfallPlugin]
public static class Main
{
	static readonly Harmony patcher = new($"coomzy.{Assembly.GetExecutingAssembly().GetName().Name}");

	public static bool initialized = false;
	public static string selectedVoicePack = "Zoe";
	public static VocalBank activeVocalBank = null;
	public static VocalBank zoeVocalBank = null;

	public static readonly List<VoicePackConfig> voicePacks = new List<VoicePackConfig>();
	public static readonly List<string> soundKeys = GetAllSoundKeys();
	public static readonly AudioClip[] emptyClips = [];

	public static string modDataPath = Path.Combine(Application.persistentDataPath, "Mods/VocalSwap/");
	public static string localCharactersPath = Path.Combine(modDataPath, "Characters/");
	public static string selectedCharacterSavePath = Path.Combine(modDataPath, "selectedCharacter.txt");

	static Main()
	{
		patcher.PatchAll();

		if (!Directory.Exists(localCharactersPath))
		{
			Directory.CreateDirectory(localCharactersPath);
		}

		var go = new GameObject(nameof(ModDebugUpdater));
		UnityEngine.Object.DontDestroyOnLoad(go);
		go.AddComponent<ModDebugUpdater>();
	}
}

public enum DamageVariantTypes
{
	None,
	Small,
	Medium,
	Big,
	Death
}

public enum ItemVariantTypes
{
	None,
	Common,
	Rare,
	Epic,
	Legendary
}

[Serializable]
public class VoicePackConfig
{
	public string characterName = "Unnamed";
	public string[] silentEntries;

	[NonSerialized] public string rootPath = null;
	[NonSerialized] public Dictionary<string, AudioClip[]> typeToClips = null;
}

[HarmonyPatch(typeof(PlayerVocalSFX))]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Used by Harmony")]
static class Patch
{	
	[HarmonyPatch("Start"), HarmonyPostfix]
	static async void Cache_VocalBank(PlayerVocalSFX __instance)
	{
		//Debug.Log($"[VocalSwap] Cache_VocalBank()");

		zoeVocalBank = __instance.vocalBank;
		activeVocalBank = CloneVocalBank(zoeVocalBank);
		__instance.vocalBank = activeVocalBank;

		await LoadAllVoicePacks();
		//Debug.Log($"[VocalSwap] Post LoadAllVoicePacks()");

		if (voicePacks.Count < 1)
		{
			Debug.LogError($"[VocalSwap] Error loading voice packs! There should always be at least one (Zoe)!");
			return;
		}

		VoicePackConfig savedVoicePack = voicePacks[0];
		int savedIndex = 0;
		
		for (int i = 0; i < voicePacks.Count; i++)
		{
			Debug.Log($"[VocalSwap] Found voice pack: {voicePacks[i].characterName} at {voicePacks[i].rootPath}");

			if (voicePacks[i].characterName == selectedVoicePack && savedIndex == 0)
			{
				savedVoicePack = voicePacks[i];
				savedIndex = i;
			}
		}

		var setting = GameHandler.Instance.SettingsHandler.GetSetting<SelectedVoicePackSetting>();
		if (setting != null)
		{
			setting.SetValue(savedIndex, null, false);
		}
		else
		{
			Debug.LogError($"[VocalSwap] Failed to get setting 'SelectedVoicePackSetting' so unable to set the selected index correctly");
		}

		ApplyVoicePack(savedVoicePack);
		initialized = true;
	}
}
