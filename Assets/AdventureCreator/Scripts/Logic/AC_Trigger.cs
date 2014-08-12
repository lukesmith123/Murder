/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"Trigger.cs"
 * 
 *	This ActionList runs when the Player enters it.
 * 
 */

using UnityEngine;
using System.Collections;
using AC;

[System.Serializable]
public class AC_Trigger : ActionList
{
	
	public int triggerType;
	public bool showInEditor = false;
	public bool cancelInteractions = false;


	public override void Interact ()
	{
		if (cancelInteractions)
		{
			PlayerInteraction playerInteraction = GameObject.FindWithTag (Tags.gameEngine).GetComponent <PlayerInteraction>();
			playerInteraction.StopInteraction ();
		}

		base.Interact ();
	}
	
	
	private void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag (Tags.player) && stateHandler && stateHandler.gameState == GameState.Normal && triggerType == 0)
		{
			if (stateHandler.triggerIsOff)
			{
				return;
			}

			Interact ();
		}
	}


	private void OnTriggerEnter2D (Collider2D other)
	{
		Debug.Log (other.gameObject.name);
		if (other.CompareTag (Tags.player) && stateHandler && stateHandler.gameState == GameState.Normal && triggerType == 0)
		{
			if (stateHandler.triggerIsOff)
			{
				return;
			}
			
			Interact ();
		}
	}

	
	private void OnTriggerStay (Collider other)
	{
		if (other.CompareTag (Tags.player) && stateHandler && stateHandler.gameState == GameState.Normal && triggerType == 1)
		{
			if (stateHandler.triggerIsOff)
			{
				return;
			}

			Interact ();
		}
	}


	private void OnTriggerStay2D (Collider2D other)
	{
		if (other.CompareTag (Tags.player) && stateHandler && stateHandler.gameState == GameState.Normal && triggerType == 1)
		{
			if (stateHandler.triggerIsOff)
			{
				return;
			}
			
			Interact ();
		}
	}


	private void OnTriggerExit (Collider other)
	{
		if (other.CompareTag (Tags.player) && stateHandler && stateHandler.gameState == GameState.Normal && triggerType == 2)
		{
			Interact ();
		}
	}


	private void OnTriggerExit2D (Collider2D other)
	{
		if (other.CompareTag (Tags.player) && stateHandler && stateHandler.gameState == GameState.Normal && triggerType == 2)
		{
			Interact ();
		}
	}


	private void TurnOn ()
	{
		if (collider)
		{
			collider.enabled = true;
		}
		else if (collider2D)
		{
			collider2D.enabled = true;
		}
		else
		{
			Debug.LogWarning ("Cannot turn " + this.name + " on because it has no collider component.");
		}
	}
	
	
	private void TurnOff ()
	{
		if (collider)
		{
			collider.enabled = false;
		}
		else if (collider2D)
		{
			collider2D.enabled = false;
		}
		else
		{
			Debug.LogWarning ("Cannot turn " + this.name + " off because it has no collider component.");
		}
	}
	
	
	private void OnDrawGizmos ()
	{
		if (showInEditor)
		{
			DrawGizmos ();
		}
	}
	
	
	private void OnDrawGizmosSelected ()
	{
		DrawGizmos ();
	}
	
	
	private void DrawGizmos ()
	{
		AdvGame.DrawCubeCollider (transform, new Color (1f, 0.3f, 0f, 0.8f));
	}
	
}
