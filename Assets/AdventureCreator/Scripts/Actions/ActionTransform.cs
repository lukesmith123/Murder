/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionTransform.cs"
 * 
 *	This action modifies a GameObject position, rotation or scale over a set time.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionTransform : Action
{

	public int constantID = 0;
	public Moveable linkedProp;
	public float transitionTime;
	public Vector3 newVector;

	public TransformType transformType;
	public MoveMethod moveMethod;
	
	public enum ToBy { To, By };
	public ToBy toBy;
	
	
	public ActionTransform ()
	{
		this.isDisplayed = true;
		title = "Object: Transform";
	}
	
	
	override public float Run ()	
	{
		if (!isRunning)
		{
			isRunning = true;

			if (isAssetFile && constantID != 0)
			{
				// Attempt to find the correct scene object
				ConstantID idObject = Serializer.returnComponent <ConstantID> (constantID);
				if (idObject != null && idObject.GetComponent <Moveable>())
				{
					linkedProp = idObject.GetComponent <Moveable>();
				}
				else
				{
					linkedProp = null;
				}
			}

			if (linkedProp)
			{
				Vector3 targetVector = newVector;	
				
				if (transformType == TransformType.Translate)
				{
					if (toBy == ToBy.By)
					{
						targetVector = linkedProp.transform.localPosition + newVector;
					}
						
					if (transitionTime == 0f)
					{
						linkedProp.transform.localPosition = targetVector;
					}
				}
				
				else if (transformType == TransformType.Rotate)
				{
					if (toBy == ToBy.By)
					{
						targetVector = linkedProp.transform.localEulerAngles + newVector;
					}
						
					if (transitionTime == 0f)
					{
						linkedProp.transform.localEulerAngles = targetVector;
					}
				}
				
				else if (transformType == TransformType.Scale)
				{
					if (toBy == ToBy.By)
					{
						targetVector = linkedProp.transform.localScale + newVector;
					}
						
					if (transitionTime == 0f)
					{
						linkedProp.transform.localScale = targetVector;
					}
				}
				
				if (transitionTime > 0f)
				{
					linkedProp.Move (targetVector, moveMethod, transitionTime, transformType);
					
					if (willWait)
					{
						return (defaultPauseTime);
					}
				}
			}
		}
		else
		{
			if (linkedProp)
			{
				if (!linkedProp.isMoving)
				{
					isRunning = false;
				}
				else
				{
					return defaultPauseTime;
				}
			}
		}

		return 0f;
	}


	override public void Skip ()	
	{
		if (isAssetFile && constantID != 0)
		{
			// Attempt to find the correct scene object
			ConstantID idObject = Serializer.returnComponent <ConstantID> (constantID);
			if (idObject != null && idObject.GetComponent <Moveable>())
			{
				linkedProp = idObject.GetComponent <Moveable>();
			}
			else
			{
				linkedProp = null;
			}
		}
			
		if (linkedProp)
		{
			linkedProp.isMoving = false;

			if (transformType == TransformType.Translate)
			{
				if (toBy == ToBy.By)
				{
					linkedProp.transform.localPosition = linkedProp.transform.localPosition + newVector;
				}
				else
				{
					linkedProp.transform.localPosition = newVector;
				}
			}
			
			else if (transformType == TransformType.Rotate)
			{
				if (toBy == ToBy.By)
				{
					linkedProp.transform.localEulerAngles = linkedProp.transform.localEulerAngles + newVector;
				}
				else
				{
					linkedProp.transform.localEulerAngles = newVector;
				}
			}
			
			else if (transformType == TransformType.Scale)
			{
				if (toBy == ToBy.By)
				{
					linkedProp.transform.localScale = linkedProp.transform.localScale + newVector;
				}
				else
				{
					linkedProp.transform.localScale = newVector;
				}
			}
		}
	}

	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		if (isAssetFile)
		{
			constantID = EditorGUILayout.IntField ("Moveable object (ID):", constantID);
		}
		else
		{
			linkedProp = (Moveable) EditorGUILayout.ObjectField ("Moveable object:", linkedProp, typeof(Moveable), true);
		}
		
		EditorGUILayout.BeginHorizontal ();
			transformType = (TransformType) EditorGUILayout.EnumPopup (transformType);
			toBy = (ToBy) EditorGUILayout.EnumPopup (toBy);
		EditorGUILayout.EndHorizontal ();
		
		newVector = EditorGUILayout.Vector3Field ("Vector:", newVector);
		transitionTime = EditorGUILayout.Slider ("Transition time:", transitionTime, 0, 10f);
		
		if (transitionTime > 0f)
		{
			moveMethod = (MoveMethod) EditorGUILayout.EnumPopup ("Move method", moveMethod);
			willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);
		}
		
		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		if (linkedProp)
		{
			labelAdd = " (" + linkedProp.name + ")";
		}
		
		return labelAdd;
	}
	
	#endif

}