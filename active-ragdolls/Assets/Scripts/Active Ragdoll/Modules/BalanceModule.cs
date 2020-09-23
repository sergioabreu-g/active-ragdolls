#pragma warning disable 649

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    public class BalanceModule : Module {
        public enum BALANCE_MODE {
            STABILIZER_JOINT,
            MANUAL,
            FREEZE_ROTATIONS,
        }

        [Header("--- GENERAL ---")]
        [SerializeField] private BALANCE_MODE _balanceMode = BALANCE_MODE.STABILIZER_JOINT;
        public BALANCE_MODE BalanceMode { get { return _balanceMode; } }

        [Header("--- STABILIZER JOINT ---")]
        [SerializeField] private JointDriveConfig _stabilizerJointDrive;
        public JointDriveConfig StabilizerJointDrive {
            get { return _stabilizerJointDrive; }
            set { if (_stabilizerJoint != null)
                    _stabilizerJoint.angularXDrive = _stabilizerJoint.angularXDrive = (JointDrive)value;
                }
        }

        private GameObject _stabilizerGameobject;
        private Rigidbody _stabilizerRigidbody;
        private ConfigurableJoint _stabilizerJoint;

        [Header("--- MANUAL ---")]
        public float torque = 500;

        private Vector2 _torqueInput;



        private void Start() {
            InitializeStabilizerJoint();
            StartBalance();

            _activeRagdoll.Input.OnMoveDelegates += MoveInput;
        }

        /// <summary> Creates the stabilizer GameObject with a Rigidbody and a ConfigurableJoint,
        /// and connects this last one to the torso. </summary>
        private void InitializeStabilizerJoint() {
            _stabilizerGameobject = new GameObject("Stabilizer", typeof(Rigidbody), typeof(ConfigurableJoint));
            _stabilizerGameobject.transform.parent = _activeRagdoll.PhysicalTorso.transform.parent;
            _stabilizerGameobject.transform.rotation = _activeRagdoll.PhysicalTorso.rotation;

            _stabilizerJoint = _stabilizerGameobject.GetComponent<ConfigurableJoint>();
            _stabilizerRigidbody = _stabilizerGameobject.GetComponent<Rigidbody>();
            _stabilizerRigidbody.isKinematic = true;

            var joint = _stabilizerGameobject.GetComponent<ConfigurableJoint>();
            joint.connectedBody = _activeRagdoll.PhysicalTorso;
        }

        void FixedUpdate() {
            switch (_balanceMode) {
                case BALANCE_MODE.FREEZE_ROTATIONS:
                    _activeRagdoll.AnimatedTorso.transform.rotation =
                                                _activeRagdoll.AnimatedTorso.rotation;
                    break;

                case BALANCE_MODE.STABILIZER_JOINT:
                    // Move stabilizer to player (useless, but improves clarity)
                    _stabilizerRigidbody.MovePosition(_activeRagdoll.PhysicalTorso.position);
                    _stabilizerRigidbody.MoveRotation(_activeRagdoll.AnimatedTorso.rotation);
                    break;

                case BALANCE_MODE.MANUAL:
                    var force = _torqueInput * torque;
                    _activeRagdoll.PhysicalTorso.AddRelativeTorque(force.y, 0, force.x);

                    break;

                default: break;
            }
        }

        public void SetBalanceMode(BALANCE_MODE balanceMode) {
            if (_balanceMode == balanceMode) {
#if UNITY_EDITOR
                Debug.LogWarning("SetBalanceMode was called but the mode selected was the same as the current one. No changes made."); ;
#endif
                return;
            }

            StopBalance();
            _balanceMode = balanceMode;
            StartBalance();
        }

        /// <summary> Starts to balance depending on the balance mode selected </summary>
        private void StartBalance() {
            switch (_balanceMode) {
                case BALANCE_MODE.FREEZE_ROTATIONS:
                    _activeRagdoll.PhysicalTorso.constraints =
                    RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                    break;

                case BALANCE_MODE.STABILIZER_JOINT:
                    var jointDrive = (JointDrive) _stabilizerJointDrive;
                    _stabilizerJoint.angularXDrive = _stabilizerJoint.angularYZDrive = jointDrive;
                    break;

                case BALANCE_MODE.MANUAL:
                    break;

                default: break;
            }
        }

        /// <summary> Cleans up everything that was being used for the current balance mode. </summary>
        private void StopBalance() {
            switch (_balanceMode) {
                case BALANCE_MODE.FREEZE_ROTATIONS:
                    _activeRagdoll.PhysicalTorso.constraints = 0;
                    break;

                case BALANCE_MODE.STABILIZER_JOINT:
                    var jointDrive = (JointDrive) JointDriveConfig.ZERO;
                    _stabilizerJoint.angularXDrive = _stabilizerJoint.angularYZDrive = jointDrive;
                    break;

                case BALANCE_MODE.MANUAL:
                    break;

                default: break;
            }
        }

        public void MoveInput(Vector2 manualStabilizationInput) {
            _torqueInput = manualStabilizationInput;
        }
    }
} // namespace ActiveRagdoll