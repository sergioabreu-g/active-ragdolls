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
        }
        
        [Serializable] public struct Config {
            public BALANCE_MODE balanceMode;
            public JointDriveConfig stabilizerJointConfig;
        }
        private Config _config;

        private GameObject _stabilizerGameobject;

        void Start() {

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

            var rigidbody = _stabilizerGameobject.GetComponent<Rigidbody>();
            rigidbody.isKinematic = true;

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
}