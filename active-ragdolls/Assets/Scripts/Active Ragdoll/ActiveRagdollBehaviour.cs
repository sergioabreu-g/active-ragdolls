using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActiveRagdoll;

/// <summary>
/// This is what a custom ActiveRagdoll script should look like
/// </summary>
public class ActiveRagdollBehaviour : MonoBehaviour {
    // Author: Sergio Abreu García | https://sergioabreu.me
    [SerializeField] private BalanceModule _balanceModule;

    private void OnValidate() {
        if (_balanceModule == null)
            _balanceModule = GetComponent<BalanceModule>();
    }

    public void OnFloorChanged(bool onFloor) {
        if (onFloor)
            _balanceModule.SetBalanceMode(BalanceModule.BALANCE_MODE.STABILIZER_JOINT);
        else
            _balanceModule.SetBalanceMode(BalanceModule.BALANCE_MODE.MANUAL);
    }
}
