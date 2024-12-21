#ifndef SERVERINFO_H
#define SERVERINFO_H

#include <string>
#include <ctime>
#include "json.hpp"

class SubServer {
private:
    std::string ipAddress;  // ���� IP �ּ�
    int tcpPort;               // ���� ��Ʈ ��ȣ
    int udpPort;


public:
    // ������
    SubServer(const std::string& ip, int tcpPort, int udpPort);

    // ���� ���� ���� ����
    void connect();
    void disconnect();

    nlohmann::json subServerToJson() {
        return {
            {"ipAddress", ipAddress},
            {"tcpPort", tcpPort},
            {"udpPort", udpPort}
        };
    }

    // ���� ���� ���
    void printInfo() const;

    // Getter �Լ���
    std::string getIpAddress() const;
    int getTcpPort() const;
    int getUdpPort() const;
    //bool getIsConnected() const;
    //std::string getServerID() const;
};

#endif // SERVERINFO_H
