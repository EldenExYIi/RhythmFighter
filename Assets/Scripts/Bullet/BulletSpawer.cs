using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    [Header("组件引用")]
    public RhythmManager rhythmManager;
    public Transform spawnLine;
    public Transform judgeLine;
    public Transform notesRoot;
    public GameObject bulletPrefab;

    [Header("轨道边界")]
    public Transform leftBorder;
    public Transform rightBorder;

    [Header("物理参数")]
    public Vector3 defaultAcceleration = Vector3.zero;

    [Header("子弹谱面数据")]
    public List<BulletGroupData> bulletGroups = new List<BulletGroupData>();

    [Header("测试模式")]
    public bool useTestMode = false;
    public int testGroupCount = 2;

    private Dictionary<int, int> groupProgress = new Dictionary<int, int>();

    void Start()
    {
        if (useTestMode)
        {
            GenerateTestGroups();
        }

        if (bulletGroups == null)
            bulletGroups = new List<BulletGroupData>();
    }

    void Update()
    {
        if (rhythmManager == null || !rhythmManager.isPlaying) return;
        if (bulletPrefab == null) return;
        if (bulletGroups == null || bulletGroups.Count == 0) return;

        double currentTime = rhythmManager.currentDspTime;

        for (int i = 0; i < bulletGroups.Count; i++)
        {
            var group = bulletGroups[i];
            if (!groupProgress.ContainsKey(i))
                groupProgress[i] = 0;

            int spawned = groupProgress[i];
            if (spawned >= group.count) continue;

            double startTime = BeatToTime(group.startBeat);
            double endTime = BeatToTime(group.endBeat);
            double interval = (endTime - startTime) / group.count;
            double nextHitTime = startTime + spawned * interval;

            float vz = group.velocityZ;
            float az = group.accelerationZ;
            if (Mathf.Abs(vz) < 0.001f)
            {
                Debug.LogWarning($"子弹组 {group.id} velocityZ=0，跳过");
                groupProgress[i] = group.count;
                continue;
            }

            double flightTime = CalculateFlightTimeZ(vz, az, spawnLine.position.z, judgeLine.position.z);
            if (double.IsInfinity(flightTime) || flightTime <= 0)
            {
                Debug.LogWarning($"子弹组 {group.id} 飞行时间无效，跳过");
                groupProgress[i] = group.count;
                continue;
            }

            if (currentTime >= nextHitTime - flightTime)
            {
                SpawnBulletFromGroup(group, spawned, nextHitTime);
                groupProgress[i] = spawned + 1;
            }
        }
    }

    private double BeatToTime(BeatData beat)
    {
        if (rhythmManager == null) return 0.0;
        double bpm = rhythmManager.currentSongsBpm;
        if (bpm <= 0) bpm = 120;
        return rhythmManager.BeatToSeconds(beat, bpm) + rhythmManager.CurrentChartData.timing.offsetMs / 1000.0;
    }

    private void SpawnBulletFromGroup(BulletGroupData group, int index, double hitTime)
    {
        float spawnX = Mathf.Lerp(leftBorder.position.x, rightBorder.position.x, group.spawnLaneRatio);
        float judgeX = Mathf.Lerp(leftBorder.position.x, rightBorder.position.x, group.judgeLaneRatio);
        Vector3 spawnPos = new Vector3(spawnX, spawnLine.position.y, spawnLine.position.z);

        double flightTime = CalculateFlightTimeZ(group.velocityZ, group.accelerationZ,
                                                 spawnLine.position.z, judgeLine.position.z);
        float vx = (float)((judgeX - spawnX) / flightTime);
        Vector3 velocity = new Vector3(vx, 0, group.velocityZ);
        Vector3 acceleration = new Vector3(0, 0, group.accelerationZ);

        spawnPos.y += 0.2f;

        GameObject obj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity, notesRoot);
        Bullet bullet = obj.GetComponent<Bullet>();
        if (bullet == null)
        {
            Debug.LogError("BulletSpawner: 子弹预制体缺少 Bullet 组件！");
            Destroy(obj);
            return;
        }
        bullet.Init(spawnPos, velocity, acceleration, hitTime, rhythmManager);
    }

    private double CalculateFlightTimeZ(float vz, float az, float startZ, float endZ)
    {
        float dz = endZ - startZ;
        if (Mathf.Abs(az) < 0.001f)
        {
            if (Mathf.Abs(vz) < 0.001f) return double.PositiveInfinity;
            return dz / vz;
        }
        else
        {
            float a = 0.5f * az;
            float b = vz;
            float c = -dz;
            float disc = b * b - 4 * a * c;
            if (disc < 0) return double.PositiveInfinity;
            float sqrtDisc = Mathf.Sqrt(disc);
            float t1 = (-b + sqrtDisc) / (2 * a);
            float t2 = (-b - sqrtDisc) / (2 * a);
            if (t1 > 0 && t2 > 0) return Mathf.Min(t1, t2);
            if (t1 > 0) return t1;
            if (t2 > 0) return t2;
            return double.PositiveInfinity;
        }
    }

    private void GenerateTestGroups()
    {
        bulletGroups.Clear();
        for (int i = 0; i < testGroupCount; i++)
        {
            bulletGroups.Add(new BulletGroupData
            {
                id = i,
                startBeat = new BeatData { bar = i * 2, numerator = 0, denominator = 4 },
                endBeat = new BeatData { bar = i * 2, numerator = 3, denominator = 4 },
                count = 4,
                velocityZ = -12f,
                accelerationZ = 0f,
                spawnLaneRatio = 0.125f + i * 0.2f,
                judgeLaneRatio = 0.875f - i * 0.2f
            });
        }
        Debug.Log($"BulletSpawner: 测试模式生成 {bulletGroups.Count} 组子弹");
    }
}