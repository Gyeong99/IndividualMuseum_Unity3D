using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.Barracuda;

namespace MeetAgain {
    public class ButterflyAgentChaseModel : Agent
    {

        // rBody��� Rigidbody �Լ� ����
        Rigidbody rBody;
        
        private bool isEndByBreak;
        private Transform Target;
        void Awake()
        {
            // rBody�Լ��� ���� GameObject�� Rigidbody Component�� ����
            Target = GameObject.FindWithTag("AITarget").transform;
            rBody = GetComponent<Rigidbody>();
            isEndByBreak = false;
        }

        
        
        public override void OnEpisodeBegin()
        {
            
            //Agent�� �ʹ� �ָ� �������� angularVelocity/velocity=0����, ��ġ�� �ʱ� ��ǥ�� ����
            if (isEndByBreak)
            {
                this.rBody.angularVelocity = Vector3.zero;
                this.rBody.velocity = Vector3.zero;
                isEndByBreak = !isEndByBreak;
            }
            // Target�� Random.value�Լ��� Ȱ���ؼ� ���ο� ������ ��ġ�� �̵�
            this.transform.localPosition = new Vector3(Target.localPosition.x + Random.value * 30 - 15,
                Random.value * 3 + 0.3f, 
                Target.localPosition.z + Random.value * 30 - 15);
            
            
        }

        public override void CollectObservations(VectorSensor sensor)       // ml ������Ʈ�� ȯ�� ���� �� ������ �ִ� �� (���� ���� �� ������ ���ְ� �ϴ� �Լ��̴�.)
        {
            // Target/Agent�� ��ġ ���� ����
            sensor.AddObservation(Vector3.Distance(this.transform.position, Target.position));
            sensor.AddObservation(transform.localPosition);

            // Agent�� velocity ���� ����
            sensor.AddObservation(rBody.velocity.x);
            sensor.AddObservation(rBody.velocity.y);
            sensor.AddObservation(rBody.velocity.z);
        }

        private float forceMultiplier = 5;
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
            if (distanceToTarget <= 1.42f)
            {
                SetReward(1.0f);                // �̰� ������ �������� , 1ȸ�� �������� ���簡 �ʿ�
                EndEpisode();
            }

            // �÷��� ������ ������ Episode ����

             if (distanceToTarget >= 50.0f || this.transform.position.y <= - 0.1f || this.transform.position.y >= 15.0f)
            {
                isEndByBreak = !isEndByBreak;
                SetReward(-0.01f);
                EndEpisode();
            }



        }

        
    }
}
