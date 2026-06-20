using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    public float hp = 100f;

    public void TakeDamage(float amount)
    {
        hp -= amount;
        Debug.Log($"좀비가 {amount}의 데미지를 받음. 남은 체력: {hp}");

        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("좀비 처치 완료!");
        Destroy(gameObject); // 좀비 오브젝트 제거
    }
}