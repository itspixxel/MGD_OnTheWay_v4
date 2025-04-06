using UnityEngine;

public class CameraViewSwitcher : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    
    // Current view settings
    private Vector3 currentViewPosition = new Vector3(-1f, 10f, -4f);
    private Vector3 currentViewRotation = new Vector3(45f, 30f, 0f);
    
    // Top-down isometric view settings
    private Vector3 topDownPosition = new Vector3(0f, 15f, 0f);
    private Vector3 topDownRotation = new Vector3(90f, 0f, 0f);
    
    private bool isTopDownView = false;
    
    public bool IsTopDownView => isTopDownView;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Set initial camera position and rotation
        SetCameraTransform(currentViewPosition, currentViewRotation);
    }

    public void ToggleCameraView()
    {
        isTopDownView = !isTopDownView;
        
        if (isTopDownView)
        {
            SetCameraTransform(topDownPosition, topDownRotation);
        }
        else
        {
            SetCameraTransform(currentViewPosition, currentViewRotation);
        }
    }

    private void SetCameraTransform(Vector3 position, Vector3 rotation)
    {
        mainCamera.transform.position = position;
        mainCamera.transform.rotation = Quaternion.Euler(rotation);
    }
} 