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

    private bool canMove = true;

    public bool CanMove => canMove;

    // 이동 중인지 확인(PlayerActiom.IsHandling에서 사용) <- 이동 중이면 다른 액션 중복 수행 불가
    public bool IsAutoMoving => moveCoroutine != null;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        //초기화
        playerAnimation = GetComponent<PlayerAnimation>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        SFX = GetComponent<AudioSource>();
    }

    #region 클릭 이동
    public void Move(List<Node> path, System.Action onComplete = null)
    {
        if (!canMove)
            return;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        soundManager.PlaySFX("SFX_GUI_Button", SFX, true);
        moveCoroutine = StartCoroutine(MoveAlongPath(path, onComplete));
    }

    IEnumerator MoveAlongPath(List<Node> path, System.Action onComplete)
    {
        //이동 명령 하달 시에 상태 전달
        playerAnimation.IsMoving = true;

        foreach (Node node in path)
        {
            Vector3 target = gridManager.GetStandPosition(node.x, node.y);

            while (Vector3.Distance(transform.position, target) > 0.05f)
            {
                Vector2 direction = (target - transform.position).normalized;

                //애니메이션 스크립트에 이동 방향 전달
                playerAnimation.MoveDirection = direction;

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

        //이동 종료 상태 전달
        playerAnimation.IsMoving = false;
        soundManager.StopSFX(SFX);

        moveCoroutine = null;
        onComplete?.Invoke();
    }

    public void MoveTo(Vector2Int target, System.Action onComplete = null)
    {
        if (!canMove)
            return;

        // 현재 칸 위치를 매번 계산
        Vector2Int currentPos = gridManager.CharacterToGrid(transform.position);

        List<Node> path = pathfinder.FindPath(currentPos, target);

        if (path == null)
            return;

        if (path.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        Move(path, onComplete);
    }

    public void MoveToWorld(Vector2 worldPos, System.Action onComplete = null)
    {
        if (!canMove)
            return;

        Vector2Int currentPos =
            gridManager.CharacterToGrid(transform.position);

        Vector2Int targetPos =
            gridManager.WorldToGrid(worldPos);

        List<Node> path =
            pathfinder.FindPath(currentPos, targetPos);

        if (path == null || path.Count == 0)
            return;

        Move(path, onComplete);
    }

    // 진행 중인 클릭 이동을 취소
    private void CancelAutoMove()
    {
        if (moveCoroutine == null)
            return;

        StopCoroutine(moveCoroutine);
        moveCoroutine = null;

        playerAnimation.IsMoving = false;
        soundManager.StopSFX(SFX);
    }
    #endregion

    #region 키보드 이동
    private Vector2 keyboardDir;
    private bool isKeyboardMoving;

    // InputManager에서 호출
    public void SetKeyboardInput(Vector2 dir)
    {
        // 대각선이 빨라지지 않도록 정규화 (W+D = (1,1) -> 길이 1)
        keyboardDir = dir.sqrMagnitude > 1f ? dir.normalized : dir;
    }

    private void Update()
    {
        if (!canMove || keyboardDir == Vector2.zero)
        {
            if (isKeyboardMoving)
                StopKeyboardMove();

            return;
        }

        KeyboardMove();
    }

    private void KeyboardMove()
    {
        // 키보드 입력이 우선: 클릭 이동(과 도착 콜백)을 취소
        CancelAutoMove();

        Vector2 pos = transform.position;
        float step = speed * Time.deltaTime;

        Vector2 next = pos + keyboardDir * step;

        // 막혔으면 벽을 따라 미끄러지도록 X, Y를 따로 시도
        if (!IsWalkableWorld(next))
        {
            Vector2 nextX = pos + new Vector2(keyboardDir.x, 0f) * step;
            Vector2 nextY = pos + new Vector2(0f, keyboardDir.y) * step;

            if (IsWalkableWorld(nextX)) next = nextX;
            else if (IsWalkableWorld(nextY)) next = nextY;
            else next = pos;
        }

        if (next == pos)
        {
            if (isKeyboardMoving)
                StopKeyboardMove();

            return;
        }

        if (!isKeyboardMoving)
            StartKeyboardMove();

        playerAnimation.MoveDirection = keyboardDir;
        transform.position = new Vector3(next.x, next.y, transform.position.z);
    }

    private void StartKeyboardMove()
    {
        isKeyboardMoving = true;
        playerAnimation.IsMoving = true;
        soundManager.PlaySFX("SFX_GUI_Button", SFX, true);
    }

    private void StopKeyboardMove()
    {
        isKeyboardMoving = false;
        playerAnimation.IsMoving = false;
        soundManager.StopSFX(SFX);
    }

    private bool IsWalkableWorld(Vector2 worldPos)
    {
        Vector2Int cell = gridManager.CharacterToGrid(worldPos);

        // 현재 서 있는 칸 안에서의 이동은 허용 (막힌 칸 위에서 갇히는 것 방지)
        if (cell == gridManager.CharacterToGrid(transform.position))
            return true;

        return gridManager.IsWalkable(cell.x, cell.y);
    }
    #endregion

    #region 상태 / 불러오기

    public void SetCanMove(bool value)
    {
        canMove = value;

        // 작업 시작처럼 이동이 막히는 순간 소리/애니메이션이 남지 않게 정리
        if (!value && isKeyboardMoving)
            StopKeyboardMove();
    }

    public void SetWorking(bool working)
    {
        animator.SetBool("isWorking", working);
    }

    // 불러오기 등으로 위치를 직접 지정할 때 사용 (진행 중인 이동은 모두 끊음)
    public void TeleportTo(Vector3 savedPos)
    {
        CancelAutoMove();

        if (isKeyboardMoving)
            StopKeyboardMove();

        keyboardDir = Vector2.zero;

        // 칸 기준으로 맞춰서 1번/2번 보정 방식과 무관하게 복원
        Vector2Int cell = gridManager.CharacterToGrid(savedPos);
        transform.position = gridManager.GetStandPosition(cell.x, cell.y);

        canMove = true;
    }

    #endregion

    
}