using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveRagdoll {
    // Original author: Sergio Abreu García | https://sergioabreu.me

    public class InputModule : Module {
        [Serializable] public struct Config {

        }
        private Config _config;

        override public void StateChanged(in ActiveRagdollState state) {
            _config = state.inputModuleConfig;
        }



        // ---------- INPUT HANDLERS ----------

        public void OnMove(InputValue value) {
            Vector2 movement = value.Get<Vector2>();
            SendMessage("InputMove", movement, SendMessageOptions.DontRequireReceiver);
        }
    }
} // namespace ActiveRagdoll