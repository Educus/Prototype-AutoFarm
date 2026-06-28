using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    private Player player;
    private PlayerController controller;

    private FarmBuilding targetFarm;
    private FarmTile targetTile;

    private AnimalBase targetAnimal;

    private void Awake()
    {
        player = GetComponent<Player>();
        controller = GetComponent<PlayerController>();
    }

    #region Farm

    public void StartFarmAction(
        FarmBuilding farm,
        FarmTile tile)
    {
        targetFarm = farm;
        targetTile = tile;

        int index =
            farm.tiles.IndexOf(tile);

        Vector2Int target =
            GridManager.Instance.WorldToGrid(
                farm.tileViews[index]
                .transform.position);

        controller.MoveTo(
            target,
            ExecuteFarmAction);
    }

    private void ExecuteFarmAction()
    {
        // 1. 수확
        if (targetTile.IsReady())
        {
            int item =
                targetFarm.TryHarvest(targetTile);

            if (item > 0)
            {
                player.AddItemToInventory(item, 1);
            }

            ClearFarm();
            return;
        }

        // 2. 심기
        if (targetTile.CanPlant())
        {
            if (player.selectedSubSlotIndex >= 0)
            {
                InventorySlot slot =
                    player.subInventory.slots[
                        player.selectedSubSlotIndex];

                if (!slot.IsEmpty())
                {
                    if (player.subInventory.TakeUpTo(
                        slot.itemID,
                        1) > 0)
                    {
                        targetFarm.TryPlant(
                            targetTile,
                            slot.itemID);
                    }
                }
            }

            ClearFarm();
            return;
        }

        // 3. 물주기
        if (!targetTile.watered)
        {
            targetFarm.Water(targetTile);
        }

        ClearFarm();
    }

    private void ClearFarm()
    {
        targetFarm = null;
        targetTile = null;
    }

    #endregion

    #region Animal

    public void StartAnimalAction(
        AnimalBase animal)
    {
        targetAnimal = animal;

        Vector2Int target =
            GridManager.Instance.WorldToGrid(
                animal.transform.position);

        controller.MoveTo(
            target,
            ExecuteAnimalAction);
    }

    private void ExecuteAnimalAction()
    {
        if (targetAnimal == null)
            return;

        int item =
            targetAnimal.Harvest();

        if (item > 0)
        {
            player.AddItemToInventory(item, 1);
        }

        targetAnimal = null;
    }

    #endregion
}