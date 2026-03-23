using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NotePrefab : MonoBehaviour
{
    public RhythmManager rhythmManager;// 引用 RhythmManager 获取节奏信息
    public Transform spawnLine; // 音符生成线
    public Transform judgeLine; // 音符判定线
    public JudgeManager judgeManager; // 引用 JudgeManager 进行判定
    protected double hitTime;
    protected ChartNoteData noteData;
    private bool initialized = false;
    Vector3 spawnPos;

    public virtual void Initialize(double hitTime, RhythmManager rhythmManager, Transform spawnLine, Transform judgeLine, ChartNoteData noteData, JudgeManager judgeManager)
    {
        this.hitTime = hitTime;
        this.rhythmManager = rhythmManager;
        this.spawnLine = spawnLine;
        this.judgeLine = judgeLine;
        this.noteData = noteData;
        this.judgeManager = judgeManager;
        initialized = true;
        spawnPos = transform.position;
    }

    public virtual void Update()
    {
        if (!initialized) return;
        if(noteData.isJudged) // 如果该音符已经被判定，直接返回，不再更新位置
        {
            Destroy(gameObject);
            return;
        }
        float updateDis = (float)(rhythmManager.currentDspTime - hitTime) * (spawnLine.position.z - judgeLine.position.z) / (float)rhythmManager.spawnToHitTime;
        transform.position = new Vector3(spawnPos.x, spawnPos.y, judgeLine.position.z - updateDis);
    }
}
