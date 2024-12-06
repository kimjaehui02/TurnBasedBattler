// ServerState.cpp
#include "ServerState.h"

ServerState::ServerState() : clientCount(0) {}

void ServerState::incrementClientCount() {
    std::lock_guard<std::mutex> lock(mtx);  // 뮤텍스를 이용해 동기화
    ++clientCount;
}

void ServerState::decrementClientCount() {
    std::lock_guard<std::mutex> lock(mtx);  // 뮤텍스를 이용해 동기화
    if (clientCount > 0) {
        --clientCount;
    }
}

int ServerState::getClientCount() {
    std::lock_guard<std::mutex> lock(mtx);  // 뮤텍스를 이용해 동기화
    return clientCount;
}
