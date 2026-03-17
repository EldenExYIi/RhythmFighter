using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class JudgeManager : MonoBehaviour
{
    public RhythmManager rhythmManager; // 引用 RhythmManager 获取节奏信息
    public InputManager inputManager; // 引用 InputManager 获取玩家输入事件
    
    [Header("Note Prefabs")]
    public JudgeEffectsPrefab judgeEffectsPrefab; // 判定效果预制体
    public Transform judgeLine; // 基于判定线位置生成判定效果
    public Transform judgeEffectsRoot; // 音符的父物体，便于管理生成的判定效果

    public enum JudgeResult
    {
        Perfect,
        Good,
        Miss
    }

    [Header("Judgement Windows (in seconds)")]
    public double perfectWindow = 0.1f;
    public double goodWindow = 0.5f;

    void Start()
    {

    }

    void Update()
    {
        if (!rhythmManager.isPlaying)
        {
            return;
        }
        while (inputManager.GetNextInputEvent(out var inputEvent))
        {
            if(inputEvent.eventType == InputManager.InputEventType.Down)
            {
                HandleKeyDown(inputEvent.lane, inputEvent.dspTime);
            }
        }
    }

    private void HandleKeyDown(int lane, double inputDspTime)
    {
        ChartNoteData closestNote = null;
        double closestTimeDiff = double.MaxValue;

        foreach (var note in rhythmManager.CurrentChartData.notes)
        {
            if (note.lane != lane || note.isJudged)
            {
                continue;
            }
            double noteHitTime = rhythmManager.BeatToSeconds(note.beat, rhythmManager.CurrentChartData.timing.bpms[0].bpm) + rhythmManager.CurrentChartData.timing.offsetMs / 1000.0; // 计算音符的目标击打时间，考虑全局偏移
            double timeDiff = Mathf.Abs((float)(inputDspTime - noteHitTime));
            if (timeDiff < closestTimeDiff)
            {
                closestTimeDiff = timeDiff;
                closestNote = note;
            }
        }

        if (closestNote != null)
        {
            JudgeResult result;
            if (closestTimeDiff <= perfectWindow)
            {
                result = JudgeResult.Perfect;
            }
            else if (closestTimeDiff <= goodWindow)
            {
                result = JudgeResult.Good;
            }
            else
            {
                result = JudgeResult.Miss;
            }

            //        NotePrefab noteView = Instantiate(notePrefab, spawnPosition, Quaternion.identity, notesRoot);
            Vector3 effectPosition = new Vector3(judgeLine.position.x - 3, judgeLine.position.y + 1, judgeLine.position.z + 3); // 可以根据需要调整判定效果的位置
            JudgeEffectsPrefab effectView = Instantiate(judgeEffectsPrefab, effectPosition, Quaternion.identity, judgeEffectsRoot);
            effectView.Initialize(result);
            Debug.Log($"JudgeManager: lane {lane} input at {inputDspTime:F2}s, closest note ID {closestNote.id} at beat {closestNote.beat.bar}:{closestNote.beat.numerator}/{closestNote.beat.denominator}, hit time diff: {closestTimeDiff:F2}s, result: {result}");
            closestNote.isJudged = true; // 标记该音符已经被判定，避免重复判定
        }
        else
        {
            Debug.Log($"JudgeManager: lane {lane} input at {inputDspTime:F2}s, no matching note found, result: Miss");
        }
    }
}
