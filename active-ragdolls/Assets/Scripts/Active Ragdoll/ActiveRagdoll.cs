using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Original author: Sergio Abreu García | https://sergioabreu.me

    public class ActiveRagdoll : MonoBehaviour {
        // MODULES
        private List<Module> _modules;

        [Header("General")]
        public bool _debugMode = true;

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



        void Awake() {
#if !UNITY_EDITOR
            _debugMode = false;
#endif

            // Get & Init all the Modules
            _modules = new List<Module>();
            GetComponents<Module>(_modules);

            foreach (Module module in _modules)
                module.Initialize(this);

            if (_debugMode)
                Debug.Log(_modules.Count + " Modules Initialized.");
        }

        void Start() {
            _animatedBody = _animatedTorso.GetComponentsInChildren<Transform>();
            _joints = _physicalTorso.GetComponentsInChildren<ConfigurableJoint>();
            _initialJointsRotation = new Quaternion[_joints.Length];

            var tempAnimators = GetComponentsInChildren<Animator>();
            _animatedAnimator = tempAnimators[0];
            _physicalAnimator = tempAnimators[1];

            // Save the initial rotation of the joints, necessary for later matching
            // the animated body
            for (int i = 0; i < _joints.Length; i++) {
                _initialJointsRotation[i] = _joints[i].transform.localRotation;
            }
        }
        
        void FixedUpdate() {
            // Make the physical body match the animated one
            for (int i = 0; i < _joints.Length; i++) {
                ConfigurableJointExtensions.SetTargetRotationLocal(_joints[i], _animatedBody[i + 1].localRotation, _initialJointsRotation[i]);
            }
        }



        // ------------------- GETTERS & SETTERS -------------------

        /// <summary>
        /// Gets the transform of the given ANIMATED BODY'S BONE
        /// </summary>
        /// <param name="bone">Bone you want the transform of</param>
        /// <returns>The transform of the given ANIMATED bone</returns>
        public Transform GetAnimatedBoneTransform(HumanBodyBones bone) {
            return _animatedAnimator.GetBoneTransform(bone);
        }

        /// <summary>
        /// Gets the transform of the given PHYSICAL BODY'S BONE
        /// </summary>
        /// <param name="bone">Bone you want the transform of</param>
        /// <returns>The transform of the given PHYSICAL bone</returns>
        public Transform GetPhysicalBoneTransform(HumanBodyBones bone) {
            return _physicalAnimator.GetBoneTransform(bone);
        }

        /// <summary>
        /// Gets the Rigidboy of the physical body's torso
        /// </summary>
        /// <returns>The Rigidboy of the physical body's torso</returns>
        public Rigidbody GetPhysicalTorso() {
            return _physicalTorso;
        }
    }
}