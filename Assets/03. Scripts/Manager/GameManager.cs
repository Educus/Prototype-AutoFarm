using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 시스템, 이벤트 호출 담당

    // 게임 모드 관리
    public bool isBuildMode = false;

    // 일시정지
    public bool isPlay = false;

    private void Start()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPlay = !isPlay;

            Debug.Log(isPlay ? "게임 시작" : "게임 일시정지");
        }
    }
}
