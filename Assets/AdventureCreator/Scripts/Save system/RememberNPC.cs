/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberNPC.cs"
 * 
 *	This script is attached to NPCs in the scene
 *	with path and transform data we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class RememberNPC : ConstantID
{

	public AC_OnOff startState = AC_OnOff.On;

	
	public void Awake ()
	{
		SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
		
		if (settingsManager && GetComponent <RememberHotspot>() == null && GameIsPlaying ())
		{
			if (startState == AC_OnOff.On)
			{
				this.gameObject.layer = LayerMask.NameToLayer (settingsManager.hotspotLayer);
			}
			else
			{
				this.gameObject.layer = LayerMask.NameToLayer (settingsManager.deactivatedLayer);
			}
		}
	}


	public NPCData SaveData ()
	{
		NPCData npcData = new NPCData();
		
		npcData.objectID = constantID;
		
		if (gameObject.layer == LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer))
		{
			npcData.isOn = true;
		}
		else
		{
			npcData.isOn = false;
		}
		
		npcData.LocX = transform.position.x;
		npcData.LocY = transform.position.y;
		npcData.LocZ = transform.position.z;
		
		npcData.RotX = transform.eulerAngles.x;
		npcData.RotY = transform.eulerAngles.y;
		npcData.RotZ = transform.eulerAngles.z;
		
		npcData.ScaleX = transform.localScale.x;
		npcData.ScaleY = transform.localScale.y;
		npcData.ScaleZ = transform.localScale.z;
		
		if (GetComponent <NPC>())
		{
			NPC npc = GetComponent <NPC>();

			if (npc.animationEngine == AnimationEngine.Sprites2DToolkit || npc.animationEngine == AnimationEngine.SpritesUnity)
			{
				npcData.idleAnim = npc.idleAnimSprite;
				npcData.walkAnim = npc.walkAnimSprite;
				npcData.talkAnim = npc.talkAnimSprite;
				npcData.runAnim = npc.runAnimSprite;
			}
			else if (npc.animationEngine == AnimationEngine.Legacy)
			{
				npcData.idleAnim = npc.GetStandardAnimClipName (AnimStandard.Idle);
				npcData.walkAnim = npc.GetStandardAnimClipName (AnimStandard.Walk);
				npcData.runAnim = npc.GetStandardAnimClipName (AnimStandard.Run);
				npcData.talkAnim = npc.GetStandardAnimClipName (AnimStandard.Talk);
			}

			npcData.walkSound = npc.GetStandardSoundName (AnimStandard.Walk);
			npcData.runSound = npc.GetStandardSoundName (AnimStandard.Run);
			if (npc.portraitGraphic)
			{
				npcData.portraitGraphic = npc.portraitGraphic.name;
			}
			else
			{
				npcData.portraitGraphic = "";
			}
			
			npcData.walkSpeed = npc.walkSpeedScale;
			npcData.runSpeed = npc.runSpeedScale;

			// Rendering
			npcData.lockDirection = npc.lockDirection;
			npcData.lockScale = npc.lockScale;
			if (npc.spriteChild && npc.spriteChild.GetComponent <FollowSortingMap>())
			{
				npcData.lockSorting = npc.spriteChild.GetComponent <FollowSortingMap>().lockSorting;
			}
			else if (npc.GetComponent <FollowSortingMap>())
			{
				npcData.lockSorting = npc.GetComponent <FollowSortingMap>().lockSorting;
			}
			else
			{
				npcData.lockSorting = false;
			}
			npcData.spriteDirection = npc.spriteDirection;
			npcData.spriteScale = npc.spriteScale;
			if (npc.spriteChild && npc.spriteChild.renderer)
			{
				npcData.sortingOrder = npc.spriteChild.renderer.sortingOrder;
				npcData.sortingLayer = npc.spriteChild.renderer.sortingLayerName;
			}
			else if (npc.renderer)
			{
				npcData.sortingOrder = npc.renderer.sortingOrder;
				npcData.sortingLayer = npc.renderer.sortingLayerName;
			}
			
			if (npc.GetPath ())
			{
				npcData.targetNode = npc.GetTargetNode ();
				npcData.prevNode = npc.GetPrevNode ();
				npcData.isRunning = npc.isRunning;
				
				if (npc.GetPath () == GetComponent <Paths>())
				{
					npcData.pathData = Serializer.CreatePathData (GetComponent <Paths>());
					npcData.pathID = 0;
				}
				else
				{
					if (npc.GetPath ().GetComponent <ConstantID>())
					{
						npcData.pathID = npc.GetPath ().GetComponent <ConstantID>().constantID;
					}
					else
					{
						Debug.LogWarning ("Want to save path data for " + name + " but path has no ID!");
					}
				}
			}
	
			if (npc.followTarget)
			{
				if (!npc.followTargetIsPlayer)
				{
					if (npc.followTarget.GetComponent <ConstantID>())
					{
						npcData.followTargetID = npc.followTarget.GetComponent <ConstantID>().constantID;
						npcData.followTargetIsPlayer = npc.followTargetIsPlayer;
						npcData.followFrequency = npc.followFrequency;
						npcData.followDistance = npc.followDistance;
						npcData.followDistanceMax= npc.followDistanceMax;
					}
					else
					{
						Debug.LogWarning ("Want to save follow data for " + name + " but " + npc.followTarget.name + " has no ID!");
					}
				}
				else
				{
					npcData.followTargetID = 0;
					npcData.followTargetIsPlayer = npc.followTargetIsPlayer;
					npcData.followFrequency = npc.followFrequency;
					npcData.followDistance = npc.followDistance;
					npcData.followDistanceMax = npc.followDistanceMax;
				}
			}
			else
			{
				npcData.followTargetID = 0;
				npcData.followTargetIsPlayer = false;
				npcData.followFrequency = 0f;
				npcData.followDistance = 0f;
				npcData.followDistanceMax = 0f;
			}
		}
		
		return npcData;
	}


	public void LoadData (NPCData data)
	{
		if (data.isOn)
		{
			gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.hotspotLayer);
		}
		else
		{
			gameObject.layer = LayerMask.NameToLayer (AdvGame.GetReferences ().settingsManager.deactivatedLayer);
		}
		
		transform.position = new Vector3 (data.LocX, data.LocY, data.LocZ);
		transform.eulerAngles = new Vector3 (data.RotX, data.RotY, data.RotZ);
		transform.localScale = new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ);
		
		if (GetComponent <NPC>())
		{
			NPC npc = GetComponent <NPC>();

			npc.EndPath ();
			
			if (npc.animationEngine == AnimationEngine.Sprites2DToolkit || npc.animationEngine == AnimationEngine.SpritesUnity)
			{
				npc.idleAnimSprite = data.idleAnim;
				npc.walkAnimSprite = data.walkAnim;
				npc.talkAnimSprite = data.talkAnim;
				npc.runAnimSprite = data.runAnim;
			}
			else if (npc.animationEngine == AnimationEngine.Legacy)
			{
				npc.AssignStandardAnimClipFromResource (AnimStandard.Idle, data.idleAnim);
				npc.AssignStandardAnimClipFromResource (AnimStandard.Walk, data.walkAnim);
				npc.AssignStandardAnimClipFromResource (AnimStandard.Talk, data.talkAnim);
				npc.AssignStandardAnimClipFromResource (AnimStandard.Run, data.runAnim);
			}

			npc.AssignStandardSoundFromResource (AnimStandard.Walk, data.walkSound);
			npc.AssignStandardSoundFromResource (AnimStandard.Run, data.runSound);
			npc.AssignPortraitGraphicFromResource (data.portraitGraphic);
			
			npc.walkSpeedScale = data.walkSpeed;
			npc.runSpeedScale = data.runSpeed;

			// Rendering
			npc.lockDirection = data.lockDirection;
			npc.lockScale = data.lockScale;
			if (npc.spriteChild && npc.spriteChild.GetComponent <FollowSortingMap>())
			{
				npc.spriteChild.GetComponent <FollowSortingMap>().lockSorting = data.lockSorting;
			}
			else if (npc.GetComponent <FollowSortingMap>())
			{
				npc.GetComponent <FollowSortingMap>().lockSorting = data.lockSorting;
			}
			else
			{
				npc.ReleaseSorting ();
			}
			
			if (data.lockDirection)
			{
				npc.spriteDirection = data.spriteDirection;
			}
			if (data.lockScale)
			{
				npc.spriteScale = data.spriteScale;
			}
			if (data.lockSorting)
			{
				if (npc.spriteChild && npc.spriteChild.renderer)
				{
					npc.spriteChild.renderer.sortingOrder = data.sortingOrder;
					npc.spriteChild.renderer.sortingLayerName = data.sortingLayer;
				}
				else if (npc.renderer)
				{
					npc.renderer.sortingOrder = data.sortingOrder;
					npc.renderer.sortingLayerName = data.sortingLayer;
				}
			}
		
			AC.Char charToFollow = null;
			if (data.followTargetID != 0)
			{
				RememberNPC followNPC = Serializer.returnComponent <RememberNPC> (data.followTargetID);
				if (followNPC.GetComponent <AC.Char>())
				{
					charToFollow = followNPC.GetComponent <AC.Char>();
				}
			}
			
			npc.FollowAssign (charToFollow, data.followTargetIsPlayer, data.followFrequency, data.followDistance, data.followDistanceMax);
			npc.Halt ();
			
			if (data.pathData != null && data.pathData != "" && GetComponent <Paths>())
			{
				Paths savedPath = GetComponent <Paths>();
				savedPath = Serializer.RestorePathData (savedPath, data.pathData);
				npc.SetPath (savedPath, data.targetNode, data.prevNode);
				npc.isRunning = data.isRunning;
			}
			else if (data.pathID != 0)
			{
				Paths pathObject = Serializer.returnComponent <Paths> (data.pathID);
				
				if (pathObject != null)
				{
					npc.SetPath (pathObject, data.targetNode, data.prevNode);
				}
				else
				{
					Debug.LogWarning ("Trying to assign a path for NPC " + this.name + ", but the path was not found - was it deleted?");
				}
			}
		}
	}

}


[System.Serializable]
public class NPCData
{
	public int objectID;
	
	public bool isOn;
	
	public float LocX;
	public float LocY;
	public float LocZ;
	
	public float RotX;
	public float RotY;
	public float RotZ;
	
	public float ScaleX;
	public float ScaleY;
	public float ScaleZ;
	
	public string idleAnim;
	public string walkAnim;
	public string talkAnim;
	public string runAnim;

	public string walkSound;
	public string runSound;
	public string portraitGraphic;

	public float walkSpeed;
	public float runSpeed;

	public bool lockDirection;
	public string spriteDirection;
	public bool lockScale;
	public float spriteScale;
	public bool lockSorting;
	public int sortingOrder;
	public string sortingLayer;
	
	public int pathID;
	public int targetNode;
	public int prevNode;
	public string pathData;
	public bool isRunning;
	
	public int followTargetID = 0;
	public bool followTargetIsPlayer = false;
	public float followFrequency = 0f;
	public float followDistance = 0f;
	public float followDistanceMax = 0f;
	
	public NPCData () { }
}