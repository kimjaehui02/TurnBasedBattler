#ifndef SERVER_H
#define SERVER_H

#include <iostream>
#include <thread>
#include <vector>
#include <mutex>
#include <atomic>
#include <winsock2.h>
#include "SubServer.h"  // ServerState 클래스 포함
#include "ConnectionState.h"
#include "SubServerManager.h"
#include "SubServer.h"
#include <functional>

// Server의 SendToTCPClient 함수 포인터 타입 정의
using SendFunction = std::function<void(SOCKET, ConnectionState, const nlohmann::json&)>;

class Server {
public:
    Server(int port);  // 생성자
    ~Server();         // 소멸자

    void start();      // 서버 시작
    void stop();       // 서버 종료

private:
    void acceptConnections();  // 클라이언트 연결 처리
    //void handleClient(SOCKET clientSocket);  // 클라이언트 처리

    //void startConnections();

    void RunServerAsync(SOCKET clientSocket);

    // 클라이언트간의 교류
    void SendToTCPClient(SOCKET clientSocket, ConnectionState connectionState, const nlohmann::json& messageData);
    void ReceiveFromTCPClient(SOCKET clientSocket, char* buffer, int bytesReceived);

    // UDP서버간의 교류
    //void SendToUDPServer();
    //void ReceiveFromUDPServer();

    //void HandleConnectionState();



    int port;  // 서버 포트
    SOCKET serverSocket;  // 서버 소켓
    std::atomic<bool> running;  // 서버 실행 여부
    std::vector<std::thread> clientThreads;  // 클라이언트 핸들링 스레드들
    std::mutex mtx;  // 멀티스레드 안전을 위한 뮤텍스
    //SubServerManager serverState;  // ServerState 인스턴스

    // c#서브 서버들을 관리할 리스트

};

#endif
