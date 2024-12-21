#ifndef SERVERINFO_H
#define SERVERINFO_H

#include <string>
#include <ctime>
#include "json.hpp"

class SubServer {
private:
    std::string ipAddress;  // 서버 IP 주소
    int tcpPort;               // 서버 포트 번호
    int udpPort;


public:
    // 생성자
    SubServer(const std::string& ip, int tcpPort, int udpPort);

    // 서버 연결 상태 변경
    void connect();
    void disconnect();

    nlohmann::json subServerToJson() {
        return {
            {"ipAddress", ipAddress},
            {"tcpPort", tcpPort},
            {"udpPort", udpPort}
        };
    }

    // 서버 정보 출력
    void printInfo() const;

    // Getter 함수들
    std::string getIpAddress() const;
    int getTcpPort() const;
    int getUdpPort() const;
    //bool getIsConnected() const;
    //std::string getServerID() const;
};

#endif // SERVERINFO_H
