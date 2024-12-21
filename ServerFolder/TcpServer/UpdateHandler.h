#pragma once
#ifndef COMMUNICATIONHANDLER_H
#define COMMUNICATIONHANDLER_H

#include <iostream>
#include <string>
#include <winsock2.h>
#include <functional>
#include "json.hpp"
#include "ConnectionState.h"
using json = nlohmann::json;

typedef void (*SendToTCPClient)(SOCKET clientSocket, ConnectionState connectionState, const std::string& messageData);

class UpdateHandler {
public:
    UpdateHandler(SOCKET clientSocket);
    void HandleConnectionState(char* buffer, int bytesReceived, SendToTCPClient callback = nullptr);

    void HandleConnecting(const json& message, SendToTCPClient callback = nullptr);

    void HandleDataSyncing(const json& message, SendToTCPClient callback = nullptr);

    void HandleDisconnecting(const json& message, SendToTCPClient callback = nullptr);

    void HandleError(const json& message, SendToTCPClient callback = nullptr);

    void HandleTcpToUdp(const json& message, SendToTCPClient callback = nullptr);


private:
    void handleInitialDataRequest();
    

    SOCKET clientSocket;
};

#endif // COMMUNICATIONHANDLER_H
