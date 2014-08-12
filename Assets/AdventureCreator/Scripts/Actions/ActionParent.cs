/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionParent.cs"
 * 
 *	This action is used to set and clear the parent of GameObjects.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionParent : Action
{

	public int parentTransformID = 0;
	public int obToAffectID = 0;

	public enum ParentAction { SetParent, ClearParent };
	public ParentAction parentAction;

	public Transform parentTransform;
	
	public GameObject obToAffect;
	public bool isPlayer;
	
	public bool setPosition;
	public Vector3 newPosition;
	
	public bool setRotation;
	public Vector3 newRotation;
	

	public ActionParent ()
	{
		this.isDisplayed = true;
		title = "Object: Set parent";
	}
	
	
	override public float Run ()
	{
		if (isAssetFile)
		{
			if (parentTransformID != 0)
			{
				ConstantID idObject = Serializer.returnComponent <ConstantID> (parentTransformID);
				if (idObject != null)
				{
					parentTransform = idObject.transform;
				}
				else
				{
					parentTransform = null;
				}
			}

			if (obToAffectID != 0)
			{
				ConstantID idObject = Serializer.returnComponent <ConstantID> (obToAffectID);
				if (idObject != null)
				{
					obToAffect = idObject.gameObject;
				}
				else
				{
					obToAffect = null;
				}
			}
		}

		if (isPlayer)
		{
			obToAffect = GameObject.FindWithTag ("Player");
		}

		if (parentAction == ParentAction.SetParent && parentTransform)
		{
			obToAffect.transform.parent = parentTransform;
			
			if (setPosition)
			{
				obToAffect.transform.localPosition = newPosition;
			}
			
			if (setRotation)
			{
				obToAffect.transform.localRotation = Quaternion.LookRotation (newRotation);
			}
		}

		else if (parentAction == ParentAction.ClearParent)
		{
			obToAffect.transform.parent = null;
		}
		
		return 0f;
	}
	
	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
		if (!isPlayer)
		{
			if (isAssetFile)
			{
				obToAffectID = EditorGUILayout.IntField ("Object to affect (ID):", obToAffectID);
			}
			else
			{
				obToAffect = (GameObject) EditorGUILayout.ObjectField ("Object to affect:", obToAffect, typeof(GameObject), true);
			}
		}

		parentAction = (ParentAction) EditorGUILayout.EnumPopup ("Method:", parentAction);
		if (parentAction == ParentAction.SetParent)
		{
			if (isAssetFile)
			{
				parentTransformID = EditorGUILayout.IntField ("Parent to (ID):", parentTransformID);
			}
			else
			{
				parentTransform = (Transform) EditorGUILayout.ObjectField ("Parent to:", parentTransform, typeof(Transform), true);
			}
		
			setPosition = EditorGUILayout.Toggle ("Set local position?", setPosition);
			if (setPosition)
			{
				newPosition = EditorGUILayout.Vector3Field ("Position vector:", newPosition);
			}
			
			setRotation = EditorGUILayout.Toggle ("Set local rotation?", setRotation);
			if (setRotation)
			{
				newRotation = EditorGUILayout.Vector3Field ("Rotation vector:", newRotation);
			}
		}
		
		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (obToAffect)
		{
			labelAdd = " (" + obToAffect.name + ")";
		}
		
		return labelAdd;
	}

	#endif

}