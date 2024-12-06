#include "UpdateHandler.h"
#include "json.hpp"
using json = nlohmann::json;

UpdateHandler::UpdateHandler(SOCKET clientSocket) : clientSocket(clientSocket) {}

void UpdateHandler::processRequest(char* buffer, int bytesReceived) {
    // 받은 데이터를 문자열로 변환
    std::string receivedMessage(buffer, bytesReceived);
    std::cout << "Received message: " << receivedMessage << std::endl;

    try {
        // 문자열을 JSON으로 파싱
        json receivedJson = json::parse(receivedMessage);

        // 명령어(command) 추출
        std::string command = receivedJson["command"];

        // "PING" 명령에 응답
        if (command == "PING") {
            json response;
            response["command"] = "PONG";  // 응답 명령어
            sendResponse(response.dump()); // JSON을 문자열로 직렬화하여 응답 전송
        }
        // "RequestInitialData" 명령에 응답
        else if (command == "RequestInitialData") {
            handleInitialDataRequest();
        }
        // 그 외의 경우
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

    // 응답 메시지 생성
    json response;
    response["command"] = "PONG";
    response["message"] = "Initial data response from server";
    sendResponse(response.dump());
}

void UpdateHandler::sendResponse(const std::string& response) {
    send(clientSocket, response.c_str(), response.size(), 0);   // 클라이언트로 응답 전송
}
