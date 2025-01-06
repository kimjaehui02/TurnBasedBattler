using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace SimulationClass
{
    // 클라이언트 데이터 구조를 클래스 형태로 정의
    class ClientData
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    class Issue2
    {
        public void TestMain()
        {
            UdpClient udpServer = new UdpClient(12345); // 서버 포트 12345
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12346); // 클라이언트와 동일한 IP와 포트

            Console.WriteLine("UDP 서버 시작...");

            // 각 클라이언트의 데이터를 저장할 변수
            var clientData = new
            {
                client0 = new ClientData(),
                client1 = new ClientData()
            };

            while (true)
            {
                // 클라이언트로부터 메시지 받기
                byte[] data = udpServer.Receive(ref endPoint);
                string message = Encoding.UTF8.GetString(data);
                Console.WriteLine($"수신된 메시지: {message}");

                // JSON 메시지를 파싱하여 각 클라이언트의 데이터 저장
                try
                {
                    dynamic jsonMessage = JsonConvert.DeserializeObject(message);
                    int numb = jsonMessage.numb;

                    // numb 값에 따라 각 클라이언트의 위치 업데이트
                    if (numb == 0)
                    {
                        clientData.client0.x = jsonMessage.x;
                        clientData.client0.y = jsonMessage.y;
                        clientData.client0.z = jsonMessage.z;
                    }
                    else if (numb == 1)
                    {
                        clientData.client1.x = jsonMessage.x;
                        clientData.client1.y = jsonMessage.y;
                        clientData.client1.z = jsonMessage.z;
                    }

                    Console.WriteLine($"클라이언트 0 데이터: {clientData.client0.x}, {clientData.client0.y}, {clientData.client0.z}");
                    Console.WriteLine($"클라이언트 1 데이터: {clientData.client1.x}, {clientData.client1.y}, {clientData.client1.z}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("JSON 파싱 오류: " + ex.Message);
                }

                // 클라이언트 데이터 병합
                var mergedData = new
                {
                    client0 = clientData.client0,
                    client1 = clientData.client1
                };

                // 병합된 데이터를 JSON 형식으로 변환
                string responseMessage = JsonConvert.SerializeObject(mergedData);
                byte[] responseData = Encoding.UTF8.GetBytes(responseMessage);

                // 병합된 데이터를 클라이언트로 전송
                udpServer.Send(responseData, responseData.Length, endPoint);
                Console.WriteLine("응답 메시지 전송: " + responseMessage);
            }
        }
    }
}
