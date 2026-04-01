using System;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    // 골드 관리
    public int gold { get; private set; } = 1000; // 초기 골드
    // NPC 관리
    public int npcCount { get; private set; } = 0; // 초기 NPC 수
    // 창고 관리
    public int storageCapacity { get; private set; } = 100; // 초기 창고 용량

    // ui 관리
    public event Action onSetGold;      // 자동 골드 관리
    public event Action onSetNPC;       // 자동 npc 관리
    public event Action onSetStorage;   // 자동 창고 관리

    private void Start()
    {
        onSetNPC?.Invoke();
        onSetStorage?.Invoke();
        onSetGold?.Invoke();
    }
    /*
     dadasfklaflakdn flndkl
     ssssssssssssssssssssssssssssssssssssssssssss
     
     */
}
