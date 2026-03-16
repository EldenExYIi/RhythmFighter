using System.IO;
using UnityEngine;

public static class ChartLoader
{
    public static ChartData LoadFromJson(string json) //从JSON字符串加载ChartData对象
    {
        if (string.IsNullOrEmpty(json)) //检查输入的JSON字符串是否为null或空
        {
            Debug.LogError("ChartLoader: json is null or empty.");
            return null;
        }

        try
        {
            ChartData data = JsonUtility.FromJson<ChartData>(json);

            if (data == null) //检查解析后的ChartData对象是否为null，可能是因为JSON格式不正确或与ChartData结构不匹配
            {
                Debug.LogError("ChartLoader: failed to parse ChartData.");
                return null;
            }

            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ChartLoader: exception while parsing json.\n{e}");
            return null;
        }
    }

    public static ChartData LoadFromFile(string filePath) //从文件路径加载ChartData对象
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Debug.LogError("ChartLoader: filePath is null or empty."); //检查输入的文件路径是否为null、空或仅包含空白字符
            return null;
        }

        if (!File.Exists(filePath)) //检查指定路径的文件是否存在，如果不存在则输出错误日志并返回null
        {
            Debug.LogError($"ChartLoader: file not found -> {filePath}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            return LoadFromJson(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ChartLoader: exception while reading file.\n{e}");
            return null;
        }
    }
}
