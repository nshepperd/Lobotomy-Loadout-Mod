using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Inventory;
using Harmony;
using UnityEngine;

namespace Loadout {
    public class LoadoutManager {
        public FileManager fileManager;

        public LoadoutManager(FileManager fileManager) {
            this.fileManager = fileManager;
        }

        public void SaveLoadout(int slot, InventoryUI inventoryUI, InventoryItemController controller) {
            try {
                StringBuilder sb = new StringBuilder();
                GameObject itemInfoArea = inventoryUI.transform.Find("Canvas").Find("ActiveControl").Find("ItemInfoArea").gameObject;
                foreach (InventoryWeaponSlot weaponSlot in itemInfoArea.gameObject.GetComponentsInChildren<InventoryWeaponSlot>(true)) {
                    string weaponName = weaponSlot.Name.text;
                    for (int i = 0; i < weaponSlot.owners.Count; i++) {
                        if (weaponSlot.owners[i] is null) {
                            continue;
                        }
                        string agentName = weaponSlot.owners[i]._agentName.GetName();
                        sb.AppendLine("weapon|" + weaponName + "|" + i + "|" + agentName);
                    }
                }
                foreach (InventoryArmorSlot armorSlot in itemInfoArea.gameObject.GetComponentsInChildren<InventoryArmorSlot>(true)) {
                    string armorName = armorSlot.Name.text;
                    for (int i = 0; i < armorSlot.owners.Count; i++) {
                        if (armorSlot.owners[i] is null) {
                            continue;
                        }
                        string agentName = armorSlot.owners[i]._agentName.GetName();
                        sb.AppendLine("armor|" + armorName + "|" + i + "|" + agentName);
                    }
                }
                fileManager.WriteFile("loadout_" + slot + ".txt", sb.ToString());
            } catch (Exception e) {
                Logging.Info("Error saving loadout: " + e.Message + "\n" + e.StackTrace);
            }
        }

        public void LoadLoadout(int slot, InventoryUI inventoryUI, InventoryItemController controller) {
            try {
                GameObject itemInfoArea = inventoryUI.transform.Find("Canvas").Find("ActiveControl").Find("ItemInfoArea").gameObject;

                Dictionary<string, InventoryWeaponSlot> weaponSlots = new Dictionary<string, InventoryWeaponSlot>();
                Dictionary<string, InventoryArmorSlot> armorSlots = new Dictionary<string, InventoryArmorSlot>();
                Dictionary<string, AgentModel> agents = new Dictionary<string, AgentModel>();

                foreach (InventoryWeaponSlot weaponSlot in itemInfoArea.gameObject.GetComponentsInChildren<InventoryWeaponSlot>(true)) {
                    FileLog.Log("weaponSlot: " + weaponSlot.Name.text);
                    weaponSlots.Add(weaponSlot.Name.text, weaponSlot);
                }

                foreach (InventoryArmorSlot armorSlot in itemInfoArea.gameObject.GetComponentsInChildren<InventoryArmorSlot>(true)) {
                    FileLog.Log("armorSlot: " + armorSlot.Name.text);
                    armorSlots.Add(armorSlot.Name.text, armorSlot);
                }

                foreach (AgentModel agent in AgentManager.instance.GetAgentList()) {
                    FileLog.Log("agentModel: " + agent._agentName.GetName());
                    agents.Add(agent._agentName.GetName(), agent);
                }

                string loadout = fileManager.ReadFile("loadout_" + slot + ".txt");
                if (loadout is null) {
                    return;
                }
                foreach(string line in loadout.Split('\n')) {
                    FileLog.Log("line: " + line.Trim());
                    if (line.StartsWith("weapon|")) {
                        string[] parts = line.Trim().Split('|');
                        string weaponName = parts[1];
                        int slotIndex = int.Parse(parts[2]);
                        string agentName = parts[3];

                        if (!weaponSlots.ContainsKey(weaponName)) continue;
                        if (!agents.ContainsKey(agentName)) continue;

                        InventoryWeaponSlot weaponSlot = weaponSlots[weaponName];
                        AgentModel agent = agents[agentName];
                        if (weaponSlot.equipments.Count < slotIndex + 1) continue;
                        EquipmentModel equipmentModel = weaponSlot.equipments[slotIndex];

                        // Skip if already equipped.
                        if (equipmentModel.owner == agent) continue;

                        FileLog.Log("Trying to equip weapon " + weaponName + " " + slotIndex + " to " + agentName);
                        controller.OnEquipAction(equipmentModel, agent);
                    } else if (line.StartsWith("armor|")) {
                        string[] parts = line.Trim().Split('|');
                        string armorName = parts[1];
                        int slotIndex = int.Parse(parts[2]);
                        string agentName = parts[3];

                        if (!armorSlots.ContainsKey(armorName)) continue;
                        if (!agents.ContainsKey(agentName)) continue;

                        InventoryArmorSlot armorSlot = armorSlots[armorName];
                        AgentModel agent = agents[agentName];
                        if (armorSlot.equipments.Count < slotIndex + 1) continue;
                        EquipmentModel equipmentModel = armorSlot.equipments[slotIndex];

                        // Skip if already equipped.
                        if (equipmentModel.owner == agent) continue;

                        FileLog.Log("Trying to equip armor " + armorName + " " + slotIndex + " to " + agentName);
                        controller.OnEquipAction(equipmentModel, agent);
                    }   
                }
            } catch (Exception e) {
                Logging.Info("Error loading loadout: " + e.Message + "\n" + e.StackTrace);
            } 
        }
    }
}
