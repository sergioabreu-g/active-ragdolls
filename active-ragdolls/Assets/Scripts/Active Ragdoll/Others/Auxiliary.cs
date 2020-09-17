using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Original author: Sergio Abreu García | https://sergioabreu.me

    [Serializable] public struct JointDriveConfig {
        public float positionSpring, positionDamper, maximumForce;
    }
}
