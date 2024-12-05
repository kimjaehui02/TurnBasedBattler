#include <iostream>
#include <string>  // �� ���� �߰��ؾ� �մϴ�.
#include "Server.h"

int main() {
    int port = 8080;  // ����� ��Ʈ ��ȣ ����
    Server server(port);  // ���� ��ü ����

    // ���� ����
    server.start();

    // ������ ����Ǵ� ���� ����� �Է��� �޾� ������ �� �ֵ��� ����
    std::string command;
    std::cout << "Type 'exit' to stop the server..." << std::endl;
    while (true) {
        std::getline(std::cin, command);
        if (command == "exit") {
            server.stop();  // ���� ����
            break;
        }
    }

    return 0;
}
