#pragma once
#ifndef COMMUNICATIONHANDLER_H
#define COMMUNICATIONHANDLER_H

#include <iostream>
#include <string>
#include <winsock2.h>
#include "json.hpp"
using json = nlohmann::json;

class UpdateHandler {
public:
    UpdateHandler(SOCKET clientSocket);
    void HandleConnectionState(char* buffer, int bytesReceived);

    void HandleConnecting(const json& message);

    void HandleDataSyncing(const json& message);

    void HandleDisconnecting(const json& message);

    void HandleError(const json& message);

    void HandleTcpToUdp(const json& message);


private:
    void handleInitialDataRequest();
    void sendResponse(const std::string& response);

    SOCKET clientSocket;
};

#endif // COMMUNICATIONHANDLER_H
