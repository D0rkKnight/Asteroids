using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public Player[] players;
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

    public ScreenWrapper castZonePrefab;

    // Difficulty control
    public int pointsPerAstWeight = 500;

    public enum ALIGN
    {
        PLAYER, ENEMY
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (sing != null)
            throw new System.Exception("GM Singleton broken");

        sing = this;
    }

    private void Start()
    {
        for (int i = 0; i < players.Length; i++)
        {
            respawnPlayer(i);
        }
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

        int astWeightTarget = baseAstWeightTarget + score / pointsPerAstWeight;
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
            foreach (Player p in players)
                if (p != null) {
                    astValid = astValid && Vector2.Distance(p.transform.position, spawnPoint) > playerSpawnBlockRange;
                    foreach (GameObject ghost in p.GetComponent<ScreenWrapper>().ghosts)
                        if (ghost != null)
                            astValid = astValid && Vector2.Distance(ghost.transform.position, spawnPoint) > playerSpawnBlockRange;
                }

            if (astValid)
            {
                Vector2 spawnDir = Quaternion.Euler(0, 0, Random.Range(-30, 30)) * -spawnPoint.normalized;
                Vector2 exterpSP = spawnPoint + -spawnDir * 3;

                Asteroid ast = spawnAsteroid(targetSize, exterpSP);

                // Give random velocity
                ast.phys.moveVelo = spawnDir * ast.naturalSpeed;
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

    public static void pulseAt(Vector2 pos, float radius, float strength, GameObject[] banList)
    {
        ScreenWrapper pulse = Instantiate(sing.castZonePrefab, pos, Quaternion.identity);
        pulse.GetComponent<CircleCollider2D>().radius = radius;
        pulse.GetComponent<CircleVisualizer>().radius = radius;

        pulse.onCollision.AddListener((GameObject obj) => {
            if (System.Array.Exists(banList, (GameObject gObj) => { return gObj == obj;  })) {
                return; // Item is banned
            }

            PhysicsObject phys = obj.GetComponent<PhysicsObject>();

            if (phys != null)
            {
                phys.moveVelo += strength * ((Vector2)obj.transform.position - pos).normalized;
            }
        });
    }

    public static void parryAt(GameObject caster, Vector2 pos, float radius, float ejectSpeed, GameObject[] banList)
    {
        ScreenWrapper parry = Instantiate(sing.castZonePrefab, pos, Quaternion.identity);
        parry.GetComponent<CircleCollider2D>().radius = radius;
        parry.GetComponent<CircleVisualizer>().radius = radius;

        parry.onCollision.AddListener((GameObject obj) => {
            if (System.Array.Exists(banList, (GameObject gObj) => { return gObj == obj; }))
            {
                return; // Item is banned
            }

            PhysicsObject phys = obj.GetComponent<PhysicsObject>();

            if (phys != null)
            {
                // Throw them away
                Vector2 dir = ((Vector2)phys.transform.position - pos).normalized;
                phys.moveVelo = dir * ejectSpeed;
            }

            Asteroid ast = obj.GetComponent<Asteroid>();
            if (ast != null)
            {
                FlyingObject[] children = ast.onHit();
                foreach (FlyingObject fObj in children)
                    parry.GetComponent<ScreenWrapper>().colBanList.Add(fObj.gameObject);
            }

            // Parry boosts
            Bullet bul = obj.GetComponent<Bullet>();
            if (bul != null)
            {
                bul.transform.localScale *= 2;

                // Reflect it
                Vector2 dir = ((Vector2)bul.transform.position - pos).normalized;
                Vector2 perp = Vector2.Dot(dir, bul.phys.moveVelo) * dir;
                Vector2 tang = bul.phys.moveVelo - perp;

                // Don't reflect if object is travelling away
                int refCoef = (int) Mathf.Sign(Vector2.Dot(bul.phys.moveVelo, dir));

                bul.phys.moveVelo = (refCoef * perp + tang) * 2f;
                bul.piercing *= 3;

                // Recoil
                PhysicsObject cPhys = caster.GetComponent<PhysicsObject>();
                if (cPhys != null)
                {
                    cPhys.moveVelo -= dir * 2.0f;
                }
            }
        });
    }

    public void onPlayerDeath(Player deadPlayer)
    {
        int index = -1;
        for (int i=0; i<players.Length; i++)
        {
            if (players[i] == deadPlayer)
            {
                index = i;
                break;
            }
        }

        if (index >= 0)
        {
            lives--;
            if (lives > 0)
                StartCoroutine(playerDeathCR(index));
            else
                gameOver();
        }
    }

    public IEnumerator playerDeathCR(int index)
    {
        yield return new WaitForSeconds(1f);

        respawnPlayer(index);
    }

    public void respawnPlayer(int index)
    {
        string ctrlScheme = "Keyboard&Mouse";

        if (players.Length > 1)
            switch (index)
            {
                case 0:
                    ctrlScheme = "Left";
                    break;
                case 1:
                    ctrlScheme = "Right";
                    break;
            }

        players[index] = PlayerInput.Instantiate(playerPrefab.gameObject, controlScheme: ctrlScheme, pairWithDevice: Keyboard.current).GetComponent<Player>();

        // Array out players in a row
        Vector2 pos = Vector2.zero;

        if (players.Length > 1)
        {
            float extents = 3;
            pos = new Vector2(-extents + (extents * 2 / (players.Length - 1)) * index, 0);
        }
        players[index].transform.position = pos;

        StartCoroutine(players[index].invulnFor(2f));
    }

    public void gameOver()
    {
        gameIsOver = true;
    }

    public void restartGame()
    {
        lives = 3;
        score = 0;

        for (int i = 0; i < players.Length; i++)
        {
            respawnPlayer(i);
        }

        // Clear every asteroid
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(asteroidTag))
            Destroy(obj);

        gameIsOver = false;
    }
}
