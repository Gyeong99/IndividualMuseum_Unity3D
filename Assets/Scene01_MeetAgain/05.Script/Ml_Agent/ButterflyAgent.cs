using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

namespace MeetAgain {
    public class ButterflyAgent : Agent
    {

        // rBody라는 Rigidbody 함수 정의
        Rigidbody rBody;
        Vector3 spawnPoint;
        private bool isEndByBreak;
        void Start()
        {
            // rBody함수는 현재 GameObject의 Rigidbody Component를 참조
            rBody = GetComponent<Rigidbody>();
            spawnPoint = Vector3.zero;
            isEndByBreak = false;
        }

        // Target이라는 public Transform 함수를 선언하여 차후 Inspector 윈도우에서 지정 
        public Transform Target;
        public override void OnEpisodeBegin()
        {
            
            //Agent가 너무 멀리 떨어지면 angularVelocity/velocity=0으로, 위치를 초기 좌표로 리셋
            if (isEndByBreak)
            {
                this.rBody.angularVelocity = Vector3.zero;
                this.rBody.velocity = Vector3.zero;
                this.transform.localPosition = new Vector3(Target.localPosition.x + Random.value * 30 - 15 , Random.value * 3 + 0.3f, Target.localPosition.z + Random.value * 30 - 15);   
                isEndByBreak = !isEndByBreak;
            }
            // Target을 Random.value함수를 활용해서 새로운 무작위 위치에 이동
            Target.localPosition = new Vector3(Random.value * 5, Random.value * 1 + 2.5f, Random.value * 8 - 3);
            this.spawnPoint = transform.position;
        }

        public override void CollectObservations(VectorSensor sensor)       // ml 에이전트의 환경 관찰 및 정보를 주는 것 (쉽게 말해 힘 조절을 해주게 하는 함수이다.)
        {
            // Target/Agent의 위치 정보 수집
            sensor.AddObservation(Target.localPosition);
            sensor.AddObservation(this.transform.localPosition);

            // Agent의 velocity 정보 수집
            sensor.AddObservation(rBody.velocity.x);
            sensor.AddObservation(rBody.velocity.y);
            sensor.AddObservation(rBody.velocity.z);
        }

        public float forceMultiplier = 1;
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            
            float distanceFromSpawnPoint = Vector3.Distance(spawnPoint, Target.position);
            // Agent가 Target쪽으로 이동하기 위해 X, Z축으로의 Force를 정의
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = actionBuffers.ContinuousActions[0];
            controlSignal.y = actionBuffers.ContinuousActions[1] * 0.1f;
            controlSignal.z = actionBuffers.ContinuousActions[2];
            rBody.AddForce(controlSignal * forceMultiplier);

            // Agent와 Target사이의 거리를 측정
            float distanceToTarget = Vector3.Distance(this.transform.position, Target.position);
            
            // Target에 도달하는 경우 (거리가 1.42보다 작은 경우) Episode 종료
            if (distanceToTarget <= 1.42)
            {
                SetReward(1.0f);                // 이게 완전한 보상인지 , 1회차 보상인지 조사가 필요
                EndEpisode();
            }

            // 플랫폼 밖으로 나가면 Episode 종료

             if (distanceToTarget >= 50.0f || this.transform.position.y <= - 0.1f || this.transform.position.y >= 15.0f)
            {
                isEndByBreak = !isEndByBreak;
                SetReward(-0.005f);
                EndEpisode();
            }



        }

        
    }
}
