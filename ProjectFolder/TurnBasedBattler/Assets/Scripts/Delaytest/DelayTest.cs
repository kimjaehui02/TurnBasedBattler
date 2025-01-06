using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class DelayTest : MonoBehaviour
{
    private UdpClient udpClient;
    private IPEndPoint serverEndPoint;
    private const float sendInterval = 0.02f; // 좌표 전송 간격
    private float lastSendTime = 0f;

    public int testss;

    public GameObject gameObject1;
    public GameObject gameObject2;
    public int ports;

    void Start()
    {
        udpClient = new UdpClient(ports); // 클라이언트가 수신할 포트
        serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345); // 서버의 IP와 포트

        // 서버로 메시지 전송
        SendMessageToServer("안녕하세요, 서버!");

        // 코루틴을 시작하여 메시지를 수신
        StartCoroutine(ReceiveMessages());
    }

    void SendMessageToServer(object messageData)
    {
        // object를 string으로 변환한 후 바이트 배열로 변환
        string messageString = JsonConvert.SerializeObject(messageData); // 변경: 메시지 객체를 직렬화
        byte[] data = Encoding.UTF8.GetBytes(messageString);
        udpClient.Send(data, data.Length, serverEndPoint);
        //udpClient.Send(data, data.Length);
        Debug.Log("서버에 메시지 전송: " + messageString);
    }

    // 코루틴을 사용하여 서버로부터 메시지를 비동기적으로 수신
    IEnumerator ReceiveMessages()
    {
        while (true)
        {
            if (udpClient.Available > 0)
            {
                byte[] data = udpClient.Receive(ref serverEndPoint);
                string message = Encoding.UTF8.GetString(data);
                Debug.Log("서버로부터 받은 메시지: " + message);

                // 메시지를 JSON 객체로 변환
                try
                {
                    dynamic jsonMessage = JsonConvert.DeserializeObject(message);

                    // testss 값에 따라 위치 업데이트
                    if (testss == 0)
                    {
                        // client0의 데이터로 gameObject2 위치 업데이트
                        gameObject2.transform.position = new Vector3(
                            (float)jsonMessage.client1.x, // 명시적 타입 변환
                            (float)jsonMessage.client1.y, // 명시적 타입 변환
                            (float)jsonMessage.client1.z  // 명시적 타입 변환
                        );
                    }
                    else if (testss == 1)
                    {
                        // client1의 데이터로 gameObject1 위치 업데이트
                        gameObject1.transform.position = new Vector3(
                            (float)jsonMessage.client0.x, // 명시적 타입 변환
                            (float)jsonMessage.client0.y, // 명시적 타입 변환
                            (float)jsonMessage.client0.z  // 명시적 타입 변환
                        );
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("JSON 파싱 오류: " + ex.Message);
                }
            }
            yield return null; // 매 프레임마다 대기
        }
    }

    void OnApplicationQuit()
    {
        udpClient.Close();
    }

    private void Update()
    {
        DelayUpdate();
    }

    public void DelayUpdate()
    {
        if (testss == -1)
        {
            return;
        }
        float currentTime = Time.realtimeSinceStartup;
        if (currentTime - lastSendTime >= sendInterval)
        {
            dynamic messagein = null;
            if (testss == 0)
            {
                messagein = new
                {
                    numb = testss,
                    x = gameObject1.transform.position.x,
                    y = gameObject1.transform.position.y,
                    z = gameObject1.transform.position.z
                };

            }
            else
            {
                messagein = new
                {
                    numb = testss,
                    x = gameObject2.transform.position.x,
                    y = gameObject2.transform.position.y,
                    z = gameObject2.transform.position.z
                };
            }

            // 메시지를 직렬화하여 서버에 전송
            SendMessageToServer(messagein);

            lastSendTime = currentTime;
        }
    }
}
