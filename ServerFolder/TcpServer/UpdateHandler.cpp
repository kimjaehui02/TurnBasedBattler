#include "UpdateHandler.h"
#include "json.hpp"

using json = nlohmann::json;

UpdateHandler::UpdateHandler(SOCKET clientSocket) : clientSocket(clientSocket) {}

void UpdateHandler::HandleConnectionState(char* buffer, int bytesReceived) 
{
    // 받은 데이터를 문자열로 변환
    std::string jsonMessage(buffer, bytesReceived);
    std::cout << "Received message: " << jsonMessage << std::endl;

    try {
        // JSON 파싱
        json parsedMessage = json::parse(jsonMessage); // 여기서 예외 발생 가능

        // connectionState 추출 (키가 존재하는지 확인)
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
    // JSON 파싱에서 발생한 예외를 잡기
    catch (const nlohmann::json::parse_error& e) 
    {
        std::cerr << "JSON parsing error: " << e.what() << std::endl;
    }
    // 그 외의 예외를 잡기
    catch (const std::exception& e) 
    {
        std::cerr << "General error: " << e.what() << std::endl;
    }
}

void UpdateHandler::HandleConnecting(const json& message) {
    std::cout << "Handling Connecting state with message: " << message.dump() << std::endl;
    // 추가 처리 로직...
}

void UpdateHandler::HandleDataSyncing(const json& message) {
    std::cout << "Handling DataSyncing state with message: " << message.dump() << std::endl;
    // 추가 처리 로직...
}

void UpdateHandler::HandleDisconnecting(const json& message) {
    std::cout << "Handling Disconnecting state with message: " << message.dump() << std::endl;
    // 추가 처리 로직...
}

void UpdateHandler::HandleError(const json& message) {
    std::cerr << "Handling Error state with message: " << message.dump() << std::endl;
    // 추가 처리 로직...
}

void UpdateHandler::HandleTcpToUdp(const json& message) {
    std::cerr << "Handling TcpToUdp state with message: " << message.dump() << std::endl;
    // 추가 처리 로직...
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
