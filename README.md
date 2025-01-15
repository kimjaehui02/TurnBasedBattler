# RPG 게임의 메인 서버 및 서브 서버 구조 개발

- [1. 프로젝트 개요](#1-프로젝트-개요)
- [2. 주요 기능 및 구현](#2-주요-기능-및-구현)
- [3. 시스템 아키텍처](#3-시스템-아키텍처)
- [4. 기술 스택](#4-기술-스택)
- [5. 서버간 통신 최적화](#5-서버간-통신-최적화)
- [6. 향후 계획 및 개선 사항](#6-향후-계획-및-개선-사항)
- [7. 다이어그램](#7-다이어그램)
- [8. 시연 영상](#8-시연-영상)

## 1. 프로젝트 개요
<details>
<summary>프로젝트 개요 보기</summary>
  
이 프로젝트의 목표는 C++로 작성된 **메인 서버**, C#으로 작성된 **서브 서버**, 그리고 **유니티 클라이언트** 간의 효율적인 연결과 **위치 동기화** 기능을 구현하는 것이었습니다. 이 시스템은 멀티플레이어 RPG 게임에서 여러 서버 간의 효율적인 통신과 데이터 전송, 위치 동기화를 위한 기반을 제공합니다.
</details>

---

## 2. 주요 기능 및 구현
<details>
<summary>주요 기능 보기</summary>
  
1. **C++ 멀티스레드 소켓 기반 메인 서버**:
   - 메인 서버는 C# 서브 서버와 **TCP 프로토콜**을 통해 안정적인 데이터 연결을 유지하며 서버 간 통신을 담당합니다.

2. **클라이언트 연결 및 분배**:
   - 클라이언트가 C++ 메인 서버에 연결을 요청하면, 메인 서버는 적절한 C# 서브 서버 정보를 전달하여 클라이언트와 서브 서버 간의 연결을 설정합니다.

3. **클라이언트 간 실시간 동기화**:
   - 클라이언트는 **C# 서브 서버**와 연결된 상태에서 데이터 통신을 수행합니다.
   - **UDP 프로토콜**을 사용하여 캐릭터의 위치 정보 등 실시간 데이터 동기화를 처리하며,
   - 기타 게임 이벤트 및 상태 정보는 **TCP 프로토콜**로 처리하여 신뢰성과 데이터 안정성을 보장합니다.

## 위치 동기화 및 데이터 처리

- **위치 동기화 알고리즘**:
  - 클라이언트는 일정 주기로 자신의 좌표 정보를 서브 서버에 전송하며, 서브 서버는 이를 처리하여 다른 클라이언트로 브로드캐스트합니다.
  - **UDP 프로토콜**을 통해 빠르고 효율적인 실시간 동기화를 구현합니다.

- **추가 데이터 통신**:
  - 위치 외의 데이터(예: 게임 이벤트, 상태 변경)는 **TCP 프로토콜**로 전송되며, 신뢰성과 순서를 보장합니다.

</details>



---

## 3. 시스템 아키텍처
<details>
<summary>시스템 아키텍처 보기</summary>
  
### 서버 아키텍처
- **메인 서버**: 게임의 핵심 로직 처리 및 데이터 저장 담당
- **서브 서버**: 서버 로드 분산, 위치 동기화, 이벤트 처리 담당
- **서버 간 통신**: **TCP**를 기반으로 메인 서버와 서브 서버 간의 데이터 전송. 안정적이고 순차적인 데이터 처리 및 연결을 유지하기 위해 TCP 프로토콜을 사용.


### 클라이언트 아키텍처
- 클라이언트는 서버와 실시간으로 데이터를 송수신하며 게임의 UI를 갱신합니다.
- 클라이언트에서 서버로 보내는 요청 및 응답 구조:
  - 위치 정보, 이벤트 처리 요청, 상태 업데이트 등
- 클라이언트에서 게임 데이터를 처리하고 UI를 갱신하는 방식:
  - 서버로부터 받은 데이터에 기반한 UI 업데이트 및 상태 반영
- **위치 정보 동기화**:
  - 캐릭터 오브젝트의 좌표 정보는 **UDP** 프로토콜을 사용하여 빠르고 효율적으로 전달. 
  - UDP를 사용하여 실시간으로 캐릭터의 위치를 동기화하고, 빠른 반응성을 제공합니다.
- **기타 정보 처리**:
  - 위치 외의 게임 상태 정보나 이벤트 처리 요청 등은 **TCP** 프로토콜을 통해 안정적인 데이터 전송을 보장하며 처리됩니다.

</details>

---

## 4. 기술 스택

<details>
<summary>기술 스택 보기</summary>

- **서버**
  - 메인 서버: **C++**
  - 서브 서버: **C#**
  
- **클라이언트**
  - Unity

- **통신 프로토콜**
  - **C++ 서버와 C# 서버 간의 통신**: **TCP** 프로토콜을 사용하여 안정적인 데이터 전송과 연결을 유지.
  - **C# 서버와 클라이언트 간의 통신**: **TCP**와 **UDP** 프로토콜을 혼합하여 사용. **TCP**는 연결 지향적이고 안정적인 데이터 전송을 위해, **UDP**는 빠른 데이터 전송이 필요한 실시간 업데이트와 상태 동기화를 위해 사용.

- **압축 기술**:
  - 데이터 전송 최적화를 위해 **Gzip**을 사용하여 데이터를 압축하여 전송 효율성을 높임.

- **개발 도구**:
  - 개발 환경: **Visual Studio 2019**, **Visual Studio 2022**

- **라이브러리 및 프레임워크**:
  - **JSON 라이브러리**: 
    - C#: **Newtonsoft.Json** 라이브러리 사용하여 서버와 클라이언트 간 데이터 직렬화/역직렬화 처리
    - C++: **nlohmann/json** 라이브러리를 통해 JSON 처리 작업 수행

</details>

---

## 5. 서버간 통신 최적화
<details>
<summary>기술적 도전과 해결 방법 보기</summary>
  
### 성능 최적화

- **서버 간 데이터 전송 최적화**:  
   - Gzip으로 패킷 데이터 압축, 16바이트 이하 구조체 사용
   - 직렬화/역직렬화 효율성 향상으로 데이터 전송 속도 및 대역폭 절감
   - 데이터 크기 최적화 (불필요한 데이터 제외, 필수 데이터만 전송)

[JsonCompressionManager.cs 파일 보기](https://github.com/kimjaehui02/TurnBasedBattler/blob/main/ServerFolder/UDPServer/JsonCompressionManager.cs)

- **클라이언트 로딩 속도 및 서버 응답 시간 최적화**:  
   - **응답 시간 최적화**:  
     - 네트워크 직렬화 및 역직렬화 성능 개선으로 서버와 클라이언트 간의 응답 시간 단축
     - 16바이트 이하 구조체 직렬화 방식을 사용하여 네트워크 지연시간 최소화
     - 직렬화 시간 단축을 통해 실시간 데이터 전송 및 빠른 응답 보장
       
## 성능 비교

#### 1. JSON 직렬화/역직렬화 성능 (클래스 사용)

- **Original JSON Serialization**: 190ms  
- **Original JSON Deserialization**: 29ms  

#### 2. JSON 직렬화/역직렬화 성능 (구조체 사용)

- **Compact JSON Serialization**: 2ms  
- **Compact JSON Deserialization**: 0ms  

### 참고:
- 해당 성능 테스트는 [여기](https://github.com/kimjaehui02/TurnBasedBattler/blob/main/SimC/SimulationClass/Issue1.cs)에서 확인할 수 있습니다. 


## 참고 문서

- [클래스와 구조체 간의 선택 - Framework Design Guidelines](https://learn.microsoft.com/ko-kr/dotnet/standard/design-guidelines/choosing-between-class-and-struct)


### 동기화 문제 해결
- **위치 동기화 정확성 보장**:
  - 알고리즘 및 기술을 통해 위치 동기화의 정확성 보장
  - 네트워크 지연이나 타이밍 문제를 해결하는 방법




</details>



---

## 6. 향후 계획 및 개선 사항
<details>
<summary>향후 계획 보기</summary>

  - **리팩토링**:
  - 오래된 통신 코드와 클래스를 직관적이고 유지보수 가능하게 변경
- **MySQL과 Firebase 연결**:
  - 서버와 클라이언트를 위한 데이터베이스 연결을 목표로 함
  - 사용자 데이터 관리 및 실시간 상태 동기화를 위한 데이터베이스 통합
  - 데이터 저장소를 MySQL과 Firebase로 분리하여 확장성을 높임
- **서버 선택 기능 및 재연결 기능 추가**:
  - 여러 서버 중에서 클라이언트가 적절한 서버를 선택할 수 있는 기능 구현
  - 연결이 끊길 경우 재연결 및 서버 상태 확인 기능 추가
- **클라이언트 컨텐츠 추가**:
  - 새로운 게임 컨텐츠(아이템, 몬스터 등)를 클라이언트에 추가하여 지속적인 게임 확장 가능성 확보

</details>


---



## 7. 다이어그램

<details>
<summary>다이어그램 보기</summary>
  
![다이어그램](https://github.com/kimjaehui02/TurnBasedBattler/blob/main/one.png)

</details>


---



## 8. 시연 영상

<details>
<summary>시연영상 보기</summary>
  
[![시연 영상](https://img.youtube.com/vi/fYYXfPfBqIY/0.jpg)](https://youtu.be/fYYXfPfBqIY)

</details>
