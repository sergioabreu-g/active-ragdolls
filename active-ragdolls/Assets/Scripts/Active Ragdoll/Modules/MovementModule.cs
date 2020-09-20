using System;
using UnityEditor;
using UnityEngine;

namespace ActiveRagdoll {
    // Original author: Sergio Abreu García | https://sergioabreu.me

    public class MovementModule : Module {
        [Serializable] public struct Config {
            [Header("--- ARMS & HEAD IK ---")]
            [Tooltip("Those values define the rotation range in which the camera influences the arm movement.")]
            public float minCameraAngle;
            [Tooltip("Those values define the rotation range in which the camera influences the arm movement.")]
            public float maxCameraAngle;

            [Space(10)]
            [Tooltip("The limits of the arms direction. How far down/up should they be able to point?")]
            public float minArmsAngle, maxArmsAngle;
            [Tooltip("The limits of the look direction. How far down/up should the character be able to look?")]
            public float minLookAngle, maxLookAngle;

            [Space(10)]
            [Tooltip("The vertical offset of the look direction in reference to the camera direction.")]
            public float lookAngleOffset;
            [Tooltip("The vertical offset of the arms direction in reference to the camera direction.")]
            public float armsAngleOffset;

            [Space(10)]
            [Tooltip("Defines the orientation of the hands")]
            public float handsRotationOffset;

            [Tooltip("How far apart to place the arms")]
            public float armsSeparation;

            [Tooltip("The distance from the body to the hands in relation to how high/low they are")]
            public AnimationCurve armsDistance;
        }
        private Config _config;

        // ----- Body -----
        /// <summary> Required to set the target rotations of the joints </summary>
        private Quaternion[] _initialJointsRotation;
        private ConfigurableJoint[] _joints;
        private Transform[] _animatedBones;

        // ----- Animation -----
        private AnimatorHelper _animatorHelper;

        // ----- Movement -----
        private Vector2 _movementInput;

        // ----- IK -----
        private Vector3 _armsDir, _lookDir;



        override protected void LateAwake() {
            _joints = _activeRagdoll.GetJoints();
            _animatedBones = _activeRagdoll.GetAnimatedBones();
            _animatorHelper = _activeRagdoll.GetAnimatorHelper();

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

        override public void StateChanged(in ActiveRagdollState state) {
            _config = state.movementModuleConfig;
        }

        /// <summary> Makes the physical bones match the rotation of the animated ones </summary>
        private void UpdateJointTargets() {
            for (int i = 0; i < _joints.Length; i++) {
                ConfigurableJointExtensions.SetTargetRotationLocal(_joints[i], _animatedBones[i + 1].localRotation, _initialJointsRotation[i]);
            }
        }

        private void UpdateIK() {
            // Get the necessary variables
            Vector3 cameraDir = _activeRagdoll.GetCharacterCamera().transform.forward;
            Vector3 bodyForward = _activeRagdoll.GetAnimatedTorso().forward;
            Vector3 bodyRight = _activeRagdoll.GetAnimatedTorso().right;
            Quaternion bodyRot = _activeRagdoll.GetPhysicalTorso().rotation;

            // Reflect the direction when looking backwards, avoids neck-breaking twists
            bool lookingBackwards = Vector3.Angle(cameraDir, bodyForward) > 90;
            if (lookingBackwards) cameraDir = Vector3.Reflect(cameraDir, bodyForward);

            Vector3 cameraDir2D = Auxiliary.GetFloorProjection(cameraDir);
            Vector3 chestPos = _activeRagdoll.GetAnimatedBone(HumanBodyBones.Spine).position;

            // Calculate the percentage of the camera vertical angle (how much it is looking up)
            float cameraAngle = _activeRagdoll.GetCharacterCamera().transform.eulerAngles.x;
            if (cameraAngle > 180) cameraAngle = - (360 - cameraAngle);
            float camVerticalPercent = 1 - Mathf.Clamp01((cameraAngle - _config.minCameraAngle) / Mathf.Abs(_config.maxCameraAngle - _config.minCameraAngle));

            // Translate the previous percentage into the range of the look and arms vertical angle
            // This allows to use the camera to direct the movement without having to use its
            // actual vertical direction, which usually leads to exaggerated head and arms movements.
            float lookVerticalAngle = camVerticalPercent * Mathf.Abs(_config.maxLookAngle - _config.minLookAngle) + _config.minLookAngle;
            float armsVerticalAngle = camVerticalPercent * Mathf.Abs(_config.maxArmsAngle - _config.minArmsAngle) + _config.minArmsAngle;
            lookVerticalAngle += _config.lookAngleOffset;
            armsVerticalAngle += _config.armsAngleOffset;

            _lookDir = Quaternion.AngleAxis(-lookVerticalAngle, bodyRight) * cameraDir2D;
            _armsDir = Quaternion.AngleAxis(-armsVerticalAngle, bodyRight) * cameraDir2D;

            // Now use the animation curve of the arms to see how much they should be extended
            // given its current vertical angle. This is useful to create more realistic movement,
            // since it allows to lock/unlock the elbows to recreate certain movement patterns.
            // e.g. lifting a weight overhead
            float currentArmsDistance = _config.armsDistance.Evaluate(camVerticalPercent);

            // Calculate the final targets and set them through the AnimatorHelper
            Vector3 lookPoint = _activeRagdoll.GetAnimatedBone(HumanBodyBones.Head).position + _lookDir;
            _animatorHelper.LookAtPoint(lookPoint);

            Vector3 armsMiddleTarget = chestPos + _armsDir * currentArmsDistance;
            Vector3 upRef = Vector3.Cross(_armsDir, bodyRight).normalized;
            Vector3 armsHorizontalVec = Vector3.Cross(_armsDir, upRef).normalized;
            Quaternion handsRot = Quaternion.LookRotation(_armsDir, upRef);

            _animatorHelper.GetLeftHandTarget().position = armsMiddleTarget + armsHorizontalVec * _config.armsSeparation / 2;
            _animatorHelper.GetRightHandTarget().position = armsMiddleTarget - armsHorizontalVec * _config.armsSeparation / 2;
            _animatorHelper.GetLeftHandTarget().rotation = handsRot * Quaternion.Euler(0, 0, 45 + _config.handsRotationOffset);
            _animatorHelper.GetRightHandTarget().rotation = handsRot * Quaternion.Euler(0, 0, -45 - _config.handsRotationOffset);
        }

        /// <summary> Update the movement animation and rotation of the character </summary>
        private void UpdateMovement() {
            if (_movementInput == Vector2.zero) {
                PlayAnimation("Idle");
                return;
            }

            float angleOffset = Vector2.SignedAngle(_movementInput, Vector2.up);
            Vector3 targetForward = Quaternion.AngleAxis(angleOffset, Vector3.up) * Auxiliary.GetFloorProjection(_activeRagdoll.GetCharacterCamera().transform.forward);
            _activeRagdoll.GetAnimatedAnimator().transform.rotation = Quaternion.LookRotation(targetForward, Vector3.up);

            PlayAnimation("Moving", _movementInput.magnitude);
        }

        /// <summary> Plays an animation using the animator. The speed doesn't change the actual
        /// speed of the animator, but a parameter of the same name that can be used to multiply
        /// the speed of certain animations. </summary>
        /// <param name="animation">The name of the animation state to be played</param>
        /// <param name="speed">The speed to be set</param>
        public void PlayAnimation(string animation, float speed = 1) {
            var animator = _activeRagdoll.GetAnimatedAnimator();

            animator.Play(animation);
            animator.SetFloat("speed", speed);
        }

        public void InputMove(Vector2 movement) {
            _movementInput = movement;
        }
        
        public void InputLeftArm(float weight) {
            _animatorHelper.SetLeftArmIKWeight(weight);
        }

        public void InputRightArm(float weight) {
            _animatorHelper.SetRightArmIKWeight(weight);
        }
    }
} // namespace ActiveRagdoll