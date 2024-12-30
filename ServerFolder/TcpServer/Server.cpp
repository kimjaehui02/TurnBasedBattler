#include "Server.h"  // Server 클래스 정의를 포함한 헤더 파일
#include <iostream>  // 콘솔 입출력을 위한 라이브러리
#include <thread>    // 멀티스레딩 처리를 위한 라이브러리
#include <winsock2.h>  // Windows 소켓 API를 사용하기 위한 라이브러리
#include <Ws2tcpip.h>  // 윈도우에서 IP 주소 변환을 위한 헤더 파일


#pragma warning(disable: 28020)
// json.hpp가 포함된 코드
#include "json.hpp"
#pragma warning(default: 28020) // 다시 경고 활성화

#include "UpdateHandler.h"
#include "SubServer.h"



// Server 생성자
Server::Server(int port)
    : port(port), running(false), serverSocket(INVALID_SOCKET) 
{ // 멤버 변수 초기화
    WSADATA wsaData; // Winsock 초기화를 위한 구조체
    // Winsock 초기화
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {  // 실패하면 오류 메시지 출력 후 종료
        std::cerr << "WSAStartup failed." << std::endl;
        return;
    }
    std::cout << "Winsock initialized." << std::endl; // 초기화 성공 메시지
}

// Server 소멸자
Server::~Server() {
    WSACleanup();  // Winsock 정리: 사용 후 반드시 호출해야 함
    std::cout << "Winsock cleaned up." << std::endl; // 정리 완료 메시지
}

// Server 시작
void Server::start() 
{
    // 서버 소켓 생성: IPv4(AF_INET), TCP(SOCK_STREAM) 사용
    serverSocket = socket(AF_INET, SOCK_STREAM, 0);
    if (serverSocket == INVALID_SOCKET) { // 소켓 생성 실패 시 오류 메시지 출력
        std::cerr << "Failed to create socket. Error: " << WSAGetLastError() << std::endl;
        return;
    }

    // 서버 주소 설정 구조체
    sockaddr_in serverAddr = {};               // 구조체 초기화
    serverAddr.sin_family = AF_INET;           // IPv4 프로토콜 사용
    serverAddr.sin_addr.s_addr = INADDR_ANY;   // 모든 네트워크 인터페이스에서 요청을 수락
    serverAddr.sin_port = htons(port);         // 포트를 네트워크 바이트 순서로 설정

    // 서버 소켓에 주소 및 포트 바인딩
    if (bind(serverSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
        std::cerr << "Binding failed. Error: " << WSAGetLastError() << std::endl; // 실패 메시지
        closesocket(serverSocket); // 소켓 닫기
        return;
    }

    // 연결 대기 시작
    if (listen(serverSocket, SOMAXCONN) == SOCKET_ERROR) {
        std::cerr << "Listening failed. Error: " << WSAGetLastError() << std::endl; // 실패 메시지
        closesocket(serverSocket); // 소켓 닫기
        return;
    }

    running.store(true); // 서버 상태를 실행 중으로 설정
    std::cout << "Server started on port " << port << std::endl; // 서버 시작 메시지

    // 클라이언트 연결 대기
    acceptConnections(); // 클라이언트 연결 수락을 처리하는 함수 호출
}

// Server 종료
void Server::stop() {
    running.store(false); // 서버 상태를 중지로 설정
    // 모든 클라이언트 스레드 종료 대기
    for (auto& t : clientThreads) {
        if (t.joinable()) { // joinable() 상태 확인 후
            t.join();       // 스레드 종료 대기
        }
    }

    closesocket(serverSocket);  // 서버 소켓 닫기
    std::cout << "Server stopped." << std::endl; // 서버 종료 메시지
}

// 클라이언트 연결 수락
void Server::acceptConnections() {
    while (running.load()) { // 서버 실행 상태일 동안 루프
        // 클라이언트 연결 요청 수락
        SOCKET clientSocket = accept(serverSocket, NULL, NULL);
        if (clientSocket != INVALID_SOCKET) { // 연결 성공 시

            std::cerr << "새 소켓의 접근 감지: " << std::endl;
            // 클라이언트가 시작될때 해줄 처리들을 모아놓은것
            //startConnections();



            // 새로운 스레드에서 클라이언트 처리 시작
            {
                std::lock_guard<std::mutex> lock(mtx); // mtx 잠금을 시작
                clientThreads.emplace_back(&Server::RunServerAsync, this, clientSocket); // 벡터에 새로운 스레드 추가
            }
        }
        else {
            // 연결 실패 시 오류 메시지 출력
            std::cerr << "Accept failed. Error: " << WSAGetLastError() << std::endl;
        }
    }
}

//void Server::startConnections()
//{
//    serverState.incrementClientCount();
//    std::cout << "Client connected. Total clients: " << serverState.getClientCount() << std::endl;
//}





// 클라이언트 처리
void Server::RunServerAsync(SOCKET clientSocket) {
    char buffer[1024];      // 데이터를 저장할 버퍼
    int bytesReceived;      // 수신된 데이터 크기
    
    int ServerCheck = -1;
    std::string ServerIp = "";
    int tcpPort = 0;
    int udpPort = 0;
    // 클라이언트로부터 데이터를 지속적으로 수신
    while ((bytesReceived = recv(clientSocket, buffer, sizeof(buffer), 0)) > 0) 
    {
        if (ServerCheck == -1)
        {
            // 수신한 데이터를 UTF-8 문자열로 변환
            std::string jsonMessage(buffer, bytesReceived);

            // JSON 파싱
            nlohmann::json parsedMessage = nlohmann::json::parse(jsonMessage);

            if (parsedMessage["data"].contains("playerName") && parsedMessage["data"]["playerName"] == "udpServer")
            {
                ServerIp = parsedMessage["data"]["ServerIp"];
                tcpPort = parsedMessage["data"]["tcpPort"];
                udpPort = parsedMessage["data"]["udpPort"];

                ServerCheck = 1;
                std::cerr << "서버가 접근하엿습니다" << std::endl;
                std::cerr << SubServerManager::subServerListToJson() << std::endl;
            }
            else
            {
                ServerCheck = 0;
            }


        }

        ReceiveFromTCPClient(clientSocket, buffer, bytesReceived);
    }



    // 수신 실패 시 오류 처리
    if (bytesReceived == SOCKET_ERROR) {
        std::cerr << "recv failed. Error: " << WSAGetLastError() << std::endl;
    }

    if (ServerCheck == 1)
    {
        std::cerr <<  SubServerManager::subServerListToJson() << std::endl;
        std::cerr << "서버가 제거되었습니다" << ServerIp << port << std::endl;
        SubServer sub(ServerIp, tcpPort, udpPort);
        SubServerManager::decrementClientCount(sub);
        
        std::cerr << "서버가 제거되었습니다"<< SubServerManager::getClientCount() << std::endl;
    }


    // 클라이언트 소켓 닫기
    closesocket(clientSocket);

    // 클라이언트 수 감소
    //serverState.decrementClientCount();
    //std::cout << "Client disconnected. Total clients: " << serverState.getClientCount() << std::endl;
}




void Server::ReceiveFromTCPClient(SOCKET clientSocket, char* buffer, int bytesReceived)
{
    // 수신한 데이터를 UTF-8 문자열로 변환
    std::string jsonMessage(buffer, bytesReceived);

    // 디버깅용으로 JSON 출력
    std::cout << "Received JSON: " << jsonMessage << std::endl;

    // JSON을 객체로 변환
    try 
    {
        // JSON 파싱
        nlohmann::json parsedMessage = nlohmann::json::parse(jsonMessage);

        // connectionState와 data 추출
        std::string connectionState = parsedMessage["connectionState"];
        auto data = parsedMessage["data"];

        // 디버깅용으로 데이터 출력
        std::cout << "Connection State: " << connectionState << std::endl;
        std::cout << "Data: " << data << std::endl;

        // 필요한 처리를 여기서 계속할 수 있음...
        // 클라이언트가 진행중일때 해줄 처리들을 모아놓은것
        UpdateHandler handler(clientSocket);
        SendFunction sendFunc = std::bind(&Server::SendToTCPClient, this, std::placeholders::_1, ConnectionState::Connecting, std::placeholders::_3);
        handler.HandleConnectionState(buffer, bytesReceived, sendFunc);  // 메시지 처리



       
    }
    catch (const std::exception& ex) 
    {
        std::cerr << "JSON 파싱 오류: " << ex.what() << std::endl;
    }
}

void Server::SendToTCPClient(SOCKET clientSocket, ConnectionState connectionState, const nlohmann::json& messageData)
{
    std::cerr << "SendToTCPClient" << std::endl;
    // 연결 상태를 문자열로 변환 (ConnectionState를 문자열로 변환)
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

    // 보낼 데이터 객체 만들기 (JSON)
    nlohmann::json message;
    message["connectionState"] = connectionStateStr;
    message["data"] = messageData;  // 여기에 필요한 데이터를 추가

    // JSON을 문자열로 변환
    std::string jsonMessage = message.dump(); // JSON 문자열

    // 문자열을 버퍼에 복사
    int messageLength = jsonMessage.length();
    char* sendBuffer = new char[messageLength + 1]; // 메시지 길이 + null 종료 문자
    strcpy_s(sendBuffer, messageLength + 1, jsonMessage.c_str());

    // 클라이언트로 데이터 전송
    int bytesSent = send(clientSocket, sendBuffer, messageLength, 0);
    if (bytesSent == SOCKET_ERROR) 
    {
        std::cerr << "Send failed. Error: " << WSAGetLastError() << std::endl;
    }
    else 
    {
        std::cout << "Message sent to client: " << jsonMessage << std::endl;
    }

    // 동적으로 할당된 메모리 해제
    delete[] sendBuffer;
}

