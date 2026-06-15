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
    public Vector2 moveDirection;
    public bool isMoving;
    public bool isWorking;

    private void Awake()
    {
        init();
    }
    
    // Update is called once per frame
    void Update()
    {

        //이동 관련 함수
        MovingController();

        //작업 관련 함수
        WorkingController();
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
