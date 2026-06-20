using UnityEngine;

public class FieldWeaponItem : MonoBehaviour, IInteractable
{
    [Header("플레이어에게 쥐여줄 인게임 무기 프리펩(bat 또는 pistol)")]
    public GameObject weaponPrefab;
    public string weaponName = "무기";

    public string GetInteractionPrompt()
    {
        return $"[F] {weaponName} 줍기";
    }

    public void Interact(PlayerController player)
    {
        player.EquipWeapon(weaponPrefab);
        Destroy(gameObject); // 땅에 있던 아이템 삭제
    }
}