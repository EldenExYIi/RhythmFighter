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
        // 使判定效果逐渐变淡
        Color fade = spriteRenderer.color;
        fade.a = 1.0f - (float)(AudioSettings.dspTime - dspTime) * 2.0f; // 判定效果逐渐变淡
        spriteRenderer.color = fade;
        
        transform.position += Vector3.up * Time.deltaTime * 0.5f; // 判定效果向上漂浮

        transform.localScale -= Vector3.one * Time.deltaTime * 0.5f; // 判定效果逐渐变小

        if(AudioSettings.dspTime - dspTime > 0.5f) // 判定效果显示0.5秒后自动销毁
        {
            Destroy(gameObject);
        }
    }
}
