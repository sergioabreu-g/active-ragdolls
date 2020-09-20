using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Original author: Sergio Abreu García | https://sergioabreu.me

    /// <summary>
    /// Helper class that contains a lot of necessary functionality to control the animator,
    /// and more especifically the IK.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimatorHelper : MonoBehaviour {
        private Animator _animator;

        // ----- Look -----
        private enum LookMode {
            TARGET,
            POINT,
        }
        private LookMode _lookMode = LookMode.POINT;
        private Transform _lookTarget;
        private Vector3 _lookPoint = Vector3.zero;

        // ----- IK Targets -----
        private Transform _targetsParent;
        private Transform _leftHandTarget, _rightHandTarget, _leftHandHint, _rightHandHint;
        private Transform _leftFootTarget, _rightFootTarget;

        /// <summary> How much influence the IK will have over the animation </summary>
        private float _leftArmIKWeight = 0, _rightArmIKWeight = 0,
                      _leftLegIKWeight = 0, _rightLegIKWeight = 0;

        /// <summary> How much the chest IK will influence the animation at its maximum </summary>
        private float _chestMaxLookWeight = 0.5f;

        /// <summary> Smooths the transition between IK an animation to avoid snapping </summary>
        private bool _smoothIKTransition = true;
        private float _ikTransitionSpeed = 10;

        // Values used for animating the transition between animation & IK
        private float _currentLeftArmIKWeight = 0, _currentRightArmIKWeight = 0;
        private float _currentLeftLegIKWeight = 0, _currentRightLegIKWeight = 0;

        void Awake() {
            _animator = GetComponent<Animator>();

            _targetsParent = new GameObject("IK Targets").transform;
            _targetsParent.parent = transform.parent;

            _leftHandTarget = new GameObject("LeftHandTarget").transform;
            _rightHandTarget = new GameObject("RightHandTarget").transform;
            _leftHandTarget.parent = _targetsParent;
            _rightHandTarget.parent = _targetsParent;

            _leftHandHint = new GameObject("LeftHandHint").transform;
            _rightHandHint = new GameObject("RightHandHint").transform;
            _leftHandHint.parent = _leftHandTarget;
            _rightHandHint.parent = _rightHandTarget;
            _leftHandHint.Translate(new Vector3(-1, -1, 0), Space.Self);
            _rightHandHint.Translate(new Vector3(1, -1, 0), Space.Self);

            _leftFootTarget = new GameObject("LeftFootTarget").transform;
            _rightFootTarget = new GameObject("RightFootTarget").transform;
            _leftFootTarget.parent = _targetsParent;
            _rightFootTarget.parent = _targetsParent;
        }

        private void Update() {
            UpdateIKTransitions();
        }

        private void UpdateIKTransitions() {
            _currentLeftArmIKWeight = Mathf.Lerp(_currentLeftArmIKWeight, _leftArmIKWeight, Time.deltaTime * _ikTransitionSpeed);
            _currentLeftLegIKWeight = Mathf.Lerp(_currentLeftLegIKWeight, _leftLegIKWeight, Time.deltaTime * _ikTransitionSpeed);
            _currentRightArmIKWeight = Mathf.Lerp(_currentRightArmIKWeight, _rightArmIKWeight, Time.deltaTime * _ikTransitionSpeed);
            _currentRightLegIKWeight = Mathf.Lerp(_currentRightLegIKWeight, _rightLegIKWeight, Time.deltaTime * _ikTransitionSpeed);
        }

        private void OnAnimatorIK(int layerIndex) {
            // Look
            _animator.SetLookAtWeight(1, ((_leftArmIKWeight + _rightArmIKWeight) / 2) * _chestMaxLookWeight, 1, 1, 0);

            Vector3 lookPos = Vector3.zero;
            if (_lookMode == LookMode.TARGET) lookPos = _lookTarget.position;
            if (_lookMode == LookMode.POINT) lookPos = _lookPoint;

            _animator.SetLookAtPosition(lookPos);

            // Left arm
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, _currentLeftArmIKWeight);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, _currentLeftArmIKWeight);
            _animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, _leftArmIKWeight);

            _animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandTarget.position);
            _animator.SetIKRotation(AvatarIKGoal.LeftHand, _leftHandTarget.rotation);
            _animator.SetIKHintPosition(AvatarIKHint.LeftElbow, _leftHandHint.position);

            // Right arm
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, _currentRightArmIKWeight);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, _currentRightArmIKWeight);
            _animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, _rightArmIKWeight);

            _animator.SetIKPosition(AvatarIKGoal.RightHand, _rightHandTarget.position);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, _rightHandTarget.rotation);
            _animator.SetIKHintPosition(AvatarIKHint.RightElbow, _rightHandHint.position);

            // Left leg
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, _currentLeftLegIKWeight);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, _currentLeftLegIKWeight);

            _animator.SetIKPosition(AvatarIKGoal.LeftFoot, _leftFootTarget.position);
            _animator.SetIKRotation(AvatarIKGoal.LeftFoot, _leftFootTarget.rotation);

            // Right leg
            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, _currentRightLegIKWeight);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, _currentRightLegIKWeight);

            _animator.SetIKPosition(AvatarIKGoal.RightFoot, _rightFootTarget.position);
            _animator.SetIKRotation(AvatarIKGoal.RightFoot, _rightFootTarget.rotation);
        }

        // ------------------- GETTERS & SETTERS -------------------

        public Transform GetLookTarget() {
            return _lookTarget;
        }

        public Vector3 GetLookPoint() {
            return _lookPoint;
        }

        public void LookAtTarget(Transform target) {
            _lookTarget = target;
            _lookMode = LookMode.TARGET;
        }

        public void LookAtPoint(Vector3 point) {
            _lookPoint = point;
            _lookMode = LookMode.POINT;
        }

        public Transform GetLeftHandTarget() {
            return _leftHandTarget;
        }
        public Transform GetRightHandTarget() {
            return _rightHandTarget;
        }
        public Transform GetLeftFootTarget() {
            return _leftFootTarget;
        }
        public Transform GetRightFootTarget() {
            return _rightFootTarget;
        }

        public void SetLeftArmIKWeight(float weight) {
            _leftArmIKWeight = weight;
        }

        public void SetRightArmIKWeight(float weight) {
            _rightArmIKWeight = weight;
        }
        public void SetLeftLegIKWeight(float weight) {
            _leftLegIKWeight = weight;
        }

        public void SetRightLegIKWeight(float weight) {
            _rightLegIKWeight = weight;
        }

        public void SetChestMaxLookWeight(float chestMaxLookWeight) {
            _chestMaxLookWeight = chestMaxLookWeight;
        }

        public float GetLeftArmIKWeight() {
            return _leftArmIKWeight;
        }

        public float GetRightArmIKWeight() {
            return _rightArmIKWeight;
        }

        public float GetLeftLegIKWeight() {
            return _leftLegIKWeight;
        }

        public float GetRightLegIKWeight() {
            return _rightLegIKWeight;
        }

        public void SetSmoothIKTransition(bool smoothIKTransition) {
            _smoothIKTransition = smoothIKTransition;
        }

        public bool GetSmoothIKTransition() {
            return _smoothIKTransition;
        }

        public void SetIKTransitionSpeed(float speed) {
            _ikTransitionSpeed = speed;
        }

        public float GetIKTransitionDuration() {
            return _ikTransitionSpeed;
        }
    }
} // namespace ActiveRagdoll