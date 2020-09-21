#pragma warning disable 649

#if UNITY_EDITOR
#define DEBUG_MODE
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    [RequireComponent(typeof(BalanceModule))]
    [RequireComponent(typeof(MovementModule))]
    [RequireComponent(typeof(InputModule))]
    [RequireComponent(typeof(CameraModule))]
    public class ActiveRagdoll : MonoBehaviour {
        // ----- GENERAL -----
        [Header("--- GENERAL ---")]
        [SerializeField] private ActiveRagdollConfig _config;

        [Header("   Advanced")]
        [Tooltip("To avoid overloading the physics engine, solver iterations are set higher only" +
                 "for the active ragdoll rigidbodies, instead of modifying the general physics configuration.")]
        [SerializeField] private int _solverIterations = 11;
        [SerializeField] private int _velSolverIterations = 11;

        private static uint _ID_COUNT = 0;
        /// <summary> The unique ID of this Active Ragdoll instance. </summary>
        private uint _id;

        private Camera _characterCamera;

        // ----- MODULES -----
        private List<Module> _modules;

        // ----- BODY -----
        [Header("--- BODY ---")]
        [SerializeField] private Transform _animatedTorso;
        [SerializeField] private Rigidbody _physicalTorso;

        private Transform[] _animatedBones;
        private ConfigurableJoint[] _joints;
        private Rigidbody[] _rigidbodies;

        // ----- ANIMATORS -----
        private Animator _animatedAnimator, _physicalAnimator;
        private AnimatorHelper _animatorHelper;



        void Awake() {
            _id = _ID_COUNT++;

            _animatedBones = _animatedTorso.GetComponentsInChildren<Transform>();
            _joints = _physicalTorso.GetComponentsInChildren<ConfigurableJoint>();
            _rigidbodies = _physicalTorso.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in _rigidbodies) {
                rb.solverIterations = _solverIterations;
                rb.solverVelocityIterations = _velSolverIterations;
            }

            var tempAnimators = GetComponentsInChildren<Animator>();
            _animatedAnimator = tempAnimators[0];
            _physicalAnimator = tempAnimators[1];

            _animatorHelper = _animatedAnimator.gameObject.AddComponent<AnimatorHelper>();

            // MODULES
            _modules = new List<Module>();
            GetComponents<Module>(_modules);

            foreach (Module module in _modules)
                module.Initialize(this);


            // Sets the configuration for each module
            SetConfig(_config);

#if DEBUG_MODE
            Debug.Log("Active Ragdoll: " + _modules.Count + " Modules Initialized.");
#endif
        }

        void FixedUpdate() {
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

            _animatedAnimator.transform.position = _physicalTorso.position + (_animatedAnimator.transform.position - _animatedTorso.position);
        }




        // ------------------- GETTERS & SETTERS -------------------

        public uint GetID() {
            return _id;
        }

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

        /// <summary>
        /// Gets the Rigidboy of the physical body's torso
        /// </summary>
        /// <returns>The Rigidboy of the physical body's torso</returns>
        public Rigidbody GetPhysicalTorso() {
            return _physicalTorso;
        }

        /// <summary>
        /// Gets the Transform of the animated body's torso
        /// </summary>
        /// <returns>The Transform of the animated body's torso</returns>
        public Transform GetAnimatedTorso() {
            return _animatedTorso;
        }

        public ConfigurableJoint[] GetJoints() {
            return _joints;
        }

        public Rigidbody[] GetRigidbodies() {
            return _rigidbodies;
        }

        /// <summary> Gets all the bone transforms of the ANIMATED BODY </summary>
        /// <returns>All the animated body's bones</returns>
        public Transform[] GetAnimatedBones() {
            return _animatedBones;
        }

        /// <summary> Gets the animator of the ANIMATED BODY </summary>
        /// <returns>The animator of the ANIMATED BODY</returns>
        public Animator GetAnimatedAnimator() {
            return _animatedAnimator;
        }

        /// <summary>  Gets the animator of the PHYSICAL BODY </summary>
        /// <returns>The animator of the PHYSICAL BODY</returns>
        public Animator GetPhysicalAnimator() {
            return _physicalAnimator;
        }

        public AnimatorHelper GetAnimatorHelper() {
            return _animatorHelper;
        }

        public ActiveRagdollConfig GetCurrentState() {
            return _config;
        }

        public string GetCurrentStateName() {
            return _config.name;
        }

        public void SetConfig(ActiveRagdollConfig config) {
            foreach (Module mod in _modules)
                mod.ConfigChanged(_config);
        }

        public ActiveRagdollConfig GetConfig() {
            return _config;
        }

        public Camera GetCharacterCamera() {
            if (_characterCamera == null)
                Debug.LogError("No camera has been assigned to this Active Ragdoll." +
                                "Maybe you're using a custom Camera and have forgotten to call 'ActiveRagdoll.SetCamera(yourCamera)'.");

            return _characterCamera;
        }

        public void SetCharacterCamera(Camera camera) {
            _characterCamera = camera;
        }
    }
} // namespace ActiveRagdoll