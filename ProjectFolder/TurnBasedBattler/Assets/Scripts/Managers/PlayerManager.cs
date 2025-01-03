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

    void Update()
    {
        HandlePlayerMovement();  // �� �÷��̾� �̵� ó��
        //UpdateOtherPlayerPosition();  // �ٸ� �÷��̾� ��ġ ������Ʈ
    }

    // �÷��̾� �̵� ó��
    private void HandlePlayerMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");  // ����Ű �Ǵ� A/D
        float moveY = Input.GetAxisRaw("Vertical");    // ����Ű �Ǵ� W/S

        // �̵� ���� ���
        Vector2 moveDirection = new Vector2(moveX, moveY).normalized;

        // �̵�: Rigidbody2D�� ����Ͽ� ��ġ ������Ʈ
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.deltaTime);
    }

    // UDP�� ���� �ٸ� �÷��̾��� ��ġ�� ������Ʈ
    //public void UpdateOtherPlayerPosition(Vector2 newPosition)
    //{
    //    otherPlayerPosition = newPosition;
    //}

    // �ٸ� �÷��̾��� ��ġ�� ȭ�鿡 ������Ʈ
    //private void UpdateOtherPlayerPosition()
    //{
    //    if (otherPlayer != null)
    //    {
    //        // �ٸ� �÷��̾ �����ϴ� ���, �ش� ��ġ�� �̵�
    //        // �ε巴�� �̵���Ű�� ���� Lerp ���
    //        otherPlayer.transform.position = Vector2.Lerp(
    //            otherPlayer.transform.position,
    //            otherPlayerPosition,
    //            Time.deltaTime * moveSpeed);  // �̵� �ӵ��� ���� �ε巴�� �̵�
    //    }
    //}
}
