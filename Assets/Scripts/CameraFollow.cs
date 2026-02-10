using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    public float zoomAmount = 5f;
    public float zoomSpeed = 2f;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        Vector3 desiredPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoomAmount, zoomSpeed * Time.deltaTime);
    }
}
