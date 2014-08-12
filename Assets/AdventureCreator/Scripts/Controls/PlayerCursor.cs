/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerCursor.cs"
 * 
 *	This script displays a cursor graphic on the screen.
 *	PlayerInput decides if this should be at the mouse position,
 *	or a position based on controller input.
 *	The cursor graphic changes based on what hotspot is underneath it.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	public class PlayerCursor : MonoBehaviour
	{
		
		private int selectedCursor = -1; // -2 = inventory, -1 = pointer, 0+ = cursor array
		private bool showCursor = false;
		private bool canShowHardwareCursor = false;
		private float pulse = 0f;
		private int pulseDirection = 0; // 0 = none, 1 = in, -1 = out

		private SettingsManager settingsManager;
		private CursorManager cursorManager;
		private StateHandler stateHandler;
		private RuntimeInventory runtimeInventory;
		private PlayerInput playerInput;
		private PlayerInteraction playerInteraction;
		private PlayerMenus playerMenus;


		
		private void Awake ()
		{
			playerInput = this.GetComponent <PlayerInput>();
			playerInteraction = this.GetComponent <PlayerInteraction>();

			if (AdvGame.GetReferences () == null)
			{
				Debug.LogError ("A References file is required - please use the Adventure Creator window to create one.");
			}
			else
			{
				if (AdvGame.GetReferences ().settingsManager)
				{
					settingsManager = AdvGame.GetReferences ().settingsManager;
				}
				if (AdvGame.GetReferences ().cursorManager)
				{
					cursorManager = AdvGame.GetReferences ().cursorManager;
				}
			}
		}
		
		
		private void Start ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
			{
				stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
			}
			
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>())
			{
				runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
			}

			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>())
			{
				playerMenus = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>();
			}
		}
		
		
		private void Update ()
		{
			if (stateHandler && stateHandler.cursorIsOff)
			{
				return;
			}

			if (!canShowHardwareCursor)
			{
				Screen.showCursor = false;
			}
			else if (settingsManager && cursorManager && (!cursorManager.allowMainCursor || cursorManager.pointerTexture == null) && runtimeInventory.selectedItem == null && settingsManager.inputMethod == InputMethod.MouseAndKeyboard && stateHandler.gameState != GameState.Cutscene)
			{
				Screen.showCursor = true;
			}
			else if (cursorManager == null)
			{
				Screen.showCursor = true;
			}
			else
			{
				Screen.showCursor = false;
			}
			
			if (settingsManager && stateHandler)
			{
				if (stateHandler.gameState == GameState.Cutscene)
				{
					if (cursorManager.waitTexture != null)
					{
						showCursor = true;
					}
					else
					{
						showCursor = false;
					}
				}
				else if (stateHandler.gameState != GameState.Normal && settingsManager.inputMethod == InputMethod.KeyboardOrController)
				{
					showCursor = false;
				}
				else if (cursorManager)
				{
					if (stateHandler.gameState == GameState.Paused && (cursorManager.cursorDisplay == CursorDisplay.OnlyWhenPaused || cursorManager.cursorDisplay == CursorDisplay.Always))
					{
						showCursor = true;
					}
					else if ((stateHandler.gameState == GameState.Normal || stateHandler.gameState == GameState.DialogOptions) && (cursorManager.cursorDisplay == CursorDisplay.Always))
					{
						showCursor = true;
					}
					else
					{
						showCursor = false;
					}
				}
				else
			    {
					showCursor = true;
				}
				
				if (stateHandler.gameState == GameState.Normal && settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot &&
				    (playerInput.buttonPressed == 2 || playerInput.InputGetButtonDown ("CycleCursors")) && cursorManager && cursorManager.cycleCursors)
				{
					CycleCursors ();
				}	
			}
			
		}
		
		
		private void OnGUI ()
		{
			if (stateHandler && stateHandler.cursorIsOff)
			{
				return;
			}

			if (playerInput && playerInteraction && stateHandler && settingsManager && cursorManager && runtimeInventory && showCursor)
			{
				if (!playerInput.canMoveMouse && settingsManager.hideLockedCursor)
				{
					canShowHardwareCursor = false;
					return;
				}

				GUI.depth = -1;
				canShowHardwareCursor = true;
				
				if (runtimeInventory.selectedItem != null && cursorManager.inventoryHandling != InventoryHandling.ChangeHotspotLabel && stateHandler.gameState != GameState.Paused)
				{
					// Cursor becomes selected inventory
					selectedCursor = -2;
					canShowHardwareCursor = false;
				}
				else if (settingsManager.interactionMethod != AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					if (settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive && cursorManager.allowInteractionCursorForInventory && playerMenus.GetHoverInventory () != null)
					{
						ShowContextIcons (playerMenus.GetHoverInventory ());
						return;
					}
					else if (playerInteraction.hotspot && stateHandler.gameState == GameState.Normal && (playerInteraction.hotspot.HasContextUse () || playerInteraction.hotspot.HasContextLook ()) && cursorManager.allowInteractionCursor)
					{
						canShowHardwareCursor = false;
						selectedCursor = 0;

						if (settingsManager.interactionMethod == AC_InteractionMethod.ContextSensitive)
						{
							ShowContextIcons ();
						}
					}
					else
					{
						selectedCursor = -1;
					}
				}
				else if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && (stateHandler.gameState == GameState.DialogOptions || stateHandler.gameState == GameState.Paused))
				{
					selectedCursor = -1;
				}

				if (stateHandler.gameState == GameState.Cutscene && cursorManager.waitTexture != null)
				{
					// Wait
					GUI.DrawTexture (AdvGame.GUIBox (playerInput.mousePosition, cursorManager.iconCursorSize), cursorManager.waitTexture, ScaleMode.ScaleToFit, true, 0f);
				}
				else if (selectedCursor == -2 && runtimeInventory.selectedItem != null && (settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction || settingsManager.inventoryInteractions == InventoryInteractions.Single))
				{
					// Inventory
					canShowHardwareCursor = false;

					if (settingsManager.inventoryActiveEffect != InventoryActiveEffect.None && runtimeInventory.selectedItem.activeTex && playerMenus.GetHotspotLabel () != "" &&
					   (settingsManager.activeWhenUnhandled || playerInteraction.DoesHotspotHaveInventoryInteraction () || (playerMenus.GetHoverInventory () != null && playerMenus.GetHoverInventory ().DoesHaveInventoryInteraction (runtimeInventory.selectedItem))))
					{
						DrawActiveCursor ();
					}
					else if (runtimeInventory.selectedItem.tex)
					{
						GUI.DrawTexture (AdvGame.GUIBox (playerInput.mousePosition, cursorManager.inventoryCursorSize), runtimeInventory.selectedItem.tex, ScaleMode.ScaleToFit, true, 0f);
						pulseDirection = 0;
					}
					else
					{
						selectedCursor = -1;
						runtimeInventory.SetNull ();
						pulseDirection = 0;
					}
				}
				else if (selectedCursor >= 0 && settingsManager.interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot && cursorManager.allowInteractionCursor)
				{
					//	Custom icon

					pulseDirection = 0;
					canShowHardwareCursor = false;

					if (cursorManager.cursorIcons [selectedCursor].texture)
					{
						GUI.DrawTexture (AdvGame.GUIBox (playerInput.mousePosition, cursorManager.iconCursorSize), cursorManager.cursorIcons [selectedCursor].texture, ScaleMode.ScaleToFit, true, 0f);
					}
				}
				else if (cursorManager.allowMainCursor || settingsManager.inputMethod == InputMethod.KeyboardOrController)
				{
					// Pointer

					pulseDirection = 0;

					if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && playerInteraction.hotspot != null && !playerInput.interactionMenuIsOn)
					{
						if (playerInteraction.hotspot.IsSingleInteraction ())
						{
							ShowContextIcons ();
						}
						else if (cursorManager.mouseOverTexture)
						{
							GUI.DrawTexture (AdvGame.GUIBox (playerInput.mousePosition, cursorManager.mouseOverCursorSize), cursorManager.mouseOverTexture, ScaleMode.ScaleToFit, true, 0f);
						}
						else
						{
							DrawMainCursor ();
						}
						/*else if (cursorManager.pointerTexture)
						{
							GUI.DrawTexture (AdvGame.GUIBox (playerInput.mousePosition, cursorManager.normalCursorSize), cursorManager.pointerTexture, ScaleMode.ScaleToFit, true, 0f);
						}*/
					}
					else if (selectedCursor == -1 || settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
					{
						DrawMainCursor ();
					}
					else if (selectedCursor == -2 && runtimeInventory.selectedItem == null)
					{
						selectedCursor = -1;
						Debug.Log ("Item reset");
					}
					/*else if (cursorManager.pointerTexture)
					{
						if (selectedCursor == -1 || settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
						{
							GUI.DrawTexture (AdvGame.GUIBox (playerInput.mousePosition, cursorManager.normalCursorSize), cursorManager.pointerTexture, ScaleMode.ScaleToFit, true, 0f);
						}
					}
					else
					{
						Debug.LogWarning ("No 'main' texture defined - please set in SettingsManager.");
					}*/
				}

				// Drag line
				if ((settingsManager.movementMethod == MovementMethod.Drag || (settingsManager.inputMethod == InputMethod.TouchScreen && settingsManager.movementMethod != MovementMethod.PointAndClick)) &&
				    stateHandler.gameState == GameState.Normal && playerInput.activeArrows == null && playerInput.dragStartPosition != Vector2.zero)
				{
					Vector2 pointA = playerInput.dragStartPosition;
				    Vector2 pointB = playerInput.invertedMouse;
				    DrawStraightLine.Draw (pointA, pointB, settingsManager.dragLineColor, settingsManager.dragLineWidth, true);
				}
			}
		}


		private void DrawMainCursor ()
		{
			if (cursorManager.walkTexture && playerInput && !playerInput.mouseOverMenu && !playerInput.interactionMenuIsOn && stateHandler && stateHandler.gameState == GameState.Normal)
			{
				GUI.DrawTexture (AdvGame.GUIBox (playerInput.mousePosition, cursorManager.normalCursorSize), cursorManager.walkTexture, ScaleMode.ScaleToFit, true, 0f);
			}
			else if (cursorManager.pointerTexture)
			{
				GUI.DrawTexture (AdvGame.GUIBox (playerInput.mousePosition, cursorManager.normalCursorSize), cursorManager.pointerTexture, ScaleMode.ScaleToFit, true, 0f);
			}
			else
			{
				Debug.LogWarning ("No 'main' texture defined - please set in SettingsManager.");
			}
		}
		
		
		private void ShowContextIcons ()
		{
			Vector2 useOffset = playerInput.mousePosition;
			Vector2 lookOffset = playerInput.mousePosition;

			if (playerInteraction.hotspot.HasContextUse () && playerInteraction.hotspot.HasContextLook () && cursorManager.lookUseCursorAction == LookUseCursorAction.DisplayBothSideBySide)
			{
				useOffset.x -= cursorManager.iconCursorSize * Screen.width / 2f;
				lookOffset.x += cursorManager.iconCursorSize * Screen.width / 2f;
			}
			
			if (playerInteraction.hotspot.HasContextUse ())
			{
				if (playerInteraction.hotspot.useButtons != null && playerInteraction.hotspot.oneClick && playerInteraction.hotspot.useButtons.Count == 1)
				{
					GUI.DrawTexture (AdvGame.GUIBox (useOffset, cursorManager.iconCursorSize), cursorManager.GetTextureFromID (playerInteraction.hotspot.useButtons[0].iconID), ScaleMode.ScaleToFit, true, 0f);
					return;
				}
				else if (playerInteraction.hotspot.useButton != null && playerInteraction.hotspot.useButton.iconID > -1)
				{
					GUI.DrawTexture (AdvGame.GUIBox (useOffset, cursorManager.iconCursorSize), cursorManager.GetTextureFromID (playerInteraction.hotspot.useButton.iconID), ScaleMode.ScaleToFit, true, 0f);
				}
			}
			
			if (playerInteraction.hotspot.HasContextLook () &&
			    (!playerInteraction.hotspot.HasContextUse () ||
			 	(playerInteraction.hotspot.HasContextUse () && cursorManager.lookUseCursorAction == LookUseCursorAction.DisplayBothSideBySide)))
			{
				if (cursorManager.cursorIcons.Count > 0)
				{
					GUI.DrawTexture (AdvGame.GUIBox (lookOffset, cursorManager.iconCursorSize), cursorManager.GetTextureFromID (cursorManager.lookCursor_ID), ScaleMode.ScaleToFit, true, 0f);
				}
			}	
		}


		private void ShowContextIcons (InvItem invItem)
		{
			Vector2 useOffset = playerInput.mousePosition;
			Vector2 lookOffset = playerInput.mousePosition;
			
			if (invItem.lookActionList != null && cursorManager.lookUseCursorAction == LookUseCursorAction.DisplayBothSideBySide)
			{
				useOffset.x -= cursorManager.iconCursorSize * Screen.width / 2f;
				lookOffset.x += cursorManager.iconCursorSize * Screen.width / 2f;
			}
			
			if (cursorManager.cursorIcons.Count > 0)
			{
				GUI.DrawTexture (AdvGame.GUIBox (useOffset, cursorManager.iconCursorSize), cursorManager.GetTextureFromID (invItem.useIconID), ScaleMode.ScaleToFit, true, 0f);

				if (invItem.lookActionList != null)
				{
					GUI.DrawTexture (AdvGame.GUIBox (lookOffset, cursorManager.iconCursorSize), cursorManager.GetTextureFromID (cursorManager.lookCursor_ID), ScaleMode.ScaleToFit, true, 0f);
				}
			}	
		}


		private void DrawActiveCursor ()
		{
			if (settingsManager.inventoryActiveEffect == InventoryActiveEffect.Simple)
			{
				GUI.DrawTexture (AdvGame.GUIBox (playerInput.mousePosition, cursorManager.inventoryCursorSize), runtimeInventory.selectedItem.activeTex, ScaleMode.ScaleToFit, true, 0f);
			}
			else if (settingsManager.inventoryActiveEffect == InventoryActiveEffect.Pulse && runtimeInventory.selectedItem.tex)
			{
				if (pulseDirection == 0)
				{
					pulse = 0f;
					pulseDirection = 1;
				}
				else if (pulse > 1f)
				{
					pulse = 1f;
					pulseDirection = -1;
				}
				else if (pulse < 0f)
				{
					pulse = 0f;
					pulseDirection = 1;
				}
				else if (pulseDirection == 1)
				{
					pulse += settingsManager.inventoryPulseSpeed * Time.deltaTime;
				}
				else if (pulseDirection == -1)
				{
					pulse -= settingsManager.inventoryPulseSpeed * Time.deltaTime;
				}
				
				Color backupColor = GUI.color;
				Color tempColor = GUI.color;
				
				tempColor.a = pulse;
				GUI.color = tempColor;
				GUI.DrawTexture (AdvGame.GUIBox (playerInput.mousePosition, cursorManager.inventoryCursorSize), runtimeInventory.selectedItem.activeTex, ScaleMode.ScaleToFit, true, 0f);
				GUI.color = backupColor;
				GUI.DrawTexture (AdvGame.GUIBox (playerInput.mousePosition, cursorManager.inventoryCursorSize), runtimeInventory.selectedItem.tex, ScaleMode.ScaleToFit, true, 0f);
			}
		}
		
		
		public void CycleCursors ()
		{
			if (cursorManager && cursorManager.cursorIcons.Count > 0)
			{
				selectedCursor ++;
				
				if (selectedCursor >= cursorManager.cursorIcons.Count)
				{
					selectedCursor = -1;
				}
			}
			else
			{
				// Pointer
				selectedCursor = -1;
			}
		}
		
		
		public int GetSelectedCursor ()
		{
			return selectedCursor;
		}


		public int GetSelectedCursorID ()
		{
			if (cursorManager && cursorManager.cursorIcons.Count > 0 && selectedCursor > -1)
			{
				return cursorManager.cursorIcons [selectedCursor].id;
			}

			return -1;
		}
		
		
		public void ResetSelectedCursor ()
		{
			selectedCursor = -1;
		}
		
		
		public void SetCursorFromID (int ID)
		{
			if (cursorManager && cursorManager.cursorIcons.Count > 0)
			{
				foreach (CursorIcon cursor in cursorManager.cursorIcons)
				{
					if (cursor.id == ID)
					{
						selectedCursor = cursorManager.cursorIcons.IndexOf (cursor);
					}
				}
			}
		}

		
		private void OnDestroy ()
		{
			stateHandler = null;
			runtimeInventory = null;
			playerInput = null;
			playerInteraction = null;
			playerMenus = null;
		}

	}

}