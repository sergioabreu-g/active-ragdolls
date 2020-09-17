using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Original author: Sergio Abreu García | https://sergioabreu.me

    /// <summary>
    /// The active ragdoll functionality is subdivided in modules, so
    /// everything is easier to manage and can be easily debugged and
    /// mantained. This is the class every Module must inherit from.
    /// </summary>
    public class Module : MonoBehaviour {
        // The Active Ragdoll this module instace is attached to
        protected ActiveRagdoll _activeRagdoll;

        protected virtual void Start() {
            if (_activeRagdoll == null)
                Debug.LogError("No Active Ragdoll script could be found in this GameObject. Modules can only be attached to GameObjects with an ActiveRagdoll script.");
        }

        /// <summary>
        /// Initializes this module, linking it to the given Active Ragdoll
        /// </summary>
        /// <param name="activeRagdoll">Active Ragdoll to link this module to</param>
        public void Initialize(ActiveRagdoll activeRagdoll) {
            _activeRagdoll = activeRagdoll;
        }
    }
}