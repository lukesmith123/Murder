/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MenuButton.cs"
 * 
 *	This MenuElement can be clicked on to perform a function defined in MenuSystem.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;	
#endif

namespace AC
{

	[System.Serializable]
	public class MenuButton : MenuElement
	{
		
		public string label = "Element";
		public TextAnchor anchor;
		public bool doOutline;
		public AC_ButtonClickType buttonClickType;
		public SimulateInputType simulateInput = SimulateInputType.Button;
		public float simulateValue = 1f;
		public bool doFade;
		public string switchMenuTitle;
		public string inventoryBoxTitle;
		public AC_ShiftInventory shiftInventory;
		public bool loopJournal = false;
		public MenuActionList actionList;
		public string inputAxis;

		
		public override void Declare ()
		{
			label = "Button";
			isVisible = true;
			isClickable = true;
			doOutline = false;
			buttonClickType = AC_ButtonClickType.CustomScript;
			simulateInput = SimulateInputType.Button;
			simulateValue = 1f;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize (new Vector2 (10f, 5f));
			doFade = false;
			switchMenuTitle = "";
			inventoryBoxTitle = "";
			shiftInventory = AC_ShiftInventory.ShiftLeft;
			loopJournal = false;
			actionList = null;
			inputAxis = "";
			
			base.Declare ();
		}
		
		
		public void CopyButton (MenuButton _element)
		{
			label = _element.label;
			anchor = _element.anchor;
			doOutline = _element.doOutline;
			buttonClickType = _element.buttonClickType;
			simulateInput = _element.simulateInput;
			simulateValue = _element.simulateValue;
			doFade = _element.doFade;
			switchMenuTitle = _element.switchMenuTitle;
			inventoryBoxTitle = _element.inventoryBoxTitle;
			shiftInventory = _element.shiftInventory;
			loopJournal = _element.loopJournal;
			actionList = _element.actionList;
			inputAxis = _element.inputAxis;
					
			base.Copy (_element);
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
				label = EditorGUILayout.TextField ("Button text:", label);
				anchor = (TextAnchor) EditorGUILayout.EnumPopup ("Text alignment:", anchor);
				doOutline = EditorGUILayout.Toggle ("Outline text?", doOutline);
				buttonClickType = (AC_ButtonClickType) EditorGUILayout.EnumPopup ("Click type:", buttonClickType);
			
				if (buttonClickType == AC_ButtonClickType.TurnOffMenu)
				{
					doFade = EditorGUILayout.Toggle ("Do transition?", doFade);
				}
				else if (buttonClickType == AC_ButtonClickType.Crossfade)
				{
					switchMenuTitle = EditorGUILayout.TextField ("Menu to switch to:", switchMenuTitle);
				}
				else if (buttonClickType == AC_ButtonClickType.OffsetInventory)
				{
					inventoryBoxTitle = EditorGUILayout.TextField ("InventoryBox to affect:", inventoryBoxTitle);
					shiftInventory = (AC_ShiftInventory) EditorGUILayout.EnumPopup ("Offset type:", shiftInventory);
				}
				else if (buttonClickType == AC_ButtonClickType.OffsetJournal)
				{
					inventoryBoxTitle = EditorGUILayout.TextField ("Journal to affect:", inventoryBoxTitle);
					shiftInventory = (AC_ShiftInventory) EditorGUILayout.EnumPopup ("Offset type:", shiftInventory);
					loopJournal = EditorGUILayout.Toggle ("Cycle pages?", loopJournal);
				}
				else if (buttonClickType == AC_ButtonClickType.RunActionList)
				{
					actionList = (MenuActionList) EditorGUILayout.ObjectField ("ActionList to run:", actionList, typeof (MenuActionList), false);
				}
				else if (buttonClickType == AC_ButtonClickType.CustomScript)
				{
					ShowClipHelp ();
				}
				else if (buttonClickType == AC_ButtonClickType.SimulateInput)
				{
					simulateInput = (SimulateInputType) EditorGUILayout.EnumPopup ("Simulate:", simulateInput);
					inputAxis = EditorGUILayout.TextField ("Input axis:", inputAxis);
					if (simulateInput == SimulateInputType.Axis)
					{
						simulateValue = EditorGUILayout.FloatField ("Input value:", simulateValue);
					}
				}
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI ();
		}
		
		#endif
		
		
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);

			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}
			
			if (doOutline)
			{
				AdvGame.DrawTextOutline (ZoomRect (relativeRect, zoom), TranslateLabel (label), _style, Color.black, _style.normal.textColor, 2);
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), TranslateLabel (label), _style);
			}
		}


		public override string GetLabel (int slot)
		{
			return TranslateLabel (label);
		}

		
		protected override void AutoSize ()
		{
			if (label == "" && backgroundTexture != null)
			{
				GUIContent content = new GUIContent (backgroundTexture);
				AutoSize (content);
			}
			else
			{
				GUIContent content = new GUIContent (TranslateLabel (label));
				AutoSize (content);
			}
		}
		
	}

}