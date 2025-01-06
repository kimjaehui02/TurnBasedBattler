using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace SimulationClass
{
    // 기존 ObjectTransform 클래스 정의
    public class ObjectTransform
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }

/*        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }
        public float RotationW { get; set; }

        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float ScaleZ { get; set; }*/
    }

    // 최적화된 구조체 CompactTransform 정의 (예시)
    public class CompactTransform
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }

/*        public int RotationX { get; set; }
        public int RotationY { get; set; }
        public int RotationZ { get; set; }
        public int RotationW { get; set; }

        public int ScaleX { get; set; }
        public int ScaleY { get; set; }
        public int ScaleZ { get; set; }*/
    }

    class Issue1
    {
        public void TestMain()
        {
            Stopwatch stopwatch = new Stopwatch();

            // 기존 구조체 JSON 직렬화 시간
            //var originalTransform = new ObjectTransform { PositionX = 1.23f, PositionY = 2.34f, PositionZ = 3.45f, RotationX = 0f, RotationY = 0f, RotationZ = 0f, RotationW = 1f, ScaleX = 1f, ScaleY = 1f, ScaleZ = 1f };
            var originalTransform = new ObjectTransform { PositionX = 1.23f, PositionY = 2.34f, PositionZ = 3.45f};
            stopwatch.Start();
            string originalJson = JsonConvert.SerializeObject(originalTransform);
            stopwatch.Stop();
            Console.WriteLine($"Original JSON Serialization: {stopwatch.ElapsedMilliseconds}ms");

            // 기존 구조체 JSON 역직렬화 시간
            stopwatch.Restart();
            var deserializedOriginalTransform = JsonConvert.DeserializeObject<ObjectTransform>(originalJson);
            stopwatch.Stop();
            Console.WriteLine($"Original JSON Deserialization: {stopwatch.ElapsedMilliseconds}ms");

            // 최적화된 구조체 JSON 직렬화 시간
            //var compactTransform = new CompactTransform { PositionX = 123, PositionY = 234, PositionZ = 345, RotationX = 1, RotationY = 1, RotationZ = 1, RotationW = 1, ScaleX = 2, ScaleY = 2, ScaleZ = 2 };
            var compactTransform = new CompactTransform { PositionX = 123, PositionY = 234, PositionZ = 345};
            stopwatch.Restart();
            string compactJson = JsonConvert.SerializeObject(compactTransform);
            stopwatch.Stop();
            Console.WriteLine($"Compact JSON Serialization: {stopwatch.ElapsedMilliseconds}ms");

            // 최적화된 구조체 JSON 역직렬화 시간
            stopwatch.Restart();
            var deserializedCompactTransform = JsonConvert.DeserializeObject<CompactTransform>(compactJson);
            stopwatch.Stop();
            Console.WriteLine($"Compact JSON Deserialization: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
