using UnityEngine;
using Unity.Mathematics;

public class CameraController : MonoBehaviour 
{
    private Camera _camera;
    
    [Header("Fitting Settings")]
    [SerializeField] private Vector3 boundsBufferSize = new Vector3(1f, 0f, 1f);
    [SerializeField] private float fitAdjustSpeed = 5f;
    [SerializeField] private float fitPositionThreshold = 0.1f;
    [SerializeField] private float fitSizeThreshold = 0.1f;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveStep = 5f;

    private Bounds _targetBounds;
    private bool _isFitting;
    private float3 _targetPosition;
    public static CameraController Instance { get; private set; }

    private void Awake() 
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(this);
            return;
        }
        
        _camera = GetComponent<Camera>();
        Instance = this;
        _targetPosition = _camera.transform.position;
    }

    private void Update() 
    {
        UpdateCameraPosition();
    }

    public void FitToBounds(Bounds bounds) 
    {
        bounds.Expand(boundsBufferSize);
        _targetBounds = bounds;
        _targetPosition = new float3(
                bounds.center.x,
                _targetPosition.y,
                bounds.center.z
            );
        _isFitting = true;
    }

    public void ZoomIn() 
    {
        _targetBounds.size *= 0.5f;
        _isFitting = true;
    }
    
    public void ZoomOut() 
    {
        _targetBounds.size *= 1.5f;
        _isFitting = true;
    }

    public void Move(Vector2 direction) {
        float multiplier = math.max(1f, _camera.orthographicSize / 100);
        _targetPosition = new float3(
            _targetPosition.x + moveStep * direction.x * multiplier, 
            _targetPosition.y,
            _targetPosition.z + moveStep * direction.y * multiplier
            );
        _isFitting = true;
    }

    private void UpdateCameraPosition()
    {
        if (!_isFitting) {
            return;
        }
        // Calculate required orthographic size
        float aspect = _camera.aspect;
        float widthBasedSize = _targetBounds.size.x * 0.5f / aspect;
        float heightBasedSize = _targetBounds.size.z * 0.5f;
        float targetSize = math.max(widthBasedSize, heightBasedSize);
        
        // Smoothly adjust to fit
        _camera.transform.position = math.lerp(
            _camera.transform.position,
            _targetPosition,
            moveSpeed * Time.deltaTime
            );
        
        _camera.orthographicSize = math.lerp(
            _camera.orthographicSize,
            targetSize,
            fitAdjustSpeed * Time.deltaTime
        );
        
        // Check if fitting is complete
        float positionDistance = Vector3.Distance(_camera.transform.position, _targetPosition);
        float sizeDistance = math.abs(_camera.orthographicSize - targetSize);
        
        if (positionDistance < fitPositionThreshold && sizeDistance < fitSizeThreshold)
        {
            _camera.transform.position = _targetPosition;
            _camera.orthographicSize = targetSize;
            _isFitting = false;
        }
    }
}