#pragma once

enum class ConnectionState
{
    Default,      // �⺻ ����
    Connecting,   // ���� �õ� ��
    DataSyncing,  // ������ ����ȭ ��
    Disconnecting,// ���� ���� �õ� ��
    Error,        // ���� �߻�
    TcpToUdp      // tcp���� udp�� �̵���Ŵ
};