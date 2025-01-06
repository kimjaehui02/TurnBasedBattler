# RPG 게임의 메인 서버 및 서브 서버 구조 개발

## 1. 프로젝트 개요
<details>
<summary>프로젝트 개요 보기</summary>
  
이 프로젝트의 목표는 C++로 작성된 **메인 서버**, C#으로 작성된 **서브 서버**, 그리고 **유니티 클라이언트** 간의 효율적인 연결과 **위치 동기화** 기능을 구현하는 것이었습니다. 이 시스템은 멀티플레이어 RPG 게임에서 여러 서버 간의 효율적인 통신과 데이터 전송, 위치 동기화를 위한 기반을 제공합니다.
</details>

---

## 2. 기술 스택

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

## 4. 주요 기능 및 구현
<details>
<summary>주요 기능 보기</summary>
  
### 서버-클라이언트 연결
- **연결 방식**: **TCP/IP** 및 **WebSocket**을 사용하여 서버와 클라이언트 간 안정적인 연결을 유지
- **연결 초기화**: 세션 관리 및 클라이언트 연결을 위한 프로토콜 구현
- 
### 위치 동기화 기능
- **위치 동기화 알고리즘**:
  - 클라이언트는 일정 주기로 자신의 위치 정보를 서버에 전송.
  - 서버는 이 정보를 바탕으로 다른 클라이언트들과 위치를 동기화.
  - 주기적인 위치 전송을 통해 클라이언트들의 실시간 위치 정보를 정확하게 동기화함.

</details>

---

## 5. 기술적 도전과 해결 방법
<details>
<summary>기술적 도전과 해결 방법 보기</summary>
  
### 성능 최적화

- **서버 간 데이터 전송 최적화**:  
   - 데이터 압축, 효율적인 데이터 구조 사용
   - 직렬화/역직렬화 효율성 향상으로 데이터 전송 속도 및 대역폭 절감
   - 데이터 크기 최적화 (불필요한 데이터 제외, 필수 데이터만 전송)

- **클라이언트 로딩 속도 및 서버 응답 시간 최적화**:  
   - **응답 시간 최적화**:  
     - 네트워크 직렬화 및 역직렬화 성능 개선으로 서버와 클라이언트 간의 응답 시간 단축
     - 효율적인 직렬화 방식(예: JSON 최적화, 데이터 크기 감소)을 사용하여 네트워크 지연시간 최소화
     - 직렬화 시간 단축을 통해 실시간 데이터 전송 및 빠른 응답 보장
       
[Issue1.cs 파일 보기](https://github.com/kimjaehui02/TurnBasedBattler/blob/main/SimC/SimulationClass/Issue1.cs)



### 동기화 문제 해결
- **위치 동기화 정확성 보장**:
  - 알고리즘 및 기술을 통해 위치 동기화의 정확성 보장
  - 네트워크 지연이나 타이밍 문제를 해결하는 방법

### 에러 처리 및 복구
- **서버 다운 처리 및 재연결**: 클라이언트와의 재연결 및 서버 복구 처리
- **데이터 유실 방지 및 트랜잭션 처리**: 서버에서 클라이언트로의 데이터 전송 중 유실 방지
</details>

---

## 6. 테스트 및 디버깅
<details>
<summary>테스트 및 디버깅 보기</summary>
  
### 테스트 전략
- **서버-클라이언트 간 통신 테스트**
- **위치 동기화 및 클라이언트-서버 간 동기화 테스트**
- **서버와 클라이언트 에러 처리 테스트**

### 디버깅 과정
- **네트워크 관련 버그 해결**: 네트워크 통신과 관련된 문제 해결
- **동기화 오류 수정**: 위치 동기화 오류 및 타이밍 문제 해결
</details>

---

## 7. 향후 계획 및 개선 사항
<details>
<summary>향후 계획 보기</summary>
  
- **서버 확장성**: 더 많은 사용자를 지원할 수 있도록 서버 구조 확장
- **보안 강화**: 데이터 암호화 및 클라이언트-서버 간 안전한 통신 구현
- **위치 동기화 방식 개선**: 더 나은 동기화 알고리즘을 도입하여 정확성과 효율성 개선
</details>

---

## 8. 결론
<details>
<summary>결론 보기</summary>
이 프로젝트를 통해 얻은 경험과 교훈:
- 서버와 클라이언트 간의 안정적인 연결과 동기화 기능 구현을 통해 멀티플레이어 게임에서의 중요한 요소를 잘 이해하게 되었습니다.
- 향후, 해당 프로젝트는 성능 및 보안을 강화하고, 더 나은 게임 경험을 제공하기 위한 기반으로 발전할 수 있습니다.
</details>

---



### 코드 예시

```cpp
// 서버에서 클라이언트의 위치를 동기화하는 C++ 코드 예시
void SyncPlayerPosition(Player player) {
    // 클라이언트에서 받은 위치 데이터를 기반으로 동기화
    player.position = receivedPosition;
    // 서버에서 해당 위치를 클라이언트에게 전송
    SendPositionToClient(player);
}
```
## 10. 다이어그램




