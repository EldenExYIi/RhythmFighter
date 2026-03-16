using System.IO;
using UnityEngine;

public static class SongConfigLoader
{
    public static SongConfigData LoadFromJson(string json) //从JSON字符串加载SongConfigData对象
    {
        if (string.IsNullOrEmpty(json)) //检查输入的JSON字符串是否为null或空
        {
            Debug.LogError("SongConfigLoader: json is null or empty.");
            return null;
        }

        try
        {
            SongConfigData data = JsonUtility.FromJson<SongConfigData>(json);

            if (data == null) //检查解析后的SongConfigData对象是否为null，可能是因为JSON格式不正确或与SongConfigData结构不匹配
            {
                Debug.LogError("SongConfigLoader: failed to parse SongConfigData.");
                return null;
            }

            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SongConfigLoader: exception while parsing json.\n{e}");
            return null;
        }
    }

    public static SongConfigData LoadFromFile(string filePath) //从文件路径加载SongConfigData对象
    {
        if (string.IsNullOrWhiteSpace(filePath)) //检查输入的文件路径是否为null、空或仅包含空白字符
        {
            Debug.LogError("SongConfigLoader: filePath is null or empty.");
            return null;
        }

        if (!File.Exists(filePath)) //检查指定路径的文件是否存在，如果不存在则输出错误日志并返回null
        {
            Debug.LogError($"SongConfigLoader: file not found -> {filePath}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            return LoadFromJson(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SongConfigLoader: exception while reading file.\n{e}");
            return null;
        }
    }

}