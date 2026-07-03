using UnityEngine;

public static class InteractionHandler
{
    public static ILeftInteractable GetInteractable(Vector2 position)
    {
        Collider2D col = Physics2D.OverlapPoint(position);

        if (col == null)
        {
            return null;
        }

        return col.GetComponent<ILeftInteractable>();
    }
}
