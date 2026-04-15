using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 initialVelocity;
    private Vector3 currentVelocity;
    private Vector3 acceleration;
    private double spawnDspTime;
    private double hitDspTime;        // 保留用于可能的判定逻辑，不再用于销毁
    private RhythmManager rhythmManager;
    private Vector3 spawnPosition;
    private float judgeZ;             // 判定线的 Z 坐标，用于位置销毁判断
    private float angle;              // 子弹的旋转角度，用于视觉效果

    private bool initialized = false;

    public void Init(Vector3 startPos, Vector3 velocity, Vector3 accel, double hitTime, RhythmManager rhythmMgr)
    {
        spawnPosition = startPos;
        transform.position = startPos;
        initialVelocity = velocity;
        acceleration = accel;
        hitDspTime = hitTime;
        rhythmManager = rhythmMgr;
        spawnDspTime = rhythmMgr != null ? rhythmMgr.currentDspTime : AudioSettings.dspTime;

        // 从 RhythmManager 获取判定线的 Z 坐标，若不存在则使用默认值
        if (rhythmMgr != null && rhythmMgr.judgeLine != null)
            judgeZ = rhythmMgr.judgeLine.position.z;
        else
            judgeZ = 0f; // 保底值

        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        double currentTime = rhythmManager != null ? rhythmManager.currentDspTime : AudioSettings.dspTime;
        double elapsed = currentTime - spawnDspTime;

        // 根据匀变速公式更新位置
        float t = (float)elapsed;
        
        currentVelocity = initialVelocity + acceleration * t;
        angle = Mathf.Atan2(currentVelocity.z, currentVelocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, -(angle + 90), 0);

        Vector3 displacement = initialVelocity * t + 0.5f * acceleration * t * t;
        transform.position = spawnPosition + displacement;

        // 当 Z 坐标小于等于判定线 Z 时，视为到达判定线并销毁
        if (transform.position.z <= judgeZ)
        {
            OnReachJudgeLine();
            Destroy(gameObject);
        }
    }

    private void OnReachJudgeLine()
    {
        Debug.Log($"Bullet reached judge line at X={transform.position.x:F2}");
        // 后续接入判定系统
    }
}