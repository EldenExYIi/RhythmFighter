using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class BulletLoader
{
    public static List<BulletGroupData> LoadFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("BulletLoader: json is null or empty.");
            return null;
        }

        try
        {
            BulletChartData file = JsonUtility.FromJson<BulletChartData>(json);
            if (file == null || file.bulletGroups == null)
            {
                Debug.LogError("BulletLoader: Failed to parse bullet chart.");
                return null;
            }
            return file.bulletGroups;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BulletLoader: Exception while parsing json.\n{e}");
            return null;
        }
    }

    public static List<BulletGroupData> LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"BulletLoader: File not found -> {filePath}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            return LoadFromJson(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BulletLoader: Exception while reading file.\n{e}");
            return null;
        }
    }
}