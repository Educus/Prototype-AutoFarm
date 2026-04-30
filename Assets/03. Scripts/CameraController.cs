using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    // 줌 
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 10f;

    // 이동
    public float moveSpeed = 10f;
    public float edgeSize = 10f;

    // 이동 제한
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Camera camera;

    void Start()
    {
        camera = Camera.main;
    }

    void Update()
    {
        HandleZoom();
        HandleMovement();
        ClampCamera();
    }

    // 줌 인 아웃
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0 && !EventSystem.current.IsPointerOverGameObject())
        {
            camera.orthographicSize -= scroll * zoomSpeed;
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, minZoom, maxZoom);
        }
    }

    // 카메라 이동
    void HandleMovement()
    {
        Vector3 pos = transform.position;

        if (Input.mousePosition.x >= Screen.width - edgeSize)
        {
            pos.x += moveSpeed * Time.deltaTime;
        }
        else if (Input.mousePosition.x <= edgeSize)
        {
            pos.x -= moveSpeed * Time.deltaTime;
        }

        if (Input.mousePosition.y >= Screen.height - edgeSize)
        {
            pos.y += moveSpeed * Time.deltaTime;
        }
        else if (Input.mousePosition.y <= edgeSize)
        {
            pos.y -= moveSpeed * Time.deltaTime;
        }

        transform.position = pos;
    }

    // 카메라 최대값
    void ClampCamera()
    {
        float camHeight = camera.orthographicSize;
        float camWidth = camHeight * Screen.width / Screen.height;

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        pos.y = Mathf.Clamp(pos.y, minBounds.y + camHeight, maxBounds.y - camHeight);

        transform.position = pos;
    }
}
