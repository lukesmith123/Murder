/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"PlayerInput.cs"
 * 
 *	This script records all input and processes it for other scripts.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{
	
	public class PlayerInput : MonoBehaviour
	{
		[HideInInspector] public int buttonPressed;
		[HideInInspector] public bool mouseOverMenu = false;
		[HideInInspector] public bool interactionMenuIsOn = false;
		
		[HideInInspector] public Vector2 moveKeys = new Vector2 (0f, 0f);
		[HideInInspector] public bool isRunning = false;
		[HideInInspector] public float timeScale = 1f;
		
		[HideInInspector] public bool isUpLocked = false;
		[HideInInspector] public bool isDownLocked = false;
		[HideInInspector] public bool isLeftLocked = false;
		[HideInInspector] public bool isRightLocked = false;
		[HideInInspector] public PlayerMoveLock runLock = PlayerMoveLock.Free;
		
		[HideInInspector] public int selected_option;
		[HideInInspector] public Vector2 invertedMouse;
		
		public float clickDelay = 0.3f;
		private float clickTime = 0f;
		public float doubleClickDelay = 1f;
		private float doubleClickTime = 0;
		[HideInInspector] public bool hasUnclickedSinceClick = false;
		
		// Menu input override
		[HideInInspector] public string menuButtonInput;
		[HideInInspector] public float menuButtonValue;
		[HideInInspector] public SimulateInputType menuInput;
		
		// Controller movement
		private Vector2 xboxCursor;
		private float cursorMoveSpeed = 4f;
		[HideInInspector] public Vector2 mousePosition;
		private bool scrollingLocked = false;
		
		// Touch-Screen movement
		[HideInInspector] public Vector2 dragStartPosition = Vector2.zero;
		[HideInInspector] public float dragSpeed = 0f;
		[HideInInspector] public Vector2 dragVector;
		
		// 1st person movement
		[HideInInspector] public Vector2 freeAim;
		public bool canMoveMouse = true;
		
		[HideInInspector] public Conversation activeConversation = null;
		[HideInInspector] public ArrowPrompt activeArrows = null;
		[HideInInspector] public Container activeContainer = null;
		
		private StateHandler stateHandler;
		private RuntimeInventory runtimeInventory;
		private SettingsManager settingsManager;
		private MainCamera mainCamera;
		
		
		private void Awake ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
				
				if (settingsManager.movementMethod == MovementMethod.FirstPerson)
				{
					canMoveMouse = false;
				}
			}
			
			if (GameObject.FindWithTag (Tags.mainCamera) && GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>())
			{
				mainCamera = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
			}
			
			ResetClick ();
			
			xboxCursor.x = Screen.width / 2;
			xboxCursor.y = Screen.height / 2;
		}
		
		
		private void Start ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine))
			{
				if (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
				{
					stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
				}
				
				if (GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>())
				{
					runtimeInventory = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeInventory>();
				}
			}
		}
		
		
		private void Update ()
		{
			if (stateHandler && stateHandler.inputIsOff)
			{
				return;
			}
			
			if (clickTime > 0f)
			{
				clickTime -= 0.1f;
			}
			if (clickTime < 0f)
			{
				clickTime = 0f;
			}
			
			if (doubleClickTime > 0f)
			{
				doubleClickTime -= 0.1f;
			}
			if (doubleClickTime < 0f)
			{
				doubleClickTime = 0f;
			}
			
			buttonPressed = 0;
			
			if (stateHandler && settingsManager && Time.time > 0f)
			{
				try
				{
					if (InputGetButtonDown ("ToggleCursor") && stateHandler.gameState == GameState.Normal)
					{
						if (canMoveMouse)
						{
							canMoveMouse = false;
						}
						else
						{
							canMoveMouse = true;
						}
					}
				}
				catch
				{
					canMoveMouse = true;
				}
				
				if (InputGetButtonDown ("EndCutscene") && stateHandler.gameState == GameState.Cutscene)
				{
					this.GetComponent <ActionListManager>().EndCutscene ();
				}
				
				// Handle cursor position
				if (settingsManager.movementMethod == MovementMethod.FirstPerson)
				{
					if (settingsManager.inputMethod == InputMethod.KeyboardOrController)
					{
						if (!canMoveMouse && stateHandler.gameState == GameState.Normal)
						{
							mousePosition = new Vector2 (Screen.width / 2, Screen.height / 2);
							freeAim = new Vector2 (InputGetAxis ("CursorHorizontal") * 50f, InputGetAxis ("CursorVertical") * 50f);
						}
						else
						{
							xboxCursor.x += InputGetAxis ("CursorHorizontal") * cursorMoveSpeed * Screen.width;
							xboxCursor.y += InputGetAxis ("CursorVertical") * cursorMoveSpeed * Screen.width;
							
							xboxCursor.x = Mathf.Clamp (xboxCursor.x, 0f, Screen.width);
							xboxCursor.y = Mathf.Clamp (xboxCursor.y, 0f, Screen.height);
							
							mousePosition = xboxCursor;
							
							freeAim = Vector2.zero;
						}
					}
					
					else if (settingsManager.inputMethod == InputMethod.MouseAndKeyboard)
					{
						if (stateHandler.gameState == GameState.Normal && !canMoveMouse)
						{
							mousePosition = new Vector2 (Screen.width / 2, Screen.height / 2);
							freeAim = new Vector2 (InputGetAxis ("CursorHorizontal"), InputGetAxis ("CursorVertical"));
						}
						else
						{
							mousePosition = Input.mousePosition;
							freeAim = Vector2.zero;
						}
					}
					
					else if (settingsManager.inputMethod == InputMethod.TouchScreen)
					{
						#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
						if (Input.touchCount > 0)
						{
							mousePosition = Input.GetTouch (0).position;
						}
						#else
						mousePosition = Input.mousePosition;
						#endif
						
						if (dragStartPosition != Vector2.zero)
						{
							if (settingsManager.dragAffects == DragAffects.Movement)
							{
								freeAim = new Vector2 (dragVector.x * settingsManager.freeAimTouchSpeed, 0f);
							}
							else
							{
								freeAim = new Vector2 (dragVector.x * settingsManager.freeAimTouchSpeed, dragVector.y * settingsManager.freeAimTouchSpeed);
							}
						}
						else
						{
							freeAim = Vector2.zero;
						}
					}
				}
				else
				{
					if (!canMoveMouse && stateHandler.gameState == GameState.Normal)
					{
						mousePosition = new Vector2 (Screen.width / 2, Screen.height / 2);
						freeAim = new Vector2 (InputGetAxis ("CursorHorizontal") * 50f, InputGetAxis ("CursorVertical") * 50f);
					}
					
					else if (settingsManager.inputMethod == InputMethod.MouseAndKeyboard)
					{
						mousePosition = Input.mousePosition;
					}
					else if (settingsManager.inputMethod == InputMethod.TouchScreen)
					{
						#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
						if (Input.touchCount > 0)
						{
							mousePosition = Input.GetTouch (0).position;
						}
						#else
						mousePosition = Input.mousePosition;
						#endif
					}
					
					else if (settingsManager.inputMethod == InputMethod.KeyboardOrController && stateHandler.gameState == GameState.Normal)
					{
						xboxCursor.x += InputGetAxis ("CursorHorizontal") * cursorMoveSpeed * Screen.width;
						xboxCursor.y += InputGetAxis ("CursorVertical") * cursorMoveSpeed * Screen.width;
						
						xboxCursor.x = Mathf.Clamp (xboxCursor.x, 0f, Screen.width);
						xboxCursor.y = Mathf.Clamp (xboxCursor.y, 0f, Screen.height);
						
						mousePosition = xboxCursor;
					}
				}
				
				if (mainCamera)
				{
					mousePosition = mainCamera.LimitMouseToAspect (mousePosition);
				}
				
				invertedMouse = new Vector2 (mousePosition.x, Screen.height - mousePosition.y);
				
				if (buttonPressed == 0 && !hasUnclickedSinceClick)
				{
					hasUnclickedSinceClick = true;
				}
				
				// Handle mouse position
				if (settingsManager.inputMethod == InputMethod.MouseAndKeyboard)
				{
					if (Input.GetMouseButtonDown (0))
					{
						buttonPressed = 1;
					}
					else if (Input.GetMouseButtonDown (1))
					{
						buttonPressed = 2;
					}
					else if (Input.GetMouseButton (0) && settingsManager.movementMethod == MovementMethod.Drag)
					{
						buttonPressed = 3;
					}
					else if (Input.GetMouseButton (0))
					{
						buttonPressed = 4;
					}
				}
				else if (settingsManager.inputMethod == InputMethod.TouchScreen)
				{
					#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
					
					if (Input.touchCount == 1 && Input.GetTouch (0).phase == TouchPhase.Began)
					{
						buttonPressed = 1;
					}
					else if (Input.touchCount > 2)
					{
						buttonPressed = 2;
						clickTime = 0f;
					}
					else if (Input.touchCount == 1 && (Input.GetTouch (0).phase == TouchPhase.Stationary || Input.GetTouch (0).phase == TouchPhase.Moved))
					{
						buttonPressed = 3;
					}
					
					#else
					
					if (Input.GetMouseButtonDown (0))
					{
						buttonPressed = 1;
					}
					else if (Input.GetMouseButton (0) && Input.GetMouseButtonDown (1) || (Input.GetMouseButton (0) && Input.touchCount > 1))
					{
						buttonPressed = 2;
						clickTime = 0f;
					}
					else if (Input.GetMouseButton (0))
					{
						buttonPressed = 3;
					}
					
					#endif
				}
				else if (settingsManager.inputMethod == InputMethod.KeyboardOrController)
				{
					if (InputGetButtonDown ("InteractionA"))
					{
						buttonPressed = 1;
					}
					else if (InputGetButtonDown ("InteractionB"))
					{
						buttonPressed = 2;
					}
					else if (Input.GetButton ("InteractionA") && settingsManager.movementMethod == MovementMethod.Drag)
					{
						buttonPressed = 3;
					}
					
					// Menu option changing
					if (stateHandler.gameState == GameState.DialogOptions || stateHandler.gameState == GameState.Paused)
					{
						if (!scrollingLocked)
						{
							if (InputGetAxisRaw ("Vertical") > 0.1 || InputGetAxisRaw ("Horizontal") < -0.1)
							{
								// Up / Left
								scrollingLocked = true;
								selected_option --;
							}
							else if (InputGetAxisRaw ("Vertical") < -0.1 || InputGetAxisRaw ("Horizontal") > 0.1)
							{
								// Down / Right
								scrollingLocked = true;
								selected_option ++;
							}
						}
						else if (InputGetAxisRaw ("Vertical") < 0.05 && InputGetAxisRaw ("Vertical") > -0.05 && InputGetAxisRaw ("Horizontal") < 0.05 && InputGetAxisRaw ("Horizontal") > -0.05)
						{
							scrollingLocked = false;
						}
					}
				}	
			}	
		}
		
		
		private void FixedUpdate ()
		{
			if (stateHandler && stateHandler.inputIsOff)
			{
				return;
			}
			
			if (stateHandler && stateHandler.gameState == GameState.Normal)
			{
				if (buttonPressed == 2 && runtimeInventory.selectedItem != null && settingsManager && settingsManager.interactionMethod != AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					runtimeInventory.SetNull ();
					GetComponent <PlayerCursor>().ResetSelectedCursor ();
					ResetClick ();
				}
				
				if (settingsManager && settingsManager.inputMethod != InputMethod.TouchScreen && activeArrows && (activeArrows.arrowPromptType == ArrowPromptType.KeyOnly || activeArrows.arrowPromptType == ArrowPromptType.KeyAndClick))
				{
					// Arrow Prompt is displayed: respond to movement keys
					if (Input.GetAxis("Vertical") > 0.1)
					{
						activeArrows.DoUp ();
					}
					
					else if (InputGetAxis ("Vertical") < -0.1)
					{
						activeArrows.DoDown ();
					}
					
					else if (InputGetAxis ("Horizontal") < -0.1)
					{
						activeArrows.DoLeft ();
					}
					
					else if (InputGetAxis ("Horizontal") > 0.1)
					{
						activeArrows.DoRight ();
					}
				}
				
				if (activeArrows && (activeArrows.arrowPromptType == ArrowPromptType.ClickOnly || activeArrows.arrowPromptType == ArrowPromptType.KeyAndClick))
				{
					// Arrow Prompt is displayed: respond to mouse clicks
					if (buttonPressed == 1)
					{
						if (activeArrows.upArrow.rect.Contains (invertedMouse))
						{
							activeArrows.DoUp ();
						}
						
						else if (activeArrows.downArrow.rect.Contains (invertedMouse))
						{
							activeArrows.DoDown ();
						}
						
						else if (activeArrows.leftArrow.rect.Contains (invertedMouse))
						{
							activeArrows.DoLeft ();
						}
						
						else if (activeArrows.rightArrow.rect.Contains (invertedMouse))
						{
							activeArrows.DoRight ();
						}
					}
				}
				
				
				if (activeArrows == null && settingsManager.movementMethod != MovementMethod.PointAndClick)
				{
					float h = 0f;
					float v = 0f;
					bool run;
					
					if (settingsManager.inputMethod == InputMethod.TouchScreen || settingsManager.movementMethod == MovementMethod.Drag)
					{
						h = dragVector.x;
						v = -dragVector.y;
					}
					else
					{
						h = InputGetAxis ("Horizontal");
						v = InputGetAxis ("Vertical");
					}
					
					if ((isUpLocked && v > 0f) || (isDownLocked && v < 0f))
					{
						v = 0f;
					}
					
					if ((isLeftLocked && h > 0f) || (isRightLocked && h < 0f))
					{
						h = 0f;
					}
					
					if (runLock == PlayerMoveLock.Free)
					{
						if (settingsManager.inputMethod == InputMethod.TouchScreen || settingsManager.movementMethod == MovementMethod.Drag)
						{
							if (dragStartPosition != Vector2.zero && dragSpeed > settingsManager.dragRunThreshold * 10f)
							{
								run = true;
							}
							else
							{
								run = false;
							}
						}
						else
						{
							try
							{
								run = Input.GetButton ("Run");
							}
							catch
							{
								run = false;
								Debug.LogWarning ("No 'Run' button exists - please define one in the Input Manager.");
							}
						}
					}
					else if (runLock == PlayerMoveLock.AlwaysWalk)
					{
						run = false;
					}
					else
					{
						run = true;
					}
					
					isRunning = run;
					moveKeys = new Vector2 (h, v);
				}
				
				if (InputGetButtonDown ("FlashHotspots"))
				{
					FlashHotspots ();
				}
			}
		}
		
		
		private void FlashHotspots ()
		{
			if (this.GetComponent <PlayerInteraction>())
			{
				PlayerInteraction playerInteraction = this.GetComponent <PlayerInteraction>();
				Hotspot[] hotspots = FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
				
				foreach (Hotspot hotspot in hotspots)
				{
					if (hotspot.IsOn () && hotspot.highlight && hotspot != playerInteraction.hotspot)
					{
						hotspot.highlight.Flash ();
					}
				}
			}
		}
		
		
		public void RemoveActiveArrows ()
		{
			if (activeArrows)
			{
				activeArrows.TurnOff ();
			}
		}
		
		
		public void ResetClick ()
		{
			clickTime = clickDelay;
			hasUnclickedSinceClick = false;
		}
		
		
		public void ResetDoubleClick ()
		{
			doubleClickTime = doubleClickDelay;
		}
		
		
		public bool CanClick ()
		{
			if (clickTime == 0f)
			{
				return true;
			}
			
			return false;
		}
		
		
		public bool CanDoubleClick ()
		{
			if (doubleClickTime > 0f)
			{
				return true;
			}
			
			return false;
		}
		
		
		public void SimulateInput (SimulateInputType input, string axis, float value)
		{
			if (axis != "")
			{
				menuInput = input;
				menuButtonInput = axis;
				
				if (input == SimulateInputType.Button)
				{
					menuButtonValue = 1f;
				}
				else
				{
					menuButtonValue = value;
				}
				
				CancelInvoke ();
				Invoke ("StopSimulatingInput", 0.1f);
			}
		}
		
		
		public bool CanMoveMouse ()
		{
			return canMoveMouse;
		}
		
		
		private void StopSimulatingInput ()
		{
			menuButtonInput = "";
		}
		
		
		private float InputGetAxisRaw (string axis)
		{
			try
			{
				if (Input.GetAxisRaw (axis) != 0f)
				{
					return Input.GetAxisRaw (axis);
				}
			}
			catch {}
			
			if (menuButtonInput != "" && menuButtonInput == axis && menuInput == SimulateInputType.Axis)
			{
				return menuButtonValue;
			}
			
			return 0f;
		}
		
		
		private float InputGetAxis (string axis)
		{
			try
			{
				if (Input.GetAxis (axis) != 0f)
				{
					return Input.GetAxisRaw (axis);
				}
			}
			catch {}
			
			if (menuButtonInput != "" && menuButtonInput == axis && menuInput == SimulateInputType.Axis)
			{
				return menuButtonValue;
			}
			
			return 0f;
		}
		
		
		public bool InputGetButtonDown (string axis)
		{
			try
			{
				if (Input.GetButtonDown (axis))
				{
					return true;
				}
			}
			catch {}
			
			if (menuButtonInput != "" && menuButtonInput == axis && menuInput == SimulateInputType.Button)
			{
				if (menuButtonValue > 0f)
				{
					ResetClick ();
					StopSimulatingInput ();	
					return true;
				}
				
				StopSimulatingInput ();
			}
			
			return false;
		}
		
		
		private void OnDestroy ()
		{
			stateHandler = null;
			runtimeInventory = null;
			settingsManager = null;
		}
		
	}
	
}