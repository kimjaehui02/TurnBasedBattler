using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ServerUIManager : MonoBehaviour
{
    public GameObject serverUIPrefab;  // ��ġ�� ������
    public Transform parentContainer;  // ���ڵ��� �θ�� ��ġ�� �����̳�

    private List<ServerInfo> serverInfos;
    public List<GameObject> gameObjects;

    private Stack<GameObject> objectPool = new Stack<GameObject>();

    private void CreateServerUI(int numberOfRows)
    {
        ClearServerUI();  // ���� �ν��Ͻ� ����

        float startX = -240;  // ù ��° ������ X ��ǥ
        float spacingX = 480; // ���� ����
        float startY = 350;   // ù ��° ������ Y ��ǥ
        float spacingY = 250; // ���� ����

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

                // ���� ��ġ ����
                float xPos = startX + col * spacingX;
                float yPos = startY - row * spacingY;

                // UI ��ġ ����
                serverUI.transform.localPosition = new Vector3(xPos, yPos, 0);
                // ������ �ν��Ͻ��� ����Ʈ�� �߰�
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
                obj.SetActive(false);  // Ǯ���� ���� ��Ȱ��ȭ
                objectPool.Push(obj);   // Ǯ���� ���� ����
            }
        }
        gameObjects.Clear();  // ����Ʈ ����
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

