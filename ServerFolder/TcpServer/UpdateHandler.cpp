#include "UpdateHandler.h"
#include "SubServer.h"
#include "SubServerManager.h"
#include "json.hpp"


using json = nlohmann::json;




UpdateHandler::UpdateHandler(SOCKET clientSocket) : clientSocket(clientSocket) {}

void UpdateHandler::HandleConnectionState(char* buffer, int bytesReceived, const SendFunction& callback)
{
    // ���� �����͸� ���ڿ��� ��ȯ
    std::string jsonMessage(buffer, bytesReceived);
    std::cout << "Received message: " << jsonMessage << std::endl;

    try {
        // JSON �Ľ�
        json parsedMessage = json::parse(jsonMessage); // ���⼭ ���� �߻� ����

        // connectionState ���� (Ű�� �����ϴ��� Ȯ��)
        if (parsedMessage.contains("connectionState")) 
        {
            std::string connectionState = parsedMessage["connectionState"];

            if (connectionState == "Connecting") 
            {
                HandleConnecting(parsedMessage, callback);
            }
            else if (connectionState == "DataSyncing") 
            {
                HandleDataSyncing(parsedMessage, callback);
            }
            else if (connectionState == "Disconnecting") 
            {
                HandleDisconnecting(parsedMessage, callback);
            }
            else if (connectionState == "Error")
            {
                HandleError(parsedMessage, callback);
            }
            else if (connectionState == "TcpToUdp")
            {
                HandleTcpToUdp(parsedMessage, callback);
            }
            else 
            {
                std::cerr << "Unknown connection state: " << connectionState << std::endl;
            }
        }
        else 
        {
            std::cerr << "connectionState not found in the message" << std::endl;
        }
    }
    // JSON �Ľ̿��� �߻��� ���ܸ� ���
    catch (const nlohmann::json::parse_error& e) 
    {
        std::cerr << "JSON parsing error: " << e.what() << std::endl;
    }
    // �� ���� ���ܸ� ���
    catch (const std::exception& e) 
    {
        std::cerr << "General error: " << e.what() << std::endl;
    }
}

void UpdateHandler::HandleConnecting(const json& message, const SendFunction& callback) {
    std::cout << "Handling Connecting state with message: " << message.dump() << std::endl;
    // �߰� ó�� ����...
    std::string playerName = message["data"]["playerName"];
    if (playerName == "udpServer") {
        std::cout << "Player name is 'udpServer'." << std::endl;
        // �߰� ó�� ����...
        std::string serverIp = message["data"]["ServerIp"];
        int tcpPort = message["data"]["tcpPort"];
        int udpPort = message["data"]["udpPort"];

        // SubServer ��ü ����
        SubServer server(serverIp, tcpPort, udpPort);

        // SubServerManager�� incrementClientCount ȣ��
        SubServerManager::incrementClientCount(server);




    }
    else if (playerName == "client") {
        std::cout << "Player name is 'client'." << std::endl;
        // �߰� ó�� ����...

        returnListOfServer(callback);

    }
    else {
        std::cout << "�ٸ�����Դϴ� �÷��̾������ : " << playerName << std::endl;
    }
}

void UpdateHandler::HandleDataSyncing(const json& message, const SendFunction& callback) {
    std::cout << "Handling DataSyncing state with message: " << message.dump() << std::endl;
    // �߰� ó�� ����...
}

void UpdateHandler::HandleDisconnecting(const json& message, const SendFunction& callback) {
    std::cout << "Handling Disconnecting state with message: " << message.dump() << std::endl;
    // �߰� ó�� ����...
}

void UpdateHandler::HandleError(const json& message, const SendFunction& callback) {
    std::cerr << "Handling Error state with message: " << message.dump() << std::endl;
    // �߰� ó�� ����...
}

void UpdateHandler::HandleTcpToUdp(const json& message, const SendFunction& callback) {
    std::cerr << "Handling TcpToUdp state with message: " << message.dump() << std::endl;
    // �߰� ó�� ����...
}

void UpdateHandler::handleInitialDataRequest() {

    // ���� �޽��� ����
    json response;
    response["command"] = "PONG";
    response["message"] = "Initial data response from server";
    //sendResponse(response.dump());
}




// �Լ� �Ű������� ����
void UpdateHandler::returnListOfServer(const SendFunction& sendFunction) {
    std::cerr << "void UpdateHandler::returnListOfServer(SendFunction)" << std::endl;

    ConnectionState state = ConnectionState::Connecting;

    // ���޹��� �Լ� ȣ��
    sendFunction(clientSocket, state, SubServerManager::subServerListToJson());
}