using System;
using System.Collections;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    [SerializeField] private float workTime = 5f;

    private Player player;
    private PlayerController controller;

    private FarmBuilding targetFarm;
    private FarmTile targetTile;

    private AnimalBase targetAnimal;

    //애니메이션 스크립트 참조
    private PlayerAnimation playerAnimation;

    public bool IsWorking { get; private set; }

    private void Awake()
    {
        player = GetComponent<Player>();
        controller = GetComponent<PlayerController>();

        //애니메이션 스크립트 초기화
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    #region 공통
    private IEnumerator IEWorkRoutine(Func<bool> canWork, Action work)
    {
        if (!canWork())
            yield break;

        IsWorking = true;

        controller.SetCanMove(false);
        controller.SetWorking(true);

        //작업 애니메이션 시작
        playerAnimation.IsWorking = true;

        yield return new WaitForSeconds(workTime);

        //작업 애니메이션 종료
        playerAnimation.IsWorking = false;

        work?.Invoke();

        controller.SetWorking(false);
        controller.SetCanMove(true);

        IsWorking = false;
    }
    #endregion

    #region Farm

    public void StartFarmAction(
        FarmBuilding farm,
        FarmTile tile)
    {
        if (IsWorking)
            return;

        if (!tile.CanWork(player))
            return;

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
            () =>
            {
                StartCoroutine(IEWorkRoutine(() => targetTile.CanWork(player), ExecuteFarmAction));
            });
    }

    private void ExecuteFarmAction()
    {
        Debug.Log("농장 작업 실행");

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
            targetFarm.TryWater(targetTile);
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
        if (IsWorking)
            return;

        targetAnimal = animal;

        Vector2Int target =
            GridManager.Instance.WorldToGrid(
                animal.transform.position);

        controller.MoveTo(
            target,
            () =>
            {
                StartCoroutine(IEWorkRoutine(() => targetAnimal.CanWork(), ExecuteAnimalAction));
            });
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