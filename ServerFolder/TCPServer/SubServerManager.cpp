#include "SubServerManager.h"
#include "SubServer.h"
#include <iostream>
#include <ctime>

// 초기화된 스태틱 멤버들
std::mutex SubServerManager::mtx;               // 동기화를 위한 뮤텍스
std::list<SubServer> SubServerManager::subServers;  // SubServer 리스트

// SubServerManager 생성자는 삭제해서 인스턴스 생성 불가하게 하기


// 클라이언트 수 증가
void SubServerManager::incrementClientCount(const SubServer& server) 
{
    
    addSubServer(server);
    std::cerr << "서버가 증가하였습니다 : " << SubServerManager::getClientCount() << std::endl;
}

// 클라이언트 수 감소
void SubServerManager::decrementClientCount(const SubServer& server)
{
    
    std::cerr << "decrementClientCount 호출됨: " << server.getIpAddress() << ":" << server.getTcpPort() << server.getUdpPort() << std::endl;
    removeSubServer(server);
    std::cerr << "서버가 감소하였습니다 : " << SubServerManager::getClientCount() << std::endl;
}


// 현재 클라이언트 수 가져오기
int SubServerManager::getClientCount() 
{

    return getSubServerCount();
}




// 서브 서버 추가
void SubServerManager::addSubServer(const SubServer& server) 
{
    std::lock_guard<std::mutex> lock(mtx);  // 뮤텍스를 이용한 동기화
    subServers.push_back(server);
}

// 서브 서버 제거
void SubServerManager::removeSubServer(const SubServer& server)
{
    // 스레드 안전성을 보장하기 위해 mutex를 사용하여 동시 접근 방지
    std::lock_guard<std::mutex> lock(mtx);

    // 로그 출력: 제거 대상 서버의 IP 주소와 TCP/UDP 포트 번호를 출력
    std::cerr << "removeSubServer 호출됨: " << server.getIpAddress()
        << ":" << server.getTcpPort() << ", " << server.getUdpPort() << std::endl;

    // 조건에 따라 리스트에서 해당 서버 제거
    // 서버의 IP 주소, TCP 포트, UDP 포트가 모두 일치하는 경우에만 제거
    subServers.remove_if([&server](const SubServer& s) {
        return s.getIpAddress() == server.getIpAddress() && // IP 주소 일치 확인
            s.getTcpPort() == server.getTcpPort() &&     // TCP 포트 일치 확인
            s.getUdpPort() == server.getUdpPort();       // UDP 포트 일치 확인
        });
}

// 서브 서버 목록 가져오기
std::list<SubServer> SubServerManager::getSubServers() {
    //std::lock_guard<std::mutex> lock(mtx);  // 뮤텍스를 이용한 동기화
    return subServers;  // 리스트를 복사해서 반환
}

// 서브 서버 개수 가져오기
int SubServerManager::getSubServerCount() {
    //std::lock_guard<std::mutex> lock(mtx);  // 뮤텍스를 이용한 동기화
    //std::cerr << "서버가 감소하였습니다 : " << std::endl;
    return static_cast<int>(subServers.size()); // size_t를 int로 변환
}
