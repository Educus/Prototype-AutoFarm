using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class InputManager : MonoBehaviour
{
    // 좌클릭 시 UI매니저를 통한 UI 오픈
    // 우클릭 시 플레이어 이동 및 해당 위치에 상호작용 대상이 있을 경우 상호작용
    // 창고, NPC의 경우 UI오픈 // 물탱크, 마른밭의 경우 행동 상호작용

    [SerializeField] private MovingManager movingManager;
    [SerializeField] private InteractionUIManager uiManager;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Pathfinder pathfinder;
    
    [SerializeField] private UIBuildMod buildModeUI;
    [SerializeField] private UIManagement uiManagement;
    [SerializeField] private InvenMod invenMod;

    private Player player;
    private PlayerController playerController;
    private PlayerAction playerAction;


    private Camera mainCamera;

    int layerMask;
    Coroutine moveCoroutine;

    private void Start()
    {
        player = GameManager.Instance.player;
        playerController = player.GetComponent<PlayerController>();
        playerAction = player.GetComponent<PlayerAction>();

        mainCamera = Camera.main;

        layerMask = LayerMask.GetMask("NPC", "Structure");
    }

    void Update()
    {
        HandleSystemInput();

        HandleUIInput();

        HandleSelectSlot();

        HandleMouseInput();
    }

    void HandleSystemInput()
    {
        // 테스트용 ESC
        if (KeyCodeDataManager.Instance.GetKeyDown("PopUp_Setting"))
        {
            if (GameManager.Instance.IsMode(GameMode.None))
            {
                // 설정 열기

                Debug.Log("설정 열기");
            } 
            else if (GameManager.Instance.IsMode(GameMode.Build))
            {
                DataManager.Instance.BuildingManager.ExitBuildMode();

                buildModeUI.BuildMode(false);
            }
            else if (GameManager.Instance.IsMode(GameMode.Popup))
            {
                uiManagement.ExitButton();

                UIStorageManagement.Instance.CloseStorageManagement();
            }
            else if (GameManager.Instance.IsMode(GameMode.Work))
            {
                Debug.Log("작업 모드 종료");
                GameManager.Instance.ExitWorkMode();

                UIStorageManagement.Instance.NPCInv();
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
    }

    void HandleUIInput()
    {
        if (KeyCodeDataManager.Instance.GetKeyDown("PopUp_Chart"))
        {

        }

        if (KeyCodeDataManager.Instance.GetKeyDown("PopUp_Shop"))
        {

        }

        if (KeyCodeDataManager.Instance.GetKeyDown("PopUp_Robot"))
        {
            uiManagement.OpenManagement();
        }

        if (KeyCodeDataManager.Instance.GetKeyDown("PopUp_Storage"))
        {

        }

        if (KeyCodeDataManager.Instance.GetKeyDown("PopUp_Hydro"))
        {

        }
    }

    void HandleSelectSlot()
    {
        if (KeyCodeDataManager.Instance.GetKeyDown("Select_Slot1"))
        {
            invenMod.SelectSubSlot(0);
        }

        if (KeyCodeDataManager.Instance.GetKeyDown("Select_Slot2"))
        {
            invenMod.SelectSubSlot(1);
        }

        if (KeyCodeDataManager.Instance.GetKeyDown("Select_Slot3"))
        {
            invenMod.SelectSubSlot(2);
        }
    }

    void HandleMouseInput()
    {
        // 좌클릭
        if (KeyCodeDataManager.Instance.GetKeyDown("PC_Movement"))
        {
            LeftClick();
        }

        // 우클릭
        if (KeyCodeDataManager.Instance.GetKeyDown("PC_Interaction"))
        {
            RightClick();
        }
    }

    void LeftClick()
    {
        if (UnityEngine.EventSystems.EventSystem.current
        .IsPointerOverGameObject())
            return;

        // Popup, Build 모드는 차단
        if (GameManager.Instance.IsMode(GameMode.Popup) ||
            GameManager.Instance.IsMode(GameMode.Build))
            return;

        Vector2 mousePos =
            Camera.main.ScreenToWorldPoint(
                Input.mousePosition);

        Collider2D hit =
            GetPriorityHit(mousePos);

        if (hit == null)
            return;

        // WorkMode
        if (GameManager.Instance.IsMode(GameMode.Work))
        {
            BuildingBase building =
                hit.GetComponentInParent<BuildingBase>();

            if (building != null)
            {
                building.OnWorkInteract();

                return;
            }
        }

        // 일반 상호작용
        ILeftInteractable interactable =
            hit.GetComponentInParent<ILeftInteractable>();

        interactable?.OnInteract();
    }

    void RightClick()
    {
        if (UnityEngine.EventSystems.EventSystem.current
        .IsPointerOverGameObject())
            return;

        // 모드 중엔 입력 차단
        if (!GameManager.Instance.IsMode(GameMode.None))
            return;

        Vector2 mousePos =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        Collider2D hit =
            GetPriorityHit(mousePos);

        IRightInteractable interactable = null;

        if (hit != null)
        {
            interactable =
                hit.GetComponentInParent<IRightInteractable>();
        }

        // 가까우면 즉시 상호작용
        if (interactable != null)
        {
            float distance =
                Vector2.Distance(
                    player.transform.position,
                    hit.transform.position
                );

            if (distance <= 1f)
            {
                interactable.OnInteract(player.subInventory.slots[player.selectedSubSlotIndex].itemID);
                return;
            }
        }

        // 이동 처리
        Vector2 currentWorldPos =
            (Vector2)player.transform.position + Vector2.up * 0.5f;

        Vector2Int currentPos =
            gridManager.WorldToGrid(currentWorldPos);

        Vector2Int targetPos =
            gridManager.WorldToGrid(mousePos);

        List<Node> path =
            pathfinder.FindPath(currentPos, targetPos);

        if (path == null || path.Count == 0)
            return;

        // 이동 후 상호작용
        playerController.Move(
            path,
            () =>
            {
                if (interactable != null)
                {
                    interactable.OnInteract(player.subInventory.slots[player.selectedSubSlotIndex].itemID);
                }
            });
    }

    private Collider2D GetPriorityHit(Vector2 mousePos)
    {
        Collider2D[] hits =
            Physics2D.OverlapPointAll(mousePos, layerMask);

        Collider2D structureHit = null;

        foreach (Collider2D hit in hits)
        {
            // NPC를 가장 높은 우선순위
            if (hit.GetComponentInParent<NPC>() != null)
                return hit;

            // 건물은 후보로만 저장
            if (structureHit == null &&
                hit.GetComponentInParent<BuildingBase>() != null)
            {
                structureHit = hit;
            }
        }

        return structureHit;
    }

    public void Move(List<Node> path)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine =
            StartCoroutine(MoveAlongPath(path, 1f));
    }

    IEnumerator MoveAlongPath(List<Node> path, float speed)
    {
        foreach (Node node in path)
        {
            Vector3 targetPos =
                new Vector3(node.x, node.y, 0);

            while (Vector3.Distance(
                transform.position,
                targetPos) > 0.05f)
            {
                transform.position =
                    Vector3.MoveTowards(
                        transform.position,
                        targetPos,
                        speed * Time.deltaTime
                    );

                yield return null;
            }
        }
    }
}
