/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2020
 *	
 *	"ActionScene.cs"
 * 
 *	This action loads a new scene.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionScene : Action
	{
		
		public ChooseSceneBy chooseSceneBy = ChooseSceneBy.Number;
		public int sceneNumber;
		public int sceneNumberParameterID = -1;
		public string sceneName;
		public int sceneNameParameterID = -1;
		public bool assignScreenOverlay;
		public bool onlyPreload = false;

		public bool relativePosition = false;
		public Marker relativeMarker;
		protected Marker runtimeRelativeMarker;
		public int relativeMarkerID;
		public int relativeMarkerParameterID = -1;
		public bool forceReload = false;


		public ActionScene ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Scene;
			title = "Switch";
			description = "Moves the Player to a new scene. The scene must be listed in Unity's Build Settings. By default, the screen will cut to black during the transition, but the last frame of the current scene can instead be overlayed. This allows for cinematic effects: if the next scene fades in, it will cause a crossfade effect; if the next scene doesn't fade, it will cause a straight cut.";
		}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			sceneNumber = AssignInteger (parameters, sceneNumberParameterID, sceneNumber);
			sceneName = AssignString (parameters, sceneNameParameterID, sceneName);
			runtimeRelativeMarker = AssignFile <Marker> (parameters, relativeMarkerParameterID, relativeMarkerID, relativeMarker);
		}
		
		
		public override float Run ()
		{
			if (sceneNumber > -1 || chooseSceneBy == ChooseSceneBy.Name)
			{
				int runtimeSceneIndex = (chooseSceneBy == ChooseSceneBy.Name) ? KickStarter.sceneChanger.NameToIndex (sceneName) : sceneNumber;

				if (runtimeSceneIndex < 0) return 0f;

				if (onlyPreload)
				{
					if (AdvGame.GetReferences ().settingsManager.useAsyncLoading)
					{
						KickStarter.sceneChanger.PreloadScene (runtimeSceneIndex);
					}
					else if (AdvGame.GetReferences ().settingsManager.useLoadingScreen)
					{
						LogWarning ("Scenes cannot be preloaded when loading scenes are used in the Settings Manager.");
					}
					else
					{
						LogWarning ("To pre-load scenes, 'Load scenes asynchronously?' must be enabled in the Settings Manager.");
					}
					return 0f;
				}

				if (relativePosition && runtimeRelativeMarker != null)
				{
					KickStarter.sceneChanger.SetRelativePosition (runtimeRelativeMarker);
				}

				KickStarter.sceneChanger.ChangeScene (runtimeSceneIndex, true, forceReload, assignScreenOverlay);
			}
			return 0f;
		}


		public override ActionEnd End (List<Action> actions)
		{
			if (onlyPreload)
			{
				return base.End (actions);
			}
			if (isAssetFile)
			{
				return base.End (actions);
			}
			return GenerateStopActionEnd ();
		}
		

		#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			chooseSceneBy = (ChooseSceneBy) EditorGUILayout.EnumPopup ("Choose scene by:", chooseSceneBy);
			if (chooseSceneBy == ChooseSceneBy.Name)
			{
				sceneNameParameterID = Action.ChooseParameterGUI ("Scene name:", parameters, sceneNameParameterID, ParameterType.String);
				if (sceneNameParameterID < 0)
				{
					sceneName = EditorGUILayout.TextField ("Scene name:", sceneName);
				}
			}
			else
			{
				sceneNumberParameterID = Action.ChooseParameterGUI ("Scene number:", parameters, sceneNumberParameterID, ParameterType.Integer);
				if (sceneNumberParameterID < 0)
				{
					sceneNumber = EditorGUILayout.IntField ("Scene number:", sceneNumber);
				}
			}

			onlyPreload = EditorGUILayout.ToggleLeft ("Don't change scene, just preload data?", onlyPreload);

			if (onlyPreload)
			{
				if (AdvGame.GetReferences () != null && AdvGame.GetReferences ().settingsManager != null && AdvGame.GetReferences ().settingsManager.useAsyncLoading)
				{}
				else if (AdvGame.GetReferences () != null && AdvGame.GetReferences ().settingsManager != null && AdvGame.GetReferences ().settingsManager.useLoadingScreen)
				{
					EditorGUILayout.HelpBox ("Preloaded scene data can not be used if loading screens are used.", MessageType.Warning);
				}
				else
				{
					EditorGUILayout.HelpBox ("To pre-load scenes, 'Load scenes asynchronously?' must be enabled in the Settings Manager.", MessageType.Warning);
				}

				numSockets = 1;
				AfterRunningOption ();
			}
			else
			{
				forceReload = EditorGUILayout.ToggleLeft ("Reload even if scene is already open?", forceReload);
				relativePosition = EditorGUILayout.ToggleLeft ("Position Player relative to Marker?", relativePosition);
				if (relativePosition)
				{
					relativeMarkerParameterID = Action.ChooseParameterGUI ("Relative Marker:", parameters, relativeMarkerParameterID, ParameterType.GameObject);
					if (relativeMarkerParameterID >= 0)
					{
						relativeMarkerID = 0;
						relativeMarker = null;
					}
					else
					{
						relativeMarker = (Marker) EditorGUILayout.ObjectField ("Relative Marker:", relativeMarker, typeof(Marker), true);
						
						relativeMarkerID = FieldToID (relativeMarker, relativeMarkerID);
						relativeMarker = IDToField (relativeMarker, relativeMarkerID, false);
					}
				}

				assignScreenOverlay = EditorGUILayout.ToggleLeft ("Overlay current screen during switch?", assignScreenOverlay);
				if (isAssetFile)
				{
					EditorGUILayout.HelpBox ("To perform any Actions afterwards, 'Is skippable?' must be unchecked, and 'Survive scene changes?' must be checked, in the ActionList asset's properties.", MessageType.Info);
					numSockets = 1;
					AfterRunningOption ();
				}
				else
				{
					numSockets = 0;
				}
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			AssignConstantID (relativeMarker, relativeMarkerID, relativeMarkerParameterID);
		}
		
		
		public override string SetLabel ()
		{
			if (chooseSceneBy == ChooseSceneBy.Name)
			{
				return sceneName;
			}
			return sceneNumber.ToString ();
		}


		public override bool ReferencesObjectOrID (GameObject gameObject, int id)
		{
			if (relativePosition && relativeMarkerParameterID < 0)
			{
				if (relativeMarker != null && relativeMarker.gameObject == gameObject) return true;
				if (relativeMarkerID == id) return true;
			}
			return false;
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Scene: Switch' Action, set to preload a new scene</summary>
		 * <param name = "preloadSceneIndex">The index of the scene to preload</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionScene CreateNew_PreloadOnly (int preloadSceneIndex)
		{
			ActionScene newAction = (ActionScene) CreateInstance <ActionScene>();

			newAction.sceneName = string.Empty;
			newAction.sceneNumber = preloadSceneIndex;
			newAction.chooseSceneBy = ChooseSceneBy.Number;
			newAction.onlyPreload = true;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Scene: Switch' Action, set to switch to a new scene</summary>
		 * <param name = "newSceneIndex">The scene index to switch to</param>
		 * <param name = "forceReload">If True, the scene will be loaded even if currently open</param>
		 * <param name = "overlayCurrentScreen">If True, the previous scene will be displayed during the switch to mask the transition</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionScene CreateNew_Switch (int newSceneIndex, bool forceReload, bool overlayCurrentScreen)
		{
			ActionScene newAction = (ActionScene) CreateInstance <ActionScene>();
			newAction.sceneName = string.Empty;
			newAction.sceneNumber = newSceneIndex;
			newAction.chooseSceneBy = ChooseSceneBy.Number;
			newAction.onlyPreload = false;
			newAction.forceReload = forceReload;
			newAction.assignScreenOverlay = overlayCurrentScreen;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Scene: Switch' Action, set to preload a new scene</summary>
		 * <param name = "preloadSceneName">The name of the scene to preload</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionScene CreateNew_PreloadOnly (string preloadSceneName)
		{
			ActionScene newAction = (ActionScene)CreateInstance<ActionScene> ();

			newAction.sceneName = preloadSceneName;
			newAction.sceneNumber = -1;
			newAction.chooseSceneBy = ChooseSceneBy.Name;
			newAction.onlyPreload = true;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Scene: Switch' Action, set to switch to a new scene</summary>
		 * <param name = "newSceneName">The scene name to switch to</param>
		 * <param name = "forceReload">If True, the scene will be loaded even if currently open</param>
		 * <param name = "overlayCurrentScreen">If True, the previous scene will be displayed during the switch to mask the transition</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionScene CreateNew_Switch (string newSceneName, bool forceReload, bool overlayCurrentScreen)
		{
			ActionScene newAction = (ActionScene)CreateInstance<ActionScene> ();
			newAction.sceneName = newSceneName;
			newAction.sceneNumber = -1;
			newAction.chooseSceneBy = ChooseSceneBy.Name;
			newAction.onlyPreload = false;
			newAction.forceReload = forceReload;
			newAction.assignScreenOverlay = overlayCurrentScreen;
			return newAction;
		}

	}

}