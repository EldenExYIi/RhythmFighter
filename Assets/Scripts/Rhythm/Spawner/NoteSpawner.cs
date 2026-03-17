using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public RhythmManager rhythmManager;// 引用 RhythmManager 获取节奏信息
    
    [Header("Note Prefabs")]
    public NotePrefab tapNotePrefab;  // Tap Note 预制体
    public NotePrefab holdNotePrefab; // Hold Note 预制体，可以在后续添加更多类型的音符预制体
    public Transform[] lanes;         // 音符轨道的数组
    public float currentSongsBpm; // 歌曲的BPM，后续可以根据谱面中的BPM变化进行调整

    [Header("Position Settings")]
    public Transform lane_0;
    public Transform lane_1;
    public Transform lane_2;
    public Transform lane_3;
    public Transform spawnLine; // 音符生成线
    public Transform judgeLine; // 音符判定线
    public Transform notesRoot;  // 音符的父物体，便于管理生成的音符
    

    private void Awake()
    {
        lanes = new Transform[4]
        {
            lane_0,
            lane_1,
            lane_2,
            lane_3
        };
    }

    void Update()
    {
        if (!rhythmManager.isPlaying)
        {
            return;
        }
        foreach(var bpmsData in rhythmManager.CurrentChartData.timing.bpms)
        {
            if (bpmsData.beat.bar == 0 && bpmsData.beat.numerator == 0) // 在谱面开始时设置初始BPM
            {
                currentSongsBpm = bpmsData.bpm;
                break;
            }
            double bpmChangeDspTime = rhythmManager.BeatToSeconds(bpmsData.beat, currentSongsBpm) + rhythmManager.CurrentChartData.timing.offsetMs/1000.0; // 考虑全局偏移
            if (rhythmManager.currentDspTime >= bpmChangeDspTime)
            {
                currentSongsBpm = bpmsData.bpm;
                Debug.Log($"NoteSpawner: BPM changed to {currentSongsBpm} at beat {bpmsData.beat.bar}:{bpmsData.beat.numerator}/{bpmsData.beat.denominator}");
            }

        }
        foreach(var noteData in rhythmManager.CurrentChartData.notes)
        {
            double noteDspTime = rhythmManager.BeatToSeconds(noteData.beat, currentSongsBpm) + rhythmManager.CurrentChartData.timing.offsetMs/1000.0; // 考虑全局偏移
            if (noteDspTime <= rhythmManager.currentDspTime+rhythmManager.CurrentChartData.meta.spawnToHitTime && !noteData.isSpawned)
            {
                SpawnNote(noteData);
                noteData.isSpawned = true; // 标记该音符已生成，避免重复生成
            }
        }
    
    }
    private void SpawnNote(ChartNoteData noteData)
    {
        NotePrefab notePrefab = null;
        switch (noteData.type)
        {
            case "tap":
                notePrefab = tapNotePrefab;
                break;
            case "hold":
                notePrefab = holdNotePrefab;
                break;
            default:
                Debug.LogError($"NoteSpawner: unknown note type -> {noteData.type}");
                return;
        }

        if (notePrefab == null)
        {
            Debug.LogError("NoteSpawner: note prefab is not assigned.");
            return;
        }

        if (noteData.lane < 0 || noteData.lane >= lanes.Length)
        {
            Debug.LogError($"NoteSpawner: invalid lane index -> {noteData.lane}");
            return;
        }

        Transform laneTransform = lanes[noteData.lane];
        Vector3 spawnPosition = new Vector3(laneTransform.position.x, spawnLine.position.y, spawnLine.position.z);
        NotePrefab noteView = Instantiate(notePrefab, spawnPosition, Quaternion.identity, notesRoot);
        double aimedHitTime = rhythmManager.BeatToSeconds(noteData.beat, currentSongsBpm) + rhythmManager.CurrentChartData.timing.offsetMs/1000.0; // 计算音符的目标击打时间，考虑全局偏移
        noteView.Initialize(aimedHitTime); // 计算音符的目标击打时间，考虑全局偏移
        Debug.Log($"NoteSpawner: spawned note ID {noteData.id} at lane {noteData.lane} (beat: {noteData.beat.bar}:{noteData.beat.numerator}/{noteData.beat.denominator}),spawnTime:{rhythmManager.currentDspTime:F2}s,aimedHitTime:{aimedHitTime},chartTime:{rhythmManager.BeatToSeconds(noteData.beat, currentSongsBpm):F2}s");
    }
}
