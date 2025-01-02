using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UDPServer.Manager
{
    public class ObjectTransformManager
    {
        // 플레이어 ID별 ObjectTransform 리스트를 저장하는 Dictionary
        //private Dictionary<int, List<ObjectTransform>> objectTransforms;

        /// <summary>
        /// 첫 인트 - 클라이언트 식별id
        /// 두번째 인트 - 오브젝트 식별id
        /// </summary>
        private Dictionary<int, Dictionary<int, ObjectTransform>> objectTransforms;
        //private Dictionary<Tuple<int, int>, ObjectTransform> objectTransforms2;

        // 생성자: ObjectTransform Dictionary 초기화
        public ObjectTransformManager()
        {
            objectTransforms = new Dictionary<int, Dictionary<int, ObjectTransform>>();
        }

        // Dictionary를 JSON으로 직렬화하여 반환
        public string ToAllJson()
        {
            if (objectTransforms == null)
            {
                Console.WriteLine("objectTransforms is null");
                Console.WriteLine("objectTransforms is null");
                Console.WriteLine("objectTransforms is null");
                Console.WriteLine("objectTransforms is null");
                Console.WriteLine("objectTransforms is null");
                return string.Empty;
            }

            try
            {
                // objectTransforms를 직렬화하여 JSON 문자열로 변환
                string jsonString = JsonConvert.SerializeObject(objectTransforms, Formatting.Indented);

                // 직렬화된 JSON 출력
                Console.WriteLine("objectTransforms is asdf");
                Console.WriteLine("objectTransforms is asdf");
                Console.WriteLine("objectTransforms is asdf");
                Console.WriteLine("objectTransforms is asdf");
                Console.WriteLine(jsonString);
            }
            catch (Exception ex)
            {
                // 직렬화 중 오류가 발생하면 예외 메시지 출력
                Console.WriteLine($"Error serializing objectTransforms: {ex.Message}");
            }

            return JsonConvert.SerializeObject(objectTransforms, Formatting.Indented);
        }


        public void UpdateObjectTransformsForPlayer(dynamic message)
        {
            // 메시지에서 PlayerId를 추출하여 id 초기화
            int id = message.PlayerId;

            // message 내의 Dictionary<int, ObjectTransform> 값을 추출하여 objectTransforms에 업데이트
            Dictionary<int, ObjectTransform> playerTransforms = JsonConvert.DeserializeObject<Dictionary<int, ObjectTransform>>(message.data.ToString());

            // 해당 playerId에 대한 데이터 갱신
            objectTransforms[id] = playerTransforms;

            Console.WriteLine($"Player {id}의 objectTransforms가 업데이트되었습니다.");
            Console.WriteLine($"결과물입니다");
            Console.WriteLine($"결과물입니다");
            Console.WriteLine($"결과물입니다");
            Console.WriteLine($"결과물입니다");
            Console.WriteLine($"\n{ToAllJson()}\n");

            Console.WriteLine($"결과물입니다");
            Console.WriteLine($"결과물입니다");
            Console.WriteLine($"결과물입니다");
            Console.WriteLine($"결과물입니다");
        }



    }
}
