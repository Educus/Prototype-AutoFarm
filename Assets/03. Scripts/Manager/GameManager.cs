using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 시스템, 이벤트 호출 담당

    // 게임 모드 관리
    public bool isBuildMode = false;
    public bool isWorkMode = false;

    // 일시정지
    public bool isPlay = false;

    // 카메라 타겟
    public GameObject targetLock;

    public static GameManager Instance;

    // Save/Load
    public event Action save;
    public event Action load;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPlay = !isPlay;

            Debug.Log(isPlay ? "게임 시작" : "게임 일시정지");
        }
    }
    private void Update()
    {
        if (targetLock == null) return;
        else
        {
            Debug.Log($"타겟 대상 : {targetLock.name}");
        }
    }

}
