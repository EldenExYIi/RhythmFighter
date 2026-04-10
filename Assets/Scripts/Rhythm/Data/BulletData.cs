using System;
using System.Collections.Generic;

[Serializable]
public class BulletChartData
{
    public List<BulletGroupData> bulletGroups;
}

[Serializable]
public class BulletGroupData
{
    public int id;
    public BeatData startBeat;
    public BeatData endBeat;
    public int count;
    public float velocityZ;
    public float accelerationZ;
    public float spawnLaneRatio;
    public float judgeLaneRatio;
}