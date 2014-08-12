﻿	/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"AnimEngine.cs"
 * 
 *	This script is a base class for the Animation engine scripts.
 *  Create a subclass of name "AnimEngine_NewMethodName" and
 * 	add "NewMethodName" to the AnimationEngine enum to integrate
 * 	a new method into the engine.
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
public class AnimEngine : ScriptableObject
{

	// Character variables
	public AC.Char character;
	public bool turningIsLinear = false;
	public bool rootMotion = false;
	public bool isSpriteBased = false;


	public virtual void Declare (AC.Char _character)
	{
		character = _character;
		turningIsLinear = false;
		rootMotion = false;
		isSpriteBased = false;
	}

	public virtual void CharSettingsGUI ()
	{ 
		#if UNITY_EDITOR
		#endif
	}

	public virtual void ActionCharAnimGUI (ActionCharAnim action)
	{
		#if UNITY_EDITOR
		action.method = (ActionCharAnim.AnimMethodChar) EditorGUILayout.EnumPopup ("Method:", action.method);
		#endif
	}

	public virtual float ActionCharAnimRun (ActionCharAnim action)
	{
		return 0f;
	}

	public virtual void ActionCharAnimSkip (ActionCharAnim action)
	{ }

	public virtual void ActionCharHoldGUI (ActionCharHold action)
	{
		#if UNITY_EDITOR
		EditorGUILayout.HelpBox ("This Action is not compatible with this Character's Animation Engine.", MessageType.Info);
		#endif
	}
	
	public virtual void ActionCharHoldRun (ActionCharHold action)
	{ }

	public virtual void ActionSpeechGUI (ActionSpeech action)
	{
		#if UNITY_EDITOR
		#endif
	}
	
	public virtual void ActionSpeechRun (ActionSpeech action)
	{ }

	public virtual void ActionSpeechSkip (ActionSpeech action)
	{ }

	public virtual void ActionAnimGUI (ActionAnim action)
	{
		#if UNITY_EDITOR
		#endif
	}

	public virtual string ActionAnimLabel (ActionAnim action)
	{
		return "";
	}
	
	public virtual float ActionAnimRun (ActionAnim action)
	{
		return 0f;
	}

	public virtual void ActionAnimSkip (ActionAnim action)
	{ }

	public virtual void ActionCharRenderGUI (ActionCharRender action)
	{ }

	public virtual float ActionCharRenderRun (ActionCharRender action)
	{
		return 0f;
	}

	public virtual void PlayIdle ()
	{ }
	
	public virtual void PlayWalk ()
	{ }

	public virtual void PlayRun ()
	{ }
	
	public virtual void PlayTalk ()
	{ }

	public virtual void PlayTurnLeft ()
	{
		PlayIdle ();
	}
	
	public virtual void PlayTurnRight ()
	{
		PlayIdle ();
	}

}
