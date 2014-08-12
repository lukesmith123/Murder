/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionFade.cs"
 * 
 *	This action controls the MainCamera's fading.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionFade : Action
{
	
	public FadeType fadeType;
	public bool isInstant;
	public float fadeSpeed = 0.5f;
	
	
	public ActionFade ()
	{
		this.isDisplayed = true;
		title = "Camera: Fade";
	}
	
	
	override public float Run ()
	{
		if (!isRunning)
		{
			isRunning = true;
			
			MainCamera mainCam = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();

			if (mainCam)
			{
				if (fadeType == FadeType.fadeIn)
				{
					if (isInstant)
					{
						mainCam.FadeIn (0);
					}
					else
					{
						mainCam.FadeIn (fadeSpeed);
					}
				}
				else
				{
					if (isInstant)
					{
						mainCam.FadeOut (0);
					}
					else
					{
						mainCam.FadeOut (fadeSpeed);
					}
				}
				
				if (willWait && !isInstant)
				{
					return (fadeSpeed);
				}
			}

			return 0f;
		}

		else
		{
			isRunning = false;
			return 0f;
		}
	}


	override public void Skip ()
	{
		MainCamera mainCam = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
		
		if (mainCam)
		{
			if (fadeType == FadeType.fadeIn)
			{
				mainCam.FadeIn (0);
			}
			else
			{
				mainCam.FadeOut (0);
			}
		}
	}

	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		fadeType = (FadeType) EditorGUILayout.EnumPopup ("Type:", fadeType);
		
		isInstant = EditorGUILayout.Toggle ("Instant?", isInstant);
		if (!isInstant)
		{
			fadeSpeed = EditorGUILayout.Slider ("Time to fade:", fadeSpeed, 0, 3);
			willWait = EditorGUILayout.Toggle ("Pause until finish?", willWait);
		}

		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (fadeType == FadeType.fadeIn)
		{
			labelAdd = " (In)";
		}
		else
		{
			labelAdd = " (Out)";
		}
		
		return labelAdd;
	}

	#endif

}