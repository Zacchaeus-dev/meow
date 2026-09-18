using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerInventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private GameObject panel;

    [Header("Slots")]
    [SerializeField] private Button[] slotButtons;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

    [Header("Water Bucket")]
    [SerializeField] private GameObject waterPickupPrefab;
    [SerializeField] private float waterPlaceRange = 5f;

    private bool isOpen;
    private TMP_Text[] slotLabels;
    private object[] boundItems; // holds either a Rune or a RuneAbility per slot. [Model-View-Presenter / Model-View Uncoupling]

    
    private void Start()
    {
        panel.SetActive(false);

        boundItems = new object[slotButtons.Length];
        slotLabels = new TMP_Text[slotButtons.Length];

        for (int i = 0; i < slotButtons.Length; i++)
        {
            slotLabels[i] = slotButtons[i].GetComponentInChildren<TMP_Text>();

            // Data Binding
            int index = i;
            slotButtons[i].onClick.AddListener(() => OnSlotClicked(index));
        }

        if (playerInventory == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerInventory = playerObj.GetComponent<PlayerInventory>();
            }
        }

        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += RefreshSlots; // [Observer Pattern]
        }
    }

   
    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= RefreshSlots; // [Observer Pattern]
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isOpen = !isOpen;
            panel.SetActive(isOpen);

            if (isOpen)
            {
                RefreshSlots();
            }
        }
    }

    // Refresh the UI slots based on the player's inventory
    private void RefreshSlots()
    {
        if (playerInventory == null) return;

        int slotIndex = 0;

        // Runes first
        foreach (Rune rune in playerInventory.Runes)
        {
            if (slotIndex >= slotButtons.Length) break;
            boundItems[slotIndex] = rune;
            slotLabels[slotIndex].text = rune.RuneType.ToString();
            slotIndex++;
        }

        // Clear remaining empty slots
        for (int i = slotIndex; i < slotButtons.Length; i++)
        {
            boundItems[i] = null;
            slotLabels[i].text = "";
        }
    }

    private void OnSlotClicked(int index)
    {
        object item = boundItems[index];

        if (item is Rune rune)
        {
            if (rune.RuneType == RuneType.WATER)
            {
                PlaceWater(rune);
            }
            else
            {
                Debug.Log($"Clicked {rune.RuneType} rune (no action yet).");
            }
        }
    }

    private void PlaceWater(Rune rune)
    {
        if (!rune.IsFilled)
        {
            Debug.Log("Bucket is empty - nothing to place.");
            return;
        }

        Transform player = playerInventory.transform;

        // pick a point out in front of the player, along the ground plane
        Vector3 targetXZ = player.position + player.forward * waterPlaceRange;

        // raycast straight down from above that point to find the actual ground surface
        Vector3 downOrigin = targetXZ + Vector3.up * 5f;
        Debug.DrawRay(downOrigin, Vector3.down * 10f, Color.cyan, 3f);

        if (Physics.Raycast(downOrigin, Vector3.down, out RaycastHit hit, 10f))
        {
            Instantiate(waterPickupPrefab, hit.point, Quaternion.identity);
            rune.Empty();
            Debug.Log($"Placed water at {hit.point}, hit object: {hit.collider.name}");
            RefreshSlots();
        }
        else
        {
            Debug.Log($"No ground found below target point: {targetXZ}");
        }
    }
}