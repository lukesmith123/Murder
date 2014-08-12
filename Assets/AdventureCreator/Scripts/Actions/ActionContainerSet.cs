/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionContainerSet.cs"
 * 
 *	This action is used to add or remove items from a container,
 *	with items being defined in the Inventory Manager.
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
public class ActionContainerSet : Action
{
	
	public enum ContainerAction {Add, Remove, RemoveAll};
	public ContainerAction containerAction;
	
	public int invID;
	private int invNumber;

	public bool useActive = false;
	public int constantID = 0;
	public Container container;

	public bool setAmount = false;
	public int amount = 1;
	public bool transferToPlayer = false;
	
	private InventoryManager inventoryManager;
	
	
	override public float Run ()
	{
		if (useActive)
		{
			container = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>().activeContainer;
		}
		else if (isAssetFile && constantID != 0)
		{
			// Attempt to find the correct scene object
			container = Serializer.returnComponent <Container> (constantID);
		}
		
		if (container == null)
		{
			return 0f;
		}

		if (!setAmount)
		{
			amount = 1;
		}

		if (containerAction == ContainerAction.Add)
		{
			container.Add (invID, amount);
		}
		else if (containerAction == ContainerAction.Remove)
		{
			if (transferToPlayer)
			{
				RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
				runtimeInventory.Add (invID, amount);
			}

			container.Remove (invID, amount);
		}
		else if (containerAction == ContainerAction.RemoveAll)
		{
			if (transferToPlayer)
			{
				RuntimeInventory runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
				foreach (ContainerItem item in container.items)
				{
					runtimeInventory.Add (item.linkedID, item.count);
				}
			}

			container.items.Clear ();
		}

		PlayerMenus.ResetInventoryBoxes ();

		return 0f;
	}

	
	#if UNITY_EDITOR

	public ActionContainerSet ()
	{
		this.isDisplayed = true;
		title = "Container: Add or remove";
	}
	
	
	override public void ShowGUI ()
	{
		if (AdvGame.GetReferences ().inventoryManager)
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

				useActive = EditorGUILayout.Toggle ("Affect active container?", useActive);
				if (!useActive)
				{
					if (isAssetFile)
					{
						constantID = EditorGUILayout.IntField ("Container (ID):", constantID);
					}
					else
					{
						container = (Container) EditorGUILayout.ObjectField ("Container:", container, typeof (Container), true);
					}
				}

				containerAction = (ContainerAction) EditorGUILayout.EnumPopup ("Method:", containerAction);

				if (containerAction == ContainerAction.RemoveAll)
				{
					transferToPlayer = EditorGUILayout.Toggle ("Transfer to Player?", transferToPlayer);
				}
				else
				{
					if (containerAction == ContainerAction.Add)
					{
						invNumber = EditorGUILayout.Popup ("Item to add:", invNumber, labelList.ToArray());
					}
					else if (containerAction == ContainerAction.Remove)
					{
						invNumber = EditorGUILayout.Popup ("Item to remove:", invNumber, labelList.ToArray());
						transferToPlayer = EditorGUILayout.Toggle ("Transfer to Player?", transferToPlayer);
					}
					invID = inventoryManager.items[invNumber].id;

					if (inventoryManager.items[invNumber].canCarryMultiple)
					{
						setAmount = EditorGUILayout.Toggle ("Set amount?", setAmount);
					
						if (setAmount)
						{
							if (containerAction == ContainerAction.Add)
							{
								amount = EditorGUILayout.IntField ("Increase count by:", amount);
							}
							else if (containerAction == ContainerAction.Remove)
							{
								amount = EditorGUILayout.IntField ("Reduce count by:", amount);
							}
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
		
		if (containerAction == ContainerAction.Add)
		{
			labelAdd = " (Add" + labelItem + ")";
		}
		else if (containerAction == ContainerAction.Remove)
		{
			labelAdd = " (Remove" + labelItem + ")";
		}
		else if (containerAction == ContainerAction.RemoveAll)
		{
			labelAdd = " (Remove all)";
		}
	
		return labelAdd;
	}

	#endif

}