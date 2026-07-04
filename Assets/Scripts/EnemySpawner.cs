using UnityEngine;
using System.Collections;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text waveText;

    [Header("스폰 설정")]
    public GameObject enemyPrefab;
    public int enemiesPerWave = 5;      // 한 웨이브당 스폰할 적 수

    [Header("웨이브 타이밍")]
    public float waveDuration = 10f;    // 한 웨이브 총 시간
    public float spawnDuration = 6f;    // 이 시간 안에 몬스터 다 스폰

    [Header("스폰 위치 범위")]
    public float spawnWidth = 4f;

    private int currentWave = 0;
    private bool allWavesSpawned = false;

    void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        int totalWaves = GameManager.Instance.targetWave;

        while (currentWave < totalWaves)
        {
            currentWave++;
            if (waveText != null)
                waveText.text = "웨이브 " + currentWave + " / " + totalWaves;

            Debug.Log("웨이브 " + currentWave + " / " + totalWaves);

            // 스폰 간격 = 스폰 시간 / 몬스터 수 (6초 안에 균등 분배)
            float spawnInterval = (enemiesPerWave > 0) ? spawnDuration / enemiesPerWave : 0f;

            for (int i = 0; i < enemiesPerWave; i++)
            {
                SpawnOneEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            // 마지막 웨이브가 아니면, 웨이브 총 시간(10초)에서 스폰 시간(6초) 뺀 만큼 대기
            if (currentWave < totalWaves)
            {
                float remainingTime = waveDuration - spawnDuration;   // 10 - 6 = 4초
                if (remainingTime > 0f)
                    yield return new WaitForSeconds(remainingTime);
            }
        }

        allWavesSpawned = true;
        Debug.Log("모든 웨이브 스폰 완료! 남은 적 처치 대기...");
    }

    void Update()
    {
        if (allWavesSpawned && currentWave > 0)
        {
            EnemyHealth[] remainingEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            if (remainingEnemies.Length == 0)
            {
                GameManager.Instance.OnAllWavesCleared();
                allWavesSpawned = false;
            }
        }
    }

    void SpawnOneEnemy()
    {
        float randomX = Random.Range(-spawnWidth, spawnWidth);
        Vector3 spawnPos = new Vector3(transform.position.x + randomX,
                                       transform.position.y,
                                       transform.position.z);

        Instantiate(enemyPrefab, spawnPos, enemyPrefab.transform.rotation);
    }
}