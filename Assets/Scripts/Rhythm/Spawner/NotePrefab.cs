using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NotePrefab : MonoBehaviour
{
    public RhythmManager rhythmManager;// 引用 RhythmManager 获取节奏信息
    public Transform spawnLine; // 音符生成线
    public Transform judgeLine; // 音符判定线
    private double hitTime;
    private bool initialized = false;
    Vector3 spawnPos;

    public void Initialize(double hitTime)
    {
        this.hitTime = hitTime;
        initialized = true;
        spawnPos = transform.position;
    }

    void Update()
    {
        if (!initialized) return;
        float updateDis = (float)(rhythmManager.currentDspTime - hitTime) * (spawnLine.position.z - judgeLine.position.z) / rhythmManager.CurrentChartData.meta.spawnToHitTime;
        transform.position = new Vector3(spawnPos.x, spawnPos.y, judgeLine.position.z - updateDis);
        if(rhythmManager.currentDspTime > hitTime + 0.2*rhythmManager.CurrentChartData.meta.spawnToHitTime) // 音符经过判定线后销毁，避免过多未被击打的音符占用资源
        {
            Destroy(gameObject);
        }
    }
}
