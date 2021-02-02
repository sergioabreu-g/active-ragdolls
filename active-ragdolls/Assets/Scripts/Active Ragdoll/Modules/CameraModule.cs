#pragma warning disable 649

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveRagdoll {
    // Author: Sergio Abreu García | https://sergioabreu.me

    public class CameraModule : Module {
        [Header("--- GENERAL ---")]
        [Tooltip("Where the camera should point to. Head by default.")]
        public Transform _lookPoint;

        public float lookSensitivity = 1;
        public float scrollSensitivity = 1;
        public bool invertY = false, invertX = false;

        public GameObject Camera { get; private set; }
        private Vector2 _cameraRotation;
        private Vector2 _inputDelta;


        [Header("--- SMOOTHING ---")]
        public float smoothSpeed = 5;
        public bool smooth = true;

        private Vector3 _smoothedLookPoint, _startDirection;


        [Header("--- STEEP INCLINATIONS ---")]
        [Tooltip("Allows the camera to make a crane movement over the head when looking down," +
        " increasing visibility downwards.")]
        public bool improveSteepInclinations = true;
        public float inclinationAngle = 30, inclinationDistance = 1.5f;


        [Header("--- DISTANCES ---")]
        public float minDistance = 2;
        public float maxDistance = 5, initialDistance = 3.5f;

        private float _currentDistance;


        [Header("--- LIMITS ---")]
        [Tooltip("How far can the camera look down.")]
        public float minVerticalAngle = -30;

        [Tooltip("How far can the camera look up.")]
        public float maxVerticalAngle = 60;

        [Tooltip("Which layers don't make the camera reposition. Mainly the ActiveRagdoll one.")]
        public LayerMask dontBlockCamera;

        [Tooltip("How far to reposition the camera from an obstacle.")]
        public float cameraRepositionOffset = 0.15f;

        private void OnValidate() {
            if (_lookPoint == null)
                _lookPoint = _activeRagdoll.GetPhysicalBone(HumanBodyBones.Head);
        }

        void Start() {
            Camera = new GameObject("Active Ragdoll Camera", typeof(UnityEngine.Camera));
            Camera.transform.parent = transform;

            _smoothedLookPoint = _lookPoint.position;
            _currentDistance = initialDistance;

            _startDirection = _lookPoint.forward;
        }

        void Update() {
            UpdateCameraInput();
            UpdateCameraPosRot();
            AvoidObstacles();
        }

        private void UpdateCameraInput() {
            _cameraRotation.x = Mathf.Repeat(_cameraRotation.x + _inputDelta.x * (invertX ? -1 : 1) * lookSensitivity, 360);
            _cameraRotation.y = Mathf.Clamp(_cameraRotation.y + _inputDelta.y * (invertY ? 1 : -1) * lookSensitivity,
                                    minVerticalAngle, maxVerticalAngle);
        }

        private void UpdateCameraPosRot() {
            // Improve steep inclinations
            Vector3 movedLookPoint = _lookPoint.position;
            if (improveSteepInclinations) {
                float anglePercent = (_cameraRotation.y - minVerticalAngle) / (maxVerticalAngle - minVerticalAngle);
                float currentDistance = ((anglePercent * inclinationDistance) - inclinationDistance / 2);
                movedLookPoint += (Quaternion.Euler(inclinationAngle, 0, 0)
                    * Auxiliary.GetFloorProjection(Camera.transform.forward)) * currentDistance;
            }
            
            // Smooth
            _smoothedLookPoint = Vector3.Lerp(_smoothedLookPoint, movedLookPoint, smooth ? smoothSpeed * Time.deltaTime : 1);

            Camera.transform.position = _smoothedLookPoint - (_startDirection * _currentDistance);
            Camera.transform.RotateAround(_smoothedLookPoint, Vector3.right, _cameraRotation.y);
            Camera.transform.RotateAround(_smoothedLookPoint, Vector3.up, _cameraRotation.x);
            Camera.transform.LookAt(_smoothedLookPoint);
        }

        private void AvoidObstacles() {
            Ray cameraRay = new Ray(_lookPoint.position, Camera.transform.position - _lookPoint.position);
            bool hit = Physics.Raycast(cameraRay, out RaycastHit hitInfo,
                                       Vector3.Distance(Camera.transform.position, _lookPoint.position), ~dontBlockCamera);

            if (hit) {
                Camera.transform.position = hitInfo.point + (hitInfo.normal * cameraRepositionOffset);
                Camera.transform.LookAt(_smoothedLookPoint);
            }
        }



        // ------------- Input Handlers -------------

        public void OnLook(InputValue value) {
            _inputDelta = value.Get<Vector2>() / 10 ;
        }

        public void OnScrollWheel(InputValue value) {
            var scrollValue = value.Get<Vector2>();
            _currentDistance = Mathf.Clamp(_currentDistance + scrollValue.y / 1200 * - scrollSensitivity,
                                    minDistance, maxDistance);
        }
    }
} // namespace ActiveRagdoll
