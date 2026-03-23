using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapNotePrefab : NotePrefab
{
    public override void Initialize(double hitTime, RhythmManager rhythmManager, Transform spawnLine, Transform judgeLine, ChartNoteData noteData)
    {
        base.Initialize(hitTime, rhythmManager, spawnLine, judgeLine, noteData);
    }
    public override void Update()
    {
        base.Update();
        if(rhythmManager.currentDspTime > hitTime + 0.2*rhythmManager.spawnToHitTime) // 音符经过判定线后销毁，避免过多未被击打的音符占用资源
        {
            Destroy(gameObject);
        }
    }
}
