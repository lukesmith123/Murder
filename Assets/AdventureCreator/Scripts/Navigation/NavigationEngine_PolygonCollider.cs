/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"NavigationEngine_PolygonCollider.cs"
 * 
 *	This script uses a Polygon collider 2D to
 *	allow pathfinding in a scene.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AC;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NavigationEngine_PolygonCollider : NavigationEngine
{
	
	private bool pathFailed = false;
	private int originalLayer;
	
	private SettingsManager settingsManager;
	private SceneSettings sceneSettings;
	
	
	public override void Awake ()
	{
		GetReferences ();
		sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
	}


	public override Vector3[] GetPointsArray (Vector3 originPos, Vector3 targetPos)
	{
		Vector2[] pointsList2D = GetPointsArray (new Vector2 (originPos.x, originPos.y), new Vector2 (targetPos.x, targetPos.y));
		List <Vector3> pointsList3D = new List<Vector3>();

		foreach (Vector2 point2D in pointsList2D)
		{
			pointsList3D.Add (new Vector3 (point2D.x, point2D.y, originPos.z));
		}

		return pointsList3D.ToArray ();
	}
	
	
	public Vector2[] GetPointsArray (Vector2 originPos, Vector2 targetPos)
	{
		List <Vector2> pointsList = new List<Vector2>();
		
		if (sceneSettings && sceneSettings.navMesh && sceneSettings.navMesh.collider2D)
		{
			Vector2 originalOriginPos = originPos;
			Vector2 originalTargetPos = targetPos;
			originPos = GetNearestToMesh (originPos);
			targetPos = GetNearestToMesh (targetPos);
			
			pointsList.Add (originPos);
			
			if (!IsLineClear (targetPos, originPos, false))
			{
				pointsList = FindComplexPath (originPos, targetPos, false);

				if (pathFailed)
				{
					Vector2 newTargetPos = GetLineBreak (pointsList [pointsList.Count - 1], targetPos);
					
					if (newTargetPos != Vector2.zero)
					{
						targetPos = newTargetPos;
						
						if (!IsLineClear (targetPos, originPos, true))
						{
							pointsList = FindComplexPath (originPos, targetPos, true);
							
							if (pathFailed)
							{
								// Couldn't find an alternative, so just clear the path
								pointsList.Clear ();
								pointsList.Add (originPos);
							}
						}
						else
						{
							// Line between origin and new target is clear
							pointsList.Clear ();
							pointsList.Add (originPos);
						}
					}
				}
			}
			
			// Finally, remove any extraneous points
			if (pointsList.Count > 2)
			{
				for (int i=0; i<pointsList.Count; i++)
				{
					for (int j=i; j<pointsList.Count; j++)
					{
						if (IsLineClear (pointsList[i], pointsList[j], false) && j > i+1)
						{
							// Point i+1 is irrelevant, remove and reset
							pointsList.RemoveRange (i+1, j-i-1);
							j=0;
							i=0;
						}
					}
				}
			}
			pointsList.Add (targetPos);
			
			if (pointsList[0] == originalOriginPos)
				pointsList.RemoveAt (0);	// Remove origin point from start
			
			// Special case where player is stuck on a collider above the mesh
			if (pointsList.Count == 1 && pointsList[0] == originPos)
			{
				pointsList[0] = originalTargetPos;
			}
		}
		else
		{
			// Special case: no collider, no path
			pointsList.Add (targetPos);
		}

		return pointsList.ToArray ();
	}
	
	
	public override string GetPrefabName ()
	{
		return ("NavMesh2D");
	}
	
	
	public override void SetVisibility (bool visibility)
	{
		#if UNITY_EDITOR
		NavigationMesh[] navMeshes = FindObjectsOfType (typeof (NavigationMesh)) as NavigationMesh[];
		Undo.RecordObjects (navMeshes, "Navigation visibility");
		
		foreach (NavigationMesh navMesh in navMeshes)
		{
			navMesh.showInEditor = visibility;
			EditorUtility.SetDirty (navMesh);
		}
		#endif
	}
	
	
	public override void SceneSettingsGUI ()
	{
		#if UNITY_EDITOR
		GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>().navMesh = (NavigationMesh) EditorGUILayout.ObjectField ("Default NavMesh:", GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>().navMesh, typeof (NavigationMesh), true);
		if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager && !AdvGame.GetReferences ().settingsManager.IsUnity2D ())
		{
			EditorGUILayout.HelpBox ("This method is only compatible with 'Unity 2D' mode.", MessageType.Warning);
		}
		#endif
	}
	
	
	private bool IsVertexImperfect (Vector2 vertex, Vector2[] blackList)
	{
		bool answer = false;
		
		foreach (Vector2 candidate in blackList)
		{
			if (vertex == candidate)
				answer = true;
		}
		
		return answer;
	}
	
	
	private float GetPathLength (List <Vector2> _pointsList, Vector2 candidatePoint, Vector2 endPos)
	{
		float length = 0f;
		
		List <Vector2> newPath = new List<Vector2>();
		foreach (Vector2 point in _pointsList)
		{
			newPath.Add (point);
		}
		newPath.Add (candidatePoint);
		newPath.Add (endPos);
		
		for (int i=1; i<newPath.Count; i++)
		{
			length += Vector2.Distance (newPath[i], newPath[i-1]);
		}
		
		return (length);
	}
	
	
	private bool IsLineClear (Vector2 startPos, Vector2 endPos, bool ignoreOthers)
	{
		if (settingsManager == null)
		{
			return true;
		}

		Vector2 actualPos = startPos;
		
		for (float i=0f; i<1f; i+= 0.01f)
		{
			actualPos = startPos + ((endPos - startPos) * i);

			RaycastHit2D hit = Physics2D.Raycast (actualPos + new Vector2 (-0.005f, 0f), new Vector2 (1f, 0), 0.01f, 1 << sceneSettings.navMesh.gameObject.layer);
			if (hit)
			{
				if (hit.collider.gameObject != sceneSettings.navMesh.gameObject && !ignoreOthers)
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}
		return true;
	}
	
	
	private Vector2 GetLineBreak (Vector2 startPos, Vector2 endPos)
	{
		if (settingsManager == null)
		{
			return Vector2.zero;
		}

		Vector2 actualPos = startPos;

		for (float i=0f; i<1f; i+= 0.01f)
		{
			actualPos = startPos + ((endPos - startPos) * i);

			RaycastHit2D hit = Physics2D.Raycast (actualPos, endPos - actualPos, 0.01f, 1 << sceneSettings.navMesh.gameObject.layer);
			if (hit && hit.collider.gameObject != sceneSettings.navMesh.gameObject)
			{
				return actualPos;
			}
		}
		return Vector2.zero;
	}
	
	
	private Vector2[] CreateVertexArray (Vector2 targetPos)
	{
		List <Vector2> vertexData = new List<Vector2>();
		PolygonCollider2D poly = sceneSettings.navMesh.transform.GetComponent <PolygonCollider2D>();

		for (int i=0; i<poly.pathCount; i++)
		{
			Vector2[] _vertices = poly.GetPath (i);
			
			List<NavMeshData2D> navMeshData2D = new List<NavMeshData2D>();
			foreach (Vector2 vertex in _vertices)
			{
				navMeshData2D.Add (new NavMeshData2D (vertex, targetPos, sceneSettings.navMesh.transform));
			}
			
			navMeshData2D.Sort (delegate (NavMeshData2D a, NavMeshData2D b) {return a.distance.CompareTo (b.distance);});
			
			foreach (NavMeshData2D data in navMeshData2D)
			{
				vertexData.Add (data.vertex);
			}
		}
		return (vertexData.ToArray ());
	}
	
	
	private List<Vector2> FindComplexPath (Vector2 originPos, Vector2 targetPos, bool ignoreOthers)
	{
		targetPos = GetNearestToMesh (targetPos);

		pathFailed = false;
		List <Vector2> pointsList = new List<Vector2>();
		pointsList.Add (originPos);
		
		// Find nearest vertex to targetPos that originPos can also "see"
		bool pathFound = false;
		
		// An array of the navMesh's vertices, in order of distance from the target position
		Vector2[] vertices = CreateVertexArray (targetPos);

		int j=0;
		float pathLength = 0f;
		bool foundCandidate = false;
		bool foundCandidateForBoth = false;
		Vector2 candidatePoint = Vector2.zero;
		List<Vector2> imperfectCandidates = new List<Vector2>();
		
		while (!pathFound)
		{
			pathLength = 0f;
			foundCandidate = false;
			foundCandidateForBoth = false;
			
			foreach (Vector2 vertex in vertices)
			{
				if (!IsVertexImperfect (vertex, imperfectCandidates.ToArray ()))
				{
					if (IsLineClear (vertex, pointsList [pointsList.Count - 1], ignoreOthers))
					{
						// Do we now have a clear path?
						if (IsLineClear (targetPos, vertex, ignoreOthers))
						{
							if (!foundCandidateForBoth)
							{
								// Test a new candidate
								float testPathLength = GetPathLength (pointsList, vertex, targetPos);
								
								if (testPathLength < pathLength || !foundCandidate)
								{
									foundCandidate = true;
									foundCandidateForBoth = true;
									candidatePoint = vertex;
									pathLength = testPathLength;
								}
							}
							else
							{
								// Test a new candidate
								float testPathLength = GetPathLength (pointsList, vertex, targetPos);
								
								if (testPathLength < pathLength)
								{
									candidatePoint = vertex;
									pathLength = testPathLength;
								}
							}
						}
						else if (!foundCandidateForBoth)
						{
							if (!foundCandidate)
							{
								candidatePoint = vertex;
								foundCandidate = true;
								pathLength = GetPathLength (pointsList, vertex, targetPos);
							}
							else
							{
								// Test a new candidate
								float testPathLength = GetPathLength (pointsList, vertex, targetPos);
								
								if (testPathLength < pathLength)
								{
									candidatePoint = vertex;
									pathLength = testPathLength;
								}
							}
						}
					}
				}
			}
			
			if (foundCandidate)
			{
				pointsList.Add (candidatePoint);
				
				if (foundCandidateForBoth)
				{
					pathFound = true;
				}
				else
				{
					imperfectCandidates.Add (candidatePoint);
				}
			}
			
			j++;
			if (j > vertices.Length)
			{
				pathFailed = true;
				return pointsList;
			}
		}
		
		return pointsList;
	}
	
	
	private Vector2 GetNearestToMesh (Vector2 point)
	{
		// Test to make sure starting on the collision mesh
		if (settingsManager && !Physics2D.Raycast (point - new Vector2 (0.005f, 0f), new Vector2 (1f, 0f), 0.01f, 1 << sceneSettings.navMesh.gameObject.layer))
		{
			Vector2[] vertices = CreateVertexArray (point);
			return vertices[0];
		}
		return (point);	
	}
	
	
	private void GetReferences ()
	{
		settingsManager = AdvGame.GetReferences ().settingsManager;
		
		if (GameObject.FindWithTag (Tags.gameEngine) && GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>())
		{
			sceneSettings = GameObject.FindWithTag (Tags.gameEngine).GetComponent <SceneSettings>();
		}
	}
	
	private void OnDestroy ()
	{
		settingsManager = null;
		sceneSettings = null;
	}
	
}


public class NavMeshData2D
{
	
	public Vector2 vertex;
	public float distance;
	
	
	public NavMeshData2D (Vector2 _vertex, Vector2 _target, Transform navObject)
	{
		Vector3 vertex3D = navObject.TransformPoint (new Vector3 (_vertex.x, _vertex.y, navObject.position.z));
		vertex = new Vector2 (vertex3D.x, vertex3D.y);
		distance = Vector2.Distance (vertex, _target);
	}
	
}