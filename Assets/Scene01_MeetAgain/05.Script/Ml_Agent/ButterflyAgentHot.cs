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
            }
            // Target/Agent�� ��ġ ���� ����
            sensor.AddObservation(Vector3.Distance(this.transform.position, Target.position));
            sensor.AddObservation(this.transform.localPosition);
            // Agent�� velocity ���� ����
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
                    // SetReward ��� �Ǵ��� Ȯ�� �� �ʱ�ȭ ���� �ľ�.
                    EndEpisode();
                }
                else
                {
                    SetReward(-1.0f);
                    // SetReward ��� �Ǵ��� Ȯ�� �� �ʱ�ȭ ���� �ľ�.
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
            // Target�� Random.value�Լ��� Ȱ���ؼ� ���ο� ������ ��ġ�� �̵�
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
            //Agent�� �ʹ� �ָ� �������� angularVelocity/velocity=0����, ��ġ�� �ʱ� ��ǥ�� ����
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


