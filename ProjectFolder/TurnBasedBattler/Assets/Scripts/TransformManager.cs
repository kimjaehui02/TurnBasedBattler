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
        HashSet<int> receivedClientIds = new HashSet<int>(keyValuePairs.Keys);

        // 1. Ŭ���̾�Ʈ�� ����Ǿ��� �� ������Ʈ ����
        List<GameObject> objectsToRemove = new List<GameObject>();

        // ���� ������Ʈ�� ��ȸ�ϸ� Ŭ���̾�Ʈ ID�� �� �̻� ������ ����
        foreach (var parentObject in GameManager.Instance.ServerObjects)
        {
            int userId;
            if (int.TryParse(parentObject.name, out userId))
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



public struct ObjectTransform
{
    // ��ġ ����
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }

    // ȸ�� ���� (Quaternion ����)
    //public float RotationX { get; set; }
    //public float RotationY { get; set; }
    //public float RotationZ { get; set; }
    //public float RotationW { get; set; }

    // ������ ����
    //public float ScaleX { get; set; }
    //public float ScaleY { get; set; }
    //public float ScaleZ { get; set; }

    // ���� ������Ʈ �÷���
    //public bool IsUpdated { get; private set; }

    // �⺻���� �����ϴ� ������
    //public ObjectTransform()
    //{
    //    PositionX = 0f;
    //    PositionY = 0f;
    //    PositionZ = 0f;
    //    RotationX = 0f;
    //    RotationY = 0f;
    //    RotationZ = 0f;
    //    RotationW = 1f;
    //    ScaleX = 1f;
    //    ScaleY = 1f;
    //    ScaleZ = 1f;
    //    //IsUpdated = false;
    //}

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

    //public void UpdateRotation(float x, float y, float z, float w)
    //{
    //    if (RotationX != x || RotationY != y || RotationZ != z || RotationW != w)
    //    {
    //        RotationX = x;
    //        RotationY = y;
    //        RotationZ = z;
    //        RotationW = w;
    //        //IsUpdated = true;
    //    }
    //}

    //public void UpdateScale(float x, float y, float z)
    //{
    //    if (ScaleX != x || ScaleY != y || ScaleZ != z)
    //    {
    //        ScaleX = x;
    //        ScaleY = y;
    //        ScaleZ = z;
    //        //IsUpdated = true;
    //    }
    //}

    // ������Ʈ �� ���� ����
    /*    public void ResetUpdatedFlag()
        {
            //IsUpdated = false;
        }*/

    // ����׿� ���
    public override string ToString()
    {
        return $"Position: ({PositionX}, {PositionY}, {PositionZ}), ";
               //$"Rotation: ({RotationX}, {RotationY}, {RotationZ}, {RotationW}), ";
               //$"Scale: ({ScaleX}, {ScaleY}, {ScaleZ}), Updated: ";
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
                //RotationX = objTransform.rotation.x,
                //RotationY = objTransform.rotation.y,
                //RotationZ = objTransform.rotation.z,
                //RotationW = objTransform.rotation.w,
                //ScaleX = objTransform.localScale.x,
                //ScaleY = objTransform.localScale.y,
                //ScaleZ = objTransform.localScale.z
            };

            // ��ųʸ��� �߰�
            objectTransforms[key] = objectTransform;
        }

        return objectTransforms;
    }
}

