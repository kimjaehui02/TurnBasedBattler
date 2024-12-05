#include <iostream>
#include <string>  // 이 줄을 추가해야 합니다.
#include "Server.h"

int main() {
    int port = 8080;  // 사용할 포트 번호 설정
    Server server(port);  // 서버 객체 생성

    // 서버 시작
    server.start();

    // 서버가 실행되는 동안 사용자 입력을 받아 종료할 수 있도록 설정
    std::string command;
    std::cout << "Type 'exit' to stop the server..." << std::endl;
    while (true) {
        std::getline(std::cin, command);
        if (command == "exit") {
            server.stop();  // 서버 종료
            break;
        }
    }

    return 0;
}
