using System;
using System.Collections.Generic;

[Serializable]
public class ChartData
{
    public int version; //谱面版本号，方便后续更新谱面格式时进行兼容处理
    public ChartMetaData meta; //谱面元数据，包含谱面ID、名称、难度等级、轨道数量等信息
    public ChartTimingData timing; //谱面节奏数据，包含全局偏移、BPM变化、速度变化等信息
    public List<ChartNoteData> notes; //谱面音符数据列表，每个元素包含音符ID、类型、位置、持续时间等信息
}

[Serializable]
public class ChartMetaData
{
    public string chartID; //谱面ID，唯一标识一个谱面
    public string chartName; //谱面名称，便于在游戏中显示和选择，以及后续管理谱面数据
    public int level; //谱面难度等级，整数表示，数值越大表示难度越高
    public int laneNumber; //谱面轨道数量，整数表示，数值越大表示需要操作的轨道越多
}

[Serializable]
public class ChartTimingData
{
    public int offsetMs; //全局偏移，单位为毫秒，用于调整谱面与音乐的同步，正值表示谱面整体向后偏移，负值表示向前偏移
    public List<ChartBpmData> bpms; //谱面BPM变化数据列表，每个元素包含变化发生的节拍和新的BPM值，用于实现歌曲中不同部分的节奏变化
    public List<ChartSpeedChangeData> speedChanges; //谱面速度变化数据列表，每个元素包含变化发生的节拍和新的速度倍率，用于实现歌曲中不同部分的速度变化，例如加速或减速效果
}

[Serializable]
public class ChartBpmData
{
    public BeatData beat; //变化发生的节拍，单位为小节数，例如在一小节四拍子中2.5表示在第2小节的第2拍发生变化
    public float bpm; //新的BPM值，单位为每分钟节拍数，例如120表示每分钟120拍，即每秒2拍
}

[Serializable]
public class ChartSpeedChangeData
{
    public BeatData beat; //变化发生的节拍，单位为小节数，例如在一小节四拍子中2.5表示在第2小节的第2拍发生变化
    public float speed; //新的速度倍率，用于实现歌曲中不同部分的速度变化，例如1.0表示正常速度，0.5表示半速，2.0表示双倍速
}

[Serializable]
public class ChartNoteData
{
    public int id; //音符ID，唯一标识一个音符，便于后续管理和操作，例如在游戏中判断玩家是否正确击打了某个音符
    public BeatData beat; //音符发生的时间，单位为小节数
    public int lane; //音符所在的轨道，整数表示
    public string type; //音符类型，例如"normal"、"hold"等
    public BeatData endBeat; //音符结束的时间，单位为小节数，仅在type为"hold"时有效，表示长按音符的结束时间
}

[Serializable]
public class BeatData
{
    public int bar;
    public int numerator;
    public int denominator;
}
