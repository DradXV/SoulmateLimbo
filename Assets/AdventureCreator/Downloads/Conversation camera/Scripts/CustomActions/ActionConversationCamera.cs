using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionConversationCamera : Action
	{

		[SerializeField] private CharacterData characterDataA = new CharacterData ();
		[SerializeField] private CharacterData characterDataB = new CharacterData ();

		[SerializeField] private StopStart stopStart = StopStart.Start;
		private enum StopStart { Stop, Start };


		public ActionConversationCamera ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Camera;
			title = "Conversation";
			description = "Configures the state of a conversation camera sequcence between two characters";
		}


		public override float Run ()
		{
			ConversationCamera conversationCamera = ConversationCamera.Instance;
			if (conversationCamera == null)
			{
				LogWarning ("Cannot find instance of ConversationCamera in the scene.");
				return 0f;
			}

			switch (stopStart)
			{
				case StopStart.Start:
					conversationCamera.Begin (characterDataA.Character, characterDataB.Character);
					break;

				case StopStart.Stop:
					conversationCamera.End ();
					break;
			}

			return 0f;
		}


		public override void Skip ()
		{
			ConversationCamera conversationCamera = ConversationCamera.Instance;
			if (conversationCamera == null)
			{
				return;
			}

			switch (stopStart)
			{
				case StopStart.Start:
					conversationCamera.Begin (characterDataA.Character, characterDataB.Character);
					break;

				case StopStart.Stop:
					conversationCamera.End (true);
					break;
			}
		}


		#if UNITY_EDITOR

		public override void ShowGUI ()
		{
			stopStart = (StopStart) EditorGUILayout.EnumPopup ("Method:", stopStart);

			if (stopStart == StopStart.Start)
			{
				EditorGUILayout.LabelField ("Character A:");
				characterDataA.isPlayer = EditorGUILayout.Toggle ("Is player?", characterDataA.isPlayer);
				if (characterDataA.isPlayer)
				{
					characterDataA.playerID = ChoosePlayerGUI (characterDataA.playerID, true);
				}
				else
				{
					characterDataA.npc = (NPC) EditorGUILayout.ObjectField ("NPC:", characterDataA.npc, typeof (NPC), true);
				}

				EditorGUILayout.LabelField ("Character B:");
				characterDataB.isPlayer = EditorGUILayout.Toggle ("Is player?", characterDataB.isPlayer);
				if (characterDataB.isPlayer)
				{
					characterDataB.playerID = ChoosePlayerGUI (characterDataB.playerID, true);
				}
				else
				{
					characterDataB.npc = (NPC) EditorGUILayout.ObjectField ("NPC:", characterDataB.npc, typeof (NPC), true);
				}
			}

			AfterRunningOption ();
		}

		#endif


		[System.Serializable]
		private class CharacterData
		{

			public NPC npc = null;
			public bool isPlayer = false;
			public int playerID = -1;


			public Char Character
			{
				get
				{
					if (isPlayer)
					{
						if (KickStarter.settingsManager == null || KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow) return KickStarter.player;
						if (playerID < 0) return KickStarter.player;

						return KickStarter.settingsManager.GetPlayerPrefab (playerID).GetSceneInstance ();
					}

					return npc;
				}
			}

		}

	}

}