#include <iostream>
#include <string>
#include <thread>
#include "Server.h"
//#include "DatabaseHelper.h"
#include <mysql.h>
void real();
void sub();

int main() {


    
    real();
    //sub();





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
    std::cout << "ClientInfo : " << mysql_get_client_info() << std::endl;
    // MySQL �����ͺ��̽� ���� ����
    const char* server = "localhost";  // MySQL ���� �ּ� (���� ������ ��� localhost)
    const char* user = "root";  // MySQL ����� �̸�
    const char* password = "13878490";  // MySQL ����� ��й�ȣ
    const char* database = "rpgsqldb";  // ����Ϸ��� �����ͺ��̽� �̸�

    MYSQL* conn = mysql_init(nullptr);  // MySQL �ʱ�ȭ
    if (conn == nullptr) {
        std::cerr << "mysql_init() failed\n";
        return;
    }

    if (mysql_real_connect(conn, server, user, password, database, 0, nullptr, 0) == nullptr) {
        std::cerr << "mysql_real_connect() failed\n";
        mysql_close(conn);
        return;
    }

    std::cout << "ClientInfo : " << mysql_get_client_info() << std::endl;

    // ����: ������ ��ȸ ���� ����
    const char* query = "SELECT * FROM Players";
    if (mysql_query(conn, query)) {
        std::cerr << "QUERY failed: " << mysql_error(conn) << std::endl;
    }
    else {
        MYSQL_RES* result = mysql_store_result(conn);
        if (result) {
            int num_fields = mysql_num_fields(result);
            while (MYSQL_ROW row = mysql_fetch_row(result)) {
                for (int i = 0; i < num_fields; i++) {
                    if (row[i]) {
                        std::cout << row[i] << "\t";
                    }
                    else {
                        std::cout << "NULL\t";
                    }
                }
                std::cout << std::endl;
            }
            mysql_free_result(result);
        }
    }

    // MySQL ���� ����
    mysql_close(conn);

    //// �����ͺ��̽� ���� ����
    //std::string host = "localhost";          // ��: "localhost"
    //std::string username = "root";            // �����ͺ��̽� ����ڸ�
    //std::string password = "13878490";            // �����ͺ��̽� ��й�ȣ
    //std::string databaseName = "rpgsqldb";   // �����ͺ��̽� �̸�

    //// DatabaseHelper ��ü ����
    //DatabaseHelper dbHelper(host, username, password, databaseName);

    //// �����ͺ��̽� ���� �õ�
    //if (dbHelper.connect()) {
    //    std::cout << "Database connected successfully." << std::endl;

    //    // ���� ���� ����
    //    std::string query = "SELECT * FROM your_table_name";  // ��: ���̺��
    //    if (dbHelper.executeQuery(query)) {
    //        std::unique_ptr<sql::ResultSet> result = dbHelper.getResult(query);

    //        // ���� ��� ó��
    //        while (result->next()) {
    //            std::cout << "Column1: " << result->getString(1) << std::endl;
    //            std::cout << "Column2: " << result->getString(2) << std::endl;
    //        }
    //    }
    //    else {
    //        std::cerr << "Failed to execute query." << std::endl;
    //    }

    //    dbHelper.disconnect();  // �����ͺ��̽� ���� ����
    //}
    //else {
    //    std::cerr << "Failed to connect to the database." << std::endl;
    //}

}