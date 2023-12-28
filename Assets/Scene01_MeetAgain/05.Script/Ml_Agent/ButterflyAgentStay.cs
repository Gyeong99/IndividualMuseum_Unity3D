using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

namespace MeetAgain
{
    public class ButterflyAgentStay : Agent
    {

        // rBody��� Rigidbody �Լ� ����
        Rigidbody rBody;
        Vector3 spawnPoint;
        private bool isEndByBreak;
        void Start()
        {
            // rBody�Լ��� ���� GameObject�� Rigidbody Component�� ����
            rBody = GetComponent<Rigidbody>();
            spawnPoint = Vector3.zero;
            isEndByBreak = false;
        }

        // Target�̶�� public Transform �Լ��� �����Ͽ� ���� Inspector �����쿡�� ���� 
        public Transform Target;
        public override void OnEpisodeBegin()
        {

            //Agent�� �ʹ� �ָ� �������� angularVelocity/velocity=0����, ��ġ�� �ʱ� ��ǥ�� ����
            if (isEndByBreak)
            {
                this.rBody.angularVelocity = Vector3.zero;
                this.rBody.velocity = Vector3.zero;
                this.transform.localPosition = new Vector3(Target.localPosition.x + Random.value * 10 - 5, Random.value * 1.5f + 2.2f, Target.localPosition.z + Random.value * 10 - 5);
                isEndByBreak = !isEndByBreak;
            }
            // Target�� Random.value�Լ��� Ȱ���ؼ� ���ο� ������ ��ġ�� �̵�
            
            this.spawnPoint = transform.position;
        }

        public override void CollectObservations(VectorSensor sensor)       // ml ������Ʈ�� ȯ�� ���� �� ������ �ִ� �� (���� ���� �� ������ ���ְ� �ϴ� �Լ��̴�.)
        {
            // Target/Agent�� ��ġ ���� ����
            sensor.AddObservation(Vector3.Distance(this.transform.position, Target.position));
            // Agent�� velocity ���� ����
            sensor.AddObservation(rBody.velocity.x);
            sensor.AddObservation(rBody.velocity.y);
            sensor.AddObservation(rBody.velocity.z);
        }

        public float forceMultiplier = 1;
        /// <summary>
        /// OnActionReceived�� FixedUpdate��ŭ �ʴ� 20������ ���� ȣ��ȴ�.
        /// ���� ������� ���� ���� ���¿����� ����
        /// ���� ���� ����ߴٸ� OnActionRececived�� FixedUpdateó�� ����� �����ϴ�
        /// ���� actionBuffer�� �ִ� ��(OnActionReveived�� ȣ����)���� ������ �ȴ�. �ڼ��Ѱ� �����ؾ���
        ///</summary>
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
            

            // �÷��� ������ ������ Episode ����

            if (distanceToTarget >= 6.0f || this.transform.position.y <= -0.1f || this.transform.position.y >= 8.0f)
            {
                isEndByBreak = !isEndByBreak;
                SetReward(-0.05f);
                EndEpisode();
            }
            else if (distanceToTarget >= 1.0f && distanceToTarget < 5.0f)
            {
                SetReward(1.0f);
                EndEpisode();
            }

               


        }


    }
}
