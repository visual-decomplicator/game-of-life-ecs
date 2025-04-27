using Unity.Mathematics;
using UnityEngine;

public class CameraFitter : MonoBehaviour 
{
    private Camera _camera;
    [SerializeField] private Vector3 boundsBufferSize = new Vector3(1f, 0f, 1f);
    [SerializeField] private float cameraAdjustSpeed = 5f;
    [SerializeField] private float positionThreshold = 0.1f;
    [SerializeField] private float sizeThreshold = 0.1f;
    
    private Bounds _targetBounds;
    private bool _isAdjusting;
    
    public static CameraFitter Instance { get; private set; }

    private void Awake() {
        Instance = this;
        _camera = GetComponent<Camera>();
    }

    public void DoFitCamera(Bounds bounds) 
    {
        bounds.Expand(boundsBufferSize);
        _targetBounds = bounds;
        _isAdjusting = true;
    }

    void Update() 
    {
        if (!_isAdjusting) return;
        
        // Calculate target position (center of bounds at camera height)
        Vector3 targetPosition = _targetBounds.center;
        targetPosition.y = _camera.transform.position.y; // Maintain camera height
        
        // Calculate required orthographic size
        float aspect = _camera.aspect;
        float widthBasedSize = _targetBounds.size.x * 0.5f / aspect;
        float heightBasedSize = _targetBounds.size.z * 0.5f;
        float targetSize = math.max(widthBasedSize, heightBasedSize);
        
        // Smoothly adjust position
        _camera.transform.position = Vector3.Lerp(
            _camera.transform.position,
            targetPosition,
            cameraAdjustSpeed * Time.deltaTime
        );
        
        // Smoothly adjust size
        _camera.orthographicSize = math.lerp(
            _camera.orthographicSize,
            targetSize,
            cameraAdjustSpeed * Time.deltaTime
        );
        
        // Check if adjustment is complete
        float positionDistance = Vector3.Distance(_camera.transform.position, targetPosition);
        float sizeDistance = math.abs(_camera.orthographicSize - targetSize);
        
        if (positionDistance < positionThreshold && sizeDistance < sizeThreshold)
        {
            _camera.transform.position = targetPosition;
            _camera.orthographicSize = targetSize;
            _isAdjusting = false;
        }
    }
}
