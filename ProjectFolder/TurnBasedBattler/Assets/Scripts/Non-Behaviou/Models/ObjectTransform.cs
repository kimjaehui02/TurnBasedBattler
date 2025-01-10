
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

