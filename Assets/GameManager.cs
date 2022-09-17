using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player;
    public Player playerPrefab;

    public Asteroid[] asteroidPrefabs;
    public float[] astSpawnWeights;
    public int astWeightTarget = 5;
    public float astSpawnVelocity = 1f;
    public float astSpawnSpin = 20f;
    public float playerSpawnBlockRange = 2f;

    public float perimPadding = 1;
    public int score = 0;
    public int lives = 3;
    public bool gameIsOver = false;

    public static GameManager sing;
    public const string asteroidTag = "Asteroid";

    // Start is called before the first frame update
    void Start()
    {
        if (sing != null)
            throw new System.Exception("GM Singleton broken");

        sing = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
            loopObject(player.transform);

        // Count the # of asteroids and spawn more if few are left
        int totalAstWeight = 0;

        GameObject[] asteroids = GameObject.FindGameObjectsWithTag(asteroidTag);
        foreach (GameObject g in asteroids)
        {
            // Loop the asteroid
            loopObject(g.transform);

            Asteroid a = g.GetComponent<Asteroid>();

            totalAstWeight += a.size; // Use something else later
        }

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
            Vector2 bl = Camera.main.ViewportToWorldPoint(Vector2.zero) - new Vector3(perimPadding, perimPadding);
            Vector2 ur = Camera.main.ViewportToWorldPoint(new Vector2(1, 1)) + new Vector3(perimPadding, perimPadding);
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

            // Check point distance from player
            if (player == null || Vector2.Distance(player.transform.position, spawnPoint) > playerSpawnBlockRange)
            {
                Asteroid ast = spawnAsteroid(targetSize, spawnPoint);

                // Give random velocity
                ast.phys.moveVelo = Random.insideUnitCircle.normalized * astSpawnVelocity;
                ast.phys.spinVelo = Random.Range(-astSpawnSpin, astSpawnSpin);
            }
        }
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

        return spawned;
    }

    public static void loopObject(Transform obj)
    {
        // Loop the target if they leave the camera
        float padding = sing.perimPadding;

        Vector2 pPos = obj.position;
        Vector2 bl = Camera.main.ViewportToWorldPoint(Vector2.zero) - new Vector3(padding, padding);
        Vector2 ur = Camera.main.ViewportToWorldPoint(new Vector2(1, 1)) + new Vector3(padding, padding);
        Vector2 dims = ur - bl;

        if (pPos.x < bl.x)
            pPos.x += dims.x;
        if (pPos.x > ur.x)
            pPos.x -= dims.x;
        if (pPos.y < bl.y)
            pPos.y += dims.y;
        if (pPos.y > ur.y)
            pPos.y -= dims.y;

        obj.position = pPos;
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
