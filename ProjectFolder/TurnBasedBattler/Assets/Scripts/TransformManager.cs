using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor.VersionControl;


public class TransformManager : MonoBehaviour
{
    public GameObject prefab;

    // 오브젝트 변환을 최신화하는 함수
    public void UpdateObjectTransforms(Dictionary<int, Dictionary<int, ObjectTransform>> objectTransforms)
    {
        // 게임 오브젝트 딕셔너리
        var gameObjectTransforms = new Dictionary<int, Dictionary<int, GameObject>>();

        // 현재 플레이어의 ID 가져오기
        int currentPlayerId = GameManager.Instance.GetPlayerId();

        // 외부 딕셔너리 순회
        foreach (var outerPair in objectTransforms)
        {
            int playerId = outerPair.Key; // 플레이어 ID

            // 현재 플레이어의 ID와 비교하여 처리
            if (playerId == currentPlayerId)
                continue;  // 현재 플레이어의 오브젝트는 처리하지 않음

            var innerTransforms = outerPair.Value; // 해당 플레이어의 ObjectTransform 딕셔너리

            if (!gameObjectTransforms.ContainsKey(playerId))
            {
                gameObjectTransforms[playerId] = new Dictionary<int, GameObject>();
            }

            // 해당 플레이어의 모든 ObjectTransform에 대해 처리
            foreach (var innerPair in innerTransforms)
            {
                int objectId = innerPair.Key; // 오브젝트 ID
                ObjectTransform transformData = innerPair.Value;

                // 해당 오브젝트가 존재하는지 확인
                if (gameObjectTransforms[playerId].ContainsKey(objectId))
                {
                    // 이미 오브젝트가 있으면 트랜스폼만 업데이트
                    UpdateObjectTransform(gameObjectTransforms[playerId][objectId], transformData);
                }
                else
                {
                    // 오브젝트가 없으면 새로 생성
                    CreateNewObject(playerId, objectId, transformData, gameObjectTransforms);
                }
            }
        }

        // 삭제된 오브젝트를 처리
        RemoveDeletedObjects(objectTransforms);
    }

    // 게임 오브젝트의 Transform을 최신화하는 함수
    private void UpdateObjectTransform(GameObject obj, ObjectTransform transformData)
    {
        if (obj != null)
        {
            obj.transform.position = new Vector3(transformData.PositionX, transformData.PositionY, transformData.PositionZ);
            obj.transform.rotation = new Quaternion(transformData.RotationX, transformData.RotationY, transformData.RotationZ, transformData.RotationW);
            obj.transform.localScale = new Vector3(transformData.ScaleX, transformData.ScaleY, transformData.ScaleZ);
        }
    }

    // 새로운 오브젝트를 생성하는 함수 (필요 시)
    private void CreateNewObject(int playerId, int objectId, ObjectTransform transformData, Dictionary<int, Dictionary<int, GameObject>> gameObjectTransforms)
    {
        // 예시로 프리팹을 사용하여 오브젝트를 생성하는 방법
        GameObject newObject = Instantiate(prefab);

        if (newObject != null)
        {
            // 새 오브젝트를 딕셔너리에 추가
            gameObjectTransforms[playerId][objectId] = newObject;
            UpdateObjectTransform(newObject, transformData); // Transform 최신화
        }
        else
        {
            Debug.LogError("Prefab 로딩 실패!");
        }
    }

    // 삭제된 오브젝트를 찾아서 제거하는 함수
    private void RemoveDeletedObjects(Dictionary<int, Dictionary<int, ObjectTransform>> objectTransforms)
    {
        // 게임 오브젝트 딕셔너리에서 현재 존재하는 오브젝트들의 목록을 가져옴
        foreach (var playerPair in GameManager.Instance.gameObjects2)
        {
            int playerId = playerPair.Key;
            var existingObjects = playerPair.Value;

            // 수신된 데이터에서 해당 플레이어 ID가 없으면 삭제
            if (!objectTransforms.ContainsKey(playerId))
            {
                DeleteAllObjects(playerId);
            }
            else
            {
                // 해당 플레이어의 모든 오브젝트를 비교하여 삭제
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

    // 플레이어의 모든 오브젝트를 삭제하는 함수
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

    // 오브젝트를 삭제하는 함수
    private void DeleteObject(int playerId, int objectId)
    {
        if (GameManager.Instance.gameObjects2.ContainsKey(playerId) && GameManager.Instance.gameObjects2[playerId].ContainsKey(objectId))
        {
            GameObject objToRemove = GameManager.Instance.gameObjects2[playerId][objectId];
            if (objToRemove != null)
            {
                Destroy(objToRemove); // 오브젝트 삭제
                GameManager.Instance.gameObjects2[playerId].Remove(objectId); // 딕셔너리에서 제거
                Debug.Log($"Player {playerId}, Object {objectId} 삭제");
            }
        }
        else
        {
            Debug.LogWarning($"Player {playerId}, Object {objectId}가 이미 삭제됨");
        }
    }

    // 외부에서 데이터가 들어올 때마다 호출하는 방법을 권장 (네트워크 데이터 수신 시 호출)
    public void OnObjectTransformsReceived(Dictionary<int, Dictionary<int, ObjectTransform>> receivedTransforms)
    {
        UpdateObjectTransforms(receivedTransforms);
    }
}



public class ObjectTransform
{
    // 위치 정보
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }

    // 회전 정보 (Quaternion 형태)
    public float RotationX { get; set; }
    public float RotationY { get; set; }
    public float RotationZ { get; set; }
    public float RotationW { get; set; }

    // 스케일 정보
    public float ScaleX { get; set; }
    public float ScaleY { get; set; }
    public float ScaleZ { get; set; }

    // 상태 업데이트 플래그
    //public bool IsUpdated { get; private set; }

    // 기본값을 설정하는 생성자
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

    // 업데이트 후 상태 리셋
    /*    public void ResetUpdatedFlag()
        {
            //IsUpdated = false;
        }*/

    // 디버그용 출력
    public override string ToString()
    {
        return $"Position: ({PositionX}, {PositionY}, {PositionZ}), " +
               $"Rotation: ({RotationX}, {RotationY}, {RotationZ}, {RotationW}), " +
               $"Scale: ({ScaleX}, {ScaleY}, {ScaleZ}), Updated: ";
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
                RotationX = objTransform.rotation.x,
                RotationY = objTransform.rotation.y,
                RotationZ = objTransform.rotation.z,
                RotationW = objTransform.rotation.w,
                ScaleX = objTransform.localScale.x,
                ScaleY = objTransform.localScale.y,
                ScaleZ = objTransform.localScale.z
            };

            // 딕셔너리에 추가
            objectTransforms[key] = objectTransform;
        }

        return objectTransforms;
    }
}

