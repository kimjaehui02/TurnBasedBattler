using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class TransformData : MonoBehaviour
{
    void Start()
    {
        // 100���� Ʈ������ �����͸� �����ϰ� JSON���� ��ȯ�ϴ� �Լ� ȣ��
        string json = GetTransformsJson();
        Debug.Log(json);
    }

    // 100���� Ʈ�������� JSON���� ��ȯ�ϴ� �Լ�
    public string GetTransformsJson()
    {
        // 100���� Ʈ������ �����͸� ������ ����Ʈ
        List<TransformDataStruct> transforms = new List<TransformDataStruct>();

        // ���÷� 100���� Ʈ������ �����͸� ä���
        for (int i = 0; i < 100; i++)
        {
            transforms.Add(new TransformDataStruct
            {
                position = new SerializableVector3(i * 1.0f, i * 2.0f, i * 3.0f),
                rotation = new SerializableQuaternion(i * 10.0f, i * 20.0f, i * 30.0f, 1.0f),
                scale = new SerializableVector3(1.0f, 1.0f, 1.0f)
            });
        }

        // JSON���� ��ȯ
        string json = JsonConvert.SerializeObject(transforms, Formatting.None);
        return json;
    }
}

// Ʈ������ �����͸� ���� ����ü
public struct TransformDataStruct
{
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public SerializableVector3 scale;
}

// SerializableVector3�� ����ȭ �����ϵ��� ����
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

    // Vector3�� SerializableVector3�� ��ȯ
    public static explicit operator SerializableVector3(Vector3 v)
    {
        return new SerializableVector3(v.x, v.y, v.z);
    }

    // SerializableVector3�� Vector3�� ��ȯ
    public static explicit operator Vector3(SerializableVector3 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }
}

// SerializableQuaternion�� ����ȭ �����ϵ��� ����
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

    // Quaternion�� SerializableQuaternion���� ��ȯ
    public static explicit operator SerializableQuaternion(Quaternion q)
    {
        return new SerializableQuaternion(q.x, q.y, q.z, q.w);
    }

    // SerializableQuaternion�� Quaternion���� ��ȯ
    public static explicit operator Quaternion(SerializableQuaternion q)
    {
        return new Quaternion(q.x, q.y, q.z, q.w);
    }
}
