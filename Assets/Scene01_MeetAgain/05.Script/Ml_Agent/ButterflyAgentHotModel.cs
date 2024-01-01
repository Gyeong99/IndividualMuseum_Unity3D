using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;
using Random = UnityEngine.Random;

namespace MeetAgain {

    public class ButterflyAgentHotModel : Agent
    {
        Rigidbody rBody;
        Vector3 targetOriginPoint;
        private bool isEndByBreak;
        public Transform Target;
        public bool stateChange;
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
            targetOriginPoint = Target.transform.position;
            m_GoalSensor = this.GetComponent<VectorSensorComponent>();
            CurrentState = State.Chase;
        }
        
        public override void OnEpisodeBegin()
        {
            /* Array values = Enum.GetValues(typeof(State));
       
             if (m_GoalSensor is object)
            {
                CurrentState = (State)values.GetValue(Random.Range(0, values.Length));
            }
            else
            {
                CurrentState = State.Chase;
            }
            if (CurrentState == State.Chase)
            {
                ChaseEpisodeInit();
            }
        
            else
            {
                StayEpisodeInit();
            }
        
             */
            //this.rBody.angularVelocity = Vector3.zero;
            //this.rBody.velocity = Vector3.zero;
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
                Debug.Log(values.Length);
            }
            // Target/Agent의 위치 정보 수집
            sensor.AddObservation(Vector3.Distance(this.transform.position, Target.position));
            sensor.AddObservation(transform.localPosition);

            // Agent의 velocity 정보 수집
            sensor.AddObservation(rBody.velocity.x);
            sensor.AddObservation(rBody.velocity.y);
            sensor.AddObservation(rBody.velocity.z);
        }

        public float forceMultiplier = 1;
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {

            // Agent가 Target쪽으로 이동하기 위해 X, Z축으로의 Force를 정의
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = actionBuffers.ContinuousActions[0];
            controlSignal.y = actionBuffers.ContinuousActions[1] * 0.1f;
            controlSignal.z = actionBuffers.ContinuousActions[2];
            rBody.AddForce(controlSignal * forceMultiplier);

            // Agent와 Target사이의 거리를 측정
            float distanceToTarget = Vector3.Distance(this.transform.position, Target.position);

            if (distanceToTarget >= 50.0f || this.transform.position.y <= -0.1f || this.transform.position.y >= 15.0f)
            {
                SetReward(-1.0f);
                EndEpisode();
            }

            // Target에 도달하는 경우 (거리가 1.42보다 작은 경우) Episode 종료
            if (distanceToTarget >= 1.0f && distanceToTarget < 5.0f)
            {
                CurrentState = State.Stay;
                //SeparateReward(State.Stay);
            }
            else
            {
                CurrentState = State.Chase;
            }
           

            // 플랫폼 밖으로 나가면 Episode 종료

            
        }

        private void SeparateReward(State state)
        {
            if (CurrentState == state)
            {
                SetReward(1f);
            }
            else
            {
                SetReward(-0.05f);
            }
        }

        private void ChaseEpisodeInit()
        {
            Target.localPosition = new Vector3(Random.value * 5, Random.value * 1 + 2.5f, Random.value * 8 - 3);
            this.transform.localPosition = new Vector3(Target.localPosition.x + Random.value * 30 - 15, Random.value * 3 + 0.3f, Target.localPosition.z + Random.value * 30 - 15);
        }

        private void StayEpisodeInit()
        {
            Target.localPosition = targetOriginPoint;
            this.transform.localPosition = new Vector3(Target.localPosition.x + Random.value * 10 - 5, Random.value * 1.5f + 2.2f, Target.localPosition.z + Random.value * 10 - 5);
        }

    }
}


