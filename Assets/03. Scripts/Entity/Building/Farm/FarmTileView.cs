using UnityEngine;

public class FarmTileView : MonoBehaviour
{
    // 농장 이미지 애니메이션
    // 이후 추가 수정

    public int index;
    public Animator animator;

    public void UpdateView(FarmTile tile, int seedID)
    {
        if (!tile.hasCrop)
        {
            animator.Play("Empty");
            return;
        }

        // 물 여부 이미지
        if (tile.watered)
            animator.Play("Wet_Land");
        else
            animator.Play("Dry_Land");

        // 씨앗 별 이미지
        int grow = (int)(tile.growth * DataManager.Instance.productsData[seedID].growthTime);
        animator.Play($"{seedID}_{grow}");
    }
}
