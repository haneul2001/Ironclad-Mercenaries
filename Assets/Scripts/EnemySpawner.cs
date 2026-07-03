using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject enemyPrefab;      // 생성할 적 프리팹
    public int enemiesPerWave = 5;      // 웨이브당 적 수
    public float spawnInterval = 1f;    // 적 하나씩 나오는 간격(초)
    public float timeBetweenWaves = 3f; // 웨이브 사이 쉬는 시간(초)

    [Header("스폰 위치 범위")]
    public float spawnWidth = 4f;       // 좌우로 퍼지는 범위

    private int currentWave = 0;

    void Start()
    {
        // 웨이브 반복을 시작
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        // 무한히 웨이브를 진행
        while (true)
        {
            currentWave++;
            Debug.Log("웨이브 " + currentWave + " 시작!");

            // 이번 웨이브의 적들을 순서대로 생성
            for (int i = 0; i < enemiesPerWave; i++)
            {
                SpawnOneEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            // 다음 웨이브까지 대기
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    void SpawnOneEnemy()
    {
        // 좌우로 랜덤한 위치 계산 (X축으로 퍼지게)
        float randomX = Random.Range(-spawnWidth, spawnWidth);
        Vector3 spawnPos = new Vector3(transform.position.x + randomX,
                                       transform.position.y,
                                       transform.position.z);

        // 적 생성
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}