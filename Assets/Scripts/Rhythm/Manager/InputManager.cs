using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    public RhythmManager rhythmManager; // 引用 RhythmManager 获取节奏信息

    public enum InputEventType
    {
        Down,
        Up
    }

    public struct RhythmInputEvent
    {
        public int lane;
        public InputEventType eventType;
        public double dspTime;

        public RhythmInputEvent(int lane, InputEventType eventType, double dspTime)
        {
            this.lane = lane;
            this.eventType = eventType;
            this.dspTime = dspTime;
        }
    }

    [Header("4K Keys")]
    private KeyCode lane0Key = KeyCode.Q;
    private KeyCode lane1Key = KeyCode.W;
    private KeyCode lane2Key = KeyCode.E;
    private KeyCode lane3Key = KeyCode.R;
    // Start is called before the first frame update

    private readonly Queue<RhythmInputEvent> inputQueue = new Queue<RhythmInputEvent>();
    private readonly bool[] isHolding = new bool[4];

    private KeyCode[] keys;

    void Start()
    {
        keys = new KeyCode[4]
        {
            lane0Key, lane1Key, lane2Key, lane3Key
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (!rhythmManager.isPlaying)
        {
            return;
        }

        double currentDspTime = rhythmManager.currentDspTime;

        for (int lane = 0; lane < 4; lane++)
        {
            if (Input.GetKeyDown(keys[lane])) //记录按下
            {
                Debug.Log($"InputManager: KeyDown detected on lane {lane} at dspTime {currentDspTime:F2}s");
                isHolding[lane] = true;
                inputQueue.Enqueue(new RhythmInputEvent(lane, InputEventType.Down, currentDspTime));
            }

            if (Input.GetKeyUp(keys[lane])) //记录抬起
            {
                isHolding[lane] = false;
                inputQueue.Enqueue(new RhythmInputEvent(lane, InputEventType.Up, currentDspTime));
            }
        }
    }
    public bool GetNextInputEvent(out RhythmInputEvent inputEvent) //尝试从输入队列中取出一个事件，如果队列为空则返回false
    {
        if (inputQueue.Count > 0)
        {
            inputEvent = inputQueue.Dequeue();
            return true;
        }

        inputEvent = default;
        return false;
    }

    public bool IsLaneHolding(int lane)
    {
        if (lane < 0 || lane > 3) return false;
        return isHolding[lane];
    }
}
