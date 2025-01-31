﻿using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
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
        private ConcurrentDictionary<int, Dictionary<int, ObjectTransform>> objectTransforms;
        //private Dictionary<Tuple<int, int>, ObjectTransform> objectTransforms2;

        // 생성자: ObjectTransform Dictionary 초기화
        public ObjectTransformManager()
        {
            objectTransforms = new ConcurrentDictionary<int, Dictionary<int, ObjectTransform>>();
        }

        // Dictionary를 JSON으로 직렬화하여 반환
        public string ToAllJson()
        {
            if (objectTransforms == null)
            {

                return string.Empty;
            }

            try
            {
                // objectTransforms를 직렬화하여 JSON 문자열로 변환
                string jsonString = JsonConvert.SerializeObject(objectTransforms, Formatting.Indented);

                // 직렬화된 JSON 출력

                //Console.WriteLine(jsonString);
            }
            catch (Exception ex)
            {
                // 직렬화 중 오류가 발생하면 예외 메시지 출력
                Console.WriteLine($"Error serializing objectTransforms: {ex.Message}");
            }

            return JsonConvert.SerializeObject(objectTransforms, Formatting.Indented);
        }

        private static readonly object lockObject = new object();
        public void UpdateObjectTransformsForPlayer(in dynamic message)
        {
            lock (lockObject)
            {
                // 메시지에서 PlayerId를 추출하여 id 초기화
                int id = message.PlayerId;

                // message 내의 Dictionary<int, ObjectTransform> 값을 추출하여 objectTransforms에 업데이트
                Dictionary<int, ObjectTransform> playerTransforms = JsonConvert.DeserializeObject<Dictionary<int, ObjectTransform>>(message.data.ToString());

                // 해당 playerId에 대한 데이터 갱신
                objectTransforms[id] = playerTransforms;
            }

        }

        public void DeleteUserData(int userId)
        {
            foreach (var outerKey in objectTransforms.Keys)
            {
                Console.WriteLine($"Outer Key: {outerKey}");

                // 내부 Dictionary에서의 모든 키 값 출력
                foreach (var innerKey in objectTransforms[outerKey].Keys)
                {
                    Console.WriteLine($"    Inner Key: {innerKey}");
                }
            }

            //

            // userId에 해당하는 데이터가 존재하는지 확인
            if (objectTransforms.ContainsKey(userId))
            {
                // 해당 userId의 데이터를 삭제
                objectTransforms.TryRemove(userId, out _);
                Console.WriteLine($"User {userId}의 데이터가 삭제되었습니다.");
            }
            else
            {
                Console.WriteLine($"User {userId}의 데이터가 존재하지 않습니다.");
            }
        }


    }
}
