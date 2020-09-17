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
    [RequireComponent(typeof(ActiveRagdoll))]
    public class Module : MonoBehaviour {
        // The Active Ragdoll this module instace is attached to
        protected ActiveRagdoll _activeRagdoll;

        /// <summary>
        /// Initializes this module, linking it to the given Active Ragdoll. This function
        /// gets called from the ActiveRagdoll 'Awake()' method, so it can be used as an awake
        /// to ensure
        /// </summary>
        /// <param name="activeRagdoll">Active Ragdoll to link this module to</param>
        public void Initialize(ActiveRagdoll activeRagdoll) {
            _activeRagdoll = activeRagdoll;
            LateAwake();
        }

        /// <summary>
        /// This method gets called right after the initialization of the module, which is
        /// done at the end of the ActiveRagdoll's 'Awake' function.
        /// </summary>
        virtual protected void LateAwake() { }

        /// <summary>
        /// Method called by the ActiveRagdoll when its state changes. Used by each module to update
        /// itself automatically
        /// </summary>
        /// <param name="state">The new active state of the Active Ragdoll</param>
        virtual public void StateChanged(in ActiveRagdollState state) { }
    }
} // namespace ActiveRagdoll