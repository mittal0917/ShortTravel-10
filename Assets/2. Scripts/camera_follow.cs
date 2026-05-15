using UnityEngine;

public class camera_follow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;

    void LateUpdate()
    {
        if (target == null) return;

        // 캐릭터 위치 추적, Z축 -10 고정
        transform.position = new Vector3(target.position.x, target.position.y, -10f);
    }
}
