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
        /// // Vector Sensor component�� ���� ������ Throw�Ѵ�.
        /// ��������� State�� Int ���� 2���� ���� ������ �Ѿ��
        /// AddOneHotObsevations �Լ��� AddFloatObs�� ���� ���������� �ѱ��.
        /// �ش� ��ũ��Ʈ�� ���� FeatureVector�� 2���� ����(���°� 2��)�̸�,
        /// VectorSensor ������Ʈ�� Observation Size�� 2�̴�.
        /// �� Behavior Parameters�� VectorObservation-SpaceSize�� 0�̴�. (Agent���� �ѱ�°� �ƴϱ� ����)
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
            // Target/Agent�� ��ġ ���� ����
            sensor.AddObservation(Vector3.Distance(this.transform.position, Target.position));
            sensor.AddObservation(transform.localPosition);

            // Agent�� velocity ���� ����
            sensor.AddObservation(rBody.velocity.x);
            sensor.AddObservation(rBody.velocity.y);
            sensor.AddObservation(rBody.velocity.z);
        }

        public float forceMultiplier = 1;
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {

            // Agent�� Target������ �̵��ϱ� ���� X, Z�������� Force�� ����
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = actionBuffers.ContinuousActions[0];
            controlSignal.y = actionBuffers.ContinuousActions[1] * 0.1f;
            controlSignal.z = actionBuffers.ContinuousActions[2];
            rBody.AddForce(controlSignal * forceMultiplier);

            // Agent�� Target������ �Ÿ��� ����
            float distanceToTarget = Vector3.Distance(this.transform.position, Target.position);

            if (distanceToTarget >= 50.0f || this.transform.position.y <= -0.1f || this.transform.position.y >= 15.0f)
            {
                SetReward(-1.0f);
                EndEpisode();
            }

            // Target�� �����ϴ� ��� (�Ÿ��� 1.42���� ���� ���) Episode ����
            if (distanceToTarget >= 1.0f && distanceToTarget < 5.0f)
            {
                CurrentState = State.Stay;
                //SeparateReward(State.Stay);
            }
            else
            {
                CurrentState = State.Chase;
            }
           

            // �÷��� ������ ������ Episode ����

            
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


