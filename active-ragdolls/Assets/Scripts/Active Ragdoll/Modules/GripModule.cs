using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    public class GripModule : Module {
        [Tooltip("What's the minimum weight the arm IK should have over the whole " +
        "animation to activate the grip")]
        public float leftArmWeightThreshold = 0.5f, rightArmWeightThreshold = 0.5f;
        public JointMotionsConfig defaultMotionsConfig;

        [Tooltip("Whether to only use Colliders marked as triggers to detect grip collisions.")]
        public bool onlyUseTriggers = false;
        public bool canGripYourself = false;

        private Gripper _leftGrip, _rightGrip;


        private void Start() {
            var leftHand = _activeRagdoll.GetPhysicalBone(HumanBodyBones.LeftHand).gameObject;
            var rightHand = _activeRagdoll.GetPhysicalBone(HumanBodyBones.RightHand).gameObject;

            (_leftGrip = leftHand.AddComponent<Gripper>()).GripMod = this;
            (_rightGrip = rightHand.AddComponent<Gripper>()).GripMod = this;
        }


        public void UseLeftGrip(float weight) {
            _leftGrip.enabled = weight > leftArmWeightThreshold;
        }

        public void UseRightGrip(float weight) {
            _rightGrip.enabled = weight > rightArmWeightThreshold;
        }
    }
} // namespace ActiveRagdoll