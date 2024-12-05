// Server.h
#ifndef SERVER_H
#define SERVER_H

#include <iostream>
#include <thread>
#include <vector>
#include <mutex>
#include <atomic>
#include <winsock2.h>

class Server {
public:
    Server(int port);  // ������
    ~Server();         // �Ҹ���

    void start();      // ���� ����
    void stop();       // ���� ����

private:
    void acceptConnections();  // Ŭ���̾�Ʈ ���� ó��
    void handleClient(SOCKET clientSocket);  // Ŭ���̾�Ʈ ó��

    int port;  // ���� ��Ʈ
    SOCKET serverSocket;  // ���� ����
    std::atomic<bool> running;  // ���� ���� ����
    std::vector<std::thread> clientThreads;  // Ŭ���̾�Ʈ �ڵ鸵 �������
    std::mutex mtx;  // ��Ƽ������ ������ ���� ���ؽ�
};

#endif
