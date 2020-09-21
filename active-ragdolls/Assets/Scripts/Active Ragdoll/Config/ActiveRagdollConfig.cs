using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    [CreateAssetMenu(fileName = "Active Ragdoll State", menuName = "ScriptableObjects/Active Ragdoll State", order = 1)]
    public class ActiveRagdollConfig : ScriptableObject {
        public string stateName;

        public BalanceModule.Config balanceModuleConfig;
        public MovementModule.Config movementModuleConfig;
        public InputModule.Config inputModuleConfig;
        public CameraModule.Config cameraModuleConfig;
        public GripModule.Config gripModuleConfig;
    }
}
