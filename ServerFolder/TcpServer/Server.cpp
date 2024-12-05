// Server.cpp
#include "Server.h"
#include <iostream>
#include <thread>
#include <winsock2.h>

Server::Server(int port) : port(port), running(false) {
    WSADATA wsaData;
    WSAStartup(MAKEWORD(2, 2), &wsaData);  // Winsock �ʱ�ȭ
}

Server::~Server() {
    WSACleanup();  // Winsock ����
}

void Server::start() {
    serverSocket = socket(AF_INET, SOCK_STREAM, 0);  // ���� ���� ����
    if (serverSocket == INVALID_SOCKET) {
        std::cerr << "Failed to create socket." << std::endl;
        return;
    }

    sockaddr_in serverAddr;
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_addr.s_addr = INADDR_ANY;  // ��� �������̽����� ��û�� ����
    serverAddr.sin_port = htons(port);  // ��Ʈ ����

    if (bind(serverSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        std::cerr << "Binding failed." << std::endl;
        return;
    }

    if (listen(serverSocket, SOMAXCONN) == SOCKET_ERROR) {
        std::cerr << "Listening failed." << std::endl;
        return;
    }

    running.store(true);
    std::cout << "Server started on port " << port << std::endl;

    // Ŭ���̾�Ʈ ���� ���
    acceptConnections();
}

void Server::stop() {
    running.store(false);
    for (auto& t : clientThreads) {
        if (t.joinable()) {
            t.join();
        }
    }

    closesocket(serverSocket);  // ���� ���� �ݱ�
    std::cout << "Server stopped." << std::endl;
}

void Server::acceptConnections() {
    while (running.load()) {
        SOCKET clientSocket = accept(serverSocket, NULL, NULL);  // Ŭ���̾�Ʈ ���� �ޱ�
        if (clientSocket != INVALID_SOCKET) {
            std::lock_guard<std::mutex> lock(mtx);  // ��Ƽ������ ���� ó��
            clientThreads.push_back(std::thread(&Server::handleClient, this, clientSocket));
        }
    }
}

void Server::handleClient(SOCKET clientSocket) {
    char buffer[1024];
    int bytesReceived;
    while ((bytesReceived = recv(clientSocket, buffer, sizeof(buffer), 0)) > 0) {
        std::string receivedMessage(buffer, bytesReceived);
        std::cout << "Received message: " << receivedMessage << std::endl;

        // Ŭ���̾�Ʈ�κ��� "RequestInitialData" �޽����� ���� ����
        if (receivedMessage == "RequestInitialData") {
            std::string response = "Initial data response from server";
            send(clientSocket, response.c_str(), response.size(), 0);
        }
    }
    closesocket(clientSocket);
}

