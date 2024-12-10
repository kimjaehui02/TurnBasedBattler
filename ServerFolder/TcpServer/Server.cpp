#include "Server.h"  // Server Ŭ���� ���Ǹ� ������ ��� ����
#include <iostream>  // �ܼ� ������� ���� ���̺귯��
#include <thread>    // ��Ƽ������ ó���� ���� ���̺귯��
#include <winsock2.h>  // Windows ���� API�� ����ϱ� ���� ���̺귯��
#pragma warning(disable: 28020)
// json.hpp�� ���Ե� �ڵ�
#include "json.hpp"
#pragma warning(default: 28020) // �ٽ� ��� Ȱ��ȭ

#include "UpdateHandler.h"


// Server ������
Server::Server(int port)
    : port(port), running(false), serverSocket(INVALID_SOCKET) { // ��� ���� �ʱ�ȭ
    WSADATA wsaData; // Winsock �ʱ�ȭ�� ���� ����ü
    // Winsock �ʱ�ȭ
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {  // �����ϸ� ���� �޽��� ��� �� ����
        std::cerr << "WSAStartup failed." << std::endl;
        return;
    }
    std::cout << "Winsock initialized." << std::endl; // �ʱ�ȭ ���� �޽���
}

// Server �Ҹ���
Server::~Server() {
    WSACleanup();  // Winsock ����: ��� �� �ݵ�� ȣ���ؾ� ��
    std::cout << "Winsock cleaned up." << std::endl; // ���� �Ϸ� �޽���
}

// Server ����
void Server::start() {
    // ���� ���� ����: IPv4(AF_INET), TCP(SOCK_STREAM) ���
    serverSocket = socket(AF_INET, SOCK_STREAM, 0);
    if (serverSocket == INVALID_SOCKET) { // ���� ���� ���� �� ���� �޽��� ���
        std::cerr << "Failed to create socket. Error: " << WSAGetLastError() << std::endl;
        return;
    }

    // ���� �ּ� ���� ����ü
    sockaddr_in serverAddr = {};               // ����ü �ʱ�ȭ
    serverAddr.sin_family = AF_INET;           // IPv4 �������� ���
    serverAddr.sin_addr.s_addr = INADDR_ANY;   // ��� ��Ʈ��ũ �������̽����� ��û�� ����
    serverAddr.sin_port = htons(port);         // ��Ʈ�� ��Ʈ��ũ ����Ʈ ������ ����

    // ���� ���Ͽ� �ּ� �� ��Ʈ ���ε�
    if (bind(serverSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        std::cerr << "Binding failed. Error: " << WSAGetLastError() << std::endl; // ���� �޽���
        closesocket(serverSocket); // ���� �ݱ�
        return;
    }

    // ���� ��� ����
    if (listen(serverSocket, SOMAXCONN) == SOCKET_ERROR) {
        std::cerr << "Listening failed. Error: " << WSAGetLastError() << std::endl; // ���� �޽���
        closesocket(serverSocket); // ���� �ݱ�
        return;
    }

    running.store(true); // ���� ���¸� ���� ������ ����
    std::cout << "Server started on port " << port << std::endl; // ���� ���� �޽���

    // Ŭ���̾�Ʈ ���� ���
    acceptConnections(); // Ŭ���̾�Ʈ ���� ������ ó���ϴ� �Լ� ȣ��
}

// Server ����
void Server::stop() {
    running.store(false); // ���� ���¸� ������ ����
    // ��� Ŭ���̾�Ʈ ������ ���� ���
    for (auto& t : clientThreads) {
        if (t.joinable()) { // joinable() ���� Ȯ�� ��
            t.join();       // ������ ���� ���
        }
    }

    closesocket(serverSocket);  // ���� ���� �ݱ�
    std::cout << "Server stopped." << std::endl; // ���� ���� �޽���
}

// Ŭ���̾�Ʈ ���� ����
void Server::acceptConnections() {
    while (running.load()) { // ���� ���� ������ ���� ����
        // Ŭ���̾�Ʈ ���� ��û ����
        SOCKET clientSocket = accept(serverSocket, NULL, NULL);
        if (clientSocket != INVALID_SOCKET) { // ���� ���� ��


            // Ŭ���̾�Ʈ�� ���۵ɶ� ���� ó������ ��Ƴ�����
            //startConnections();



            // ���ο� �����忡�� Ŭ���̾�Ʈ ó�� ����
            {
                std::lock_guard<std::mutex> lock(mtx); // mtx ����� ����
                clientThreads.emplace_back(&Server::RunServerAsync, this, clientSocket); // ���Ϳ� ���ο� ������ �߰�
            }
        }
        else {
            // ���� ���� �� ���� �޽��� ���
            std::cerr << "Accept failed. Error: " << WSAGetLastError() << std::endl;
        }
    }
}

//void Server::startConnections()
//{
//    serverState.incrementClientCount();
//    std::cout << "Client connected. Total clients: " << serverState.getClientCount() << std::endl;
//}





// Ŭ���̾�Ʈ ó��
void Server::RunServerAsync(SOCKET clientSocket) {
    char buffer[1024];      // �����͸� ������ ����
    int bytesReceived;      // ���ŵ� ������ ũ��

    // Ŭ���̾�Ʈ�κ��� �����͸� ���������� ����
    while ((bytesReceived = recv(clientSocket, buffer, sizeof(buffer), 0)) > 0) 
    {
        ReceiveFromTCPClient(clientSocket, buffer, bytesReceived);
    }

    // ���� ���� �� ���� ó��
    if (bytesReceived == SOCKET_ERROR) {
        std::cerr << "recv failed. Error: " << WSAGetLastError() << std::endl;
    }

    // Ŭ���̾�Ʈ ���� �ݱ�
    closesocket(clientSocket);

    // Ŭ���̾�Ʈ �� ����
    //serverState.decrementClientCount();
    //std::cout << "Client disconnected. Total clients: " << serverState.getClientCount() << std::endl;
}


void Server::ReceiveFromTCPClient(SOCKET clientSocket, char* buffer, int bytesReceived)
{
    // ������ �����͸� UTF-8 ���ڿ��� ��ȯ
    std::string jsonMessage(buffer, bytesReceived);

    // ���������� JSON ���
    std::cout << "Received JSON: " << jsonMessage << std::endl;

    // JSON�� ��ü�� ��ȯ
    try {
        // JSON �Ľ�
        nlohmann::json parsedMessage = nlohmann::json::parse(jsonMessage);

        // connectionState�� data ����
        std::string connectionState = parsedMessage["connectionState"];
        auto data = parsedMessage["data"];

        // ���������� ������ ���
        std::cout << "Connection State: " << connectionState << std::endl;
        std::cout << "Data: " << data << std::endl;

        // �ʿ��� ó���� ���⼭ ����� �� ����...
        // Ŭ���̾�Ʈ�� �������϶� ���� ó������ ��Ƴ�����
        UpdateHandler handler(clientSocket);
        handler.processRequest(buffer, bytesReceived);  // �޽��� ó��
    }
    catch (const std::exception& ex) {
        std::cerr << "JSON �Ľ� ����: " << ex.what() << std::endl;
    }
}
