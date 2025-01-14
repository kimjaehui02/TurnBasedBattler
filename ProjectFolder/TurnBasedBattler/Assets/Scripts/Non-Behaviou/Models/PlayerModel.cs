using static UnityEditor.Progress;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel
{
    // �⺻ ����
    public int PlayerId { get; set; } // ���� ID
    public string PlayerName { get; set; } // �÷��̾� �̸�
    public int Level { get; set; } // ����
    public int Experience { get; set; } // ����ġ
    public string Class { get; set; } // ���� (����, ������ ��)

    // �ɷ�ġ
    public int Health { get; set; } // ü��
    public int Mana { get; set; } // ����
    public int Strength { get; set; } // ��
    public int Dexterity { get; set; } // ��ø
    public int Intelligence { get; set; } // ����

    // ���
    public List<Item> EquippedItems { get; set; } // ���� ���� ��� ����Ʈ

    // �κ��丮
    public List<Item> Inventory { get; set; } // �κ��丮�� �ִ� ������ ����Ʈ

    // ����Ʈ ���� ��Ȳ
    //public List<Quest> Quests { get; set; } // �÷��̾ ���� ���� ����Ʈ

    // �߰� ����
    public int Gold { get; set; } // ������
    public Vector3 Position { get; set; } // ���� ��ġ
    public string LastLogin { get; set; } // ������ �α��� �ð�
}
