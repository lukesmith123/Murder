/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCameraSplit.cs"
 * 
 *	This Action splits the screen horizontally or vertically.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionCameraSplit : Action
{

	public bool turnOff;
	public MenuOrientation orientation;
	public _Camera cam1;
	public _Camera cam2;
	public bool mainIsTopLeft;
	
	
	public ActionCameraSplit ()
	{
		this.isDisplayed = true;
		title = "Camera: Split-screen";
	}
	
	
	override public float Run ()
	{
		MainCamera mainCamera = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();

		if (turnOff)
		{
			mainCamera.RemoveSplitScreen ();
			return 0f;
		}

		if (cam1 == null || cam2 == null)
		{
			return 0f;
		}

		if (mainIsTopLeft)
		{
			mainCamera.SetSplitScreen (cam1, cam2, orientation, mainIsTopLeft);
		}
		else
		{
			mainCamera.SetSplitScreen (cam2, cam1, orientation, mainIsTopLeft);
		}

		return 0f;
	}
	
	
	#if UNITY_EDITOR
	
	override public void ShowGUI ()
	{
		turnOff = EditorGUILayout.Toggle ("Disable previous split?", turnOff);
		if (!turnOff)
		{
			orientation = (MenuOrientation) EditorGUILayout.EnumPopup ("Divider:", orientation);
			if (orientation == MenuOrientation.Horizontal)
			{
				cam1 = (_Camera) EditorGUILayout.ObjectField ("Top camera:", cam1, typeof (_Camera), true);
				cam2 = (_Camera) EditorGUILayout.ObjectField ("Bottom camera:", cam2, typeof (_Camera), true);
				mainIsTopLeft = EditorGUILayout.Toggle ("Main Camera is top?", mainIsTopLeft);
			}
			else
			{
				cam1 = (_Camera) EditorGUILayout.ObjectField ("Left camera:", cam1, typeof (_Camera), true);
				cam2 = (_Camera) EditorGUILayout.ObjectField ("Right camera:", cam2, typeof (_Camera), true);
				mainIsTopLeft = EditorGUILayout.Toggle ("Main Camera is left?", mainIsTopLeft);
			}
		}
		
		AfterRunningOption ();
	}
	
	
	public override string SetLabel ()
	{
		// Return a string used to describe the specific action's job.
		
		string labelAdd = "";
		return labelAdd;
	}
	
	#endif
	
}