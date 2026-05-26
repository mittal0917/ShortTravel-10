using UnityEngine;

public class character_move : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float saveTimer;
    private bool hasStarted;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (!GameProgress.ConsumeNewGameRequest()
            && GameProgress.TryLoadPlayerPosition(out Vector3 savedPosition))
        {
            transform.position = savedPosition;
        }

        hasStarted = true;
    }

    void Update()
    {
        // WASD + 방향키 입력
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(x, y).normalized;

        saveTimer += Time.deltaTime;
        if (saveTimer >= 0.5f)
        {
            saveTimer = 0f;
            SaveCurrentPosition();
        }
    }

    void FixedUpdate()
    {
        // 물리 이동
        rb.velocity = moveInput * moveSpeed;
    }

    void OnDisable()
    {
        SaveCurrentPosition();
    }

    void OnDestroy()
    {
        SaveCurrentPosition();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveCurrentPosition();
        }
    }

    void OnApplicationQuit()
    {
        SaveCurrentPosition();
    }

    public void SaveCurrentPosition()
    {
        if (!hasStarted)
        {
            return;
        }

        GameProgress.SavePlayerPosition(transform.position);
    }
}
