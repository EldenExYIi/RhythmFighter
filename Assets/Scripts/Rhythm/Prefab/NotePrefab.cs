using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NotePrefab : MonoBehaviour
{
    public RhythmManager rhythmManager;// 引用 RhythmManager 获取节奏信息
    public Transform spawnLine; // 音符生成线
    public Transform judgeLine; // 音符判定线
    protected double hitTime;
    protected ChartNoteData noteData;
    private bool initialized = false;
    Vector3 spawnPos;

    public virtual void Initialize(double hitTime, RhythmManager rhythmManager, Transform spawnLine, Transform judgeLine, ChartNoteData noteData)
    {
        this.hitTime = hitTime;
        this.rhythmManager = rhythmManager;
        this.spawnLine = spawnLine;
        this.judgeLine = judgeLine;
        this.noteData = noteData;
        initialized = true;
        spawnPos = transform.position;
    }

    public virtual void Update()
    {
        if (!initialized) return;
        float updateDis = (float)(rhythmManager.currentDspTime - hitTime) * (spawnLine.position.z - judgeLine.position.z) / (float)rhythmManager.spawnToHitTime;
        transform.position = new Vector3(spawnPos.x, spawnPos.y, judgeLine.position.z - updateDis);
    }
}
