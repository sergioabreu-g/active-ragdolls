using System;
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

        private Vector2 _movementInput;

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
            UpdateJointTargets();
            UpdateLookPoint();
            UpdateMovement();
        }

        override public void StateChanged(in ActiveRagdollState state) {
            _config = state.movementModuleConfig;
        }

        /// <summary>
        /// Makes the physical bones match the rotation of the animated ones
        /// </summary>
        private void UpdateJointTargets() {
            for (int i = 0; i < _joints.Length; i++) {
                ConfigurableJointExtensions.SetTargetRotationLocal(_joints[i], _animatedBones[i + 1].localRotation, _initialJointsRotation[i]);
            }
        }

        private void UpdateLookPoint() {
            // TEMPORAL
            Vector3 lookPoint = _activeRagdoll.GetAnimatedBone(HumanBodyBones.Head).position;
            lookPoint += _activeRagdoll.GetDirector().forward;

            _activeRagdoll.GetAnimatorHelper().LookAtPoint(lookPoint);
        }

        private void UpdateMovement() {
            if (_movementInput == Vector2.zero) {
                PlayAnimation("Idle");
                return;
            }

            float angleOffset = Vector2.SignedAngle(_movementInput, Vector2.up);
            Vector3 targetForward = Quaternion.AngleAxis(angleOffset, Vector3.up) * Auxiliary.GetFloorProjection(_activeRagdoll.GetDirector().forward);
            _activeRagdoll.GetAnimatedAnimator().transform.rotation = Quaternion.LookRotation(targetForward, Vector3.up);

            PlayAnimation("Moving", _movementInput.magnitude);
        }

        public void PlayAnimation(string animation, float speed = 1) {
            var animator = _activeRagdoll.GetAnimatedAnimator();

            animator.Play(animation);
            animator.SetFloat("speed", speed);
        }

        public void InputMove(Vector2 movement) {
            _movementInput = movement;
        }
    }
} // namespace ActiveRagdoll