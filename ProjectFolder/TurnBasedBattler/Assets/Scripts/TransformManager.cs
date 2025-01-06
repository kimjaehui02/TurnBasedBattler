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
        HashSet<int> receivedClientIds = new HashSet<int>(keyValuePairs.Keys);

        // 1. 클라이언트가 종료되었을 때 오브젝트 제거
        List<GameObject> objectsToRemove = new List<GameObject>();

        // 기존 오브젝트들 순회하며 클라이언트 ID가 더 이상 없으면 제거
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



public struct ObjectTransform
{
    // 위치 정보
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }

    // 회전 정보 (Quaternion 형태)
    //public float RotationX { get; set; }
    //public float RotationY { get; set; }
    //public float RotationZ { get; set; }
    //public float RotationW { get; set; }

    // 스케일 정보
    //public float ScaleX { get; set; }
    //public float ScaleY { get; set; }
    //public float ScaleZ { get; set; }

    // 상태 업데이트 플래그
    //public bool IsUpdated { get; private set; }

    // 기본값을 설정하는 생성자
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

    // 업데이트 메서드
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

    // 업데이트 후 상태 리셋
    /*    public void ResetUpdatedFlag()
        {
            //IsUpdated = false;
        }*/

    // 디버그용 출력
    public override string ToString()
    {
        return $"Position: ({PositionX}, {PositionY}, {PositionZ}), ";
               //$"Rotation: ({RotationX}, {RotationY}, {RotationZ}, {RotationW}), ";
               //$"Scale: ({ScaleX}, {ScaleY}, {ScaleZ}), Updated: ";
    }
}



public class TransformConverter
{
    // Dictionary<int, GameObject>를 Dictionary<int, ObjectTransform>으로 변환하는 메서드
    public static Dictionary<int, ObjectTransform> ConvertGameObjectsToTransforms(Dictionary<int, GameObject> gameObjects)
    {
        Dictionary<int, ObjectTransform> objectTransforms = new Dictionary<int, ObjectTransform>();

        foreach (var item in gameObjects)
        {
            int key = item.Key;  // GameObject의 ID
            GameObject obj = item.Value;

            // GameObject의 트랜스폼 정보 얻기
            Transform objTransform = obj.transform;

            // ObjectTransform 객체 생성 및 값 할당
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

            // 딕셔너리에 추가
            objectTransforms[key] = objectTransform;
        }

        return objectTransforms;
    }
}

