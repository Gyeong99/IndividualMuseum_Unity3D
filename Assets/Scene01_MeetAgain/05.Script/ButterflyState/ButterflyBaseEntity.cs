using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ButterflyBaseEntity : MonoBehaviour
{
    
    
    //�� Ŭ������ ���� ��ӹ޴� ���͵��� ID�� 1�� �����Ͽ� ������ȣ�� ������.

    private int id;
    public int ID
    {
        set
        {
            id = value;
        }
        get => id;
    }
    private float targetDistance;

    public float TargetDistance
    {
        set
        {
            targetDistance = value;
        }
        get => targetDistance;
    }
    private string entityName;      //��ƼƼ �̸�

    //�Ļ� Ŭ�������� base.Setup()���� ȣ��.
    public virtual void SetUp(int butterflyCount)
    {
        //��ȣ ����
        ID = butterflyCount;
        //�̸� ����
        entityName = $"{ID:D2}_Butterfly_{ID}";
    }

    

    //Manager Ŭ�������� ��� ������Ʈ�� Updated()�� ȣ���� ������Ʈ�� �����Ѵ�.
    public abstract void FixedUpdated();

    public void PrintText(string text)
    {
        Debug.Log($"{entityName} : {text}");
    }
}
