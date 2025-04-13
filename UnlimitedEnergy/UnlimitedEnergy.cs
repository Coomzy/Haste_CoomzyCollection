using Landfall.Haste;
using Landfall.Modding;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Localization;
using Zorro.Settings;

namespace UnlimitedEnergy;

[LandfallPlugin]
public class Program
{
    static Program()
	{
		var go = new GameObject(nameof(EnergyUpdater));
		UnityEngine.Object.DontDestroyOnLoad(go);
		go.AddComponent<EnergyUpdater>();
    }
}

public class EnergyUpdater : MonoBehaviour
{
	void Update()
	{
		if (GameHandler.Instance?.SettingsHandler == null)
			return;

		var setting = GameHandler.Instance.SettingsHandler.GetSetting<UnlimitedEnergyTypeSetting>();

		if (setting == null)
			return;

		if (setting.Value == UnlimitedEnergyType.Off)
			return;

		if (setting.Value == UnlimitedEnergyType.HubOnly)
		{
			if (!GM_Hub.isInHub)
			{
				return;
			}
		}

		if (StopHandler.IsStopped)
			return;

		if (Player.localPlayer?.character == null)
			return;

		if (!Player.localPlayer.character.data.allowInput)
			return;

		var maxEnergy = Player.localPlayer.stats.maxEnergy.baseValue * Player.localPlayer.stats.maxEnergy.multiplier;
		if (Player.localPlayer.data.energy >= maxEnergy)
			return;

		Player.localPlayer.data.energy = maxEnergy;
		//PlayerCharacter.localPlayer.data.stopGainEnergy = false;
	}
}

public enum UnlimitedEnergyType
{
	Off,
	HubOnly,
	AlwaysOn
}

[HasteSetting]
public class UnlimitedEnergyTypeSetting : EnumSetting<UnlimitedEnergyType>, IExposedSetting
{
	public override void ApplyValue()
	{
		//Debug.Log($"UnlimitedEnergyTypeSetting apply value {Value}");
	}

	protected override UnlimitedEnergyType GetDefaultValue() => UnlimitedEnergyType.HubOnly;

	public override List<LocalizedString> GetLocalizedChoices() =>
	[
		new UnlocalizedString("Off"),
		new UnlocalizedString("Hub Only"),
		new UnlocalizedString("Always On")
	];

	public LocalizedString GetDisplayName() => new UnlocalizedString("Unlimited Energy Type");
	public string GetCategory() => "Mods";
}