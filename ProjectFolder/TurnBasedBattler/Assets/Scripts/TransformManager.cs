using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor.VersionControl;


public class TransformManager : MonoBehaviour
{
    public GameObject prefab;

    // ������Ʈ ��ȯ�� �ֽ�ȭ�ϴ� �Լ�
    public void UpdateObjectTransforms(Dictionary<int, Dictionary<int, ObjectTransform>> objectTransforms)
    {
        // ���� ������Ʈ ��ųʸ�
        var gameObjectTransforms = new Dictionary<int, Dictionary<int, GameObject>>();

        // ���� �÷��̾��� ID ��������
        int currentPlayerId = GameManager.Instance.GetPlayerId();

        // �ܺ� ��ųʸ� ��ȸ
        foreach (var outerPair in objectTransforms)
        {
            int playerId = outerPair.Key; // �÷��̾� ID

            // ���� �÷��̾��� ID�� ���Ͽ� ó��
            if (playerId == currentPlayerId)
                continue;  // ���� �÷��̾��� ������Ʈ�� ó������ ����

            var innerTransforms = outerPair.Value; // �ش� �÷��̾��� ObjectTransform ��ųʸ�

            if (!gameObjectTransforms.ContainsKey(playerId))
            {
                gameObjectTransforms[playerId] = new Dictionary<int, GameObject>();
            }

            // �ش� �÷��̾��� ��� ObjectTransform�� ���� ó��
            foreach (var innerPair in innerTransforms)
            {
                int objectId = innerPair.Key; // ������Ʈ ID
                ObjectTransform transformData = innerPair.Value;

                // �ش� ������Ʈ�� �����ϴ��� Ȯ��
                if (gameObjectTransforms[playerId].ContainsKey(objectId))
                {
                    // �̹� ������Ʈ�� ������ Ʈ�������� ������Ʈ
                    UpdateObjectTransform(gameObjectTransforms[playerId][objectId], transformData);
                }
                else
                {
                    // ������Ʈ�� ������ ���� ����
                    CreateNewObject(playerId, objectId, transformData, gameObjectTransforms);
                }
            }
        }

        // ������ ������Ʈ�� ó��
        RemoveDeletedObjects(objectTransforms);
    }

    // ���� ������Ʈ�� Transform�� �ֽ�ȭ�ϴ� �Լ�
    private void UpdateObjectTransform(GameObject obj, ObjectTransform transformData)
    {
        if (obj != null)
        {
            obj.transform.position = new Vector3(transformData.PositionX, transformData.PositionY, transformData.PositionZ);
            obj.transform.rotation = new Quaternion(transformData.RotationX, transformData.RotationY, transformData.RotationZ, transformData.RotationW);
            obj.transform.localScale = new Vector3(transformData.ScaleX, transformData.ScaleY, transformData.ScaleZ);
        }
    }

    // ���ο� ������Ʈ�� �����ϴ� �Լ� (�ʿ� ��)
    private void CreateNewObject(int playerId, int objectId, ObjectTransform transformData, Dictionary<int, Dictionary<int, GameObject>> gameObjectTransforms)
    {
        // ���÷� �������� ����Ͽ� ������Ʈ�� �����ϴ� ���
        GameObject newObject = Instantiate(prefab);

        if (newObject != null)
        {
            // �� ������Ʈ�� ��ųʸ��� �߰�
            gameObjectTransforms[playerId][objectId] = newObject;
            UpdateObjectTransform(newObject, transformData); // Transform �ֽ�ȭ
        }
        else
        {
            Debug.LogError("Prefab �ε� ����!");
        }
    }

    // ������ ������Ʈ�� ã�Ƽ� �����ϴ� �Լ�
    private void RemoveDeletedObjects(Dictionary<int, Dictionary<int, ObjectTransform>> objectTransforms)
    {
        // ���� ������Ʈ ��ųʸ����� ���� �����ϴ� ������Ʈ���� ����� ������
        foreach (var playerPair in GameManager.Instance.gameObjects2)
        {
            int playerId = playerPair.Key;
            var existingObjects = playerPair.Value;

            // ���ŵ� �����Ϳ��� �ش� �÷��̾� ID�� ������ ����
            if (!objectTransforms.ContainsKey(playerId))
            {
                DeleteAllObjects(playerId);
            }
            else
            {
                // �ش� �÷��̾��� ��� ������Ʈ�� ���Ͽ� ����
                foreach (var objectPair in existingObjects)
                {
                    int objectId = objectPair.Key;
                    if (!objectTransforms[playerId].ContainsKey(objectId))
                    {
                        DeleteObject(playerId, objectId);
                    }
                }
            }
        }
    }

    // �÷��̾��� ��� ������Ʈ�� �����ϴ� �Լ�
    private void DeleteAllObjects(int playerId)
    {
        if (GameManager.Instance.gameObjects2.ContainsKey(playerId))
        {
            var playerObjects = GameManager.Instance.gameObjects2[playerId];
            foreach (var objectPair in playerObjects)
            {
                DeleteObject(playerId, objectPair.Key);
            }
        }
    }

    // ������Ʈ�� �����ϴ� �Լ�
    private void DeleteObject(int playerId, int objectId)
    {
        if (GameManager.Instance.gameObjects2.ContainsKey(playerId) && GameManager.Instance.gameObjects2[playerId].ContainsKey(objectId))
        {
            GameObject objToRemove = GameManager.Instance.gameObjects2[playerId][objectId];
            if (objToRemove != null)
            {
                Destroy(objToRemove); // ������Ʈ ����
                GameManager.Instance.gameObjects2[playerId].Remove(objectId); // ��ųʸ����� ����
                Debug.Log($"Player {playerId}, Object {objectId} ����");
            }
        }
        else
        {
            Debug.LogWarning($"Player {playerId}, Object {objectId}�� �̹� ������");
        }
    }

    // �ܺο��� �����Ͱ� ���� ������ ȣ���ϴ� ����� ���� (��Ʈ��ũ ������ ���� �� ȣ��)
    public void OnObjectTransformsReceived(Dictionary<int, Dictionary<int, ObjectTransform>> receivedTransforms)
    {
        UpdateObjectTransforms(receivedTransforms);
    }
}



public class ObjectTransform
{
    // ��ġ ����
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }

    // ȸ�� ���� (Quaternion ����)
    public float RotationX { get; set; }
    public float RotationY { get; set; }
    public float RotationZ { get; set; }
    public float RotationW { get; set; }

    // ������ ����
    public float ScaleX { get; set; }
    public float ScaleY { get; set; }
    public float ScaleZ { get; set; }

    // ���� ������Ʈ �÷���
    //public bool IsUpdated { get; private set; }

    // �⺻���� �����ϴ� ������
    public ObjectTransform()
    {
        PositionX = 0f;
        PositionY = 0f;
        PositionZ = 0f;
        RotationX = 0f;
        RotationY = 0f;
        RotationZ = 0f;
        RotationW = 1f;
        ScaleX = 1f;
        ScaleY = 1f;
        ScaleZ = 1f;
        //IsUpdated = false;
    }

    // ������Ʈ �޼���
    public void UpdatePosition(float x, float y, float z)
    {
        if (PositionX != x || PositionY != y || PositionZ != z)
        {
            PositionX = x;
            PositionY = y;
            PositionZ = z;
            //IsUpdated = true;
        }
    }

    public void UpdateRotation(float x, float y, float z, float w)
    {
        if (RotationX != x || RotationY != y || RotationZ != z || RotationW != w)
        {
            RotationX = x;
            RotationY = y;
            RotationZ = z;
            RotationW = w;
            //IsUpdated = true;
        }
    }

    public void UpdateScale(float x, float y, float z)
    {
        if (ScaleX != x || ScaleY != y || ScaleZ != z)
        {
            ScaleX = x;
            ScaleY = y;
            ScaleZ = z;
            //IsUpdated = true;
        }
    }

    // ������Ʈ �� ���� ����
    /*    public void ResetUpdatedFlag()
        {
            //IsUpdated = false;
        }*/

    // ����׿� ���
    public override string ToString()
    {
        return $"Position: ({PositionX}, {PositionY}, {PositionZ}), " +
               $"Rotation: ({RotationX}, {RotationY}, {RotationZ}, {RotationW}), " +
               $"Scale: ({ScaleX}, {ScaleY}, {ScaleZ}), Updated: ";
    }
}



public class TransformConverter
{
    // Dictionary<int, GameObject>�� Dictionary<int, ObjectTransform>���� ��ȯ�ϴ� �޼���
    public static Dictionary<int, ObjectTransform> ConvertGameObjectsToTransforms(Dictionary<int, GameObject> gameObjects)
    {
        Dictionary<int, ObjectTransform> objectTransforms = new Dictionary<int, ObjectTransform>();

        foreach (var item in gameObjects)
        {
            int key = item.Key;  // GameObject�� ID
            GameObject obj = item.Value;

            // GameObject�� Ʈ������ ���� ���
            Transform objTransform = obj.transform;

            // ObjectTransform ��ü ���� �� �� �Ҵ�
            ObjectTransform objectTransform = new ObjectTransform()
            {
                PositionX = objTransform.position.x,
                PositionY = objTransform.position.y,
                PositionZ = objTransform.position.z,
                RotationX = objTransform.rotation.x,
                RotationY = objTransform.rotation.y,
                RotationZ = objTransform.rotation.z,
                RotationW = objTransform.rotation.w,
                ScaleX = objTransform.localScale.x,
                ScaleY = objTransform.localScale.y,
                ScaleZ = objTransform.localScale.z
            };

            // ��ųʸ��� �߰�
            objectTransforms[key] = objectTransform;
        }

        return objectTransforms;
    }
}

