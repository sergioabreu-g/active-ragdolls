using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    public class BalanceModule : Module {
        public enum BALANCE_MODE {
            STABILIZER_JOINT,
            FREEZE_ROTATIONS,
            MANUAL_STABILIZATION,
        }

        [Serializable]
        public struct Config {
            [Tooltip("FREEZE ROTATIONS: Freezes torso rotations over all of the axis except for the Y.\n\n" +
                    "STABILIZER JOINT: Creates a stabilization rigidbody that pulls the torso towards " +
                    "a vertical position through a joint.\n\n" +
                    "MANUAL_STABILIZATION: same as the stabilizer joint, but the stabilizer body" +
                    "rotation is controlled by the user instead of always adopting a vertical orientation.")]
            public BALANCE_MODE balanceMode;
            public JointDriveConfig stabilizerJointConfig;
        }
        private Config _config;

        // ----- STABILIZER JOINT -----
        private GameObject _stabilizerGameobject;
        private Rigidbody _stabilizerRigidbody;



        /// <summary> Initilizes depending on the balance mode selected </summary>
        private void InitBalance() {
            switch (_config.balanceMode) {
                case BALANCE_MODE.FREEZE_ROTATIONS:
                    _activeRagdoll.GetPhysicalTorso().constraints =
                    RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                    break;

                case BALANCE_MODE.STABILIZER_JOINT:
                    InitializeStabilizerJoint();
                    break;

                case BALANCE_MODE.MANUAL_STABILIZATION:
                    break;

                default: break;
            }
        }

        /// <summary> Cleans up everything that was being used for balance. </summary>
        private void StopBalance() {
            switch (_config.balanceMode) {
                case BALANCE_MODE.FREEZE_ROTATIONS:
                    _activeRagdoll.GetPhysicalTorso().constraints = 0;
                    break;

                case BALANCE_MODE.STABILIZER_JOINT:
                    Destroy(_stabilizerGameobject);
                    break;

                case BALANCE_MODE.MANUAL_STABILIZATION:
                    break;

                default: break;
            }
        }

        /// <summary> Creates the stabilizer GameObject with a Rigidbody and a ConfigurableJoint,
        /// and connects this last one to the torso. </summary>
        private void InitializeStabilizerJoint() {
            _stabilizerGameobject = new GameObject("Stabilizer", typeof(Rigidbody), typeof(ConfigurableJoint));
            _stabilizerGameobject.transform.parent = _activeRagdoll.GetPhysicalTorso().transform.parent;
            _stabilizerGameobject.transform.eulerAngles =
                new Vector3(0, _activeRagdoll.GetPhysicalTorso().transform.eulerAngles.y, 0);

            _stabilizerRigidbody = _stabilizerGameobject.GetComponent<Rigidbody>();
            _stabilizerRigidbody.isKinematic = true;

            var joint = _stabilizerGameobject.GetComponent<ConfigurableJoint>();
            joint.connectedBody = _activeRagdoll.GetPhysicalTorso();

            var jointDrive = new UnityEngine.JointDrive();
            jointDrive.positionSpring = _config.stabilizerJointConfig.positionSpring;
            jointDrive.positionDamper = _config.stabilizerJointConfig.positionDamper;
            jointDrive.maximumForce = _config.stabilizerJointConfig.maximumForce;

            joint.angularXDrive = jointDrive;
            joint.angularYZDrive = jointDrive;
        }

        void FixedUpdate() {
            switch (_config.balanceMode) {
                case BALANCE_MODE.FREEZE_ROTATIONS:
                    _activeRagdoll.GetPhysicalTorso().transform.rotation =
                                                _activeRagdoll.GetAnimatedTorso().rotation;
                    break;

                case BALANCE_MODE.STABILIZER_JOINT:
                    // Move stabilizer to player (useless, but improves clarity)
                    _stabilizerRigidbody.MovePosition(_activeRagdoll.GetPhysicalTorso().position);
                    _stabilizerRigidbody.MoveRotation(_activeRagdoll.GetAnimatedTorso().rotation);
                    break;

                case BALANCE_MODE.MANUAL_STABILIZATION:
                    break;

                default: break;
            }
        }

        override public void ConfigChanged(in ActiveRagdollConfig state) {
            // Reset the balance to match the new state requirements
            StopBalance();
            _config = state.balanceModuleConfig;
            InitBalance();
        }
    }
} // namespace ActiveRagdoll