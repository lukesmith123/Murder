/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionContainerOpen.cs"
 * 
 *	This action makes a Container active for display in an
 *	InventoryBox. To de-activate it, close a Menu with AppearType
 *	set to OnContainer.
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
public class ActionContainerOpen : Action
{

	public bool useActive = false;
	public int constantID = 0;
	public Container container;
	
	
	public ActionContainerOpen ()
	{
		this.isDisplayed = true;
		title = "Container: Open";
	}
	
	
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

		container.Interact ();

		return 0f;
	}
	

	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
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
	}
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (container)
		{
			labelAdd = " (" + container + ")";
		}
		
		return labelAdd;
	}

	#endif
	
}