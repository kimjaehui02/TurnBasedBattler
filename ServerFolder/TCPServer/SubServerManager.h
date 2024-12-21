// ServerState.h
#ifndef SERVERSTATE_H
#define SERVERSTATE_H

#include <mutex>
#include <list>
#include "SubServer.h"

class SubServerManager 
{

private:
    static std::mutex mtx;               // ����ȭ�� ���� ���ؽ�
    static std::list<SubServer> subServers;  // SubServer ����Ʈ

    // ���� ���� ���� �޼���
    static void addSubServer(const SubServer& server);   // ���� ���� �߰�
    static void removeSubServer(const SubServer& server); // ���� ���� ����
    static int getSubServerCount();
    static std::list<SubServer> getSubServers();

public:
    SubServerManager() = delete;  // �����ڴ� �����Ͽ� �ν��Ͻ��� �������� ���ϰ� ��

    static nlohmann::json subServerListToJson() {
        nlohmann::json jsonArray = nlohmann::json::array();

        for (SubServer& server : subServers) {
            jsonArray.push_back(server.subServerToJson());
        }

        return jsonArray;
    }

    static void incrementClientCount(const SubServer& server);  // Ŭ���̾�Ʈ �� ����
    static void decrementClientCount(const SubServer& server);  // Ŭ���̾�Ʈ �� ����
    static int getClientCount();         // ���� Ŭ���̾�Ʈ �� ��������
};

#endif // SERVERSTATE_H
