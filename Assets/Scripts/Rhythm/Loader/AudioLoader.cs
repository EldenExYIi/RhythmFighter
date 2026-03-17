using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class RhythmAudioLoader
{
    public static IEnumerator LoadAudioClipRoutine(string fullAudioPath, System.Action<AudioClip> onLoaded)
    {
        if (string.IsNullOrWhiteSpace(fullAudioPath))
        {
            Debug.LogError("RhythmAudioLoader: fullAudioPath is null or empty.");
            onLoaded?.Invoke(null);
            yield break;
        }

        if (!File.Exists(fullAudioPath))
        {
            Debug.LogError($"RhythmAudioLoader: audio file not found -> {fullAudioPath}");
            onLoaded?.Invoke(null);
            yield break;
        }

        string fileUrl = "file://" + fullAudioPath.Replace("\\", "/");
        AudioType audioType = GetAudioTypeByExtension(fullAudioPath);

        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(fileUrl, audioType);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"RhythmAudioLoader: failed to load audio clip.\n{request.error}");
            onLoaded?.Invoke(null);
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
        if (clip == null)
        {
            Debug.LogError("RhythmAudioLoader: loaded audio clip is null.");
            onLoaded?.Invoke(null);
            yield break;
        }

        onLoaded?.Invoke(clip);
    }

    private static AudioType GetAudioTypeByExtension(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();

        switch (extension)
        {
            case ".ogg":
                return AudioType.OGGVORBIS;
            case ".wav":
                return AudioType.WAV;
            case ".mp3":
                return AudioType.MPEG;
            default:
                Debug.LogWarning($"RhythmAudioLoader: unknown audio extension {extension}, fallback to UNKNOWN.");
                return AudioType.UNKNOWN;
        }
    }
}