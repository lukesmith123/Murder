/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"RememberTransform.cs"
 * 
 *	This script is attached to gameObjects in the scene
 *	with transform data we wish to save.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

public class RememberTransform : ConstantID
{

	public bool saveParent;


	public TransformData SaveData ()
	{
		TransformData transformData = new TransformData();
		
		transformData.objectID = constantID;
		
		transformData.LocX = transform.position.x;
		transformData.LocY = transform.position.y;
		transformData.LocZ = transform.position.z;
		
		transformData.RotX = transform.eulerAngles.x;
		transformData.RotY = transform.eulerAngles.y;
		transformData.RotZ = transform.eulerAngles.z;
		
		transformData.ScaleX = transform.localScale.x;
		transformData.ScaleY = transform.localScale.y;
		transformData.ScaleZ = transform.localScale.z;

		if (saveParent)
		{
			// Attempt to find the "hand" bone of a character
			Transform t = transform.parent;
			while (t.parent != null)
			{
				t = t.parent;

				if (t.GetComponent <AC.Char>())
				{
					AC.Char parentCharacter = t.GetComponent <AC.Char>();
					
					if (parentCharacter is Player || (parentCharacter.GetComponent <ConstantID>() && parentCharacter.GetComponent <ConstantID>().constantID != 0))
					{
						if (transform.parent == parentCharacter.leftHandBone || transform.parent == parentCharacter.rightHandBone)
						{
							if (parentCharacter is Player)
							{
								transformData.parentIsPlayer = true;
								transformData.parentIsNPC = false;
								transformData.parentID = 0;
							}
							else
							{
								transformData.parentIsPlayer = false;
								transformData.parentIsNPC = true;
								transformData.parentID = parentCharacter.GetComponent <ConstantID>().constantID;
							}
							
							if (transform.parent == parentCharacter.leftHandBone)
							{
								transformData.heldHand = ActionCharHold.Hand.Left;
							}
							else
							{
								transformData.heldHand = ActionCharHold.Hand.Right;
							}
							
							return transformData;
						}
					}
					
					break;
				}
			}

			if (transform.parent.GetComponent <ConstantID>() && transform.parent.GetComponent <ConstantID>().constantID != 0)
			{
				transformData.parentID = transform.parent.GetComponent <ConstantID>().constantID;
			}
			else
			{
				transformData.parentID = 0;
				Debug.LogWarning ("Could not save " + this.name + "'s parent since it has no Constant ID");
			}
		}

		return transformData;
	}


	public void LoadData (TransformData data)
	{
		if (data.parentIsPlayer)
		{
			Player player = GameObject.FindWithTag (Tags.player).GetComponent <Player>();
			if (player != null)
			{
				if (data.heldHand == ActionCharHold.Hand.Left)
				{
					transform.parent = player.leftHandBone;
				}
				else
				{
					transform.parent = player.rightHandBone;
				}
			}
		}
		else if (data.parentID != 0)
		{
			ConstantID parentObject = Serializer.returnComponent <ConstantID> (data.parentID);

			if (parentObject != null)
			{
				if (data.parentIsNPC && parentObject.GetComponent <NPC>())
				{
					if (data.heldHand == ActionCharHold.Hand.Left)
					{
						transform.parent = parentObject.GetComponent <NPC>().leftHandBone;
					}
					else
					{
						transform.parent = parentObject.GetComponent <NPC>().rightHandBone;
					}
				}
				else
				{
					transform.parent = parentObject.gameObject.transform;
				}
			}
		}
		
		transform.position = new Vector3 (data.LocX, data.LocY, data.LocZ);
		transform.eulerAngles = new Vector3 (data.RotX, data.RotY, data.RotZ);
		transform.localScale = new Vector3 (data.ScaleX, data.ScaleY, data.ScaleZ);
	}

}


[System.Serializable]
public class TransformData
{
	
	public int objectID;
	
	public float LocX;
	public float LocY;
	public float LocZ;
	
	public float RotX;
	public float RotY;
	public float RotZ;
	
	public float ScaleX;
	public float ScaleY;
	public float ScaleZ;

	public int parentID;
	public bool parentIsNPC = false;
	public bool parentIsPlayer = false;
	public ActionCharHold.Hand heldHand;

	public TransformData () { }
	
}