using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

namespace MeetAgain
{
    public class ButterflyAgentDefault : Agent
    {
        Rigidbody rBody;
        private bool isEndByBreak;
        void Start()
        {
            // rBody�Լ��� ���� GameObject�� Rigidbody Component�� ����
            rBody = GetComponent<Rigidbody>();
            isEndByBreak = false;
        }

        // Target�̶�� public Transform �Լ��� �����Ͽ� ���� Inspector �����쿡�� ���� 
        private Transform Target;
        public override void OnEpisodeBegin()
        {
            Target = GameObject.FindWithTag("AITarget").transform;
            //Agent�� �ʹ� �ָ� �������� angularVelocity/velocity=0����, ��ġ�� �ʱ� ��ǥ�� ����
            if (isEndByBreak)
            {
                this.rBody.angularVelocity = Vector3.zero;
                this.rBody.velocity = Vector3.zero;
                this.transform.localPosition = new Vector3(Target.localPosition.x + Random.value * 30 - 15,
                    Random.value * 3 + 0.3f,
                    Target.localPosition.z + Random.value * 30 - 15);
                isEndByBreak = !isEndByBreak;
            }

        }

        public override void CollectObservations(VectorSensor sensor)       // ml ������Ʈ�� ȯ�� ���� �� ������ �ִ� �� (���� ���� �� ������ ���ְ� �ϴ� �Լ��̴�.)
        {
            // Target/Agent�� ��ġ ���� ����
            sensor.AddObservation(Target.localPosition);
            sensor.AddObservation(this.transform.localPosition);
            sensor.AddObservation(Vector3.Distance(this.transform.position , Target.position));

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

            // Target�� �����ϴ� ��� (�Ÿ��� 1.42���� ���� ���) Episode ����
            if (distanceToTarget <= 1.42)
            {
                SetReward(1.0f);                // �̰� ������ �������� , 1ȸ�� �������� ���簡 �ʿ�
                Debug.Log("Episode End");
                EndEpisode();
            }

            // �÷��� ������ ������ Episode ����

            if (distanceToTarget >= 50.0f || this.transform.position.y <= -0.1f || this.transform.position.y >= 15.0f)
            {
                isEndByBreak = !isEndByBreak;
                SetReward(-0.005f);
                EndEpisode();
            }



        }


    }
}