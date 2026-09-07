using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 15, -10);
    public float smoothSpeed = 0.125f;

    // Zoom
    public float minZoom = 5f;
    public float maxZoom = 100f;
    public float zoomSpeed = 10f;

    // Rotación
    public float rotationSpeed = 0.2f;
    private float currentAngle = 0f;
    private Vector2 lastTouchPos;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif

        Quaternion rotation = Quaternion.Euler(0f, currentAngle, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        transform.LookAt(target);
    }

    void HandleMouseInput()
    {
        // ZOOM PC
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        offset.y -= scroll * zoomSpeed;
        offset.y = Mathf.Clamp(offset.y, minZoom, maxZoom);

        // ROTACIÓN PC (botón derecho)
        if (Input.GetMouseButton(1))
        {
            float h = Input.GetAxis("Mouse X");
            currentAngle += h * rotationSpeed * 100f * Time.deltaTime;
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                currentAngle += touch.deltaPosition.x * rotationSpeed;
            }
        }
        else if (Input.touchCount == 2)
        {
            // ZOOM móvil con pinzas
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
            Vector2 prevPos1 = touch1.position - touch1.deltaPosition;

            float prevMag = (prevPos0 - prevPos1).magnitude;
            float currentMag = (touch0.position - touch1.position).magnitude;

            float difference = prevMag - currentMag;

            offset.y += difference * Time.deltaTime * zoomSpeed;
            offset.y = Mathf.Clamp(offset.y, minZoom, maxZoom);
        }
    }
}