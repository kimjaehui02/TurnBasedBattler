using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ServerUIManager : MonoBehaviour
{
    public GameObject serverUIPrefab;  // 배치할 프리펩
    public Transform parentContainer;  // 상자들이 부모로 배치될 컨테이너

    private List<ServerInfo> serverInfos;
    public List<GameObject> gameObjects;

    private Stack<GameObject> objectPool = new Stack<GameObject>();

    private void CreateServerUI(int numberOfRows)
    {
        ClearServerUI();  // 기존 인스턴스 정리

        float startX = -240;  // 첫 번째 상자의 X 좌표
        float spacingX = 480; // 가로 간격
        float startY = 350;   // 첫 번째 상자의 Y 좌표
        float spacingY = 250; // 세로 간격

        int numb = 0;
        int serversPerRow = 2;
        for (int row = 0; row < numberOfRows; row++)
        {
            if (numberOfRows == numb)
            {
                break;
            }
            for (int col = 0; col < serversPerRow; col++)
            {
                if (numberOfRows == numb)
                {
                    break;
                }

                GameObject serverUI;
                if (objectPool.Count > 0)
                {
                    serverUI = objectPool.Pop();
                    serverUI.SetActive(true);
                }
                else
                {
                    serverUI = Instantiate(serverUIPrefab, parentContainer);
                }

                serverUI.GetComponent<SingleServerUIManager>().UIUpdate(numb, serverInfos[numb]);
                numb++;

                // 계산된 위치 설정
                float xPos = startX + col * spacingX;
                float yPos = startY - row * spacingY;

                // UI 배치 설정
                serverUI.transform.localPosition = new Vector3(xPos, yPos, 0);
                // 생성된 인스턴스를 리스트에 추가
                gameObjects.Add(serverUI);
            }
        }
    }

    private void ClearServerUI()
    {
        foreach (GameObject obj in gameObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);  // 풀링을 위해 비활성화
                objectPool.Push(obj);   // 풀링을 위한 관리
            }
        }
        gameObjects.Clear();  // 리스트 비우기
    }





    public void UpdateModel(List<ServerInfo> input)
    {
        Debug.Log("public void UpdateModel(List<ServerInfo> input)");
        Debug.Log("public void UpdateModel(List<ServerInfo> input)");
        Debug.Log("public void UpdateModel(List<ServerInfo> input)");
        Debug.Log("public void UpdateModel(List<ServerInfo> input)");

        serverInfos = input;
        CreateServerUI(input.Count);
    }

    public void UpdateUi()
    {
        
    }

}

