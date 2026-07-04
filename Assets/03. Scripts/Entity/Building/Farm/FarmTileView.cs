using UnityEngine;

public class FarmTileView : MonoBehaviour, IRightInteractable
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

        if (sprite == null)
        {
            Debug.LogError($"{name}에는 SpriteRenderer가 없습니다.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UpdateView(0, null);
        }
    }
    public void UpdateView(int value, Sprite image)
    {
        if (sprite == null)
        {
            Debug.LogError($"sprite가 사라짐. index : {transform.parent.gameObject.name}_{index}");
            return;
        }

        if (cropsSprite == null)
        {
            Debug.LogError($"cropsSprite가 사라짐. index : {index}");
            return;
        }

        sprite.sprite = field[value];

        cropsSprite.sprite = image;
    }

    public void OnInteract(Player player)
    {
        FarmBuilding farm = 
            gameObject.GetComponentInParent<FarmBuilding>();

        FarmTile tile =
            farm.tiles[index - 1];

        player.GetComponent<PlayerAction>().StartFarmAction(farm, tile);
    }
}
