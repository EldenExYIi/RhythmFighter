using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "音游/游戏设置")]
public class GameSettings : ScriptableObject
{
    [Header("运行时设置")]
    public float noteSpeed = 1.0f;       // 流速倍率
    public float spawnOffsetY = 0f;      // 音符生成位置偏移（Y轴）
    public float masterVolume = 0.8f;    // 主音量

    // 保存到 PlayerPrefs
    public void Save()
    {
        PlayerPrefs.SetFloat("NoteSpeed", noteSpeed);
        PlayerPrefs.SetFloat("SpawnOffsetY", spawnOffsetY);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();
    }

    // 从 PlayerPrefs 加载
    public void Load()
    {
        noteSpeed = PlayerPrefs.GetFloat("NoteSpeed", 1.0f);
        spawnOffsetY = PlayerPrefs.GetFloat("SpawnOffsetY", 0f);
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
    }
}