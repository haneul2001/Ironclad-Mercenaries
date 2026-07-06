using UnityEngine;
using System.Collections;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text waveText;

    [Header("스폰 설정")]
    public GameObject enemyPrefab;
    public int baseEnemiesPerWave = 5;

    [Header("웨이브 타이밍")]
    public float waveDuration = 10f;
    public float spawnDuration = 6f;

    [Header("스폰 위치 범위")]
    public float spawnWidth = 4f;
    public float bossSpawnYOffset = 0f;

    [Header("난이도 상승 (Day별)")]
    public int enemiesAddPerDay = 0;
    public float healthGrowthPerDay = 1.5f;   // 하루마다 체력 배율 (1.5 = 매일 1.5배, 지수 증가)

    [Header("보스")]
    public GameObject bossPrefab;
    public int[] bossDays = { 10, 20, 30 };
    public int bossBaseHp = 2000;

    private int currentWave = 0;
    private bool allWavesSpawned = false;
    private int currentDay = 1;
    private int enemiesPerWave;

    public void StartDay(int day)
    {
        currentDay = day;
        enemiesPerWave = baseEnemiesPerWave + enemiesAddPerDay * (day - 1);

        currentWave = 0;
        allWavesSpawned = false;

        StopAllCoroutines();
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

            if (IsBossDay(currentDay) && currentWave == totalWaves)
            {
                SpawnBoss();
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

        EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
        if (eh != null)
        {
            int baseHp = eh.maxHealth;
            // 하루마다 healthGrowthPerDay배씩 (지수 증가): baseHp × 배율^(day-1)
            float finalHpF = baseHp * Mathf.Pow(healthGrowthPerDay, currentDay - 1);
            eh.SetMaxHealth(Mathf.RoundToInt(finalHpF));
        }
    }

    void SpawnBoss()
    {
        if (bossPrefab == null) return;

        Vector3 spawnPos = new Vector3(transform.position.x,
                                       transform.position.y + bossSpawnYOffset,
                                       transform.position.z);

        GameObject boss = Instantiate(bossPrefab, spawnPos, bossPrefab.transform.rotation);

        EnemyHealth eh = boss.GetComponent<EnemyHealth>();
        if (eh != null)
        {
            // 날짜별 체력 배율: 10일차=1배, 20일차=100배, 30일차=10000배
            float dayMultiplier = 1f;
            if (currentDay >= 30)
                dayMultiplier = 10000f;
            else if (currentDay >= 20)
                dayMultiplier = 100f;

            int finalHp = Mathf.RoundToInt(bossBaseHp * dayMultiplier);
            eh.SetMaxHealth(finalHp);
        }

        if (BossHealthBar.Instance != null && eh != null)
            BossHealthBar.Instance.ShowBoss(eh);

        Debug.Log($"[Day {currentDay}] 보스 등장!");
    }

    private bool IsBossDay(int day)
    {
        foreach (int d in bossDays)
            if (d == day) return true;
        return false;
    }
}