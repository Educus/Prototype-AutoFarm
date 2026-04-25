using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InteractionUI : MonoBehaviour
{
    public Text nameText;
    public Text statusText;

    IInteractable interactable;

    Coroutine pollingCoroutine;

    public void SetTartget(IInteractable target)
    {
        interactable = target;

        if (pollingCoroutine != null )
        {
            StopCoroutine(pollingCoroutine);
        }

        pollingCoroutine = StartCoroutine(PollingRoutine());
    }

    IEnumerator PollingRoutine()
    {
        while (interactable != null)
        {
            // nameText.text = interactable.GetName();
            // statusText.text = interactable.GetStatus();

            yield return new WaitForSeconds(1f);
        }
    }

    public void Hide()
    {
        interactable = null;
        gameObject.SetActive(false);
    }
}
