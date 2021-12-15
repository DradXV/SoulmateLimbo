/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2020
 *	
 *	"SpeechPlayableBehaviour.cs"
 * 
 *	A PlayableBehaviour that allows for AC speech playback in Timelines
 * 
 */

#if UNITY_EDITOR
using System.Reflection;
using System;
#endif

#if AddressableIsPresent
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
#endif

#if !ACIgnoreTimeline
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace AC
{

	/**
	 * A PlayableBehaviour that allows for AC speech playback in Timelines
	 */
	[System.Serializable]
	public class SpeechPlayableBehaviour : PlayableBehaviour
	{

		#region Variables

		protected SpeechPlayableData speechPlayableData;
		protected SpeechTrackPlaybackMode speechTrackPlaybackMode;
		protected Char speaker;
		protected bool isPlaying;
		protected int trackInstanceID;

		#if AddressableIsPresent
		protected bool isAwaitingAddressable = false;
		protected AudioClip addressableAudioClip = null;
		#endif

		#endregion


		#region PublicFunctions

		public void Init (SpeechPlayableData _speechPlayableData, Char _speaker, SpeechTrackPlaybackMode _speechTrackPlaybackMode, int _trackInstanceID)
		{
			speechPlayableData = _speechPlayableData;
			speaker = _speaker;
			speechTrackPlaybackMode = _speechTrackPlaybackMode;
			trackInstanceID = _trackInstanceID;

			#if AddressableIsPresent
			PrepareAddressable ();
			#endif
		}


		public override void OnBehaviourPlay (Playable playable, FrameData info)
		{
			isPlaying = IsValid ();

			base.OnBehaviourPlay (playable, info);
		}


		public override void ProcessFrame (Playable playable, FrameData info, object playerData)
		{
			if (isPlaying)
			{
				isPlaying = false;

				if (Application.isPlaying)
				{
					string messageText = speechPlayableData.messageText;

					int languageNumber = Options.GetLanguage ();
					if (languageNumber > 0)
					{
						// Not in original language, so pull translation in from Speech Manager
						messageText = KickStarter.runtimeLanguages.GetTranslation (messageText, speechPlayableData.lineID, languageNumber, AC_TextType.Speech);
					}

					if (speechTrackPlaybackMode == SpeechTrackPlaybackMode.ClipDuration)
					{
						messageText += "[hold]";
					}
					
					#if AddressableIsPresent
					KickStarter.dialog.StartDialog (speaker, messageText, false, speechPlayableData.lineID, false, true, addressableAudioClip);
					#else
					KickStarter.dialog.StartDialog (speaker, messageText, false, speechPlayableData.lineID, false, true);
					#endif
				}
				#if UNITY_EDITOR
				else if (KickStarter.menuPreview)
				{
					Speech previewSpeech = new Speech (speaker, speechPlayableData.messageText);
					KickStarter.menuPreview.SetPreviewSpeech (previewSpeech, trackInstanceID);
				}
				#else
				else
				{
					ACDebug.Log ("Playing speech line with track ID: " + trackInstanceID);
				}
				#endif
			}

			base.ProcessFrame (playable, info, playerData);
		}

		#endregion


		#region ProtectedFunctions

		protected bool IsValid ()
		{
			if (speechPlayableData != null && !string.IsNullOrEmpty (speechPlayableData.messageText))
			{
				return true;
			}
			return false;
		}


		#if AddressableIsPresent

		protected void PrepareAddressable ()
		{
			if (!isAwaitingAddressable && KickStarter.speechManager.referenceSpeechFiles == ReferenceSpeechFiles.ByAddressable && speechPlayableData.lineID >= 0)
			{
				SpeechLine speechLine = KickStarter.speechManager.GetLine (speechPlayableData.lineID);
				if (speechLine != null)
				{
					string filename = speechLine.GetFilename ();
					Addressables.LoadAssetAsync<AudioClip>(filename).Completed += OnCompleteLoad;
					isAwaitingAddressable = true;
				}
			}
		}


		protected void OnCompleteLoad (AsyncOperationHandle<AudioClip> obj)
		{
			isAwaitingAddressable = false;
			addressableAudioClip = obj.Result;
		}

		#endif

		#endregion


		#region GetSet

		/** The speaking character */
		public Char Speaker
		{
			get
			{
				return speaker;
			}
		}

		#endregion

	}

}
#endif