using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public Player[] players;
    public Player playerPrefab;

    public SpawnProfile spawnProfile;

    public int baseAstWeightTarget = 5;
    public float astSpawnSpin = 20f;
    public float playerSpawnBlockRange = 2f;

    public float perimPadding = 1;
    public int score = 0;
    public int lives = 3;
    public bool gameIsOver = false;

    public static GameManager sing;
    public const string fObjTag = "FlyingObject";

    public ScreenWrapper castZonePrefab;
    public Afterimage afterimagePrefab;

    // Difficulty control
    public int pointsPerAstWeight = 500;

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

        GameObject[] asteroids = GameObject.FindGameObjectsWithTag(fObjTag);
        foreach (GameObject g in asteroids)
        {
            FlyingObject a = g.GetComponent<FlyingObject>();

            totalAstWeight += a.size + 1; // Use something else later
        }

        int astWeightTarget = baseAstWeightTarget + score / pointsPerAstWeight;
        if (totalAstWeight < astWeightTarget)
        {
            // Get size
            float wRange = 0f;
            foreach (SpawnProfile.weightPair pair in spawnProfile.pairs)
                wRange += pair.weight;

            float randWeight = Random.Range(0, wRange);

            SpawnProfile.weightPair targetPair = spawnProfile.pairs[0];
            foreach (SpawnProfile.weightPair pair in spawnProfile.pairs)
            {
                if (randWeight < pair.weight)
                {
                    targetPair = pair;
                    break;
                }
                randWeight -= pair.weight;
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
            bool spawnValid = true;
            foreach (Player p in players)
                if (p != null) {
                    spawnValid = spawnValid && Vector2.Distance(p.transform.position, spawnPoint) > playerSpawnBlockRange;
                    foreach (GameObject ghost in p.GetComponent<ScreenWrapper>().ghosts)
                        if (ghost != null)
                            spawnValid = spawnValid && Vector2.Distance(ghost.transform.position, spawnPoint) > playerSpawnBlockRange;
                }

            if (spawnValid)
            {
                Vector2 spawnDir = Quaternion.Euler(0, 0, Random.Range(-30, 30)) * -spawnPoint.normalized;
                Vector2 exterpSP = spawnPoint + -spawnDir * 3;

                FlyingObject spawn = spawnFlyingObj(targetPair.prefab.GetComponent<FlyingObject>(), exterpSP);

                // Give random velocity
                spawn.phys.moveVelo = spawnDir * spawn.naturalSpeedCap;
                spawn.phys.spinVelo = Random.Range(-astSpawnSpin, astSpawnSpin);
            }
        }
    }

    public static void getGameCorners(out Vector2 bl, out Vector2 ur)
    {
        float padding = sing.perimPadding;
        bl = Camera.main.ViewportToWorldPoint(Vector2.zero) - new Vector3(padding, padding);
        ur = Camera.main.ViewportToWorldPoint(new Vector2(1, 1)) + new Vector3(padding, padding);
    }

    public static FlyingObject spawnFlyingObj(FlyingObject fObj, Vector2 pos)
    {
        // Spawn and send it out
        FlyingObject spawned = Instantiate(fObj, pos, Quaternion.identity);
        spawned.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-180f, 180f));
        spawned.GetComponent<ScreenWrapper>().loopable = false;

        return spawned;
    }

    public static Asteroid spawnAsteroid(int size, Vector2 pos)
    {
        List<Asteroid> valid = new();

        foreach (SpawnProfile.weightPair pair in sing.spawnProfile.pairs)
        {
            Asteroid ast = pair.prefab.GetComponent<Asteroid>();

            if (ast != null && ast.size == size)
                valid.Add(ast);
        }

        Asteroid pick = valid[Random.Range(0, valid.Count)];
        Asteroid spawn = (Asteroid) spawnFlyingObj(pick, pos);

        return spawn;
    }

    public static void pulseAt(GameObject caster, Vector2 pos, float radius, float strength, GameObject[] banList)
    {
        ScreenWrapper pulse = Instantiate(sing.castZonePrefab, pos, Quaternion.identity);
        pulse.GetComponent<CircleCollider2D>().radius = radius;
        pulse.GetComponent<CircleVisualizer>().radius = radius;
        pulse.colBanList = new List<GameObject>(banList);

        pulse.onCollision.AddListener((GameObject obj) => {
            PhysicsObject phys = obj.GetComponent<PhysicsObject>();

            if (phys != null)
            {
                Vector2 newVelo = phys.moveVelo + strength * ((Vector2)obj.transform.position - pos).normalized;

                // Some objects aren't rotable
                if (phys.pushRotable)
                    rotateWithVeloChange(phys, newVelo);
                else
                    phys.moveVelo = newVelo;
            }

            FlyingObject fObj = obj.GetComponent<FlyingObject>();
        });
    }

    public static void parryAt(GameObject caster, Vector2 pos, float radius, float ejectSpeed, GameObject[] banList)
    {
        ScreenWrapper parry = Instantiate(sing.castZonePrefab, pos, Quaternion.identity);
        parry.GetComponent<CircleCollider2D>().radius = radius;
        parry.GetComponent<CircleVisualizer>().radius = radius;
        parry.colBanList = new List<GameObject>(banList);

        parry.onCollision.AddListener((GameObject obj) => {
            PhysicsObject phys = obj.GetComponent<PhysicsObject>();
            FlyingObject fObj = obj.GetComponent<FlyingObject>();
            Bullet bul = obj.GetComponent<Bullet>();

            /*if (fObj != null)
            {
                FlyingObject[] children = fObj.onHit(null);
                foreach (FlyingObject child in children)
                    parry.GetComponent<ScreenWrapper>().colBanList.Add(child.gameObject);
            }*/

            // Parry boosts
            if (bul != null)
            {
                bul.transform.localScale *= 1.7f;

                // Reflect it
                Vector2 dir = ((Vector2)bul.transform.position - pos).normalized;
                //Vector2 perp = Vector2.Dot(dir, bul.phys.moveVelo) * dir;
                //Vector2 tang = bul.phys.moveVelo - perp;

                // Don't reflect if object is travelling away
                // int refCoef = (int) Mathf.Sign(Vector2.Dot(bul.phys.moveVelo, dir));

                // Vector2 newVelo = (refCoef * perp + tang) * 2f;
                Vector2 newVelo = dir * phys.moveVelo.magnitude * 2f;
                rotateWithVeloChange(bul.phys, newVelo);
                bul.piercing *= 3;

                // Recoil
                PhysicsObject cPhys = caster.GetComponent<PhysicsObject>();
                if (cPhys != null)
                {
                    cPhys.moveVelo -= dir * 2.0f;
                }

                // Switch allegiance
                Allegiance cAlleg = caster.GetComponent<Allegiance>();
                Allegiance bAlleg = bul.GetComponent<Allegiance>();

                if (cAlleg != null && bAlleg != null)
                    bAlleg.alignment = cAlleg.alignment;

                // Put on trail
                bul.StartCoroutine(spawnAfterImages(bul.gameObject, 1f/5, (int) bul.life * 5));
            }
            else if (phys != null)
            {
                // Throw them away
                Vector2 dir = ((Vector2)phys.transform.position - pos).normalized;
                phys.moveVelo = dir * ejectSpeed;
            }
        });
    }

    public static void rotateWithVeloChange(PhysicsObject pObj, Vector2 newVelo)
    {
        // Rotate bullet
        float turnAng = Vector2.SignedAngle(pObj.moveVelo, newVelo);
        pObj.transform.Rotate(0, 0, turnAng);

        pObj.moveVelo = newVelo;
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

        // If this player is a tracked player
        if (index >= 0)
        {
            lives = Mathf.Max(0, lives-1);
            players[index] = null;

            if (lives > 0)
                StartCoroutine(playerDeathCR(index));
            else
            {
                // If it was the last player, throw it in the towel
                bool gOver = true;
                foreach (Player p in players)
                    if (p != null)
                    {
                        gOver = false;
                        break;
                    }

                if (gOver)
                    gameOver();
            }
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

        // Pulse at the location
        pulseAt(players[index].gameObject, pos, 3f, 1f, new GameObject[] { players[index].gameObject });

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
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(fObjTag))
            Destroy(obj);

        gameIsOver = false;
    }

    public static IEnumerator spawnAfterImages(GameObject caster, float dur, int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            // Spawn an afterimage
            Afterimage ai = Instantiate(sing.afterimagePrefab, caster.transform.position, caster.transform.rotation);
            ai.transform.localScale = caster.transform.localScale;

            // Copy over sprite
            ai.rend.sprite = caster.GetComponent<SpriteRenderer>().sprite;

            yield return new WaitForSeconds(dur);
        }
    }
}
