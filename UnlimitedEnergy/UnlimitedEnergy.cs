using Landfall.Haste;
using Landfall.Modding;
using UnityEngine;
using UnityEngine.Localization;
using Zorro.Settings;

namespace FullEnergyStart;

[LandfallPlugin]
public class UnlimitedEnergy
{
	public static bool IsInHub { get; private set; }

	public static UnlimitedEnergyType UnlimitedEnergyType;

	static UnlimitedEnergy()
	{
		On.RunHandler.StartNewRun += OnStartNewRun;
		On.GM_Hub.Start += GmHubStart;
		On.GM_Hub.OnDestroy += GmHubOnDestroy;

		var go = new GameObject(nameof(EnergyUpdater));
		UnityEngine.Object.DontDestroyOnLoad(go);
		go.AddComponent<EnergyUpdater>();
	}

	private static void GmHubStart(On.GM_Hub.orig_Start orig, GM_Hub self)
	{
		orig(self);
		IsInHub = true;
	}

	private static void GmHubOnDestroy(On.GM_Hub.orig_OnDestroy orig, GM_Hub self)
	{
		orig(self);
		IsInHub = false;
	}

	private static void OnStartNewRun(On.RunHandler.orig_StartNewRun orig, RunConfig setconfig, int shardid, int seed, RunConfigRuntimeData setrunconfigruntimedata)
	{
		orig(setconfig, shardid, seed, setrunconfigruntimedata);
		IsInHub = false;

		var isStartingAtMax = GameHandler.Instance.SettingsHandler.GetSetting<StartWithFullEnergySetting>().Value;
		EnergyUpdater.SetPlayerEnergy(isStartingAtMax);
	}
}

public class EnergyUpdater : MonoBehaviour
{
	internal void Update()
	{
		switch (UnlimitedEnergy.UnlimitedEnergyType)
		{
			case UnlimitedEnergyType.Off:
			case UnlimitedEnergyType.HubOnly when !UnlimitedEnergy.IsInHub:
				return;

			case UnlimitedEnergyType.AlwaysOn:
			default:
				SetPlayerEnergy();
				break;
		}
	}

	public static void SetPlayerEnergy(bool isFull = true)
	{
		if (StopHandler.IsStopped
			|| Player.localPlayer?.character == null
			|| !Player.localPlayer.character.data.allowInput)
		{
			return;
		}

		if (!isFull)
		{
			Player.localPlayer.data.energy = 0;
			return;
		}

		var maxEnergy = Player.localPlayer.stats.maxEnergy.baseValue * Player.localPlayer.stats.maxEnergy.multiplier;

		if (Player.localPlayer.data.energy < maxEnergy)
		{
			Player.localPlayer.data.energy = maxEnergy;
		}
	}
}

public enum UnlimitedEnergyType
{
	Off = 0,
	HubOnly = 1,
	AlwaysOn = 2
}

[HasteSetting]
public class UnlimitedEnergyTypeSetting : EnumSetting<UnlimitedEnergyType>, IExposedSetting
{
	public override void ApplyValue()
	{
		UnlimitedEnergy.UnlimitedEnergyType = Value;
	}

	public override void Load(ISettingsSaveLoad loader)
	{
		base.Load(loader);
		UnlimitedEnergy.UnlimitedEnergyType = Value;
	}

	protected override UnlimitedEnergyType GetDefaultValue() => UnlimitedEnergyType.HubOnly;

	public override List<LocalizedString> GetLocalizedChoices() => new()
	{
		new UnlocalizedString("Off"),
		new UnlocalizedString("Hub Only"),
		new UnlocalizedString("Always On")
	};

	public LocalizedString GetDisplayName() => new UnlocalizedString("Unlimited Energy Type");

	public string GetCategory() => "Mods";
}

[HasteSetting]
public class StartWithFullEnergySetting : BoolSetting, IExposedSetting
{
	public override void ApplyValue()
	{
	}

	protected override bool GetDefaultValue() => false;

	public override LocalizedString OffString => new UnlocalizedString("Disabled (Default)");

	public override LocalizedString OnString => new UnlocalizedString("Enabled");

	public LocalizedString GetDisplayName() => new UnlocalizedString("Unlimited Energy: Start run with full energy");

	public string GetCategory() => "Mods";
}