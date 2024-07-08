using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Harmony;
using Inventory;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Loadout;

public class LoadoutButton : MonoBehaviour
{
    public Color ActiveColor;
    public Color NormalColor;
    public Text Text;
    public Image Frame;
    public int loadoutSlot;
    public InventoryUI inventoryUI;
    public InventoryItemController controller;

    public enum Type
    {
        Load, Save
    };
    public Type type;

    public void Init()
    {
        Frame.color = NormalColor;
        Text.color = NormalColor;
        Text.text = (type == Type.Load) ? "Load" : "Save";
    }

    public void Start()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = null;
        entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((data) => { OnPointerClick((PointerEventData)data); });
        trigger.triggers.Add(entry);
        entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener((data) => { OnPointerEnter((PointerEventData)data); });
        trigger.triggers.Add(entry);
        entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entry.callback.AddListener((data) => { OnPointerExit((PointerEventData)data); });
        trigger.triggers.Add(entry);
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (type == Type.Save)
        {
            Logging.Debug("Would save loadout " + loadoutSlot);
            Harmony_Patch.loadoutManager.SaveLoadout(loadoutSlot, inventoryUI, controller);
        }
        else
        {
            Logging.Debug("Would load loadout " + loadoutSlot);
            Harmony_Patch.loadoutManager.LoadLoadout(loadoutSlot, inventoryUI, controller);
        }
    }

    public void OnPointerEnter(PointerEventData data)
    {
        Frame.color = ActiveColor;
        Text.color = ActiveColor;
    }

    public void OnPointerExit(PointerEventData data)
    {
        Frame.color = NormalColor;
        Text.color = NormalColor;
    }
}

[HarmonyPatch(typeof(Inventory.InventoryUI), nameof(InventoryUI.CreateWindow))]
class InventoryUI_CreateWindow
{
    private static GameObject MakeText(string name)
    {
        GameObject textObj = new GameObject(name, typeof(RectTransform));
        Text text = textObj.AddComponent<Text>();
        FontLoadScript fontLoadScript = textObj.AddComponent<FontLoadScript>();
        // text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 20;
        text.alignment = TextAnchor.MiddleCenter;
        fontLoadScript.neededType = FontType.SUBTITLE;
        return textObj;
    }

    private static GameObject MakeButton(LoadoutButton.Type type, Color activeColor, Transform activeControl)
    {
        Transform itemInfoArea = activeControl.Find("ItemInfoArea");
        InventoryItemController inventoryItemController = activeControl.gameObject.GetComponentInChildren<InventoryItemController>();
        GameObject zayin = itemInfoArea.Find("RankLabel").Find("Zayin").gameObject;

        GameObject buttonObj = new GameObject("LoadoutButton", typeof(RectTransform));
        Image buttonImage = buttonObj.AddComponent<Image>();
        GameObject textObj = MakeText("Text");
        Text buttonText = textObj.GetComponent<Text>();

        textObj.transform.SetParent(buttonObj.transform, false);

        // Log rect transforms
        // Logging.Debug("ranklabel: " + PrintRectTransform(itemInfoArea.Find("RankLabel").gameObject.GetComponent<RectTransform>()));
        // Logging.Debug("zayin:" + PrintRectTransform(zayin.GetComponent<RectTransform>()));
        // LogGameObjectHierarchy(zayin, 4);

        // Logging.Debug("zayin colors (active): " + zayin.GetComponent<InventoryRankButton>().AreaColor);
        // Logging.Debug("zayin colors (normal): " + zayin.GetComponent<InventoryRankButton>().NormalColor);

        // Logging.Debug("font???");
        // Copy Image
        Image existingImage = zayin.GetComponent<Image>();
        buttonImage.sprite = existingImage.sprite;
        buttonImage.type = existingImage.type;

        LoadoutButton button = buttonObj.AddComponent<LoadoutButton>();
        EventTrigger trigger = buttonObj.AddComponent<EventTrigger>();
        button.ActiveColor = activeColor;
        button.NormalColor = zayin.GetComponent<InventoryRankButton>().NormalColor;
        button.Text = buttonText;
        button.Frame = buttonImage;
        button.inventoryUI = InventoryUI.CurrentWindow;
        button.controller = inventoryItemController;
        button.loadoutSlot = 1;
        button.type = type;

        Logging.Debug("button.Init()");

        button.Init();

        Logging.Debug("button.Init() done");

        // Set size and position
        RectTransform newRectTransform = buttonObj.GetComponent<RectTransform>();
        newRectTransform.anchorMin = new Vector2(0.5f, 1.0f);
        newRectTransform.anchorMax = new Vector2(0.5f, 1.0f);
        newRectTransform.anchoredPosition = new Vector2(0.0f, 10.0f); //-1.0f, 171.4f + 129.8f);
        newRectTransform.sizeDelta = new Vector2(121.0f, 45.0f);
        newRectTransform.pivot = new Vector2(0.0f, 0.0f);

        // Set text to full size of button.
        RectTransform textRectTransform = textObj.GetComponent<RectTransform>();
        textRectTransform.anchorMin = new Vector2(0.15f, 0.0f);
        textRectTransform.anchorMax = new Vector2(1.0f, 1.0f);
        textRectTransform.sizeDelta = new Vector2(0.0f, 0.0f);
        textRectTransform.pivot = new Vector2(0.5f, 0.5f);
        textRectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
        return buttonObj;
    }

    static void Setup()
    {
        Transform activeControl = InventoryUI.CurrentWindow.transform.Find("Canvas").Find("ActiveControl"); // Canvas/ActiveControl, this seems to be the main inventory window

        Transform itemInfoArea = activeControl.Find("ItemInfoArea");
        InventoryItemController inventoryItemController = activeControl.gameObject.GetComponentInChildren<InventoryItemController>();

        GameObject zayin = itemInfoArea.Find("RankLabel").Find("Zayin").gameObject;
        GameObject aleph = itemInfoArea.Find("RankLabel").Find("Aleph").gameObject;

        // Create a new button
        GameObject loadButton = MakeButton(LoadoutButton.Type.Load, zayin.GetComponent<InventoryRankButton>().AreaColor, activeControl);
        GameObject saveButton = MakeButton(LoadoutButton.Type.Save, aleph.GetComponent<InventoryRankButton>().AreaColor, activeControl);

        loadButton.transform.SetParent(itemInfoArea, false);
        saveButton.transform.SetParent(itemInfoArea, false);

        RectTransform newRectTransform = loadButton.GetComponent<RectTransform>();
        newRectTransform.anchorMin = new Vector2(0.5f, 1.0f);
        newRectTransform.anchorMax = new Vector2(0.5f, 1.0f);
        newRectTransform.anchoredPosition = new Vector2(0.0f, 10.0f);

        newRectTransform = saveButton.GetComponent<RectTransform>();
        newRectTransform.anchorMin = new Vector2(0.5f, 1.0f);
        newRectTransform.anchorMax = new Vector2(0.5f, 1.0f);
        newRectTransform.anchoredPosition = new Vector2(141.0f, 10.0f);

        GameObject loadoutLabel = MakeText("LoadoutLabel");
        Text loadoutText = loadoutLabel.GetComponent<Text>();
        loadoutText.color = zayin.GetComponent<InventoryRankButton>().NormalColor;
        loadoutText.text = "Loadout";
        newRectTransform = loadoutLabel.GetComponent<RectTransform>();
        newRectTransform.SetParent(itemInfoArea, false);
        newRectTransform.anchorMin = new Vector2(0.5f, 1.0f);
        newRectTransform.anchorMax = new Vector2(0.5f, 1.0f);
        newRectTransform.anchoredPosition = new Vector2(-45.0f, 45+10.0f);
        newRectTransform.sizeDelta = new Vector2(121.0f, 35.0f);
        newRectTransform.pivot = new Vector2(0.0f, 0.0f);
    }

    static void Postfix(ref InventoryUI __result)
    {
        // InventoryUI.CurrentWindow.gameObject
        Logging.Debug("InventoryUI.CreateWindow()");
        try
        {
            if (InventoryUI.CurrentWindow.gameObject.GetComponentInChildren<LoadoutButton>() == null)
            {
                Setup();
            }
        }
        catch (Exception e)
        {
            Logging.Info("Exception:\n" + e.Message + "\n" + e.StackTrace);
        }
    }

    static string PrintRectTransform(RectTransform rectTransform)
    {
        return $"RectTransform: {rectTransform.name}, AnchorMin: {rectTransform.anchorMin}, AnchorMax: {rectTransform.anchorMax}, AnchoredPosition: {rectTransform.anchoredPosition}, SizeDelta: {rectTransform.sizeDelta}, Pivot: {rectTransform.pivot}";
    }

    public static void LogGameObjectHierarchy(GameObject obj, int maxdepth, string indent = "", bool includeComponents = true)
    {
        if (maxdepth <= 0) return;
        if (obj == null) return;

        Logging.Debug($"{indent}{obj.name}");

        if (includeComponents)
        {
            Component[] components = obj.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component != null)
                    Logging.Debug($"{indent}  - {component.GetType().Name}");
            }
        }

        foreach (Transform child in obj.transform)
        {
            LogGameObjectHierarchy(child.gameObject, maxdepth - 1, indent + "  ", includeComponents);
        }
    }
}