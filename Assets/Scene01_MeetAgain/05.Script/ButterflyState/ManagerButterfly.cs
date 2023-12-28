using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.UIElements;
using System.Runtime.CompilerServices;

public class ManagerButterfly : MonoBehaviour
{

    [SerializeField]
    private GameObject chaseButterflyPrefab;
    [SerializeField]
    private GameObject stayButterflyPrefab;
    [SerializeField]
    private Transform destination;
    [SerializeField]
    private Transform spawnArea;
    [SerializeField]
    private float butterflySpawnDistance;
    [SerializeField]
    private int maxButterflyCount;

    private List<ButterflyBaseEntity> chaseButterflyentitys;
    private List<ButterflyBaseEntity> stayButterflyentitys;
    private int butterflyCount = 0;
    private Vector3 butterflySpawnPoints;
    private IEnumerator butterflySpawnTimer;
    
    private void Awake()
    {
        chaseButterflyentitys = new List<ButterflyBaseEntity>();
        stayButterflyentitys = new List<ButterflyBaseEntity>();
        butterflySpawnPoints = Vector3.zero;
        butterflySpawnTimer = ButterflySpawnTimer();
        StartCoroutine(butterflySpawnTimer);
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < chaseButterflyentitys.Count; i++)
        {
            chaseButterflyentitys[i].FixedUpdated();
            if (chaseButterflyentitys[i].TargetDistance <= 5.5f)
            {
                AddStayButterflyList(chaseButterflyentitys[i].transform.position , chaseButterflyentitys[i].ID);
                Destroy(chaseButterflyentitys[i].gameObject);
                chaseButterflyentitys.RemoveAt(i);
            }
        }
        for (int i = 0; i < stayButterflyentitys.Count; i++)
        {
            stayButterflyentitys[i].FixedUpdated();
            if (stayButterflyentitys.Count == maxButterflyCount)
            {
                ResetStayButterfly(stayButterflyentitys);
                StartCoroutine(butterflySpawnTimer);
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
        GameObject clone = Instantiate(chaseButterflyPrefab);
        Butterfly butterflyEntity = clone.GetComponent<Butterfly>();
        butterflyEntity.SetUp(butterflyCount);
        chaseButterflyentitys.Add(butterflyEntity);
        clone.transform.position = butterflySpawnPoints;
        clone.transform.SetParent(spawnArea);
    }

    private void AddStayButterflyList(Vector3 spawnPoint , int entityCount)
    {
        GameObject clone = Instantiate(stayButterflyPrefab);
        Butterfly butterflyEntity = clone.GetComponent<Butterfly>();
        butterflyEntity.SetUp(entityCount);
        stayButterflyentitys.Add(butterflyEntity);
        clone.transform.position = spawnPoint;
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
