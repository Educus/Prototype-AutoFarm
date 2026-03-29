using UnityEngine;

public class InputManager : MonoBehaviour
{
    // 좌클릭 시 UI매니저를 통한 UI 오픈
    // 우클릭 시 플레이어 이동 및 해당 위치에 상호작용 대상이 있을 경우 상호작용
    // 창고, NPC의 경우 UI오픈 // 물탱크, 마른밭의 경우 행동 상호작용
    [SerializeField] private MovingManager movingManager;
    [SerializeField] private InteractionUIManager uiManager;

    [SerializeField] public Player player;

    private Camera camera;

    int layerMask;

    private void Start()
    {
        camera = Camera.main;

        layerMask = LayerMask.GetMask("NPC", "Structure");
    }

    void Update()
    {
        // 좌클릭, 우클릭
        if (Input.GetMouseButtonDown(0))
        {
            LeftClick();
        }

        if (Input.GetMouseButtonDown(1))
        {
            RightClick();
        }
    }

    void LeftClick()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Collider2D hit = Physics2D.OverlapPoint(mousePos, layerMask);

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
}
