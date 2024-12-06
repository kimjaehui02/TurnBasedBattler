#include <iostream>
#include <string>
#include <thread>
#include "Server.h"

int main() {
    int port = 8080;  // ����� ��Ʈ ��ȣ ����
    Server server(port);  // ���� ��ü ����

    // ������ �����ϴ� ������
    std::thread serverThread([&server]() {
        server.start();  // ���� ����
        });

    // ����� �Է��� ó���ϴ� ����
    std::string command;
    std::cout << "Type 'exit' to stop the server..." << std::endl;
    while (true) {
        std::getline(std::cin, command);  // ����� �Է� �ޱ�
        if (command == "exit") {         // �Է��� 'exit'�� ���
            server.stop();               // ���� ����
            break;                       // ���� ����
        }
    }

    // ���� �����尡 ����� ������ ���
    if (serverThread.joinable()) {
        serverThread.join();
    }

    return 0;
}
