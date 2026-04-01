using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public Vector3 minBounds = new Vector3(-3.75f, 0.02f, -7f);   // 最小坐标 (x, y, z)
    public Vector3 maxBounds = new Vector3(3.75f, 0.02f, -7f);
    void Start()
    {
        transform.position = new Vector3(0, 0.02f, -7f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.LeftArrow)) move += Vector3.left;      // 左 (X-)
        if (Input.GetKey(KeyCode.RightArrow)) move += Vector3.right;     // 右 (X+)

        transform.Translate(move * speed * Time.deltaTime);

        // 限制位置在指定范围内
        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minBounds.x, maxBounds.x);
        transform.position = clampedPos;
    }
}
