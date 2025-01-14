using static UnityEditor.Progress;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel
{
    // 기본 정보
    public int PlayerId { get; set; } // 고유 ID
    public string PlayerName { get; set; } // 플레이어 이름
    public int Level { get; set; } // 레벨
    public int Experience { get; set; } // 경험치
    public string Class { get; set; } // 직업 (전사, 마법사 등)

    // 능력치
    public int Health { get; set; } // 체력
    public int Mana { get; set; } // 마나
    public int Strength { get; set; } // 힘
    public int Dexterity { get; set; } // 민첩
    public int Intelligence { get; set; } // 지능

    // 장비
    public List<Item> EquippedItems { get; set; } // 착용 중인 장비 리스트

    // 인벤토리
    public List<Item> Inventory { get; set; } // 인벤토리에 있는 아이템 리스트

    // 퀘스트 진행 상황
    //public List<Quest> Quests { get; set; } // 플레이어가 진행 중인 퀘스트

    // 추가 정보
    public int Gold { get; set; } // 소지금
    public Vector3 Position { get; set; } // 현재 위치
    public string LastLogin { get; set; } // 마지막 로그인 시간
}
