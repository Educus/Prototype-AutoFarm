using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Animator playerAnimator;
    private SpriteRenderer spriteRenderer;

    //방향 벡터, 이동, 작업은 PlayerController.cs 에서 값을 넘겨주세요.
    //각 작업 별로 시작과 끝에 bool값 조절
    private Vector2 moveDirection;
    private bool isMoving;
    private bool isWorking;

    public Vector2 MoveDirection 
    { 
        get => moveDirection;
        set
        {
            if(Vector2.Distance(moveDirection, value) > 0.001f)
            {
                moveDirection = value;
                MovingController();
            }
        }
    }
    public bool IsMoving
    {
        get => isMoving;
        set
        {
            isMoving = value;
            MovingController();
        }
    }
    public bool IsWorking 
    { 
        get => isWorking;
        set
        {
            isWorking = value;
            WorkingController();
        } 
    }


    private void Awake()
    {
        init();
    }

    //초기화
    private void init()
    {
        playerAnimator = GetComponent<Animator>();
        spriteRenderer = playerAnimator.GetComponentInChildren<SpriteRenderer>();
        moveDirection = Vector2.zero;
        isMoving = false;
        isWorking = false;
    }

    private void MovingController()
    {
        if(playerAnimator == null ||  spriteRenderer == null) return;

        playerAnimator.SetBool("isMoving", isMoving);

        if (isMoving && moveDirection != Vector2.zero)
        {
            playerAnimator.SetFloat("DrtX", moveDirection.x);
            playerAnimator.SetFloat("DrtY", moveDirection.y);

            spriteRenderer.flipX = moveDirection.x > 0f;
        }
    }

    private void WorkingController()
    {
        if (playerAnimator == null || spriteRenderer == null) return;

        playerAnimator.SetBool("isWorking", isWorking);

        if(isWorking && moveDirection != Vector2.zero) spriteRenderer.flipX = moveDirection.x > 0f;

    }
}
