using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UnityEngine.Camera))]
public class Camera : MonoBehaviour {
    [Header("Config")]
    [SerializeField] private float _mouseSensitivity = 1;
    [SerializeField] private float _scrollSensitivity = 1;
    [SerializeField] private bool _invertY = false, _invertX = false;

    [Header("Look point & smoothing")]
    [SerializeField] private Transform _lookPoint;
    [SerializeField] private float _smoothSpeed = 5;
    [SerializeField] private bool _smooth = true;

    [Header("Improve steep inclinations")]
    [SerializeField] private bool _improveSteepInclinations = true;
    [SerializeField]
    [Range(0, 90)] private float _inclinationAngle = 60;
    [SerializeField] private float _inclinationDistance = 0.6f, _minDistanceToLookedObject = 1.5f;

    private Vector3 _smoothedLookPoint, _startDirection;

    [Header("Distances")]
    [SerializeField] private float _minDistance = 2f;
    [SerializeField] private float _maxDistance = 5f, _defaultDistance = 3f;

    private float _currentDistance;

    [Header("Limits")]
    [SerializeField] private float _minVerticalAngle = -60;
    [SerializeField] private float _maxVerticalAngle = 60;
    [SerializeField] private LayerMask _dontBlockCamera;
    [SerializeField] [Tooltip("If there's an obstacle between the camera and the lookpoint, how far should it be repositioned from it.")] private float _cameraRepositionOffset = 0.02f;

    private UnityEngine.Camera _camera;
    private Vector2 _mouse;
    private Vector2 _mouseDelta;

    void Start() {
        _camera = GetComponent<UnityEngine.Camera>();
        _smoothedLookPoint = _lookPoint.position;
        _currentDistance = _defaultDistance;

        _startDirection = _lookPoint.forward;
    }

    void Update() {
        // Cache previous camera pos. If the camera can't reposition properly, it will reset to the last position.
        // This avoids the problem of getting stuck alltogether, a fucking blast. Also cache the mouse not to get
        // the player input stuck.
        Vector3 previousPos = transform.position;
        Vector2 previousMouse = _mouse;

        // Update the mouse
        _mouse.x = Mathf.Repeat(_mouse.x + _mouseDelta.x * (_invertX ? -1 : 1) * _mouseSensitivity, 360);
        _mouse.y = Mathf.Clamp(_mouse.y + _mouseDelta.y * (_invertY ? 1 : -1) * _mouseSensitivity,
                                _minVerticalAngle, _maxVerticalAngle);

        // Move the look point to improve steep inclinations visibility
        Vector3 movedLookPoint = _lookPoint.position;

        if (_improveSteepInclinations) {
            float anglePercent = (_mouse.y - _minVerticalAngle) / (_maxVerticalAngle - _minVerticalAngle);
            float currentDistance = ((anglePercent * _inclinationDistance) - _inclinationDistance / 2);
            movedLookPoint += (Quaternion.Euler(_inclinationAngle, 0, 0)
                * ActiveRagdoll.Auxiliary.GetFloorProjection(transform.forward)) * currentDistance;
        }

        // Smooth look point to avoid jittering if the object movements are jerky
        _smoothedLookPoint = Vector3.Lerp(_smoothedLookPoint, movedLookPoint, _smooth ? _smoothSpeed * Time.deltaTime : 1);

        // Update the camera position & rotation
        transform.position = _smoothedLookPoint - (_startDirection * _currentDistance);
        transform.RotateAround(_smoothedLookPoint, Vector3.right, _mouse.y);
        transform.RotateAround(_smoothedLookPoint, Vector3.up, _mouse.x);
        transform.LookAt(_smoothedLookPoint);

        // Check for objects in the way and readjust camera position/look if necessary
        Ray cameraRay = new Ray(_lookPoint.position, transform.position - _lookPoint.position);
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(cameraRay, out hitInfo,
                                   Vector3.Distance(transform.position, _lookPoint.position), ~_dontBlockCamera);

        if (hit) {
            transform.position = hitInfo.point + (hitInfo.normal * _cameraRepositionOffset);
            transform.LookAt(_smoothedLookPoint);
        }

        if (Vector3.Distance(_lookPoint.position, transform.position) <= _minDistanceToLookedObject) {
            transform.position = previousPos;
            transform.LookAt(_smoothedLookPoint);
            _mouse = previousMouse;
        }
    }


    // ------------- Input Handlers -------------

    public void OnLook(InputValue value) {
        _mouseDelta = value.Get<Vector2>();
    }

    public void OnScrollWheel(InputValue value) {
        var scrollValue = value.Get<Vector2>();
        _currentDistance = Mathf.Clamp(_currentDistance + scrollValue.y / 1200 * -_scrollSensitivity,
                                _minDistance, _maxDistance);
    }
}
