using Landfall.Haste;
using Landfall.Modding;
using System.Collections;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UIElements;
using Zorro.ControllerSupport;
using Zorro.Settings;

namespace AbilitySwitcher;

[LandfallPlugin]
public class Program
{
    static Program()
	{
		var go = new GameObject(nameof(AbilitySwitcherUpdater));
		UnityEngine.Object.DontDestroyOnLoad(go);
		go.AddComponent<AbilitySwitcherUpdater>();
	}
}

public class AbilitySwitcherUpdater : MonoBehaviour
{
	// This are rebindable but you have to know the control paths... that sucks but I don't want to invest a ton of time into it
	public static InputAction boardBoostAction = new InputAction("BoardBoost", InputActionType.Button);
	public static InputAction slomoAction = new InputAction("Slomo", InputActionType.Button);
	public static InputAction grappleAction = new InputAction("Grapple", InputActionType.Button);
	public static InputAction flyAction = new InputAction("Fly", InputActionType.Button);

	IEnumerator Start()
	{
		while (GameHandler.Instance?.SettingsHandler == null)
		{
			yield return null;
		}

		SetupKeybinds();
	}

	void Update()
	{
		if (boardBoostAction.WasPerformedThisFrame())
		{
			var result = TrySetAbilityKind(AbilityKind.BoardBoost);
			//Debug.Log($"Input.GetKeyDown(KeyCode.J) - Set Ability BoardBoost! Result: {result}");
		}
		if (slomoAction.WasPerformedThisFrame())
		{
			var result = TrySetAbilityKind(AbilityKind.Slomo);
			//Debug.Log($"Input.GetKeyDown(KeyCode.K) - Set Ability Slomo! Result: {result}");
		}
		if (grappleAction.WasPerformedThisFrame())
		{
			var result = TrySetAbilityKind(AbilityKind.Grapple);
			//Debug.Log($"Input.GetKeyDown(KeyCode.L) - Set Ability Grapple! Result: {result}");
		}
		if (flyAction.WasPerformedThisFrame())
		{
			var result = TrySetAbilityKind(AbilityKind.Fly);
			//Debug.Log($"Input.GetKeyDown(KeyCode.B) - Set Ability Fly! Result: {result}");
		}

		/* The original plan was to have a toggle switch, but I've moved to a per button option
		 * 
		 * if (Input.GetKeyDown(KeyCode.N))
		{
			Debug.Log($"Input.GetKeyDown(KeyCode.N) - Change Ability Forwards!");
			StepAbility();
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			Debug.Log($"Input.GetKeyDown(KeyCode.M) - Change Ability Backwards!");
			StepAbility(false);
		}*/
	}

	public void StepAbility(bool forward = true, AbilityKind? firstChecked = null, int? currentRaw = null)
	{
		int max = Enum.GetValues(typeof(AbilityKind)).Cast<int>().Max();
		int current = currentRaw ?? (int)FactSystem.GetFact(MetaProgression.ActiveAbility);

		// Assign firstChecked once at the very beginning
		if (firstChecked == null)
			firstChecked = (AbilityKind)current;

		int next = forward ? current + 1 : current - 1;

		// Wrap around
		if (next > max) next = 0;
		if (next < 0) next = max;

		var nextAbility = (AbilityKind)next;


		if (!MetaProgression.IsUnlocked(nextAbility))
		{
			// Check for full loop
			if (nextAbility == firstChecked)
				return;
			StepAbility(forward, firstChecked, next);
			return;
		}

		FactSystem.SetFact(MetaProgression.ActiveAbility, (float)nextAbility);
	}


	public bool TrySetAbilityKind(AbilityKind abilityKind)
	{
		if (!MetaProgression.IsUnlocked(abilityKind))
		{
			return false;
		}

		FactSystem.SetFact(MetaProgression.ActiveAbility, (float)abilityKind);
		return true;
	}

	void OnEnable()
	{
		// This fails the first time, that's what the Start() coroutine is for
		if (GameHandler.Instance?.SettingsHandler == null)
			return;

		SetupKeybinds();
	}

	public static void SetupKeybinds()
	{
		if (GameHandler.Instance?.SettingsHandler == null)
		{
			Debug.LogError($"NO! You tried to call SetupKeybinds() before SettingsHandler was ready");
			return;
		}

		boardBoostAction.RemoveAllBindingOverrides();
		boardBoostAction.AddBinding(GetKeybindString<KeybindSetting_BoardBoost_Keyboard>());
		boardBoostAction.AddBinding(GetKeybindString<KeybindSetting_BoardBoost_Gamepad>());
		boardBoostAction.Enable();

		slomoAction.RemoveAllBindingOverrides();
		slomoAction.AddBinding(GetKeybindString<KeybindSetting_Slomo_Keyboard>());
		slomoAction.AddBinding(GetKeybindString<KeybindSetting_Slomo_Gamepad>());
		slomoAction.Enable();

		grappleAction.RemoveAllBindingOverrides();
		grappleAction.AddBinding(GetKeybindString<KeybindSetting_Grapple_Keyboard>());
		grappleAction.AddBinding(GetKeybindString<KeybindSetting_Grapple_Gamepad>());
		grappleAction.Enable();

		flyAction.RemoveAllBindingOverrides();
		flyAction.AddBinding(GetKeybindString<KeybindSetting_Fly_Keyboard>());
		flyAction.AddBinding(GetKeybindString<KeybindSetting_Fly_Gamepad>());
		flyAction.Enable();
	}

	public static string GetKeybindString<T>() where T : StringSetting
	{
		var setting = GameHandler.Instance?.SettingsHandler.GetSetting<T>();

		if (setting == null)
			return "";

		return setting.Value;
	}
}

[HasteSetting]
public class KeybindSetting_BoardBoost_Keyboard : StringSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Board Boost - Keyboard");
	public string GetCategory() => "Ability Switcher";
	public override void ApplyValue() => AbilitySwitcherUpdater.SetupKeybinds();
	protected override string GetDefaultValue() => "<Keyboard>/z";
}

[HasteSetting]
public class KeybindSetting_BoardBoost_Gamepad : StringSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Board Boost - Gamepad");
	public string GetCategory() => "Ability Switcher";
	public override void ApplyValue() => AbilitySwitcherUpdater.SetupKeybinds();
	protected override string GetDefaultValue() => "<Gamepad>/dpad/up";
}

[HasteSetting]
public class KeybindSetting_Slomo_Keyboard : StringSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Slomo - Keyboard");
	public string GetCategory() => "Ability Switcher";
	public override void ApplyValue() => AbilitySwitcherUpdater.SetupKeybinds();
	protected override string GetDefaultValue() => "<Keyboard>/x";
}

[HasteSetting]
public class KeybindSetting_Slomo_Gamepad : StringSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Slomo - Gamepad");
	public string GetCategory() => "Ability Switcher";
	public override void ApplyValue() => AbilitySwitcherUpdater.SetupKeybinds();
	protected override string GetDefaultValue() => "<Gamepad>/dpad/right";
}

[HasteSetting]
public class KeybindSetting_Grapple_Keyboard : StringSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Grapple - Keyboard");
	public string GetCategory() => "Ability Switcher";
	public override void ApplyValue() => AbilitySwitcherUpdater.SetupKeybinds();
	protected override string GetDefaultValue() => "<Keyboard>/c";
}

[HasteSetting]
public class KeybindSetting_Grapple_Gamepad : StringSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Grapple - Gamepad");
	public string GetCategory() => "Ability Switcher";
	public override void ApplyValue() => AbilitySwitcherUpdater.SetupKeybinds();
	protected override string GetDefaultValue() => "<Gamepad>/dpad/down";
}

[HasteSetting]
public class KeybindSetting_Fly_Keyboard : StringSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Fly - Keyboard");
	public string GetCategory() => "Ability Switcher";
	public override void ApplyValue() => AbilitySwitcherUpdater.SetupKeybinds();
	protected override string GetDefaultValue() => "<Keyboard>/v";
}

[HasteSetting]
public class KeybindSetting_Fly_Gamepad : StringSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("Fly - Gamepad");
	public string GetCategory() => "Ability Switcher";
	public override void ApplyValue() => AbilitySwitcherUpdater.SetupKeybinds();
	protected override string GetDefaultValue() => "<Gamepad>/dpad/left";
}


/*public enum UnlimitedEnergyType
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
}*/


/*[HasteSetting]
public class AbilitySwitcherSetting : InputRebindSetting, IExposedSetting
{
	public override void ApplyValue()
	{
		Debug.Log($"AbilitySwitcherSetting apply value {Value}");
	}

	public LocalizedString GetDisplayName() => new UnlocalizedString("Keybind");
	public string GetCategory() => "AbilitySwitcher";

	protected override KeyCode GetDefaultKey()
	{
		return KeyCode.O;
	}
}*/
