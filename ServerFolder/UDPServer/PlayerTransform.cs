using System;

namespace UDPServer
{
    struct PlayerTransform
    {
        // 좌표 정보 포함
        public float PositionX { get; set; }
        public float PositionY { get; set; }

        // 생성자
        public PlayerTransform(float positionX, float positionY)
        {
            PositionX = positionX;
            PositionY = positionY;
        }

        // 좌표를 업데이트하는 메서드
        public void UpdatePosition(float newX, float newY)
        {
            PositionX = newX;
            PositionY = newY;
        }

        public override string ToString()
        {
            return $"Position: ({PositionX}, {PositionY})";
        }
    }
}
