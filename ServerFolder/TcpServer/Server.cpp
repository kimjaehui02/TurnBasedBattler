#include "Server.h"  // Server Ŭ���� ���Ǹ� ������ ��� ����
#include <iostream>  // �ܼ� ������� ���� ���̺귯��
#include <thread>    // ��Ƽ������ ó���� ���� ���̺귯��
#include <winsock2.h>  // Windows ���� API�� ����ϱ� ���� ���̺귯��
//#include "json.hpp"
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
            startConnections();



            // ���ο� �����忡�� Ŭ���̾�Ʈ ó�� ����
            {
                std::lock_guard<std::mutex> lock(mtx); // mtx ����� ����
                clientThreads.emplace_back(&Server::handleClient, this, clientSocket); // ���Ϳ� ���ο� ������ �߰�
            }
        }
        else {
            // ���� ���� �� ���� �޽��� ���
            std::cerr << "Accept failed. Error: " << WSAGetLastError() << std::endl;
        }
    }
}

void Server::startConnections()
{
    serverState.incrementClientCount();
    std::cout << "Client connected. Total clients: " << serverState.getClientCount() << std::endl;
}

// Ŭ���̾�Ʈ ó��
void Server::handleClient(SOCKET clientSocket) {
    char buffer[1024];      // �����͸� ������ ����
    int bytesReceived;      // ���ŵ� ������ ũ��
    // Ŭ���̾�Ʈ�κ��� �����͸� ���������� ����
    while ((bytesReceived = recv(clientSocket, buffer, sizeof(buffer), 0)) > 0) {


        // Ŭ���̾�Ʈ�� �������϶� ���� ó������ ��Ƴ�����
        UpdateHandler handler(clientSocket);
        handler.processRequest(buffer, bytesReceived);  // �޽��� ó��

        //updateClient(buffer, &bytesReceived, &clientSocket);

    }

    // ���� ���� �� ���� ó��
    if (bytesReceived == SOCKET_ERROR) {
        std::cerr << "recv failed. Error: " << WSAGetLastError() << std::endl;
    }

    // Ŭ���̾�Ʈ ���� �ݱ�
    closesocket(clientSocket);

    // Ŭ���̾�Ʈ �� ����
    serverState.decrementClientCount();
    std::cout << "Client disconnected. Total clients: " << serverState.getClientCount() << std::endl;
}



//void Server::updateClient(char* buffer, int* bytesReceived, SOCKET* clientSocket)
//{
//    // ���� �����͸� ���ڿ��� ��ȯ
//    std::string receivedMessage(*buffer, *bytesReceived);
//    std::cout << "Received message: " << receivedMessage << std::endl; // ���� �޽��� ���
//
//    // Ŭ���̾�Ʈ�κ��� "RequestInitialData" �޽����� ���� ����
//    if (receivedMessage == "RequestInitialData") {
//        std::string response = "Initial data response from server"; // ���� �޽���
//        send(*clientSocket, response.c_str(), response.size(), 0);   // Ŭ���̾�Ʈ�� ���� ����
//    }
//}
