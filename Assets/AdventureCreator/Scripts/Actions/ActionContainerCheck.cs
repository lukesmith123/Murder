/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionContainerCheck.cs"
 * 
 *	This action checks to see if a particular inventory item
 *	is inside a container, and performs something accordingly.
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
public class ActionContainerCheck : ActionCheck
{

	public int invID;
	private int invNumber;

	public bool useActive = false;
	public int constantID = 0;
	public Container container;

	public bool doCount;
	public int intValue = 1;
	public enum IntCondition { EqualTo, NotEqualTo, LessThan, MoreThan };
	public IntCondition intCondition;

	private InventoryManager inventoryManager;
	
	
	public ActionContainerCheck ()
	{
		this.isDisplayed = true;
		title = "Container: Check";
	}

	
	override public bool CheckCondition ()
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
			return false;
		}

		int count = container.GetCount (invID);
		
		if (doCount)
		{
			if (intCondition == IntCondition.EqualTo)
			{
				if (count == intValue)
				{
					return true;
				}
			}
			
			else if (intCondition == IntCondition.NotEqualTo)
			{
				if (count != intValue)
				{
					return true;
				}
			}
			
			else if (intCondition == IntCondition.LessThan)
			{
				if (count < intValue)
				{
					return true;
				}
			}
			
			else if (intCondition == IntCondition.MoreThan)
			{
				if (count > intValue)
				{
					return true;
				}
			}
		}
		
		else if (count > 0)
		{
			return true;
		}
		
		return false;	
	}
	

	#if UNITY_EDITOR
	
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
					// If an item has been removed, make sure selected variable is still valid
					if (_item.id == invID)
					{
						invNumber = i;
					}
					
					i++;
				}
				
				if (invNumber == -1)
				{
					// Wasn't found (item was possibly deleted), so revert to zero
					Debug.LogWarning ("Previously chosen item no longer exists!");
					
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

				invNumber = EditorGUILayout.Popup ("Item to check:", invNumber, labelList.ToArray());
				invID = inventoryManager.items[invNumber].id;

				if (inventoryManager.items[invNumber].canCarryMultiple)
				{
					doCount = EditorGUILayout.Toggle ("Query count?", doCount);
				
					if (doCount)
					{
						EditorGUILayout.BeginHorizontal ("");
							EditorGUILayout.LabelField ("Count is:", GUILayout.MaxWidth (70));
							intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
							intValue = EditorGUILayout.IntField (intValue);
						
							if (intValue < 1)
							{
								intValue = 1;
							}
						EditorGUILayout.EndHorizontal ();
					}
				}
				else
				{
					doCount = false;
				}
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
		if (!inventoryManager)
		{
			inventoryManager = AdvGame.GetReferences ().inventoryManager;
		}

		if (inventoryManager)
		{
			if (inventoryManager.items.Count > 0 && inventoryManager.items.Count > invNumber && invNumber > -1)
			{
				return (" (" + inventoryManager.items[invNumber].label + ")");
			}
		}
		
		return "";
	}

	#endif
	
}