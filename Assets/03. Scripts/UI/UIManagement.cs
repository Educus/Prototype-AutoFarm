using System.Collections;
using UnityEngine;

public class UIManagement : MonoBehaviour
{
    [SerializeField] private GameObject management;
    [SerializeField] public DataManager dataManager;

    [SerializeField] private GameObject[] chips0;
    [SerializeField] private GameObject[] chips1;
    [SerializeField] private GameObject[] chips2;

    [SerializeField] private GameObject[] icon0;
    [SerializeField] private GameObject[] icon1;
    [SerializeField] private GameObject[] icon2;
    [SerializeField] private GameObject[] icon3;

    [SerializeField] private GameObject[] chipMenu;

    [SerializeField] private GameObject coiceB;


    private GameObject[][] chips;
    private GameObject[][] icons;

    private int onChips;

    private void Start()
    {
        chips = new GameObject[][] { chips0, chips1, chips2 };
        icons = new GameObject[][] { icon0, icon1, icon2, icon3 };

        foreach (var chip in chips)
        {
            chip[0].SetActive(true);
            chip[1].SetActive(false);
        }

        foreach (var icon in icons)
        {
            icon[0].SetActive(true);
            icon[1].SetActive(false);
            icon[2].SetActive(false);
        }

        ChangeChip(0);
        ChangeIcon(0);

        StartCoroutine(IEStart());
    }

    IEnumerator IEStart()
    {
        yield return null;

        ChangeIcon(0);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            management.SetActive(!management.activeSelf);
        }
    }

    public void OpenManagement()
    {
        management.SetActive(!management.activeSelf);
    }

    public void ChangeChip(int value)
    {
        onChips = value;

        for (int i = 0; i < chips.Length; i++)
        {
            bool isActive = (i == value);

            chips[i][0].SetActive(!isActive);
            chips[i][1].SetActive(isActive);

            foreach (var icon in icons)
            {
                icon[i].SetActive(isActive);
            }

            chipMenu[i].SetActive(isActive);

            ChangeIcon(0);
        }
    }

    public void ChangeIcon(int value)
    {
        int count = chipMenu[onChips].transform.childCount;

        for (int i = 0; i < count; i++)
        {
            chipMenu[onChips].transform.GetChild(i).gameObject.SetActive(i == value);
        }

        coiceB.transform.position = icons[value][0].transform.position;
    }

    public void ExitButton()
    {
        management.SetActive(false);
    }
}
