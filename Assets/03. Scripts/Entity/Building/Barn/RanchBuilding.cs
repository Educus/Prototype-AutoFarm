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
        if (animal == null)
            return false;

        if (animals.Contains(animal))
            return false;

        if (!CanAddAnimal())
            return false;

        // 리스트에 추가
        animals.Add(animal);

        // 현재 인덱스에 해당하는 위치 사용
        int index = animals.Count - 1;

        Transform targetPosition =
            animalPositions[index];

        // 월드 좌표 기준으로 배치
        animal.transform.position =
            targetPosition.position;

        animal.transform.rotation =
            targetPosition.rotation;

        // 이미 작업 가능한 상태라면 NPC에게 알림
        if (animal.IsReady())
        {
            RequestWork();
        }

        return true;
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

        // 고정 위치 배치
        Transform targetPosition =
            animalPositions[animals.Count - 1];

        // 월드 좌표 기준
        animal.transform.position =
            targetPosition.position;

        animal.transform.rotation =
            targetPosition.rotation;

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


    #region 동물 위치

    // 목장의 모든 동물을 지정된 위치에 다시 배치
    private void RefreshAnimalPositions()
    {
        for (int i = 0; i < animals.Count; i++)
        {
            if (animals[i] == null)
                continue;

            if (i >= animalPositions.Count)
                continue;

            // 월드 좌표 기준
            animals[i].transform.position =
                animalPositions[i].position;

            animals[i].transform.rotation =
                animalPositions[i].rotation;
        }
    }

    #endregion


    #region 목장 파괴

    // 목장이 파괴될 때 동물을 미배정 상태로 변경
    //
    // 동물은 RanchBuilding의 자식이 아니므로
    // SetParent(null)이 필요하지 않음.
    //
    // BuildingManager에서 실제 Destroy() 전에
    // ReleaseAnimals()를 먼저 호출해야 함.

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

            // AnimalManager의 미배정 Queue로 이동
            DataManager.Instance.AnimalManager
                .EnqueueAnimal(animal);
        }
    }

    #endregion


    public override string GetJsonData()
    {
        throw new System.NotImplementedException();
    }

    public override void LoadJsonData(string json)
    {
        throw new System.NotImplementedException();
    }

    public override void OnInteract()
    {
        // 상호작용 없음
    }
}