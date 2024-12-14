using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UDPServer
{
    class TcpConnection
    {
        #region 변수들

        #region 메인서버의 정보들
        private const string ServerIp = "127.0.0.1";  // 서버 IP
        private const int ServerPort = 8080;  // 서버 포트
        private static NetworkStream _stream;
        private static TcpClient _client;
        #endregion

        #region 이 서버의 클라이언트들
        private const string ServerIpMine = "127.0.0.1";  // 서버 IP
        private const int ServerPortMine = 9090;  // 서버 포트
        private List<TcpClient> clients = new List<TcpClient>();
        //private static TcpClient _clientMine;
        private TcpListener _listener;
        #endregion


        #endregion

        #region json선언부
        public enum ConnectionState
        {
            Default,      // 기본 상태
            Connecting,   // 연결 시도 중
            DataSyncing,  // 데이터 동기화 중
            Disconnecting,// 연결 종료 시도 중
            Error,        // 오류 발생
            TcpToUdp      // tcp에서 udp로 이동시킵니다
        }
        #endregion


        #region tcp송수신



        #region tcp송신함수
        static void SendToTcpServer(ConnectionState connectionState, object messageData)
        {
            try
            {
                // 메시지 구성
                var message = new
                {
                    connectionState = connectionState.ToString(), // Enum 값을 문자열로 변환
                    data = messageData
                };

                // 직렬화 시 무한 참조 방지 설정
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                // 객체를 JSON 문자열로 직렬화
                string jsonMessage = JsonConvert.SerializeObject(message, Formatting.Indented, settings);

                // 디버깅용으로 JSON 출력
                /*Console.WriteLine($"보내는 JSON: {jsonMessage}");*/

                // TCP를 통해 서버로 메시지 전송
                byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
                _stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP 전송 중 오류 발생: {ex.Message}");
            }
        }
        #endregion


        #region tcp수신함수

        // 1. 런
        public async Task RunServerAsync()
        {
            try
            {
                // 클라이언트가 연결되어 있을 때만 계속 데이터 수신
                while (_client?.Connected ?? false)
                {
                    // 데이터를 비동기적으로 수신
                    await ReceiveFromTCPServerAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"서버 실행 중 오류 발생: {ex.Message}");
            }
        }

        // 2. 리시브
        /// <summary>
        /// 메인서버측에게서 데이터를 받아오는 함수
        /// </summary>
        /// <returns></returns>
        public async Task ReceiveFromTCPServerAsync()
        {
            Console.WriteLine("ReceiveFromTCPServerAsync()");

            byte[] buffer = new byte[1024];

            try
            {
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length); // 비동기적으로 데이터 수신

                if (bytesRead > 0)
                {
                    byte[] data = new byte[bytesRead];
                    Array.Copy(buffer, 0, data, 0, bytesRead);

                    // 받은 데이터를 UTF-8 문자열로 변환
                    string json = Encoding.UTF8.GetString(data);

                    // JSON 처리
                    try
                    {
                        var message = JsonConvert.DeserializeObject<dynamic>(json);
                        if (message != null)
                        {
                            HandleConnectionState(message);  // 상태 처리
                        }
                        else
                        {
                            Console.WriteLine("Failed to parse JSON.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing JSON: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading data: {ex.Message}");

            }

        }

        // 3. 분배
        #region 분배들
        private void HandleConnectionState(dynamic message)
        {
            string connectionState = message.connectionState;

            switch (connectionState)
            {
                case "Connecting":
                    HandleConnecting(message);
                    break;

                case "DataSyncing":
                    HandleDataSyncing(message);
                    break;

                case "Disconnecting":
                    HandleDisconnecting(message);
                    break;

                case "Error":
                    HandleError(message);
                    break;

                default:
                    Console.WriteLine($"Unknown connection state: {connectionState}");
                    break;
            }
        }


        private void HandleConnecting(dynamic message)
        {
            Console.WriteLine("Handling Connecting state...");

        }

        private void HandleDataSyncing(dynamic message)
        {
            Console.WriteLine("Handling Data Syncing state...");
        }

        private void HandleDisconnecting(dynamic message)
        {
            Console.WriteLine("Handling Disconnecting state...");
        }

        private void HandleError(dynamic message)
        {
            Console.WriteLine("Handling Error state...");
        }
        #endregion

        #endregion tcp수신함수 끝


        #region 클라와의 tcp연결함수

        // 0. 런 올 클라이언트
        public async Task RunServerAsyncToAllClient()
        {
            List<Task> clientTasks = new List<Task>();

            // 여러 클라이언트 연결을 처리
            foreach (var client in clients)
            {
                // 각 클라이언트에 대해 비동기적으로 데이터 수신
                clientTasks.Add(RunServerAsyncToClient(client));
            }

            // 모든 클라이언트의 작업을 동시에 처리
            await Task.WhenAll(clientTasks);
        }

        // 1. 런
        public async Task RunServerAsyncToClient(TcpClient asyncToClient)
        {
            try
            {
                while (asyncToClient == null || !asyncToClient.Connected)  // 서버가 꺼지지 않은 이상 계속 반복
                {
                    await ReceiveFromClientAsync(asyncToClient);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"클라이언트 연결 대기 중 오류 발생: {ex.Message}");
            }
        }

        // 2. 리시브
        public async Task ReceiveFromClientAsync(TcpClient asyncToClient)
        {
            Console.WriteLine("ReceiveFromTCPServerAsync()");

            byte[] buffer = new byte[1024];

            try
            {
                // 클라이언트 연결 대기
                Console.WriteLine("클라이언트 연결을 기다리는 중...");

                // 연결된 클라이언트의 NetworkStream 가져오기
                NetworkStream stream = asyncToClient.GetStream();

                // 데이터를 비동기적으로 수신
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);  // 비동기적으로 데이터 수신


                if (bytesRead > 0)
                {
                    byte[] data = new byte[bytesRead];
                    Array.Copy(buffer, 0, data, 0, bytesRead);

                    // 받은 데이터를 UTF-8 문자열로 변환
                    string json = Encoding.UTF8.GetString(data);

                    // JSON 처리
                    try
                    {
                        var message = JsonConvert.DeserializeObject<dynamic>(json);
                        if (message != null)
                        {
                            HandleConnectionStateToClient(message);  // 상태 처리
                        }
                        else
                        {
                            Console.WriteLine("Failed to parse JSON.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing JSON: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading data: {ex.Message}");

            }

        }


        // 3. 분배
        #region 분배들
        private void HandleConnectionStateToClient(dynamic message)
        {
            string connectionState = message.connectionState;

            switch (connectionState)
            {
                case "Connecting":
                    HandleConnecting(message);
                    break;

                case "DataSyncing":
                    HandleDataSyncing(message);
                    break;

                case "Disconnecting":
                    HandleDisconnecting(message);
                    break;

                case "Error":
                    HandleError(message);
                    break;

                default:
                    Console.WriteLine($"Unknown connection state: {connectionState}");
                    break;
            }
        }


        private void HandleConnectingToClient(dynamic message)
        {
            Console.WriteLine("Handling Connecting state...");

        }

        private void HandleDataSyncingToClient(dynamic message)
        {
            Console.WriteLine("Handling Data Syncing state...");
        }

        private void HandleDisconnectingToClient(dynamic message)
        {
            Console.WriteLine("Handling Disconnecting state...");
        }

        private void HandleErrorToClient(dynamic message)
        {
            Console.WriteLine("Handling Error state...");
        }
        #endregion


        #endregion 클라와의 tcp연결함수 끝

        #endregion tcp송수신 끝

        #region 통신 시작

        public async Task StartConnection()
        {
            // 메인서버와 연결하는 부분들
            try
            {
                // TCP 클라이언트 연결
                _client = new TcpClient(ServerIp, ServerPort);
                _stream = _client.GetStream();

                // tcp서버 연결
                _listener = new TcpListener(IPAddress.Parse(ServerIp), ServerPortMine);

                // 서버와 연결 후 초기화 작업
                SendToTcpServer(ConnectionState.Connecting, new { playerName = "Player1" });

                // TCP 데이터 수신 시작
                await RunServerAsync();


            }
            catch (Exception ex)
            {
                Console.WriteLine($"서버 연결 중 오류 발생: {ex.Message}");
            }

            // 클라이언트와 연결하는 부분들
            // 작성중...



        }

        #endregion

    }
    
}
