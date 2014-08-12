/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionInventorySelect.cs"
 * 
 *	This action is used to automatically-select an inventory item.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionInventorySelect : Action
{
	
	public bool giveToPlayer = false;
	
	public int invID;
	private int invNumber;
	
	private InventoryManager inventoryManager;
	
	
	override public float Run ()
	{
		RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
		
		if (runtimeInventory)
		{
			if (giveToPlayer)
			{
				runtimeInventory.Add (invID, 1);
			}
			
			runtimeInventory.SelectItemByID (invID);
		}
		
		return 0f;
	}

	
	#if UNITY_EDITOR

	public ActionInventorySelect ()
	{
		this.isDisplayed = true;
		title = "Inventory: Select";
	}
	
	
	override public void ShowGUI ()
	{
		if (!inventoryManager)
		{
			inventoryManager = AdvGame.GetReferences ().inventoryManager;
		}
		
		if (inventoryManager)
		{
			// Create a string List of the field's names (for the PopUp box)
			List<string> labelList = new List<string>();
			
			int i = 0;
			invNumber = -1;
			
			if (inventoryManager.items.Count > 0)
			{
				foreach (InvItem _item in inventoryManager.items)
				{
					labelList.Add (_item.label);
					
					// If an item has been removed, make sure selected variable is still valid
					if (_item.id == invID)
					{
						invNumber = i;
					}
					
					i++;
				}
				
				if (invNumber == -1)
				{
					Debug.Log ("Previously chosen item no longer exists!");
					invNumber = 0;
					invID = 0;
				}
				
				invNumber = EditorGUILayout.Popup ("Inventory item:", invNumber, labelList.ToArray());
				invID = inventoryManager.items[invNumber].id;
		
				giveToPlayer = EditorGUILayout.Toggle ("Give to player if not held?", giveToPlayer);
				
				AfterRunningOption ();
			}
	
			else
			{
				EditorGUILayout.LabelField ("No inventory items exist!");
				invID = -1;
				invNumber = -1;
			}
		}
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		string labelItem = "";
		
		if (inventoryManager)
		{
			if (inventoryManager.items.Count > 0)
			{
				if (invNumber > -1)
				{
					labelItem = " " + inventoryManager.items[invNumber].label;
				}
			}
		}
		
		labelAdd = " (" + labelItem + ")";
	
		return labelAdd;
	}

	#endif

}