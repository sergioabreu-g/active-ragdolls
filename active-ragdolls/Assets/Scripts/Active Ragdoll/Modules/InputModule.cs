using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

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

        public void OnLeftArm(InputValue value) {
            SendMessage("InputLeftArm", value.Get<float>(), SendMessageOptions.DontRequireReceiver);
        }

        public void OnRightArm(InputValue value) {
            SendMessage("InputRightArm", value.Get<float>(), SendMessageOptions.DontRequireReceiver);
        }
    }
} // namespace ActiveRagdoll