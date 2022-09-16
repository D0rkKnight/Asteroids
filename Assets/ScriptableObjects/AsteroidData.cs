using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AsteroidData", menuName = "ScriptableObjects/AsteroidData", order = 1)]
public class AsteroidData : ScriptableObject
{
    public int size; // 0 is smallest
    public Sprite spr;
}
