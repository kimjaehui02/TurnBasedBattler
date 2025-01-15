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
}

void sub()
{
    std::cout << "ClientInfo : " << mysql_get_client_info() << std::endl;
    // MySQL 데이터베이스 연결 정보
    const char* server = "localhost";  // MySQL 서버 주소 (로컬 서버일 경우 localhost)
    const char* user = "root";  // MySQL 사용자 이름
    const char* password = "13878490";  // MySQL 사용자 비밀번호
    const char* database = "rpgsqldb";  // 사용하려는 데이터베이스 이름

    MYSQL* conn = mysql_init(nullptr);  // MySQL 초기화
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

    // 예시: 데이터 조회 쿼리 실행
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

    // MySQL 연결 종료
    mysql_close(conn);

    //// 데이터베이스 정보 설정
    //std::string host = "localhost";          // 예: "localhost"
    //std::string username = "root";            // 데이터베이스 사용자명
    //std::string password = "13878490";            // 데이터베이스 비밀번호
    //std::string databaseName = "rpgsqldb";   // 데이터베이스 이름

    //// DatabaseHelper 객체 생성
    //DatabaseHelper dbHelper(host, username, password, databaseName);

    //// 데이터베이스 연결 시도
    //if (dbHelper.connect()) {
    //    std::cout << "Database connected successfully." << std::endl;

    //    // 쿼리 실행 예시
    //    std::string query = "SELECT * FROM your_table_name";  // 예: 테이블명
    //    if (dbHelper.executeQuery(query)) {
    //        std::unique_ptr<sql::ResultSet> result = dbHelper.getResult(query);

    //        // 쿼리 결과 처리
    //        while (result->next()) {
    //            std::cout << "Column1: " << result->getString(1) << std::endl;
    //            std::cout << "Column2: " << result->getString(2) << std::endl;
    //        }
    //    }
    //    else {
    //        std::cerr << "Failed to execute query." << std::endl;
    //    }

    //    dbHelper.disconnect();  // 데이터베이스 연결 해제
    //}
    //else {
    //    std::cerr << "Failed to connect to the database." << std::endl;
    //}

}