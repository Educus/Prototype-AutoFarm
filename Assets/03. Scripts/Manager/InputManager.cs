using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Player player;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            LeftClick();
        }

        if (Input.GetMouseButtonDown(1))
        {
            RightClick();
        }
    }

    void LeftClick()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        var target = InteractionHandler.GetInteractable(worldPos);

        if (target != null)
        {
            // target.Interact(player);
        }
    }

    void RightClick()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        var target = InteractionHandler.GetInteractable(worldPos);

        // player.MoveTo(worldPos, target);
    }
}
