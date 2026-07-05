using UnityEngine;
using System.Collections;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text waveText;

    [Header("스폰 설정")]
    public GameObject enemyPrefab;
    public int baseEnemiesPerWave = 5;   // 1일차 웨이브당 적 수 (기준값)

    [Header("웨이브 타이밍")]
    public float waveDuration = 10f;
    public float spawnDuration = 6f;

    [Header("스폰 위치 범위")]
    public float spawnWidth = 4f;

    [Header("난이도 상승 (Day별) — 값은 직접 조정")]
    public int enemiesAddPerDay = 0;        // Day마다 웨이브당 적 수 +이만큼
    public float healthAddPerDay = 0f;      // Day마다 적 체력 +이만큼 (프리팹 기본체력에 가산)

    private int currentWave = 0;
    private bool allWavesSpawned = false;
    private int currentDay = 1;

    // 이번 날 적용될 웨이브당 적 수 (Day에 따라 계산됨)
    private int enemiesPerWave;

    // 하루(3웨이브) 시작
    public void StartDay(int day)
    {
        currentDay = day;

        // Day별 적 수 계산 (day=1이면 기준값 그대로). 체력은 스폰 시 프리팹 기준으로 계산.
        enemiesPerWave = baseEnemiesPerWave + enemiesAddPerDay * (day - 1);

        currentWave = 0;
        allWavesSpawned = false;

        StopAllCoroutines();          // 혹시 이전 코루틴 남아있으면 정리
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

            Debug.Log($"[Day {currentDay}] 웨이브 {currentWave}/{totalWaves} (적 {enemiesPerWave}마리)");

            float spawnInterval = (enemiesPerWave > 0) ? spawnDuration / enemiesPerWave : 0f;

            for (int i = 0; i < enemiesPerWave; i++)
            {
                SpawnOneEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            if (currentWave < totalWaves)
            {
                float remainingTime = waveDuration - spawnDuration;
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

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, enemyPrefab.transform.rotation);

        // 프리팹 기본 체력 + Day 증가량을 이 적에 적용
        EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
        if (eh != null)
        {
            int baseHp = eh.maxHealth;   // 프리팹이 들고 있는 기본 체력
            int finalHp = Mathf.RoundToInt(baseHp + healthAddPerDay * (currentDay - 1));
            eh.SetMaxHealth(finalHp);
        }
    }
}