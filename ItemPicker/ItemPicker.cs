using Landfall.Haste;
using Landfall.Modding;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UIElements;
using Zorro.ControllerSupport;
using Zorro.Core;
using Zorro.Settings;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace ItemPicker;

[LandfallPlugin]
public class Program
{
    static Program()
	{
		var go = new GameObject(nameof(ItemPickerUpdater));
		UnityEngine.Object.DontDestroyOnLoad(go);
		go.AddComponent<ItemPickerUpdater>();
	}
}

public class ItemPickerUpdater : MonoBehaviour
{
	public static List<ItemInstance> items = new List<ItemInstance>();
	private static readonly string savePath = Path.Combine(Application.persistentDataPath, "Mods/ItemPicker/SavedItems.txt");

	public static InputAction addSelectedItemAction = new InputAction("addSelectedItem", InputActionType.Button);
	public static InputAction resetItemsAction = new InputAction("resetItemsAction", InputActionType.Button);

	public void Awake()
	{
		StartCoroutine(WaitForLoad());
	}

	IEnumerator WaitForLoad()
	{
		while (GameHandler.Instance?.SettingsHandler == null)
		{
			yield return null;
		}

		yield return null;

		var setting = GameHandler.Instance.SettingsHandler.GetSetting<ResetItemsTypeSetting>();

		if (setting == null)
			yield break;

		if (setting.Value != ResetItemsType.Never)
			yield break;

		while (ItemDatabase.instance?.items == null && ItemDatabase.instance.items.Count > 0)
		{
			yield return null;
		}

		LoadItems();
	}

	// This does work
	public IEnumerator Start()
	{
		while (GameHandler.Instance?.SettingsHandler == null)
		{
			yield return null;
		}

		SetupKeybinds();
	}

	public void OnEnable()
	{
		GM_API.SpawnedInHub += SpawnedInHub;
		GM_API.StartNewRun += StartNewRun;

		// This fails the first time, that's what the Start() coroutine is for.. I'm not even sure that works
		if (GameHandler.Instance?.SettingsHandler == null)
			return;

		SetupKeybinds();
	}

	public void OnDisable()
	{
		GM_API.SpawnedInHub -= SpawnedInHub;
		GM_API.StartNewRun -= StartNewRun;
	}

	void SpawnedInHub()
	{
		/*if (!GM_Hub.isInHub)
			return;*/

		//ApplyItems();

		if (Player.localPlayer.character.restartAction == null ||
			!Player.localPlayer.character.restartAction.GetInvocationList().Contains((Action)PlayerRestart))
		{
			Player.localPlayer.character.restartAction += PlayerRestart;
		}
	}

	void StartNewRun()
	{
		ApplyItems();

		if (GameHandler.Instance?.SettingsHandler == null)
			return;

		var setting = GameHandler.Instance.SettingsHandler.GetSetting<ResetItemsTypeSetting>();

		if (setting == null)
			return;

		if (setting.Value != ResetItemsType.ShardRun)
			return;

		items.Clear();
	}

	int lastFrameRestarted = -1;
	void PlayerRestart()
	{
		if (Time.frameCount == lastFrameRestarted)
			return;

		lastFrameRestarted = Time.frameCount;

		ApplyItems();
	}

	public void Update()
	{
		if (!UI_UnlockedItemsScreen.IsOpen)
			return;

		if (resetItemsAction.WasPressedThisFrame())
		{
			RemoveAllItems();
		}

		if (!addSelectedItemAction.WasPressedThisFrame())
			return;

		UI_ItemIcon selectedIcon = null;
		foreach (var iconGO in UI_UnlockedItemsScreen.Instance.icons)
		{
			var itemIcon = iconGO.GetComponentInChildren<UI_ItemIcon>();

			var isHovered = itemIcon.GetProperty<bool>("isHovered");
			if (!isHovered)
				continue;

			selectedIcon = itemIcon;
			break;
		}

		if (selectedIcon == null)
		{
			return;
		}

		var selectedItem = selectedIcon.itemRef;
		if (selectedItem == null)
		{
			return;
		}

		items.Add(selectedItem);
		ApplyItem(selectedItem);
		SaveItems();
	}

	//[Conditional("DEBUG")]
	/*void OnGUI()
	{
		GUILayout.BeginArea(new Rect(10, 10, 400, Screen.height - 20));

		if (items == null)
		{
			GUILayout.Label("items is null");
			GUILayout.EndArea();
			return;
		}

		GUILayout.Label($"items count: {items.Count}");
		for (int i = 0; i < items.Count; i++)
		{
			var item = items[i];
			GUILayout.Label($"items[{i}]: {item.itemName}");
		}

		//GUILayout.Label($"lastFailCase: {lastFailCase}");

		GUILayout.EndArea();
	}*/


	public static void ApplyItems()
	{
		foreach (var item in items)
		{
			if (item == null)
				continue;
			ApplyItem(item);
		}
	}

	public static void ApplyItem(ItemInstance itemInstance)
	{
		if (itemInstance == null)
		{
			return;
		}

		Player.localPlayer.Call("AddItem", itemInstance);
		Player.localPlayer.Call("BoughtItem", itemInstance);
	}

	public static void RemoveAllItems()
	{
		Player.localPlayer.ResetPlayer();
		items.Clear();
		SaveItems();
	}

	public static void SaveItems()
	{
		Directory.CreateDirectory(Path.GetDirectoryName(savePath));

		var lines = items
			.Where(item => item != null)
			.Select(item => item.itemName)
			.ToArray();

		File.WriteAllLines(savePath, lines);
		//Debug.Log($"Saved {lines.Length} items to {savePath}");
	}

	public static void LoadItems()
	{
		if (!File.Exists(savePath))
		{
			//Debug.LogWarning("Save file not found: " + savePath);
			return;
		}

		var names = File.ReadAllLines(savePath);

		foreach (var name in names)
		{
			var match = ItemDatabase.instance.items.FirstOrDefault(item => item.itemName == name);
			if (match != null)
			{
				items.Add(match);
			}
			else
			{
				Debug.LogWarning($"[ItemPicker] Item not found in database: {name}");
			}
		}

		//Debug.Log($"Loaded {items.Count} items from {savePath}");
	}

	public static void SetupKeybinds()
	{
		if (GameHandler.Instance?.SettingsHandler == null)
		{
			Debug.LogError($"NO! You tried to call SetupKeybinds() before SettingsHandler was ready");
			return;
		}

		addSelectedItemAction.RemoveAllBindingOverrides();
		addSelectedItemAction.AddBinding(GetKeybindString<KeybindSetting_AddSelectedItem_Keyboard>());
		addSelectedItemAction.AddBinding(GetKeybindString<KeybindSetting_AddSelectedItem_Gamepad>());
		addSelectedItemAction.Enable();

		resetItemsAction.RemoveAllBindingOverrides();
		resetItemsAction.AddBinding(GetKeybindString<KeybindSetting_ResetItems_Keyboard>());
		resetItemsAction.AddBinding(GetKeybindString<KeybindSetting_ResetItems_Gamepad>());
		resetItemsAction.Enable();
	}

	public static string GetKeybindString<T>() where T : StringSetting
	{
		var setting = GameHandler.Instance?.SettingsHandler.GetSetting<T>();

		if (setting == null)
			return "";

		return setting.Value;
	}
}

public enum ResetItemsType
{
	Never,
	Session,
	ShardRun
}

[HasteSetting]
public class ResetItemsTypeSetting : EnumSetting<ResetItemsType>, IExposedSetting
{
	public override void ApplyValue()
	{
		//Debug.Log($"UnlimitedEnergyTypeSetting apply value {Value}");
	}

	protected override ResetItemsType GetDefaultValue() => ResetItemsType.Never;

	public override List<LocalizedString> GetLocalizedChoices() =>
	[
		new UnlocalizedString("Never"),
		new UnlocalizedString("Session"),
		new UnlocalizedString("Shard Run")
	];

	public LocalizedString GetDisplayName() => new UnlocalizedString("Item Reset Type");
	public string GetCategory() => "Item Picker";
}

[HasteSetting]
public class ResetItemsSetting : ButtonSetting, IExposedSetting
{
	public override void ApplyValue()
	{
		//Debug.Log($"UnlimitedEnergyTypeSetting apply value {Value}");
	}

	public LocalizedString GetDisplayName() => new UnlocalizedString("Item Picker Items");
	public string GetCategory() => "Item Picker";

	public override void OnClicked(ISettingHandler settingHandler)
	{
		ItemPickerUpdater.RemoveAllItems();
	}

	public override string GetButtonText() => "Reset";
}

[HasteSetting]
public class KeybindSetting_AddSelectedItem_Keyboard : StringSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("AddSelectedItem - Keyboard");
	public string GetCategory() => "Item Picker";
	public override void ApplyValue() => ItemPickerUpdater.SetupKeybinds();
	protected override string GetDefaultValue() => "/Keyboard/space";
}

[HasteSetting]
public class KeybindSetting_AddSelectedItem_Gamepad : StringSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("AddSelectedItem - Gamepad");
	public string GetCategory() => "Item Picker";
	public override void ApplyValue() => ItemPickerUpdater.SetupKeybinds();
	protected override string GetDefaultValue() => "<Gamepad>/rightTrigger";
}

[HasteSetting]
public class KeybindSetting_ResetItems_Keyboard : StringSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("ResetItems - Keyboard");
	public string GetCategory() => "Item Picker";
	public override void ApplyValue() => ItemPickerUpdater.SetupKeybinds();
	protected override string GetDefaultValue() => "/Mouse/rightButton";
}

[HasteSetting]
public class KeybindSetting_ResetItems_Gamepad : StringSetting, IExposedSetting
{
	public LocalizedString GetDisplayName() => new UnlocalizedString("ResetItems - Gamepad");
	public string GetCategory() => "Item Picker";
	public override void ApplyValue() => ItemPickerUpdater.SetupKeybinds();
	protected override string GetDefaultValue() => "<Gamepad>/leftTrigger";
}