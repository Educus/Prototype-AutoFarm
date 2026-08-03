using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Pathfinder pathfinder;

    private Coroutine moveCoroutine;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    //애니메이션 스크립트 참조
    private PlayerAnimation playerAnimation;

    //사운드 매니저 참조 및 오디오 소스
    private SoundManager soundManager;
    private AudioSource SFX;

    public float speed = 0.5f;

    private Vector2Int currentGridPos;

    private bool canMove = true;

    public bool CanMove => canMove;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        //초기화
        playerAnimation = GetComponent<PlayerAnimation>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        SFX = GetComponent<AudioSource>();
    }
    void Start()
    {
        currentGridPos = gridManager.WorldToGrid(transform.position);
    }

    public void Move(List<Node> path, System.Action onComplete = null)
    {
        if (!canMove)
            return;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        //animator.SetBool("isMoving", true);
        soundManager.PlaySFX("SFX_GUI_Button", SFX, true);
        moveCoroutine = StartCoroutine(MoveAlongPath(path, onComplete));
    }

    IEnumerator MoveAlongPath(List<Node> path, System.Action onComplete)
    {
        //이동 명령 하달 시에 상태 전달
        playerAnimation.IsMoving = true;
        foreach (Node node in path)
        {
            Vector3 target =
                new Vector3(node.x + 0.5f, node.y, 0);

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                Vector2 direction = (target - transform.position).normalized;

                //애니메이션 스크립트에 이동 방향 전달
                playerAnimation.MoveDirection = direction;

                //animator.SetFloat("DrtX", direction.x);
                //animator.SetFloat("DrtY", direction.y);

                // direction.x == 0에 가까우면 Flip 유지
                //if (direction.x < 0)
                //{
                //    spriteRenderer.flipX = false;
                //}
                //else if (direction.x > 0)
                //{
                //    spriteRenderer.flipX = true;
                //}

                transform.position =
                    Vector3.MoveTowards(
                        transform.position,
                        target,
                        speed * Time.deltaTime
                    );

                yield return null;
            }

            transform.position = target;
        }

        // foreach (Node node in path)
        // {
        //     Vector3 target = new Vector3(node.x + 0.5f, node.y, 0);
        // 
        //     while (Vector3.Distance(transform.position, target) > 0.05f)
        //     {
        //         transform.position = Vector3.MoveTowards(
        //             transform.position,
        //             target,
        //             speed * Time.deltaTime
        //         );
        // 
        //         yield return null;
        //     }
        // 
        //     currentGridPos = new Vector2Int(node.x, node.y);
        // }

        //이동 종료 상태 전달
        playerAnimation.IsMoving = false;
        //animator.SetBool("isMoving", false);
        soundManager.StopSFX(SFX);
        onComplete?.Invoke();
    }

    public Vector2Int GetCurrentGridPos()
    {
        return currentGridPos;
    }

    // public void MoveTo(Vector2Int target, System.Action onComplete = null)
    // {
    //     List<Node> path =
    //         pathfinder.FindPath(
    //             currentGridPos,
    //             target);
    // 
    //     if (path == null || path.Count == 0)
    //         return;
    // 
    //     Move(path, onComplete);
    // }

    public void MoveTo(
        Vector2Int target,
        System.Action onComplete = null)
    {
        if (!canMove)
            return;

        Vector2 currentWorldPos =
            (Vector2)transform.position + Vector2.up * 0.5f;

        Vector2Int currentPos =
            gridManager.WorldToGrid(currentWorldPos);

        List<Node> path =
            pathfinder.FindPath(currentPos, target);

        if (path == null)
            return;

        if (path.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        Move(path, onComplete);

        Debug.Log($"Path Count : {path?.Count}");
    }

    public void MoveToWorld(Vector2 worldPos, System.Action onComplete = null)
    {
        if (!canMove)
            return;

        Vector2 currentWorldPos =
            (Vector2)transform.position + Vector2.up * 0.5f;

        Vector2Int currentPos =
            gridManager.WorldToGrid(currentWorldPos);

        Vector2Int targetPos =
            gridManager.WorldToGrid(worldPos);

        List<Node> path =
            pathfinder.FindPath(currentPos, targetPos);

        if (path == null || path.Count == 0)
            return;

        Move(path, onComplete);
    }
    
    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    public void SetWorking(bool working)
    {
        animator.SetBool("isWorking", working);
    }
}