using System;
using UnityEditor;
using UnityEngine;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    public class MovementModule : Module {
        [Header("--- BODY ---")]
        /// <summary> Required to set the target rotations of the joints </summary>
        private Quaternion[] _initialJointsRotation;
        private ConfigurableJoint[] _joints;
        private Transform[] _animatedBones;

        [Header("--- ANIMATION ---")]
        private AnimatorHelper _animatorHelper;

        [Header("--- MOVEMENT ---")]
        public bool _enableMovement = true;

        private Vector2 _movementInput;

        [Header("--- INVERSE KINEMATICS ---")]
        public bool _enableIK = true;

        [Tooltip("Those values define the rotation range in which the target direction influences the arm movement.")]
        public float minTargetDirAngle = - 30;
        [Tooltip("Those values define the rotation range in which the target direction influences the arm movement.")]
        public float maxTargetDirAngle = 60;

        [Space(10)]
        [Tooltip("The limits of the arms direction. How far down/up should they be able to point?")]
        public float minArmsAngle = -70;
        public float maxArmsAngle = 83;
        [Tooltip("The limits of the look direction. How far down/up should the character be able to look?")]
        public float minLookAngle = -50, maxLookAngle = 60;

        [Space(10)]
        [Tooltip("The vertical offset of the look direction in reference to the target direction.")]
        public float lookAngleOffset;
        [Tooltip("The vertical offset of the arms direction in reference to the target direction.")]
        public float armsAngleOffset;
        [Tooltip("Defines the orientation of the hands")]
        public float handsRotationOffset = 0;

        [Space(10)]
        [Tooltip("How far apart to place the arms")]
        public float armsHorizontalSeparation = 0.7f;

        [Tooltip("The distance from the body to the hands in relation to how high/low they are")]
        public AnimationCurve armsDistance;

        private Vector3 _armsDir, _lookDir;



        private void Start() {
            _joints = _activeRagdoll.Joints;
            _animatedBones = _activeRagdoll.AnimatedBones;
            _animatorHelper = _activeRagdoll.AnimatorHelper;

            _initialJointsRotation = new Quaternion[_joints.Length];
            for (int i = 0; i < _joints.Length; i++) {
                _initialJointsRotation[i] = _joints[i].transform.localRotation;
            }
        }

        void FixedUpdate() {
            UpdateJointTargets();
            UpdateIK();
            UpdateMovement();
        }

        /// <summary> Makes the physical bones match the rotation of the animated ones </summary>
        private void UpdateJointTargets() {
            for (int i = 0; i < _joints.Length; i++) {
                ConfigurableJointExtensions.SetTargetRotationLocal(_joints[i], _animatedBones[i + 1].localRotation, _initialJointsRotation[i]);
            }
        }

        private void UpdateIK() {
            if (!_enableIK) {
                _animatorHelper.LeftArmIKWeight = 0;
                _animatorHelper.RightArmIKWeight = 0;
                return;
            }

            // Get the necessary variables
            Vector3 targetDir = _activeRagdoll.TargetDirection;
            Vector3 animBodyForward = _activeRagdoll.AnimatedTorso.forward;
            Vector3 animBodyRight = _activeRagdoll.AnimatedTorso.right;
            Quaternion physBodyRot = _activeRagdoll.PhysicalTorso.rotation;

            // Reflect the direction when looking backwards, avoids neck-breaking twists
            bool lookingBackwards = Vector3.Angle(targetDir, animBodyForward) > 90;
            if (lookingBackwards) targetDir = Vector3.Reflect(targetDir, animBodyForward);

            Vector3 targetDir2D = Auxiliary.GetFloorProjection(targetDir);
            Vector3 chestPos = _activeRagdoll.GetAnimatedBone(HumanBodyBones.Spine).position;

            // Calculate the percentage of the direction vertical angle (how much it is looking up)
            float directionAngle = Vector3.Angle(targetDir, Vector3.up);
            directionAngle -= 90;
            float directionVerticalPercent = 1 - Mathf.Clamp01((directionAngle - minTargetDirAngle) / Mathf.Abs(maxTargetDirAngle - minTargetDirAngle));

            // Translate the previous percentage into the range of the look and arms vertical angle
            // This allows to use the target direction to direct the movement without having to use its
            // actual vertical direction, which usually leads to exaggerated head and arms movements.
            float lookVerticalAngle = directionVerticalPercent * Mathf.Abs(maxLookAngle - minLookAngle) + minLookAngle;
            float armsVerticalAngle = directionVerticalPercent * Mathf.Abs(maxArmsAngle - minArmsAngle) + minArmsAngle;
            lookVerticalAngle += lookAngleOffset;
            armsVerticalAngle += armsAngleOffset;

            _lookDir = Quaternion.AngleAxis(-lookVerticalAngle, animBodyRight) * targetDir2D;
            _armsDir = Quaternion.AngleAxis(-armsVerticalAngle, animBodyRight) * targetDir2D;

            // Now use the animation curve of the arms to see how much they should be extended
            // given its current vertical angle. This is useful to create more realistic movement,
            // since it allows to lock/unlock the elbows to recreate certain movement patterns.
            // e.g. lifting a weight overhead
            float currentArmsDistance = armsDistance.Evaluate(directionVerticalPercent);

            // Calculate the final targets and set them through the AnimatorHelper
            Vector3 lookPoint = _activeRagdoll.GetAnimatedBone(HumanBodyBones.Head).position + _lookDir;
            _animatorHelper.LookAtPoint(lookPoint);

            Vector3 armsMiddleTarget = chestPos + _armsDir * currentArmsDistance;
            Vector3 upRef = Vector3.Cross(_armsDir, animBodyRight).normalized;
            Vector3 armsHorizontalVec = Vector3.Cross(_armsDir, upRef).normalized;
            Quaternion handsRot = Quaternion.LookRotation(_armsDir, upRef);

            _animatorHelper.LeftHandTarget.position = armsMiddleTarget + armsHorizontalVec * armsHorizontalSeparation / 2;
            _animatorHelper.RightHandTarget.position = armsMiddleTarget - armsHorizontalVec * armsHorizontalSeparation / 2;
            _animatorHelper.LeftHandTarget.rotation = handsRot * Quaternion.Euler(0, 0, 45 + handsRotationOffset);
            _animatorHelper.RightHandTarget.rotation = handsRot * Quaternion.Euler(0, 0, -45 - handsRotationOffset);
        }

        /// <summary> Update the movement animation and rotation of the character </summary>
        private void UpdateMovement() {
            if (_movementInput == Vector2.zero || !_enableMovement) {
                PlayAnimation("Idle");
                return;
            }

            float angleOffset = Vector2.SignedAngle(_movementInput, Vector2.up);
            Vector3 targetForward = Quaternion.AngleAxis(angleOffset, Vector3.up) * Auxiliary.GetFloorProjection(_activeRagdoll.TargetDirection);
            _activeRagdoll.AnimatedAnimator.transform.rotation = Quaternion.LookRotation(targetForward, Vector3.up);

            PlayAnimation("Moving", _movementInput.magnitude);
        }

        /// <summary> Plays an animation using the animator. The speed doesn't change the actual
        /// speed of the animator, but a parameter of the same name that can be used to multiply
        /// the speed of certain animations. </summary>
        /// <param name="animation">The name of the animation state to be played</param>
        /// <param name="speed">The speed to be set</param>
        public void PlayAnimation(string animation, float speed = 1) {
            var animator = _activeRagdoll.AnimatedAnimator;

            animator.Play(animation);
            animator.SetFloat("speed", speed);
        }

        public void InputMove(Vector2 movement) {
            _movementInput = movement;
        }
        
        public void InputLeftArm(float weight) {
            if (!_enableIK)
                return;

            _animatorHelper.LeftArmIKWeight = weight;
        }

        public void InputRightArm(float weight) {
            if (!_enableIK)
                return;

            _animatorHelper.RightArmIKWeight = weight;
        }
    }
} // namespace ActiveRagdoll