// ServerState.cpp
#include "ServerState.h"

ServerState::ServerState() : clientCount(0) {}

void ServerState::incrementClientCount() {
    std::lock_guard<std::mutex> lock(mtx);  // ���ؽ��� �̿��� ����ȭ
    ++clientCount;
}

void ServerState::decrementClientCount() {
    std::lock_guard<std::mutex> lock(mtx);  // ���ؽ��� �̿��� ����ȭ
    if (clientCount > 0) {
        --clientCount;
    }
}

int ServerState::getClientCount() {
    std::lock_guard<std::mutex> lock(mtx);  // ���ؽ��� �̿��� ����ȭ
    return clientCount;
}
