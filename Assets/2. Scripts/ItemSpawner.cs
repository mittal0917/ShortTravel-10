using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject Supply;
    public int gunCount = 20;

    public Vector2 minPos;
    public Vector2 maxPos;
    void Start()
    {
        for (int i = 0; i < gunCount; i++)
        {
            Vector2 randomPos = new Vector2(
    Random.Range(minPos.x, maxPos.x),
    Random.Range(minPos.y, maxPos.y));
        }
    }
}
