/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionVisible.cs"
 * 
 *	This action controls the visibilty of a GameObject and it's children.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionVisible : Action
{

	public int constantID = 0;
	public enum VisState { Visible, Invisible };
	public GameObject obToAffect;
	public bool affectChildren;
	public VisState visState = 0;
	
	
	public ActionVisible ()
	{
		this.isDisplayed = true;
		title = "Object: Visibility";
	}
	
	
	override public float Run ()
	{
		if (isAssetFile && constantID != 0)
		{
			// Attempt to find the correct scene object
			ConstantID idObject = Serializer.returnComponent <ConstantID> (constantID);
			if (idObject != null)
			{
				obToAffect = idObject.gameObject;
			}
			else
			{
				obToAffect = null;
			}
		}

		bool state = false;
		if (visState == VisState.Visible)
		{
			state = true;
		}
		
		if (obToAffect)
		{
			if (obToAffect.renderer)
			{
				obToAffect.renderer.enabled = state;
			}
			
			if (affectChildren)
			{
				foreach (Transform child in obToAffect.transform)
				{
					if (child.gameObject.renderer)
					{
						child.gameObject.renderer.enabled = state;
					}
				}
			}
				
		}
		
		return 0f;
	}
	
	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		if (isAssetFile)
		{
			constantID = EditorGUILayout.IntField ("Object to affect (ID):", constantID);
		}
		else
		{
			obToAffect = (GameObject) EditorGUILayout.ObjectField ("Object to affect:", obToAffect, typeof (GameObject), true);
		}

		visState = (VisState) EditorGUILayout.EnumPopup ("Visibility:", visState);
		affectChildren = EditorGUILayout.Toggle ("Affect children?", affectChildren);
		
		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (obToAffect)
				labelAdd = " (" + obToAffect.name + ")";
		
		return labelAdd;
	}

	#endif

}