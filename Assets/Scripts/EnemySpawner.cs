using UnityEngine;
using System.Collections;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text waveText;

    [Header("스폰 설정")]
    public GameObject enemyPrefab;
    public int enemiesPerWave = 5; // 한 웨이브당 스폰할 적 수
    public float spawnInterval = 1f;
    public float timeBetweenWaves = 3f;

    [Header("스폰 위치 범위")]
    public float spawnWidth = 4f;

    private int currentWave = 0;
    private bool allWavesSpawned = false;  // 모든 웨이브 스폰 완료 여부

    void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        // GameManager의 목표 웨이브 수만큼 반복
        int totalWaves = GameManager.Instance.targetWave;

        while (currentWave < totalWaves)
        {
            currentWave++;
            if(waveText!= null)
            {
                waveText.text = "웨이브 "+currentWave + " / " + totalWaves;
            }

            Debug.Log("웨이브 " + currentWave + " / " + totalWaves);

            for (int i = 0; i < enemiesPerWave; i++)
            {
                SpawnOneEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }

        // 모든 웨이브 스폰 완료
        allWavesSpawned = true;
        Debug.Log("모든 웨이브 스폰 완료! 남은 적 처치 대기...");
    }

    void Update()
    {
        // 모든 웨이브를 스폰했고, 남은 적이 없으면 승리
        if (allWavesSpawned && currentWave > 0)
        {
            // 씬에 남은 적이 있는지 확인
            EnemyHealth[] remainingEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            if (remainingEnemies.Length == 0)
            {
                GameManager.Instance.OnAllWavesCleared();
                allWavesSpawned = false;  // 중복 호출 방지
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