using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Original author: Sergio Abreu García | https://sergioabreu.me

    [Serializable] public struct JointDriveConfig {
        public float positionSpring, positionDamper, maximumForce;
    }

    public static class Auxiliary {
        /// <summary>
        /// Calculates the normalized projection of the Vector3 'vec'
        /// onto the horizontal plane defined by the orthogonal vector (0, 1, 0)
        /// </summary>
        /// <param name="vec">The vector to project</param>
        /// <returns>The normalized projection of 'vec' onto the horizontal plane</returns>
        public static Vector3 GetFloorProjection(in Vector3 vec) {
            return Vector3.ProjectOnPlane(vec, Vector3.up).normalized;
        }
    }

}
