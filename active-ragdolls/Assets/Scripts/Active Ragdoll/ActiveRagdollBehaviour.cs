using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActiveRagdoll;

/// <summary>
/// This is what a custom ActiveRagdoll script should look like
/// </summary>
public class ActiveRagdollBehaviour : MonoBehaviour {
    // Author: Sergio Abreu García | https://sergioabreu.me
    private ActiveRagdoll.ActiveRagdoll _activeRagdoll;
    private SensorsModule _sensors;
    private MovementModule _movement;

    void Start() {
        _activeRagdoll = GetComponent<ActiveRagdoll.ActiveRagdoll>();
        _sensors = GetComponent<SensorsModule>();
        _movement = GetComponent<MovementModule>();
    }

    public void OnFloorChanged(bool onFloor) {

    }
}
