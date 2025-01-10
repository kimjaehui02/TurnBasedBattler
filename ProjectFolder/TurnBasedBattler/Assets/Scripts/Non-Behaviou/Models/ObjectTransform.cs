
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

