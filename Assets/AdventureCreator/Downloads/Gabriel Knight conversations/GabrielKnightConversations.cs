using UnityEngine;

namespace AC
{

	[RequireComponent (typeof (Conversation))]
	public class GabrielKnightConversations : MonoBehaviour
	{

		#region Variables

		[SerializeField] private NPC npcSpeakingTo;
		[SerializeField] private string conversationMenuName = "GK_Conversation";
		[SerializeField] private string[] menusToLock = new string[2] { "InGame", "Subtitles" };

		private Menu conversationMenu;
		private MenuDialogList dialogList;
		private MenuLabel playerSubs;
		private MenuLabel npcSubs;
		private MenuGraphic playerPortrait;
		private MenuGraphic npcPortrait;

		#endregion


		#region UnityStandards

		private void OnEnable ()
		{
			AssignMenuVars ();

			EventManager.OnBeginActionList += OnBeginActionList;
			EventManager.OnEnterGameState += OnEnterGameState;
			EventManager.OnStartSpeech_Alt += OnStartSpeech_Alt;
			EventManager.OnStopSpeech += OnStopSpeech;
			EventManager.OnStartConversation += OnStartConversation;
			EventManager.OnEndConversation += OnEndConversation;
			EventManager.OnClickConversation += OnClickConversation;
		}


		private void OnDisable ()
		{
			EventManager.OnBeginActionList -= OnBeginActionList;
			EventManager.OnEnterGameState -= OnEnterGameState;
			EventManager.OnStartSpeech_Alt -= OnStartSpeech_Alt;
			EventManager.OnStopSpeech -= OnStopSpeech;
			EventManager.OnStartConversation -= OnStartConversation;
			EventManager.OnEndConversation -= OnEndConversation;
			EventManager.OnClickConversation -= OnClickConversation;
		}

		#endregion


		#region EventHooks

		private void OnBeginActionList (ActionList actionList, ActionListAsset actionListAsset, int startingIndex, bool isSkipping)
		{
			if (ActionListReferencesConversation (actionList))
			{
				BeginDialogueMode ();
			}
		}


		private void OnEnterGameState (GameState gameState)
		{
			if (gameState == GameState.Normal)
			{
				EndDialogueMode ();
			}
		}


		private void OnStartSpeech_Alt (Speech speech)
		{
			if (speech.GetSpeakingCharacter () == KickStarter.player)
			{
				playerSubs.SetSpeech (speech);
			}
			else if (speech.GetSpeakingCharacter () == npcSpeakingTo)
			{
				npcSubs.SetSpeech (speech);
			}
		}


		private void OnStopSpeech (Char speakingCharacter)
		{
			if (speakingCharacter == KickStarter.player)
			{
				playerSubs.SetSpeech (null);
			}
			else if (speakingCharacter == npcSpeakingTo)
			{
				npcSubs.SetSpeech (null);
			}
		}


		private void OnClickConversation (Conversation conversation, int optionID)
		{
			dialogList.IsVisible = false;
		}


		private void OnStartConversation (Conversation conversation)
		{
			dialogList.IsVisible = true;
		}


		private void OnEndConversation (Conversation conversation)
		{
			dialogList.IsVisible = true;
		}

		#endregion


		#region PrivateFunctions

		private void AssignMenuVars ()
		{
			conversationMenu = PlayerMenus.GetMenuWithName (conversationMenuName);
			dialogList = conversationMenu.GetElementWithName ("DialogueList") as MenuDialogList;

			playerSubs = conversationMenu.GetElementWithName ("PlayerSubs") as MenuLabel;
			playerSubs.SetSpeech (null);
			npcSubs = conversationMenu.GetElementWithName ("NPCSubs") as MenuLabel;
			npcSubs.SetSpeech (null);

			playerPortrait = conversationMenu.GetElementWithName ("PlayerPortrait") as MenuGraphic;
			npcPortrait = conversationMenu.GetElementWithName ("NPCPortrait") as MenuGraphic;
		}


		private void BeginDialogueMode ()
		{
			playerPortrait.PortraitCharacterOverride = KickStarter.player;
			npcPortrait.PortraitCharacterOverride = npcSpeakingTo;

			conversationMenu.isLocked = false;

			foreach (string menuToLock in menusToLock)
			{
				Menu menu = PlayerMenus.GetMenuWithName (menuToLock);
				if (menu != null) menu.isLocked = true;
			}
		}


		private void EndDialogueMode ()
		{
			if (!conversationMenu.isLocked)
			{
				conversationMenu.isLocked = true;

				foreach (string menuToLock in menusToLock)
				{
					Menu menu = PlayerMenus.GetMenuWithName (menuToLock);
					if (menu != null) menu.isLocked = false;
				}
			}
		}


		private bool ActionListReferencesConversation (ActionList actionList)
		{
			foreach (Action action in actionList.actions)
			{
				ActionConversation actionConversation = action as ActionConversation;
				if (actionConversation != null && actionConversation.conversation == GetComponent <Conversation>())
				{
					return true;
				}
			}
			return false;
		}

		#endregion
		
	}

}