/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionInventorySet.cs"
 * 
 *	This action is used to add or remove items from the player's inventory, defined in the Inventory Manager.
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
public class ActionInventorySet : Action
{
	
	public enum InvAction {Add, Remove};
	public InvAction invAction;
	
	public int invID;
	private int invNumber;
	
	public bool setAmount = false;
	public int amount = 1;
	
	private InventoryManager inventoryManager;
	
	
	override public float Run ()
	{
		RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
		
		if (runtimeInventory)
		{
			if (!setAmount)
			{
				amount = 1;
			}

			if (invAction == InvAction.Add)
			{
				runtimeInventory.Add (invID, amount);
			}
			else
			{
				runtimeInventory.Remove (invID, amount);
			}

			PlayerMenus.ResetInventoryBoxes ();
		}
		
		return 0f;
	}

	
	#if UNITY_EDITOR

	public ActionInventorySet ()
	{
		this.isDisplayed = true;
		title = "Inventory: Add or remove";
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
					
					// If a item has been removed, make sure selected variable is still valid
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
				
				EditorGUILayout.BeginHorizontal();
					invAction = (InvAction) EditorGUILayout.EnumPopup ("Inventory item:", invAction);

					invNumber = EditorGUILayout.Popup (invNumber, labelList.ToArray());
					invID = inventoryManager.items[invNumber].id;
				EditorGUILayout.EndHorizontal();
		
				if (inventoryManager.items[invNumber].canCarryMultiple)
				{
					setAmount = EditorGUILayout.Toggle ("Set amount?", setAmount);
				
					if (setAmount)
					{
						if (invAction == InvAction.Add)
						{
							amount = EditorGUILayout.IntField ("Increase count by:", amount);
						}
						else
						{
							amount = EditorGUILayout.IntField ("Reduce count by:", amount);
						}
					}
				}

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

		if (!inventoryManager)
		{
			inventoryManager = AdvGame.GetReferences ().inventoryManager;
		}

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
		
		if (invAction == InvAction.Add)
		{
			labelAdd = " (Add" + labelItem + ")";
		}
		else
		{
			labelAdd = " (Remove" + labelItem + ")";
		}
	
		return labelAdd;
	}

	#endif

}