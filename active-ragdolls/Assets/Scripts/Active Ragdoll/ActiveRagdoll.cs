using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    public class ActiveRagdoll : MonoBehaviour {
        [Header("Body")]
        [SerializeField] private Transform _animatedTorso;
        [SerializeField] private Rigidbody _physicalTorso;

        // Storage for all the bodies information
        private Transform[] _animatedBody;
        private ConfigurableJoint[] _joints;
        private Quaternion[] _initialJointsRotation;

        // Animators of both bodies
        private Animator _animatedAnimator, _physicalAnimator;


        [Header("Advanced")]
        // To avoid overloading the physics engine, solver iterations are set higher only
        // for the active ragdoll rigidbodies, instead of modifying the general physics configuration.
        [SerializeField] private int _solverIterations = 11;
        [SerializeField] private int _velSolverIterations = 11;

        void Start() {
            _animatedBody = _animatedTorso.GetComponentsInChildren<Transform>();
            _joints = _physicalTorso.GetComponentsInChildren<ConfigurableJoint>();
            _initialJointsRotation = new Quaternion[_joints.Length];

            var tempAnimators = GetComponentsInChildren<Animator>();
            _animatedAnimator = tempAnimators[0];
            _physicalAnimator = tempAnimators[1];

            for (int i = 0; i < _joints.Length; i++) {
                _initialJointsRotation[i] = _joints[i].transform.localRotation;
            }
        }

        void FixedUpdate() {
            for (int i = 0; i < _joints.Length; i++) {
                ConfigurableJointExtensions.SetTargetRotationLocal(_joints[i], _animatedBody[i + 1].localRotation, _initialJointsRotation[i]);
            }
        }



        // ------------------- GETTERS & SETTERS -------------------

        /// <summary>
        /// Gets the transform of the given bone of the ANIMATED BODY
        /// </summary>
        /// <param name="bone">Bone you want the transform of</param>
        /// <returns>The transform of the given ANIMATED bone</returns>
        public Transform GetAnimatedBoneTransform(HumanBodyBones bone) {
            return _animatedAnimator.GetBoneTransform(bone);
        }

        /// <summary>
        /// Gets the transform of the given bone of the PHYSICAL BODY
        /// </summary>
        /// <param name="bone">Bone you want the transform of</param>
        /// <returns>The transform of the given PHYSICAL bone</returns>
        public Transform GetPhysicalBoneTransform(HumanBodyBones bone) {
            return _physicalAnimator.GetBoneTransform(bone);
        }
    }
}