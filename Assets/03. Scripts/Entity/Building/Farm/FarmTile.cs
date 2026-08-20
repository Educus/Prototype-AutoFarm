using UnityEngine;

[System.Serializable]
public class FarmTile
{
    public bool hasCrop;
    public int cropID;

    public bool watered;
    public float waterTime;

    public float growth;

    // 수확 여부
    public bool IsReady()
    {
        return hasCrop && growth <= 0f;
    }

    // 작물 심기 여부
    public bool CanPlant()
    {
        return !hasCrop;
    }

    // 작업 가능 여부
    public bool CanWork(Player player)
    {
        if (IsReady())
            return true;

        if (hasCrop && !watered)
            return true;

        if (CanPlant())
        {
            InventorySlot slot =
                player.subInventory.slots[player.selectedSubSlotIndex];

            return !slot.IsEmpty() &&
                   DataManager.Instance.itemsData[slot.itemID].itemType == ItemType.Seed;
        }

        return false;
    }

    // 작물 심기
    public void Plant(int seedID)
    {
        hasCrop = true;
        cropID = seedID + 1;
        growth = DataManager.Instance.productsData[cropID].growthTime * 24f * 60f;
        watered = false;
        waterTime = 24f * 60f;
    }

    // 물 여부
    public void Water()
    {
        if (!hasCrop)
            return;

        if (IsReady())
            return;

        watered = true;
        waterTime = 24f * 60f;
    }

    // 성장 및 물 초기화
    public bool UpdateGrowthTime(int minute)
    {
        if (!hasCrop)
            return false;

        if (IsReady())
            return false;

        if (!watered)
            return false;

        float previousGrowth = growth;

        growth -= minute;

        if (growth < 0f)
            growth = 0f;

        waterTime -= minute;

        if (waterTime <= 0f)
        {
            waterTime = 0f;
            watered = false;
        }

        return !Mathf.Approximately(previousGrowth, growth);
    }

    // 리턴 작물
    public int Harvest()
    {
        if (!IsReady())
            return 0;

        int result = cropID;

        hasCrop = false;
        cropID = 0;

        watered = false;
        waterTime = 0f;

        growth = 0f;

        return result;
    }

    // 성장 상태 퍼센티지 리턴
    public int GetGrowthValue()
    {
        if (!hasCrop)
            return 0;

        float maxGrowth =
            DataManager.Instance.productsData[cropID].growthTime * 24f * 60f;

        if (growth <= 0f)
            return 2;

        if (growth <= maxGrowth * 0.5f)
            return 1;

        return 0;
    }
}