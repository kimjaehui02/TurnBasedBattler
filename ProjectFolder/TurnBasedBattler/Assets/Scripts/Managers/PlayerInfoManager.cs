using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoManager : MonoBehaviour
{
    // �������� �ο����� ���� ID
    [SerializeField]
    private int PlayerId;

    // �������� ���� ID�� �÷��̾� ���� �ʱ�ȭ
    public void InitializePlayerInfo(int playerId)
    {
        PlayerId = playerId;
    }

    // �÷��̾� ID�� ��ȯ�ϴ� �Լ� (����� ��� ����)
    public int GetPlayerId()
    {
        return PlayerId;
    }
}
