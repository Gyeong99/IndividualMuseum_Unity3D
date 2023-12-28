using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ButterflyBaseEntity : MonoBehaviour
{
    
    
    //이 클래스를 통해 상속받는 몬스터들은 ID가 1씩 증가하여 고유번호로 존재함.

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
    private string entityName;      //엔티티 이름

    //파생 클래스에서 base.Setup()으로 호출.
    public virtual void SetUp(int butterflyCount)
    {
        //번호 설정
        ID = butterflyCount;
        //이름 설정
        entityName = $"{ID:D2}_Butterfly_{ID}";
    }

    

    //Manager 클래스에서 모든 에이전트의 Updated()를 호출해 에이전트를 구동한다.
    public abstract void FixedUpdated();

    public void PrintText(string text)
    {
        Debug.Log($"{entityName} : {text}");
    }
}
