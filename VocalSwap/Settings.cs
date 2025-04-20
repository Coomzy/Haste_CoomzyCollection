using Landfall.Haste;
using Landfall.Modding;
using UnityEngine;
using Zorro.Settings;
using HarmonyLib;
using System.Reflection;
using static PlayerVocalSFX;
using static VocalSwap.Main;
using static VocalSwap.Utility;
using UnityEngine.Localization;
using System.Globalization;
using UnityEngine.Localization.Settings;
using Zorro.Core;
using Zorro.Settings.DebugUI;
using Unity.Collections.LowLevel.Unsafe;
using System.IO;

namespace VocalSwap;

[HasteSetting]
public class SelectedVoicePackSetting : Setting, IEnumSetting, IExposedSetting
{
	public int index = 0;

	public override void Load(ISettingsSaveLoad loader)
	{
		//Debug.Log($"[VocalSwap] Load()");
		if (File.Exists(selectedCharacterSavePath))
		{
			string loadedSelectedCharacter = File.ReadAllText(selectedCharacterSavePath);
			selectedVoicePack = loadedSelectedCharacter;
		}
	}

	public override void Save(ISettingsSaveLoad saver){}

	public override void ApplyValue()
	{
		if (!initialized)
			return;

		//Debug.Log($"[VocalSwap] ApplyValue()");
		if (voicePacks.WithinRange(index))
		{
			ApplyVoicePack(voicePacks[index]);
		}
	}

	public override SettingUI GetDebugUI(ISettingHandler settingHandler)
	{
		return new EnumSettingsUI(this, settingHandler);
	}

	public override GameObject GetSettingUICell()
	{
		return SingletonAsset<InputCellMapper>.Instance.EnumSettingCell;
	}

	public List<string> GetUnlocalizedChoices()
	{
		List<string> choices = new List<string>();
		foreach (var voicePack in voicePacks)
		{
			choices.Add(voicePack.characterName);
		}
		return choices;
	}

	public List<LocalizedString> GetLocalizedChoices()
	{
		return null;
	}

	public LocalizedString GetDisplayName()
	{
		return new UnlocalizedString("Vocals");
	}

	public string GetCategory()
	{
		return "VoiceSwap";
	}

	public int GetValue()
	{
		return index;
	}

	public void SetValue(int v, ISettingHandler settingHandler, bool fromUI)
	{
		index = v;
		this.ApplyValue();
	}
}

[HasteSetting]
public class ResetItemsSetting : ButtonSetting, IExposedSetting
{
	public override void ApplyValue()
	{
		//Debug.Log($"UnlimitedEnergyTypeSetting apply value {Value}");
	}

	public LocalizedString GetDisplayName() => new UnlocalizedString("Voice Packs");
	public string GetCategory() => "VoiceSwap";

	public override void OnClicked(ISettingHandler settingHandler)
	{
		ReloadVoicePacks();
	}

	public override string GetButtonText() => "Reload";
}

[HasteSetting]
public class EnableDebugModeSetting : BoolSetting, IExposedSetting
{
	public override void ApplyValue()
	{
		ModDebugUpdater.instance.enabled = Value;
	}

	protected override bool GetDefaultValue() => false;

	public LocalizedString GetDisplayName() => new UnlocalizedString("Debug Mode");
	public string GetCategory() => "VoiceSwap";
	public override LocalizedString OffString => new UnlocalizedString("Off");
	public override LocalizedString OnString => new UnlocalizedString("On");
}

/*[HasteSetting]
public class Running_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString(GetType().Name.Replace("_Setting", ""));
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound(GetType().Name.Replace("_Setting", ""));
	public override string GetButtonText() => "Test";
}*/

/*[HasteSetting]
public class Running_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Running");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("Running");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class StartFastRun_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("StartFastRun");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("StartFastRun");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class Damage_Small_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Damage_Small");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("Damage_Small");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class Damage_Medium_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Damage_Medium");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("Damage_Medium");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class Damage_Big_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Damage_Big");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("Damage_Big");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class Damage_Death_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Damage_Death");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("Damage_Death");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class Death_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Death");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("Death");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class DeathTooSlow_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("DeathTooSlow");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("DeathTooSlow");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class DeathFallOut_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("DeathFallOut");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("DeathFallOut");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class CloseCall_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("CloseCall");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("CloseCall");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class EnterPortal_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("EnterPortal");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("EnterPortal");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class Landing_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Landing");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("Landing");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class PerfectLanding_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("PerfectLanding");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("PerfectLanding");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class DiveLanding_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("DiveLanding");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("DiveLanding");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class Jumping_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Jumping");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("Jumping");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class Dive_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Dive");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("Dive");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class BigJumping_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("BigJumping");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("BigJumping");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class EnterShard_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("EnterShard");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("EnterShard");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class ExitShard_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("ExitShard");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("ExitShard");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class EnterRest_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("EnterRest");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("EnterRest");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class Hello_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Hello");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("Hello");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class Goodbye_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Goodbye");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("Goodbye");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class NoAfford_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("NoAfford");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("NoAfford");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class FoundEncounter_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("FoundEncounter");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("FoundEncounter");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class FoundChallenge_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("FoundChallenge");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("FoundChallenge");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class WonEncounter_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("WonEncounter");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("WonEncounter");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class LostEncounter_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("LostEncounter");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("LostEncounter");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class AlmostWonEncounter_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("AlmostWonEncounter");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("AlmostWonEncounter");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class WinRun_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("WinRun");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("WinRun");
	public override string GetButtonText() => "Test";
}

[HasteSetting]
public class LoseRun_Setting : ButtonSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("LoseRun");
	public string GetCategory() => "VoiceSwap";
	public override void OnClicked(ISettingHandler settingHandler) => Settings.TestSound("LoseRun");
	public override string GetButtonText() => "Test";
}*/
