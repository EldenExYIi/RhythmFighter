using System.IO;
using UnityEngine;

public class RhythmManager : MonoBehaviour
{
    [Header("Song Config")]
    [SerializeField] private string songFolderName = "SongDemo";
    [SerializeField] private string songConfigFileName = "ConfigDemo.json";

    [Header("Chart Select")]
    [SerializeField] private int targetLevel = 1;

    public SongConfigData CurrentSongConfig { get; private set; }
    public ChartData CurrentChartData { get; private set; }

    private void Start()
    {
        LoadSong(songFolderName, targetLevel);
    }

    public bool LoadSong(string folderName, int level)
    {
        string songRootPath = Path.Combine(Application.dataPath, "Songs", folderName); //构建歌曲根目录路径，例如 "Assets/Songs/SongDemo"
        string configPath = Path.Combine(songRootPath, "Config", songConfigFileName); //构建歌曲配置文件路径，例如 "Assets/Songs/SongDemo/Config/ConfigDemo.json"

        Debug.Log($"RhythmManager: loading song config -> {configPath}");

        CurrentSongConfig = SongConfigLoader.LoadFromFile(configPath); //使用SongConfigLoader从文件加载歌曲配置数据
        if (CurrentSongConfig == null)
        {
            Debug.LogError("RhythmManager: failed to load SongConfigData.");
            return false;
        }

        SongChartEntryData chartEntry = FindChartEntryByLevel(CurrentSongConfig, level); //在歌曲配置中查找指定难度等级的谱面条目
        if (chartEntry == null)
        {
            Debug.LogError($"RhythmManager: chart level {level} not found in song config.");
            return false;
        }

        string chartPath = Path.Combine(songRootPath, chartEntry.file); //构建谱面文件路径，例如 "Assets/Songs/SongDemo/Charts/ChartDemo.json"

        Debug.Log($"RhythmManager: loading chart -> {chartPath}");

        CurrentChartData = ChartLoader.LoadFromFile(chartPath); //使用ChartLoader从文件加载谱面数据
        if (CurrentChartData == null)
        {
            Debug.LogError("RhythmManager: failed to load ChartData.");
            return false;
        }

        Debug.Log($"RhythmManager: load success. Song={CurrentSongConfig.songName}, Level={level}, Notes={CurrentChartData.notes?.Count ?? 0}");

        InitializeRhythmGameplay();

        return true;
    }

    private SongChartEntryData FindChartEntryByLevel(SongConfigData config, int level) //在歌曲配置数据中查找指定难度等级的谱面条目，如果找到则返回该条目，否则返回null
    {
        if (config == null || config.charts == null || config.charts.Count == 0)
        {
            Debug.LogError("RhythmManager: chart list is null or empty.");
            return null;
        }

        for (int i = 0; i < config.charts.Count; i++)
        {
            if (config.charts[i].level == level)
            {
                return config.charts[i];
            }
        }

        return null;
    }

    private void InitializeRhythmGameplay() //在成功加载歌曲配置和谱面数据后，初始化节奏游戏玩法，例如加载音频、设置节奏管理器、生成音符等
    {
        if (CurrentSongConfig == null || CurrentChartData == null)
        {
            Debug.LogError("RhythmManager: cannot initialize rhythm gameplay, data is missing.");
            return;
        }

        Debug.Log("RhythmManager: initialize rhythm gameplay.");
        Debug.Log($"Song Name: {CurrentSongConfig.songName}");
        Debug.Log($"Artist: {CurrentSongConfig.artist}");
        Debug.Log($"Audio Path: {CurrentSongConfig.audio}");
        Debug.Log($"Cover Path: {CurrentSongConfig.cover}");
        Debug.Log($"Chart Name: {CurrentChartData.meta.chartName}");
        Debug.Log($"Lane Count: {CurrentChartData.meta.laneNumber}");
        Debug.Log($"Offset(ms): {CurrentChartData.timing.offsetMs}");

        // 1. 加载音频
        // 2. 初始化 TimingManager
        // 3. 生成 Note
        // 4. 初始化判定系统
    }
}