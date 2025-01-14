#include <iostream>
#include <string>
#include <thread>
#include "Server.h"
#include "DatabaseHelper.h"

void real();
void sub();

int main() {


    
    //real();
    sub();





    return 0;
}

void real()
{

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
}

void sub()
{
    // �����ͺ��̽� ���� ����
    std::string host = "localhost";          // ��: "localhost"
    std::string username = "root";            // �����ͺ��̽� ����ڸ�
    std::string password = "13878490";            // �����ͺ��̽� ��й�ȣ
    std::string databaseName = "rpgsqldb";   // �����ͺ��̽� �̸�

    // DatabaseHelper ��ü ����
    DatabaseHelper dbHelper(host, username, password, databaseName);

    // �����ͺ��̽� ���� �õ�
    if (dbHelper.connect()) {
        std::cout << "Database connected successfully." << std::endl;

        // ���� ���� ����
        std::string query = "SELECT * FROM your_table_name";  // ��: ���̺��
        if (dbHelper.executeQuery(query)) {
            std::unique_ptr<sql::ResultSet> result = dbHelper.getResult(query);

            // ���� ��� ó��
            while (result->next()) {
                std::cout << "Column1: " << result->getString(1) << std::endl;
                std::cout << "Column2: " << result->getString(2) << std::endl;
            }
        }
        else {
            std::cerr << "Failed to execute query." << std::endl;
        }

        dbHelper.disconnect();  // �����ͺ��̽� ���� ����
    }
    else {
        std::cerr << "Failed to connect to the database." << std::endl;
    }

}