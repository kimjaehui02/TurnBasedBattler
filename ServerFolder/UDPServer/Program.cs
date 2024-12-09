using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

class UdpServer
{
    #region 통신용 변수들
    private const int Port = 7777; // 서버 포트
    #endregion

    private static Dictionary<int, Player> players = new Dictionary<int, Player>(); // 플레이어 관리
    private static Dictionary<int, IPEndPoint> playerEndpoints = new Dictionary<int, IPEndPoint>(); // 플레이어의 IP와 포트 관리
    private static Queue<int> availablePlayerIds = new Queue<int>(); // 사용되지 않은 플레이어 ID 관리
    private static int nextPlayerId = 1; // 다음 플레이어 ID

    static async Task Main(string[] args)
    {
        Console.WriteLine("UDP 서버 시작...");
        UdpClient udpServer = new UdpClient(Port);
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);


        // CancellationTokenSource 생성
        var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;

        // 서버 작업 비동기로 실행
        var serverTask = RunServerAsync(udpServer, remoteEP, token);

        // 취소 요청을 위한 예시
        Console.WriteLine("서버를 종료하려면 'Enter'를 누르세요.");
        Console.ReadLine();
        cancellationTokenSource.Cancel();

        // 서버 작업이 종료될 때까지 기다림
        await serverTask;
        Console.WriteLine("서버가 종료되었습니다.");
    }

    static async Task RunServerAsync(UdpClient udpServer, IPEndPoint remoteEP, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                // 데이터 수신
                byte[] data = await Task.Run(() => udpServer.Receive(ref remoteEP), token);
                string message = Encoding.UTF8.GetString(data);

                // JSON 데이터를 디시리얼화해서 객체로 변환
                var clientRequest = JsonConvert.DeserializeObject<ClientRequest>(message); // JsonConvert로 변경
                Console.WriteLine($"수신: {clientRequest.action} from {remoteEP} (PlayerId: {clientRequest.playerId})");
                Console.WriteLine("받은 메세지의 내용입니다: " + message);

                // 플레이어 ID를 요청하는 메시지 처리
                if (clientRequest.action == "GET_ID")
                {
                    // 새로운 플레이어에 대해 ID 할당
                    int newPlayerId = AssignPlayerId(remoteEP);

                    // ID 응답
                    var response = new ServerResponse
                    {
                        command = "Assigned ID",
                        data = newPlayerId.ToString()
                    };

                    string responseJson = JsonConvert.SerializeObject(response); // JsonConvert로 변경
                    byte[] responseData = Encoding.UTF8.GetBytes(responseJson);
                    udpServer.Send(responseData, responseData.Length, remoteEP);
                    Console.WriteLine($"응답: {responseJson} to {remoteEP}");

                    // 새로운 플레이어가 추가되었으면, 다른 모든 플레이어에게 현재 상태를 전송
                    SendPlayerListToAllPlayers(udpServer);
                }
                // 플레이어 위치 업데이트 메시지 처리
                else if (clientRequest.action == "UPDATE_POSITION")
                {
                    // 플레이어 위치 갱신
                    if (clientRequest.playerId.HasValue)  // playerId가 null이 아니면
                    {
                        int playerId = clientRequest.playerId.Value;  // playerId를 직접 사용
                        if (players.ContainsKey(playerId))
                        {
                            // x와 y도 null 체크 후 처리
                            if (clientRequest.x.HasValue && clientRequest.y.HasValue)
                            {
                                players[playerId].X = clientRequest.x.Value;
                                players[playerId].Y = clientRequest.y.Value;
                            }
                            else
                            {
                                Console.WriteLine("위치 정보가 없습니다.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Player not found!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("playerId가 유효하지 않습니다.");
                    }

                    // 현재 상태를 모든 클라이언트에게 전송
                    SendPlayerListToAllPlayers(udpServer);
                }
                // 종료 신호 처리
                else if (clientRequest.action == "SHUTDOWN")
                {
                    Console.WriteLine("(clientRequest.action == SHUTDOWN)");
                    if (clientRequest.playerId.HasValue)
                    {
                        Console.WriteLine("(if (clientRequest.playerId.HasValue)");
                        int playerId = clientRequest.playerId.Value;
                        if (players.ContainsKey(playerId))
                        {
                            // 플레이어 제거
                            players.Remove(playerId);
                            playerEndpoints.Remove(playerId);
                            Console.WriteLine($"플레이어 {playerId} 종료 신호 수신, 제거되었습니다.");

                            // 종료된 클라이언트에게 확인 응답 전송
                            var response = new ServerResponse
                            {
                                command = $"Player {playerId} has been removed."
                            };
                            string responseJson = JsonConvert.SerializeObject(response); // JsonConvert로 변경
                            byte[] responseData = Encoding.UTF8.GetBytes(responseJson);
                            udpServer.Send(responseData, responseData.Length, remoteEP);

                            // 모든 클라이언트에게 갱신된 플레이어 리스트 전송
                            SendPlayerListToAllPlayers(udpServer);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"오류 발생: {ex.Message}");
            }
        }
    }

    static int AssignPlayerId(IPEndPoint remoteEP)
    {
        int playerId;

        // 사용되지 않는 ID가 있으면 그것을 사용하고, 없으면 새로운 ID를 생성
        if (availablePlayerIds.Count > 0)
        {
            playerId = availablePlayerIds.Dequeue();
        }
        else
        {
            playerId = nextPlayerId++;
        }

        players[playerId] = new Player { Id = playerId, X = 0, Y = 0 }; // 초기 위치 0,0
        playerEndpoints[playerId] = remoteEP; // 클라이언트의 IP와 포트 저장
        return playerId;
    }

    static void SendPlayerListToAllPlayers(UdpClient udpServer)
    {
        // 모든 플레이어의 위치 리스트를 JSON으로 변환
        var playerList = new PlayerList();
        foreach (var player in players.Values)
        {
            playerList.players.Add(player);
        }

        string json = JsonConvert.SerializeObject(playerList); // JsonConvert로 변경

        // 모든 플레이어에게 전송 (자기 자신에게는 전송하지 않음)
        foreach (var player in playerEndpoints)
        {
            // 자기 자신에게 전송하지 않기 위해 player.Key는 playerId
            IPEndPoint remoteEP = player.Value;
            byte[] data = Encoding.UTF8.GetBytes(json);

            if (remoteEP.Address.ToString() != "127.0.0.1") // 자기 자신에게 응답을 방지
            {
                udpServer.Send(data, data.Length, remoteEP);
            }
        }
    }

    #region json선언부
    public enum ConnectionState
    {
        Default,      // 기본 상태
        Connecting,   // 연결 시도 중
        DataSyncing,  // 데이터 동기화 중
        Disconnecting,// 연결 종료 시도 중
        Error         // 오류 발생
    }

    #endregion




}
