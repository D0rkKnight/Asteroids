using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player;
    public Player playerPrefab;

    public Asteroid[] asteroidPrefabs;
    public float[] astSpawnWeights;
    public int baseAstWeightTarget = 5;
    public float astSpawnSpin = 20f;
    public float playerSpawnBlockRange = 2f;

    public float perimPadding = 1;
    public int score = 0;
    public int lives = 3;
    public bool gameIsOver = false;

    public static GameManager sing;
    public const string asteroidTag = "Asteroid";

    // Start is called before the first frame update
    void Awake()
    {
        if (sing != null)
            throw new System.Exception("GM Singleton broken");

        sing = this;
    }

    // Update is called once per frame
    void Update()
    {
        // Count the # of asteroids and spawn more if few are left
        int totalAstWeight = 0;

        GameObject[] asteroids = GameObject.FindGameObjectsWithTag(asteroidTag);
        foreach (GameObject g in asteroids)
        {
            Asteroid a = g.GetComponent<Asteroid>();

            totalAstWeight += a.size + 1; // Use something else later
        }

        int astWeightTarget = baseAstWeightTarget + score / 1000;
        if (totalAstWeight < astWeightTarget)
        {
            // Get size
            float wRange = 0f;
            foreach (float f in astSpawnWeights)
                wRange += f;

            float randWeight = Random.Range(0, wRange);

            int targetSize = 0;
            for (int i=0; i<astSpawnWeights.Length; i++)
            {
                if (randWeight < astSpawnWeights[i])
                {
                    targetSize = i;
                    break;
                }
                randWeight -= astSpawnWeights[i];
            }

            // Get random spot on the perimeter
            getGameCorners(out Vector2 bl, out Vector2 ur);
            float pw = ur.x - bl.x;
            float ph = ur.y - bl.y;
            float pRange = (pw + ph) * 2;

            // Cycle ccw from bl
            float randPerim = Random.Range(0, pRange);
            Vector2 spawnPoint = bl;

            // Actually genius solution
            spawnPoint.x += Mathf.Min(pw, randPerim);
            randPerim = Mathf.Max(0, randPerim - pw);

            spawnPoint.y += Mathf.Min(ph, randPerim);
            randPerim = Mathf.Max(0, randPerim - ph);

            spawnPoint.x -= Mathf.Min(pw, randPerim);
            randPerim = Mathf.Max(0, randPerim - pw);

            spawnPoint.y -= Mathf.Min(ph, randPerim);

            // Prevent asteroid from spawning on top of player
            bool astValid = true;
            if (player != null) {
                astValid = astValid && Vector2.Distance(player.transform.position, spawnPoint) > playerSpawnBlockRange;
                foreach (GameObject ghost in player.GetComponent<ScreenWrapper>().ghosts)
                    if (ghost != null)
                        astValid = astValid && Vector2.Distance(ghost.transform.position, spawnPoint) > playerSpawnBlockRange;
            }

            if (astValid)
            {
                Vector2 spawnVelo = Quaternion.Euler(0, 0, Random.Range(-30, 30)) * -spawnPoint.normalized;
                Vector2 exterpSP = spawnPoint + -spawnVelo * 3;

                Asteroid ast = spawnAsteroid(targetSize, exterpSP);

                // Give random velocity
                ast.phys.moveVelo = Random.insideUnitCircle.normalized * ast.naturalSpeed;
                ast.phys.spinVelo = Random.Range(-astSpawnSpin, astSpawnSpin);
            }
        }
    }

    public static void getGameCorners(out Vector2 bl, out Vector2 ur)
    {
        float padding = sing.perimPadding;
        bl = Camera.main.ViewportToWorldPoint(Vector2.zero) - new Vector3(padding, padding);
        ur = Camera.main.ViewportToWorldPoint(new Vector2(1, 1)) + new Vector3(padding, padding);
    }

    public static Asteroid spawnAsteroid(int size, Vector2 pos)
    {
        // Builds valid asteroid list on demand because I really don't want to mantain a cache
        List<Asteroid> valid = new List<Asteroid>();

        foreach (Asteroid a in sing.asteroidPrefabs)
            if (a.size == size)
                valid.Add(a);

        int rand = Random.Range(0, valid.Count);
        Asteroid pref = valid[rand];

        Asteroid spawned = Instantiate(pref, pos, Quaternion.identity);
        spawned.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-180f, 180f));
        spawned.GetComponent<ScreenWrapper>().loopable = false;

        return spawned;
    }

    public static void pulseAt(Vector2 pos, float radius, float strength)
    {
        Collider2D[] results = new Collider2D[100]; // 100 item cap
        int collsFound = Physics2D.OverlapCircle(pos, radius, new ContactFilter2D().NoFilter(), results);

        // Push gameobjects away
        for (int i=0; i<collsFound; i++)
        {
            Collider2D coll = results[i];
            PhysicsObject phys = coll.GetComponent<PhysicsObject>();

            if (phys != null)
            {
                phys.moveVelo += strength * ((Vector2)coll.transform.position - pos).normalized;
            }
        }
    }

    public void onPlayerDeath()
    {
        lives--;

        if (lives > 0)
            StartCoroutine(playerDeathCR());
        else
            gameOver();
    }

    public IEnumerator playerDeathCR()
    {
        yield return new WaitForSeconds(1f);

        respawnPlayer();
    }

    public void respawnPlayer()
    {
        player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        StartCoroutine(player.invulnFor(2f));
    }

    public void gameOver()
    {
        gameIsOver = true;
    }

    public void restartGame()
    {
        lives = 3;
        score = 0;

        respawnPlayer();

        // Clear every asteroid
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(asteroidTag))
            Destroy(obj);

        gameIsOver = false;
    }
}
