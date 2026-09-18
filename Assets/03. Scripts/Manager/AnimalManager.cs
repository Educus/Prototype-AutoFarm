using System.Collections.Generic;
using UnityEngine;

public enum AnimalType
{
    NONE,
    COW,
    CHICKEN
}

public class AnimalManager : MonoBehaviour
{
    // 모든 동물 정보
    public Dictionary<string, AnimalBase> animals =
        new Dictionary<string, AnimalBase>();

    // 할당되지 않은 동물
    public Queue<string> animalQueue =
        new Queue<string>();


    // Inspector 확인용
    [System.Serializable]
    public class AnimalDebugData
    {
        public string id;
        public AnimalBase animal;
    }

    [SerializeField]
    private List<AnimalDebugData> debugAnimals =
        new List<AnimalDebugData>();


    #region 등록

    // 새로운 동물 등록
    public void Register(AnimalBase animal)
    {
        if (animal == null)
            return;

        if (animals.ContainsKey(animal.id))
            return;

        animals.Add(animal.id, animal);

        EnqueueAnimal(animal);
    }


    // 할당되지 않은 동물로 등록
    public void EnqueueAnimal(AnimalBase animal)
    {
        if (animal == null)
            return;

        // 이미 Queue에 들어있는지 확인
        if (animalQueue.Contains(animal.id))
            return;

        animalQueue.Enqueue(animal.id);

        AssignAnimalToBuilding();
    }

    #endregion


    #region 찾기

    // 동물 정보 찾기
    public AnimalBase Get(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("Animal ID is null or empty.");
            return null;
        }

        if (animals.TryGetValue(id, out AnimalBase animal))
        {
            return animal;
        }

        Debug.LogWarning(
            $"Animal with ID '{id}' not found."
        );

        return null;
    }

    #endregion


    #region 동물 자동 할당

    // 동물을 건물에 자동 할당
    // 1. 동물이 생겼을 때
    // 2. 건물이 생겼을 때
    // 3. 목장이 삭제되어 동물이 돌아왔을 때
    public void AssignAnimalToBuilding()
    {
        // 동물이 없으면 종료
        if (animalQueue.Count == 0)
            return;


        // Ranch 건물 가져오기
        List<RanchBuilding> ranchBuildings =
            DataManager.Instance.BuildingManager
                .GetBuildingsByType<RanchBuilding>(
                    BuildingType.Ranch
                );


        // Ranch가 없으면 대기
        if (ranchBuildings.Count == 0)
        {
            return;
        }


        // 할당 가능한 Ranch가 있는 동안 반복
        while (animalQueue.Count > 0)
        {
            RanchBuilding targetRanch = null;


            // 빈 공간이 있는 Ranch 찾기
            foreach (RanchBuilding ranch in ranchBuildings)
            {
                if (ranch == null)
                    continue;

                if (ranch.CanAddAnimal())
                {
                    targetRanch = ranch;
                    break;
                }
            }


            // 빈 Ranch가 없으면 종료
            if (targetRanch == null)
            {
                break;
            }


            // Queue에서 동물 가져오기
            string animalID = animalQueue.Dequeue();

            AnimalBase animal = Get(animalID);

            if (animal == null)
                continue;


            // Ranch에 동물 추가
            bool added = targetRanch.AddAnimal(animal);

            // 추가 실패하면 다시 Queue에 넣음
            if (!added)
            {
                animalQueue.Enqueue(animalID);
                break;
            }
        }
    }

    #endregion


    #region Debug

    private void RefreshDebugList()
    {
        debugAnimals.Clear();

        foreach (var pair in animals)
        {
            debugAnimals.Add(new AnimalDebugData
            {
                id = pair.Key,
                animal = pair.Value
            });
        }
    }

    #if UNITY_EDITOR
    private void Update()
    {
        RefreshDebugList();
    }
    #endif
    #endregion
}