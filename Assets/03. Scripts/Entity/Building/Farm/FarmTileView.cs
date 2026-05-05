using UnityEngine;

public class FarmTileView : MonoBehaviour
{
    // 농장 이미지 애니메이션
    // 이후 추가 수정

    [HideInInspector]
    public int index;
    public Sprite[] field;
    private SpriteRenderer sprite;
    [SerializeField] private SpriteRenderer cropsSprite;

    private void Awake()
    {
        index = transform.GetSiblingIndex();
        sprite = GetComponent<SpriteRenderer>();
    }

    public void UpdateView(int value, Sprite image)
    {
        sprite.sprite = field[value];

        cropsSprite.sprite = image;
    }
}
