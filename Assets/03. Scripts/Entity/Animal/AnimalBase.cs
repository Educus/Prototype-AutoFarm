using System;
using Unity.Burst.Intrinsics;
using UnityEngine;
using static UnityEditor.Progress;

public abstract class AnimalBase : MonoBehaviour
{
    public int itemId;
    public string id;
    public string animalName;
    public Vector3 position;
    public int productItemID;

    private int workStack;
    public int isStack;
    public bool isReady = false;

    private AnimalType type;

    private void Start()
    {
        TimeManager.Instance.onDayEvent += AddStack;

        ObjectData objData = DataManager.Instance.objectsData[itemId];

        workStack = objData.WorkDuration;
        if (Enum.TryParse(objData.ObjectName, out AnimalType result))
            type = result;
    }

    private void AddStack()
    {
        if (isReady) return;

        isStack += 1;

        if (workStack == isStack)
        {
            isStack = 0;
            isReady = true;
        }
    }

    public int Harvest()
    {
        if (!isReady) return -1;

        int itemID = -1;

        switch (type)
        {
            case AnimalType.COW:
                itemID = 4012;
                break;

            case AnimalType.CHICKEN:
                itemID = -1;
                break;

            case AnimalType.NONE:
            default:
                return -1;
        }

        // 수확 완료 처리
        isReady = false;
        isStack = 0;

        return itemID;
    }

    private void OnMouseDown()
    {
        GameManager.Instance.player
            .GetComponent<PlayerAction>()
            .StartAnimalAction(this);
    }

    #region Save/Load
    public AnimalSaveData GetSaveData()
    {
        position = gameObject.transform.position;

        return new AnimalSaveData
        {
            itemId = this.itemId,
            id = this.id,
            animalName = this.animalName,
            position = this.position,
            isStack = this.isStack,
            isReady = this.isReady
        };
    }
    public void Load(AnimalSaveData data)
    {
        this.itemId = data.itemId;
        this.id = data.id;
        this.animalName = data.animalName;
        this.position = data.position;
        this.isStack = data.isStack;
        this.isReady = data.isReady;
    }
    #endregion
}
