using System;
using System.Collections.Generic;


[Serializable]
public class SongConfigData
{
    public string songID; //歌曲ID，唯一标识一首歌曲，便于后续管理和操作，例如在游戏中判断玩家选择了哪首歌曲
    public string songName; //歌曲名称，便于在游戏中显示和选择
    public string artist; //艺术家名称
    public string audio; //音频文件路径
    public string cover; //封面图片路径
    public string background; //背景图片路径
    public float previewStart; //预览开始时间
    public float previewLength; //预览长度
    public List<SongChartEntryData> charts;
}

[Serializable]
public class SongChartEntryData
{
    public int level; //谱面难度等级
    public string file; //谱面文件路径
}

