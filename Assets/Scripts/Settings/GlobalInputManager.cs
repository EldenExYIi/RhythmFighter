using UnityEngine;

public class GlobalInputManager : MonoBehaviour
{
    public GameObject settingsPanel;  // 在 Inspector 中拖入 SettingsPanel
    public RhythmManager rhythmManager;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null)
            {
                bool isActive = settingsPanel.activeSelf;
                settingsPanel.SetActive(!isActive);
                if (rhythmManager != null)
                {
                    rhythmManager.SetPause(!isActive); // 打开设置面板时暂停游戏，关闭时恢复
                }
            }
        }
    }
}