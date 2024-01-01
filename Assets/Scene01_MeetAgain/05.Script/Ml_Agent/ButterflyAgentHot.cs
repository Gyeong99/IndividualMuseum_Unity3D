using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;
using Random = UnityEngine.Random;
using Unity.VisualScripting;

namespace MeetAgain {

    public class ButterflyAgentHot : Agent
    {
        Rigidbody rBody;
        public Transform Target;
        public enum State {
            Chase,
            Stay
        }

        public GameObject ChaseBottom;
        public GameObject StayBottom;

        State m_CurrentState;

        VectorSensorComponent m_GoalSensor;
        public State CurrentState
        {
            get { return m_CurrentState; }
            set
            {
                switch (value)
                {
                    case State.Chase:
                        ChaseBottom.SetActive(true);
                        StayBottom.SetActive(false);
                        break;
                    case State.Stay:
                        ChaseBottom.SetActive(false);
                        StayBottom.SetActive(true);
                        break;
                }
                m_CurrentState = value;
            }
        }
        public override void Initialize()
        { 
            rBody = GetComponent<Rigidbody>();
            m_GoalSensor = this.GetComponent<VectorSensorComponent>();
        }
        public override void OnEpisodeBegin()
        {
            Array values = Enum.GetValues(typeof(State));
            if (m_GoalSensor is object)
            {
                CurrentState = (State)values.GetValue(Random.Range(0, values.Length));
            }
            else
            {
                CurrentState = State.Chase;
            }
            SetReward(0.0f);
            EpisodeInit();
        }

        /// <summary>
        /// // Vector Sensor component로 관찰 정보를 Throw한다.
        /// 결과적으로 State의 Int 인자 2개가 관찰 정보로 넘어가며
        /// AddOneHotObsevations 함수의 AddFloatObs로 인해 관찰정보를 넘긴다.
        /// 해당 스크립트가 가진 FeatureVector는 2차원 벡터(상태가 2개)이며,
        /// VectorSensor 컴포넌트의 Observation Size는 2이다.
        /// 단 Behavior Parameters의 VectorObservation-SpaceSize는 0이다. (Agent에서 넘기는게 아니기 떄문)
        /// </summary>
        public override void CollectObservations(VectorSensor sensor)       
        {
            Array values = Enum.GetValues(typeof(State));

            if (m_GoalSensor is object)
            {
                int goalNum = (int)CurrentState;
                m_GoalSensor.GetSensor().AddOneHotObservation(goalNum, values.Length);
            }
            // Target/Agent의 위치 정보 수집
            sensor.AddObservation(Vector3.Distance(this.transform.position, Target.position));
            sensor.AddObservation(this.transform.localPosition);
            // Agent의 velocity 정보 수집
            sensor.AddObservation(rBody.velocity.x);
            sensor.AddObservation(rBody.velocity.y);
            sensor.AddObservation(rBody.velocity.z);
        }

        public float forceMultiplier = 1;
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = actionBuffers.ContinuousActions[0];
            controlSignal.y = actionBuffers.ContinuousActions[1] * 0.1f;
            controlSignal.z = actionBuffers.ContinuousActions[2];
            rBody.AddForce(controlSignal * forceMultiplier);
            AddReward(-0.005f);
            float distanceToTarget = Vector3.Distance(this.transform.position, Target.position);
            if (distanceToTarget > 30.0f || this.transform.position.y <= -0.1f || this.transform.position.y >= 8.0f)
            {
                SetReward(-1.0f);
                EpisodeReset();
            }

            if (distanceToTarget < 1.6f)
            {
                if (CurrentState == State.Chase)
                {
                    SetReward(1.0f);
                    // SetReward 어떻게 되는지 확인 후 초기화 수요 파악.
                    EndEpisode();
                }
                else
                {
                    SetReward(-1.0f);
                    // SetReward 어떻게 되는지 확인 후 초기화 수요 파악.
                    EndEpisode();
                }
            }
            else if (distanceToTarget < 5.0f && CurrentState == State.Stay)
            {
                AddReward(0.015f);
                if (GetCumulativeReward() > 1.0f)
                {
                    EndEpisode();
                }
            }           
            else
            {
                SeparateDecreaseReward(distanceToTarget);
            }
            
        }

       
        private void EpisodeReset()
        {
            EpisodeInit();
        }
        private void EpisodeInit()
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(Target.localPosition.x + Random.value * 30 - 15, Random.value * 3 + 0.3f, Target.localPosition.z + Random.value * 30 - 15);
            this.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        /*private void ChaseEpisodeInit()
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            // Target을 Random.value함수를 활용해서 새로운 무작위 위치에 이동
            this.transform.localPosition = new Vector3(Target.localPosition.x + Random.value * 30 - 15, Random.value * 3 + 0.3f, Target.localPosition.z + Random.value * 30 - 15);
            Target.localPosition = new Vector3(Random.value * 5, Random.value * 1 + 2.5f, Random.value * 8 - 3);
        }

        private void StayEpisodeInit()
        {
            Target.localPosition = targetOriginPoint;
            this.rBody.velocity = Vector3.zero;
            this.rBody.angularVelocity = Vector3.zero;
            if (staySpawnPoint == Vector3.zero)
            {
                this.transform.localPosition = new Vector3(Target.localPosition.x + Random.value * 5f - 3.5f + 1f, Random.value * 1.5f + 2.2f, Target.localPosition.z + Random.value * 5f - 3.5f + 1f);
                staySpawnPoint = this.transform.localPosition;
            }
            else
            {
                this.transform.localPosition = staySpawnPoint;
            }
            //Agent가 너무 멀리 떨어지면 angularVelocity/velocity=0으로, 위치를 초기 좌표로 리셋
            if (isEndByBreak)
            {
                this.transform.localPosition = staySpawnPoint;
                isEndByBreak = !isEndByBreak;
            }
        }
        */

        private void SeparateDecreaseReward(float distanceToTarget)
        {
           if (CurrentState == State.Chase)
           {
                if (distanceToTarget > 1.6f)
                {
                    AddReward(-0.001f * distanceToTarget);
                }
           }
            else
            {
                if (distanceToTarget > 5.0f)
                {
                    AddReward(-0.01f * distanceToTarget);
                }
            }
        }

    }
}


