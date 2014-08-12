﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"AnimEngine_SpritesUnity.cs"
 * 
 *	This script uses Unity's built-in 2D
 *	sprite engine for animation.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AnimEngine_SpritesUnity : AnimEngine
{
	
	public override void Declare (AC.Char _character)
	{
		character = _character;
		turningIsLinear = true;
		rootMotion = false;
		isSpriteBased = true;
	}
	
	
	public override void CharSettingsGUI ()
	{
		#if UNITY_EDITOR
		
		EditorGUILayout.LabelField ("Standard 2D animations:", EditorStyles.boldLabel);
		
		character.talkingAnimation = (TalkingAnimation) EditorGUILayout.EnumPopup ("Talk animation style:", character.talkingAnimation);
		character.spriteChild = (Transform) EditorGUILayout.ObjectField ("Sprite child:", character.spriteChild, typeof (Transform), true);
		character.idleAnimSprite = EditorGUILayout.TextField ("Idle name:", character.idleAnimSprite);
		character.walkAnimSprite = EditorGUILayout.TextField ("Walk name:", character.walkAnimSprite);
		character.runAnimSprite = EditorGUILayout.TextField ("Run name:", character.runAnimSprite);
		if (character.talkingAnimation == TalkingAnimation.Standard)
		{
			character.talkAnimSprite = EditorGUILayout.TextField ("Talk name:", character.talkAnimSprite);
		}
		character.doDirections = EditorGUILayout.Toggle ("Multiple directions?", character.doDirections);
		if (character.doDirections)
		{
			character.doDiagonals = EditorGUILayout.Toggle ("Diagonal sprites?", character.doDiagonals);
			character.frameFlipping = (AC_2DFrameFlipping) EditorGUILayout.EnumPopup ("Frame flipping:", character.frameFlipping);
		}
		character.crossfadeAnims = EditorGUILayout.Toggle ("Crossfade animation?", character.crossfadeAnims);
		
		#endif
	}
	
	
	public override void ActionCharAnimGUI (ActionCharAnim action)
	{
		#if UNITY_EDITOR

		action.method = (ActionCharAnim.AnimMethodChar) EditorGUILayout.EnumPopup ("Method:", action.method);
		
		if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
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
		else if (action.method == ActionCharAnim.AnimMethodChar.StopCustom)
		{
			EditorGUILayout.HelpBox ("This Action does not work for Sprite-based characters.", MessageType.Info);
		}
		else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
		{
			action.clip2D = EditorGUILayout.TextField ("Clip:", action.clip2D);
			action.standard = (AnimStandard) EditorGUILayout.EnumPopup ("Change:", action.standard);

			if (action.standard == AnimStandard.Walk || action.standard == AnimStandard.Run)
			{
				action.changeSound = EditorGUILayout.Toggle ("Change sound?", action.changeSound);
				if (action.changeSound)
				{
					action.newSound = (AudioClip) EditorGUILayout.ObjectField ("New sound:", action.newSound, typeof (AudioClip), false);
				}
				action.changeSpeed = EditorGUILayout.Toggle ("Change speed?", action.changeSpeed);
				if (action.changeSpeed)
				{
					action.newSpeed = EditorGUILayout.FloatField ("New speed:", action.newSpeed);
				}
			}
		}
		else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
		{
			action.idleAfterCustom = EditorGUILayout.Toggle ("Wait for animation to finish?", action.idleAfterCustom);
		}
		
		#endif
	}


	public override float ActionCharAnimRun (ActionCharAnim action)
	{
		string clip2DNew = action.clip2D;
		if (action.includeDirection)
		{
			clip2DNew += action.animChar.GetSpriteDirection ();
		}
		
		if (!action.isRunning)
		{
			action.isRunning = true;
			
			if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom && action.clip2D != "")
			{
				if (action.animChar.spriteChild && action.animChar.spriteChild.GetComponent <Animator>())
				{
					action.animChar.charState = CharState.Custom;
					action.animChar.spriteChild.GetComponent <Animator>().CrossFade (clip2DNew, action.fadeTime, action.layerInt);
				}
			}
			
			else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
			{
				if (action.idleAfterCustom)
				{
					action.layerInt = 0;
					return (action.defaultPauseTime);
				}
				else
				{
					action.animChar.ResetBaseClips ();
					action.animChar.charState = CharState.Idle;
				}
			}
			
			else if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
			{
				if (action.clip2D != "")
				{
					if (action.standard == AnimStandard.Idle)
					{
						action.animChar.idleAnimSprite = action.clip2D;
					}
					else if (action.standard == AnimStandard.Walk)
					{
						action.animChar.walkAnimSprite = action.clip2D;
					}
					else if (action.standard == AnimStandard.Talk)
					{
						action.animChar.talkAnimSprite = action.clip2D;
					}
					else if (action.standard == AnimStandard.Run)
					{
						action.animChar.runAnimSprite = action.clip2D;
					}
				}
				
				if (action.changeSpeed)
				{
					if (action.standard == AnimStandard.Walk)
					{
						action.animChar.walkSpeedScale = action.newSpeed;
					}
					else if (action.standard == AnimStandard.Run)
					{
						action.animChar.runSpeedScale = action.newSpeed;
					}
				}
				
				if (action.changeSound)
				{
					if (action.standard == AnimStandard.Walk)
					{
						if (action.newSound != null)
						{
							action.animChar.walkSound = action.newSound;
						}
						else
						{
							action.animChar.walkSound = null;
						}
					}
					else if (action.standard == AnimStandard.Run)
					{
						if (action.newSound != null)
						{
							action.animChar.runSound = action.newSound;
						}
						else
						{
							action.animChar.runSound = null;
						}
					}
				}
			}
			
			if (action.willWait && action.clip2D != "")
			{
				if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
				{
					return (action.defaultPauseTime);
				}
			}
		}	
		
		else
		{
			if (action.animChar.spriteChild && action.animChar.spriteChild.GetComponent <Animator>())
			{
				// Calc how much longer left to wait
				float totalLength = action.animChar.spriteChild.GetComponent <Animator>().GetCurrentAnimatorStateInfo (action.layerInt).length;
				float timeLeft = (1f - action.animChar.spriteChild.GetComponent <Animator>().GetCurrentAnimatorStateInfo (action.layerInt).normalizedTime) * totalLength;
				
				// Subtract a small amount of time to prevent overshooting
				timeLeft -= 0.1f;
				
				if (timeLeft > 0f)
				{
					return (timeLeft);
				}
				else
				{
					if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
					{
						action.animChar.ResetBaseClips ();
						action.animChar.charState = CharState.Idle;
					}
					else if (action.idleAfter)
					{
						action.animChar.charState = CharState.Idle;
					}
					
					action.isRunning = false;
					return 0f;
				}
			}
			else
			{
				action.isRunning = false;
				action.animChar.charState = CharState.Idle;
				return 0f;
			}
		}
		
		return 0f;
	}
	
	
	public override void ActionCharAnimSkip (ActionCharAnim action)
	{
		if (action.method == ActionCharAnim.AnimMethodChar.SetStandard)
		{
			ActionCharAnimRun (action);
			return;
		}
		else if (action.method == ActionCharAnim.AnimMethodChar.ResetToIdle)
		{
			action.animChar.ResetBaseClips ();
			action.animChar.charState = CharState.Idle;
			return;
		}

		string clip2DNew = action.clip2D;
		if (action.includeDirection)
		{
			clip2DNew += action.animChar.GetSpriteDirection ();
		}

		if (action.method == ActionCharAnim.AnimMethodChar.PlayCustom)
		{
			if (action.willWait && action.idleAfter)
			{
				action.animChar.charState = CharState.Idle;
			}
			else
			{
				action.animChar.charState = CharState.Custom;
				action.animChar.spriteChild.GetComponent <Animator>().Play (clip2DNew, action.layerInt, 0.8f);
			}
		}
	}
	
	
	public override void ActionSpeechGUI (ActionSpeech action)
	{
		#if UNITY_EDITOR
		
		if (action.speaker.talkingAnimation == TalkingAnimation.CustomFace)
		{
			action.play2DHeadAnim = EditorGUILayout.BeginToggleGroup ("Custom head animation?", action.play2DHeadAnim);
			action.headClip2D = EditorGUILayout.TextField ("Head animation:", action.headClip2D);
			action.headLayer = EditorGUILayout.IntField ("Mecanim layer:", action.headLayer);
			EditorGUILayout.EndToggleGroup ();
			
			action.play2DMouthAnim = EditorGUILayout.BeginToggleGroup ("Custom mouth animation?", action.play2DMouthAnim);
			action.mouthClip2D = EditorGUILayout.TextField ("Mouth animation:", action.mouthClip2D);
			action.mouthLayer = EditorGUILayout.IntField ("Mecanim layer:", action.mouthLayer);
			EditorGUILayout.EndToggleGroup ();
		}
		
		#endif
	}
	
	
	public override void ActionSpeechRun (ActionSpeech action)
	{
		if (action.speaker.talkingAnimation == TalkingAnimation.CustomFace)
		{
			if (action.play2DHeadAnim && action.headClip2D != "")
			{
				try
				{
					action.speaker.GetComponent <Animator>().Play (action.headClip2D, action.headLayer);
				}
				catch {}
			}
			
			if (action.play2DMouthAnim && action.mouthClip2D != "")
			{
				try
				{
					action.speaker.GetComponent <Animator>().Play (action.mouthClip2D, action.mouthLayer);
				}
				catch {}
			}
		}
	}
	
	
	public override void ActionAnimGUI (ActionAnim action)
	{
		#if UNITY_EDITOR

		action.method = (AnimMethod) EditorGUILayout.EnumPopup ("Method:", action.method);

		if (action.method == AnimMethod.PlayCustom)
		{
			if (action.isAssetFile)
			{
				action.constantID = EditorGUILayout.IntField ("Animator object (ID):", action.constantID);
			}
			else
			{
				action.animator = (Animator) EditorGUILayout.ObjectField ("Object:", action.animator, typeof (Animator), true);
			}

			action.clip2D = EditorGUILayout.TextField ("Clip:", action.clip2D);
			action.layerInt = EditorGUILayout.IntField ("Mecanim layer:", action.layerInt);
			action.fadeTime = EditorGUILayout.Slider ("Transition time:", action.fadeTime, 0f, 2f);
			action.willWait = EditorGUILayout.Toggle ("Pause until finish?", action.willWait);
		}
		else if (action.method == AnimMethod.StopCustom)
		{
			EditorGUILayout.HelpBox ("'Stop Custom' is not available for Unity-based 2D animation.", MessageType.Info);
		}
		else if (action.method == AnimMethod.BlendShape)
		{
			EditorGUILayout.HelpBox ("BlendShapes are not available in 2D animation.", MessageType.Info);
		}
		
		#endif
	}
	
	
	public override string ActionAnimLabel (ActionAnim action)
	{
		string label = "";
		
		if (action.animator)
		{
			label = action.animator.name;
			
			if (action.method == AnimMethod.PlayCustom && action.clip2D != "")
			{
				label += " - " + action.clip2D;
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
			
			if (action.animator && action.clip2D != "")
			{
				if (action.method == AnimMethod.PlayCustom)
				{
					action.animator.CrossFade (action.clip2D, action.fadeTime, action.layerInt);
					
					if (action.willWait)
					{
						return (action.defaultPauseTime);
					}
				}
				else if (action.method == AnimMethod.BlendShape)
				{
					Debug.LogWarning ("BlendShapes not available for 2D animation.");
					return 0f;
				}
			}
		}
		else
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
		
		return 0f;
	}


	public override void ActionAnimSkip (ActionAnim action)
	{
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

		if (action.animator && action.clip2D != "")
		{
			if (action.method == AnimMethod.PlayCustom)
			{
				action.animator.Play (action.clip2D, action.layerInt, 0.8f);
			}
		}
	}


	public override void ActionCharRenderGUI (ActionCharRender action)
	{
		#if UNITY_EDITOR

		EditorGUILayout.Space ();
		action.renderLock_scale = (RenderLock) EditorGUILayout.EnumPopup ("Sprite scale:", action.renderLock_scale);
		if (action.renderLock_scale == RenderLock.Set)
		{
			action.scale = EditorGUILayout.IntField ("New scale (%):", action.scale);
		}

		EditorGUILayout.Space ();
		action.renderLock_direction = (RenderLock) EditorGUILayout.EnumPopup ("Sprite direction:", action.renderLock_direction);
		if (action.renderLock_direction == RenderLock.Set)
		{
			action.direction = (CharDirection) EditorGUILayout.EnumPopup ("New direction:", action.direction);
		}

		#endif
	}


	public override float ActionCharRenderRun (ActionCharRender action)
	{
		if (action.renderLock_scale == RenderLock.Set)
		{
			action._char.lockScale = true;
			action._char.spriteScale = (float) action.scale / 100f;
		}
		else if (action.renderLock_scale == RenderLock.Release)
		{
			action._char.lockScale = false;
		}

		if (action.renderLock_direction == RenderLock.Set)
		{
			action._char.lockDirection = true;
			action._char.SetSpriteDirection (action.direction);
		}
		else if (action.renderLock_direction == RenderLock.Release)
		{
			action._char.lockDirection = false;
		}

		return 0f;
	}
	
	
	public override void PlayIdle ()
	{
		PlayStandardAnim (character.idleAnimSprite, character.doDirections);
	}
	
	
	public override void PlayWalk ()
	{
		PlayStandardAnim (character.walkAnimSprite, character.doDirections);
	}
	
	
	public override void PlayRun ()
	{
		if (character.runAnimSprite != "")
		{
			PlayStandardAnim (character.runAnimSprite, character.doDirections);
		}
		else
		{
			PlayWalk ();
		}
	}
	
	
	public override void PlayTalk ()
	{
		PlayStandardAnim (character.talkAnimSprite, character.doDirections);
	}
	
	
	private void PlayStandardAnim (string clip, bool includeDirection)
	{
		if (character && character.animator && clip != "")
		{
			if (includeDirection)
			{
				clip += character.GetSpriteDirection ();
			}
			
			if (character.crossfadeAnims)
			{
				try
				{
					character.animator.CrossFade (clip, character.animCrossfadeSpeed, 0);
				}
				catch
				{
					Debug.LogError ("Cannot play clip " + clip + " on " + character.name);
				}
			}
			else
			{
				try
				{
					character.animator.Play (clip, 0);
				}
				catch
				{
					Debug.LogError ("Cannot play clip " + clip + " on " + character.name);
				}
			}
		}
	}
	
}

