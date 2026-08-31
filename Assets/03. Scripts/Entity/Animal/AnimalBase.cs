using System;
using UnityEngine;

public abstract class AnimalBase : MonoBehaviour, IRightInteractable
{
    // 이걸 부모로 쓰는 동물들
    // 동물 타입만 연결해주면 모두 연결, 행동 가능하게 만들기
    // 추가한다면 동물 설명?

    [HideInInspector] public int itemId;         // 아이디
    [HideInInspector] public string id;          // 아이디
    [HideInInspector] public string animalName;  // 동물 이름

    public AnimalType type;     // 동물 타입
    public int productItemID;   // 생산 아이템
    [Tooltip("생산시간(10분 단위)")]
    public int productionTime = 60;  // 생산 시간

    private int remainingTime;

    private int workStack;
    [HideInInspector] public int isStack;
    public bool isReady = false;


    // 상호작용
    public void OnInteract(Player player)
    {
        Debug.Log("동물 클릭 됨");

        // 생산품 있는 상태에서 플레이어 상호작용 시 생산
        GameManager.Instance.player
            .GetComponent<PlayerAction>()
            .StartAnimalAction(this);
    }

    public void Initialize(string animalID)
    {
        id = animalID;
        remainingTime = productionTime;

        gameObject.name = animalID;

        DataManager.Instance.AnimalManager.Register(this);

        TimeManager.Instance.onMinuteEvent += ReproducingItem;
    }

    private void OnDestroy()
    {
        TimeManager.Instance.onMinuteEvent -= ReproducingItem;
    }

    public bool CanWork()
    {
        // 생산된 품목이 1개 이상이면 작업 가능
        return isStack > 0;
    }

    // 아이템 재생산
    private void ReproducingItem(int minute)
    {
        if (isReady) return;

        remainingTime -= minute;

        if (remainingTime <= 0)
        {
            isStack++;

            if (workStack <= isStack)
            {
                isReady = true;
            }
        }
    }

    public int Harvest()
    {
        if (!isReady) return -1;

        if (type == AnimalType.NONE)
        {

            return -1;
        }
        else
        {
            // 수확 완료 처리
            isReady = false;
            isStack = 0;

            return productItemID;
        }
    }

    // 할당된 건물이 없다면 마음대로 움직임 (추가 예정)


    #region Save/Load
    public AnimalSaveData GetSaveData()
    {
        return new AnimalSaveData
        {
            itemId = this.itemId,
            id = this.id,
            animalName = this.animalName,
            isStack = this.isStack,
            isReady = this.isReady,

            position = transform.position
        };
    }
    public void Load(AnimalSaveData data)
    {
        this.itemId = data.itemId;
        this.id = data.id;
        this.animalName = data.animalName;
        this.isStack = data.isStack;
        this.isReady = data.isReady;

        transform.position = data.position;
    }
    #endregion
}
