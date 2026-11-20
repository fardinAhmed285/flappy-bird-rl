using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class FlappyAgent : Agent
{
    [SerializeField] private float flapVelocity = 5.5f;
    [SerializeField] private float startY = 0f;
    [SerializeField] private int startingLives = 1;
    [SerializeField] private int maxLives = 3;
    [SerializeField] private Transform spawnerParent;

    private Rigidbody2D rb;
    private float startX;
    private int lives;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        startX = transform.localPosition.x;
        if (spawnerParent == null)
        {
            var spawner = FindFirstObjectByType<PipeSpawner>();
            if (spawner != null) spawnerParent = spawner.transform;
        }
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(startX, startY, transform.localPosition.z);
        rb.linearVelocity = Vector2.zero;
        lives = startingLives;

        ClearSpawnedObjects();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1-3: Bird state (normalized roughly to [-1, 1])
        sensor.AddObservation(transform.localPosition.y / 5f);
        sensor.AddObservation(rb.linearVelocity.y / 10f);
        sensor.AddObservation((float)lives / maxLives);

        // 4-5: Next pipe ahead of the bird (dx, dy)
        Transform nextPipe = FindClosestAhead("PipePair");
        if (nextPipe != null)
        {
            sensor.AddObservation((nextPipe.position.x - transform.position.x) / 10f);
            sensor.AddObservation((nextPipe.position.y - transform.position.y) / 5f);
        }
        else
        {
            sensor.AddObservation(1f);
            sensor.AddObservation(0f);
        }

        // 6-7: Next prize ahead of the bird (dx, dy)
        Transform nextPrize = FindClosestAheadWithTag("Prize");
        if (nextPrize != null)
        {
            sensor.AddObservation((nextPrize.position.x - transform.position.x) / 10f);
            sensor.AddObservation((nextPrize.position.y - transform.position.y) / 5f);
        }
        else
        {
            sensor.AddObservation(1f);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int flapAction = actions.DiscreteActions[0];
        if (flapAction == 1)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, flapVelocity);
            // Tiny energy cost for flapping so it doesn't spam flap into the ceiling
            AddReward(-0.002f);
        }

        // Survival reward per step
        AddReward(0.01f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) ? 1 : 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Prize"))
        {
            lives = Mathf.Min(lives + 1, maxLives);
            AddReward(0.5f);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Pipe"))
        {
            Destroy(other.gameObject);
            HandleHit();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Pipe") ||
            collision.gameObject.name == "Ceiling" ||
            collision.gameObject.name == "Floor")
        {
            HandleHit();
        }
    }

    private void HandleHit()
    {
        lives--;
        AddReward(-0.5f);

        if (lives <= 0)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
        else
        {
            // Reset to center height on extra-life save so it isn't stuck on Ceiling/Floor
            transform.localPosition = new Vector3(startX, startY, transform.localPosition.z);
            rb.linearVelocity = Vector2.zero;
        }
    }

    private Transform FindClosestAhead(string namePrefix)
    {
        if (spawnerParent == null) return null;
        Transform best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < spawnerParent.childCount; i++)
        {
            Transform child = spawnerParent.GetChild(i);
            if (!child.name.StartsWith(namePrefix)) continue;
            float dx = child.position.x - transform.position.x;
            if (dx >= -0.5f && dx < bestDist)
            {
                bestDist = dx;
                best = child;
            }
        }
        return best;
    }

    private Transform FindClosestAheadWithTag(string tagName)
    {
        if (spawnerParent == null) return null;
        Transform best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < spawnerParent.childCount; i++)
        {
            Transform child = spawnerParent.GetChild(i);
            if (!child.CompareTag(tagName)) continue;
            float dx = child.position.x - transform.position.x;
            if (dx >= -0.5f && dx < bestDist)
            {
                bestDist = dx;
                best = child;
            }
        }
        return best;
    }

    private void ClearSpawnedObjects()
    {
        if (spawnerParent == null) return;
        for (int i = spawnerParent.childCount - 1; i >= 0; i--)
        {
            Destroy(spawnerParent.GetChild(i).gameObject);
        }
    }
}