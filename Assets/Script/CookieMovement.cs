using UnityEngine;

public class CookieMovement : MonoBehaviour
{
    public float moveSpeed = 3f; // 이동 속도

    private Animator animator;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 오른쪽으로 이동
        rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);

        // 뛰는 애니메이션 계속 유지
        animator.Play("New Run"); // 여기에 애니메이션 이름 정확히!
    }
}
