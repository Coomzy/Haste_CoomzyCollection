using Landfall.Haste;
using Landfall.Modding;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Localization;
using Zorro.Settings;
using HarmonyLib;

namespace FullEnergyStart;

[LandfallPlugin]
public class Program
{
	private static readonly Harmony patcher = new($"coomzy.{nameof(FullEnergyStartUpdater)}");

	static Program()
	{
		patcher.PatchAll();

		var go = new GameObject(nameof(FullEnergyStartUpdater));
		UnityEngine.Object.DontDestroyOnLoad(go);
		go.AddComponent<FullEnergyStartUpdater>();
    }
}

public class FullEnergyStartUpdater : MonoBehaviour
{
	void OnEnable()
	{
		GM_API.NewLevel += ReplenishEnergy;
		GM_API.LevelRestart += ReplenishEnergy;
		GM_API.SpawnedInHub += ReplenishEnergy;
	}

	void OnDisable()
	{
		GM_API.NewLevel -= ReplenishEnergy;
		GM_API.LevelRestart -= ReplenishEnergy;
		GM_API.SpawnedInHub -= ReplenishEnergy;
	}

	public static void ReplenishEnergy()
	{
		if (Player.localPlayer?.character == null)
		{
			//Debug.Log($"[FullEnergyStart] ReplenishEnergy() but no character :(");
			return;
		}

		var maxEnergy = Player.localPlayer.stats.maxEnergy.baseValue * Player.localPlayer.stats.maxEnergy.multiplier;
		if (Player.localPlayer.data.energy >= maxEnergy)
			return;

		Player.localPlayer.data.energy = maxEnergy;
	}
}

[HarmonyPatch(typeof(EndBoss))]
static class EndBossPatch
{
	[HarmonyPatch(nameof(EndBoss.StartBoss))]
	[HarmonyPostfix]
	static void Postfix(EndBoss __instance)
	{
		FullEnergyStartUpdater.ReplenishEnergy();
	}
}
