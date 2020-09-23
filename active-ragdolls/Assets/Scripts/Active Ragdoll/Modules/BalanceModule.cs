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
        public BALANCE_MODE BalanceMode { get { return _balanceMode; }
                                          set { Debug.Log("UNFINISHED WORK HERE"); } }

        [Header("--- STABILIZER JOINT ---")]
        [SerializeField] private JointDriveConfig _stabilizerJointDrive;
        public JointDriveConfig StabilizerJointDrive {
            get { return _stabilizerJointDrive; }
            set { Debug.Log("UNFINISHED WORK HERE"); }
        }

        private GameObject _stabilizerGameobject;
        private Rigidbody _stabilizerRigidbody;

        [Header("--- MANUAL ---")]
        public float torque = 500;

        private Vector2 _torqueInput;



        private void Start() {
            InitBalance();
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



        /// <summary> Initilizes depending on the balance mode selected </summary>
        private void InitBalance() {
            switch (_balanceMode) {
                case BALANCE_MODE.FREEZE_ROTATIONS:
                    _activeRagdoll.PhysicalTorso.constraints =
                    RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                    break;

                case BALANCE_MODE.STABILIZER_JOINT:
                    InitializeStabilizerJoint();
                    break;

                case BALANCE_MODE.MANUAL:
                    break;

                default: break;
            }
        }

        /// <summary> Cleans up everything that was being used for balance. </summary>
        private void StopBalance() {
            switch (_balanceMode) {
                case BALANCE_MODE.FREEZE_ROTATIONS:
                    _activeRagdoll.PhysicalTorso.constraints = 0;
                    break;

                case BALANCE_MODE.STABILIZER_JOINT:
                    Destroy(_stabilizerGameobject);
                    break;

                case BALANCE_MODE.MANUAL:
                    break;

                default: break;
            }
        }

        /// <summary> Creates the stabilizer GameObject with a Rigidbody and a ConfigurableJoint,
        /// and connects this last one to the torso. </summary>
        private void InitializeStabilizerJoint() {
            _stabilizerGameobject = new GameObject("Stabilizer", typeof(Rigidbody), typeof(ConfigurableJoint));
            _stabilizerGameobject.transform.parent = _activeRagdoll.PhysicalTorso.transform.parent;
            _stabilizerGameobject.transform.rotation = _activeRagdoll.PhysicalTorso.rotation;

            _stabilizerRigidbody = _stabilizerGameobject.GetComponent<Rigidbody>();
            _stabilizerRigidbody.isKinematic = true;

            var joint = _stabilizerGameobject.GetComponent<ConfigurableJoint>();
            joint.connectedBody = _activeRagdoll.PhysicalTorso;

            var jointDrive = new UnityEngine.JointDrive();
            jointDrive.positionSpring = _stabilizerJointDrive.positionSpring;
            jointDrive.positionDamper = _stabilizerJointDrive.positionDamper;
            jointDrive.maximumForce = _stabilizerJointDrive.maximumForce;

            joint.angularXDrive = jointDrive;
            joint.angularYZDrive = jointDrive;
        }

        public void InputMove(Vector2 manualStabilizationInput) {
            _torqueInput = manualStabilizationInput;
        }
    }
} // namespace ActiveRagdoll