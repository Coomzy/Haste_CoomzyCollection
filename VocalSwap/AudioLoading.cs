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
	public static async Task<AudioClip> LoadAudioClipAsync(string filePath)
	{
		string url = GetFileUrl(filePath);
		AudioType type = GetAudioType(filePath);

		if (type == AudioType.UNKNOWN)
			throw new NotSupportedException($"Unsupported audio file type: {Path.GetExtension(filePath)}");

		using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, type);
		var operation = request.SendWebRequest();

		while (!operation.isDone)
			await Task.Yield(); // keep the async task non-blocking

		if (request.result != UnityWebRequest.Result.Success)
			throw new Exception($"Audio load failed: {request.error}");

		return DownloadHandlerAudioClip.GetContent(request);
	}

	public static string GetFileUrl(string path)
	{
		return path.StartsWith("file://") ? path : $"file://{path}";
	}

	public static AudioType GetAudioType(string path)
	{
		return Path.GetExtension(path).ToLowerInvariant() switch
		{
			".mp3" => AudioType.MPEG,
			".wav" => AudioType.WAV,
			".ogg" => AudioType.OGGVORBIS,
			_ => AudioType.UNKNOWN
		};
	}	
}