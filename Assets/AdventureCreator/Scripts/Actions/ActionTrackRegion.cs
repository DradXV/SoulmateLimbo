/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2020
 *	
 *	"ActionTrackRegion.cs"
 * 
 *	This action is used to set the enabled state of a Track region
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
	public class ActionTrackRegion : Action
	{
		
		public DragTrack track;
		public int trackConstantID = 0;
		public int trackRegionID = 0;
		public int trackRegionParameterID = -1;
		protected DragTrack runtimeTrack;
		public bool enable;
 

		public ActionTrackRegion ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Moveable;
			title = "Toggle track region";
			description = "Enables or disables a Track region";
		}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			runtimeTrack = AssignFile <DragTrack> (trackConstantID, track);
			trackRegionID = AssignInteger (parameters, trackRegionParameterID, trackRegionID);
		}

		
		public override float Run ()
		{
			if (runtimeTrack)
			{
				TrackSnapData snapData = track.GetSnapData (trackRegionID);
				if (snapData != null)
				{
					snapData.IsEnabled = enable;
				}
			}
			return 0f;
		}


		#if UNITY_EDITOR

		public override void ShowGUI (List<ActionParameter> parameters)
		{
			track = (DragTrack) EditorGUILayout.ObjectField ("Track:", track, typeof(DragTrack), true);

			trackConstantID = FieldToID<DragTrack> (track, trackConstantID);
			track = IDToField<DragTrack> (track, trackConstantID, false);

			if (track)
			{
				trackRegionParameterID = Action.ChooseParameterGUI ("Region ID:", parameters, trackRegionParameterID, ParameterType.Integer);
				if (trackRegionParameterID >= 0)
				{
					enable = EditorGUILayout.Toggle("Enable?", enable);
				}
				else
				{
					List<string> labelList = new List<string>();
					int snapIndex = 0;

					if (track.allTrackSnapData != null && track.allTrackSnapData.Count > 0)
					{
						for (int i = 0; i < track.allTrackSnapData.Count; i++)
						{
							labelList.Add(track.allTrackSnapData[i].EditorLabel);

							if (track.allTrackSnapData[i].ID == trackRegionID)
							{
								snapIndex = i;
							}
						}

						snapIndex = EditorGUILayout.Popup ("Region:", snapIndex, labelList.ToArray());
						trackRegionID = track.allTrackSnapData[snapIndex].ID;

						enable = EditorGUILayout.Toggle ("Enable?", enable);
					}
					else
					{
						EditorGUILayout.HelpBox("The chosen Drag object's Track has no snap points.", MessageType.Warning);
					}
				}
			}
			
			AfterRunningOption ();
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (saveScriptsToo)
			{
				AddSaveScript<RememberTrack> (track);
			}
			AssignConstantID <DragTrack> (track, trackConstantID, -1);
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Moveable: Toggle Track region' Action</summary>
		 * <param name = "_track">The DragTrack to affect</param>
		 * <param name = "_regionID">The region's ID</param>
		 * <param name = "_enable">If True, the region will be enabled</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionTrackRegion CreateNew (DragTrack _track, int _regionID, bool _enable)
		{
			ActionTrackRegion newAction = (ActionTrackRegion) CreateInstance <ActionTrackRegion>();
			newAction.track = _track;
			newAction.trackRegionID = _regionID;
			newAction.enable = _enable;
			return newAction;
		}

	}

}