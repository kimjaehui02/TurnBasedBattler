// ServerState.h
#ifndef SERVERSTATE_H
#define SERVERSTATE_H

#include <mutex>
#include <list>
#include "SubServer.h"

class SubServerManager 
{

private:
    static std::mutex mtx;               // 동기화를 위한 뮤텍스
    static std::list<SubServer> subServers;  // SubServer 리스트

    // 서브 서버 관리 메서드
    static void addSubServer(const SubServer& server);   // 서브 서버 추가
    static void removeSubServer(const SubServer& server); // 서브 서버 제거
    static int getSubServerCount();
    static std::list<SubServer> getSubServers();

public:
    SubServerManager() = delete;  // 생성자는 삭제하여 인스턴스를 생성하지 못하게 함

    static nlohmann::json subServerListToJson() {
        nlohmann::json jsonArray = nlohmann::json::array();

        for (SubServer& server : subServers) {
            jsonArray.push_back(server.subServerToJson());
        }

        return jsonArray;
    }

    static void incrementClientCount(const SubServer& server);  // 클라이언트 수 증가
    static void decrementClientCount(const SubServer& server);  // 클라이언트 수 감소
    static int getClientCount();         // 현재 클라이언트 수 가져오기
};

#endif // SERVERSTATE_H
