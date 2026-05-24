using UnityEngine;

public class character_move : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // WASD + 방향키 입력
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(x, y).normalized;
    }

    void FixedUpdate()
    {
        // 물리 이동
        rb.velocity = moveInput * moveSpeed;
    }
}
