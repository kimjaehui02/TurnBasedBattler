// ServerState.h
#ifndef SERVERSTATE_H
#define SERVERSTATE_H

#include <mutex>

class ServerState {
private:
    int clientCount;  // 서버에 접속한 클라이언트 수
    std::mutex mtx;   // 동기화를 위한 뮤텍스

public:
    ServerState();  // 생성자
    void incrementClientCount();  // 클라이언트 수 증가
    void decrementClientCount();  // 클라이언트 수 감소
    int getClientCount();         // 현재 클라이언트 수 가져오기
};

#endif // SERVERSTATE_H
