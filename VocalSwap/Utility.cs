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
using Zorro.UI;

namespace VocalSwap;

public static partial class Utility
{
	public static bool isOnMainMenu = true;
	public static bool isOnMainMenuSettings = false;

	public static bool isLookingAtModSettings => (EscapeMenu.IsOpen || isOnMainMenuSettings) && lastShownCategory == "VoiceSwap";
	public static string lastShownCategory = "";

	[HarmonyPatch(typeof(MainMenuMainPage))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Used by Harmony")]
	static class MainMenuMainPage_Patch
	{
		[HarmonyPatch("Start"), HarmonyPostfix]
		static void Start(MainMenuMainPage __instance)
		{
			isOnMainMenu = true;
		}

		[HarmonyPatch("OnPlayButtonClicked"), HarmonyPostfix]
		static void OnPlayButtonClicked(MainMenuMainPage __instance)
		{
			isOnMainMenu = false;
		}
	}

	[HarmonyPatch(typeof(SettingsUIPage))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Used by Harmony")]
	static class SettingsUIPage_Patch
	{
		[HarmonyPatch("ShowSettings"), HarmonyPostfix]
		static void ShowSettings(SettingsUIPage __instance, string category)
		{
			lastShownCategory = category;
			//Debug.Log($"[VoiceSwap] SettingsUIPage::ShowSettings() __category: {category}");
		}
	}

	[HarmonyPatch(typeof(PageBase))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Used by Harmony")]
	static class MainMenuSettingsPage_Patch
	{
		[HarmonyPatch("OnPageEnter"), HarmonyPostfix]
		static void OnPageEnter(MainMenuSettingsPage __instance)
		{
			if (__instance.GetType() != typeof(MainMenuSettingsPage))
				return;

			//Debug.Log($"[VocalSwap] MainMenuSettingsPage::OnPageEnter()");
			isOnMainMenuSettings = true;
		}

		[HarmonyPatch("OnPageExit"), HarmonyPostfix]
		static void OnPageExit(MainMenuSettingsPage __instance)
		{
			if (__instance.GetType() != typeof(MainMenuSettingsPage))
				return;

			//Debug.Log($"[VocalSwap] MainMenuSettingsPage::OnPageExit()");
			isOnMainMenuSettings = false;
		}		
	}
}