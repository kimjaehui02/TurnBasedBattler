#include "UpdateHandler.h"
#include "json.hpp"

using json = nlohmann::json;

UpdateHandler::UpdateHandler(SOCKET clientSocket) : clientSocket(clientSocket) {}

void UpdateHandler::HandleConnectionState(char* buffer, int bytesReceived) 
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
                HandleConnecting(parsedMessage);
            }
            else if (connectionState == "DataSyncing") 
            {
                HandleDataSyncing(parsedMessage);
            }
            else if (connectionState == "Disconnecting") 
            {
                HandleDisconnecting(parsedMessage);
            }
            else if (connectionState == "Error")
            {
                HandleError(parsedMessage);
            }
            else if (connectionState == "TcpToUdp")
            {
                HandleTcpToUdp(parsedMessage);
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

void UpdateHandler::HandleConnecting(const json& message) {
    std::cout << "Handling Connecting state with message: " << message.dump() << std::endl;
    // �߰� ó�� ����...
}

void UpdateHandler::HandleDataSyncing(const json& message) {
    std::cout << "Handling DataSyncing state with message: " << message.dump() << std::endl;
    // �߰� ó�� ����...
}

void UpdateHandler::HandleDisconnecting(const json& message) {
    std::cout << "Handling Disconnecting state with message: " << message.dump() << std::endl;
    // �߰� ó�� ����...
}

void UpdateHandler::HandleError(const json& message) {
    std::cerr << "Handling Error state with message: " << message.dump() << std::endl;
    // �߰� ó�� ����...
}

void UpdateHandler::HandleTcpToUdp(const json& message) {
    std::cerr << "Handling TcpToUdp state with message: " << message.dump() << std::endl;
    // �߰� ó�� ����...
}

void UpdateHandler::handleInitialDataRequest() {

    // ���� �޽��� ����
    json response;
    response["command"] = "PONG";
    response["message"] = "Initial data response from server";
    sendResponse(response.dump());
}

void UpdateHandler::sendResponse(const std::string& response) {
    send(clientSocket, response.c_str(), response.size(), 0);   // Ŭ���̾�Ʈ�� ���� ����
}
