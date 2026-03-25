using System.Collections;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RhythmManager : MonoBehaviour
{
    [Header("Song Config")]
    [SerializeField] private string songFolderName = "SongDemo";
    [SerializeField] private string songConfigFileName = "ConfigDemo.json";

    [Header("Chart Select")]
    [SerializeField] private int targetLevel = 1;

    public SongConfigData CurrentSongConfig { get; private set; }
    public ChartData CurrentChartData { get; private set; }

    [Header("Audio")]
    [SerializeField] private bool playOnLoad = true;
    public AudioClip CurrentAudioClip { get; private set; }
    private AudioSource audioSource;
    public bool isPlaying { get; private set;}

    [Header("Timing")]
    public double musicStartDspTime; // 音乐开始播放的DSP时间，用于与TimingManager同步
    public double currentDspTime => isPlaying ? AudioSettings.dspTime - musicStartDspTime : 0.0; // 当前的DSP时间
    public double spawnToHitTime; // 音符从生成到需要被击打的时间，单位为秒，用于计算音符生成时机和位置
    public double prepareTime=2.0; // 在音乐开始前提前生成音符的时间，单位为秒，确保玩家有足够的时间看到音符并做出反应

    [Header("Speed")]
    public float currentSongsBpm; // 歌曲的BPM，后续可以根据谱面中的BPM变化进行调整
    public float currentSongsSpeed; // 歌曲的速度，从谱面数据中获取

    [Header("Position Settings")]
    public Transform spawnLine; // 音符生成线
    public Transform judgeLine; // 音符判定线
    public float moveDistance; // 音符从生成线移动到判定线的距离，用于计算spawnToHitTime

    [Header("Rhythm Line")]
    public RhythmLinePrefab rhythmLinePrefab; // 引用RhythmLinePrefab预制体，用于实例化音符
    public Transform rhythmLinesRoot; // 音符的父物体，便于管理生成的音符
    private int currentBar; // 当前小节数，用于生成节奏线
    // private int currentBeatInBar; // 当前小节内的节拍数，用于生成节奏线

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        moveDistance = Mathf.Abs(spawnLine.position.z - judgeLine.position.z);
    }
    private void Start()
    {
        StartCoroutine(LoadSongRoutine(songFolderName, targetLevel));
    }
    private void Update()
    {
        foreach(var bpmsData in CurrentChartData.timing.bpms)
        {
            if (bpmsData.beat.bar == 0 && bpmsData.beat.numerator == 0) // 在谱面开始时设置初始BPM
            {
                currentSongsBpm = bpmsData.bpm;
                break;
            }
            double bpmChangeDspTime = BeatToSeconds(bpmsData.beat, currentSongsBpm) + CurrentChartData.timing.offsetMs/1000.0; // 考虑全局偏移
            if (currentDspTime >= bpmChangeDspTime && !bpmsData.isChanged) // 根据当前DSP时间进行BPM变化处理，确保在BPM变化发生时及时更新节奏管理器的BPM值
            {
                currentSongsBpm = bpmsData.bpm;
                bpmsData.isChanged = true; // 标记该BPM变化已被处理，避免重复处理同一个BPM变化
                Debug.Log($"NoteSpawner: BPM changed to {currentSongsBpm} at beat {bpmsData.beat.bar}:{bpmsData.beat.numerator}/{bpmsData.beat.denominator}");
            }

        }
        foreach(var speedChangeData in CurrentChartData.timing.speedChanges)
        {
            double speedChangeDspTime = BeatToSeconds(speedChangeData.beat, currentSongsBpm) + CurrentChartData.timing.offsetMs/1000.0; // 考虑全局偏移
            if (currentDspTime >= speedChangeDspTime && !speedChangeData.isChanged) // 根据当前DSP时间进行速度变化处理，确保在速度变化发生时及时更新节奏管理器的速度值
            {
                currentSongsSpeed = speedChangeData.speedRate * CurrentChartData.meta.speed; // 将速度倍率应用到谱面基础速度上
                spawnToHitTime = moveDistance / currentSongsSpeed; // 根据新的速度重新计算spawnToHitTime
                speedChangeData.isChanged = true; // 标记该速度变化已被处理，避免重复处理同一个速度变化
                Debug.Log($"NoteSpawner: Speed changed to {currentSongsSpeed} at beat {speedChangeData.beat.bar}:{speedChangeData.beat.numerator}/{speedChangeData.beat.denominator}");
            }
        }
        SpawnRhythmLine(); // 在每帧更新中调用SpawnRhythmLine方法，根据当前DSP时间生成节奏线，确保节奏线在正确的时间出现在生成线上
    }

    public IEnumerator LoadSongRoutine(string folderName, int level)
    {
        string songRootPath = Path.Combine(Application.dataPath, "Songs", folderName); //构建歌曲根目录路径，例如 "Assets/Songs/SongDemo"
        string configPath = Path.Combine(songRootPath, "Config", songConfigFileName); //构建歌曲配置文件路径，例如 "Assets/Songs/SongDemo/Config/ConfigDemo.json"

        // Debug.Log($"RhythmManager: loading song config -> {configPath}");

        CurrentSongConfig = SongConfigLoader.LoadFromFile(configPath); //使用SongConfigLoader从文件加载歌曲配置数据
        if (CurrentSongConfig == null)
        {
            Debug.LogError("RhythmManager: failed to load SongConfigData.");
            yield break;
        }

        SongChartEntryData chartEntry = FindChartEntryByLevel(CurrentSongConfig, level); //在歌曲配置中查找指定难度等级的谱面条目
        if (chartEntry == null)
        {
            Debug.LogError($"RhythmManager: chart level {level} not found in song config.");
            yield break;
        }

        string chartPath = Path.Combine(songRootPath, chartEntry.file); //构建谱面文件路径，例如 "Assets/Songs/SongDemo/Charts/ChartDemo.json"

        // Debug.Log($"RhythmManager: loading chart -> {chartPath}");

        CurrentChartData = ChartLoader.LoadFromFile(chartPath); //使用ChartLoader从文件加载谱面数据
        if (CurrentChartData == null)
        {
            Debug.LogError("RhythmManager: failed to load ChartData.");
            yield break;
        }

        string fullAudioPath = Path.Combine(songRootPath, CurrentSongConfig.audio);
        yield return StartCoroutine(RhythmAudioLoader.LoadAudioClipRoutine(fullAudioPath, (clip) => CurrentAudioClip = clip)); //使用RhythmAudioLoader的协程方法异步加载音频剪辑，加载完成后将结果赋值给CurrentAudioClip属性

        if (CurrentAudioClip == null)
        {
            Debug.LogError("RhythmManager: failed to load audio clip.");
            yield break;
        }

        audioSource.clip = CurrentAudioClip; //将加载的音频剪辑设置到AudioSource组件上
        spawnToHitTime = moveDistance / CurrentChartData.meta.speed; // 从谱面数据中获取spawnToHitTime，确保与谱面设置一致

        // Debug.Log($"RhythmManager: load success. Song={CurrentSongConfig.songName}, Level={level}, Notes={CurrentChartData.notes?.Count ?? 0}");

        InitializeRhythmGameplay();

        if (playOnLoad)
        {
            PlayMusic();
        }

        yield break;
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

        currentBar = 0; // 当前小节数，用于生成节奏线
        // currentBeatInBar = 0; // 当前小节内的节拍数，用于生成节奏线

        // Debug.Log("RhythmManager: initialize rhythm gameplay.");
        // Debug.Log($"Song Name: {CurrentSongConfig.songName}");
        // Debug.Log($"Artist: {CurrentSongConfig.artist}");
        // Debug.Log($"Audio Path: {CurrentSongConfig.audio}");
        // Debug.Log($"Cover Path: {CurrentSongConfig.cover}");
        // Debug.Log($"Chart Name: {CurrentChartData.meta.chartName}");
        // Debug.Log($"Lane Count: {CurrentChartData.meta.laneNumber}");
        // Debug.Log($"Offset(ms): {CurrentChartData.timing.offsetMs}");

    }

    public void PlayMusic()
    {
        if (audioSource == null)
        {
            Debug.LogError("RhythmManager: AudioSource is missing.");
            return;
        }

        if (audioSource.clip == null)
        {
            Debug.LogError("RhythmManager: AudioSource clip is null.");
            return;
        }

        musicStartDspTime = AudioSettings.dspTime; //记录音乐开始播放的DSP时间，用于与TimingManager同步
        isPlaying = true;
        audioSource.Play();
        Debug.Log($"RhythmManager: music started at DSP time {musicStartDspTime}");
    }

    public void SpawnRhythmLine()
    {
        //每拍开始时生成一条线
        //double currentBeatTime = BeatToSeconds(new BeatData { bar = currentBar, numerator = currentBeatInBar, denominator = CurrentChartData.meta.beatsPerBar }, currentSongsBpm) + CurrentChartData.timing.offsetMs/1000.0; //计算当前节奏线对应的DSP时间，考虑全局偏移
        // if(currentDspTime >= currentBeatTime - spawnToHitTime && currentDspTime < currentBeatTime) //在当前DSP时间接近节奏线的DSP时间时生成节奏线，确保节奏线在正确的时间出现在生成线上
        // {
        //     RhythmLinePrefab newLine = Instantiate(rhythmLinePrefab, new Vector3(spawnLine.position.x, rhythmLinesRoot.position.y, spawnLine.position.z), Quaternion.identity, rhythmLinesRoot); //实例化一个新的节奏线预制体，设置其位置为生成线的位置，父物体为rhythmLinesRoot
        //     newLine.Initialize(currentBeatTime, this, spawnLine, judgeLine); //调用节奏线预制体的Initialize方法，传入当前节奏线的DSP时间、节奏管理器实例、生成线和判定线的Transform引用
        //     currentBeatInBar++; //更新当前小节内的节拍数
        //     if(currentBeatInBar >= CurrentChartData.meta.beatsPerBar) //如果当前小节内的节拍数超过每小节的节拍数，说明进入了下一个小节
        //     {
        //         currentBeatInBar = 0; //重置当前小节内的节拍数
        //         currentBar++; //更新当前小节数
        //     }
        // }

        //每小节开始时生成一条线
        double currentBarTime = BeatToSeconds(new BeatData { bar = currentBar, numerator = 0, denominator = CurrentChartData.meta.beatsPerBar }, currentSongsBpm) + CurrentChartData.timing.offsetMs/1000.0; //计算当前小节开始时的DSP时间，考虑全局偏移
        if(currentDspTime >= currentBarTime - spawnToHitTime) //在当前DSP时间接近小节开始的DSP时间时生成节奏线，确保节奏线在正确的时间出现在生成线上
        {
            RhythmLinePrefab newLine = Instantiate(rhythmLinePrefab, new Vector3(spawnLine.position.x, rhythmLinesRoot.position.y, spawnLine.position.z), Quaternion.identity, rhythmLinesRoot); //实例化一个新的节奏线预制体，设置其位置为生成线的位置，父物体为rhythmLinesRoot
            newLine.Initialize(currentBarTime, this, spawnLine, judgeLine); //调用节奏线预制体的Initialize方法，传入当前小节开始的DSP时间、节奏管理器实例、生成线和判定线的Transform引用
            currentBar++; //更新当前小节数
        }
    }

    public void StopMusic()
    {
        if (audioSource == null)
        {
            return;
        }
        isPlaying = false;
        audioSource.Stop();
        // Debug.Log("RhythmManager: music stopped.");
    }

    public void PauseMusic()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.Pause();
        // Debug.Log("RhythmManager: music paused.");
    }
    public double BeatToSeconds(BeatData beat, double bpm) //将谱面中的节拍数据转换为对应的时间（秒）
    {
        if (CurrentChartData == null || CurrentChartData.meta.beatsPerBar <= 0)
        {
            Debug.LogError("RhythmManager: invalid chart data for beat to seconds conversion.");
            return 0.0;
        }

        double beatsPerSecond = bpm / 60.0; //计算每秒钟的节拍数，例如120 BPM表示每秒钟2拍
        double totalBeats = (beat.bar + (double)beat.numerator / beat.denominator) * CurrentChartData.meta.beatsPerBar; //计算总节拍数，例如在4/4拍中，1.5小节表示6拍
        double seconds = totalBeats / beatsPerSecond; //将总节拍数除以每秒钟的节拍数得到对应的时间（秒）

        return seconds;
    }

    public BeatData SecondsToBeat(double seconds, double bpm) //将时间（秒）转换为谱面中的节拍数据
    {
        if (CurrentChartData == null || CurrentChartData.meta.beatsPerBar <= 0)
        {
            Debug.LogError("RhythmManager: invalid chart data for seconds to beat conversion.");
            return new BeatData();
        }

        double beatsPerSecond = bpm / 60.0; //计算每秒钟的节拍数，例如120 BPM表示每秒钟2拍
        double totalBeats = seconds * beatsPerSecond; //计算总节拍数
        int bar = (int)(totalBeats / CurrentChartData.meta.beatsPerBar); //计算小节数
        double beatInBar = totalBeats % CurrentChartData.meta.beatsPerBar; //计算当前小节内的节拍数
        int numerator = (int)(beatInBar * 1000); //将小节内的节拍数转换为分子，乘以1000是为了保留小数部分
        int denominator = 1000; //分母固定为1000

        return new BeatData { bar = bar, numerator = numerator, denominator = denominator };
    }
    public double DistanceToTime(float distance) //将距离转换为时间，单位为秒
    {
        if (currentSongsSpeed <= 0)
        {
            Debug.LogError("RhythmManager: invalid speed for distance to time conversion.");
            return 0.0f;
        }

        return distance / currentSongsSpeed; //将距离除以谱面设置的速度得到对应的时间（秒）
    }
    public float TimeToDistance(double time) //将时间转换为距离，单位为单位长度
    {
        if (currentSongsSpeed <= 0)
        {
            Debug.LogError("RhythmManager: invalid speed for time to distance conversion.");
            return 0.0f;
        }

        return (float)(time * currentSongsSpeed); //将时间乘以谱面设置的速度得到对应的距离
    }
}
