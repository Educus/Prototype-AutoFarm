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

    // 불러오기 등으로 진행중인 행동 모두 취소
    public void ResetAction()
    {
        StopAllCoroutines();    // IEWorkRoutine 중지

        IsWorking = false;
        playerAnimation.IsWorking = false;
        controller.SetWorking(false);
        controller.SetCanMove(true);

        targetFarm = null;
        targetTile = null;
        targetAnimal = null;
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

    // 동물을 향해 이동 중이거나 작업 중이면 true
    public bool IsHandling(AnimalBase animal)
    {
        return targetAnimal == animal && (IsWorking || controller.IsAutoMoving);
    }

    public void StartAnimalAction(
        AnimalBase animal)
    {
        if (IsWorking)
            return;

        targetAnimal = animal;

        Vector2Int target =
            GridManager.Instance.CharacterToGrid(animal.transform.position);

        controller.MoveTo(
            target,
            () =>
            {
                StartCoroutine(IEWorkRoutine(() => targetAnimal.IsReady(), ExecuteAnimalAction));
            });
    }

    private void ExecuteAnimalAction()
    {
        if (targetAnimal == null)
            return;

        // 작업 시간이 끝난 시점에 다시 수확 가능 여부 확인
        if (!targetAnimal.IsReady())
        {
            targetAnimal = null;
            return;
        }

        int item = targetAnimal.Harvest(player);

        if (item > 0)
        {
            // 수확 성공했으므로 필요한 아이템 1개 소모
            if (targetAnimal.harvestRequiredItemID >= 0)
            {
                player.subInventory.TakeUpTo(
                    targetAnimal.harvestRequiredItemID,
                    1);
            }

            player.AddItemToInventory(item, 1);
        }

        targetAnimal = null;
    }

    #endregion
}