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

    [Header("난이도 상승 (Day별)")]
    public int enemiesAddPerDay = 0;
    public float healthAddPerDay = 0f;

    [Header("보스")]
    public GameObject bossPrefab;             // 보스 몬스터 프리팹
    public int[] bossDays = { 10, 20, 30 };   // 보스 등장하는 날
    public int bossBaseHp = 2000;             // 보스 기본 체력

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

            // 일반 적 스폰
            for (int i = 0; i < enemiesPerWave; i++)
            {
                SpawnOneEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            // 보스 날이고 마지막 웨이브면 보스 스폰
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
            int finalHp = Mathf.RoundToInt(baseHp + baseHp * healthAddPerDay * (currentDay - 1));
            eh.SetMaxHealth(finalHp);
        }
    }

    void SpawnBoss()
    {
        if (bossPrefab == null) return;

        float randomX = Random.Range(-spawnWidth, spawnWidth);
        Vector3 spawnPos = new Vector3(transform.position.x + randomX,
                                       transform.position.y,
                                       transform.position.z);

        GameObject boss = Instantiate(bossPrefab, spawnPos, bossPrefab.transform.rotation);

        EnemyHealth eh = boss.GetComponent<EnemyHealth>();
        if (eh != null)
        {
            int finalHp = Mathf.RoundToInt(bossBaseHp + bossBaseHp * healthAddPerDay * (currentDay - 1));
            eh.SetMaxHealth(finalHp);
        }

        Debug.Log($"[Day {currentDay}] 보스 등장!");
    }

    private bool IsBossDay(int day)
    {
        foreach (int d in bossDays)
            if (d == day) return true;
        return false;
    }
}