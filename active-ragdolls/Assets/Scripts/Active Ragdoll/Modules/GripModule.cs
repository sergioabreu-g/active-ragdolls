using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    public class GripModule : Module {
        [Serializable] public struct Config {
            [Tooltip("What's the minimum weight the arm IK should have over the whole " +
                    "animation to activate the grip")]
            public float leftArmWeightThreshold, rightArmWeightThreshold;

            [Tooltip("Whether to only use Colliders marked as triggers to detect grip collisions.")]
            public bool onlyUseTriggers;
            public bool canGripYourself;
        }
        private Config _config;

        private Gripper _leftGrip, _rightGrip;



        private void Start() {
            var leftHand = _activeRagdoll.GetPhysicalBone(HumanBodyBones.LeftHand).gameObject;
            var rightHand = _activeRagdoll.GetPhysicalBone(HumanBodyBones.RightHand).gameObject;

            _leftGrip = leftHand.AddComponent<Gripper>();
            _rightGrip = rightHand.AddComponent<Gripper>();

            _leftGrip.Init(_activeRagdoll, _config.onlyUseTriggers, _config.canGripYourself);
            _rightGrip.Init(_activeRagdoll, _config.onlyUseTriggers, _config.canGripYourself);
        }

        override public void ConfigChanged(in ActiveRagdollConfig state) {
            _config = state.gripModuleConfig;
        }


        public void InputLeftArm(float weight) {
            _leftGrip.enabled = weight > _config.leftArmWeightThreshold;
        }

        public void InputRightArm(float weight) {
            _rightGrip.enabled = weight > _config.rightArmWeightThreshold;
        }
    }
} // namespace ActiveRagdoll