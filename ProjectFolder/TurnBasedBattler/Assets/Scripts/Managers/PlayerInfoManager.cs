using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoManager : MonoBehaviour
{
    // 서버에서 부여받은 고유 ID
    [SerializeField]
    private int PlayerId;

    // 서버에서 받은 ID로 플레이어 정보 초기화
    public void InitializePlayerInfo(int playerId)
    {
        PlayerId = playerId;
    }

    // 플레이어 ID를 반환하는 함수 (디버깅 등에서 유용)
    public int GetPlayerId()
    {
        return PlayerId;
    }
}
