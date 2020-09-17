using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActiveRagdoll {
    // Original author: Sergio Abreu García | https://sergioabreu.me

    public class BalanceModule : Module {

        private enum BALANCE_MODE {
            FREEZE_ROTATIONS, // Freezes all but the Y axis rotations of the torso
            STABILIZER_JOINT, // Creates a stabilization rigidbody that pulls the torso towards
                              // a vertical position through a joint
        }

        [SerializeField] private BALANCE_MODE _balanceMode;

        [SerializeField] private StabilizerJointConfig _stabilizerJointConfig;
        private GameObject _stabilizerGameobject;

        void Start() {
            // Initialize the balancer depending on the selected mode
            switch (_balanceMode) {
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
            var jointDrive = new JointDrive();
            jointDrive.positionSpring = _stabilizerJointConfig.positionSpring;
            jointDrive.positionDamper = _stabilizerJointConfig.positionDamper;
            jointDrive.maximumForce = _stabilizerJointConfig.maximumForce;

            joint.angularXDrive = jointDrive;
            joint.angularYZDrive = jointDrive;
        }
    }
}