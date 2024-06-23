using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Harmony;
using Inventory;
using UnityEngine;
using UnityEngine.UI;

[HarmonyPatch(typeof(Inventory.InventoryUI), nameof(InventoryUI.CreateWindow))]
class InventoryUI_CreateWindow {
    static void Postfix(ref InventoryUI __result) {
        // InventoryUI.CurrentWindow.gameObject
        FileLog.Log("InventoryUI.CreateWindow()");
        try {
            Transform activeControl = InventoryUI.CurrentWindow.transform.Find("Canvas").Find("ActiveControl"); // Canvas/ActiveControl, this seems to be the main inventory window
            LogGameObjectHierarchy(activeControl.gameObject, 4);

            Transform itemInfoArea = activeControl.Find("ItemInfoArea");
            InventoryItemController inventoryItemController = activeControl.gameObject.GetComponentInChildren<InventoryItemController>();
            
            // Create a new button
            GameObject buttonObj = new GameObject("CustomButton");
            UnityEngine.UI.Button button = buttonObj.AddComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.Text buttonText = buttonObj.AddComponent<UnityEngine.UI.Text>();

            // Set the button's parent to the InventoryUI
            buttonObj.transform.SetParent(itemInfoArea, false);

            // Position and style the button
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(160, 30);

            // Style the button text
            buttonText.text = "Custom Button";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;

            // Add click event listener
            button.onClick.AddListener(() => OnCustomButtonClick());

            // LogGameObjectHierarchy(activeControl.gameObject, 3);
        } catch (Exception e) {
            FileLog.Log("Exception: " + e.Message + " at " + e.StackTrace);
        }
    }

    public static void LogGameObjectHierarchy(GameObject obj, int maxdepth, string indent = "", bool includeComponents = true)
    {
        if (maxdepth <= 0) return;
        if (obj == null) return;

        FileLog.Log($"{indent}{obj.name}");

        if (includeComponents)
        {
            Component[] components = obj.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component != null)
                    FileLog.Log($"{indent}  - {component.GetType().Name}");
            }
        }

        foreach (Transform child in obj.transform)
        {
            LogGameObjectHierarchy(child.gameObject, maxdepth-1, indent + "  ", includeComponents);
        }
    }

    static void OnCustomButtonClick() {
        FileLog.Log("Custom button clicked!");
        // Add your custom logic here
    }
}