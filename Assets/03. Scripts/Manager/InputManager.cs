using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // 좌클릭 시 UI매니저를 통한 UI 오픈
    // 우클릭 시 플레이어 이동 및 해당 위치에 상호작용 대상이 있을 경우 상호작용
    // 창고, NPC의 경우 UI오픈 // 물탱크, 마른밭의 경우 행동 상호작용
    [SerializeField] private MovingManager movingManager;
    [SerializeField] private InteractionUIManager uiManager;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Pathfinder pathfinder;
    
    [SerializeField] private BuildUIManager buildModeUI;
    [SerializeField] private UIManagement uiManagement;

    [SerializeField] public Player player;

    private Camera camera;

    int layerMask;
    Coroutine moveCoroutine;
    private void Start()
    {
        camera = Camera.main;

        layerMask = LayerMask.GetMask("NPC", "Structure");
    }

    void Update()
    {
        // 테스트용 ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.IsMode(GameMode.Build))
            {
                DataManager.Instance.BuildingManager.ExitBuildMode();

                buildModeUI.BuildMode(false);
            }
            else if (GameManager.Instance.IsMode(GameMode.Popup))
            {
                uiManagement.ExitButton();
            }
            else
            {
                GameManager.Instance.ExitMode();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.Instance.isPlay = !GameManager.Instance.isPlay;

            Debug.Log(
                GameManager.Instance.isPlay
                ? "게임 시작"
                : "게임 일시정지");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            uiManagement.OpenManagement();
        }

        // 좌클릭, 우클릭
        if (Input.GetMouseButtonDown(0))
        {
            LeftClick();
        }

        // if (Input.GetMouseButtonDown(1))
        // {
        //     Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //     Vector2Int gridPos = gridManager.WorldToGrid(mousePos);
        // 
        //     Vector2Int currentPos = gridManager.WorldToGrid(player.transform.position);
        //     List<Node> path = pathfinder.FindPath(currentPos, gridPos);
        // 
        //     if (path != null && path.Count > 0)
        //     {
        //         Move(path);
        //     }
        // 
        //     return;
        //     RightClick();
        // }
    }

    void LeftClick()
    {
        if (UnityEngine.EventSystems.EventSystem.current
        .IsPointerOverGameObject())
            return;

        // Build 모드 중엔 차단
        if (GameManager.Instance.IsMode(GameMode.Build))
            return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Collider2D hit = Physics2D.OverlapPoint(mousePos, layerMask);

        Debug.Log($"좌클:{hit}");

        IInteractable interactable = null;

        if (hit != null)
        {
            interactable = hit.GetComponentInParent<IInteractable>();
        }

        if (interactable != null)
        {
            interactable.OnInteract();
        }
    }

    void RightClick()
    {
        Vector2 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

        Collider2D hit = Physics2D.OverlapPoint(mousePos, layerMask);

        IInteractable interactable = null;

        if (hit != null)
        {
            interactable = hit.GetComponentInParent<IInteractable>();
        }

        // 가까울 경우 바로 상호작용
        if (Vector2.Distance(player.transform.position, mousePos) <= 1f)
        {
            if (interactable != null) interactable.OnInteract();
            return;
        }

        // 멀 경우 이동 후 상호작용
        movingManager.Moving
            (
                player.transform,
                mousePos,
                player.moveSpeed,
                () => { if (interactable != null) { interactable.OnInteract(); } }
            );
    }

    public void Move(List<Node> path)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveAlongPath(path, 1f));
    }
    IEnumerator MoveAlongPath(List<Node> path, float speed)
    {
        foreach (Node node in path)
        {
            Vector3 targetPos = new Vector3(node.x, node.y, 0);

            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    speed * Time.deltaTime
                );

                yield return null;
            }
        }
    }
}
