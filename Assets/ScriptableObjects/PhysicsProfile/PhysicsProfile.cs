using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhysicsProfile", menuName = "ScriptableObjects/PhysicsProfile", order = 1)]
public class PhysicsProfile : ScriptableObject
{
    public float linearDrag = 1f;
    public float angularSlowdown = 1f;

    public float maxSpinVelo = 1000f;
    public float maxMoveVelo = 1000f;

    public float softMaxMoveVelo = 10f;
    public float overcapLinearDrag = 4f;
}
