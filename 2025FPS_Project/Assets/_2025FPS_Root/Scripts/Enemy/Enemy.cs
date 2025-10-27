using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Enemy", menuName = "Scriptable Objects/Enemy")]
public class Enemy : ScriptableObject
{

    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float chaseSpeed = 6f;
    public LayerMask groundLayer;

    [Header("Vision Settings")]
    public float visionRadius = 6f; 
    public float visionAngle = 45f;
    public float perceptionRadius = 1f;
    public LayerMask obstacleMask;

    [Header("Alert Settings")]
    public float alertTime = 3f;

    [Header("Attack Settings")]
    public float damage = 10f;
    public float attackRangeRadius = 5f;
}
