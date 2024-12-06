#include <iostream>
#include <string>
#include <thread>
#include "Server.h"

int main() {
    int port = 8080;  // 사용할 포트 번호 설정
    Server server(port);  // 서버 객체 생성

    // 서버를 실행하는 스레드
    std::thread serverThread([&server]() {
        server.start();  // 서버 시작
        });

    // 사용자 입력을 처리하는 루프
    std::string command;
    std::cout << "Type 'exit' to stop the server..." << std::endl;
    while (true) {
        std::getline(std::cin, command);  // 사용자 입력 받기
        if (command == "exit") {         // 입력이 'exit'일 경우
            server.stop();               // 서버 종료
            break;                       // 루프 종료
        }
    }

    // 서버 스레드가 종료될 때까지 대기
    if (serverThread.joinable()) {
        serverThread.join();
    }

    return 0;
}
