#include "UpdateHandler.h"
#include "SubServer.h"
#include "SubServerManager.h"
#include "json.hpp"


using json = nlohmann::json;




UpdateHandler::UpdateHandler(SOCKET clientSocket) : clientSocket(clientSocket) {}

void UpdateHandler::HandleConnectionState(char* buffer, int bytesReceived, const SendFunction& callback)
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

void UpdateHandler::HandleConnecting(const json& message, const SendFunction& callback) {
    std::cout << "Handling Connecting state with message: " << message.dump() << std::endl;
    // 추가 처리 로직...
    std::string playerName = message["data"]["playerName"];
    if (playerName == "udpServer") {
        std::cout << "Player name is 'udpServer'." << std::endl;
        // 추가 처리 로직...
        std::string serverIp = message["data"]["ServerIp"];
        int tcpPort = message["data"]["tcpPort"];
        int udpPort = message["data"]["udpPort"];

        // SubServer 객체 생성
        SubServer server(serverIp, tcpPort, udpPort);

        // SubServerManager의 incrementClientCount 호출
        SubServerManager::incrementClientCount(server);




    }
    else if (playerName == "client") {
        std::cout << "Player name is 'client'." << std::endl;
        // 추가 처리 로직...

        returnListOfServer(callback);

    }
    else {
        std::cout << "다른경우입니다 플레이어네임은 : " << playerName << std::endl;
    }
}

void UpdateHandler::HandleDataSyncing(const json& message, const SendFunction& callback) {
    std::cout << "Handling DataSyncing state with message: " << message.dump() << std::endl;
    // 추가 처리 로직...
}

void UpdateHandler::HandleDisconnecting(const json& message, const SendFunction& callback) {
    std::cout << "Handling Disconnecting state with message: " << message.dump() << std::endl;
    // 추가 처리 로직...
}

void UpdateHandler::HandleError(const json& message, const SendFunction& callback) {
    std::cerr << "Handling Error state with message: " << message.dump() << std::endl;
    // 추가 처리 로직...
}

void UpdateHandler::HandleTcpToUdp(const json& message, const SendFunction& callback) {
    std::cerr << "Handling TcpToUdp state with message: " << message.dump() << std::endl;
    // 추가 처리 로직...
}

void UpdateHandler::handleInitialDataRequest() {

    // 응답 메시지 생성
    json response;
    response["command"] = "PONG";
    response["message"] = "Initial data response from server";
    //sendResponse(response.dump());
}




// 함수 매개변수에 적용
void UpdateHandler::returnListOfServer(const SendFunction& sendFunction) {
    std::cerr << "void UpdateHandler::returnListOfServer(SendFunction)" << std::endl;

    ConnectionState state = ConnectionState::Connecting;

    // 전달받은 함수 호출
    sendFunction(clientSocket, state, SubServerManager::subServerListToJson());
}