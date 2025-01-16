using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5f;  // 내 속도
    private Rigidbody2D rb;  // 2D Rigidbody 컴포넌트
    //public GameObject otherPlayer;  // 다른 플레이어 게임 오브젝트

    //private Vector2 otherPlayerPosition;  // 다른 플레이어의 위치

    void Start()
    {
        // Rigidbody2D 컴포넌트를 자동으로 가져오기
        rb = GetComponent<Rigidbody2D>();

        // Rigidbody2D 설정
        rb.gravityScale = 0;  // 2D에서 중력 무시
    }

    //void Update()
    //{
    //    HandlePlayerMovement();  // 내 플레이어 이동 처리
    //    //UpdateOtherPlayerPosition();  // 다른 플레이어 위치 업데이트
    //}
    void FixedUpdate()
    {
        HandlePlayerMovement();
    }
    // 플레이어 이동 처리
    private void HandlePlayerMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");  // 방향키 또는 A/D
        float moveY = Input.GetAxisRaw("Vertical");    // 방향키 또는 W/S

        // 이동 방향 계산
        Vector2 moveDirection = new Vector2(moveX, moveY).normalized;

        // 이동: Rigidbody2D를 사용하여 위치 업데이트
        rb.MovePosition(rb.position + moveSpeed * Time.deltaTime * moveDirection);
    }


}
