using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldNotePrefab : NotePrefab
{
    double lastTime; // 长按音符的持续时间
    float noteLength; // 长按音符的长度，单位为单位长度，根据谱面设置的速度和长按持续时间计算得到
    
    [Header("Part Settings")]
    private Transform headPart; // 长按音符的头部，用于显示长按音符的起始位置
    private Transform bodyPart; // 长按音符的身体，用于显示长按音符的持续部分，长度根据noteLength动态调整
    private Transform tailPart; // 长按音符的尾部，用于显示长按音符的结束位置

    public override void Initialize(double hitTime, RhythmManager rhythmManager, Transform spawnLine, Transform judgeLine, ChartNoteData noteData, JudgeManager judgeManager)
    {
        base.Initialize(hitTime, rhythmManager, spawnLine, judgeLine, noteData, judgeManager);
        lastTime = rhythmManager.BeatToSeconds(noteData.endBeat, rhythmManager.currentSongsBpm) - rhythmManager.BeatToSeconds(noteData.beat, rhythmManager.currentSongsBpm); // 计算长按音符的持续时间
        noteLength = rhythmManager.TimeToDistance(lastTime); // 根据持续时间和速度计算音符长度
        headPart = transform.Find("Head"); // 假设长按音符预制体的结构中有一个名为"Head"的子物体作为头部
        bodyPart = transform.Find("Body"); // 假设长按音符预制体的结构中有一个名为"Body"的子物体作为身体
        tailPart = transform.Find("Tail"); // 假设长按音符预制体的结构中有一个名为"Tail"的子物体作为尾部

        Debug.Log($"{noteLength}");

        float unitLength = bodyPart.localScale.x / 3; // 获取身体部分的单位长度，假设初始长度为1个单位长度
        float bodyLength = noteLength - 2 * unitLength; // 计算身体部分的长度，减去头部和尾部的长度
        float bodyScale = bodyLength / unitLength / 2; // 计算身体部分的缩放比例，减去头部和尾部的长度后除以单位长度得到需要多少个单位长度，再除以2因为身体部分的初始长度是2个单位长度

        Debug.Log($"{unitLength}, {bodyScale}");

        bodyPart.localScale = new Vector3(bodyPart.localScale.x, bodyPart.localScale.y * bodyScale, bodyPart.localScale.z); // 调整身体部分的长度
        bodyPart.localPosition = new Vector3(bodyPart.localPosition.x, bodyPart.localPosition.y, headPart.localPosition.z + bodyLength / 2 + unitLength / 2); // 调整身体部分的位置，使其连接在头部和尾部之间
        tailPart.localPosition = new Vector3(tailPart.localPosition.x, tailPart.localPosition.y, headPart.localPosition.z + bodyLength + unitLength); // 调整尾部的位置，使其连接在身体部分的末端
    }
    public override void Update()
    {
        base.Update();
        if(rhythmManager.currentDspTime > hitTime + lastTime && noteData.isHolding && !noteData.isJudged) // 如果当前时间超过长按音符的结束时间且玩家正在长按且该音符还没有被判定，显示Perfect判定效果并销毁音符
        {
            judgeManager.ShowJedgeEffect(JudgeManager.JudgeResult.Perfect); // 显示Perfect判定效果
            noteData.isJudged = true; // 标记该音符已经被判定，避免重复判定
            Destroy(gameObject);
        }
        if(rhythmManager.currentDspTime > hitTime + lastTime + 0.1*rhythmManager.spawnToHitTime) // 音符经过判定线后销毁，避免过多未被击打的音符占用资源
        {
            if(!noteData.isJudged) // 如果该音符还没有被判定，显示Miss判定效果
            {
                judgeManager.ShowJedgeEffect(JudgeManager.JudgeResult.Miss); // 显示Miss判定效果
            }
            noteData.isJudged = true; // 标记该音符已经被判定，避免重复判定
            Destroy(gameObject);
        }
    }
}
