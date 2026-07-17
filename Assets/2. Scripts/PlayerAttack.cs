using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private enum WeaponType
    {
        Bat,
        Pistol
    }

    [Header("Bat")]
    [SerializeField] private float batRangeTiles = 1f;
    [SerializeField] private float batAttackIntervalSeconds = 3f;
    [SerializeField] private float batDamage = 1f;
    [SerializeField] private float batKnockbackTiles = 1f;
    [SerializeField] private float batSwingVisualAngle = 170f;
    [SerializeField] private float batSwingDurationSeconds = 0.28f;

    [Header("Pistol")]
    [SerializeField] private float pistolRangeTiles = 6f;
    [SerializeField] private float pistolAttackIntervalSeconds = 1f;
    [SerializeField] private float pistolDamage = 3f;
    [SerializeField] private float pistolPickupRangeTiles = 1f;
    [SerializeField] private float bulletVisualSpeed = 14f;
    [SerializeField] private float bulletVisualLifetimeSeconds = 0.5f;
    [SerializeField] private Vector2 defaultPistolSpawnOffset = new Vector2(3f, 0f);

    [Header("Ammo")]
    [SerializeField] private int startingAmmo = 33;
    [SerializeField] private int maxAmmo = 33;
    [SerializeField] private int supplyAmmoBonus = 10;

    private character_move movement;
    private WeaponType currentWeapon = WeaponType.Bat;
    private Transform weaponPivot;
    private SpriteRenderer batRenderer;
    private SpriteRenderer pistolRenderer;
    private Sprite batSprite;
    private Sprite pistolSprite;
    private Sprite bulletSprite;
    private MeshRenderer batSweepRenderer;
    private MeshFilter batSweepFilter;
    private GameObject pistolPickup;
    private Collider2D playerCollider;
    private TextMesh ammoText;
    private MeshRenderer ammoTextRenderer;
    private int currentAmmo;
    private float nextAttackTime;
    private bool isSwinging;

    void Awake()
    {
        movement = GetComponent<character_move>();
        playerCollider = GetComponent<Collider2D>();
        currentAmmo = Mathf.Clamp(startingAmmo, 0, maxAmmo);
        CreateWeaponVisuals();
        CreateAmmoText();
    }

    IEnumerator Start()
    {
        EquipBat();
        yield return null;
        EnsurePistolPickupExists();
    }

    void Update()
    {
        UpdateWeaponPose();
        TryPickupPistol();
        TryAutoAttack();
    }

    private void TryAutoAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        bool attacked = currentWeapon == WeaponType.Bat ? TryBatAttack() : TryPistolAttack();
        if (attacked)
        {
            nextAttackTime = Time.time + GetAttackInterval();
        }
    }

    private bool TryBatAttack()
    {
        Enemy_NormalZombie[] enemies = FindObjectsOfType<Enemy_NormalZombie>();
        bool hitAnyEnemy = false;
        Vector2 swingDirection = GetFacingDirection();
        float closestDistance = float.MaxValue;

        foreach (Enemy_NormalZombie enemy in enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            Vector2 toEnemy = enemy.transform.position - transform.position;
            float distance = toEnemy.magnitude;
            float hitRange = batRangeTiles + GetColliderRadius(playerCollider) + GetColliderRadius(enemy.GetComponent<Collider2D>());
            if (distance > hitRange)
            {
                continue;
            }

            if (distance < closestDistance && toEnemy.sqrMagnitude > 0.001f)
            {
                closestDistance = distance;
                swingDirection = toEnemy.normalized;
            }

            enemy.TakeDamage(batDamage, toEnemy, batKnockbackTiles);
            hitAnyEnemy = true;
        }

        if (hitAnyEnemy && !isSwinging)
        {
            StartCoroutine(SwingBat(swingDirection));
        }

        return hitAnyEnemy;
    }

    private bool TryPistolAttack()
    {
        if (currentAmmo <= 0)
        {
            UpdateAmmoText();
            return false;
        }

        Enemy_NormalZombie target = FindClosestEnemy(pistolRangeTiles);
        if (target == null)
        {
            return false;
        }

        currentAmmo--;
        UpdateAmmoText();
        SpawnBulletVisual(target);
        target.TakeDamage(pistolDamage);
        StartCoroutine(FlashPistol());
        return true;
    }

    public void AddAmmoFromSupply()
    {
        currentAmmo = Mathf.Min(maxAmmo, currentAmmo + supplyAmmoBonus);
        UpdateAmmoText();
    }

    public void RefreshPistolPickupNearPlayer()
    {
        if (currentWeapon == WeaponType.Pistol)
        {
            return;
        }

        EnsurePistolPickupExists();
        if (pistolPickup == null)
        {
            return;
        }

        // 맵 크기 변경이나 저장 위치 보정 후에도 시작 총이 플레이어 근처에 보이도록 다시 배치합니다.
        pistolPickup.transform.position = FindPistolPickupPosition();
    }

    private Enemy_NormalZombie FindClosestEnemy(float range)
    {
        Enemy_NormalZombie[] enemies = FindObjectsOfType<Enemy_NormalZombie>();
        Enemy_NormalZombie closest = null;
        float closestDistance = range;

        foreach (Enemy_NormalZombie enemy in enemies)
        {
            if (enemy == null)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance <= closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    private void TryPickupPistol()
    {
        if (currentWeapon == WeaponType.Pistol || pistolPickup == null)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, pistolPickup.transform.position);
        if (distance <= pistolPickupRangeTiles && Input.GetKeyDown(KeyCode.F))
        {
            Destroy(pistolPickup);
            EquipPistol();
        }
    }

    private void EquipBat()
    {
        currentWeapon = WeaponType.Bat;
        batRenderer.enabled = false;
        pistolRenderer.enabled = false;
        UpdateAmmoText();
    }

    private void EquipPistol()
    {
        currentWeapon = WeaponType.Pistol;
        batRenderer.enabled = false;
        pistolRenderer.enabled = false;
        nextAttackTime = 0f;
        UpdateAmmoText();
    }

    private float GetAttackInterval()
    {
        return currentWeapon == WeaponType.Bat ? batAttackIntervalSeconds : pistolAttackIntervalSeconds;
    }

    private float GetColliderRadius(Collider2D targetCollider)
    {
        if (targetCollider == null)
        {
            return 0f;
        }

        Vector2 extents = targetCollider.bounds.extents;
        return Mathf.Max(extents.x, extents.y);
    }

    private Vector2 GetFacingDirection()
    {
        if (movement != null)
        {
            return movement.FacingDirection.sqrMagnitude > 0.001f ? movement.FacingDirection.normalized : Vector2.down;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        return input.sqrMagnitude > 0.001f ? input.normalized : Vector2.down;
    }

    private void CreateWeaponVisuals()
    {
        GameObject pivot = new GameObject("WeaponPivot");
        pivot.transform.SetParent(transform, false);
        pivot.transform.localPosition = Vector3.zero;
        weaponPivot = pivot.transform;

        batSprite = WeaponSpriteLibrary.CreateBatSprite();
        pistolSprite = WeaponSpriteLibrary.CreatePistolSprite();
        bulletSprite = WeaponSpriteLibrary.CreateBulletSprite();

        batRenderer = CreateChildRenderer("Bat", batSprite != null ? batSprite : BuildRectSprite(8, 40), Color.white, 12);
        batRenderer.transform.localPosition = new Vector3(0f, -0.45f, 0f);
        batRenderer.transform.localScale = batSprite != null ? new Vector3(0.7f, 0.7f, 1f) : new Vector3(0.16f, 0.55f, 1f);

        pistolRenderer = CreateChildRenderer("Pistol", pistolSprite != null ? pistolSprite : BuildRectSprite(28, 14), Color.white, 12);
        pistolRenderer.transform.localPosition = new Vector3(0f, -0.42f, 0f);
        pistolRenderer.transform.localScale = pistolSprite != null ? new Vector3(0.8f, 0.8f, 1f) : new Vector3(0.35f, 0.18f, 1f);

        CreateBatSweepVisual();
    }

    private void CreateAmmoText()
    {
        GameObject textObject = new GameObject("AmmoText");
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = new Vector3(0f, -0.95f, 0f);

        ammoText = textObject.AddComponent<TextMesh>();
        ammoText.anchor = TextAnchor.MiddleCenter;
        ammoText.alignment = TextAlignment.Center;
        ammoText.characterSize = 0.16f;
        ammoText.fontSize = 42;
        ammoText.color = Color.white;

        ammoTextRenderer = textObject.GetComponent<MeshRenderer>();
        if (ammoTextRenderer != null)
        {
            ammoTextRenderer.sortingOrder = 30;
        }

        UpdateAmmoText();
    }

    private void UpdateAmmoText()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString();
        }

        if (ammoTextRenderer != null)
        {
            ammoTextRenderer.enabled = currentWeapon == WeaponType.Pistol;
        }
    }

    private SpriteRenderer CreateChildRenderer(string objectName, Sprite sprite, Color color, int sortingOrder)
    {
        GameObject child = new GameObject(objectName);
        child.transform.SetParent(weaponPivot, false);

        SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private void UpdateWeaponPose()
    {
        if (isSwinging)
        {
            return;
        }

        Vector2 direction = GetFacingDirection();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        weaponPivot.localRotation = Quaternion.Euler(0f, 0f, angle);
        weaponPivot.localPosition = direction * 0.35f;
    }

    private IEnumerator SwingBat(Vector2 forward)
    {
        isSwinging = true;
        float baseAngle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg - 90f;
        float elapsed = 0f;
        float duration = batSwingDurationSeconds;

        batRenderer.enabled = true;
        batRenderer.transform.localPosition = new Vector3(0f, -0.75f, 0f);
        batRenderer.transform.localScale = batSprite != null ? new Vector3(0.95f, 0.95f, 1f) : new Vector3(0.2f, 0.85f, 1f);
        ShowBatSweep(baseAngle);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            float swingAngle = Mathf.Lerp(-batSwingVisualAngle * 0.5f, batSwingVisualAngle * 0.5f, easedProgress);
            float scalePulse = Mathf.Sin(progress * Mathf.PI);

            weaponPivot.localRotation = Quaternion.Euler(0f, 0f, baseAngle + swingAngle);
            weaponPivot.localPosition = forward * (0.45f + scalePulse * 0.25f);
            batRenderer.color = Color.Lerp(Color.white, new Color(1f, 0.78f, 0.38f, 1f), scalePulse);
            yield return null;
        }

        HideBatSweep();
        batRenderer.color = Color.white;
        batRenderer.transform.localPosition = new Vector3(0f, -0.45f, 0f);
        batRenderer.transform.localScale = batSprite != null ? new Vector3(0.7f, 0.7f, 1f) : new Vector3(0.16f, 0.55f, 1f);
        batRenderer.enabled = false;
        isSwinging = false;
    }

    private void CreateBatSweepVisual()
    {
        GameObject sweep = new GameObject("BatSweep");
        sweep.transform.SetParent(transform, false);
        sweep.transform.localPosition = Vector3.zero;

        batSweepFilter = sweep.AddComponent<MeshFilter>();
        batSweepRenderer = sweep.AddComponent<MeshRenderer>();
        batSweepRenderer.material = new Material(Shader.Find("Sprites/Default"));
        batSweepRenderer.material.color = new Color(1f, 0.82f, 0.25f, 0.35f);
        batSweepRenderer.sortingOrder = 11;
        batSweepRenderer.enabled = false;
    }

    private void ShowBatSweep(float baseAngle)
    {
        if (batSweepFilter == null || batSweepRenderer == null)
        {
            return;
        }

        batSweepFilter.mesh = BuildArcMesh(GetVisibleBatRange(), batSwingVisualAngle, 18, baseAngle + 90f);
        batSweepRenderer.enabled = true;
    }

    private void HideBatSweep()
    {
        if (batSweepRenderer != null)
        {
            batSweepRenderer.enabled = false;
        }
    }

    private float GetVisibleBatRange()
    {
        Collider2D targetCollider = playerCollider != null ? playerCollider : GetComponent<Collider2D>();
        return batRangeTiles + GetColliderRadius(targetCollider);
    }

    private static Mesh BuildArcMesh(float radius, float angle, int segments, float directionAngle)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        float startAngle = directionAngle - angle * 0.5f;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = (startAngle + angle * i / segments) * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Cos(currentAngle) * radius, Mathf.Sin(currentAngle) * radius, 0f);
        }

        for (int i = 0; i < segments; i++)
        {
            int triangleIndex = i * 3;
            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = i + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }

    private IEnumerator FlashPistol()
    {
        pistolRenderer.enabled = true;
        Color originalColor = pistolRenderer.color;
        pistolRenderer.color = new Color(1f, 0.9f, 0.35f, 1f);
        yield return new WaitForSeconds(0.08f);
        pistolRenderer.color = originalColor;
        pistolRenderer.enabled = false;
    }

    private void SpawnBulletVisual(Enemy_NormalZombie target)
    {
        Vector3 targetPosition = target != null ? target.transform.position : transform.position + (Vector3)GetFacingDirection();
        Vector2 direction = targetPosition - transform.position;
        if (direction.sqrMagnitude <= 0.001f)
        {
            direction = GetFacingDirection();
        }
        else
        {
            direction.Normalize();
        }

        GameObject bulletObject = new GameObject("Bullet_Visual");
        bulletObject.transform.position = transform.position + (Vector3)(direction * 0.55f);
        bulletObject.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        SpriteRenderer renderer = bulletObject.AddComponent<SpriteRenderer>();
        renderer.sprite = bulletSprite != null ? bulletSprite : BuildRectSprite(18, 6);
        renderer.color = bulletSprite != null ? Color.white : new Color(1f, 0.92f, 0.2f, 1f);
        renderer.sortingOrder = 25;

        bulletObject.transform.localScale = bulletSprite != null ? new Vector3(0.75f, 0.75f, 1f) : new Vector3(0.35f, 0.35f, 1f);
        StartCoroutine(MoveBulletVisual(bulletObject, direction, target));
    }

    private IEnumerator MoveBulletVisual(GameObject bulletObject, Vector2 direction, Enemy_NormalZombie target)
    {
        Vector3 startPosition = bulletObject != null ? bulletObject.transform.position : transform.position;
        float hitDistance = GetBulletHitDistance(startPosition, target);
        float elapsed = 0f;
        while (bulletObject != null && elapsed < bulletVisualLifetimeSeconds)
        {
            elapsed += Time.deltaTime;
            bulletObject.transform.position += (Vector3)(direction * bulletVisualSpeed * Time.deltaTime);
            // 총알이 좀비의 몸에 닿은 거리까지 이동하면 관통하지 않고 바로 사라지게 합니다.
            if (Vector2.Distance(startPosition, bulletObject.transform.position) >= hitDistance)
            {
                break;
            }

            yield return null;
        }

        if (bulletObject != null)
        {
            Destroy(bulletObject);
        }
    }

    private float GetBulletHitDistance(Vector3 startPosition, Enemy_NormalZombie target)
    {
        if (target == null)
        {
            return bulletVisualSpeed * bulletVisualLifetimeSeconds;
        }

        Collider2D targetCollider = target.GetComponent<Collider2D>();
        Vector2 hitPoint = targetCollider != null
            ? targetCollider.ClosestPoint(startPosition)
            : (Vector2)target.transform.position;

        return Mathf.Max(0.05f, Vector2.Distance(startPosition, hitPoint));
    }

    private void EnsurePistolPickupExists()
    {
        if (pistolPickup != null)
        {
            return;
        }

        GameObject existingPickup = GameObject.Find("Pickup_Pistol");
        if (existingPickup != null)
        {
            pistolPickup = existingPickup;
            return;
        }

        pistolPickup = new GameObject("Pickup_Pistol");
        pistolPickup.transform.position = FindPistolPickupPosition();

        SpriteRenderer renderer = pistolPickup.AddComponent<SpriteRenderer>();
        renderer.sprite = pistolSprite != null ? pistolSprite : BuildRectSprite(28, 14);
        renderer.color = pistolSprite != null ? Color.white : new Color(0.08f, 0.08f, 0.1f, 1f);
        renderer.sortingOrder = 8;
        pistolPickup.transform.localScale = pistolSprite != null ? new Vector3(0.9f, 0.9f, 1f) : Vector3.one;

        CircleCollider2D trigger = pistolPickup.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = pistolPickupRangeTiles;
    }

    private Vector3 FindPistolPickupPosition()
    {
        Map_Generator mapGenerator = FindObjectOfType<Map_Generator>();
        Vector3 defaultPosition = transform.position + (Vector3)defaultPistolSpawnOffset;
        if (mapGenerator == null || mapGenerator.IsWalkableWorld(defaultPosition))
        {
            return defaultPosition;
        }

        Vector2[] offsets =
        {
            new Vector2(2f, 0f),
            new Vector2(-2f, 0f),
            new Vector2(0f, 2f),
            new Vector2(0f, -2f),
            new Vector2(2f, 2f),
            new Vector2(-2f, 2f),
            new Vector2(2f, -2f),
            new Vector2(-2f, -2f)
        };

        // 기본 위치가 물/나무/돌 위라면 플레이어 주변에서 가장 가까운 통행 가능 위치를 찾습니다.
        foreach (Vector2 offset in offsets)
        {
            Vector3 candidate = transform.position + (Vector3)offset;
            if (mapGenerator.IsWalkableWorld(candidate))
            {
                return candidate;
            }
        }

        return transform.position;
    }

    private static Sprite BuildRectSprite(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), Mathf.Max(width, height));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GetVisibleBatRange());

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pistolRangeTiles);
    }
}
