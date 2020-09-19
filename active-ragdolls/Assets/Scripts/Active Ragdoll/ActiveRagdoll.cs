#if UNITY_EDITOR
#define DEBUG_MODE
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Original author: Sergio Abreu García | https://sergioabreu.me

    [RequireComponent(typeof(BalanceModule))]
    [RequireComponent(typeof(MovementModule))]
    [RequireComponent(typeof(InputModule))]
    public class ActiveRagdoll : MonoBehaviour {
        // MODULES
        private List<Module> _modules;

        [Header("General")]
        [SerializeField] private ActiveRagdollState[] _states;
        [Tooltip("Its forward vector defines the direction of the movement and look of the Active Ragdoll." +
         "It's usually his camera, but it can be set to any transform.")]
        [SerializeField] private Transform _director;
        private ActiveRagdollState _currentState;

        // For faster getting & setting methods for the states
        private Dictionary<string, ActiveRagdollState> _statesDictionary;

        [Header("Body")]
        [SerializeField] private Transform _animatedTorso;
        [SerializeField] private Rigidbody _physicalTorso;

        // Body info storage
        private Transform[] _animatedBones;
        private ConfigurableJoint[] _joints;

        // Animators of both bodies
        private Animator _animatedAnimator, _physicalAnimator;
        private AnimatorHelper _animatorHelper;

        [Header("Advanced")]
        [Tooltip("To avoid overloading the physics engine, solver iterations are set higher only" +
                 "for the active ragdoll rigidbodies, instead of modifying the general physics configuration.")]
        [SerializeField] private int _solverIterations = 11, _velSolverIterations = 11;

        void Awake() {
            if (_states.Length <= 0)
                Debug.LogError("Active Ragdoll cannot work without any assigned states. Add at least one state in the inspector.");
            else {
                _statesDictionary = new Dictionary<string, ActiveRagdollState>();
                foreach (ActiveRagdollState state in _states)
                    _statesDictionary.Add(state.name, state);
            }

            _animatedBones = _animatedTorso.GetComponentsInChildren<Transform>();
            _joints = _physicalTorso.GetComponentsInChildren<ConfigurableJoint>();

            var tempAnimators = GetComponentsInChildren<Animator>();
            _animatedAnimator = tempAnimators[0];
            _physicalAnimator = tempAnimators[1];

            _animatorHelper = _animatedAnimator.gameObject.AddComponent<AnimatorHelper>();

            // Get & Init all the Modules
            _modules = new List<Module>();
            GetComponents<Module>(_modules);

            foreach (Module module in _modules)
                module.Initialize(this);


            // Set the initial state
            SetState(_states[0].name);

#if DEBUG_MODE
            Debug.Log("Active Ragdoll: " + _modules.Count + " Modules Initialized.");
#endif
        }

        void FixedUpdate() {
            SyncAnimatedBody();
        }

        /// <summary>
        /// Updates the rotation and position of the animated body
        /// to match the ones of the physical, so the IK movements
        /// are in calculated in synchrony. e.g. when looking at something,
        /// if the animated and physical bodies are not in the same spot and
        /// with the same orientation, the head movement calculated by the IK
        /// for the animated body won't match the movement the physical body
        /// needs to look at the same thing.
        /// </summary>
        private void SyncAnimatedBody() {
            _animatedAnimator.transform.position = _physicalTorso.position + (_animatedAnimator.transform.position - _animatedTorso.position);
        }




        // ------------------- GETTERS & SETTERS -------------------

        /// <summary>
        /// Gets the transform of the given ANIMATED BODY'S BONE
        /// </summary>
        /// <param name="bone">Bone you want the transform of</param>
        /// <returns>The transform of the given ANIMATED bone</returns>
        public Transform GetAnimatedBone(HumanBodyBones bone) {
            return _animatedAnimator.GetBoneTransform(bone);
        }

        /// <summary>
        /// Gets the transform of the given PHYSICAL BODY'S BONE
        /// </summary>
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

        /// <summary>
        /// Gets all the physical body's joints
        /// </summary>
        /// <returns>All the physical body's joints</returns>
        public ConfigurableJoint[] GetJoints() {
            return _joints;
        }

        /// <summary>
        /// Gets all the animated body's bones
        /// </summary>
        /// <returns>All the animated body's bones</returns>
        public Transform[] GetAnimatedBones() {
            return _animatedBones;
        }

        /// <summary>
        /// Gets the animator of the ANIMATED BODY
        /// </summary>
        /// <returns>The animator of the ANIMATED BODY</returns>
        public Animator GetAnimatedAnimator() {
            return _animatedAnimator;
        }

        /// <summary>
        /// Gets the animator of the PHYSICAL BODY
        /// </summary>
        /// <returns>The animator of the PHYSICAL BODY</returns>
        public Animator GetPhysicalAnimator() {
            return _physicalAnimator;
        }

        /// <summary>
        /// Gets the Animator Helper
        /// </summary>
        /// <returns>The Animator Helper</returns>
        public AnimatorHelper GetAnimatorHelper() {
            return _animatorHelper;
        }

        /// <summary>
        /// Gets all the states assigned to this Active Ragdoll
        /// </summary>
        /// <returns>All the states assigned to this Active Ragdoll</returns>
        public ActiveRagdollState[] GetStates() {
            return _states;
        }

        /// <summary>
        /// Gets all the states assigned to this Active Ragdoll
        /// </summary>
        /// <returns>All the states assigned to this Active Ragdoll</returns>
        public ActiveRagdollState GetState(string stateName) {
            if (_statesDictionary.ContainsKey(stateName))
                return _statesDictionary[stateName];

#if DEBUG_MODE
            Debug.LogWarning("State '" + stateName + "' not found when calling 'GetState()'. Returning null.");
#endif
            return null;
        }

        /// <summary>
        /// Gets the currently active state for this Active Ragdoll
        /// </summary>
        /// <returns>The currently active state for this Active Ragdoll</returns>
        public ActiveRagdollState GetCurrentState() {
            return _currentState;
        }

        /// <summary>
        /// Gets the name of the currently active state for this Active Ragdoll
        /// </summary>
        /// <returns>The name of the currently active state for this Active Ragdoll</returns>
        public string GetCurrentStateName() {
            return _currentState.name;
        }

        /// <summary>
        /// Sets the given state as the current/active one
        /// </summary>
        public void SetState(string stateName) {
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

        /// <summary>
        /// Gets the director transform, whose forward vector is used to define the
        /// movement and look direction.
        /// </summary>
        /// <returns>The director transform</returns>
        public Transform GetDirector() {
            return _director;
        }
    }
} // namespace ActiveRagdoll