using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RanchBuilding : BuildingBase
{
    public List<AnimalBase> animals = new List<AnimalBase>();

    // 동물 배치 위치
    [SerializeField]
    private List<Transform> animalPositions = new List<Transform>();

    // 농장에 작업이 생겼을 때 호출
    public event Action<RanchBuilding> onWorkRequested;


    #region Unity

    protected override void Awake()
    {
        base.Awake();

        type = BuildingType.Ranch;
    }

    #endregion


    #region NPC 참조 영역

    // 플레이어
    // 건물에 직접 상호작용 없음
    // 동물에게 직접 상호작용

    // NPC
    // 수확 상호작용 하나 있음
    // 농장 참조하여 동물이 수확 상태가 되면 알림

    // 작업이 가능한가
    public bool HasAnimalWork()
    {
        return animals.Any(animal =>
            animal != null &&
            animal.IsReady());
    }

    // 작업 가능한 동물 가져오기
    public List<AnimalBase> GetWorkableAnimals()
    {
        return animals.FindAll(animal =>
            animal != null &&
            animal.IsReady());
    }

    // 작업 요청
    public void RequestWork()
    {
        onWorkRequested?.Invoke(this);
    }

    #endregion


    #region 동물 추가 / 제거

    // 동물 추가 가능 여부
    public bool CanAddAnimal()
    {
        return animals.Count < animalPositions.Count;
    }

    // 동물을 목장에 추가
    public bool AddAnimal(AnimalBase animal)
    {
        return AddAnimal(animal, true);
    }

    private bool AddAnimal(AnimalBase animal, bool notify)
    {
        if (animal == null)
            return false;

        // 이미 등록되어 있다면 추가하지 않음
        if (animals.Contains(animal))
            return false;

        // 배치할 위치가 없는 경우
        if (animals.Count >= animalPositions.Count)
        {
            Debug.LogWarning(
                $"[{name}] 목장의 동물 배치 공간이 부족합니다."
            );

            return false;
        }

        animals.Add(animal);
        animal.ranchBuilding = this;    // 배정되면 배회 코루틴 멈춤

        // 고정 위치 배치 (월드 좌표 기준)
        Transform targetPosition =
            animalPositions[animals.Count - 1];

        animal.transform.position =
            targetPosition.position;

        animal.transform.rotation =
            targetPosition.rotation;

        // 동물이 수확 가능한 상태라면 작업 요청
        if (notify && animal.IsReady())
        {
            RequestWork();
        }

        return true;
    }

    // 목장에서 동물을 제거
    // 일반적인 상황에서는 동물을 제거하지 않음.
    // 목장이 파괴될 때 ReleaseAnimals()를 통해
    // AnimalManager의 미배정 Queue로 이동함.

    #endregion

    #region 목장 파괴

    // 목장이 파괴될 때 동물을 미배정 상태로 변경
    //
    // 동물은 RanchBuilding의 자식이 아니므로
    // SetParent(null)이 필요하지 않음.
    //
    // BuildingManager에서 실제 Destroy() 전에
    // ReleaseAnimals()를 먼저 호출해야 함.

    public override void OnBeforeDestroy()
    {
        // 동물을 AnimalManager의 미배정 Queue로 돌려보냄
        // (다른 목장에 자리가 있으면 바로 그쪽으로 배정되고, 없으면 돌아다니기 시작함)
        ReleaseAnimals();
    }

    public void ReleaseAnimals()
    {
        // 리스트를 복사해서 순회
        List<AnimalBase> releaseList =
            new List<AnimalBase>(animals);

        animals.Clear();

        foreach (AnimalBase animal in releaseList)
        {
            if (animal == null)
                continue;

            animal.ranchBuilding = null;   // 배정 해제

            // AnimalManager의 미배정 Queue로 이동
            DataManager.Instance.AnimalManager
                .EnqueueAnimal(animal);
        }
    }

    #endregion


    public override void OnRInteract()
    {
        Debug.Log("RanchBuilding OnRInteract");
    }
}