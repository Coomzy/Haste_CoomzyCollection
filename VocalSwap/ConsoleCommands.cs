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
using Zorro.Core.CLI;

namespace VocalSwap;

[ConsoleClassCustomizer("VocalSwap")]
public static partial class Utility
{
	[ConsoleCommand]
	public static void ReloadVoicePacks()
	{
		_ = LoadAllVoicePacks();
	}

	/*[ConsoleCommand]
	public static void DebugSoundKey(string key)
	{
		Debug.LogWarning($"[VocalSwap] DebugSoundKey: {key}");
		var sfx_instance = GetVocalInstance(key, activeVocalBank);
		if (sfx_instance == null)
		{
			Debug.LogError($"[VocalSwap] Failed to get sfx_instance for key '{key}'");
			return;
		}
		Debug.Log($"[VocalSwap] {key}: '{sfx_instance.name}'");
	}*/
}