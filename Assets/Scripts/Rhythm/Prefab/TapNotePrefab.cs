using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapNotePrefab : NotePrefab
{
    public override void Initialize(double hitTime, RhythmManager rhythmManager, Transform spawnLine, Transform judgeLine, ChartNoteData noteData, JudgeManager judgeManager)
    {
        base.Initialize(hitTime, rhythmManager, spawnLine, judgeLine, noteData, judgeManager);
    }
    public override void Update()
    {
        base.Update();
        if(rhythmManager.currentDspTime > hitTime + 0.1*rhythmManager.spawnToHitTime) // 音符经过判定线后销毁，避免过多未被击打的音符占用资源
        {
            judgeManager.ShowJedgeEffect(JudgeManager.JudgeResult.Miss); // 显示Miss判定效果
            noteData.isJudged = true; // 标记该音符已经被判定，避免重复判定
            Destroy(gameObject);
        }
    }
}
