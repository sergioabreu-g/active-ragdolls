#pragma warning disable 649

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ActiveRagdoll {
    public class CameraModule : Module {
        [Serializable]
        public struct Config {
            [Header("Config")]
            public float _lookSesitivity;
            public float _scrollSensitivity;
            public bool _invertY, _invertX;

            [Header("Look point & smoothing")]
            public float _smoothSpeed;
            public bool _smooth;

            [Header("Improve steep inclinations")]
            public bool _improveSteepInclinations;
            [Range(0, 90)] public float _inclinationAngle;
            public float _inclinationDistance, _minDistanceToLookedObject;

            [Header("Distances")]
            public float _minDistance;
            public float _maxDistance, _defaultDistance;

            [Header("Limits")]
            public float _minVerticalAngle;
            public float _maxVerticalAngle;
            public LayerMask _dontBlockCamera;
            [Tooltip("If there's an obstacle between the camera and the lookpoint, how far should it be repositioned from it.")]
            public float _cameraRepositionOffset;
        }
        private Config _config;

        private Vector3 _smoothedLookPoint, _startDirection;
        public Transform _lookPoint;


        private float _currentDistance;

        private GameObject _camera;

        private Vector2 _mouse;
        private Vector2 _mouseDelta;

        void Start() {
            _camera = new GameObject("Active Ragdoll Camera", typeof(UnityEngine.Camera));
            _camera.transform.parent = transform;
            _activeRagdoll.SetCamera(_camera.GetComponent<Camera>());

            _smoothedLookPoint = _lookPoint.position;
            _currentDistance = _config._defaultDistance;

            _startDirection = _lookPoint.forward;
        }

        void Update() {
            // Cache previous camera pos. If the camera can't reposition properly, it will reset to the last position.
            // This avoids the problem of getting stuck alltogether, a fucking blast. Also cache the mouse not to get
            // the player input stuck.
            Vector3 previousPos = _camera.transform.position;
            Vector2 previousMouse = _mouse;

            // Update the mouse
            _mouse.x = Mathf.Repeat(_mouse.x + _mouseDelta.x * (_config._invertX ? -1 : 1) * _config._lookSesitivity, 360);
            _mouse.y = Mathf.Clamp(_mouse.y + _mouseDelta.y * (_config._invertY ? 1 : -1) * _config._lookSesitivity,
                                    _config._minVerticalAngle, _config._maxVerticalAngle);

            // Move the look point to improve steep inclinations visibility
            Vector3 movedLookPoint = _lookPoint.position;

            if (_config._improveSteepInclinations) {
                float anglePercent = (_mouse.y - _config._minVerticalAngle) / (_config._maxVerticalAngle - _config._minVerticalAngle);
                float currentDistance = ((anglePercent * _config._inclinationDistance) - _config._inclinationDistance / 2);
                movedLookPoint += (Quaternion.Euler(_config._inclinationAngle, 0, 0)
                    * Auxiliary.GetFloorProjection(_camera.transform.forward)) * currentDistance;
            }

            // Smooth look point to avoid jittering if the object movements are jerky
            _smoothedLookPoint = Vector3.Lerp(_smoothedLookPoint, movedLookPoint, _config._smooth ? _config._smoothSpeed * Time.deltaTime : 1);

            // Update the camera position & rotation
            _camera.transform.position = _smoothedLookPoint - (_startDirection * _currentDistance);
            _camera.transform.RotateAround(_smoothedLookPoint, Vector3.right, _mouse.y);
            _camera.transform.RotateAround(_smoothedLookPoint, Vector3.up, _mouse.x);
            _camera.transform.LookAt(_smoothedLookPoint);

            // Check for objects in the way and readjust camera position/look if necessary
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
