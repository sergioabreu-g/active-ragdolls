using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Original author: Sergio Abreu García | https://sergioabreu.me

    public class MovementModule : Module {
        [Serializable] public struct Config {

        }
        private Config _config;

        // Body info storage
        private Quaternion[] _initialJointsRotation;
        private ConfigurableJoint[] _joints;
        private Transform[] _animatedBones;

        override protected void LateAwake() {
            _joints = _activeRagdoll.GetJoints();
            _animatedBones = _activeRagdoll.GetAnimatedBones();

            _initialJointsRotation = new Quaternion[_joints.Length];

            // Save the initial rotation of the joints, necessary for later matching
            // the animated body
            for (int i = 0; i < _joints.Length; i++) {
                _initialJointsRotation[i] = _joints[i].transform.localRotation;
            }
        }

        void FixedUpdate() {
            // Make the physical body match the animated one
            for (int i = 0; i < _joints.Length; i++) {
                ConfigurableJointExtensions.SetTargetRotationLocal(_joints[i], _animatedBones[i + 1].localRotation, _initialJointsRotation[i]);
            }
        }

        override public void StateChanged(in ActiveRagdollState state) {
            _config = state.movementModuleConfig;
        }
    }
}