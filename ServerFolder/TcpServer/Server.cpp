// Server.cpp
#include "Server.h"
#include <iostream>
#include <thread>
#include <winsock2.h>

Server::Server(int port) : port(port), running(false) {
    WSADATA wsaData;
    WSAStartup(MAKEWORD(2, 2), &wsaData);  // Winsock 초기화
}

Server::~Server() {
    WSACleanup();  // Winsock 정리
}

void Server::start() {
    serverSocket = socket(AF_INET, SOCK_STREAM, 0);  // 서버 소켓 생성
    if (serverSocket == INVALID_SOCKET) {
        std::cerr << "Failed to create socket." << std::endl;
        return;
    }

    sockaddr_in serverAddr;
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_addr.s_addr = INADDR_ANY;  // 모든 인터페이스에서 요청을 받음
    serverAddr.sin_port = htons(port);  // 포트 설정

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

    // 클라이언트 연결 대기
    acceptConnections();
}

void Server::stop() {
    running.store(false);
    for (auto& t : clientThreads) {
        if (t.joinable()) {
            t.join();
        }
    }

    closesocket(serverSocket);  // 서버 소켓 닫기
    std::cout << "Server stopped." << std::endl;
}

void Server::acceptConnections() {
    while (running.load()) {
        SOCKET clientSocket = accept(serverSocket, NULL, NULL);  // 클라이언트 연결 받기
        if (clientSocket != INVALID_SOCKET) {
            std::lock_guard<std::mutex> lock(mtx);  // 멀티스레드 안전 처리
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

        // 클라이언트로부터 "RequestInitialData" 메시지가 오면 응답
        if (receivedMessage == "RequestInitialData") {
            std::string response = "Initial data response from server";
            send(clientSocket, response.c_str(), response.size(), 0);
        }
    }
    closesocket(clientSocket);
}

