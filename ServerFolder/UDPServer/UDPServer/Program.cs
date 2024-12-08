using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // UDP 클라이언트를 생성하고 포트 11000에 바인딩
            UdpClient udpServer = new UdpClient(8080);

            Console.WriteLine("UDP 서버가 시작되었습니다. 클라이언트 메시지를 기다립니다...");

            // 서버가 종료되지 않도록 계속 실행
            while (true)
            {
                // 클라이언트로부터 메시지를 수신
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 11000);
                byte[] receivedData = udpServer.Receive(ref remoteEndPoint);
                string receivedMessage = Encoding.UTF8.GetString(receivedData);

                Console.WriteLine($"수신된 메시지: {receivedMessage}");

                // 메시지를 처리하고 응답 생성 (예시: 받은 메시지를 그대로 반환)
                string responseMessage = $"서버 응답: {receivedMessage}";
                byte[] responseData = Encoding.UTF8.GetBytes(responseMessage);

                // 응답 전송
                udpServer.Send(responseData, responseData.Length, remoteEndPoint);
                Console.WriteLine($"응답 메시지 전송: {responseMessage}");
            }
        }
    }
}
