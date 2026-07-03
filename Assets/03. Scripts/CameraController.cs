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

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera Missing");
        }
    }

    void Update()
    {
        if (GameManager.Instance.targetLock != null)
        {
            FollowTarget();
        }

        if (!GameManager.Instance.IsMode(GameMode.Popup))
        {
            HandleZoom();
            HandleMovement();
        }

        ClampCamera();
    }

    // 줌 인 아웃
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0 && 
            (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
        {
            mainCamera.orthographicSize -= scroll * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
        }
    }

    // 카메라 이동
    void HandleMovement()
    {
        // 마우스 위치에 UI가 있으면 이동하지 않음
        // if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        // {
        //     return;
        // }

        Vector3 pos = transform.position;
        bool isMoving = false;

        if (Input.mousePosition.x >= Screen.width - edgeSize)
        {
            pos.x += moveSpeed * Time.deltaTime;
            isMoving = true;
        }
        else if (Input.mousePosition.x <= edgeSize)
        {
            pos.x -= moveSpeed * Time.deltaTime;
            isMoving = true;
        }

        if (Input.mousePosition.y >= Screen.height - edgeSize)
        {
            pos.y += moveSpeed * Time.deltaTime;
            isMoving = true;
        }
        else if (Input.mousePosition.y <= edgeSize)
        {
            pos.y -= moveSpeed * Time.deltaTime;
            isMoving = true;
        }

        if (isMoving && GameManager.Instance.targetLock != null)
        {
            GameManager.Instance.targetLock = null;
        }

        transform.position = pos;
    }

    void FollowTarget()
    {
        Vector3 targetPos = GameManager.Instance.targetLock.transform.position;
        targetPos.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }

    // 카메라 최대값
    void ClampCamera()
    {
        float camHeight = mainCamera.orthographicSize;
        float camWidth = camHeight * Screen.width / Screen.height;

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        pos.y = Mathf.Clamp(pos.y, minBounds.y + camHeight, maxBounds.y - camHeight);

        transform.position = pos;
    }
}
