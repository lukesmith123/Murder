/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"AnimEngine_Mecanim.cs"
 * 
 *	This script uses the Mecanim
 *	system for 3D animation.
 * 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AnimEngine_Mecanim : AnimEngine
{

	public override void Declare (AC.Char _character)
	{
		character = _character;
		turningIsLinear = false;
		rootMotion = true;
	}


	public override void CharSettingsGUI ()
	{
		#if UNITY_EDITOR
		
		EditorGUILayout.LabelField ("Mecanim parameters:", EditorStyles.boldLabel);

		if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.IsTopDown ())
		{
			character.spriteChild = (Transform) EditorGUILayout.ObjectField ("Animator child:", character.spriteChild, typeof (Transform), true);
		}
		else
		{
			character.spriteChild = null;
		}

		character.moveSpeedParameter = EditorGUILayout.TextField ("Move speed float:", character.moveSpeedParameter);
		character.turnParameter = EditorGUILayout.TextField ("Turn float:", character.turnParameter);
		character.talkParameter = EditorGUILayout.TextField ("Talk bool:", character.talkParameter);
		character.talkingAnimation = TalkingAnimation.Standard;
		character.relyOnRootMotion = EditorGUILayout.Toggle ("Rely on Root Motion?", character.relyOnRootMotion);

		EditorGUILayout.EndVertical ();
		EditorGUILayout.BeginVertical ("Button");
		EditorGUILayout.LabelField ("Bone transforms:", EditorStyles.boldLabel);
		
		character.leftHandBone = (Transform) EditorGUILayout.ObjectField ("Left hand:", character.leftHandBone, typeof (Transform), true);
		character.rightHandBone = (Transform) EditorGUILayout.ObjectField ("Right hand:", character.rightHandBone, typeof (Transform), true);
		
		#endif
	}


	public override void ActionCharAnimGUI (ActionCharAnim action)
	{
		#if UNITY_EDITOR

		action.methodMecanim = (AnimMethodCharMecanim) EditorGUILayout.EnumPopup ("Method:", action.methodMecanim);
		
		if (action.methodMecanim == AnimMethodCharMecanim.ChangeParameterValue)
		{
			action.parameterName = EditorGUILayout.TextField ("Parameter to affect:", action.parameterName);
			action.mecanimParameterType = (MecanimParameterType) EditorGUILayout.EnumPopup ("Parameter type:", action.mecanimParameterType);
			if (action.mecanimParameterType != MecanimParameterType.Trigger)
			{
				action.parameterValue = EditorGUILayout.FloatField ("Set as value:", action.parameterValue);
			}
		}

		else if (action.methodMecanim == AnimMethodCharMecanim.SetStandard)
		{
			action.mecanimCharParameter = (MecanimCharParameter) EditorGUILayout.EnumPopup ("Parameter to change:", action.mecanimCharParameter);
			action.parameterName = EditorGUILayout.TextField ("New parameter name:", action.parameterName);
		}

		else if (action.methodMecanim == AnimMethodCharMecanim.PlayCustom)
		{
			action.clip2D = EditorGUILayout.TextField ("Clip:", action.clip2D);
			action.includeDirection = EditorGUILayout.Toggle ("Add directional suffix?", action.includeDirection);
			
			action.layerInt = EditorGUILayout.IntField ("Mecanim layer:", action.layerInt);
			action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 1f);
			action.willWait = EditorGUILayout.Toggle ("Pause until finish?", action.willWait);
			if (action.willWait)
			{
				action.idleAfter = EditorGUILayout.Toggle ("Return to idle after?", action.idleAfter);
			}
		}

		#endif
	}
	
	
	public override float ActionCharAnimRun (ActionCharAnim action)
	{
		if (action.methodMecanim == AnimMethodCharMecanim.SetStandard)
		{
			if (action.mecanimCharParameter == MecanimCharParameter.MoveSpeedFloat)
			{
				action.animChar.moveSpeedParameter = action.parameterName;
			}
			else if (action.mecanimCharParameter == MecanimCharParameter.TalkBool)
			{
				action.animChar.talkParameter = action.parameterName;
			}
			else if (action.mecanimCharParameter == MecanimCharParameter.TurnFloat)
			{
				action.animChar.turnParameter = action.parameterName;
			}
			
			return 0f;
		}

		Animator animator = null;
		
		if (character.spriteChild && character.spriteChild.GetComponent <Animator>())
		{
			animator = character.spriteChild.GetComponent <Animator>();
		}
		if (character.GetComponent <Animator>())
		{
			animator = character.GetComponent <Animator>();
		}
		
		if (animator == null)
		{
			return 0f;
		}
		
		if (!action.isRunning)
		{
			action.isRunning = true;
			if (action.methodMecanim == AnimMethodCharMecanim.ChangeParameterValue)
			{
				if (action.parameterName != "")
				{
					if (action.mecanimParameterType == MecanimParameterType.Float)
					{
						animator.SetFloat (action.parameterName, action.parameterValue);
					}
					else if (action.mecanimParameterType == MecanimParameterType.Int)
					{
						animator.SetInteger (action.parameterName, (int) action.parameterValue);
					}
					else if (action.mecanimParameterType == MecanimParameterType.Bool)
					{
						bool paramValue = false;
						if (action.parameterValue > 0f)
						{
							paramValue = true;
						}
						animator.SetBool (action.parameterName, paramValue);
					}
					else if (action.mecanimParameterType == MecanimParameterType.Trigger)
					{
						animator.SetTrigger (action.parameterName);
					}
				}
			}
			else if (action.methodMecanim == AnimMethodCharMecanim.PlayCustom && animator)
			{
				if (action.clip2D != "")
				{
					string clip2DNew = action.clip2D;
					if (action.includeDirection)
					{
						clip2DNew += action.animChar.GetSpriteDirection ();
					}

					animator.CrossFade (clip2DNew, action.fadeTime, action.layerInt);
					
					if (action.willWait)
					{
						return (action.defaultPauseTime);
					}
				}
			}
		}
		else
		{
			if (action.methodMecanim == AnimMethodCharMecanim.PlayCustom)
			{
				if (animator && action.clip2D != "")
				{
					if (animator.GetCurrentAnimatorStateInfo (action.layerInt).normalizedTime < 0.98f)
					{
						return (action.defaultPauseTime / 6f);
					}
					else
					{
						action.isRunning = false;
						return 0f;
					}
				}
			}
		}
		
		return 0f;
	}
	
	
	public override void ActionCharAnimSkip (ActionCharAnim action)
	{
		if (action.methodMecanim != AnimMethodCharMecanim.ChangeParameterValue)
		{
			ActionCharAnimRun (action);
		}
	}


	public override void ActionCharHoldGUI (ActionCharHold action)
	{
		#if UNITY_EDITOR
		
		if (action.isAssetFile)
		{
			action.objectToHoldID = EditorGUILayout.IntField ("Object to hold (ID):", action.objectToHoldID);
		}
		else
		{
			action.objectToHold = (GameObject) EditorGUILayout.ObjectField ("Object to hold:", action.objectToHold, typeof (GameObject), true);
		}
		action.hand = (ActionCharHold.Hand) EditorGUILayout.EnumPopup ("Hand:", action.hand);
		action.rotate90 = EditorGUILayout.Toggle ("Rotate 90 degrees?", action.rotate90);
		
		#endif
	}
	
	
	public override void ActionCharHoldRun (ActionCharHold action)
	{
		if (action.objectToHold)
		{
			Transform handTransform;
			
			if (action.hand == ActionCharHold.Hand.Left)
			{
				handTransform = action._char.leftHandBone;
			}
			else
			{
				handTransform = action._char.rightHandBone;
			}
			
			if (handTransform)
			{
				action.objectToHold.transform.parent = handTransform;
				action.objectToHold.transform.localPosition = Vector3.zero;
				
				if (action.rotate90)
				{
					action.objectToHold.transform.localEulerAngles = new Vector3 (0f, 0f, 90f);
				}
				else
				{
					action.objectToHold.transform.localEulerAngles = Vector3.zero;
				}
			}
			else
			{
				Debug.Log ("Cannot parent object - no hand bone found.");
			}
		}
	}


	public override void ActionAnimGUI (ActionAnim action)
	{
		#if UNITY_EDITOR

		action.methodMecanim = (AnimMethodMecanim) EditorGUILayout.EnumPopup ("Method:", action.methodMecanim);

		if (action.methodMecanim == AnimMethodMecanim.ChangeParameterValue || action.methodMecanim == AnimMethodMecanim.PlayCustom)
		{
			if (action.isAssetFile)
			{
				action.constantID = EditorGUILayout.IntField ("Animator (ID):", action.constantID);
			}
			else
			{
				action.animator = (Animator) EditorGUILayout.ObjectField ("Animator:", action.animator, typeof (Animator), true);
			}
		}

		if (action.methodMecanim == AnimMethodMecanim.ChangeParameterValue)
		{
			action.parameterName = EditorGUILayout.TextField ("Parameter to affect:", action.parameterName);
			action.mecanimParameterType = (MecanimParameterType) EditorGUILayout.EnumPopup ("Parameter type:", action.mecanimParameterType);
			if (action.mecanimParameterType != MecanimParameterType.Trigger)
			{
				action.parameterValue = EditorGUILayout.FloatField ("Set as value:", action.parameterValue);
			}
		}
		else if (action.methodMecanim == AnimMethodMecanim.PlayCustom)
		{
			action.clip2D = EditorGUILayout.TextField ("Clip:", action.clip2D);
			action.layerInt = EditorGUILayout.IntField ("Mecanim layer:", action.layerInt);
			action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 2f);
			action.willWait = EditorGUILayout.Toggle ("Pause until finish?", action.willWait);
		}
		else if (action.methodMecanim == AnimMethodMecanim.BlendShape)
		{
			if (action.isAssetFile)
			{
				action.constantID = EditorGUILayout.IntField ("Shapeable object (ID):", action.constantID);
			}
			else
			{
				action.isPlayer = EditorGUILayout.Toggle ("Is player?", action.isPlayer);
				if (!action.isPlayer)
				{
					action.shapeObject = (Shapeable) EditorGUILayout.ObjectField ("Object:", action.shapeObject, typeof (Shapeable), true);
				}
			}

			action.shapeKey = EditorGUILayout.IntField ("Shape key:", action.shapeKey);
			action.shapeValue = EditorGUILayout.Slider ("Shape value:", action.shapeValue, 0f, 100f);
			action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 2f);
			action.willWait = EditorGUILayout.Toggle ("Pause until finish?", action.willWait);
		}
		
		#endif
	}


	public override string ActionAnimLabel (ActionAnim action)
	{
		string label = "";
		
		if (action.animator)
		{
			label = action.animator.name;
			
			if (action.methodMecanim == AnimMethodMecanim.ChangeParameterValue && action.parameterName != "")
			{
				label += " - " + action.parameterName;
			}
			else if (action.methodMecanim == AnimMethodMecanim.BlendShape)
			{
				label += " - Shapekey";
			}
		}
		
		return label;
	}


	public override float ActionAnimRun (ActionAnim action)
	{
		if (!action.isRunning)
		{
			action.isRunning = true;
			
			if (action.isAssetFile && action.constantID != 0)
			{
				// Attempt to find the correct scene object
				ConstantID idObject = Serializer.returnComponent <ConstantID> (action.constantID);
				if (idObject != null && idObject.GetComponent <Animator>())
				{
					action.animator = idObject.GetComponent <Animator>();
				}
				else
				{
					action.animator = null;
				}
			}

			if (action.methodMecanim == AnimMethodMecanim.ChangeParameterValue && action.animator && action.parameterName != "")
			{
				if (action.mecanimParameterType == MecanimParameterType.Float)
				{
					action.animator.SetFloat (action.parameterName, action.parameterValue);
				}
				else if (action.mecanimParameterType == MecanimParameterType.Int)
				{
					action.animator.SetInteger (action.parameterName, (int) action.parameterValue);
				}
				else if (action.mecanimParameterType == MecanimParameterType.Bool)
				{
					bool paramValue = false;
					if (action.parameterValue > 0f)
					{
						paramValue = true;
					}
					action.animator.SetBool (action.parameterName, paramValue);
				}
				else if (action.mecanimParameterType == MecanimParameterType.Trigger)
				{
					action.animator.SetTrigger (action.parameterName);
				}
				
				return 0f;
			}

			else if (action.methodMecanim == AnimMethodMecanim.PlayCustom && action.animator)
			{
				if (action.clip2D != "")
				{
					action.animator.CrossFade (action.clip2D, action.fadeTime, action.layerInt);

					if (action.willWait)
					{
						return (action.defaultPauseTime);
					}
				}
			}
			
			else if (action.methodMecanim == AnimMethodMecanim.BlendShape && action.shapeKey > -1)
			{
				if (action.isAssetFile && action.constantID != 0)
				{
					// Attempt to find the correct scene object
					ConstantID idObject = Serializer.returnComponent <ConstantID> (action.constantID);
					if (idObject != null && idObject.GetComponent <Shapeable>())
					{
						action.shapeObject = idObject.GetComponent <Shapeable>();
					}
					else
					{
						action.shapeObject = null;
					}
				}
				
				if (action.shapeObject)
				{
					action.shapeObject.Change (action.shapeKey, action.shapeValue, action.fadeTime);
					
					if (action.willWait)
					{
						return (action.defaultPauseTime);
					}
				}
			}
		}
		else
		{
			if (action.methodMecanim == AnimMethodMecanim.BlendShape && action.shapeObject)
			{
				if (!action.shapeObject.IsChanging ())
				{
					action.isRunning = false;
					return 0f;
				}
				else
				{
					return action.defaultPauseTime;
				}
			}
			else if (action.methodMecanim == AnimMethodMecanim.PlayCustom)
			{
				if (action.animator && action.clip2D != "")
				{
					if (action.animator.GetCurrentAnimatorStateInfo (action.layerInt).normalizedTime < 1f)
					{
						return (action.defaultPauseTime / 6f);
					}
					else
					{
						action.isRunning = false;
						return 0f;
					}
				}
			}
		}
		
		return 0f;
	}


	public override void ActionAnimSkip (ActionAnim action)
	{
		if (action.methodMecanim == AnimMethodMecanim.BlendShape)
		{
			if (action.isAssetFile && action.constantID != 0)
			{
				// Attempt to find the correct scene object
				ConstantID idObject = Serializer.returnComponent <ConstantID> (action.constantID);
				if (idObject != null && idObject.GetComponent <Shapeable>())
				{
					action.shapeObject = idObject.GetComponent <Shapeable>();
				}
				else
				{
					action.shapeObject = null;
				}
			}
			
			if (action.shapeObject)
			{
				action.shapeObject.Change (action.shapeKey, action.shapeValue, action.fadeTime);
			}
		}
		else if (action.methodMecanim == AnimMethodMecanim.PlayCustom)
		{
			ActionAnimRun (action);
		}
	}


	public override void ActionCharRenderGUI (ActionCharRender action)
	{
		#if UNITY_EDITOR
		
		EditorGUILayout.Space ();
		action.renderLock_scale = (RenderLock) EditorGUILayout.EnumPopup ("Character scale:", action.renderLock_scale);
		if (action.renderLock_scale == RenderLock.Set)
		{
			action.scale = EditorGUILayout.IntField ("New scale (%):", action.scale);
		}
		
		#endif
	}
	
	
	public override float ActionCharRenderRun (ActionCharRender action)
	{
		if (action.renderLock_scale == RenderLock.Set)
		{
			action._char.lockScale = true;
			
			if (action._char.spriteChild != null)
			{
				action._char.spriteScale = (float) action.scale / 100f;
			}
			else
			{
				float _scale = (float) action.scale;
				action._char.transform.localScale = new Vector3 (_scale, _scale, _scale);
			}
		}
		else if (action.renderLock_scale == RenderLock.Release)
		{
			action._char.lockScale = false;
		}
		
		return 0f;
	}


	public override void PlayIdle ()
	{
		Animator animator = null;

		if (character.spriteChild && character.spriteChild.GetComponent <Animator>())
		{
			animator = character.spriteChild.GetComponent <Animator>();
		}
		if (character.GetComponent <Animator>())
		{
			animator = character.GetComponent <Animator>();
		}

		if (animator == null)
		{
			return;
		}

		if (character.moveSpeedParameter != "")
		{
			animator.SetFloat (character.moveSpeedParameter, character.GetMoveSpeed ());
		}

		if (character.talkParameter != "")
		{
			animator.SetBool (character.talkParameter, false);
		}

		if (character.turnParameter != "")
		{
			animator.SetFloat (character.turnParameter, 0f);
		}
	}


	public override void PlayWalk ()
	{
		Animator animator = null;
		
		if (character.spriteChild && character.spriteChild.GetComponent <Animator>())
		{
			animator = character.spriteChild.GetComponent <Animator>();
		}
		if (character.GetComponent <Animator>())
		{
			animator = character.GetComponent <Animator>();
		}

		if (animator == null)
		{
			return;
		}

		if (character.moveSpeedParameter != "")
		{
			if (character.isReversing)
			{
				animator.SetFloat (character.moveSpeedParameter, -character.GetMoveSpeed ());
			}
			else
			{
				animator.SetFloat (character.moveSpeedParameter, character.GetMoveSpeed ());
			}
		}
	}


	public override void PlayRun ()
	{
		Animator animator = null;
		
		if (character.spriteChild && character.spriteChild.GetComponent <Animator>())
		{
			animator = character.spriteChild.GetComponent <Animator>();
		}
		if (character.GetComponent <Animator>())
		{
			animator = character.GetComponent <Animator>();
		}

		if (animator == null)
		{
			return;
		}

		if (character.moveSpeedParameter != "")
		{
			if (character.isReversing)
			{
				animator.SetFloat (character.moveSpeedParameter, -character.GetMoveSpeed ());
			}
			else
			{
				animator.SetFloat (character.moveSpeedParameter, character.GetMoveSpeed ());
			}
		}
	}


	public override void PlayTalk ()
	{
		Animator animator = null;
		
		if (character.spriteChild && character.spriteChild.GetComponent <Animator>())
		{
			animator = character.spriteChild.GetComponent <Animator>();
		}
		if (character.GetComponent <Animator>())
		{
			animator = character.GetComponent <Animator>();
		}

		if (animator == null)
		{
			return;
		}

		if (character.talkParameter != "")
		{
			animator.SetBool (character.talkParameter, true);
		}
	}


	public override void PlayTurnLeft ()
	{
		Animator animator = null;
		
		if (character.spriteChild && character.spriteChild.GetComponent <Animator>())
		{
			animator = character.spriteChild.GetComponent <Animator>();
		}
		if (character.GetComponent <Animator>())
		{
			animator = character.GetComponent <Animator>();
		}

		if (animator == null)
		{
			return;
		}

		if (character.turnParameter != "")
		{
			animator.SetFloat (character.turnParameter, -1f);
		}

		if (character.talkParameter != "")
		{
			animator.SetBool (character.talkParameter, false);
		}

		if (character.moveSpeedParameter != "")
		{
			animator.SetFloat (character.moveSpeedParameter, 0f);
		}
	}
	
	
	public override void PlayTurnRight ()
	{
		Animator animator = null;
		
		if (character.spriteChild && character.spriteChild.GetComponent <Animator>())
		{
			animator = character.spriteChild.GetComponent <Animator>();
		}
		if (character.GetComponent <Animator>())
		{
			animator = character.GetComponent <Animator>();
		}
		
		if (animator == null)
		{
			return;
		}

		if (character.turnParameter != "")
		{
			animator.SetFloat (character.turnParameter, 1f);
		}
		
		if (character.talkParameter != "")
		{
			animator.SetBool (character.talkParameter, false);
		}
		
		if (character.moveSpeedParameter != "")
		{
			animator.SetFloat (character.moveSpeedParameter, 0f);
		}
	}

}
