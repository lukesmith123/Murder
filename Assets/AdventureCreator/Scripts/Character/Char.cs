/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Char.cs"
 * 
 *	This is the base class for both NPCs and the Player.
 *	It contains the functions needed for animation and movement.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{
	
	public class Char : MonoBehaviour
	{
		
		public AnimationEngine animationEngine = AnimationEngine.SpritesUnity;	// Enum
		public AnimEngine animEngine;											// ScriptableObject
		public TalkingAnimation talkingAnimation = TalkingAnimation.Standard;
		
		public Texture2D portraitGraphic;
		public Color speechColor = Color.white;
		
		// 3D variables

		public Transform leftHandBone;
		public Transform rightHandBone;

		// Legacy variables
		
		public AnimationClip idleAnim;
		public AnimationClip walkAnim;
		public AnimationClip runAnim;
		public AnimationClip talkAnim;
		public AnimationClip turnLeftAnim;
		public AnimationClip turnRightAnim;
		
		public Transform upperBodyBone;
		public Transform leftArmBone;
		public Transform rightArmBone;
		public Transform neckBone;

		public float animCrossfadeSpeed = 0.2f;

		// Mecanim variables

		public string moveSpeedParameter = "Speed";
		public string turnParameter = "";
		public string talkParameter = "IsTalking";
		public string directionParameter = "Direction";
		public bool relyOnRootMotion = false;
		public string lastPlayedAnim = "";

		// 2D variables
		
		public Animator animator;
		public Transform spriteChild;
		
		public string idleAnimSprite = "idle";
		public string walkAnimSprite = "walk";
		public string runAnimSprite = "run";
		public string talkAnimSprite = "talk";

		public bool lockScale = false;
		public float spriteScale = 1f;
		public bool lockDirection = false;
		public string spriteDirection = "D";

		public bool doDirections = true;
		public bool crossfadeAnims = false;
		public bool doDiagonals = false;
		public bool isTalking = false;
		public AC_2DFrameFlipping frameFlipping = AC_2DFrameFlipping.None;

		private bool flipFrames = false;
		
		// Movement variables
		
		public float walkSpeedScale = 2f;
		public float runSpeedScale = 6f;
		public float turnSpeed = 7f;
		public float acceleration = 6f;
		public float deceleration = 0f;
		public float sortingMapScale = 1f;
		public bool isReversing = false;
		public bool turnBeforeWalking = false;

		// Rigidbody variables

		public bool ignoreGravity = false;
		public bool freezeRigidbodyWhenIdle = false;

		// Sound variables
		
		public AudioClip walkSound;
		public AudioClip runSound;
		public Sound soundChild;
		protected AudioSource audioSource;
		
		public Paths activePath = null;
		public bool isRunning { get; set; }
		
		public CharState charState;
		
		protected float moveSpeed;
		protected Vector3 moveDirection; 
		
		protected int targetNode = 0;
		protected bool pausePath = false;
		
		private Vector3 lookDirection;
		private float pausePathTime;
		private int prevNode = 0;
		private Vector3 oldPosition;
		
		private bool tankTurning = false;
		private bool isTurning = false;
		private bool isTurningLeft = false;
		
		protected StateHandler stateHandler;
		protected SettingsManager settingsManager;
		private MainCamera mainCamera;
		
		
		protected void Awake ()
		{
			ResetAnimationEngine ();
			ResetBaseClips ();
			GetSettingsManager ();
			
			if (spriteChild && spriteChild.GetComponent <Animator>())
			{
				animator = spriteChild.GetComponent <Animator>();
			}
			
			if (soundChild && soundChild.gameObject.GetComponent <AudioSource>())
			{
				audioSource = soundChild.gameObject.GetComponent <AudioSource>();
			}
			
			if (GetComponent <Animator>())
			{
				animator = GetComponent <Animator>();
			}
		}
		
		
		private void Start ()
		{
			if (GameObject.FindWithTag (Tags.persistentEngine) && GameObject.FindWithTag (Tags.persistentEngine).GetComponent <StateHandler>())
			{
				stateHandler = GameObject.FindWithTag(Tags.persistentEngine).GetComponent<StateHandler>();
			}
		}


		private void GetSettingsManager ()
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}
		}
		
		
		protected void Update ()
		{
			if (mainCamera == null)
			{
				mainCamera = GameObject.FindWithTag (Tags.mainCamera).GetComponent <MainCamera>();
			}

			if (settingsManager == null)
			{
				GetSettingsManager ();
			}

			if (settingsManager != null && spriteChild && mainCamera)
			{
				UpdateSpriteChild (settingsManager.IsTopDown (), settingsManager.IsUnity2D ());
			}
		}
		
		
		protected void FixedUpdate ()
		{
			if (settingsManager != null)
			{
				PathUpdate ();
				SpeedUpdate ();
				PhysicsUpdate ();
				AnimUpdate ();
				MoveUpdate ();
			}
		}

		
		protected void PathUpdate ()
		{
			if (activePath && activePath.nodes.Count > 0)
			{
				if (pausePath)
				{
					if (Time.time > pausePathTime)
					{
						pausePath = false;
						SetNextNodes ();
					}
				}
				else
				{
					Vector3 direction = activePath.nodes[targetNode] - transform.position;
					Vector3 lookDir = new Vector3 (direction.x, 0f, direction.z);

					if (settingsManager && settingsManager.IsUnity2D ())
					{
						direction.z = 0f;
						SetMoveDirection (direction);
						lookDir = new Vector3 (direction.x, 0f, direction.y);
						SetLookDirection (lookDir, false);
					}
					else if (activePath.affectY)
					{
						SetMoveDirection (direction);
						SetLookDirection (lookDir, false);
					}
					else
					{
						SetLookDirection (lookDir, false);
						SetMoveDirectionAsForward ();
					}
					
					float nodeThreshold = 0.1f;
					if (settingsManager)
					{
						nodeThreshold = 1.05f - settingsManager.destinationAccuracy;
					}
					if (isRunning)
					{
						nodeThreshold *= runSpeedScale / walkSpeedScale;
					}

					if ((settingsManager.IsUnity2D () && direction.magnitude < nodeThreshold) ||
					   (activePath.affectY && direction.magnitude < nodeThreshold) ||
					   (!activePath.affectY && lookDir.magnitude < nodeThreshold))
					{
						if ((targetNode == 0 && prevNode == 0) || activePath.nodePause <= 0)
						{
							SetNextNodes ();
						}
						else
						{
							PausePath ();
						}
					}
				}
			}
		}


		private void SpeedUpdate ()
		{
			if (charState == CharState.Move)
			{
				Accelerate ();
			}
			else if (charState == CharState.Decelerate || charState == CharState.Custom)
			{
				Decelerate ();
			}
		}
		
		
		private void PhysicsUpdate ()
		{
			if (rigidbody)
			{
				if (ignoreGravity)
				{
					rigidbody.useGravity = false;
				}

				else if (charState == CharState.Custom && moveSpeed < 0.01f)
				{
					rigidbody.useGravity = false;
				}
				else
				{
					if (activePath && activePath.affectY)
					{
						rigidbody.useGravity = false;
					}
					else
					{
						rigidbody.useGravity = true;
					}
				}
			}
			else if (rigidbody2D)
			{
				if (ignoreGravity)
				{
					rigidbody2D.gravityScale = 0f;
				}
				
				else if (charState == CharState.Custom && moveSpeed < 0.01f)
				{
					rigidbody2D.gravityScale = 0f;
				}
				else
				{
					if (activePath && activePath.affectY)
					{
						rigidbody2D.gravityScale = 0f;
					}
					else
					{
						rigidbody2D.gravityScale = 1f;
					}
				}
			}
		}
		
		
		private void AnimUpdate ()
		{
			if (charState == CharState.Idle || charState == CharState.Decelerate)
			{
				if (isTurning)
				{
					if (isTurningLeft)
					{
						animEngine.PlayTurnLeft ();
					}
					else if (!isTurningLeft)
					{
						animEngine.PlayTurnRight ();
					}
					else
					{
						animEngine.PlayIdle ();
					}
				}
				else
				{
					if (isTalking && talkingAnimation == TalkingAnimation.Standard)
					{
						animEngine.PlayTalk ();
					}
					else
					{
						animEngine.PlayIdle ();
					}
				}
				
				StopStandardAudio ();
			}
			
			else if (charState == CharState.Move)
			{
				if (isRunning)
				{
					animEngine.PlayRun ();
				}
				else
				{
					animEngine.PlayWalk ();
				}
				
				PlayStandardAudio ();
			}
			
			else
			{
				StopStandardAudio ();
			}
		}
		
		
		
		private void MoveUpdate ()
		{
			if (animEngine)
			{
				if (moveSpeed > 0.01f && (!animEngine.rootMotion || !relyOnRootMotion))
				{
					Vector3 newVel;
					newVel = moveDirection * moveSpeed * walkSpeedScale * sortingMapScale;

					if (settingsManager)
					{
						if (settingsManager.IsTopDown ())
						{
							newVel.z *= settingsManager.verticalReductionFactor;
						}
						else if (settingsManager.IsUnity2D ())
						{
							newVel.y *= settingsManager.verticalReductionFactor;
							newVel.z = 0f;
						}
					}

					if (rigidbody)
					{
						rigidbody.MovePosition (rigidbody.position + newVel * Time.deltaTime);
					}
					else
					{
						transform.position += (newVel * Time.deltaTime);
					}
				}

				if (isTurning)
				{
					if (animEngine && animEngine.turningIsLinear && moveSpeed > 0.01f)
					{
						Turn (true);
					}
					else
					{
						Turn (false);
					}
				}
			}

			if (rigidbody)
			{
				if (freezeRigidbodyWhenIdle && (charState == CharState.Custom || charState == CharState.Idle))
				{
					rigidbody.constraints = RigidbodyConstraints.FreezeAll;
				}
				else
				{
					rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
				}
			}
		}
		
		
		private void Accelerate ()
		{
			float targetSpeed;

			if (GetComponent <Animator>())
			{
				if (isRunning)
				{
					targetSpeed = runSpeedScale;
				}
				else
				{
					targetSpeed = walkSpeedScale;
				}
			}
			else
			{
				if (isRunning)
				{
					targetSpeed = moveDirection.magnitude * runSpeedScale / walkSpeedScale;
				}
				else
				{
					targetSpeed = moveDirection.magnitude;
				}
			}

			moveSpeed = Mathf.Lerp (moveSpeed, targetSpeed, Time.deltaTime * acceleration);
		}
		
		
		private void Decelerate ()
		{
			if (deceleration <= 0f)
			{
				moveSpeed = Mathf.Lerp (moveSpeed, 0f, Time.deltaTime * acceleration);
			}
			else
			{
				moveSpeed = Mathf.Lerp (moveSpeed, 0f, Time.deltaTime * deceleration);
			}
			
			if (moveSpeed < 0.01f)
			{
				moveSpeed = 0f;
				
				if (charState != CharState.Custom)
				{
					charState = CharState.Idle;
				}
			}
		}
		
		
		public bool IsTurning ()
		{
			return isTurning;
		}
		
		
		public void TankTurnLeft ()
		{
			lookDirection = -transform.right;
			tankTurning = true;
			isTurning = true;
		}
		
		
		public void TankTurnRight ()
		{
			lookDirection = transform.right;
			tankTurning = true;
			isTurning = true;
		}
		
		
		public void StopTurning ()
		{
			lookDirection = transform.forward;
			tankTurning = false;
			isTurning = false;
		}
		
		
		public void Turn (bool isInstant)
		{
			if (lookDirection != Vector3.zero)
			{
				Quaternion targetRotation = Quaternion.LookRotation (lookDirection, Vector3.up);

				if (isInstant)
				{
					isTurning = false;
					if (rigidbody)
					{
						rigidbody.rotation = targetRotation;
					}
					else
					{
						transform.rotation = targetRotation;
					}
					return;
				}

				Quaternion newRotation;
				
				if (animEngine && animEngine.turningIsLinear)
				{
					if (rigidbody)
					{
						newRotation = Quaternion.RotateTowards (rigidbody.rotation, targetRotation, 5f);
					}
					else
					{
						newRotation = Quaternion.RotateTowards (transform.rotation, targetRotation, 5f);
					}
				}
				else
				{
					float actualTurnSpeed = turnSpeed * Time.deltaTime;
					if (tankTurning || moveSpeed == 0f)
					{
						actualTurnSpeed /= 2;
					}
					if (rigidbody)
					{
						newRotation = Quaternion.Lerp (rigidbody.rotation, targetRotation, actualTurnSpeed);
					}
					else
					{
						newRotation = Quaternion.Lerp (transform.rotation, targetRotation, actualTurnSpeed);
					}
				}

				isTurning = true;
				if (rigidbody)
				{
					rigidbody.MoveRotation (newRotation);
				}
				else
				{
					transform.rotation = newRotation;
				}

				// Determine if character is turning left or right (courtesy of Duck: http://answers.unity3d.com/questions/26783/how-to-get-the-signed-angle-between-two-quaternion.html)
				
				Vector3 forwardA = targetRotation * Vector3.forward;
				Vector3 forwardB = transform.rotation * Vector3.forward;
				
				float angleA = Mathf.Atan2 (forwardA.x, forwardA.z) * Mathf.Rad2Deg;
				float angleB = Mathf.Atan2 (forwardB.x, forwardB.z) * Mathf.Rad2Deg;
				
				float angleDiff = Mathf.DeltaAngle( angleA, angleB );
				
				if (angleDiff < 0f)
				{
					isTurningLeft = false;
				}
				else
				{
					isTurningLeft = true;
				}
				
				if (isTurning && Quaternion.Angle (Quaternion.LookRotation (lookDirection), transform.rotation) < 3f)
				{
					isTurning = false;
				}
			}
		}
		
		
		public void SetLookDirection (Vector3 _direction, bool isInstant)
		{
			lookDirection = new Vector3 (_direction.x, 0f, _direction.z);
			Turn (isInstant);
		}
		
		
		public void SetMoveDirection (Vector3 _direction)
		{
			if (_direction != Vector3.zero)
			{
				Quaternion targetRotation = Quaternion.LookRotation (_direction, Vector3.up);
				moveDirection = targetRotation * Vector3.forward;
				moveDirection.Normalize ();
			}
		}
		
		
		public void SetMoveDirectionAsForward ()
		{
			isReversing = false;
			moveDirection = transform.forward;
			if (settingsManager && settingsManager.IsUnity2D ())
			{
				moveDirection = new Vector3 (moveDirection.x, moveDirection.z, 0f);
			}
			moveDirection.Normalize ();
		}
		
		
		public void SetMoveDirectionAsBackward ()
		{
			isReversing = true;
			moveDirection = -transform.forward;
			if (settingsManager && settingsManager.IsUnity2D ())
			{
				moveDirection = new Vector3 (moveDirection.x, moveDirection.z, 0f);
			}
			moveDirection.Normalize ();
		}
		
		
		public Vector3 GetMoveDirection ()
		{
			return moveDirection;	
		}
		
		
		private void SetNextNodes ()
		{
			int tempPrev = targetNode;
			
			if (this.GetComponent <Player>() && stateHandler.gameState == GameState.Normal)
			{
				targetNode = activePath.GetNextNode (targetNode, prevNode, true);
			}
			else
			{
				targetNode = activePath.GetNextNode (targetNode, prevNode, false);
			}
			
			prevNode = tempPrev;
			
			if (targetNode == -1)
			{
				EndPath ();
			}
		}
		
		
		public void EndPath ()
		{
			if (GetComponent <Paths>() && activePath == GetComponent <Paths>())
			{
				activePath.nodes.Clear ();
			}

			activePath = null;
			targetNode = 0;

			if (charState == CharState.Move)
			{
				charState = CharState.Decelerate;
			}
		}
		
		
		public void Halt ()
		{
			activePath = null;
			targetNode = 0;
			moveSpeed = 0f;

			if (charState == CharState.Move || charState == CharState.Decelerate)
			{
				charState = CharState.Idle;
			}
		}
		
		
		protected void ReverseDirection ()
		{
			int tempPrev = targetNode;
			targetNode = prevNode;
			prevNode = tempPrev;
		}
		
		
		private void PausePath ()
		{
			charState = CharState.Decelerate;
			pausePath = true;
			pausePathTime = Time.time + activePath.nodePause;
		}


		private void TurnBeforeWalking ()
		{
			Vector3 direction = activePath.nodes[1] - transform.position;
			
			if (settingsManager && settingsManager.IsUnity2D ())
			{
				SetLookDirection (new Vector3 (direction.x, 0f, direction.y), false);
			}
			else
			{
				SetLookDirection (new Vector3 (direction.x, 0f, direction.z), false);
			}
		}


		protected bool IsTurningBeforeWalking ()
		{
			if (isTurning && CanTurnBeforeMoving ())
			{
				return true;
			}
			return false;
		}


		private bool CanTurnBeforeMoving ()
		{
			if (turnBeforeWalking && activePath == GetComponent <Paths>() && targetNode <= 1 && activePath.nodes.Count > 1)
			{
				return true;
			}
			return false;
		}


		private void SetPath (Paths pathOb, PathSpeed _speed, int _targetNode, int _prevNode)
		{
			activePath = pathOb;
			targetNode = _targetNode;
			prevNode = _prevNode;

			if (CanTurnBeforeMoving ())
			{
				TurnBeforeWalking ();
			}
			
			if (pathOb)
			{
				if (_speed == PathSpeed.Run)
				{
					isRunning = true;
				}
				else
				{
					isRunning = false;
				}
			}

			charState = CharState.Idle;
		}


		public void SetPath (Paths pathOb, PathSpeed _speed)
		{
			SetPath (pathOb, _speed, 0, 0);
		}


		public void SetPath (Paths pathOb)
		{
			SetPath (pathOb, pathOb.pathSpeed, 0, 0);
		}
		
		
		public void SetPath (Paths pathOb, int _targetNode, int _prevNode)
		{
			SetPath (pathOb, pathOb.pathSpeed, _targetNode, _prevNode);
		}
		
		
		protected void CheckIfStuck ()
		{
			// Check for null movement error: if not moving on a path, end the path
			
			/*Vector3 newPosition = rigidbody.position;
			if (oldPosition == newPosition)
			{
				Debug.Log ("Stuck in active path - removing");
				EndPath ();
			}
			
			oldPosition = newPosition;*/
		}

		
		public Paths GetPath ()
		{
			return activePath;
		}


		public int GetTargetNode ()
		{
			return targetNode;
		}
		
		
		public int GetPrevNode ()
		{
			return prevNode;
		}
		
		
		public void MoveToPoint (Vector3 point, bool run)
		{
			List<Vector3> pointData = new List<Vector3>();
			pointData.Add (point);
			MoveAlongPoints (pointData.ToArray (), run);
		}


		protected void ChangePathfindSpeed (bool run)
		{
			Paths path = GetComponent <Paths>();
			if (path)
			{
				if (run && !isRunning)
				{
					SetPath (path, PathSpeed.Run);
				}
				else if (!run && isRunning)
				{
					SetPath (path, PathSpeed.Walk);
				}
			}
		}
		
		
		public void MoveAlongPoints (Vector3[] pointData, bool run)
		{
			Paths path = GetComponent <Paths>();
			if (path)
			{
				path.BuildNavPath (pointData);
				
				if (run)
				{
					SetPath (path, PathSpeed.Run);
				}
				else
				{
					SetPath (path, PathSpeed.Walk);
				}
			}
			else
			{
				Debug.LogWarning (this.name + " cannot pathfind without a Paths component");
			}
		}
		
		
		public void ResetBaseClips ()
		{
			// Remove all animations except Idle, Walk, Run and Talk

			if (spriteChild && spriteChild.animation)
			{
				List <string> clipsToRemove = new List <string>();
				
				foreach (AnimationState state in spriteChild.animation)
				{
					if ((idleAnim == null || state.name != idleAnim.name) && (walkAnim == null || state.name != walkAnim.name) && (runAnim == null || state.name != runAnim.name))
					{
						clipsToRemove.Add (state.name);
					}
				}
				
				foreach (string _clip in clipsToRemove)
				{
					spriteChild.animation.RemoveClip (_clip);
				}
			}

			if (animation)
			{
				List <string> clipsToRemove = new List <string>();
				
				foreach (AnimationState state in animation)
				{
					if ((idleAnim == null || state.name != idleAnim.name) && (walkAnim == null || state.name != walkAnim.name) && (runAnim == null || state.name != runAnim.name))
					{
						clipsToRemove.Add (state.name);
					}
				}
				
				foreach (string _clip in clipsToRemove)
				{
					animation.RemoveClip (_clip);
				}
			}
			
		}
		
		
		public string GetSpriteDirection ()
		{
			return ("_" + spriteDirection);
		}


		public int GetSpriteDirectionInt ()
		{
			if (spriteDirection == "D")
			{
				return 0;
			}
			if (spriteDirection == "L")
			{
				return 1;
			}
			if (spriteDirection == "R")
			{
				return 2;
			}
			if (spriteDirection == "U")
			{
				return 3;
			}
			if (spriteDirection == "DL")
			{
				return 4;
			}
			if (spriteDirection == "DR")
			{
				return 5;
			}
			if (spriteDirection == "UL")
			{
				return 6;
			}
			if (spriteDirection == "UR")
			{
				return 7;
			}

			return 0;
		}


		public void SetSpriteDirection (CharDirection direction)
		{
			if (direction == CharDirection.Down)
			{
				spriteDirection = "D";
			}
			else if (direction == CharDirection.Left)
			{
				spriteDirection = "L";
			}
			else if (direction == CharDirection.Right)
			{
				spriteDirection = "R";
			}
			else if (direction == CharDirection.Up)
			{
				spriteDirection = "U";
			}
		}

		
		private string SetSpriteDirection (float rightAmount, float forwardAmount)
		{
			float angle = Vector2.Angle (new Vector2 (1f, 0f), new Vector2 (rightAmount, forwardAmount));

			if (doDiagonals)
			{
				if (forwardAmount > 0f)
				{
					if (angle > 22.5f && angle < 67.5f)
					{
						return "UR";
					}
					else if (angle > 112.5f && angle < 157.5f)
					{
						return "UL";
					}
				}
				else
				{
					if (angle > 22.5f && angle < 67.55f)
					{
						return "DR";
					}
					else if (angle > 112.5f && angle < 157.5f)
					{
						return "DL";
					}
				}
			}
			
			if (forwardAmount > 0f)
			{
				if (angle > 45f && angle < 135f)
				{
					return "U";
				}
			}
			else
			{
				if (angle > 45f && angle < 135f)
				{
					return "D";
				}
			}
			
			if (rightAmount > 0f)
			{
				return "R";
			}
			
			return "L";
		}
		
		
		private void StopStandardAudio ()
		{
			if (audioSource && audioSource.isPlaying)
			{
				if ((runSound && audioSource.clip == runSound) || (walkSound && audioSource.clip == walkSound))
				{
					audioSource.Stop ();
				}
			}
		}
		
		
		private void PlayStandardAudio ()
		{
			if (audioSource)
			{
				if (isRunning && runSound)
				{
					if (audioSource.isPlaying && audioSource.clip == runSound)
					{
						return;
					}

					audioSource.loop = false;
					audioSource.clip = runSound;
					audioSource.Play ();
				}
				
				else if (walkSound)
				{
					if (audioSource.isPlaying && audioSource.clip == walkSound)
					{
						return;
					}

					audioSource.loop = false;
					audioSource.clip = walkSound;
					audioSource.Play ();
				}
			}
		}
		
		
		public void ResetAnimationEngine ()
		{
			string className = "AnimEngine_" + animationEngine.ToString ();
			
			if (animEngine == null || animEngine.ToString () != className)
			{
				animEngine = (AnimEngine) ScriptableObject.CreateInstance (className);
				animEngine.Declare (this);
			}
		}


		private void UpdateSpriteChild (bool isTopDown, bool isUnity2D)
		{
			float forwardAmount = 0f;
			float rightAmount = 0f;

			if (isTopDown || isUnity2D)
			{
				forwardAmount = Vector3.Dot (Vector3.forward, transform.forward.normalized);
				rightAmount = Vector3.Dot (Vector3.right, transform.forward.normalized);
			}
			else
			{
				forwardAmount = Vector3.Dot (mainCamera.ForwardVector ().normalized, transform.forward.normalized);
				rightAmount = Vector3.Dot (mainCamera.RightVector ().normalized, transform.forward.normalized);
			}

			if (charState == CharState.Custom)
			{
				flipFrames = false;
			}
			else
			{
				if (!lockDirection)
				{
					spriteDirection = SetSpriteDirection (rightAmount, forwardAmount);
				}

				if (!doDirections)
				{
					flipFrames = false;
				}
				else if (frameFlipping == AC_2DFrameFlipping.LeftMirrorsRight && spriteDirection.Contains ("L"))
				{
					spriteDirection = spriteDirection.Replace ("L", "R");
					flipFrames = true;
				}
				else if (frameFlipping == AC_2DFrameFlipping.RightMirrorsLeft && spriteDirection.Contains ("R"))
				{
					spriteDirection = spriteDirection.Replace ("R", "L");
					flipFrames = true;
				}
				else
				{
					flipFrames = false;
				}
			}
			
			if ((flipFrames && spriteChild.localScale.x > 0f) || (!flipFrames && spriteChild.localScale.x < 0f))
			{
				spriteChild.localScale = new Vector3 (-spriteChild.localScale.x, spriteChild.localScale.y, spriteChild.localScale.z);
			}
			
			if (isTopDown)
			{
				if (animEngine && !animEngine.isSpriteBased)
				{
					spriteChild.rotation = transform.rotation;
					spriteChild.RotateAround (transform.position, Vector3.right, 90f);
				}
				else
				{
					spriteChild.rotation = Quaternion.Euler (90f, 0, 0);
				}
			}
			else
			{
				spriteChild.rotation = Quaternion.Euler (spriteChild.rotation.eulerAngles.x, mainCamera.transform.rotation.eulerAngles.y, spriteChild.rotation.eulerAngles.z);
			}

			if (spriteChild.GetComponent <FollowSortingMap>())
			{	
				if (!lockScale)
				{
					spriteScale = spriteChild.GetComponent <FollowSortingMap>().GetLocalScale ();
				}

				if (spriteScale != 0f)
				{
					if (spriteChild.localScale.x > 0f)
					{
						spriteChild.localScale = new Vector3 (spriteScale, spriteScale, spriteScale);
					}
					else
					{
						spriteChild.localScale = new Vector3 (-spriteScale, spriteScale, spriteScale);
					}

					if (lockScale)
					{
						sortingMapScale = spriteScale;
					}
					else
					{
						sortingMapScale = spriteChild.GetComponent <FollowSortingMap>().GetLocalSpeed ();
					}
				}
			}
		}


		public void AssignPortraitGraphicFromResource (string textureName)
		{
			if (textureName != "")
			{
				Texture2D texture = AdvGame.FindTextureResource (textureName);
				if (texture != null)
				{
					portraitGraphic = texture;
				}
			}
		}


		public void AssignStandardSoundFromResource (AnimStandard standard, string clipName)
		{
			AudioClip clip = AdvGame.FindAudioClipResource (clipName);
			if (clip != null)
			{
				if (standard == AnimStandard.Walk)
				{
					walkSound = clip;
				}
				else if (standard == AnimStandard.Run)
				{
					runSound = clip;
				}
			}
		}
		
		
		public string GetStandardSoundName (AnimStandard standard)
		{
			if (standard == AnimStandard.Walk && walkSound != null)
			{
				return walkSound.name;
			}
			else if (standard == AnimStandard.Run && runSound != null)
			{
				return runSound.name;
			}

			return "";
		}


		public void AssignStandardAnimClipFromResource (AnimStandard standard, string clipName)
		{
			AnimationClip clip = AdvGame.FindAnimClipResource (clipName);
			if (clip != null)
			{
				if (standard == AnimStandard.Idle)
				{
					idleAnim = clip;
				}
				else if (standard == AnimStandard.Run)
				{
					runAnim = clip;
				}
				else if (standard == AnimStandard.Walk)
				{
					walkAnim = clip;
				}
				else if (standard == AnimStandard.Talk)
				{
					talkAnim = clip;
				}
			}
		}


		public string GetStandardAnimClipName (AnimStandard standard)
		{
			if (standard == AnimStandard.Idle && idleAnim != null)
			{
				return idleAnim.name;
			}
			else if (standard == AnimStandard.Walk && walkAnim != null)
			{
				return walkAnim.name;
			}
			else if (standard == AnimStandard.Run && runAnim != null)
			{
				return runAnim.name;
			}
			else if (standard == AnimStandard.Talk && talkAnim != null)
			{
				return talkAnim.name;
			}

			return "";
		}


		public void SetSorting (int order)
		{
			if (spriteChild)
			{
				if (spriteChild.GetComponent <FollowSortingMap>())
				{
					spriteChild.GetComponent <FollowSortingMap>().LockSortingOrder (order);
				}
				else
				{
					spriteChild.renderer.sortingOrder = order;
				}
			}

			if (renderer)
			{
				if (GetComponent <FollowSortingMap>())
				{
					GetComponent <FollowSortingMap>().LockSortingOrder (order);
				}
				else
				{
					renderer.sortingOrder = order;
				}
			}
		}


		public void SetSorting (string layer)
		{
			if (spriteChild)
			{
				if (spriteChild.GetComponent <FollowSortingMap>())
				{
					spriteChild.GetComponent <FollowSortingMap>().LockSortingLayer (layer);
				}
				else
				{
					spriteChild.renderer.sortingLayerName = layer;
				}
			}

			if (renderer)
			{
				if (GetComponent <FollowSortingMap>())
				{
					GetComponent <FollowSortingMap>().LockSortingLayer (layer);
				}
				else
				{
					renderer.sortingLayerName = layer;
				}
			}
		}


		public void ReleaseSorting ()
		{
			if (spriteChild && spriteChild.renderer && spriteChild.GetComponent <FollowSortingMap>())
			{
				spriteChild.GetComponent <FollowSortingMap>().lockSorting = false;
			}

			if (renderer && GetComponent <FollowSortingMap>())
			{
				GetComponent <FollowSortingMap>().lockSorting = false;
			}
		}


		public float GetMoveSpeed ()
		{
			return moveSpeed;
		}


		private void OnLevelWasLoaded ()
		{
			lockDirection = false;
			lockScale = false;
			ReleaseSorting ();
		}
	
	}
}
