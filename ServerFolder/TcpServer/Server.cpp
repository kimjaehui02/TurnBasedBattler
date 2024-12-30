#include "Server.h"  // Server Ŭ���� ���Ǹ� ������ ��� ����
#include <iostream>  // �ܼ� ������� ���� ���̺귯��
#include <thread>    // ��Ƽ������ ó���� ���� ���̺귯��
#include <winsock2.h>  // Windows ���� API�� ����ϱ� ���� ���̺귯��
#include <Ws2tcpip.h>  // �����쿡�� IP �ּ� ��ȯ�� ���� ��� ����


#pragma warning(disable: 28020)
// json.hpp�� ���Ե� �ڵ�
#include "json.hpp"
#pragma warning(default: 28020) // �ٽ� ��� Ȱ��ȭ

#include "UpdateHandler.h"
#include "SubServer.h"



// Server ������
Server::Server(int port)
    : port(port), running(false), serverSocket(INVALID_SOCKET) 
{ // ��� ���� �ʱ�ȭ
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
void Server::start() 
{
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

            std::cerr << "�� ������ ���� ����: " << std::endl;
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
    
    int ServerCheck = -1;
    std::string ServerIp = "";
    int tcpPort = 0;
    int udpPort = 0;
    // Ŭ���̾�Ʈ�κ��� �����͸� ���������� ����
    while ((bytesReceived = recv(clientSocket, buffer, sizeof(buffer), 0)) > 0) 
    {
        if (ServerCheck == -1)
        {
            // ������ �����͸� UTF-8 ���ڿ��� ��ȯ
            std::string jsonMessage(buffer, bytesReceived);

            // JSON �Ľ�
            nlohmann::json parsedMessage = nlohmann::json::parse(jsonMessage);

            if (parsedMessage["data"].contains("playerName") && parsedMessage["data"]["playerName"] == "udpServer")
            {
                ServerIp = parsedMessage["data"]["ServerIp"];
                tcpPort = parsedMessage["data"]["tcpPort"];
                udpPort = parsedMessage["data"]["udpPort"];

                ServerCheck = 1;
                std::cerr << "������ �����Ͽ����ϴ�" << std::endl;
                std::cerr << SubServerManager::subServerListToJson() << std::endl;
            }
            else
            {
                ServerCheck = 0;
            }


        }

        ReceiveFromTCPClient(clientSocket, buffer, bytesReceived);
    }



    // ���� ���� �� ���� ó��
    if (bytesReceived == SOCKET_ERROR) {
        std::cerr << "recv failed. Error: " << WSAGetLastError() << std::endl;
    }

    if (ServerCheck == 1)
    {
        std::cerr <<  SubServerManager::subServerListToJson() << std::endl;
        std::cerr << "������ ���ŵǾ����ϴ�" << ServerIp << port << std::endl;
        SubServer sub(ServerIp, tcpPort, udpPort);
        SubServerManager::decrementClientCount(sub);
        
        std::cerr << "������ ���ŵǾ����ϴ�"<< SubServerManager::getClientCount() << std::endl;
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
    try 
    {
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
        SendFunction sendFunc = std::bind(&Server::SendToTCPClient, this, std::placeholders::_1, ConnectionState::Connecting, std::placeholders::_3);
        handler.HandleConnectionState(buffer, bytesReceived, sendFunc);  // �޽��� ó��



       
    }
    catch (const std::exception& ex) 
    {
        std::cerr << "JSON �Ľ� ����: " << ex.what() << std::endl;
    }
}

void Server::SendToTCPClient(SOCKET clientSocket, ConnectionState connectionState, const nlohmann::json& messageData)
{
    std::cerr << "SendToTCPClient" << std::endl;
    // ���� ���¸� ���ڿ��� ��ȯ (ConnectionState�� ���ڿ��� ��ȯ)
    std::string connectionStateStr;
    switch (connectionState) 
    {
    case ConnectionState::Default: connectionStateStr = "Default"; break;
    case ConnectionState::Connecting: connectionStateStr = "Connecting"; break;
    case ConnectionState::DataSyncing: connectionStateStr = "DataSyncing"; break;
    case ConnectionState::Disconnecting: connectionStateStr = "Disconnecting"; break;
    case ConnectionState::Error: connectionStateStr = "Error"; break;
    case ConnectionState::TcpToUdp: connectionStateStr = "TcpToUdp"; break;
    default: connectionStateStr = "Unknown"; break;
    }

    // ���� ������ ��ü ����� (JSON)
    nlohmann::json message;
    message["connectionState"] = connectionStateStr;
    message["data"] = messageData;  // ���⿡ �ʿ��� �����͸� �߰�

    // JSON�� ���ڿ��� ��ȯ
    std::string jsonMessage = message.dump(); // JSON ���ڿ�

    // ���ڿ��� ���ۿ� ����
    int messageLength = jsonMessage.length();
    char* sendBuffer = new char[messageLength + 1]; // �޽��� ���� + null ���� ����
    strcpy_s(sendBuffer, messageLength + 1, jsonMessage.c_str());

    // Ŭ���̾�Ʈ�� ������ ����
    int bytesSent = send(clientSocket, sendBuffer, messageLength, 0);
    if (bytesSent == SOCKET_ERROR) 
    {
        std::cerr << "Send failed. Error: " << WSAGetLastError() << std::endl;
    }
    else 
    {
        std::cout << "Message sent to client: " << jsonMessage << std::endl;
    }

    // �������� �Ҵ�� �޸� ����
    delete[] sendBuffer;
}

