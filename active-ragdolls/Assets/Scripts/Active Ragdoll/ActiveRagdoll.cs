using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    public class ActiveRagdoll : MonoBehaviour {
        [Header("Body")]
        [SerializeField] private Transform _animatedTorso;
        [SerializeField] private Rigidbody _physicalTorso;

        private Transform[] _animatedBody;
        private ConfigurableJoint[] _joints;
        private Quaternion[] _initialJointsRotation;

        [Header("Advanced")]
        [SerializeField] private int _solverIterations = 11;
        [SerializeField] private int _velSolverIterations = 11;

        void Start() {
            _animatedBody = _animatedTorso.GetComponentsInChildren<Transform>();
            _joints = _physicalTorso.GetComponentsInChildren<ConfigurableJoint>();

            _initialJointsRotation = new Quaternion[_joints.Length];
            for (int i = 0; i < _joints.Length; i++)
                _initialJointsRotation[i] = _joints[i].transform.localRotation;
        }

        void FixedUpdate() {
            for (int i = 0; i < _joints.Length; i++) {
                ConfigurableJointExtensions.SetTargetRotationLocal(_joints[i], _animatedBody[i + 1].localRotation, _initialJointsRotation[i]);
            }
        }
    }
}