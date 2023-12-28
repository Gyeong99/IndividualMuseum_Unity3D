using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Butterfly : ButterflyBaseEntity
{
    private Transform target;
    private Vector3 lastMove;
    public override void SetUp(int butterflyCount)
    {
        base.SetUp(butterflyCount);
        gameObject.name = $"{ID:D2}_Butterfly_{name}";
        target = GameObject.FindWithTag("AITarget").transform;
        TargetDistance = Vector3.Distance(transform.position, target.position);
        lastMove = transform.position;
    }
    public override void FixedUpdated()
    {
        RotateTowardsDirection();
        TargetDistance = Vector3.Distance(transform.position, target.position);
        lastMove = transform.position;
    }

    private void RotateTowardsDirection()
    {
        Quaternion targetRotation = Quaternion.LookRotation(transform.position - lastMove, Vector3.up);

        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10 * Time.deltaTime);
    }
}
