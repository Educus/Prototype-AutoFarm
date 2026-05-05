using Unity.VisualScripting;

[System.Serializable]
public class FarmTile
{
    public bool hasCrop;
    public int cropID;

    public bool watered;
    public float growth;

    // 수확 여부
    public bool IsReady()
    {
        return hasCrop && growth >= 1f;
    }

    // 작물 심기 여부
    public bool CanPlant()
    {
        return !hasCrop;
    }

    // 작물
    public void Plant(int seedID)
    {
        hasCrop = true;
        cropID = seedID + 1;
        growth = 0f;
        watered = false;
    }

    // 물 여부
    public void Water()
    {
        watered = true;
    }

    // 성장 및 물 초기화
    public void Grow()
    {
        if (watered)
        {
            growth += 1 / DataManager.Instance.productsData[cropID].growthTime;
            watered = false;
        }
    }

    // 리턴 작물
    public int Harvest()
    {
        int result = cropID;

        hasCrop = false;
        cropID = 0;
        watered = false;
        growth = 0f;

        return result;
    }
}