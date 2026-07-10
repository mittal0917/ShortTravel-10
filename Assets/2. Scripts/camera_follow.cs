using UnityEngine;

public class camera_follow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    [SerializeField] private float movingSmoothTime = 0.12f;
    [SerializeField] private float stoppedSmoothTime = 0.35f;
    [SerializeField] private float movingSpeedThreshold = 0.05f;

    private Map_Generator mapGenerator;
    private Vector3 smoothVelocity;
    private Vector3 previousTargetPosition;
    private bool hasPreviousTargetPosition;

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (mapGenerator == null)
        {
            mapGenerator = FindObjectOfType<Map_Generator>();
        }

        float targetSpeed = CalculateTargetSpeed();
        float smoothTime = targetSpeed > movingSpeedThreshold ? movingSmoothTime : stoppedSmoothTime;
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        desiredPosition = ClampToMapBounds(desiredPosition);

        // 플레이어가 움직일 때는 빠르게 따라가고, 멈추면 부드럽게 느려지며 중앙에 정렬됩니다.
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref smoothVelocity, smoothTime);
        transform.position = ClampToMapBounds(transform.position);
    }

    private float CalculateTargetSpeed()
    {
        if (!hasPreviousTargetPosition)
        {
            previousTargetPosition = target.position;
            hasPreviousTargetPosition = true;
            return 0f;
        }

        float speed = (target.position - previousTargetPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        previousTargetPosition = target.position;
        return speed;
    }

    private Vector3 ClampToMapBounds(Vector3 position)
    {
        if (mapGenerator == null)
        {
            return new Vector3(position.x, position.y, -10f);
        }

        Vector2 cameraHalfSize = GetCameraHalfSize();
        float minX = mapGenerator.MapMinX + cameraHalfSize.x;
        float maxX = mapGenerator.MapMaxX - cameraHalfSize.x;
        float minY = mapGenerator.MapMinY + cameraHalfSize.y;
        float maxY = mapGenerator.MapMaxY - cameraHalfSize.y;

        // 맵이 카메라 화면보다 작을 때는 억지로 한쪽에 붙이지 않고 맵 중앙을 바라보게 합니다.
        float clampedX = minX > maxX ? (mapGenerator.MapMinX + mapGenerator.MapMaxX) * 0.5f : Mathf.Clamp(position.x, minX, maxX);
        float clampedY = minY > maxY ? (mapGenerator.MapMinY + mapGenerator.MapMaxY) * 0.5f : Mathf.Clamp(position.y, minY, maxY);
        return new Vector3(clampedX, clampedY, -10f);
    }

    private Vector2 GetCameraHalfSize()
    {
        Camera currentCamera = GetComponent<Camera>();
        if (currentCamera == null)
        {
            return Vector2.zero;
        }

        if (currentCamera.orthographic)
        {
            float halfHeight = currentCamera.orthographicSize;
            return new Vector2(halfHeight * currentCamera.aspect, halfHeight);
        }

        // 배경 맵 스프라이트가 z=0.5에 있으므로 그 평면 기준으로 화면 크기를 계산해야 맵 끝에서 빈 화면이 덜 보입니다.
        float distanceToMapPlane = Mathf.Abs(transform.position.z - 0.5f);
        float perspectiveHalfHeight = Mathf.Tan(currentCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distanceToMapPlane;
        return new Vector2(perspectiveHalfHeight * currentCamera.aspect, perspectiveHalfHeight);
    }
}
