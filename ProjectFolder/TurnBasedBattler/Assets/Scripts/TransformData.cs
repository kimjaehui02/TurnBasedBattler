using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class TransformData : MonoBehaviour
{
    void Start()
    {
        // 100개의 트랜스폼 데이터를 생성하고 JSON으로 반환하는 함수 호출
        string json = GetTransformsJson();
        Debug.Log(json);
    }

    // 100개의 트랜스폼을 JSON으로 반환하는 함수
    public string GetTransformsJson()
    {
        // 100개의 트랜스폼 데이터를 저장할 리스트
        List<TransformDataStruct> transforms = new List<TransformDataStruct>();

        // 예시로 100개의 트랜스폼 데이터를 채운다
        for (int i = 0; i < 100; i++)
        {
            transforms.Add(new TransformDataStruct
            {
                position = new SerializableVector3(i * 1.0f, i * 2.0f, i * 3.0f),
                rotation = new SerializableQuaternion(i * 10.0f, i * 20.0f, i * 30.0f, 1.0f),
                scale = new SerializableVector3(1.0f, 1.0f, 1.0f)
            });
        }

        // JSON으로 변환
        string json = JsonConvert.SerializeObject(transforms, Formatting.None);
        return json;
    }
}

// 트랜스폼 데이터를 담을 구조체
public struct TransformDataStruct
{
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public SerializableVector3 scale;
}

// SerializableVector3를 직렬화 가능하도록 정의
[System.Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public SerializableVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // Vector3를 SerializableVector3로 변환
    public static explicit operator SerializableVector3(Vector3 v)
    {
        return new SerializableVector3(v.x, v.y, v.z);
    }

    // SerializableVector3를 Vector3로 변환
    public static explicit operator Vector3(SerializableVector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }
}

// SerializableQuaternion을 직렬화 가능하도록 정의
[System.Serializable]
public struct SerializableQuaternion
{
    public float x;
    public float y;
    public float z;
    public float w;

    public SerializableQuaternion(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    // Quaternion을 SerializableQuaternion으로 변환
    public static explicit operator SerializableQuaternion(Quaternion q)
    {
        return new SerializableQuaternion(q.x, q.y, q.z, q.w);
    }

    // SerializableQuaternion을 Quaternion으로 변환
    public static explicit operator Quaternion(SerializableQuaternion q)
    {
        return new Quaternion(q.x, q.y, q.z, q.w);
    }
}
