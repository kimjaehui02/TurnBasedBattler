using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
//using UnityEditor.VersionControl;


public class TransformManager : MonoBehaviour
{
    public GameObject prefab;




    public void OnObjectTransformsReceived2(in Dictionary<string, Dictionary<string, ObjectTransform>> receivedTransforms)
    {
        // ���ο� Dictionary<int, Dictionary<int, ObjectTransform>> ����
        var convertedTransforms = new Dictionary<int, Dictionary<int, ObjectTransform>>();

        foreach (var outerEntry in receivedTransforms)
        {
            // �ܺ� Ű(string)�� int�� ��ȯ
            if (int.TryParse(outerEntry.Key, out int outerKey))
            {
                var innerDict = new Dictionary<int, ObjectTransform>();

                foreach (var innerEntry in outerEntry.Value)
                {
                    // ���� Ű(string)�� int�� ��ȯ
                    if (int.TryParse(innerEntry.Key, out int innerKey))
                    {
                        innerDict[innerKey] = innerEntry.Value;
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to convert inner key '{innerEntry.Key}' to int.");
                    }
                }

                convertedTransforms[outerKey] = innerDict;
            }
            else
            {
                Debug.LogWarning($"Failed to convert outer key '{outerEntry.Key}' to int.");
            }
        }

        // ��ȯ�� �����͸� ���� �޼���� ����
        ObjectUpdate(in convertedTransforms);
    }
    public void ObjectUpdate(in Dictionary<int, Dictionary<int, ObjectTransform>> keyValuePairs)
    {
        // �������� ���� Ŭ���̾�Ʈ ���
        HashSet<int> receivedClientIds = new (keyValuePairs.Keys);

        // 1. Ŭ���̾�Ʈ�� ����Ǿ��� �� ������Ʈ ����
        List<GameObject> objectsToRemove = new();

        // ���� ������Ʈ�� ��ȸ�ϸ� Ŭ���̾�Ʈ ID�� �� �̻� ������ ����
        foreach (var parentObject in GameManager.Instance.ServerObjects)
        {
            if (int.TryParse(parentObject.name, out int userId))
            {
                if (!receivedClientIds.Contains(userId))
                {
                    objectsToRemove.Add(parentObject);
                }
            }
        }

        // �����ؾ� �� ������Ʈ���� ����
        foreach (var objectToRemove in objectsToRemove)
        {
            GameManager.Instance.ServerObjects.Remove(objectToRemove);
            Destroy(objectToRemove); // ���� ������Ʈ ����
        }

        // 2. ���ο� ������Ʈ �߰� �� ���� ������Ʈ ������Ʈ
        foreach (var userPair in keyValuePairs)
        {
            int userId = userPair.Key; // ���� ID
            Dictionary<int, ObjectTransform> objects = userPair.Value;

            // ���� ID�� �ش��ϴ� �θ� ������Ʈ ã��
            GameObject parentObject = GameManager.Instance.ServerObjects.Find(obj => obj.name == userId.ToString());

            // �θ� ������Ʈ�� ������ ����
            if (parentObject == null)
            {
                parentObject = new GameObject(userId.ToString());
                GameManager.Instance.ServerObjects.Add(parentObject);
            }

            // �ڽ� ������Ʈ ������Ʈ
            foreach (var objectPair in objects)
            {
                int objectId = objectPair.Key; // ������Ʈ ID
                ObjectTransform transformData = objectPair.Value;

                // �ڽ� ������Ʈ�� ������ ���������� ����
                Transform childTransform = objectId < parentObject.transform.childCount
                    ? parentObject.transform.GetChild(objectId) // �̹� �����ϸ� ��������
                    : null;

                if (childTransform == null)
                {
                    GameObject childObject = Instantiate(prefab, parentObject.transform); // ���������� ����
                    childObject.name = objectId.ToString(); // �̸� ����
                    childTransform = childObject.transform;
                }

                // �ڽ� ������Ʈ�� Transform �� ������Ʈ
                LerpUpdate(in childTransform, in transformData);
            }
        }
    }



    public void LerpUpdate(in Transform childTransform, in ObjectTransform transformData)
    {
        // �ֽ�ȭ�� ������ �ٷ� ����
        childTransform.localPosition = new Vector3(transformData.PositionX, transformData.PositionY, transformData.PositionZ);
        //childTransform.localRotation = new Quaternion(transformData.RotationX, transformData.RotationY, transformData.RotationZ, transformData.RotationW);
        //childTransform.localScale = new Vector3(transformData.ScaleX, transformData.ScaleY, transformData.ScaleZ);
        
    }




}




