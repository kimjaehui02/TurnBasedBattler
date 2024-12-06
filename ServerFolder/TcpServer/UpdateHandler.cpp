#include "UpdateHandler.h"
#include "json.hpp"
using json = nlohmann::json;

UpdateHandler::UpdateHandler(SOCKET clientSocket) : clientSocket(clientSocket) {}

void UpdateHandler::processRequest(char* buffer, int bytesReceived) {
    // ���� �����͸� ���ڿ��� ��ȯ
    std::string receivedMessage(buffer, bytesReceived);
    std::cout << "Received message: " << receivedMessage << std::endl;

    try {
        // ���ڿ��� JSON���� �Ľ�
        json receivedJson = json::parse(receivedMessage);

        // ��ɾ�(command) ����
        std::string command = receivedJson["command"];

        // "PING" ��ɿ� ����
        if (command == "PING") {
            json response;
            response["command"] = "PONG";  // ���� ��ɾ�
            sendResponse(response.dump()); // JSON�� ���ڿ��� ����ȭ�Ͽ� ���� ����
        }
        // "RequestInitialData" ��ɿ� ����
        else if (command == "RequestInitialData") {
            handleInitialDataRequest();
        }
        // �� ���� ���
        else {
            json response;
            response["command"] = "UNKNOWN";
            response["message"] = "Unknown command";
            sendResponse(response.dump());
        }
    }
    catch (const std::exception& e) {
        std::cerr << "Error parsing JSON: " << e.what() << std::endl;
    }
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
