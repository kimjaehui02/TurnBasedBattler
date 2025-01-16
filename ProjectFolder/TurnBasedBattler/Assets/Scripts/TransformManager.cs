using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
//using UnityEditor.VersionControl;


public class TransformManager : MonoBehaviour
{
    public GameObject prefab;




    public void OnObjectTransformsReceived2(in Dictionary<string, Dictionary<string, ObjectTransform>> receivedTransforms)
    {
        // 새로운 Dictionary<int, Dictionary<int, ObjectTransform>> 생성
        var convertedTransforms = new Dictionary<int, Dictionary<int, ObjectTransform>>();

        foreach (var outerEntry in receivedTransforms)
        {
            // 외부 키(string)를 int로 변환
            if (int.TryParse(outerEntry.Key, out int outerKey))
            {
                var innerDict = new Dictionary<int, ObjectTransform>();

                foreach (var innerEntry in outerEntry.Value)
                {
                    // 내부 키(string)를 int로 변환
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

        // 변환된 데이터를 다음 메서드로 전달
        ObjectUpdate(in convertedTransforms);
    }
    public void ObjectUpdate(in Dictionary<int, Dictionary<int, ObjectTransform>> keyValuePairs)
    {
        // 서버에서 받은 클라이언트 목록
        HashSet<int> receivedClientIds = new (keyValuePairs.Keys);

        // 1. 클라이언트가 종료되었을 때 오브젝트 제거
        List<GameObject> objectsToRemove = new();

        // 기존 오브젝트들 순회하며 클라이언트 ID가 더 이상 없으면 제거
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

        // 제거해야 할 오브젝트들을 삭제
        foreach (var objectToRemove in objectsToRemove)
        {
            GameManager.Instance.ServerObjects.Remove(objectToRemove);
            Destroy(objectToRemove); // 게임 오브젝트 삭제
        }

        // 2. 새로운 오브젝트 추가 및 기존 오브젝트 업데이트
        foreach (var userPair in keyValuePairs)
        {
            int userId = userPair.Key; // 유저 ID
            Dictionary<int, ObjectTransform> objects = userPair.Value;

            // 유저 ID에 해당하는 부모 오브젝트 찾기
            GameObject parentObject = GameManager.Instance.ServerObjects.Find(obj => obj.name == userId.ToString());

            // 부모 오브젝트가 없으면 생성
            if (parentObject == null)
            {
                parentObject = new GameObject(userId.ToString());
                GameManager.Instance.ServerObjects.Add(parentObject);
            }

            // 자식 오브젝트 업데이트
            foreach (var objectPair in objects)
            {
                int objectId = objectPair.Key; // 오브젝트 ID
                ObjectTransform transformData = objectPair.Value;

                // 자식 오브젝트가 없으면 프리팹으로 생성
                Transform childTransform = objectId < parentObject.transform.childCount
                    ? parentObject.transform.GetChild(objectId) // 이미 존재하면 가져오기
                    : null;

                if (childTransform == null)
                {
                    GameObject childObject = Instantiate(prefab, parentObject.transform); // 프리팹으로 생성
                    childObject.name = objectId.ToString(); // 이름 설정
                    childTransform = childObject.transform;
                }

                // 자식 오브젝트의 Transform 값 업데이트
                LerpUpdate(in childTransform, in transformData);
            }
        }
    }



    public void LerpUpdate(in Transform childTransform, in ObjectTransform transformData)
    {
        // 최신화된 값으로 바로 대입
        childTransform.localPosition = new Vector3(transformData.PositionX, transformData.PositionY, transformData.PositionZ);
        //childTransform.localRotation = new Quaternion(transformData.RotationX, transformData.RotationY, transformData.RotationZ, transformData.RotationW);
        //childTransform.localScale = new Vector3(transformData.ScaleX, transformData.ScaleY, transformData.ScaleZ);
        
    }




}




