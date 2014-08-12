/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"MainCamera.cs"
 * 
 *	This is attached to the Main Camera, and must be tagged as "MainCamera" to work.
 *	Only one Main Camera should ever exist in the scene.
 *
 *	Shake code adapted from Mike Jasper's code: http://www.mikedoesweb.com/2012/camera-shake-in-unity/
 *
 *  Aspect-rattio code adapated from Eric Haines' code: http://wiki.unity3d.com/index.php?title=AspectRatioEnforcer
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class MainCamera : MonoBehaviour
{
	
	public Texture2D fadeTexture;
	public _Camera attachedCamera;

	[HideInInspector] public _Camera lastNavCamera;
	[HideInInspector] public bool isSmoothChanging;

	private bool isCrossfading;
	private Texture2D crossfadeTexture;
	
	private bool cursorAffectsRotation;
	
	[HideInInspector] public Vector2 perspectiveOffset = new Vector2 (0f, 0f);
	private Vector2 startPerspectiveOffset = new Vector2 (0f, 0f);

	private float timeToFade = 0f;
	private int drawDepth = -1000;
	private float alpha = 0f; 
	private FadeType fadeType;
	private float fadeStartTime;
	
	private MoveMethod moveMethod;
	private float changeTime;
	
	private	Vector3 startPosition;
	private	Quaternion startRotation;
	private float startFOV;
	private float startOrtho;
	private	float startTime;
	
	private Transform LookAtPos;
	private Vector2 lookAtAmount;
	private float LookAtZ;
	private Vector3 lookAtTarget;
	
	private SettingsManager settingsManager;
	private StateHandler stateHandler;
	private PlayerInput playerInput;
	
	private float shakeDecay;
	private bool shakeMove;
	private float shakeIntensity;
	private Vector3 shakePosition;
	private Vector3 shakeRotation;

	// Aspect ratio
	private Camera borderCam;
	public static MainCamera mainCam;
	public float borderWidth;
	public MenuOrientation borderOrientation;

	// Split-screen
	public bool isSplitScreen;
	public bool isTopLeftSplit;
	public MenuOrientation splitOrientation;
	public _Camera splitCamera;

	
	private void Awake()
	{
		if (this.transform.parent && this.transform.parent.name != "_Cameras")
		{
			if (GameObject.Find ("_Cameras"))
			{
				this.transform.parent = GameObject.Find ("_Cameras").transform;
			}
			else
			{
				this.transform.parent = null;
			}
		}
		
		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>())
		{
			playerInput = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInput>();
		}
		
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;
		}

		if (!camera)
		{
			Debug.LogError ("The MainCamera script requires a Camera component.");
			return;
		}
		
		if (settingsManager.forceAspectRatio)
		{
			#if !UNITY_IPHONE
			settingsManager.landscapeModeOnly = false;
			#endif
			if (SetAspectRatio ())
			{
				CreateBorderCamera ();
			}
			SetCameraRect ();
		}
		
	}	

	
	private void Start()
	{
		if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
		{
			stateHandler = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>();
		}
	
		foreach (Transform child in transform)
		{
			LookAtPos = child;
		}

		if (LookAtPos)
		{
			LookAtZ = LookAtPos.localPosition.z;
		}
	}
	
	
	public void Shake (float _shakeDecay, bool _shakeMove)
	{
		shakePosition = Vector3.zero;
		shakeRotation = Vector3.zero;
		
		shakeMove = _shakeMove;
		shakeDecay = _shakeDecay;
		shakeIntensity = shakeDecay * 150f;
	}
	
	
	public bool IsShaking ()
	{
		if (shakeIntensity > 0f)
		{
			return true;
		}
		
		return false;
	}
	
	
	public void StopShaking ()
	{
		shakeIntensity = 0f;
		shakePosition = Vector3.zero;
		shakeRotation = Vector3.zero;
	}
	
	
	private void FixedUpdate ()
	{
		if (stateHandler && stateHandler.cameraIsOff)
		{
			return;
		}

		if (shakeIntensity > 0f)
		{
			if (shakeMove)
			{
				shakePosition = Random.insideUnitSphere * shakeIntensity * 0.5f;
			}
			
			shakeRotation = new Vector3
			(
				Random.Range (-shakeIntensity, shakeIntensity) * 0.2f,
				Random.Range (-shakeIntensity, shakeIntensity) * 0.2f,
				Random.Range (-shakeIntensity, shakeIntensity) * 0.2f
			);
			
			shakeIntensity -= shakeDecay;
		}
		
		else if (shakeIntensity < 0f)
		{
			StopShaking ();
		}
	}
	
	
	private void Update ()
	{
		if (stateHandler)
		{
			if (stateHandler.cameraIsOff)
			{
				return;
			}

			if (stateHandler.gameState == GameState.Normal)
			{
				SetFirstPerson ();
			}
			
			if (this.GetComponent <AudioListener>())
			{
				if (stateHandler.gameState == GameState.Paused)
				{
					AudioListener.pause = true;
				}
				else
				{
					AudioListener.pause = false;
				}
			}
		}
	}
	
	
	public void PrepareForBackground ()
	{
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;

			camera.clearFlags = CameraClearFlags.Depth;
			
			if (LayerMask.NameToLayer (settingsManager.backgroundImageLayer) != -1)
			{
				camera.cullingMask = ~(1 << LayerMask.NameToLayer (settingsManager.backgroundImageLayer));
			}
		}
		else
		{
			Debug.LogError ("Could not find a Settings Manager - please set one using the main Adventure Creator window.");
		}
	}
	
	
	private void RemoveBackground ()
	{
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
		{
			settingsManager = AdvGame.GetReferences ().settingsManager;
			
			camera.clearFlags = CameraClearFlags.Skybox;
			
			if (LayerMask.NameToLayer (settingsManager.backgroundImageLayer) != -1)
			{
				camera.cullingMask = ~(1 << LayerMask.NameToLayer (settingsManager.backgroundImageLayer));
			}
		}
		else
		{
			Debug.LogError ("Could not find a Settings Manager - please set one using the main Adventure Creator window.");
		}
	}

	
	public void SetFirstPerson ()
	{
		if (settingsManager)
		{
			if (settingsManager.movementMethod == MovementMethod.FirstPerson)
			{
				SetGameCamera (GameObject.FindWithTag (Tags.firstPersonCamera).GetComponent <_Camera>());
				SnapToAttached ();
			}
		}

		if (attachedCamera)
		{
			lastNavCamera = attachedCamera;
		}
	}
	
	
	private void OnGUI()
	{
		if (stateHandler && stateHandler.cameraIsOff)
		{
			return;
		}

		if (timeToFade > 0f)
		{
			alpha = (Time.time - fadeStartTime) / timeToFade;

			if (fadeType == FadeType.fadeIn)
			{
				alpha = 1 - alpha;
			}

			alpha = Mathf.Clamp01 (alpha);
		
			if (Time.time > (fadeStartTime + timeToFade))
			{
				timeToFade = 0f;
				isCrossfading = false;
			}
		}

		if (alpha > 0f)
		{
			Color tempColor = GUI.color;
			tempColor.a = alpha;
			GUI.color = tempColor;
			GUI.depth = drawDepth;

			if (isCrossfading)
			{
				GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), crossfadeTexture);
			}
			else
			{
				GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), fadeTexture);
			}
		}
	}
	
	
	public void ResetProjection ()
	{
		if (camera)
		{
			perspectiveOffset = Vector2.zero;
			camera.projectionMatrix = AdvGame.SetVanishingPoint (camera, perspectiveOffset);
			camera.ResetProjectionMatrix ();
		}
	}


	public void ResetMoving ()
	{
		isSmoothChanging = false;
		startTime = 0f;
		changeTime = 0f;
	}

	
	private void LateUpdate ()
	{
		if (stateHandler && stateHandler.cameraIsOff)
		{
			return;
		}

		if (attachedCamera && (!(attachedCamera is GameCamera25D)))
		{
			if (!isSmoothChanging)
			{
				transform.rotation = attachedCamera.transform.rotation;
				transform.position = attachedCamera.transform.position;

				if (attachedCamera is GameCamera2D)
				{
					GameCamera2D cam2D = (GameCamera2D) attachedCamera;
					perspectiveOffset = cam2D.perspectiveOffset;
					if (!camera.orthographic)
					{
						camera.projectionMatrix = AdvGame.SetVanishingPoint (camera, perspectiveOffset);
					}
					else
					{
						GetComponent <Camera>().orthographicSize = attachedCamera.GetComponent <Camera>().orthographicSize;
					}
				}
				
				else
				{
					GetComponent <Camera>().fieldOfView = attachedCamera.GetComponent <Camera>().fieldOfView;
					if (cursorAffectsRotation)
					{
						SetLookAtPosition ();
						transform.LookAt (LookAtPos);
					}
				}
			}
			else
			{
				// Move from one GameCamera to another
				if (Time.time < startTime + changeTime)
				{
					if (attachedCamera is GameCamera2D)
					{
						GameCamera2D cam2D = (GameCamera2D) attachedCamera;
						
						perspectiveOffset.x = Mathf.Lerp (startPerspectiveOffset.x, cam2D.perspectiveOffset.x, AdvGame.Interpolate (startTime, changeTime, moveMethod));
						perspectiveOffset.y = Mathf.Lerp (startPerspectiveOffset.y, cam2D.perspectiveOffset.y, AdvGame.Interpolate (startTime, changeTime, moveMethod));

						camera.ResetProjectionMatrix ();
					}
					
					if (moveMethod == MoveMethod.Curved)
					{
						// Don't slerp y position as this will create a "bump" effect
						Vector3 newPosition = Vector3.Slerp (startPosition, attachedCamera.transform.position, AdvGame.Interpolate (startTime, changeTime, moveMethod));
						newPosition.y = Mathf.Lerp (startPosition.y, attachedCamera.transform.position.y, AdvGame.Interpolate (startTime, changeTime, moveMethod));
						transform.position = newPosition;
						
						transform.rotation = Quaternion.Slerp (startRotation, attachedCamera.transform.rotation, AdvGame.Interpolate (startTime, changeTime, moveMethod));
					}
					else
					{
						transform.position = Vector3.Lerp (startPosition, attachedCamera.transform.position, AdvGame.Interpolate (startTime, changeTime, moveMethod)); 
						transform.rotation = Quaternion.Lerp (startRotation, attachedCamera.transform.rotation, AdvGame.Interpolate (startTime, changeTime, moveMethod));
					}

					GetComponent <Camera>().fieldOfView = Mathf.Lerp (startFOV, attachedCamera.GetComponent <Camera>().fieldOfView, AdvGame.Interpolate (startTime, changeTime, moveMethod));
					GetComponent <Camera>().orthographicSize = Mathf.Lerp (startOrtho, attachedCamera.GetComponent <Camera>().orthographicSize, AdvGame.Interpolate (startTime, changeTime, moveMethod));

					if (attachedCamera is GameCamera2D && !camera.orthographic)
					{
						camera.projectionMatrix = AdvGame.SetVanishingPoint (camera, perspectiveOffset);
					}
				}
				else
				{
					LookAtCentre ();
					isSmoothChanging = false;
				}
			}
			
			if (cursorAffectsRotation)
			{
				LookAtPos.localPosition = Vector3.Lerp (LookAtPos.localPosition, lookAtTarget, Time.deltaTime * 3f);	
			}
		}
		
		else if (attachedCamera && (attachedCamera is GameCamera25D))
		{
			transform.position = attachedCamera.transform.position;
			transform.rotation = attachedCamera.transform.rotation;
		}
		
		transform.position += shakePosition;
		transform.localEulerAngles += shakeRotation;
		
	}

	
	private void LookAtCentre ()
	{
		if (LookAtPos)
		{
			lookAtTarget = new Vector3 (0, 0, LookAtZ);
		}
	}
	

	private void SetLookAtPosition ()
	{
		if (stateHandler.gameState == GameState.Normal)
		{
			Vector2 mouseOffset = new Vector2 (playerInput.mousePosition.x / (Screen.width / 2) - 1, playerInput.mousePosition.y / (Screen.height / 2) - 1);
			float distFromCentre = mouseOffset.magnitude;
	
			if (distFromCentre < 1.4f)
			{
				lookAtTarget = new Vector3 (mouseOffset.x * lookAtAmount.x, mouseOffset.y * lookAtAmount.y, LookAtZ);
			}
		}
	}
	
	
	public void SnapToAttached ()
	{
		if (attachedCamera && attachedCamera.GetComponent <Camera>())
		{
			LookAtCentre ();
			isSmoothChanging = false;
			
			GetComponent <Camera>().isOrthoGraphic = attachedCamera.GetComponent <Camera>().isOrthoGraphic;
			GetComponent <Camera>().fieldOfView = attachedCamera.GetComponent <Camera>().fieldOfView;
			GetComponent <Camera>().orthographicSize = attachedCamera.GetComponent <Camera>().orthographicSize;
			transform.position = attachedCamera.transform.position;
			transform.rotation = attachedCamera.transform.rotation;
			
			if (attachedCamera is GameCamera2D)
			{
				GameCamera2D cam2D = (GameCamera2D) attachedCamera;
				perspectiveOffset = cam2D.perspectiveOffset;
			}
			else
			{
				perspectiveOffset = new Vector2 (0f, 0f);
			}
		}
	}


	/*public void Crossfade (float _changeTime)
	{
		isSmoothChanging = false;
		isCrossfading = true;

		crossfadeTexture = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);
		crossfadeTexture.ReadPixels (new Rect (0f, 0f, Screen.width, Screen.height), 0, 0, false);
		crossfadeTexture.Apply ();

		FadeOut (0f);
		FadeIn (_changeTime);
	}*/

	public void Crossfade (float _changeTime, _Camera _linkedCamera)
	{
		object[] parms = new object[2] { _changeTime, _linkedCamera};
		StartCoroutine ("StartCrossfade", parms);
	}

	private IEnumerator StartCrossfade (object[] parms)
	{
		float _changeTime = (float) parms[0];
		_Camera _linkedCamera = (_Camera) parms[1];

		yield return new WaitForEndOfFrame ();

		crossfadeTexture = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);
		crossfadeTexture.ReadPixels (new Rect (0f, 0f, Screen.width, Screen.height), 0, 0, false);
		crossfadeTexture.Apply ();

		isSmoothChanging = false;
		isCrossfading = true;
		SetGameCamera (_linkedCamera);
		FadeOut (0f);
		FadeIn (_changeTime);
		SnapToAttached ();
	}
	
	
	public void SmoothChange (float _changeTime, MoveMethod method)
	{
		LookAtCentre ();
		moveMethod = method;
		isSmoothChanging = true;
		isCrossfading = false;
		
		startTime = Time.time;
		changeTime = _changeTime;
		
		startPosition = transform.position;
		startRotation = transform.rotation;
		startFOV = GetComponent <Camera>().fieldOfView;
		startOrtho = GetComponent <Camera>().orthographicSize;
		
		startPerspectiveOffset = perspectiveOffset;
	}
	
	
	public void SetGameCamera (_Camera _camera)
	{
		if (attachedCamera != null && attachedCamera is GameCamera25D)
		{
			if (_camera is GameCamera25D)
			{ }
			else
			{
				RemoveBackground ();
			}
		}

		camera.ResetProjectionMatrix ();
		attachedCamera = _camera;
		
		if (attachedCamera && attachedCamera.GetComponent <Camera>())
		{
			this.GetComponent <Camera>().farClipPlane = attachedCamera.GetComponent <Camera>().farClipPlane;
			this.GetComponent <Camera>().nearClipPlane = attachedCamera.GetComponent <Camera>().nearClipPlane;
			
			// Set projection
		//	this.GetComponent <Camera>().fieldOfView = attachedCamera.GetComponent <Camera>().fieldOfView;
		//	this.GetComponent <Camera>().orthographicSize = attachedCamera.GetComponent <Camera>().orthographicSize;
			this.GetComponent <Camera>().orthographic = attachedCamera.GetComponent <Camera>().orthographic;
		}
		
		// Set LookAt
		if (attachedCamera is GameCamera)
		{
			GameCamera gameCam = (GameCamera) attachedCamera;
			cursorAffectsRotation = gameCam.followCursor;
			lookAtAmount = gameCam.cursorInfluence;
		}
		else if (attachedCamera is GameCameraAnimated)
		{
			GameCameraAnimated gameCam = (GameCameraAnimated) attachedCamera;
			if (gameCam.animatedCameraType == AnimatedCameraType.SyncWithTargetMovement)
			{
				cursorAffectsRotation = gameCam.followCursor;
				lookAtAmount = gameCam.cursorInfluence;
			}
			else
			{
				cursorAffectsRotation = false;
			}
		}
		else
		{
			cursorAffectsRotation = false;
		}
		
		// Set background
		if (attachedCamera is GameCamera25D)
		{
			GameCamera25D cam25D = (GameCamera25D) attachedCamera;
			cam25D.SetActiveBackground ();
		}
		
		// TransparencySortMode
		if (attachedCamera is GameCamera2D)
		{
			this.GetComponent <Camera>().transparencySortMode = TransparencySortMode.Orthographic;
		}
		else if (attachedCamera)
		{
			if (attachedCamera.GetComponent <Camera>().orthographic)
			{
				this.GetComponent <Camera>().transparencySortMode = TransparencySortMode.Orthographic;
			}
			else
			{
				this.GetComponent <Camera>().transparencySortMode = TransparencySortMode.Perspective;
			}
		}
	}
	
	
	public void FadeIn (float _timeToFade)
	{
		if (_timeToFade > 0f)
		{
			timeToFade = _timeToFade;
			alpha = 1f;
			fadeType = FadeType.fadeIn;
			fadeStartTime = Time.time;
		}
		else
		{
			alpha = 0f;
			timeToFade = 0f;
		}
	}

	
	public void FadeOut (float _timeToFade)
	{
		if (_timeToFade > 0f)
		{
			alpha = Mathf.Clamp01 (alpha);
			timeToFade = _timeToFade;
			fadeType = FadeType.fadeOut;
			fadeStartTime = Time.time - (alpha * timeToFade);
		}
		else
		{
			alpha = 1f;
			timeToFade = 0f;
		}
	}
	
	
	public bool isFading ()
	{
		if (fadeType == FadeType.fadeOut && alpha < 1f)
		{
			return true;
		}
		else if (fadeType == FadeType.fadeIn && alpha > 0f)
		{
			return true;
		}

		return false;
	}

	
	public void OnDeserializing ()
	{
		FadeIn (0.5f);
	}
	
	
	public Vector3 PositionRelativeToCamera (Vector3 _position)
	{
		return (_position.x * ForwardVector ()) + (_position.z * RightVector ());
	}
	
	
	public Vector3 RightVector ()
	{
		return (transform.right);
	}
	
	
	public Vector3 ForwardVector ()
	{
		Vector3 camForward;
		
		camForward = transform.forward;
		camForward.y = 0;
		
		return (camForward);
	}


	private bool SetAspectRatio ()
	{
		float currentAspectRatio = 0f;
		
		if (Screen.orientation == ScreenOrientation.LandscapeRight || Screen.orientation == ScreenOrientation.LandscapeLeft)
		{
			currentAspectRatio = (float) Screen.width / Screen.height;
		}
		else
		{
			if (Screen.height  > Screen.width && settingsManager.landscapeModeOnly)
			{
				currentAspectRatio = (float) Screen.height / Screen.width;
			}
			else
			{
				currentAspectRatio = (float) Screen.width / Screen.height;
			}
		}
		
		// If the current aspect ratio is already approximately equal to the desired aspect ratio, use a full-screen Rect (in case it was set to something else previously)
		if ((int) (currentAspectRatio * 100) / 100f == (int) (settingsManager.wantedAspectRatio * 100) / 100f)
		{
			borderWidth = 0f;
			borderOrientation = MenuOrientation.Horizontal;

			if (borderCam) 
			{
				Destroy (borderCam.gameObject);
			}
			return false;
		}
		
		// Pillarbox
		if (currentAspectRatio > settingsManager.wantedAspectRatio)
		{
			borderWidth = 1f - settingsManager.wantedAspectRatio / currentAspectRatio;
			borderWidth /= 2f;
			borderOrientation = MenuOrientation.Vertical;
		}
		// Letterbox
		else
		{
			borderWidth = 1f - currentAspectRatio / settingsManager.wantedAspectRatio;
			borderWidth /= 2f;
			borderOrientation = MenuOrientation.Horizontal;
		}


		return true;
	}


	private void SetCameraRect ()
	{
		CreateBorderCamera ();
		float split = 0.49f;

		// Pillarbox
		if (borderOrientation == MenuOrientation.Vertical)
		{
			if (isSplitScreen)
			{
				if (splitOrientation == MenuOrientation.Horizontal)
				{
					if (!isTopLeftSplit)
					{
						camera.rect = new Rect (borderWidth, 0f, 1f - (2*borderWidth), split);
					}
					else
					{
						camera.rect = new Rect (borderWidth, 1f - split, 1f - (2*borderWidth), split);
					}
				}
				else
				{
					if (isTopLeftSplit)
					{
						camera.rect = new Rect (borderWidth, 0f, split - borderWidth, 1f);
					}
					else
					{
						camera.rect = new Rect (1f - split, 0f, split - borderWidth, 1f);
					}
				}
			}
			else
			{
				camera.rect = new Rect (borderWidth, 0f, 1f-(2*borderWidth), 1f);
			}

		}
		// Letterbox
		else
		{
			if (isSplitScreen)
			{
				if (splitOrientation == MenuOrientation.Horizontal)
				{
					if (isTopLeftSplit)
					{
						camera.rect = new Rect (0f, 1f - split, 1f, split - borderWidth);
					}
					else
					{
						camera.rect = new Rect (0f, borderWidth, 1f, split - borderWidth);
					}
				}
				else
				{
					if (isTopLeftSplit)
					{
						camera.rect = new Rect (0f, borderWidth, split, 1f - (2*borderWidth));
					}
					else
					{
						camera.rect = new Rect (1f - split, borderWidth, split, 1f - (2*borderWidth));
					}
				}
			}
			else
			{
				camera.rect = new Rect (0f, borderWidth, 1f, 1f - (2*borderWidth));
			}
		}
	}


	private void CreateBorderCamera ()
	{
		if (!borderCam)
		{
			// Make a new camera behind the normal camera which displays black; otherwise the unused space is undefined
			borderCam = new GameObject ("BorderCamera", typeof (Camera)).camera;
			borderCam.transform.parent = this.transform;
			borderCam.depth = int.MinValue;
			borderCam.clearFlags = CameraClearFlags.SolidColor;
			borderCam.backgroundColor = Color.black;
			borderCam.cullingMask = 0;
		}
	}
	

	public Vector2 LimitMouseToAspect (Vector2 mousePosition)
	{
		if (settingsManager == null || !settingsManager.forceAspectRatio)
		{
			return mousePosition;
		}

		if (borderOrientation == MenuOrientation.Horizontal)
		{
			// Letterbox
			int yOffset = (int) (Screen.height * borderWidth);
			
			if (mousePosition.y < yOffset)
			{
				mousePosition.y = yOffset;
			}
			else if (mousePosition.y > (Screen.height - yOffset))
			{
				mousePosition.y = Screen.height - yOffset;
			}
		}
		else
		{
			// Pillarbox
			int xOffset = (int) (Screen.width * borderWidth);

			if (mousePosition.x < xOffset)
			{
				mousePosition.x = xOffset;
			}
			else if (mousePosition.x > (Screen.width - xOffset))
			{
				mousePosition.x = Screen.width - xOffset;
			}
		}

		return mousePosition;
	}


	public static Rect _LimitMenuToAspect (Rect rect)
	{
		if (mainCam == null)
		{
			mainCam = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
		}

		return mainCam.LimitMenuToAspect (rect);
	}


	public Rect LimitMenuToAspect (Rect rect)
	{
		if (settingsManager == null || !settingsManager.forceAspectRatio)
		{
			return rect;
		}

		if (borderOrientation == MenuOrientation.Horizontal)
		{
			// Letterbox
			int yOffset = (int) (Screen.height * borderWidth);
			
			if (rect.y < yOffset)
			{
				rect.y = yOffset;
			}
			else if (rect.y + rect.height > (Screen.height - yOffset))
			{
				rect.y = Screen.height - yOffset - rect.height;
			}
		}
		else
		{
			// Pillarbox
			int xOffset = (int) (Screen.width * borderWidth);
			
			if (rect.x < xOffset)
			{
				rect.x = xOffset;
			}
			else if (rect.x + rect.width > (Screen.width - xOffset))
			{
				rect.x = Screen.width - xOffset - rect.width;
			}
		}
		
		return rect;
	}


	public void RemoveSplitScreen ()
	{
		isSplitScreen = false;
		SetCameraRect ();

		if (splitCamera)
		{
			splitCamera.RemoveSplitScreen ();
			splitCamera = null;
		}
	}


	public void SetSplitScreen (_Camera _camera1, _Camera _camera2, MenuOrientation _splitOrientation, bool _isTopLeft)
	{
		splitCamera = _camera2;
		isSplitScreen = true;
		splitOrientation = _splitOrientation;
		isTopLeftSplit = _isTopLeft;
		
		SetGameCamera (_camera1);
		SnapToAttached ();

		StartSplitScreen ();
	}


	public void StartSplitScreen ()
	{
		splitCamera.SetSplitScreen (splitOrientation, !isTopLeftSplit);
		SetCameraRect ();
	}
	
}
