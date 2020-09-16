using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveRagdoll : MonoBehaviour {
    [Header("Body")]
    [SerializeField] private Transform _animatedTorso;
    [SerializeField] private Rigidbody _physicalTorso;

    [Header("Advanced")]
    [SerializeField] private int _solverIterations = 11;
    [SerializeField] private int _velSolverIterations = 11;
}
