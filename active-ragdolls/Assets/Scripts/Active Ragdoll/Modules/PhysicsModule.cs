#pragma warning disable 649

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    public class PhysicsModule : Module {
        // --- BALANCE ---

        public enum BALANCE_MODE {
            UPRIGHT_TORQUE,
            MANUAL_TORQUE,
            STABILIZER_JOINT,
            FREEZE_ROTATIONS,
            NONE,
        }

        [Header("--- GENERAL ---")]
        [SerializeField] private BALANCE_MODE _balanceMode = BALANCE_MODE.STABILIZER_JOINT;
        public BALANCE_MODE BalanceMode { get { return _balanceMode; } }
        public float customTorsoAngularDrag = 0.05f;

        [Header("--- UPRIGHT TORQUE ---")]
        public float uprightTorque = 10000;
        [Tooltip("Defines how much torque percent is applied given the inclination angle percent [0, 1]")]
        public AnimationCurve uprightTorqueFunction;
        public float rotationTorque = 500;

        [Header("--- MANUAL TORQUE ---")]
        public float manualTorque = 500;
        public float maxManualRotSpeed = 5;

        private Vector2 _torqueInput;

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

        [Header("--- FREEZE ROTATIONS ---")]
        [SerializeField] private float freezeRotationSpeed = 5;

        // --- ROTATION ---

        public Vector3 TargetDirection { get; set; }
        private Quaternion _targetRotation;



        private void Start() {
            UpdateTargetRotation();
            InitializeStabilizerJoint();
            StartBalance();
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

        private void FixedUpdate() {
            UpdateTargetRotation();
            ApplyCustomDrag();

            switch (_balanceMode) {
                case BALANCE_MODE.UPRIGHT_TORQUE:
                    var balancePercent = Vector3.Angle(_activeRagdoll.PhysicalTorso.transform.up,
                                                         Vector3.up) / 180;
                    balancePercent = uprightTorqueFunction.Evaluate(balancePercent);
                    var rot = Quaternion.FromToRotation(_activeRagdoll.PhysicalTorso.transform.up,
                                                         Vector3.up).normalized;

                    _activeRagdoll.PhysicalTorso.AddTorque(new Vector3(rot.x, rot.y, rot.z)
                                                                * uprightTorque * balancePercent);

                    var directionAnglePercent = Vector3.SignedAngle(_activeRagdoll.PhysicalTorso.transform.forward,
                                        TargetDirection, Vector3.up) / 180;
                    _activeRagdoll.PhysicalTorso.AddRelativeTorque(0, directionAnglePercent * rotationTorque, 0);
                    break;

                case BALANCE_MODE.FREEZE_ROTATIONS:
                    var smoothedRot = Quaternion.Lerp(_activeRagdoll.PhysicalTorso.rotation,
                                       _targetRotation, Time.fixedDeltaTime * freezeRotationSpeed);
                    _activeRagdoll.PhysicalTorso.MoveRotation(smoothedRot);

                    break;

                case BALANCE_MODE.STABILIZER_JOINT:
                    // Move stabilizer to player torso (useless, but improves clarity)
                    _stabilizerRigidbody.MovePosition(_activeRagdoll.PhysicalTorso.position);
                    _stabilizerRigidbody.MoveRotation(_targetRotation);

                    break;

                case BALANCE_MODE.MANUAL_TORQUE:
                    if (_activeRagdoll.PhysicalTorso.angularVelocity.magnitude < maxManualRotSpeed) {
                        var force = _torqueInput * manualTorque;
                        _activeRagdoll.PhysicalTorso.AddRelativeTorque(force.y, 0, force.x);
                    }

                    break;

                default: break;
            }
        }

        private void UpdateTargetRotation() {
            if (TargetDirection != Vector3.zero)
                _targetRotation = Quaternion.LookRotation(TargetDirection, Vector3.up);
            else
                _targetRotation = Quaternion.identity;
        }

        private void ApplyCustomDrag() {
            var angVel = _activeRagdoll.PhysicalTorso.angularVelocity;
            angVel -= (Mathf.Pow(angVel.magnitude, 2) * customTorsoAngularDrag) * angVel.normalized;
            _activeRagdoll.PhysicalTorso.angularVelocity = angVel;
        }

        public void SetBalanceMode(BALANCE_MODE balanceMode) {
            if (_balanceMode == balanceMode) {
#if UNITY_EDITOR
                Debug.LogWarning("SetBalanceMode was called but the mode selected was the same as " +
                                "the current one. No changes made."); ;
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
                case BALANCE_MODE.UPRIGHT_TORQUE:
                    break;

                case BALANCE_MODE.FREEZE_ROTATIONS:
                    _activeRagdoll.PhysicalTorso.constraints = RigidbodyConstraints.FreezeRotation;
                    break;

                case BALANCE_MODE.STABILIZER_JOINT:
                    var jointDrive = (JointDrive) _stabilizerJointDrive;
                    _stabilizerJoint.angularXDrive = _stabilizerJoint.angularYZDrive = jointDrive;
                    break;

                case BALANCE_MODE.MANUAL_TORQUE:
                    break;

                default: break;
            }
        }

        /// <summary> Cleans up everything that was being used for the current balance mode. </summary>
        private void StopBalance() {
            switch (_balanceMode) {
                case BALANCE_MODE.UPRIGHT_TORQUE:
                    break;

                case BALANCE_MODE.FREEZE_ROTATIONS:
                    _activeRagdoll.PhysicalTorso.constraints = 0;
                    break;

                case BALANCE_MODE.STABILIZER_JOINT:
                    var jointDrive = (JointDrive) JointDriveConfig.ZERO;
                    _stabilizerJoint.angularXDrive = _stabilizerJoint.angularYZDrive = jointDrive;
                    break;

                case BALANCE_MODE.MANUAL_TORQUE:
                    break;

                default: break;
            }
        }

        public void ManualTorqueInput(Vector2 torqueInput) {
            _torqueInput = torqueInput;
        }
    }
} // namespace ActiveRagdoll