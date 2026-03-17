using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeEffectsPrefab : MonoBehaviour
{
    public Sprite perfectSprite;
    public Sprite goodSprite;
    public Sprite missSprite;

    public SpriteRenderer spriteRenderer;

    private double dspTime = 0.0f;
    bool isInitialized = false;

    public void Initialize(JudgeManager.JudgeResult judgeResult)
    {
        switch (judgeResult)
        {
            case JudgeManager.JudgeResult.Perfect:
                spriteRenderer.sprite = perfectSprite;
                break;
            case JudgeManager.JudgeResult.Good:
                spriteRenderer.sprite = goodSprite;
                break;
            case JudgeManager.JudgeResult.Miss:
                spriteRenderer.sprite = missSprite;
                break;
            default:
                Debug.LogWarning($"JudgeEffectsPrefab: unknown judge result '{judgeResult}'");
                break;
        }
        dspTime = AudioSettings.dspTime;
        isInitialized = true;
    }
    public void Update()
    {
        if (!isInitialized)
        {
            return;
        }
        if(AudioSettings.dspTime - dspTime > 0.5f) // 判定效果显示0.5秒后自动销毁
        {
            Destroy(gameObject);
        }
    }
}
