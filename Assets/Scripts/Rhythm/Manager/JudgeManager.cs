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
            if (inputEvent.eventType == InputManager.InputEventType.Down)
            {
                HandleKeyDown(inputEvent.lane, inputEvent.dspTime);
            }
            if (inputEvent.eventType == InputManager.InputEventType.Up)
            {
                HandleKeyUp(inputEvent.lane, inputEvent.dspTime);
            }

        }
    }

    private void HandleKeyDown(int lane, double inputDspTime)
    {
        ChartNoteData closestNote = null;
        double closestTimeDiff = double.MaxValue;
        bool isEarly = false; // 标记玩家输入是早于还是晚于目标击打时间，true表示早于，false表示晚于
        foreach (var note in rhythmManager.CurrentChartData.notes)
        {
            if (note.lane != lane || note.isJudged)
            {
                continue;
            }
            double noteHitTime = rhythmManager.BeatToSeconds(note.beat, rhythmManager.currentSongsBpm) + rhythmManager.CurrentChartData.timing.offsetMs / 1000.0; // 计算音符的目标击打时间，考虑全局偏移
            double timeDiff = Mathf.Abs((float)(inputDspTime - noteHitTime));
            if (timeDiff < closestTimeDiff)
            {
                closestTimeDiff = timeDiff;
                closestNote = note;
                isEarly = inputDspTime < noteHitTime; // 如果玩家输入的DSP时间小于目标击打时间，说明玩家输入早于目标击打时间，反之则晚于目标击打时间
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
                if (isEarly) return;
                result = JudgeResult.Miss;
            }

            ShowJudgeEffect(result);
            Debug.Log($"JudgeManager: lane {lane} input at {inputDspTime:F2}s, closest note ID {closestNote.id} at beat {closestNote.beat.bar}:{closestNote.beat.numerator}/{closestNote.beat.denominator}, hit time diff: {closestTimeDiff:F2}s, result: {result}");

            if (closestNote.type == "hold" && result != JudgeResult.Miss) // 如果是长按音符且没有Miss，根据玩家输入的时间与目标击打时间的关系来更新长按状态，便于在HoldNotePrefab中显示不同的判定效果
            {
                closestNote.isHolding = true;
                return; // 长按音符在按键按下时不立即判定为Miss，先更新长按状态，等到按键松开时再根据玩家是否持续按住来判定最终的结果
            }
            closestNote.isJudged = true; // 标记该音符已经被判定，避免重复判定
        }
        else
        {
            Debug.Log($"JudgeManager: lane {lane} input at {inputDspTime:F2}s, no matching note found, result: Miss");
        }
    }
    private void HandleKeyUp(int lane, double inputDspTime)
    {
        ChartNoteData closestNote = null;
        double closestTimeDiff = double.MaxValue;
        bool isEarly = false; // 标记玩家输入是早于还是晚于目标击打时间，true表示早于，false表示晚于
        foreach (var note in rhythmManager.CurrentChartData.notes)
        {
            if (note.lane != lane || note.isJudged || note.type != "hold" || !note.isHolding) // 只有当音符类型为"hold"且未被判定时才考虑更新长按状态
            {
                continue;
            }
            double noteHitTime = rhythmManager.BeatToSeconds(note.endBeat, rhythmManager.currentSongsBpm) + rhythmManager.CurrentChartData.timing.offsetMs / 1000.0; // 计算音符的目标击打时间，考虑全局偏移
            double timeDiff = Mathf.Abs((float)(inputDspTime - noteHitTime));
            if (timeDiff < closestTimeDiff)
            {
                closestTimeDiff = timeDiff;
                closestNote = note;
                isEarly = inputDspTime < noteHitTime; // 如果玩家输入的DSP时间小于目标击打时间，说明玩家输入早于目标击打时间，反之则晚于目标击打时间
            }
        }
        if (closestNote != null)
        {
            JudgeResult result;
            if (closestTimeDiff <= goodWindow)
            {
                result = JudgeResult.Perfect; // 长按音符的最终判定结果只有Perfect和Miss两种，如果玩家松键的时间在Good窗口内，说明玩家成功持续按住了整个长按音符，判定为Perfect
            }
            else if (isEarly)
                result = JudgeResult.Miss; // 如果玩家松键的时间早于长按音符的结束时间，说明玩家没有持续按住整个长按音符，判定为Miss
            else
                result = JudgeResult.Perfect; // 如果玩家松键的时间晚于或等于长按音符的结束时间，说明玩家成功持续按住了整个长按音符，判定为Perfect

            ShowJudgeEffect(result);
            Debug.Log($"JudgeManager: lane {lane} input at {inputDspTime:F2}s, closest note ID {closestNote.id} at beat {closestNote.beat.bar}:{closestNote.beat.numerator}/{closestNote.beat.denominator}, hit time diff: {closestTimeDiff:F2}s, result: {result}");
            closestNote.isJudged = true; // 标记该音符已经被判定，避免重复判定
        }
        else
        {
            Debug.Log($"JudgeManager: lane {lane} input at {inputDspTime:F2}s, no matching note found, result: Miss");
        }
    }
    public void ShowJudgeEffect(JudgeResult result)
    {
        Vector3 effectPosition = new Vector3(judgeLine.position.x - 3, judgeLine.position.y + 1, judgeLine.position.z + 3); // 可以根据需要调整判定效果的位置
        JudgeEffectsPrefab effectView = Instantiate(judgeEffectsPrefab, effectPosition, Quaternion.identity, judgeEffectsRoot);
        effectView.Initialize(result);
    }
}
