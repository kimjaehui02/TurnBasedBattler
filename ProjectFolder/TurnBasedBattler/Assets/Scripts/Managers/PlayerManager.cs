using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5f;  // �� �ӵ�
    private Rigidbody2D rb;  // 2D Rigidbody ������Ʈ
    //public GameObject otherPlayer;  // �ٸ� �÷��̾� ���� ������Ʈ

    //private Vector2 otherPlayerPosition;  // �ٸ� �÷��̾��� ��ġ

    void Start()
    {
        // Rigidbody2D ������Ʈ�� �ڵ����� ��������
        rb = GetComponent<Rigidbody2D>();

        // Rigidbody2D ����
        rb.gravityScale = 0;  // 2D���� �߷� ����
    }

    //void Update()
    //{
    //    HandlePlayerMovement();  // �� �÷��̾� �̵� ó��
    //    //UpdateOtherPlayerPosition();  // �ٸ� �÷��̾� ��ġ ������Ʈ
    //}
    void FixedUpdate()
    {
        HandlePlayerMovement();
    }
    // �÷��̾� �̵� ó��
    private void HandlePlayerMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");  // ����Ű �Ǵ� A/D
        float moveY = Input.GetAxisRaw("Vertical");    // ����Ű �Ǵ� W/S

        // �̵� ���� ���
        Vector2 moveDirection = new Vector2(moveX, moveY).normalized;

        // �̵�: Rigidbody2D�� ����Ͽ� ��ġ ������Ʈ
        rb.MovePosition(rb.position + moveSpeed * Time.deltaTime * moveDirection);
    }


}
