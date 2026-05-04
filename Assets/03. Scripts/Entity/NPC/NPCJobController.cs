using UnityEngine;

public class NPCJobController : MonoBehaviour
{
    public NPC npc;

    public float interval = 1f;
    private float timer;

    private void Awake()
    {
        npc = GetComponent<NPC>();
    }
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            ProcessJob();
        }
    }

    void ProcessJob()
    {
        if (!npc.job.IsValid())
            return;

        switch (npc.job.jobType)
        {
            case JobType.Farm:
                ProcessFarm();
                break;

            case JobType.Ranch:
                // TODO: 축사 작업 (비워둠)
                break;
        }
    }

    // =========================
    // FARM
    // =========================

    void ProcessFarm()
    {
        switch (npc.job.step)
        {
            case JobStep.Idle:
                npc.job.step = JobStep.MoveToStorage;
                break;

            case JobStep.MoveToStorage:

                if (npc.isMoving) return;

                Vector2Int storagePos = DataManager.Instance
                    .BuildingManager
                    .GetStoragePosition();

                npc.MoveTo(storagePos);

                npc.job.step = JobStep.TakeResource;

                break;

            case JobStep.TakeResource:
                int need = 9;

                int taken = DataManager.Instance.InventoryManager
                    .Get("storage")
                    .TakeUpTo(npc.job.productItemID, need);

                if (taken > 0)
                {
                    npc.subInventory.AddItem(npc.job.productItemID, taken);
                    npc.job.step = JobStep.MoveToBuilding;
                }
                else
                {
                    npc.job.step = JobStep.Waiting;
                }
                break;

            case JobStep.MoveToBuilding:
                npc.job.step = JobStep.Work;
                break;

            case JobStep.Work:
                DoFarmWork();
                break;

            case JobStep.Waiting:
                if (DataManager.Instance.InventoryManager
                    .GetTotalItemCount(npc.job.productItemID) > 0)
                {
                    npc.job.step = JobStep.MoveToStorage;
                }
                break;
        }
    }

    void DoFarmWork()
    {
        foreach (var buildingID in npc.job.buildingIDs)
        {
            var farm = DataManager.Instance.BuildingManager.Get<FarmBuilding>(buildingID);

            // 씨앗 심기
            var plantTiles = farm.GetPlantableTiles();

            foreach (var tile in plantTiles)
            {
                if (npc.subInventory.TakeUpTo(npc.job.productItemID, 1) <= 0)
                {
                    npc.job.step = JobStep.MoveToStorage;
                    return;
                }

                farm.TryPlant(tile, npc.job.productItemID);
            }

            // 물주기
            if (npc.HasUpgrade(999)) // 물 업그레이드
            {
                farm.WaterAll();
            }
            else
            {
                var waterTiles = farm.GetWaterableTiles();

                foreach (var tile in waterTiles)
                {
                    farm.Water(tile);
                }
            }

            // 수확
            var harvestTiles = farm.GetHarvestableTiles();

            foreach (var tile in harvestTiles)
            {
                int item = farm.TryHarvest(tile);

                if (item > 0)
                {
                    npc.AddItemToInventory(item, 1);
                }
            }
        }

        npc.job.step = JobStep.Idle;
    }


}