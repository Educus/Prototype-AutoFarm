using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class SaveLoadManager : MonoBehaviour
{
    // 맵, 오브젝트, 아이템 저장 및 불러오기
    // 아이템 시세 불러오기 저장하기
    // Json파일 읽기 쓰기

    private TextAsset jsonFile;

    private void Awake()
    {
        // 게임 실행 시 origin파일 불러오기

        jsonFile = Resources.Load<TextAsset>("Json/Save/");
    }

    // 이어하기
    public void ContinueGame()
    {
        // 이어하기 시 저장된 파일 불러오기
    }

    // 저장
    public void SaveJson()
    {
        // 저장 할 내용
        // 플레이어 (스탯, 위치, 인벤토리)
        // NPC (스탯, 위치, 인벤토리)
        // 맵 (날짜, 시간)
        // 작물 (종류, 성장 단계, 위치)
        // 아이템 (시세, 전일 종가 * 5)


    }

    // 로드
    public void LoadCrops()
    {
    }

    // 저장 내용 출력
    public void PrintAll()
    {
    }
}
