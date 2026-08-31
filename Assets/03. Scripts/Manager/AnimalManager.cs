using System.Collections.Generic;
using UnityEngine;
using static NPCManager;

public enum AnimalType
{
    NONE,
    COW,
    CHICKEN
}
public class AnimalManager : MonoBehaviour
{
    // 모든 동물 정보
    public Dictionary<string, AnimalBase> animals = new Dictionary<string, AnimalBase>();
    // 할당되지 않은 동물 정보(아이디 값만)
    public Queue<string> animalQueue = new Queue<string>();

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

    // 동물 정보 저장
    public void Register(AnimalBase animal)
    {
        if (!animals.ContainsKey(animal.id))
        {
            animals.Add(animal.id, animal);
            animalQueue.Enqueue(animal.id);

            AssignAnimalToBuilding();
        }
    }

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

        Debug.LogWarning($"Animal with ID '{id}' not found.");

        return null;
    }

    // 동물을 건물에 자동 할당(1.동물이 생겼을 때, 2.건물이 생겼을 때)
    public void AssignAnimalToBuilding()
    {
        // 동물이 없으면 할당하지 않음
        if (animalQueue.Count == 0)
        {
            Debug.Log("No unassigned animals available.");
            return;
        }

        // 동물에게 이미 건물이 할당되어 있다면 패스



        // Ranch 건물 가져오기
        List<RanchBuilding> ranchBuildings =
            DataManager.Instance.BuildingManager
                .GetBuildingsByType<RanchBuilding>(BuildingType.Ranch);

        // Ranch 건물이 없으면 할당하지 않음
        if (ranchBuildings.Count == 0)
        {
            Debug.Log("No Ranch buildings available.");
            return;
        }

        // 빈 건물이 없으면 할당하지 않음



        string animalID = animalQueue.Dequeue();
        AnimalBase animal = Get(animalID);
    }




    // Dictionary -> List 변환
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
    // Inspector 실시간 갱신
    private void Update()
    {
        RefreshDebugList();
    }
#endif
}
