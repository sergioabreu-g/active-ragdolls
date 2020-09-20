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
        [Header("Advanced")]
        [Tooltip("To avoid overloading the physics engine, solver iterations are set higher only" +
                 "for the active ragdoll rigidbodies, instead of modifying the general physics configuration.")]
        [SerializeField] private int _solverIterations = 11, _velSolverIterations = 11;

        private static uint _ID_COUNT = 0;
        /// <summary> The unique ID of this Active Ragdoll instance. </summary>
        private uint _id;

        // ----- STATES -----
        [Header("States")]
        [Tooltip("All the states this active ragdoll can switch between. The first of the list" +
                "will be the initial one.")]
        [SerializeField] private ActiveRagdollState[] _states;

        /// <summary> Dictionary with all the states for faster getters & setters. </summary>
        private Dictionary<string, ActiveRagdollState> _statesDictionary;
        private ActiveRagdollState _currentState;

        private Camera _characterCamera;

        // ----- MODULES -----
        private List<Module> _modules;

        // ----- BODY -----
        [Header("Body")]
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

            if (_states.Length <= 0)
                Debug.LogError("Active Ragdoll cannot work without any assigned states. Add at least one state in the inspector.");
            else {
                _statesDictionary = new Dictionary<string, ActiveRagdollState>();
                foreach (ActiveRagdollState state in _states)
                    _statesDictionary.Add(state.name, state);
            }

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


            // Sets the initial state
            SetCurrentState(_states[0].name);

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

        public ActiveRagdollState[] GetStates() {
            return _states;
        }

        public ActiveRagdollState GetState(string stateName) {
            if (_statesDictionary.ContainsKey(stateName))
                return _statesDictionary[stateName];

#if DEBUG_MODE
            Debug.LogWarning("State '" + stateName + "' not found when calling 'GetState()'. Returning null.");
#endif
            return null;
        }

        public ActiveRagdollState GetCurrentState() {
            return _currentState;
        }

        public string GetCurrentStateName() {
            return _currentState.name;
        }

        public void SetCurrentState(string stateName) {
            if (_statesDictionary.ContainsKey(stateName))
                _currentState = _statesDictionary[stateName];
            else {
#if DEBUG_MODE
                Debug.LogWarning("State '" + stateName + "' not found when calling 'SetState()'. Current state wasn't changed.");
#endif
                return;
            }

            foreach (Module mod in _modules)
                mod.StateChanged(_currentState);
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