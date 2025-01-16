using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            ObjectTransform objectTransform = new ()
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


