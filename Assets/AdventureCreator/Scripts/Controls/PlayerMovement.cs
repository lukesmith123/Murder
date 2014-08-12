/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerMovement.cs"
 * 
 *	This script analyses the variables in PlayerInput, and moves the character
 *	based on the control style, defined in the SettingsManager.
 *	To move the Player during cutscenes, a PlayerPath object must be defined.
 *	This Path will dynamically change based on where the Player must travel to.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	public class PlayerMovement : MonoBehaviour
	{
		
		private FirstPersonCamera firstPersonCamera;

		private StateHandler stateHandler;
		private Player player;
		private PlayerInput playerInput;
		private PlayerCursor playerCursor;
		private PlayerInteraction playerInteraction;
		private MainCamera mainCamera;
		private SettingsManager settingsManager;
		private SceneSettings sceneSettings;
		
		
		private void Awake ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}
			
			playerInput = this.GetComponent <PlayerInput>();
			playerInteraction = this.GetComponent <PlayerInteraction>();
			playerCursor = this.GetComponent <PlayerCursor>();
			sceneSettings = this.GetComponent <SceneSettings>();
			
			if (GameObject.FindWithTag (Tags.mainCamera) && GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>())
			{
				mainCamera = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
			}
		}
		
		
		private void Start ()
		{
			ResetPlayerReference ();
			
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
			{
				stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
			}
			
			if (GameObject.FindWithTag (Tags.firstPersonCamera) && GameObject.FindWithTag (Tags.firstPersonCamera).GetComponent <FirstPersonCamera>())
			{
				firstPersonCamera = GameObject.FindWithTag (Tags.firstPersonCamera).GetComponent <FirstPersonCamera>();
			}
		}


		private void ResetPlayerReference ()
		{
			if (GameObject.FindWithTag (Tags.player) && GameObject.FindWithTag (Tags.player).GetComponent <Player>())
			{
				player = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
			}
		}
		
		
		private void LateUpdate ()
		{
			if (stateHandler && stateHandler.movementIsOff)
			{
				return;
			}

			if (settingsManager && settingsManager.movementMethod == MovementMethod.None)
			{
				return;
			}

			if (stateHandler && settingsManager && stateHandler.gameState == GameState.Normal && player && playerInput && playerInteraction)
			{
				if (playerInput.activeArrows && settingsManager.inputMethod == InputMethod.TouchScreen)
				{
					ArrowSwipe ();
				}
				else if (playerInput.activeArrows == null)
				{
					if (playerInput.buttonPressed == 1 && !playerInput.interactionMenuIsOn && !playerInput.mouseOverMenu && playerInput.CanClick () && !playerInteraction.IsMouseOverHotspot ())
					{
						if (player.IsMovingToHotspot ())
						{
							StopMovingToHotspot ();
						}

						if (playerInteraction.hotspot)
						{
							playerInteraction.hotspot.Deselect ();
							playerInteraction.hotspot = null;
						}
					}

					if (player.IsMovingToHotspot () && settingsManager.movementMethod != MovementMethod.PointAndClick && playerInput.moveKeys != Vector2.zero)
					{
						StopMovingToHotspot ();
					}

					if (settingsManager.movementMethod == MovementMethod.Direct)
					{
						if (settingsManager.inputMethod == InputMethod.TouchScreen)
						{
							DragPlayer (true);
						}
						else
						{
							if (player.GetPath () == null || !player.lockedPath)
							{
								// Normal gameplay
								DirectControlPlayer (true);
							}
							else
							{
								// Move along pre-determined path
								DirectControlPlayerPath ();
							}
						}
					}

					else if (settingsManager.movementMethod == MovementMethod.Drag)
					{
						DragPlayer (true);
					}
					
					else if (settingsManager.movementMethod == MovementMethod.PointAndClick)
					{
						PointControlPlayer ();
					}
					
					else if (settingsManager.movementMethod == MovementMethod.FirstPerson)
					{
						if (settingsManager.inputMethod == InputMethod.TouchScreen)
						{
							FirstPersonControlPlayer ();

							if (settingsManager.dragAffects == DragAffects.Movement)
							{
								DragPlayer (false);
							}
							else
							{
								DragPlayerLook ();
							}
						}
						else
						{
							FirstPersonControlPlayer ();
							DirectControlPlayer (false);
						}
					}
				}
			}
		}
		
		
		// Drag functions

		private void ArrowSwipe ()
		{
			if (player && playerInput && settingsManager && playerInput.CanClick ())
			{
				if (playerInput.buttonPressed == 0)
				{
					playerInput.dragStartPosition = Vector2.zero;
					playerInput.dragVector = Vector2.zero;
					playerInput.dragSpeed = 0f;
				}
				
				else
				{
					if (playerInput.buttonPressed == 1)
					{
						playerInput.dragStartPosition = playerInput.invertedMouse;
					}
					else if (playerInput.buttonPressed == 3)
					{
						playerInput.dragVector = playerInput.invertedMouse - playerInput.dragStartPosition;
						playerInput.dragSpeed = playerInput.dragVector.magnitude;
						playerInput.dragVector.Normalize ();

						if (playerInput.dragSpeed > settingsManager.dragWalkThreshold * 10f)
						{
							if (playerInput.dragVector.x > 0.95f)
							{
								playerInput.activeArrows.DoRight ();
							}
							else if (playerInput.dragVector.x < -0.95f)
							{
								playerInput.activeArrows.DoLeft ();
							}
							else if (playerInput.dragVector.y > 0.95f)
							{
								playerInput.activeArrows.DoDown();
							}
							else if (playerInput.dragVector.y < -0.95f)
							{
								playerInput.activeArrows.DoUp ();
							}
						}
					}
				}
			}
		}

		
		private void DragPlayer (bool doRotation)
		{
			if (playerInteraction && playerInteraction.IsDraggingInventory ())
			{
				return;
			}

			if (player && playerInput && settingsManager && playerInput.CanClick ())
			{
				if (playerInput.buttonPressed == 0)
				{
					playerInput.dragStartPosition = Vector2.zero;
					playerInput.dragVector = Vector2.zero;
					playerInput.dragSpeed = 0f;
					
					if (player.charState == CharState.Move)
					{
						player.charState = CharState.Decelerate;
					}
				}
				
				else if (!playerInput.mouseOverMenu && !playerInput.interactionMenuIsOn && (playerInput.buttonPressed == 2 || !playerInteraction.IsMouseOverHotspot ()))
				{
					if (playerInput.buttonPressed == 1)
					{
						if (playerInteraction.hotspot)
						{
							playerInteraction.hotspot.Deselect ();
							playerInteraction.hotspot = null;
						}

						playerInput.dragStartPosition = playerInput.invertedMouse;
						
						playerInput.ResetClick ();
						playerInput.ResetDoubleClick ();
					}
					else if (playerInput.buttonPressed == 3)
					{
						playerInput.dragVector = playerInput.invertedMouse - playerInput.dragStartPosition;
						playerInput.dragSpeed = playerInput.dragVector.magnitude;
						playerInput.dragVector.Normalize ();

						Vector3 moveDirectionInput = Vector3.zero;
						
						if (settingsManager.IsTopDown ())
						{
							moveDirectionInput = (playerInput.moveKeys.y * Vector3.forward) + (playerInput.moveKeys.x * Vector3.right);
						}
						else
						{
							moveDirectionInput = (playerInput.moveKeys.y * mainCamera.ForwardVector ()) + (playerInput.moveKeys.x * mainCamera.RightVector ());
						}
						
						if (playerInput.dragSpeed > settingsManager.dragWalkThreshold * 10f)
						{
							player.isRunning = playerInput.isRunning;
							player.charState = CharState.Move;
						
							if (doRotation)
							{
								player.SetLookDirection (moveDirectionInput, false);
								player.SetMoveDirectionAsForward ();
							}
							else
							{
								if (playerInput.dragVector.y < 0f)
								{
									player.SetMoveDirectionAsForward ();
								}
								else
								{
									player.SetMoveDirectionAsBackward ();
								}
							}
						}
						else
						{
							if (player.charState == CharState.Move)
							{
								player.charState = CharState.Decelerate;
							}
						}
					}
				}
			}
		}
		
		
		// Direct-control functions
		
		private void DirectControlPlayer (bool doRotation)
		{
			if (player && playerInput)
			{
				if (settingsManager.directMovementType == DirectMovementType.RelativeToCamera)
				{
					if (playerInput.moveKeys != Vector2.zero)
					{
						Vector3 moveDirectionInput = Vector3.zero;

						if (settingsManager.IsTopDown ())
						{
							moveDirectionInput = (playerInput.moveKeys.y * Vector3.forward) + (playerInput.moveKeys.x * Vector3.right);
						}
						else
						{
							moveDirectionInput = (playerInput.moveKeys.y * mainCamera.ForwardVector ()) + (playerInput.moveKeys.x * mainCamera.RightVector ());
						}
				
						player.isRunning = playerInput.isRunning;
						player.charState = CharState.Move;
						
						if (doRotation)
						{
							player.SetLookDirection (moveDirectionInput, false);
							player.SetMoveDirectionAsForward ();
						}
						else
						{
							player.SetMoveDirection (moveDirectionInput);
						}
					}
					else if (player.charState == CharState.Move)
					{
						player.charState = CharState.Decelerate;
					}
				}
				
				else if (settingsManager.directMovementType == DirectMovementType.TankControls)
				{
					if (playerInput.moveKeys.x < -0.5f)
					{
						player.TankTurnLeft ();
					}
					else if (playerInput.moveKeys.x > 0.5f)
					{
						player.TankTurnRight ();
					}
					else
					{
						player.StopTurning ();
					}
					
					if (playerInput.moveKeys.y > 0f)
					{
						player.isRunning = playerInput.isRunning;
						player.charState = CharState.Move;
						player.SetMoveDirectionAsForward ();
					}
					else if (playerInput.moveKeys.y < 0f)
					{
						player.isRunning = playerInput.isRunning;
						player.charState = CharState.Move;
						player.SetMoveDirectionAsBackward ();
					}
					else if (player.charState == CharState.Move)
					{
						player.charState = CharState.Decelerate;
						player.SetMoveDirectionAsForward ();
					}
				}
			}
		}
		
		
		private void DirectControlPlayerPath ()
		{
			if (player && playerInput)
			{
				if (playerInput.moveKeys != Vector2.zero)
				{
					Vector3 moveDirectionInput = Vector3.zero;

					if (settingsManager.IsTopDown ())
					{
						moveDirectionInput = (playerInput.moveKeys.y * Vector3.forward) + (playerInput.moveKeys.x * Vector3.right);
					}
					else
					{
						moveDirectionInput = (playerInput.moveKeys.y * mainCamera.ForwardVector ()) + (playerInput.moveKeys.x * mainCamera.RightVector ());
					}

					if (Vector3.Dot (moveDirectionInput, player.GetMoveDirection ()) > 0f)
					{
						// Move along path, because movement keys are in the path's forward direction
						player.isRunning = playerInput.isRunning;
						player.charState = CharState.Move;
					}
				}
				else
				{
					if (player.charState == CharState.Move)
					{
						player.charState = CharState.Decelerate;
					}
				}
			}
		}
		
		
		// Point/click functions
		
		private void PointControlPlayer ()
		{
			if (player && playerInput && playerInteraction && playerCursor && settingsManager)
			{
				if (playerInput.isDownLocked && playerInput.isUpLocked && playerInput.isLeftLocked && playerInput.isRightLocked)
				{
					if (player.GetPath () == null && player.charState == CharState.Move)
					{
						player.charState = CharState.Decelerate;
					}
					return;
				}

				if (playerInput.buttonPressed == 1 && playerInput.CanClick () && !playerInput.interactionMenuIsOn && !playerInput.mouseOverMenu && !playerInteraction.IsMouseOverHotspot () && playerCursor.GetSelectedCursor () < 0)
				{
					if (settingsManager.doubleClickMovement && !playerInput.CanDoubleClick ())
					{
						playerInput.ResetDoubleClick ();
						return;
					}

					bool doubleClick = false;
					
					if (playerInput.CanDoubleClick () && !settingsManager.doubleClickMovement)
					{
						doubleClick = true;
					}
					playerInput.ResetClick ();
					playerInput.ResetDoubleClick ();

					if (settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction && GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>())
					{
						GameObject.FindWithTag (Tags.persistentEngine).GetComponent <PlayerMenus>().SetInteractionMenus (false);
					}
					if (!RaycastNavMesh (playerInput.mousePosition, doubleClick))
					{
						// Move Ray down screen until we hit something
						Vector3 simulatedMouse = playerInput.mousePosition;
		
						if (((int) Screen.height * settingsManager.walkableClickRange) > 1)
						{
							for (int i=1; i<Screen.height * settingsManager.walkableClickRange; i+=4)
							{
								if (RaycastNavMesh (new Vector2 (simulatedMouse.x, simulatedMouse.y - i), doubleClick))
								{
									break;
								}
								if (RaycastNavMesh (new Vector2 (simulatedMouse.x, simulatedMouse.y + i), doubleClick))
								{
									break;
								}
							}
						}
					}
				}
				else if (player.GetPath () == null && player.charState == CharState.Move)
				{
					player.charState = CharState.Decelerate;
				}
			}
		}


		private bool ProcessHit (Vector3 hitPoint, GameObject hitObject, bool run)
		{
			if (hitObject.layer != LayerMask.NameToLayer (settingsManager.navMeshLayer))
			{
				return false;
			}
			
			if (!run)
			{
				ShowClick (hitPoint);
			}
			
			if (playerInput.runLock == PlayerMoveLock.AlwaysRun)
			{
				run = true;
			}
			else if (playerInput.runLock == PlayerMoveLock.AlwaysWalk)
			{
				run = false;
			}

			if (GetComponent <NavigationManager>())
			{
				Vector3[] pointArray;
				pointArray = GetComponent <NavigationManager>().navigationEngine.GetPointsArray (player.transform.position, hitPoint);
				player.MoveAlongPoints (pointArray, run);
			}
			
			return true;
		}


		private bool RaycastNavMesh (Vector3 mousePosition, bool run)
		{
			if (GetComponent <SceneSettings>().navigationMethod == AC_NavigationMethod.PolygonCollider)
			{
				RaycastHit2D hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (new Vector2 (mousePosition.x, mousePosition.y)), Vector2.zero, settingsManager.navMeshRaycastLength);

				if (hit.collider != null)
				{
					return ProcessHit (hit.point, hit.collider.gameObject, run);
				}
			}
			else
			{
				Ray ray = Camera.main.ScreenPointToRay (mousePosition);
				RaycastHit hit = new RaycastHit();
				
				if (settingsManager && sceneSettings && Physics.Raycast (ray, out hit, settingsManager.navMeshRaycastLength))
				{
					return ProcessHit (hit.point, hit.collider.gameObject, run);
				}
			}
			
			return false;
		}


		private void ShowClick (Vector3 clickPoint)
		{
			if (settingsManager && settingsManager.clickPrefab)
			{
				Destroy (GameObject.Find (settingsManager.clickPrefab.name + "(Clone)"));
				Instantiate (settingsManager.clickPrefab, clickPoint, Quaternion.identity);
			}
		}

		
		// First-person functions
		
		private void FirstPersonControlPlayer ()
		{
			if (firstPersonCamera)
			{
				if (player)
				{
					float rotationX = player.transform.localEulerAngles.y + playerInput.freeAim.x * firstPersonCamera.sensitivity.x;
					firstPersonCamera.rotationY += playerInput.freeAim.y * firstPersonCamera.sensitivity.y;
					player.transform.localEulerAngles = new Vector3 (0, rotationX, 0);
				}
			}
			else
			{
				Debug.LogWarning ("Could not find first person camera");
			}
		}


		private void DragPlayerLook ()
		{
			if (player && playerInput && settingsManager && playerInput.CanClick ())
			{
				if (playerInput.isDownLocked && playerInput.isUpLocked && playerInput.isLeftLocked && playerInput.isRightLocked)
				{
					playerInput.dragStartPosition = Vector2.zero;
					playerInput.dragVector = Vector2.zero;
					playerInput.dragSpeed = 0f;
					return;
				}

				if (playerInput.buttonPressed == 0)
				{
					playerInput.dragStartPosition = Vector2.zero;
					playerInput.dragVector = Vector2.zero;
					playerInput.dragSpeed = 0f;
					return;
				}
				
				else if (!playerInput.mouseOverMenu && !playerInput.interactionMenuIsOn && (playerInput.buttonPressed == 2 || !playerInteraction.IsMouseOverHotspot ()))
				{
					if (playerInput.buttonPressed == 1)
					{
						if (playerInteraction.hotspot)
						{
							playerInteraction.hotspot.Deselect ();
							playerInteraction.hotspot = null;
						}
						
						playerInput.dragStartPosition = playerInput.invertedMouse;
						
						playerInput.ResetClick ();
						playerInput.ResetDoubleClick ();
					}
					else if (playerInput.buttonPressed == 3)
					{
						playerInput.dragVector = playerInput.invertedMouse - playerInput.dragStartPosition;
						playerInput.dragSpeed = playerInput.dragVector.magnitude;
						playerInput.dragVector.Normalize ();
						playerInput.dragVector.y *= -1;
					}
				}
			}
		}


		private void StopMovingToHotspot ()
		{
			player.hotspotMovingTo = null;
			player.EndPath ();
			playerInteraction.StopCoroutine ("UseObject");
		}
		
		
		private void OnDestroy ()
		{
			firstPersonCamera = null;
			stateHandler = null;
			player = null;
			playerInput = null;
			mainCamera = null;
			settingsManager = null;
			sceneSettings = null;
		}
		
	}

}