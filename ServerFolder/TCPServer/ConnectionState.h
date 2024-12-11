#pragma once

enum class ConnectionState
{
    Default,      // 기본 상태
    Connecting,   // 연결 시도 중
    DataSyncing,  // 데이터 동기화 중
    Disconnecting,// 연결 종료 시도 중
    Error,        // 오류 발생
    TcpToUdp      // tcp에서 udp로 이동시킴
};