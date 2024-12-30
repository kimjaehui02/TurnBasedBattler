#ifndef SERVER_H
#define SERVER_H

#include <iostream>
#include <thread>
#include <vector>
#include <mutex>
#include <atomic>
#include <winsock2.h>
#include "SubServer.h"  // ServerState Ŭ���� ����
#include "ConnectionState.h"
#include "SubServerManager.h"
#include "SubServer.h"
#include <functional>

// Server�� SendToTCPClient �Լ� ������ Ÿ�� ����
using SendFunction = std::function<void(SOCKET, ConnectionState, const nlohmann::json&)>;

class Server {
public:
    Server(int port);  // ������
    ~Server();         // �Ҹ���

    void start();      // ���� ����
    void stop();       // ���� ����

private:
    void acceptConnections();  // Ŭ���̾�Ʈ ���� ó��
    //void handleClient(SOCKET clientSocket);  // Ŭ���̾�Ʈ ó��

    //void startConnections();

    void RunServerAsync(SOCKET clientSocket);

    // Ŭ���̾�Ʈ���� ����
    void SendToTCPClient(SOCKET clientSocket, ConnectionState connectionState, const nlohmann::json& messageData);
    void ReceiveFromTCPClient(SOCKET clientSocket, char* buffer, int bytesReceived);

    // UDP�������� ����
    //void SendToUDPServer();
    //void ReceiveFromUDPServer();

    //void HandleConnectionState();



    int port;  // ���� ��Ʈ
    SOCKET serverSocket;  // ���� ����
    std::atomic<bool> running;  // ���� ���� ����
    std::vector<std::thread> clientThreads;  // Ŭ���̾�Ʈ �ڵ鸵 �������
    std::mutex mtx;  // ��Ƽ������ ������ ���� ���ؽ�
    //SubServerManager serverState;  // ServerState �ν��Ͻ�

    // c#���� �������� ������ ����Ʈ

};

#endif
