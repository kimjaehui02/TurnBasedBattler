#pragma once

#ifndef DATABASEHELPER_H
#define DATABASEHELPER_H

#include <cppconn/driver.h>
#include <cppconn/connection.h>
#include <cppconn/statement.h>
#include <cppconn/resultset.h>
#include <string>
#include <iostream>
#include <memory>  // std::unique_ptr

class DatabaseHelper {
public:
    DatabaseHelper(const std::string& host, const std::string& username, const std::string& password, const std::string& databaseName);
    ~DatabaseHelper();

    bool connect();
    void disconnect();
    bool executeQuery(const std::string& query);
    std::unique_ptr<sql::ResultSet> getResult(const std::string& query);

private:
    std::string host_;
    std::string username_;
    std::string password_;
    std::string databaseName_;
    std::unique_ptr<sql::Connection> connection_;
};

// 함수의 정의 포함
DatabaseHelper::DatabaseHelper(const std::string& host, const std::string& username, const std::string& password, const std::string& databaseName)
    : host_(host), username_(username), password_(password), databaseName_(databaseName) {}

DatabaseHelper::~DatabaseHelper() {
    disconnect();
}

bool DatabaseHelper::connect() {
    try {
        sql::Driver* driver = get_driver_instance();
        connection_ = std::unique_ptr<sql::Connection>(driver->connect(host_, username_, password_));
        connection_->setSchema(databaseName_);
        return true;
    }
    catch (sql::SQLException& e) {
        std::cerr << "SQL error: " << e.what() << std::endl;
        return false;
    }
}

void DatabaseHelper::disconnect() {
    if (connection_) {
        connection_->close();
    }
}

bool DatabaseHelper::executeQuery(const std::string& query) {
    try {
        std::unique_ptr<sql::Statement> stmt(connection_->createStatement());
        stmt->execute(query);
        return true;
    }
    catch (sql::SQLException& e) {
        std::cerr << "SQL error: " << e.what() << std::endl;
        return false;
    }
}

std::unique_ptr<sql::ResultSet> DatabaseHelper::getResult(const std::string& query) {
    try {
        std::unique_ptr<sql::Statement> stmt(connection_->createStatement());
        std::unique_ptr<sql::ResultSet> res(stmt->executeQuery(query));
        return std::move(res);
    }
    catch (sql::SQLException& e) {
        std::cerr << "SQL error: " << e.what() << std::endl;
        return nullptr;
    }
}

#endif // DATABASEHELPER_H
