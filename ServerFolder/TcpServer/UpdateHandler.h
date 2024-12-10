#pragma once
#ifndef COMMUNICATIONHANDLER_H
#define COMMUNICATIONHANDLER_H

#include <iostream>
#include <string>
#include <winsock2.h>

class UpdateHandler {
public:
    UpdateHandler(SOCKET clientSocket);
    void processRequest(char* buffer, int bytesReceived);

    void HandleConnectionState();

private:
    void handleInitialDataRequest();
    void sendResponse(const std::string& response);

    SOCKET clientSocket;
};

#endif // COMMUNICATIONHANDLER_H
