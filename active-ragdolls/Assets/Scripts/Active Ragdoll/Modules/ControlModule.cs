using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveRagdoll {
    // Original author: Sergio Abreu García | https://sergioabreu.me

    public class ControlModule : Module {
        [Serializable] public struct Config {

        }
        private Config _config;

        override public void StateChanged(in ActiveRagdollState state) {
            _config = state.controlModuleConfig;
        }


        // ---------- ACTIONS ----------

        public void PlayAnimation(string animation, float speed = 1) {
            var animator = _activeRagdoll.GetAnimatedAnimator();
            
            animator.Play(animation);
            animator.SetFloat("speed", speed);
        }
        
        public void SetState(string state) {
            _activeRagdoll.SetState(state);
        }


        // ---------- INPUT HANDLERS ----------

        public void OnMove(InputValue value) {
            Vector2 movement = value.Get<Vector2>();
            PlayAnimation("Walking", movement.y);
        }
    }
}
