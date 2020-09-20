using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ActiveRagdoll {
    // Original author: Sergio Abreu García | https://sergioabreu.me

    /// <summary> The active ragdoll functionality is subdivided in modules, so
    /// everything is easier debug and mantain. </summary>
    [RequireComponent(typeof(ActiveRagdoll))]
    public class Module : MonoBehaviour {
        protected ActiveRagdoll _activeRagdoll;



        public void Initialize(ActiveRagdoll activeRagdoll) {
            _activeRagdoll = activeRagdoll;
            LateAwake();
        }

        /// <summary> This method gets called right after the initialization of the module,
        /// which is done at the end of the ActiveRagdoll's 'Awake' function. </summary>
        virtual protected void LateAwake() { }

        /// <summary> Method called by the ActiveRagdoll when its state changes.
        /// Used by each module to update itself automatically. </summary>
        /// <param name="state">The new active state of the Active Ragdoll</param>
        virtual public void StateChanged(in ActiveRagdollState state) { }
    }



    [CustomEditor(typeof(Module), true), CanEditMultipleObjects]
    public class ModuleEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox("You can modify this Module's configuration from " +
                "                   your states files.", MessageType.Info, true);
        }
    }
} // namespace ActiveRagdoll
