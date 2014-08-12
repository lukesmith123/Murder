/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ActionCharAnim.cs"
 * 
 *	This action is used to control character animation.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ActionCharAnim : Action
{

	public int constantID = 0;

	public bool isPlayer;
	public Char animChar;
	public AnimationClip clip;
	public string clip2D;

	public enum AnimMethodChar { PlayCustom, StopCustom, ResetToIdle, SetStandard };
	public AnimMethodChar method;
	
	public AnimationBlendMode blendMode;
	public AnimLayer layer = AnimLayer.Base;
	public AnimStandard standard;
	public bool includeDirection = false;

	public bool changeSound = false;
	public AudioClip newSound;

	public int layerInt;
	public bool idleAfter = true;
	public bool idleAfterCustom = false;

	public AnimPlayMode playMode;
	public AnimPlayModeBase playModeBase = AnimPlayModeBase.PlayOnceAndClamp;

	public float fadeTime = 0f;

	public bool changeSpeed = false;
	public float newSpeed = 0f;

	public AnimMethodCharMecanim methodMecanim;
	public MecanimCharParameter mecanimCharParameter;
	public MecanimParameterType mecanimParameterType;
	public string parameterName;
	public float parameterValue;

	
	public ActionCharAnim ()
	{
		this.isDisplayed = true;
		title = "Character: Animate";
	}


	private void GetAssetFile ()
	{
		if (isPlayer)
		{
			if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <AC.Player>())
			{
				animChar = GameObject.FindWithTag (Tags.player).GetComponent <AC.Player>();
			}
		}
		else if (isAssetFile && constantID != 0)
		{
			// Attempt to find the correct scene object
			ConstantID idObject = Serializer.returnComponent <ConstantID> (constantID);
			if (idObject != null && idObject.GetComponent <Char>())
			{
				animChar = idObject.GetComponent <Char>();
			}
			else
			{
				animChar = null;
			}
		}
	}
	
	
	override public float Run ()
	{
		GetAssetFile ();

		if (animChar)
		{
			if (animChar.animEngine == null)
			{
				animChar.ResetAnimationEngine ();
			}
			
			if (animChar.animEngine != null)
			{
				return animChar.animEngine.ActionCharAnimRun (this);
			}
			else
			{
				Debug.LogWarning ("Could not create animation engine!");
			}
		}
		else
		{
			Debug.LogWarning ("Could not create animation engine!");
		}

		return 0f;
	}


	override public void Skip ()
	{
		GetAssetFile ();
		
		if (animChar)
		{
			if (animChar.animEngine == null)
			{
				animChar.ResetAnimationEngine ();
			}
			
			if (animChar.animEngine != null)
			{
				animChar.animEngine.ActionCharAnimSkip (this);
			}
		}
	}


	#if UNITY_EDITOR

	override public void ShowGUI ()
	{
		isPlayer = EditorGUILayout.Toggle ("Is Player?", isPlayer);
		if (isPlayer)
		{
			if (Application.isPlaying)
			{
				animChar = GameObject.FindWithTag (Tags.player).GetComponent <AC.Char>();
			}
			else
			{
				animChar = AdvGame.GetReferences ().settingsManager.GetDefaultPlayer ();
			}
		}
		else
		{
			animChar = (Char) EditorGUILayout.ObjectField ("Character:", animChar, typeof (Char), true);

			if (animChar && animChar.GetComponent <ConstantID>())
			{
				constantID = animChar.GetComponent <ConstantID>().constantID;
			}
		}

		if (animChar)
		{
			if (animChar.animEngine == null)
			{
				animChar.ResetAnimationEngine ();
			}
			if (animChar.animEngine)
			{
				animChar.animEngine.ActionCharAnimGUI (this);
			}
		}
		else
		{
			EditorGUILayout.HelpBox ("This Action requires a Character before more options will show.", MessageType.Info);
		}

		AfterRunningOption ();
	}

	
	override public string SetLabel ()
	{
		string labelAdd = "";
		
		if (isPlayer)
		{
			labelAdd = " (Player)";
		}
		else if (animChar)
		{
			labelAdd = " (" + animChar.name + ")";
		}
		
		return labelAdd;
	}
	
	#endif

}