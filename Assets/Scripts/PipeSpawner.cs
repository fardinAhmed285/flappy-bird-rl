using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject pipePairPrefab;
    [SerializeField] private GameObject lifePrizePrefab;
    [SerializeField] [Range(0f, 1f)] private float prizeSpawnChance = 0.35f;
    [SerializeField] private float prizeVerticalOffset = 1.6f;
    [SerializeField] private float spawnInterval = 1.6f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float spawnX = 10f;
    [SerializeField] private float destroyX = -10f;
    [SerializeField] private float minGapY = -2f;
    [SerializeField] private float maxGapY = 2f;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnPipeAndMaybePrize();
        }
    }

    private void SpawnPipeAndMaybePrize()
    {
        if (pipePairPrefab == null) return;

        float gapY = Random.Range(minGapY, maxGapY);
        Vector3 pipePos = transform.position + new Vector3(spawnX, gapY, 0f);
        GameObject pipe = Instantiate(pipePairPrefab, pipePos, Quaternion.identity, transform);

        ScrollLeft pipeMover = pipe.AddComponent<ScrollLeft>();
        pipeMover.speed = moveSpeed;
        pipeMover.destroyX = destroyX;

        if (lifePrizePrefab != null && Random.value < prizeSpawnChance)
        {
            float dir = Random.value < 0.5f ? 1f : -1f;
            float prizeY = Mathf.Clamp(gapY + dir * prizeVerticalOffset, -3.5f, 3.5f);
            Vector3 prizePos = transform.position + new Vector3(spawnX + 1.5f, prizeY, 0f);
            GameObject prize = Instantiate(lifePrizePrefab, prizePos, Quaternion.identity, transform);

            ScrollLeft prizeMover = prize.AddComponent<ScrollLeft>();
            prizeMover.speed = moveSpeed;
            prizeMover.destroyX = destroyX;
        }
    }

    private class ScrollLeft : MonoBehaviour
    {
        public float speed;
        public float destroyX;

        private void Update()
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
            if (transform.position.x < destroyX)
            {
                Destroy(gameObject);
            }
        }
    }
}