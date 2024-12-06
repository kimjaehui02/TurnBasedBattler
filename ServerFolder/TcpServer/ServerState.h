// ServerState.h
#ifndef SERVERSTATE_H
#define SERVERSTATE_H

#include <mutex>

class ServerState {
private:
    int clientCount;  // ������ ������ Ŭ���̾�Ʈ ��
    std::mutex mtx;   // ����ȭ�� ���� ���ؽ�

public:
    ServerState();  // ������
    void incrementClientCount();  // Ŭ���̾�Ʈ �� ����
    void decrementClientCount();  // Ŭ���̾�Ʈ �� ����
    int getClientCount();         // ���� Ŭ���̾�Ʈ �� ��������
};

#endif // SERVERSTATE_H
