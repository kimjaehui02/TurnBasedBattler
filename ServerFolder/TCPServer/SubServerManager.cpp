#include "SubServerManager.h"
#include "SubServer.h"
#include <iostream>
#include <ctime>

// �ʱ�ȭ�� ����ƽ �����
std::mutex SubServerManager::mtx;               // ����ȭ�� ���� ���ؽ�
std::list<SubServer> SubServerManager::subServers;  // SubServer ����Ʈ

// SubServerManager �����ڴ� �����ؼ� �ν��Ͻ� ���� �Ұ��ϰ� �ϱ�


// Ŭ���̾�Ʈ �� ����
void SubServerManager::incrementClientCount(const SubServer& server) 
{
    
    addSubServer(server);
    std::cerr << "������ �����Ͽ����ϴ� : " << SubServerManager::getClientCount() << std::endl;
}

// Ŭ���̾�Ʈ �� ����
void SubServerManager::decrementClientCount(const SubServer& server)
{
    
    std::cerr << "decrementClientCount ȣ���: " << server.getIpAddress() << ":" << server.getTcpPort() << server.getUdpPort() << std::endl;
    removeSubServer(server);
    std::cerr << "������ �����Ͽ����ϴ� : " << SubServerManager::getClientCount() << std::endl;
}


// ���� Ŭ���̾�Ʈ �� ��������
int SubServerManager::getClientCount() 
{

    return getSubServerCount();
}




// ���� ���� �߰�
void SubServerManager::addSubServer(const SubServer& server) 
{
    std::lock_guard<std::mutex> lock(mtx);  // ���ؽ��� �̿��� ����ȭ
    subServers.push_back(server);
}

// ���� ���� ����
void SubServerManager::removeSubServer(const SubServer& server)
{
    // ������ �������� �����ϱ� ���� mutex�� ����Ͽ� ���� ���� ����
    std::lock_guard<std::mutex> lock(mtx);

    // �α� ���: ���� ��� ������ IP �ּҿ� TCP/UDP ��Ʈ ��ȣ�� ���
    std::cerr << "removeSubServer ȣ���: " << server.getIpAddress()
        << ":" << server.getTcpPort() << ", " << server.getUdpPort() << std::endl;

    // ���ǿ� ���� ����Ʈ���� �ش� ���� ����
    // ������ IP �ּ�, TCP ��Ʈ, UDP ��Ʈ�� ��� ��ġ�ϴ� ��쿡�� ����
    subServers.remove_if([&server](const SubServer& s) {
        return s.getIpAddress() == server.getIpAddress() && // IP �ּ� ��ġ Ȯ��
            s.getTcpPort() == server.getTcpPort() &&     // TCP ��Ʈ ��ġ Ȯ��
            s.getUdpPort() == server.getUdpPort();       // UDP ��Ʈ ��ġ Ȯ��
        });
}

// ���� ���� ��� ��������
std::list<SubServer> SubServerManager::getSubServers() {
    //std::lock_guard<std::mutex> lock(mtx);  // ���ؽ��� �̿��� ����ȭ
    return subServers;  // ����Ʈ�� �����ؼ� ��ȯ
}

// ���� ���� ���� ��������
int SubServerManager::getSubServerCount() {
    //std::lock_guard<std::mutex> lock(mtx);  // ���ؽ��� �̿��� ����ȭ
    //std::cerr << "������ �����Ͽ����ϴ� : " << std::endl;
    return static_cast<int>(subServers.size()); // size_t�� int�� ��ȯ
}
