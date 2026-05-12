using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 시스템, 이벤트 호출 담당

    // 게임 모드 관리
    public bool isPopUpMode = false;
    public bool isBuildMode = false;
    public bool isWorkMode = false;
    public NPC selectedNPC;

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
        // 작업 할당 시 건물 색깔 표시
        RefreshHighlights();
    }
    public void RefreshHighlights()
    {
        foreach (var b in DataManager.Instance.BuildingManager.GetAll())
        {
            b.UpdateHighlight();
        }
    }
}
