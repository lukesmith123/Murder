/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionSound.cs"
 * 
 *	This action triggers the sound component of any GameObject, overriding that object's play settings.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionSound : Action
{

	public int constantID = 0;
	public Sound soundObject;
	public AudioClip audioClip;
	public enum SoundAction { Play, FadeIn, FadeOut, Stop }
	public float fadeTime;
	public bool loop;
	public SoundAction soundAction;
	
	
	public ActionSound ()
	{
		this.isDisplayed = true;
		title = "Engine: Play sound";
	}
	
	
	override public float Run ()
	{
		if (isAssetFile && constantID != 0)
		{
			// Attempt to find the correct scene object
			ConstantID idObject = Serializer.returnComponent <ConstantID> (constantID);
			if (idObject != null && idObject.GetComponent <Sound>())
			{
				soundObject = idObject.GetComponent <Sound>();
			}
			else
			{
				soundObject = null;
			}
		}

		if (soundObject)
		{
			if (audioClip && soundObject.GetComponent <AudioSource>())
			{
				if (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn)
				{
					soundObject.GetComponent <AudioSource>().clip = audioClip;
				}
			}

			if (soundObject.soundType == SoundType.Music && (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn))
			{
				Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
				foreach (Sound sound in sounds)
				{
					if (sound.IsPlayingSameMusic (audioClip))
					{
						return 0f;
					}
					sound.EndOldMusic (soundObject);
				}
			}
			
			if (soundAction == SoundAction.Play)
			{
				soundObject.Play (loop);
			}
			else if (soundAction == SoundAction.FadeIn)
			{
				if (fadeTime == 0f)
				{
					soundObject.Play (loop);
				}
				else
				{
					soundObject.FadeIn (fadeTime, loop);
				}
			}
			else if (soundAction == SoundAction.FadeOut)
			{
				if (fadeTime == 0f)
				{
					soundObject.Stop ();
				}
				else
				{
					soundObject.FadeOut (fadeTime);
				}
			}
			else if (soundAction == SoundAction.Stop)
			{
				soundObject.Stop ();
			}
		}
		
		return 0f;
	}


	override public void Skip ()
	{
		if (soundObject && (loop || soundObject.soundType == SoundType.Music))
		{
			Run ();
		}
	}
	
	
	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		if (isAssetFile)
		{
			constantID = EditorGUILayout.IntField ("Sound object (ID):", constantID);
		}
		else
		{
			soundObject = (Sound) EditorGUILayout.ObjectField ("Sound object:", soundObject, typeof(Sound), true);
		}

		soundAction = (SoundAction) EditorGUILayout.EnumPopup ("Sound action:", (SoundAction) soundAction);
		
		if (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn)
		{
			loop = EditorGUILayout.Toggle ("Loop?", loop);
			audioClip = (AudioClip) EditorGUILayout.ObjectField ("New clip (optional)", audioClip, typeof (AudioClip), false);
		}
		
		if (soundAction == SoundAction.FadeIn || soundAction == SoundAction.FadeOut)
		{
			fadeTime = EditorGUILayout.Slider ("Fade time:", fadeTime, 0f, 10f);
		}

		AfterRunningOption ();
	}
	
	
	override public string SetLabel ()
	{
		string labelAdd = "";
		if (soundObject)
		{
			labelAdd = " (" + soundAction.ToString ();
			labelAdd += " " + soundObject.name + ")";
		}
		
		return labelAdd;
	}

	#endif

}