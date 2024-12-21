#include "SubServer.h"
#include <iostream>
#include <ctime>

// ������
SubServer::SubServer(const std::string& ip, int tcpPort, int udpPort) : ipAddress(ip), tcpPort(tcpPort), udpPort(udpPort) {}


// ���� ���� ���� ����
void SubServer::connect() {

}

void SubServer::disconnect() {

}

// ���� ���� ���
void SubServer::printInfo() const {
    // std::cout << "Server ID: " << serverID << "\n";
    std::cout << "IP Address: " << ipAddress << "\n";
    std::cout << "tcpPort: " << tcpPort << "\n";
    std::cout << "udpPort: " << udpPort << "\n";
    // std::cout << "Connected: " << (isConnected ? "Yes" : "No") << "\n"; 
    //if (isConnected) {
    //    std::cout << "Connection Time: " << ctime(&connectionTime);
    //}
}

// Getter �Լ���
std::string SubServer::getIpAddress() const {
    return ipAddress;
}

int SubServer::getTcpPort() const {
    return tcpPort;
}

int SubServer::getUdpPort() const {
    return udpPort;
}

//bool ServerInfo::getIsConnected() const {
//    // return isConnected;
//}
//
//std::string ServerInfo::getServerID() const {
//    // return serverID;
//}
