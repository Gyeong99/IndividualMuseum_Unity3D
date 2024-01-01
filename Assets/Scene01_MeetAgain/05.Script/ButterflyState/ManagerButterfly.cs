using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.UIElements;
using System.Runtime.CompilerServices;

public class ManagerButterfly : MonoBehaviour
{

    [SerializeField]
    private GameObject butterflyPrefab;
    [SerializeField]
    private Transform destination;
    [SerializeField]
    private Transform spawnArea;
    [SerializeField]
    private float butterflySpawnDistance;
    [SerializeField]
    private int maxButterflyCount;

    private List<ButterflyBaseEntity> butterflyentitys;
    private int butterflyCount = 0;
    private Vector3 butterflySpawnPoints;
    private IEnumerator butterflySpawnTimer;
    
    private void Awake()
    {
        butterflyentitys = new List<ButterflyBaseEntity>();
        butterflySpawnPoints = Vector3.zero;
        butterflySpawnTimer = ButterflySpawnTimer();
        StartCoroutine(butterflySpawnTimer);
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < butterflyentitys.Count; i++)
        {
            butterflyentitys[i].FixedUpdated();
            if (butterflyentitys[i].TargetDistance <= 5.5f)
            {
                
                Destroy(butterflyentitys[i].gameObject);
                butterflyentitys.RemoveAt(i);
            }
        }
        
        if (butterflyCount >= maxButterflyCount)
        {
            StopCoroutine(butterflySpawnTimer);
        }
    }
    

    private void ResetStayButterfly(List<ButterflyBaseEntity> stayButterflyentitys)
    {
        for (int i = 0; i < stayButterflyentitys.Count; i++)
        {
            Destroy(stayButterflyentitys[i].gameObject);
        }
        stayButterflyentitys.Clear();
        butterflyCount = 0;
    }

    private void AddChaseButterflyList()
    {
        butterflySpawnPoints = GetRandomVector();
        GameObject clone = Instantiate(butterflyPrefab);
        Butterfly butterflyEntity = clone.GetComponent<Butterfly>();
        butterflyEntity.SetUp(butterflyCount);
        butterflyentitys.Add(butterflyEntity);
        clone.transform.position = butterflySpawnPoints;
        clone.transform.SetParent(spawnArea);
    }

    private Vector3 GetRandomVector() {
        float randomX = Random.value * 2 - 1.0f;
        float randomZ = Random.value * 2 - 1.0f;

        Vector3 randomVector = new Vector3(randomX, 0.0f, randomZ).normalized;
        randomVector = destination.transform.position + (randomVector * butterflySpawnDistance);
        randomVector.y = Random.value * 7 + 0.1f;
        return randomVector;
    }

    IEnumerator ButterflySpawnTimer()
    {
        while (true)
        {
            AddChaseButterflyList();
            butterflyCount++;
            yield return new WaitForSeconds(2f);
        }
    }
}
