using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

            // ��ųʸ��� �߰�
            objectTransforms[key] = objectTransform;
        }

        return objectTransforms;
    }
}


