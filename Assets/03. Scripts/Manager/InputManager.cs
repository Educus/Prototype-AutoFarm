using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    // 좌클릭: 클릭 위치의 대상(ILeftInteractable)에게 상호작용을 전달
    // 우클릭: 플레이어 이동 + 도착 후 대상(IRightInteractable)과 상호작용

    [SerializeField] private UIBuildMod buildModeUI;
    [SerializeField] private UIManagement uiManagement;
    [SerializeField] private InvenMod invenMod;

    private const float InteractRange = 1f;

    private static readonly string[] ClickLayerNames = { "NPC", "Animal", "Building", "BuildingPoint" };
    private static readonly string[] SlotKeys = { "Select_Slot1", "Select_Slot2", "Select_Slot3" };

    private readonly List<Collider2D> hitBuffer = new();

    private Player player;
    private PlayerController playerController;
    private Camera mainCamera;

    private int layerMask;
    private ContactFilter2D clickFilter;

    private void Start()
    {
        player = GameManager.Instance.player;
        playerController = player.GetComponent<PlayerController>();

        mainCamera = Camera.main;

        // 존재하지 않는 레이어 이름은 GetMask가 에러 없이 무시하므로 미리 검사
        foreach (string layerName in ClickLayerNames)
        {
            if (LayerMask.NameToLayer(layerName) < 0)
                Debug.LogError($"[InputManager] 존재하지 않는 레이어: '{layerName}'");
        }

        layerMask = LayerMask.GetMask(ClickLayerNames);

        clickFilter = new ContactFilter2D();
        clickFilter.SetLayerMask(layerMask);
        clickFilter.useTriggers = false;
    }

    private void Update()
    {
        HandleSystemInput();
        HandleUIInput();
        HandleSelectSlot();
        HandleMouseInput();
    }

    #region Keyboard

    private void HandleSystemInput()
    {
        // 테스트용 ESC
        if (KeyCodeDataManager.Instance.GetKeyDown("PopUp_Setting"))
        {
            GameManager gm = GameManager.Instance;

            if (gm.IsMode(GameMode.None))
            {
                // 설정 열기
                Debug.Log("설정 열기");
            }
            else if (gm.IsMode(GameMode.Build))
            {
                DataManager.Instance.BuildingManager.ExitBuildMode();
                buildModeUI.BuildMode(false);
            }
            else if (gm.IsMode(GameMode.Popup))
            {
                uiManagement.ExitButton();
                UIStorageManagement.Instance.CloseStorageManagement();
            }
            else if (gm.IsMode(GameMode.Work))
            {
                Debug.Log("작업 모드 종료");
                gm.ExitWorkMode();
                UIStorageManagement.Instance.NPCInv();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager gm = GameManager.Instance;

            gm.isPlay = !gm.isPlay;
            TimeManager.Instance.SetTimeScale(gm.isPlay ? 1 : 0);

            Debug.Log(gm.isPlay ? "게임 시작" : "게임 일시정지");
        }
    }

    private void HandleUIInput()
    {
        // TODO: PopUp_Chart, PopUp_Shop, PopUp_Storage, PopUp_Hydro 구현 시 추가
        if (KeyCodeDataManager.Instance.GetKeyDown("PopUp_Robot"))
            uiManagement.OpenManagement();

        if (KeyCodeDataManager.Instance.GetKeyDown("PopUp_Chart"))
        {

        }

        if (KeyCodeDataManager.Instance.GetKeyDown("PopUp_Shop"))
        {

        }

        if (KeyCodeDataManager.Instance.GetKeyDown("PopUp_Storage"))
        {

        }

        if (KeyCodeDataManager.Instance.GetKeyDown("PopUp_Hydro"))
        {

        }
    }

    private void HandleSelectSlot()
    {
        for (int i = 0; i < SlotKeys.Length; i++)
        {
            if (KeyCodeDataManager.Instance.GetKeyDown(SlotKeys[i]))
                invenMod.SelectSubSlot(i);
        }
    }

    #endregion

    #region Mouse

    private void HandleMouseInput()
    {
        // 좌클릭
        if (KeyCodeDataManager.Instance.GetKeyDown("PC_Movement"))
            LeftClick();

        // 우클릭
        if (KeyCodeDataManager.Instance.GetKeyDown("PC_Interaction"))
            RightClick();
    }

    private void LeftClick()
    {
        // Popup 모드는 차단
        if (GameManager.Instance.IsMode(GameMode.Popup))
            return;

        if (!TryGetWorldClick(out Vector2 worldPos))
            return;

        // 모드별 동작(Build / Work / None)은 대상이 직접 판단
        if (TryGetTarget<ILeftInteractable>(worldPos, GetAllowedTargets(), out _, out var target))
            target.OnLInteract();
    }

    private void RightClick()
    {
        // 모드 중에는 입력 차단
        if (!GameManager.Instance.IsMode(GameMode.None))
            return;

        // 플레이어가 작업 중이면 입력 차단
        if (!playerController.CanMove)
            return;

        if (!TryGetWorldClick(out Vector2 worldPos))
            return;

        TryGetTarget<IRightInteractable>(worldPos, ClickTarget.All, out var hit, out var interactable);

        // 가까우면 즉시 상호작용 (큰 건물을 위해 콜라이더의 가장 가까운 점 기준)
        if (interactable != null)
        {
            Vector2 playerPos = player.transform.position;
            float distance = Vector2.Distance(playerPos, hit.ClosestPoint(playerPos));

            if (distance <= InteractRange)
            {
                interactable.OnRInteract();
                return;
            }
        }

        // 멀면 이동 후 도착하면 상호작용
        playerController.MoveToWorld(worldPos, () =>
        {
            if (IsAlive(interactable))
                interactable.OnRInteract();
        });
    }

    // UI 위가 아니면 클릭 위치를 월드 좌표로 반환
    private bool TryGetWorldClick(out Vector2 worldPos)
    {
        worldPos = default;

        if (EventSystem.current.IsPointerOverGameObject())
            return false;

        worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return true;
    }

    // 현재 모드에서 클릭 가능한 대상 종류
    private ClickTarget GetAllowedTargets()
    {
        GameManager gm = GameManager.Instance;

        return (gm.IsMode(GameMode.Build) || gm.IsMode(GameMode.Work))
            ? ClickTarget.Building
            : ClickTarget.All;
    }

    // 클릭 위치에서 해당 클릭(T)을 처리할 수 있는 대상 중 우선순위가 가장 높은 하나를 반환
    // 우선순위, 모드 필터, 실행 대상이 모두 같은 컴포넌트(candidate)를 기준으로 결정됨
    private bool TryGetTarget<T>(Vector2 worldPos, ClickTarget allowed,
                                 out Collider2D hitCollider, out T target)
        where T : class, IClickPriority
    {
        hitCollider = null;
        target = null;

        Physics2D.OverlapPoint(worldPos, clickFilter, hitBuffer);

        int bestPriority = int.MinValue;
        int bestOrder = int.MinValue;

        foreach (Collider2D col in hitBuffer)
        {
            T candidate = col.GetComponentInParent<T>();
            if (candidate == null)
                continue;

            // 현재 모드에서 허용되지 않는 대상은 제외
            if ((candidate.Target & allowed) == 0)
                continue;

            // 같은 우선순위끼리는 화면에서 위에 그려지는 쪽
            int order = col.TryGetComponent(out SpriteRenderer sr) ? sr.sortingOrder : 0;

            if (candidate.ClickPriority > bestPriority ||
                (candidate.ClickPriority == bestPriority && order > bestOrder))
            {
                bestPriority = candidate.ClickPriority;
                bestOrder = order;
                hitCollider = col;
                target = candidate;
            }
        }

        return target != null;
    }

    // 인터페이스 변수는 유니티의 "파괴됨 == null" 판정을 받지 못하므로 직접 확인
    private static bool IsAlive(object target)
    {
        if (target == null)
            return false;

        if (target is UnityEngine.Object unityObject)
            return unityObject != null;   // 파괴된 오브젝트면 false

        return true;   // MonoBehaviour가 아닌 일반 클래스
    }
    #endregion
}