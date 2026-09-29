using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AltarMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Altar altar;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Transform runeListContent;
    [SerializeField] private Button runeButtonPrefab;
    [SerializeField] private TMP_Text slot1Text;
    [SerializeField] private TMP_Text slot2Text;
    [SerializeField] private Button closeButton;

    private PlayerInventory currentInventory;

    private Altar activeAltar;

    private void Start()
    {
        menuPanel.SetActive(false);

        Altar[] allAltars = FindObjectsByType<Altar>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (Altar altar in allAltars)
        {
            altar.OnMenuOpened += (inventory) => HandleMenuOpened(altar, inventory);
        }

        closeButton.onClick.AddListener(() => activeAltar?.CloseMenu());

        //altar.OnMenuOpened += HandleMenuOpened;
        //altar.OnMenuClosed += HandleMenuClosed;
        //altar.OnSlotsChanged += RefreshUI;

        //closeButton.onClick.AddListener(() => altar.CloseMenu());
    }

    private void OnDestroy()
    {
        //altar.OnMenuOpened -= HandleMenuOpened;
        //altar.OnMenuClosed -= HandleMenuClosed;
        //altar.OnSlotsChanged -= RefreshUI;

        if (activeAltar != null)
        {
            UnsubscribeFrom(activeAltar);
        }
    }

    private void HandleMenuOpened(Altar altar, PlayerInventory inventory) //PlayerInventory inventory
    {

        if (activeAltar != null && activeAltar != altar)
        {
            UnsubscribeFrom(activeAltar);
        }

        activeAltar = altar;
        currentInventory = inventory;

        activeAltar.OnMenuClosed += HandleMenuClosed;
        activeAltar.OnSlotsChanged += RefreshUI;

        menuPanel.SetActive(true);
        RefreshUI();
        //currentInventory = inventory;
        //menuPanel.SetActive(true);
        //RefreshUI();
    }

    private void HandleMenuClosed()
    {
        //menuPanel.SetActive(false);
        if (activeAltar != null)
        {
            UnsubscribeFrom(activeAltar);
        }

        activeAltar = null;
        menuPanel.SetActive(false);
    }

    private void UnsubscribeFrom(Altar altar)
    {
        altar.OnMenuClosed -= HandleMenuClosed;
        altar.OnSlotsChanged -= RefreshUI;
    }

    private void RefreshUI()
    {
        // Clear old buttons
        //foreach (Transform child in runeListContent)
        //{
        //    Destroy(child.gameObject);
        //}

        //if (currentInventory != null)
        //{
        //    foreach (Rune rune in currentInventory.Runes)
        //    {
        //        Button btn = Instantiate(runeButtonPrefab, runeListContent);
        //        btn.GetComponentInChildren<TMP_Text>().text = rune.RuneType.ToString();
        //        btn.onClick.AddListener(() => altar.TryPlaceRune(rune));
        //    }
        //}

        //slot1Text.text = altar.Slot1 != null ? altar.Slot1.RuneType.ToString() : "Empty";
        //slot2Text.text = altar.Slot2 != null ? altar.Slot2.RuneType.ToString() : "Empty";

        foreach (Transform child in runeListContent)
        {
            Destroy(child.gameObject);
        }

        if (currentInventory != null)
        {
            foreach (Rune rune in currentInventory.Runes)
            {
                Button btn = Instantiate(runeButtonPrefab, runeListContent);
                btn.GetComponentInChildren<TMP_Text>().text = rune.RuneType.ToString();
                btn.onClick.AddListener(() => activeAltar?.TryPlaceRune(rune));
            }
        }

        if (activeAltar != null)
        {
            slot1Text.text = activeAltar.Slot1 != null ? activeAltar.Slot1.RuneType.ToString() : "Empty";
            slot2Text.text = activeAltar.Slot2 != null ? activeAltar.Slot2.RuneType.ToString() : "Empty";
        }
    }
}