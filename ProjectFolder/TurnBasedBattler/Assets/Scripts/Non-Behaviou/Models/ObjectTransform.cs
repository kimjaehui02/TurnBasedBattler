
public struct ObjectTransform
{
    // 위치 정보
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }

    public byte ObjectType { get; set; } // 0~255의 정수만 저장 가능
    public byte ObjectDirection { get; set; } // 0~255의 정수만 저장 가능

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



    // 디버그용 출력
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

