#pragma warning disable 649

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    [RequireComponent(typeof(InputModule))]
    public class ActiveRagdoll : MonoBehaviour {
        [Header("--- GENERAL ---")]
        [SerializeField] private int _solverIterations;
        [SerializeField] private int _velSolverIterations;
        public int SolverIterations { get { return _solverIterations; } }
        public int VelSolverIterations { get { return _velSolverIterations; } }

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

        /// <summary> The direction where this character should be going/aiming.
        /// Generally set by the CameraModule and used by the MovementModule.</summary>
        public Vector3 TargetDirection { get; set; }

        [Header("--- ANIMATORS ---")]
        [SerializeField] private Animator _animatedAnimator;
        [SerializeField] private Animator _physicalAnimator;
        public Animator AnimatedAnimator { get { return _animatedAnimator; }
                                           private set { _animatedAnimator = value; } }
        public AnimatorHelper AnimatorHelper { get; private set; }

        private void OnValidate() {
            // Automatically retrieve the necessary references
            var animators = GetComponentsInChildren<Animator>();
            if (_animatedAnimator == null) _animatedAnimator = animators[0];
            if (_physicalAnimator == null) _physicalAnimator = animators[1];

            if (_animatedTorso == null)
                _animatedTorso = _animatedAnimator.GetBoneTransform(HumanBodyBones.Hips);
            if (_physicalTorso == null)
                _physicalTorso = _physicalAnimator.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>();

            AnimatedBones = _animatedTorso.GetComponentsInChildren<Transform>();
            Joints = _physicalTorso.GetComponentsInChildren<ConfigurableJoint>();
            Rigidbodies = _physicalTorso.GetComponentsInChildren<Rigidbody>();
        }

        private void Awake() {
            ID = _ID_COUNT++;

            foreach (Rigidbody rb in Rigidbodies) {
                rb.solverIterations = _solverIterations;
                rb.solverVelocityIterations = _velSolverIterations;
            }

            AnimatorHelper = _animatedAnimator.gameObject.AddComponent<AnimatorHelper>();
            InputModule temp;
            if (TryGetComponent<InputModule>(out temp))
                Input = temp;
#if UNITY_EDITOR
            else
                Debug.LogError("InputModule could not be found. An ActiveRagdoll must always have" +
                                "a peer InputModule.");
#endif
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

            _animatedAnimator.transform.position =_physicalTorso.position
                                + (_animatedAnimator.transform.position - _animatedTorso.position);
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
    }
} // namespace ActiveRagdoll