/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"AdvGame.cs"
 * 
 *	This script provides a number of static functions used by various game scripts.
 * 
 * 	The "DrawOutline" function is based on Bérenger's code: http://wiki.unity3d.com/index.php/ShadowAndOutline
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class AdvGame : ScriptableObject
{

	public static List<AC.Action> copiedActions = new List<AC.Action>();


	public static References GetReferences ()
	{
		References references = (References) Resources.Load (Resource.references);
		
		if (references)
		{
			return (references);
		}
		
		return (null);
	}


	public static void DrawCubeCollider (Transform transform, Color color)
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = color;
		Gizmos.DrawCube (Vector3.zero, Vector3.one);
	}


	public static void DrawBoxCollider (Transform transform, Color color)
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = color;
		Gizmos.DrawLine (new Vector3 (-0.5f, -0.5f), new Vector3 (-0.5f, 0.5f));
		Gizmos.DrawLine (new Vector3 (-0.5f, 0.5f), new Vector3 (0.5f, 0.5f));
		Gizmos.DrawLine (new Vector3 (0.5f, 0.5f), new Vector3 (0.5f, -0.5f));
		Gizmos.DrawLine (new Vector3 (0.5f, -0.5f), new Vector3 (-0.5f, -0.5f));
	}


	public static void DrawPolygonCollider (Transform transform, PolygonCollider2D poly, Color color)
	{
		Gizmos.color = color;
		Gizmos.DrawLine (transform.TransformPoint (poly.points [0]), transform.TransformPoint (poly.points [poly.points.Length-1]));
		for (int i=0; i<poly.points.Length-1; i++)
		{
			Gizmos.DrawLine (transform.TransformPoint (poly.points [i]), transform.TransformPoint (poly.points [i+1]));
		}
	}


	public static double CalculateFormula (string formula)
	{
		#if UNITY_WP8
		return 0;
		#else
		return ((double) new System.Xml.XPath.XPathDocument
				(new System.IO.StringReader("<r/>")).CreateNavigator().Evaluate
		        (string.Format("number({0})", new System.Text.RegularExpressions.Regex (@"([\+\-\*])").Replace (formula, " ${1} ").Replace ("/", " div ").Replace ("%", " mod "))));
		#endif
	}


	public static string ConvertTokens (string _text)
	{
		if (_text.Contains ("[var:"))
		{
			RuntimeVariables runtimeVariables = GameObject.FindWithTag (Tags.persistentEngine).GetComponent <RuntimeVariables>();
			foreach (GVar _var in runtimeVariables.globalVars)
			{
				string tokenText = "[var:" + _var.id + "]";
				if (_text.Contains (tokenText))
				{
					_var.Download ();
					_text = _text.Replace (tokenText, _var.GetValue ());
				}
			}
		}
		else if (_text.Contains ("[localvar:"))
		{
			LocalVariables localVariables = GameObject.FindWithTag (Tags.gameEngine).GetComponent <LocalVariables>();
			foreach (GVar _var in localVariables.localVars)
			{
				string tokenText = "[localvar:" + _var.id + "]";
				if (_text.Contains (tokenText))
				{
					_text = _text.Replace (tokenText, _var.GetValue ());
				}
			}
		}
		
		return _text;
	}


	#if UNITY_EDITOR

	public static void DrawNodeCurve (Rect start, Rect end, Color color, int offset)
	{
		Vector2 endPos = new Vector2 (end.x - 15, end.y + 10);
		DrawNodeCurve (start, endPos, color, offset);
	}
	
	
	public static void DrawNodeCurve (Rect start, Vector2 end, Color color, int offset)
	{
		Vector3 startPos = new Vector3(start.x + start.width + 16, start.y + start.height - offset - 5, 0);
		Vector3 endPos = new Vector3(end.x, end.y - 1, 0);
		
		float dist = Mathf.Abs (startPos.x - endPos.x);
		
		Vector3 startTan = startPos + Vector3.right * dist / 2;
		Vector3 endTan = endPos + Vector3.left * dist / 2;
		Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 3);
	}

	#endif


	public static void DuplicateActionsBuffer ()
	{
		List<AC.Action> tempList = new List<AC.Action>();
		foreach (AC.Action action in copiedActions)
		{
			AC.Action copyAction = Object.Instantiate (action) as AC.Action;
			copyAction.nodeRect = new Rect (0,0,300,60);
			tempList.Add (copyAction);
		}

		copiedActions.Clear ();
		copiedActions = tempList;
	}


	public static Vector3 GetScreenDirection (Vector3 originWorldPosition, Vector3 targetWorldPosition)
	{
		Vector3 originScreenPosition = Camera.main.WorldToScreenPoint (originWorldPosition);
		Vector3 targetScreenPosition = Camera.main.WorldToScreenPoint (targetWorldPosition);

		Vector3 lookVector = targetScreenPosition - originScreenPosition;
		lookVector.z = lookVector.y;
		lookVector.y = 0;

		return (lookVector);
	}


	public static Vector3 GetScreenNavMesh (Vector3 targetWorldPosition)
	{
		SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

		Vector3 targetScreenPosition = Camera.main.WorldToScreenPoint (targetWorldPosition);
		Ray ray = Camera.main.ScreenPointToRay (targetScreenPosition);
		RaycastHit hit = new RaycastHit();
		
		if (settingsManager && Physics.Raycast (ray, out hit, settingsManager.navMeshRaycastLength, 1 << LayerMask.NameToLayer (settingsManager.navMeshLayer)))
		{
			return hit.point;
		}

		return targetWorldPosition;
	}


	public static Vector2 GetMainGameViewSize ()
	{
		if (Application.isPlaying)
		{
			return new Vector2 (Screen.width, Screen.height);
		}
		
		System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
		System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod ("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
		System.Object Res = GetSizeOfMainGameView.Invoke (null,null);
		return (Vector2) Res;
	}
	
	
	public static Matrix4x4 SetVanishingPoint (Camera _camera, Vector2 perspectiveOffset)
	{
		Matrix4x4 m = _camera.projectionMatrix;
		float w = 2f * _camera.nearClipPlane / m.m00;
		float h = 2f * _camera.nearClipPlane / m.m11;
	 
		float left = -(w / 2) + perspectiveOffset.x;
		float right = left + w;
		float bottom = -(h / 2) + perspectiveOffset.y;
		float top = bottom + h;
	 
		return (PerspectiveOffCenter (left, right, bottom, top, _camera.nearClipPlane, _camera.farClipPlane));
	}


	private static Matrix4x4 PerspectiveOffCenter (float left, float right, float bottom, float top, float near, float far)
	{
		float x =  (2f * near) / (right - left);
		float y =  (2f * near) / (top - bottom);
		float a =  (right + left) / (right - left);
		float b =  (top + bottom) / (top - bottom);
		float c = -(far + near) / (far - near);
		float d = -(2f * far * near) / (far - near);
		float e = -1f;
	 
		Matrix4x4 m = new Matrix4x4();
		m[0,0] = x;		m[0,1] = 0f;	m[0,2] = a;		m[0,3] = 0f;
		m[1,0] = 0f;	m[1,1] = y;		m[1,2] = b;		m[1,3] = 0f;
		m[2,0] = 0f;	m[2,1] = 0f;	m[2,2] = c;		m[2,3] =   d;
		m[3,0] = 0f;	m[3,1] = 0f;	m[3,2] = e;		m[3,3] = 0f;
		return m;
	}
	
	
	public static string UniqueName (string name)
	{
		if (GameObject.Find (name))
		{
			string newName = name;
			
			for (int i=2; i<20; i++)
			{
				newName = name + i.ToString ();
				
				if (!GameObject.Find (newName))
				{
					break;
				}
			}
			
			return newName;
		}
		else
		{
			return name;
		}
	}
	
	
	public static string GetName (string resourceName)
	{
		int slash = resourceName.IndexOf ("/");
		string newName;
		
		if (slash > 0)
		{
			newName = resourceName.Remove (0, slash+1);
		}
		else
		{
			newName = resourceName;
		}
		
		return newName;
	}
	
	
	public static Rect GUIBox (float centre_x, float centre_y, float size)
	{
		Rect newRect;
		newRect = GUIRect (centre_x, centre_y, size, size);
		return (newRect);
	}
	
	
	public static Rect GUIRect (float centre_x, float centre_y, float width, float height)
	{
		Rect newRect;
		newRect = new Rect (Screen.width * centre_x - (Screen.width * width)/2, Screen.height * centre_y - (Screen.width * height)/2, Screen.width * width, Screen.width * height);
		return (newRect);
	}
	
	
	public static Rect GUIBox (Vector2 posVector, float size)
	{
		Rect newRect;
		newRect = GUIRect (posVector.x / Screen.width, (Screen.height - posVector.y) / Screen.height, size, size);
		return (newRect);
	}
	
	
	public static void AddAnimClip (Animation _animation, int layer, AnimationClip clip, AnimationBlendMode blendMode, WrapMode wrapMode, Transform mixingBone)
	{
		if (clip != null)
		{
			// Initialises a clip
			_animation.AddClip (clip, clip.name);
			
			if (mixingBone != null)
			{
				_animation [clip.name].AddMixingTransform (mixingBone);
			}
			
			// Set up the state
			_animation [clip.name].layer = layer;
			_animation [clip.name].normalizedTime = 0f;
			_animation [clip.name].blendMode = blendMode;
			_animation [clip.name].wrapMode = wrapMode;
			_animation [clip.name].enabled = true;
		}
	}


	public static void PlayAnimClipFrame (Animation _animation, int layer, AnimationClip clip, AnimationBlendMode blendMode, WrapMode wrapMode, float fadeTime, Transform mixingBone, float normalisedFrame)
	{
		// Initialises and plays the last frame of a clip
		
		if (clip != null)
		{
			AddAnimClip (_animation, layer, clip, blendMode, wrapMode, mixingBone);
			_animation [clip.name].normalizedTime = normalisedFrame;
			_animation [clip.name].speed *= 1f;
			_animation.Play (clip.name);
			CleanUnusedClips (_animation);
		}
	}
	
	
	public static void PlayAnimClip (Animation _animation, int layer, AnimationClip clip, AnimationBlendMode blendMode, WrapMode wrapMode, float fadeTime, Transform mixingBone, bool reverse)
	{
		// Initialises and crossfades a clip

		if (clip != null)
		{
			AddAnimClip (_animation, layer, clip, blendMode, wrapMode, mixingBone);
			if (reverse)
			{
				_animation[clip.name].speed *= -1f;
			}
			_animation.CrossFade (clip.name, fadeTime);
			CleanUnusedClips (_animation);
		}
	}


	public static Texture2D FindTextureResource (string textureName)
	{
		if (textureName == "")
		{
			return null;
		}
		
		Object[] objects = Resources.FindObjectsOfTypeAll (typeof (Texture2D));
		foreach (Object _object in objects)
		{
			if (_object.name == textureName && _object is Texture2D)
			{
				return (Texture2D) _object;
			}
		}
		
		return null;
	}


	public static AudioClip FindAudioClipResource (string clipName)
	{
		if (clipName == "")
		{
			return null;
		}
		
		Object[] objects = Resources.FindObjectsOfTypeAll (typeof (AudioClip));
		foreach (Object _object in objects)
		{
			if (_object.name == clipName && _object is AudioClip)
			{
				return (AudioClip) _object;
			}
		}
		
		return null;
	}


	public static AnimationClip FindAnimClipResource (string clipName)
	{
		if (clipName == "")
		{
			return null;
		}

		Object[] objects = Resources.FindObjectsOfTypeAll (typeof (AnimationClip));
		foreach (Object _object in objects)
		{
			if (_object.name == clipName && _object is AnimationClip)
			{
				return (AnimationClip) _object;
			}
		}

		return null;
	}

	
	public static void CleanUnusedClips (Animation _animation)
	{
		// Remove any non-playing animations
		
		List <string> removeClips = new List <string>();
		
		foreach (AnimationState state in _animation)
		{
			if (!_animation [state.name].enabled)
			{
				// Queued animations get " - Queued Clone" appended to it, so remove
				
				int queueIndex = state.name.IndexOf (" - Queued Clone");

				if (queueIndex > 0)
				{
					removeClips.Add (state.name.Substring (0, queueIndex));
				}
				else
				{
					removeClips.Add (state.name);
				}
			}
		}
		
		foreach (string _clip in removeClips)
		{
			_animation.RemoveClip (_clip);
		}
		
	}


	public static float Interpolate (float startT, float deltaT, MoveMethod moveMethod)
	{
		if (moveMethod == MoveMethod.Curved)
		{
			moveMethod = MoveMethod.Smooth;
		}

		else if (moveMethod == MoveMethod.Smooth)
		{
			return -0.5f * (Mathf.Cos (Mathf.PI * (Time.time - startT) / deltaT) - 1f);
		}
		else if (moveMethod == MoveMethod.EaseIn)
		{
			return 1f - Mathf.Cos ((Time.time - startT) / deltaT * (Mathf.PI / 2));
		}
		else if (moveMethod == MoveMethod.EaseOut)
		{
			return Mathf.Sin ((Time.time - startT) / deltaT * (Mathf.PI / 2));
		}

		return (Time.time - startT) / deltaT;
	}

	
	public static Rect Rescale (Rect _rect)
	{
		float ScaleFactor;
		ScaleFactor = Screen.width / 884.0f;
		int ScaleFactorInt = Mathf.RoundToInt(ScaleFactor);
		Rect newRect = new Rect (_rect.x * ScaleFactorInt, _rect.y * ScaleFactorInt, _rect.width * ScaleFactorInt, _rect.height * ScaleFactorInt);
		
		return (newRect);
	}
	
	
	public static int Rescale (int _int)
	{
		float ScaleFactor;
		ScaleFactor = Screen.width / 884.0f;
		int ScaleFactorInt = Mathf.RoundToInt(ScaleFactor);
		int returnValue;
		returnValue = _int * ScaleFactorInt;
		
		return (returnValue);
	}
	
	
	public static void DrawTextOutline (Rect rect, string text, GUIStyle style, Color outColor, Color inColor, float size)
	{
		float halfSize = size * 0.5F;
		GUIStyle backupStyle = new GUIStyle(style);
		Color backupColor = GUI.color;
		
		outColor.a = GUI.color.a;
		style.normal.textColor = outColor;
		GUI.color = outColor;

		rect.x -= halfSize;
		GUI.Label(rect, text, style);

		rect.x += size;
		GUI.Label(rect, text, style);

		rect.x -= halfSize;
		rect.y -= halfSize;
		GUI.Label(rect, text, style);

		rect.y += size;
		GUI.Label(rect, text, style);

		rect.y -= halfSize;
		style.normal.textColor = inColor;
		GUI.color = backupColor;
		GUI.Label(rect, text, style);

		style = backupStyle;
	}
	
}	