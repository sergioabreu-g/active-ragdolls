using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    [CreateAssetMenu(fileName = "Active Ragdoll State", menuName = "ScriptableObjects/Active Ragdoll State", order = 1)]
    public partial class ActiveRagdollState : ScriptableObject {
        public string stateName;

        public BalanceModule.Config balanceModuleConfig;
        public MovementModule.Config movementModuleConfig;
        public InputModule.Config inputModuleConfig;
    }
}
