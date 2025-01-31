﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UDPServer.Manager;

namespace UDPServer
{
    class UdpConnection
    {
        PlayerManager playerManager;
        ObjectTransformManager objectTransformManager;
        public UdpConnection(PlayerManager playerManager, ObjectTransformManager objectTransformManager)
        {
            this.playerManager = playerManager;
            this.objectTransformManager = objectTransformManager;
        }

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

        #region 송신부
        // 서버에서 클라이언트로 데이터를 송신하는 메서드
        public void SendToUDPClient(UdpClient udpClient, IPEndPoint remoteEP, ConnectionState connectionState, object messageData)
        {
            //Console.WriteLine($"SendToUDPClient의 보내는 remoteEP: {remoteEP}");

            try
            {
                // 메시지 구성
                var message = new
                {
                    connectionState = connectionState.ToString(),
                    data = messageData
                };

                // 객체를 JSON 문자열로 직렬화
                string jsonMessage = JsonConvert.SerializeObject(message, Formatting.Indented);

                // 디버깅용으로 JSON 출력
                //Console.WriteLine($"보내는 JSON: {jsonMessage}");

                // UDP를 통해 클라이언트로 메시지 전송
                //byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
                byte[] data = JsonCompressionManager.CompressJson(jsonMessage);
                udpClient.Send(data, data.Length, remoteEP);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP 전송 중 오류 발생: {ex.Message}");
            }
        }
        #endregion

        #region 수신부
        // 클라이언트로부터 데이터를 수신하는 메서드
        public async Task ReceiveFromUDPClient(UdpClient udpServer)
        {
            //Console.WriteLine($"ReceiveFromUDPClient시작점 : ");

            try
            {
                // 데이터 수신
                UdpReceiveResult result = await udpServer.ReceiveAsync();
                byte[] data = result.Buffer;  // UdpReceiveResult에서 Buffer 속성으로 데이터 추출
                IPEndPoint remoteEP = result.RemoteEndPoint;  // 클라이언트의 IP와 포트 번호를 가진 IPEndPoint

                //string json = Encoding.UTF8.GetString(data);
                string json = JsonCompressionManager.DecompressJson(data);

                // 수신한 JSON 데이터를 콘솔에 출력
                //Console.WriteLine("Received data: " + json);

                // JSON 문자열을 Newtonsoft.Json으로 처리
                try
                {
                    var message = JsonConvert.DeserializeObject<dynamic>(json);
                    //Console.WriteLine($"ReceiveFromUDPClient트라이성공 message : {message}");

                    if (message != null)
                    {
                        // 'connectionState'에 따라 처리
                        HandleConnectionState(udpServer, remoteEP, in message);
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
            catch //(Exception ex)
            {
                //Console.WriteLine($"Error receiving data: {ex.Message}");
            }
            //Console.WriteLine($"ReceiveFromUDPClient종료지점 : ");
        }
        #endregion

        #region 데이터 처리
        // 클라이언트에서 보낸 메시지의 connectionState에 따라 처리
        private void HandleConnectionState(UdpClient udpClient, IPEndPoint remoteEP, in dynamic message)
        {
            string connectionState = message.connectionState;

            //Console.WriteLine($"HandleConnectionState : {connectionState}");
            //Console.WriteLine($"HandleConnectionState : {connectionState}");
            //Console.WriteLine($"HandleConnectionState : {connectionState}");

            //Console.WriteLine($"HandleConnectionState의 보내는 remoteEP: {remoteEP}");
            switch (connectionState)
            {
                case "Connecting":
                    HandleConnecting(udpClient, remoteEP, in message);
                    break;

                case "DataSyncing":
                    HandleDataSyncing(udpClient, remoteEP, in message);
                    break;

                case "Disconnecting":
                    HandleDisconnecting(udpClient, remoteEP, in message);
                    break;

                case "Error":
                    HandleError(udpClient, remoteEP, in message);
                    break;

                default:
                    Console.WriteLine($"Unknown connection state: {connectionState}");
                    break;
            }
        }

        private void HandleConnecting(UdpClient udpClient, IPEndPoint remoteEP, in dynamic message)
        {
            Console.WriteLine("Handling Connecting state...");
            // 여기에서 플레이어 연결 처리 로직 추가
            //int id = playerManager.AddPlayer(remoteEP);
            //Console.WriteLine($"HandleConnecting의 보내는 remoteEP: {remoteEP}");
            //SendToUDPClient(udpClient, remoteEP, ConnectionState.Connecting, new { playerId = 0 });
        }

        private void HandleDataSyncing(UdpClient udpClient, IPEndPoint remoteEP, in dynamic message)
        {
            //Console.WriteLine("Handling Data Syncing state...");
            // 데이터 동기화 처리 로직 추가

            //Console.WriteLine($"message.data : {message.data}");
            //Console.WriteLine($"message.data : {message.data}");
            //Console.WriteLine($"message.data : {message.data}");
            //Console.WriteLine($"message.data : {message.data}");
            objectTransformManager.UpdateObjectTransformsForPlayer(in message.data);
            //Console.WriteLine("objectTransformManager.UpdateObjectTransformsForPlayer(message.data);");

            string allObject = objectTransformManager.ToAllJson();
            //Console.WriteLine("dynamic allObject = objectTransformManager.ToAllJson();");

            //Console.WriteLine($"allObject");
            //Console.WriteLine($"allObject");
            //Console.WriteLine($"allObject");
            //Console.WriteLine($"allObject");
            //Console.WriteLine($"\n{allObject}\n");

            //Console.WriteLine($"allObject");
            //Console.WriteLine($"allObject");
            //Console.WriteLine($"allObject");
            //Console.WriteLine($"allObject");


            //Console.WriteLine($"allObject : {allObject}");
            //Console.WriteLine($"allObject : {allObject}");
            //Console.WriteLine($"allObject : {allObject}");
            SendToUDPClient(udpClient, remoteEP, ConnectionState.DataSyncing, allObject);
            //Console.WriteLine("SendToUDPClient(udpClient, remoteEP, ConnectionState.DataSyncing, allObject);");

        }

        private void HandleDisconnecting(UdpClient udpClient, IPEndPoint remoteEP, in dynamic message)
        {
            Console.WriteLine("Handling Disconnecting state...");
            //Console.WriteLine($"message  ...{message}");
            // 연결 종료 처리 로직 추가
            // message.data.playerId가 실제로 int로 변환되는지 확인
            //int playerId = Convert.ToInt32(message.data.playerId);  // 강제 형변환

            // 연결 종료 처리 로직
            //playerManager.DeletePlayer(playerId);

        }

        private void HandleError(UdpClient udpClient, IPEndPoint remoteEP, in dynamic message)
        {
            Console.WriteLine("Handling Error state...");
            // 오류 처리 로직 추가
        }
        #endregion

        #region 서버 실행

        // 서버 실행 메서드
        public async Task RunServerAsync(UdpClient udpServer)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //Console.WriteLine($"1. RunServerAsync시작지점 : ");
            try
            {
                /*List<Task> clientTasks = new List<Task>();
                int maxConcurrentTasks = 4;*/
                //Console.WriteLine($"2. try : ");
                while (true)
                {
                    //if (stopwatch.IsRunning)
                    //{
                    //    stopwatch.Stop();
                    //    Console.WriteLine($"여기 다시 시작됨! 이전 실행부터 경과 시간: {stopwatch.ElapsedMilliseconds} ms");
                    //}
                    //stopwatch.Restart(); // 스톱워치를 재시작하여 다음 경과 시간을 측정

                    // 여러 비동기 작업을 동시에 처리
                    /*                    if (clientTasks.Count < maxConcurrentTasks)  // 최대 동시 실행 작업 수가 되지 않으면
                                        {
                                            Task receiveTask = ReceiveFromUDPClient(udpServer);
                                            clientTasks.Add(receiveTask);
                                        }

                                        // 비동기 작업들이 완료될 때까지 기다리되, 동시 실행되는 작업 수를 제한
                                        if (clientTasks.Count >= maxConcurrentTasks)
                                        {
                                            // 하나의 작업이 완료될 때까지 기다리고, 완료된 작업을 처리
                                            Task completedTask = await Task.WhenAny(clientTasks);
                                            clientTasks.Remove(completedTask);  // 완료된 작업은 리스트에서 제거
                                        }
                    */

                    /*var serverTask1 = Task.Run(() => ReceiveFromUDPClient(udpServer));
                    var serverTask2 = Task.Run(() => ReceiveFromUDPClient(udpServer));*/
                    await ReceiveFromUDPClient(udpServer);
                    
                    //await Task.WhenAll(serverTask1, serverTask2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"서버 오류 발생: {ex.Message}");
            }
            //Console.WriteLine($"4. RunServerAsync종료지점 : ");
        }

        #endregion

        #region 통신 시작

        public async Task StartConnection(string ServerIp, int ServerPort, int port)
        {
            UdpClient udpServer = new UdpClient(port); // 서버 포트 지정
            //Console.WriteLine("UdpClient udpServer = new UdpClient(Port); // 서버 포트 지정");

            await RunServerAsync(udpServer);
        }

        #endregion 통신 시작 끝
    }
}
