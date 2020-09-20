#pragma warning disable 649

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveRagdoll {
    // Original author: Sergio Abreu García | https://sergioabreu.me

    public class CameraModule : Module {
        [Serializable]
        public struct Config {
            [Header("--- GENERAL ---")]
            public float _lookSesitivity;
            public float _scrollSensitivity;
            public bool _invertY, _invertX;

            [Header("--- SMOOTHING ---")]
            public float _smoothSpeed;
            public bool _smooth;

            [Header("--- STEEP INCLINATIONS ---")]
            [Tooltip("Allows the camera to make a crane movement over the head when looking down," +
                    " increasing visibility downwards.")]
            public bool _improveSteepInclinations;
            [Range(0, 90)] public float _inclinationAngle;
            public float _inclinationDistance, _minDistanceToLookedObject;

            [Header("--- DISTANCES ---")]
            public float _minDistance;
            public float _maxDistance;
            public float _initialDistance;

            [Header("--- LIMITS ---")]
            [Tooltip("How far can the camera look down.")]
            public float _minVerticalAngle;
            [Tooltip("How far can the camera look up.")]
            public float _maxVerticalAngle;
            [Tooltip("Which layers don't make the camera reposition. Mainly the ActiveRagdoll one.")]
            public LayerMask _dontBlockCamera;
            [Tooltip("How far to reposition the camera from an obstacle.")]
            public float _cameraRepositionOffset;
        }
        private Config _config;

        public Transform _lookPoint;
        private GameObject _camera;
        private Vector3 _smoothedLookPoint, _startDirection;
        private float _currentDistance;

        private Vector2 _mouse;
        private Vector2 _mouseDelta;



        void Start() {
            if (_lookPoint == null)
                Debug.LogError("No look point was selected for the Camera Module!");

            _camera = new GameObject("Active Ragdoll Camera", typeof(UnityEngine.Camera));
            _camera.transform.parent = transform;
            _activeRagdoll.SetCharacterCamera(_camera.GetComponent<Camera>());

            _smoothedLookPoint = _lookPoint.position;
            _currentDistance = _config._initialDistance;

            _startDirection = _lookPoint.forward;
        }

        void Update() {
            // If the camera can't reposition properly, it will reset to its last pos. Avoids getting stuck.
            Vector3 previousPos = _camera.transform.position;
            Vector2 previousMouse = _mouse;

            _mouse.x = Mathf.Repeat(_mouse.x + _mouseDelta.x * (_config._invertX ? -1 : 1) * _config._lookSesitivity, 360);
            _mouse.y = Mathf.Clamp(_mouse.y + _mouseDelta.y * (_config._invertY ? 1 : -1) * _config._lookSesitivity,
                                    _config._minVerticalAngle, _config._maxVerticalAngle);


            // Improve steep inclinations visibility
            Vector3 movedLookPoint = _lookPoint.position;
            if (_config._improveSteepInclinations) {
                float anglePercent = (_mouse.y - _config._minVerticalAngle) / (_config._maxVerticalAngle - _config._minVerticalAngle);
                float currentDistance = ((anglePercent * _config._inclinationDistance) - _config._inclinationDistance / 2);
                movedLookPoint += (Quaternion.Euler(_config._inclinationAngle, 0, 0)
                    * Auxiliary.GetFloorProjection(_camera.transform.forward)) * currentDistance;
            }

            _smoothedLookPoint = Vector3.Lerp(_smoothedLookPoint, movedLookPoint, _config._smooth ? _config._smoothSpeed * Time.deltaTime : 1);

            _camera.transform.position = _smoothedLookPoint - (_startDirection * _currentDistance);
            _camera.transform.RotateAround(_smoothedLookPoint, Vector3.right, _mouse.y);
            _camera.transform.RotateAround(_smoothedLookPoint, Vector3.up, _mouse.x);
            _camera.transform.LookAt(_smoothedLookPoint);


            // Check for obstacles and reposition
            Ray cameraRay = new Ray(_lookPoint.position, _camera.transform.position - _lookPoint.position);
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(cameraRay, out hitInfo,
                                       Vector3.Distance(_camera.transform.position, _lookPoint.position), ~_config._dontBlockCamera);

            if (hit) {
                _camera.transform.position = hitInfo.point + (hitInfo.normal * _config._cameraRepositionOffset);
                _camera.transform.LookAt(_smoothedLookPoint);
            }

            if (Vector3.Distance(_lookPoint.position, _camera.transform.position) <= _config._minDistanceToLookedObject) {
                _camera.transform.position = previousPos;
                _camera.transform.LookAt(_smoothedLookPoint);
                _mouse = previousMouse;
            }
        }

        override public void StateChanged(in ActiveRagdollState state) {
            _config = state.cameraModuleConfig;
        }



        // ------------- Input Handlers -------------

        public void OnLook(InputValue value) {
            _mouseDelta = value.Get<Vector2>() / 8;
        }

        public void OnScrollWheel(InputValue value) {
            var scrollValue = value.Get<Vector2>();
            _currentDistance = Mathf.Clamp(_currentDistance + scrollValue.y / 1200 * - _config._scrollSensitivity,
                                    _config._minDistance, _config._maxDistance);
        }
    }
} // namespace ActiveRagdoll
