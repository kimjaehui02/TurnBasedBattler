#pragma once
#ifndef COMMUNICATIONHANDLER_H
#define COMMUNICATIONHANDLER_H

#include <iostream>
#include <string>
#include <winsock2.h>
#include <functional>
#include "json.hpp"
#include "Server.h"

#include "ConnectionState.h"
using json = nlohmann::json;

// º°Äª Á¤ÀÇ
using SendFunction = std::function<void(SOCKET, ConnectionState, const nlohmann::json&)>;




class UpdateHandler {
public:
    UpdateHandler(SOCKET clientSocket);
    void HandleConnectionState(char* buffer, int bytesReceived, const SendFunction& callback = nullptr);

    void HandleConnecting(const json& message, const SendFunction& callback = nullptr);

    void HandleDataSyncing(const json& message, const SendFunction& callback = nullptr);

    void HandleDisconnecting(const json& message, const SendFunction& callback = nullptr);

    void HandleError(const json& message, const SendFunction& callback = nullptr);

    void HandleTcpToUdp(const json& message, const SendFunction& callback = nullptr);


private:
    void handleInitialDataRequest();
    
    void returnListOfServer(const SendFunction& sendFunction);

    SOCKET clientSocket;
};

#endif // COMMUNICATIONHANDLER_H
