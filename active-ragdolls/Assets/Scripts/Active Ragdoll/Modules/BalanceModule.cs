using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Original author: Sergio Abreu García | https://sergioabreu.me

    public class BalanceModule : Module {

        public enum BALANCE_MODE {
            FREEZE_ROTATIONS, // Freezes all but the Y axis rotations of the torso
            STABILIZER_JOINT, // Creates a stabilization rigidbody that pulls the torso towards
                              // a vertical position through a joint
            MANUAL_STABILIZATION, // It also uses a stabilization rigidbody, but it allows the user
                                  // to control its rotation, instead of freezing it into a vertical position
        }
        
        [Serializable] public struct Config {
            public BALANCE_MODE balanceMode;
            public JointDriveConfig stabilizerJointConfig;
        }
        private Config _config;

        private GameObject _stabilizerGameobject;
        private Rigidbody _stabilizerRigidbody;

        void FixedUpdate() {
            // Move the balancer to the players position (useless, just for debugging clarity)
            switch (_config.balanceMode) {
                case BALANCE_MODE.FREEZE_ROTATIONS:
                    _activeRagdoll.GetPhysicalTorso().MoveRotation(_activeRagdoll.GetAnimatedTorso().rotation);

                    break;

                case BALANCE_MODE.STABILIZER_JOINT:
                    _stabilizerRigidbody.MovePosition(_activeRagdoll.GetPhysicalTorso().position);
                    _stabilizerRigidbody.MoveRotation(_activeRagdoll.GetAnimatedTorso().rotation);
                    break;

                case BALANCE_MODE.MANUAL_STABILIZATION:
                    break;

                default: break;
            }
        }

        /// <summary>
        /// Initilizes whatever is necessary depending on the balance mode selected
        /// </summary>
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

        /// <summary>
        /// Removes and sets back to default everything that was being used for balance.
        /// </summary>
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

        /// <summary>
        /// Creates the stabilizer GameObject with a Rigidbody and a ConfigurableJoint, and connects this
        /// last one to the torso.
        /// </summary>
        private void InitializeStabilizerJoint() {
            // Instantiates and configures the stabilizer GameObject
            _stabilizerGameobject = new GameObject("Stabilizer", typeof(Rigidbody), typeof(ConfigurableJoint));
            _stabilizerGameobject.transform.parent = _activeRagdoll.GetPhysicalTorso().transform.parent;
            _stabilizerGameobject.transform.eulerAngles =
                new Vector3(0, _activeRagdoll.GetPhysicalTorso().transform.eulerAngles.y, 0);

            _stabilizerRigidbody = _stabilizerGameobject.GetComponent<Rigidbody>();
            _stabilizerRigidbody.isKinematic = true;

            var joint = _stabilizerGameobject.GetComponent<ConfigurableJoint>();
            joint.connectedBody = _activeRagdoll.GetPhysicalTorso();

            // Sets the ConfigurableJoint angular drives defined by the user
            var jointDrive = new UnityEngine.JointDrive();
            jointDrive.positionSpring = _config.stabilizerJointConfig.positionSpring;
            jointDrive.positionDamper = _config.stabilizerJointConfig.positionDamper;
            jointDrive.maximumForce = _config.stabilizerJointConfig.maximumForce;

            joint.angularXDrive = jointDrive;
            joint.angularYZDrive = jointDrive;
        }

        override public void StateChanged(in ActiveRagdollState state) {
            StopBalance();
            _config = state.balanceModuleConfig;
            InitBalance();
        }
    }
} // namespace ActiveRagdoll