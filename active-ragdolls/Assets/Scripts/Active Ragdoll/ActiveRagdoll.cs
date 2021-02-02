#pragma warning disable 649

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    [RequireComponent(typeof(InputModule))]
    public class ActiveRagdoll : MonoBehaviour {
        [Header("--- GENERAL ---")]
        [SerializeField] private int _solverIterations = 12;
        [SerializeField] private int _velSolverIterations = 4;
        [SerializeField] private float _maxAngularVelocity = 50;
        public int SolverIterations { get { return _solverIterations; } }
        public int VelSolverIterations { get { return _velSolverIterations; } }
        public float MaxAngularVelocity { get { return _maxAngularVelocity; } }

        public InputModule Input { get; private set; }

        /// <summary> The unique ID of this Active Ragdoll instance. </summary>
        public uint ID { get; private set; }
        private static uint _ID_COUNT = 0;

        [Header("--- BODY ---")]
        [SerializeField] private Transform _animatedTorso;
        [SerializeField] private Rigidbody _physicalTorso;
        public Transform AnimatedTorso { get { return _animatedTorso; } }
        public Rigidbody PhysicalTorso { get { return _physicalTorso; } }


        public Transform[] AnimatedBones { get; private set; }
        public ConfigurableJoint[] Joints { get; private set; }
        public Rigidbody[] Rigidbodies { get; private set; }

        [SerializeField] private List<BodyPart> _bodyParts;
        public List<BodyPart> BodyParts { get { return _bodyParts; } }

        [Header("--- ANIMATORS ---")]
        [SerializeField] private Animator _animatedAnimator;
        [SerializeField] private Animator _physicalAnimator;
        public Animator AnimatedAnimator {
            get { return _animatedAnimator; }
            private set { _animatedAnimator = value; }
        }

        public AnimatorHelper AnimatorHelper { get; private set; }
        /// <summary> Whether to constantly set the rotation of the Animated Body to the Physical Body's.</summary>
        public bool SyncTorsoPositions { get; set; } = true;
        public bool SyncTorsoRotations { get; set; } = true;

        private void OnValidate() {
            // Automatically retrieve the necessary references
            var animators = GetComponentsInChildren<Animator>();
            if (animators.Length >= 2)
            {
                if (_animatedAnimator == null) _animatedAnimator = animators[0];
                if (_physicalAnimator == null) _physicalAnimator = animators[1];

                if (_animatedTorso == null)
                    _animatedTorso = _animatedAnimator.GetBoneTransform(HumanBodyBones.Hips);
                if (_physicalTorso == null)
                    _physicalTorso = _physicalAnimator.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>();
            }

            if (_bodyParts.Count == 0)
                GetDefaultBodyParts();
        }

        private void Awake() {
            ID = _ID_COUNT++;

            if (AnimatedBones == null) AnimatedBones = _animatedTorso?.GetComponentsInChildren<Transform>();
            if (Joints == null) Joints = _physicalTorso?.GetComponentsInChildren<ConfigurableJoint>();
            if (Rigidbodies == null) Rigidbodies = _physicalTorso?.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in Rigidbodies) {
                rb.solverIterations = _solverIterations;
                rb.solverVelocityIterations = _velSolverIterations;
                rb.maxAngularVelocity = _maxAngularVelocity;
            }

            foreach (BodyPart bodyPart in _bodyParts)
                bodyPart.Init();

            AnimatorHelper = _animatedAnimator.gameObject.AddComponent<AnimatorHelper>();
            if (TryGetComponent(out InputModule temp))
                Input = temp;
#if UNITY_EDITOR
            else
                Debug.LogError("InputModule could not be found. An ActiveRagdoll must always have" +
                                "a peer InputModule.");
#endif
        }

        private void GetDefaultBodyParts() {
            _bodyParts.Add(new BodyPart("Head Neck",
                TryGetJoints(HumanBodyBones.Head, HumanBodyBones.Neck)));
            _bodyParts.Add(new BodyPart("Torso",
               TryGetJoints(HumanBodyBones.Spine, HumanBodyBones.Chest, HumanBodyBones.UpperChest)));
            _bodyParts.Add(new BodyPart("Left Arm",
                TryGetJoints(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand)));
            _bodyParts.Add(new BodyPart("Right Arm",
                TryGetJoints(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand)));
            _bodyParts.Add(new BodyPart("Left Leg",
                TryGetJoints(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot)));
            _bodyParts.Add(new BodyPart("Right Leg",
                TryGetJoints(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot)));
        }

        private List<ConfigurableJoint> TryGetJoints(params HumanBodyBones[] bones) {
            List<ConfigurableJoint> jointList = new List<ConfigurableJoint>();
            foreach (HumanBodyBones bone in bones) {
                Transform boneTransform = _physicalAnimator.GetBoneTransform(bone);
                if (boneTransform != null && (boneTransform.TryGetComponent(out ConfigurableJoint joint)))
                    jointList.Add(joint);
            }

            return jointList;
        }

        private void FixedUpdate() {
            SyncAnimatedBody();
        }

        /// <summary> Updates the rotation and position of the animated body's root
        /// to match the ones of the physical.</summary>
        private void SyncAnimatedBody() {
            // This is needed for the IK movements to be synchronized between
            // the animated and physical bodies. e.g. when looking at something,
            // if the animated and physical bodies are not in the same spot and
            // with the same orientation, the head movement calculated by the IK
            // for the animated body will be different from the one the physical body
            // needs to look at the same thing, so they will look at totally different places.
            if (SyncTorsoPositions)
                _animatedAnimator.transform.position = _physicalTorso.position
                                + (_animatedAnimator.transform.position - _animatedTorso.position);
            if (SyncTorsoRotations)
                _animatedAnimator.transform.rotation = _physicalTorso.rotation;
        }


        // ------------------- GETTERS & SETTERS -------------------

        /// <summary> Gets the transform of the given ANIMATED BODY'S BONE </summary>
        /// <param name="bone">Bone you want the transform of</param>
        /// <returns>The transform of the given ANIMATED bone</returns>
        public Transform GetAnimatedBone(HumanBodyBones bone) {
            return _animatedAnimator.GetBoneTransform(bone);
        }

        /// <summary> Gets the transform of the given PHYSICAL BODY'S BONE </summary>
        /// <param name="bone">Bone you want the transform of</param>
        /// <returns>The transform of the given PHYSICAL bone</returns>
        public Transform GetPhysicalBone(HumanBodyBones bone) {
            return _physicalAnimator.GetBoneTransform(bone);
        }

        public BodyPart GetBodyPart(string name) {
            foreach (BodyPart bodyPart in _bodyParts)
                if (bodyPart.bodyPartName == name) return bodyPart;

            return null;
        }

        public void SetStrengthScaleForAllBodyParts (float scale) {
            foreach (BodyPart bodyPart in _bodyParts)
                bodyPart.SetStrengthScale(scale);
        }
    }
} // namespace ActiveRagdoll