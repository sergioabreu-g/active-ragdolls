using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    public class BalancerModule : Module {
        // Original author: Sergio Abreu García | https://sergioabreu.me

        private enum BALANCE_MODE {
            FREEZE_ROTATIONS,
        }

        [SerializeField] private BALANCE_MODE _balanceMode;

        override protected void Start() {
            base.Start();

            if (_balanceMode == BALANCE_MODE.FREEZE_ROTATIONS) {
                _activeRagdoll.GetPhysicalTorso().constraints =
                    RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }
    }
}