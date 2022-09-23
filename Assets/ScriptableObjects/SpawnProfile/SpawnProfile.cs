using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnProfile", menuName = "ScriptableObjects/SpawnProfile", order = 2)]
public class SpawnProfile : ScriptableObject
{
    [System.Serializable]
    public struct weightPair
    {
        public GameObject prefab;
        public float weight;
    }

    public weightPair[] pairs;
}
