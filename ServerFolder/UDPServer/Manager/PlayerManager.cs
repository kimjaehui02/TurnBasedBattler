using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

class PlayerManager
{
    private static Dictionary<int, PlayerConnection> players = new Dictionary<int, PlayerConnection>(); // 플레이어 관리
    private static Queue<int> availablePlayerIds = new Queue<int>(); // 삭제된 플레이어 ID 관리
    private static int nextPlayerId = 0; // 자동으로 할당되는 플레이어 ID

    // 플레이어 추가 함수 (빈 ID로 채움)
    public int AddPlayer(TcpClient asyncToClient)
    {
        int playerId;

        // 삭제된 ID가 있으면 해당 ID를 사용하고, 없으면 새로운 ID를 사용
        if (availablePlayerIds.Count > 0)
        {
            playerId = availablePlayerIds.Dequeue(); // 큐에서 ID를 꺼냄
        }
        else
        {
            playerId = nextPlayerId; // 새로운 ID 사용
            nextPlayerId++; // ID를 자동으로 증가
        }

        players.Add(playerId, new PlayerConnection(asyncToClient));
        Console.WriteLine($"Player added with ID: {playerId}");
        Console.WriteLine($"Player added with ID: {playerId}");
        Console.WriteLine($"Player added with ID: {playerId}");
        Console.WriteLine($"Player added with ID: {playerId}");
        Console.WriteLine($"nextPlayerId with ID: {nextPlayerId}");
        Console.WriteLine($"nextPlayerId with ID: {nextPlayerId}");
        Console.WriteLine($"nextPlayerId with ID: {nextPlayerId}");
        Console.WriteLine($"nextPlayerId with ID: {nextPlayerId}");
        Console.WriteLine($"players.Count: {players.Count}");
        Console.WriteLine($"players.Count: {players.Count}");
        Console.WriteLine($"players.Count: {players.Count}");
        Console.WriteLine($"players.Count: {players.Count}");
        Console.WriteLine($"players.Count: {players.Count}");

        return playerId;
    }

    // 플레이어 삭제 함수 (ID로 플레이어 삭제)
    public void DeletePlayer(int playerId)
    {
        if (players.ContainsKey(playerId))
        {
            players.Remove(playerId);
            availablePlayerIds.Enqueue(playerId); // 삭제된 ID를 큐에 추가
            Console.WriteLine($"Player with ID: {playerId} has been removed.");
            Console.WriteLine($"Player with ID: {playerId} has been removed.");
            Console.WriteLine($"Player with ID: {playerId} has been removed.");
            Console.WriteLine($"Player with ID: {playerId} has been removed.");
            Console.WriteLine($"players.Count: {players.Count}");
            Console.WriteLine($"players.Count: {players.Count}");
            Console.WriteLine($"players.Count: {players.Count}");
            Console.WriteLine($"players.Count: {players.Count}");
            Console.WriteLine($"players.Count: {players.Count}");
        }
        else
        {
            Console.WriteLine("Player ID not found.");
        }
    }

    public void UpdateTransform()
    {

    }

}
