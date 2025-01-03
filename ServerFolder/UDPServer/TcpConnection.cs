using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UDPServer.Manager;

namespace UDPServer
{
    class TcpConnection
    {
        PlayerManager playerManager;
        ObjectTransformManager objectTransformManager;


        public TcpConnection(PlayerManager playerManager, ObjectTransformManager objectTransformManager)
        {
            this.playerManager = playerManager;
            this.objectTransformManager = objectTransformManager;
        }
        #region 변수들

        // 동강동강긴급탈출
        int bytesReadss = -1;

        #region 메인서버의 정보들
        // private const string ServerIp = "127.0.0.1";  // 서버 IP
        // private const int ServerPort = 8080;  // 서버 포트
        private static NetworkStream _stream;
        private static TcpClient _client;
        #endregion

        #region 이 서버의 클라이언트들
        // private const string ServerIpMine = "127.0.0.1";  // 서버 IP
        // private const int ServerPortMine = 9090;  // 서버 포트
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
            //Console.WriteLine("public async Task RunServerAsync()");
            try
            {
                // 클라이언트가 연결되어 있을 때만 계속 데이터 수신
                while (_client?.Connected ?? false)
                {
                    //Console.WriteLine("while (_client?.Connected ?? false)");
                    // 데이터를 비동기적으로 수신
                    await ReceiveFromTCPServerAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"서버 실행 중 오류 발생: {ex.Message}");
            }
            //Console.WriteLine("public async Task RunServerAsync() END");
        }

        // 2. 리시브
        /// <summary>
        /// 메인서버측에게서 데이터를 받아오는 함수
        /// </summary>
        /// <returns></returns>
        public async Task ReceiveFromTCPServerAsync()
        {
            //Console.WriteLine("ReceiveFromTCPServerAsync()");

            byte[] buffer = new byte[1024];
            //Console.WriteLine("byte[] buffer = new byte[1024];");
            try
            {
                //Console.WriteLine("try");
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length); // 비동기적으로 데이터 수신
                //Console.WriteLine("int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length); // 비동기적으로 데이터 수신");
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

        #region 클라와의 tcp송신함수
        static void SendToTcpClient(TcpClient asyncToClient, ConnectionState connectionState, object messageData)
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
                NetworkStream stream = asyncToClient.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP 전송 중 오류 발생: {ex.Message}");
            }
        }
        #endregion 클라와의 tcp송신함수 끝

        #region 클라와의 tcp수신함수

        // 0. 런 에드 클라이언트

        /// <summary>
        /// 리스너의 신호를 받아서 1대1로 대응되는 비동기를 만들어줌
        /// </summary>
        /// <returns></returns>
        public async Task RunServerAsyncToAddClient()
        {
            //Console.WriteLine("public async Task RunServerAsyncToAddClient()");
            //Console.WriteLine($"RunServerAsyncToAddClient");
            try
            {
                // 모든 클라이언트와의 작업을 추적할 Task 리스트
                List<Task> clientTasks = new List<Task>();

                while (_listener.Server.IsBound)  // 서버가 꺼지지 않은 이상 계속 반복
                {

                    TcpClient client = _listener.AcceptTcpClient(); // 연결이 올 때까지 멈춘 상태

                    Console.WriteLine($"새로운 신호 도찪!!!!{client}");
                    Console.WriteLine($"새로운 신호 도찪!!!!{client}");
                    Console.WriteLine($"새로운 신호 도찪!!!!{client}");
                    Console.WriteLine($"새로운 신호 도찪!!!!{client}");
                    Console.WriteLine($"새로운 신호 도찪!!!!{client}");
                    Console.WriteLine($"새로운 신호 도찪!!!!{client}");
                    Console.WriteLine($"새로운 신호 도찪!!!!{client}");


                    Task clientTask = RunServerAsyncToClient(client);
                    clientTasks.Add(clientTask);
                }

                await Task.WhenAll(clientTasks);


            }
            catch (Exception ex)
            {
                Console.WriteLine($"0. 런 에드 클라이언트 중 오류 발생: {ex.Message}");
            }

            
        }

        // 1. 런

        /// <summary>
        /// 클라이언트와 1대1이 되는 비동기
        /// </summary>
        /// <param name="asyncToClient">대응시킬 클라이언트를 매개변수로</param>
        /// <returns></returns>
        public async Task RunServerAsyncToClient(TcpClient asyncToClient)
        {
            int id = -1;
            try
            {
                // 플레이어 추가
                id = playerManager.AddPlayer(asyncToClient);
                //id = test;


                Console.WriteLine($"playerManager id: {id}");
                //Console.WriteLine($"보내지는 id: {id}");
                //Console.WriteLine($"보내지는 id: {id}");
                //Console.WriteLine($"보내지는 id: {id}");
                //Console.WriteLine($"보내지는 id: {id}");


                SendToTcpClient(asyncToClient, ConnectionState.Connecting, new { playerId = id });
                // 클라이언트가 연결되어 있을 동안 데이터를 받음

                bytesReadss = -1;
                while (bytesReadss != 0)
                {
                    //Console.WriteLine($"0. 런 에드 클라이언트 중 오류 발생???: {asyncToClient.Connected}");
                    //Console.WriteLine($"0. 런 에드 클라이언트 중 오류 발생???: {asyncToClient.Connected}");
                    //Console.WriteLine($"0. 런 에드 클라이언트 중 오류 발생???: {asyncToClient.Connected}");
                    //Console.WriteLine($"0. 런 에드 클라이언트 중 오류 발생???: {asyncToClient.Connected}");
                    //Console.WriteLine($"0. 런 에드 클라이언트 중 오류 발생???: {asyncToClient.Connected}");

                    await ReceiveFromClientAsync(asyncToClient, id);
                    //SendToTcpClient(asyncToClient, ConnectionState.Connecting, null);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"1. 런 중 오류 발생: {ex.Message}");
            }
            finally
            {
 

                // 클라이언트 연결 종료 후, 플레이어 삭제
                if (id != -1)
                {
                    //id--;
                    objectTransformManager.DeleteUserData(id);
                    playerManager.DeletePlayer(id);

                    Console.WriteLine($"종료종료종료종료종료종료종료종료종료종료종료종료");
                    Console.WriteLine($"종료종료종료종료종료종료종료종료종료종료종료종료");
                    Console.WriteLine($"종료종료종료종료종료종료종료종료종료종료종료종료");
                    Console.WriteLine($"종료종료종료종료종료종료종료종료종료종료종료종료");
                    Console.WriteLine($"종료종료종료종료종료종료종료종료종료종료종료종료");


                }
            }
        }


        // 2. 리시브

        /// <summary>
        /// 클라이언트의 송신 한번마다 실행되는 비동기
        /// </summary>
        /// <param name="asyncToClient">신호를 받을 클라이언트</param>
        /// <returns></returns>
        public async Task ReceiveFromClientAsync(TcpClient asyncToClient, int id)
        {
            //Console.WriteLine("ReceiveFromTCPServerAsync()");

            byte[] buffer = new byte[1024];

            try
            {
                // 클라이언트 연결 대기
                Console.WriteLine("클라이언트 연결을 기다리는 중...");

                // 연결된 클라이언트의 NetworkStream 가져오기
                NetworkStream stream = asyncToClient.GetStream();

                // 데이터를 비동기적으로 수신
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);  // 비동기적으로 데이터 수신
                bytesReadss = bytesRead;



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
                            HandleConnectionStateToClient(message, id);  // 상태 처리
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
        private void HandleConnectionStateToClient(dynamic message, int id)
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
        #endregion 분배들 끝


        #endregion 클라와의 tcp연결함수 끝

        #endregion tcp송수신 끝

        #region 통신 시작

        public async Task StartConnection(string ServerIp, int ServerPort, int tcpPort, int udpPort)
        {

            // 메인서버와 연결하는 부분들
            try
            {

                // TCP 클라이언트 연결
                _client = new TcpClient(ServerIp, ServerPort);
                _stream = _client.GetStream();


                // tcp서버 연결
                _listener = new TcpListener(IPAddress.Parse(ServerIp), tcpPort);
                _listener.Start();

                // 서버와 연결 후 초기화 작업
                SendToTcpServer(ConnectionState.Connecting, new { playerName = "udpServer", ServerIp, tcpPort, udpPort });

                // TCP 데이터 수신 시작
                // 두 비동기 함수 동시에 실행

                // 메인서버랑 주고받는 놈
                var runServerAsyncTask = RunServerAsync();  // 비동기 실행 준비

                // 클라들이랑 주고받는 놈
                var runServerAsyncToAddClientTask = RunServerAsyncToAddClient();  // 비동기 실행 준비

                Console.WriteLine("두 함수가 동시에 실행됨!");
                // 두 비동기 함수 모두 완료될 때까지 기다림
                await Task.WhenAll(runServerAsyncTask, runServerAsyncToAddClientTask);

                Console.WriteLine("두 함수가 동시에 실행됨!");
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
