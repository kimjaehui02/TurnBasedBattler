
public struct ObjectTransform
{
    // ��ġ ����
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }

    public byte ObjectType { get; set; } // 0~255�� ������ ���� ����
    public byte ObjectDirection { get; set; } // 0~255�� ������ ���� ����

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



    // ����׿� ���
    public override readonly string ToString()
    {
        return $"Position: ({PositionX}, {PositionY}, {PositionZ}), " +
               $"ObjectType: {ObjectType}, " +
               $"ObjectDirection: {ObjectDirection}";
    }

    //public override string ToString()
    //{
    //    return $"Position: ({PositionX}, {PositionY}, {PositionZ}), ";
    //    //$"Rotation: ({RotationX}, {RotationY}, {RotationZ}, {RotationW}), ";
    //    //$"Scale: ({ScaleX}, {ScaleY}, {ScaleZ}), Updated: ";
    //}
}

