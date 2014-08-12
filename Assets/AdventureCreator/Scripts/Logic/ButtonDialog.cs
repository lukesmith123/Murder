/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ButtonDialog.cs"
 * 
 *	This script is a container class for dialogue options
 *	that are linked to Conversations.
 * 
 */

using UnityEngine;
using AC;

[System.Serializable]
public class ButtonDialog
{
	
	public string label = "(Not set)";
	public Texture2D icon;
	public bool isOn;
	public bool isLocked;
	public ConversationAction conversationAction;
	public Conversation newConversation;
	public int lineID = -1;
	public bool isEditing = false;
	
	public DialogueOption dialogueOption;
	
	public ButtonDialog ()
	{
		label = "";
		icon = null;
		isOn = true;
		isLocked = false;
		conversationAction = ConversationAction.ReturnToConveration;
		newConversation = null;
		dialogueOption = null;
		lineID = -1;
		isEditing = false;
	}

}
