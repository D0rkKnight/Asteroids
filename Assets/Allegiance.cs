using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Allegiance : MonoBehaviour
{
    public enum ALIGN
    {
        PLAYER, ENEMY
    }
    public ALIGN alignment = ALIGN.PLAYER;

    public static ALIGN[] opp = new ALIGN[2];
    public bool friendlyFire = false;

    // Start is called before the first frame update
    void Start()
    {
        opp[(int)ALIGN.PLAYER] = ALIGN.ENEMY;
        opp[(int)ALIGN.ENEMY] = ALIGN.PLAYER;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool isOpponent(Allegiance alleg)
    {
        // Friendly fire goes both ways
        return opp[(int)alignment] == alleg.alignment || friendlyFire || alleg.friendlyFire;
    }
}
