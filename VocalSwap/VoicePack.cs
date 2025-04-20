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

public static partial class Utility
{
	public static void ApplyVoicePack(VoicePackConfig config)
	{
		Debug.Log($"[VocalSwap] Applying voice pack: {config.characterName} at {config.rootPath}");

		selectedVoicePack = config.characterName;

		File.WriteAllText(selectedCharacterSavePath, selectedVoicePack);

		//foreach (PlayerVocalSFX.SoundEffectType effectType in Enum.GetValues(typeof(PlayerVocalSFX.SoundEffectType)))
		foreach (var soundKey in soundKeys)
		{
			var sfx_instance_active = GetVocalInstance(soundKey, activeVocalBank);
			if (sfx_instance_active == null)
			{
				Debug.LogError($"[VocalSwap] Failed to get sfx instance for {soundKey}");
				continue;
			}

			if (config.typeToClips.TryGetValue(soundKey, out AudioClip[] clips))
			{
				sfx_instance_active.clips = clips;
				continue;
			}

			bool isSilentOverride = config.silentEntries.Contains(soundKey);
			if (isSilentOverride)
			{
				sfx_instance_active.clips = emptyClips;
				continue;
			}

			var sfx_instance_zoe = GetVocalInstance(soundKey, zoeVocalBank);
			if (sfx_instance_zoe == null)
			{
				Debug.LogError($"[VocalSwap] Failed to get sfx instance for zoe {soundKey}");
				sfx_instance_active.clips = emptyClips;
				continue;
			}
			sfx_instance_active.clips = sfx_instance_zoe.clips;
		}
	}

	public static async Task LoadAllVoicePacks()
	{
		voicePacks.Clear();

		var zoeVoicePack = new VoicePackConfig();
		zoeVoicePack.characterName = "Zoe";
		zoeVoicePack.silentEntries = new string[0];
		zoeVoicePack.typeToClips = new();
		voicePacks.Add(zoeVoicePack);

		var paths = new List<string>();
		var localPaths = FindAllVoicePackPathsForFolder(localCharactersPath);		
		foreach (var loadedModDirectory in Modloader.LoadedItemDirectories)
		{
			//Debug.Log($"[VocalSwap] Mod '{loadedModDirectory.Key.ToString()}' path: {loadedModDirectory.Value.directory}");
			var workshopPaths = FindAllVoicePackPathsForFolder(loadedModDirectory.Value.directory);
			if (workshopPaths.Count > 0)
			{
				paths.AddRange(workshopPaths);
			}
		}

		if (localPaths.Count > 0)
		{
			paths.AddRange(localPaths);
		}

		foreach (var path in paths)
		{
			var config = await LoadVoicePackConfig(path);
			if (config == null)
			{
				Debug.LogError($"[VocalSwap] Failed to load config at path: {path}");
				continue;
			}

			voicePacks.Add(config);
		}
	}

	public static async Task<VoicePackConfig?> LoadVoicePackConfig(string path)
	{
		if (!File.Exists(path))
		{
			//Debug.LogWarning($"[VoicePack] Config not found at {path}, using defaults.");
			return null;
		}

		string json = File.ReadAllText(path);
		var config = JsonUtility.FromJson<VoicePackConfig>(json);
		if (config == null)
		{
			return null;
		}

		config.rootPath = Path.GetDirectoryName(path);
		config.typeToClips = new Dictionary<string, AudioClip[]>(StringComparer.OrdinalIgnoreCase);

		List<AudioClip> loadedClips = new List<AudioClip>();

		//foreach (PlayerVocalSFX.SoundEffectType effectType in Enum.GetValues(typeof(PlayerVocalSFX.SoundEffectType)))
		foreach (var soundKey in soundKeys)
		{
			loadedClips.Clear();

			string folderPath = Path.Combine(config.rootPath, soundKey);

			if (!Directory.Exists(folderPath))
			{
				continue;
			}

			string[] audioFiles = Directory.GetFiles(folderPath)
				.Where(f => f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
							f.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
				.ToArray();

			if (audioFiles.Length == 0)
			{
				continue;
			}

			var clips = new AudioClip[audioFiles.Length];

			foreach (var audioFile in audioFiles)
			{
				try
				{
					var clip = await LoadAudioClipAsync(audioFile);
					if (clip != null)
					{
						//Debug.LogWarning($"[VocalSwap] Clip loaded: {clip.name}, loadType: {clip.loadState}, loadInBackground: {clip.loadState}, length: {clip.length}, samples: {clip.samples}");

						loadedClips.Add(clip);
					}
					else
					{
						//Debug.LogWarning($"[VocalSwap] Failed to load clip: {clip.name}, loadType: {clip.loadState}, loadInBackground: {clip.loadState}, length: {clip.length}, samples: {clip.samples}");
						Debug.LogError($"[VocalSwap] Failed to load audio clip :(");
					}

				}
				catch (Exception ex)
				{
					Debug.LogError($"[VocalSwap] Failed to load audio: {ex.Message}");
				}
			}

			config.typeToClips[soundKey] = loadedClips.ToArray();
		}

		return config;
	}

	public static List<string> FindAllVoicePackPathsForFolder(string rootFolder)
	{
		var results = new List<string>();

		if (!Directory.Exists(rootFolder))
		{
			Debug.LogWarning($"[VocalSwap] Root folder does not exist: {rootFolder}");
			return results;
		}

		string[] configPaths = Directory.GetFiles(rootFolder, "VocalSwap.json", SearchOption.AllDirectories);

		if (configPaths != null)
		{
			results.AddRange(configPaths);
		}

		return results;
	}
}