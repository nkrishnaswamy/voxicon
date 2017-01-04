/*
 * Flashback Video Recorder
 * FlashbackEditor.cs v1.2
 * 
 * Displays editable properties in the Unity Inspector window. Properties 
 * cannot be directly modified at runtime. Instead, use the Accessor and 
 * Mutator methods in FlashbackRecorder.cs
 * 
 * 
 * Copyright 2016 LaunchPoint Games
 * One license per seat. For all terms and conditions, see included 
 * documentation, or visit http://www.launchpointgames.com/unity/flashback.html
 */


using UnityEngine;
using UnityEditor;
using System.IO;
using FlashbackVideoRecorder;

namespace FlashbackVideoRecorder{

	[CustomEditor(typeof(FlashbackRecorder))]
	public class FlashbackEditor : Editor {

		SerializedProperty m_FrameRate;
		SerializedProperty m_Width;
		SerializedProperty m_Height;
		SerializedProperty m_AutoWidth;

		SerializedProperty m_ContinuousCapture;
		SerializedProperty m_RecordTime;

		SerializedProperty m_CaptureKey;
		SerializedProperty m_CaptureAudio;

		//Capture history settings
		GUIContent[] m_captureOptions = {new GUIContent("Continuous"), new GUIContent("Toggle")};
		int m_selectedCaptureOption = 0;

		//Capture resolution settings
		GUIContent[] m_resolutionOptions = {new GUIContent("Native"), new GUIContent("By Scale"), new GUIContent("By Value")};
		int m_selectedResolution = 0;
		SerializedProperty m_ResScale;
		SerializedProperty m_ResType;

		//File output settings
		SerializedProperty m_SaveToGif;
		SerializedProperty m_SaveToMp4;
		SerializedProperty m_SaveToOgg;

		void OnEnable(){

			FlashbackRecorder.ConfigureFFmpegDirectories ();

			m_FrameRate = serializedObject.FindProperty("m_FrameRate");
			m_AutoWidth = serializedObject.FindProperty ("m_AutomaticWidth");
			m_Height = serializedObject.FindProperty ("m_Height");
			m_Width = serializedObject.FindProperty ("m_Width");
			m_ContinuousCapture = serializedObject.FindProperty ("m_ContinuousCapture");
			m_RecordTime = serializedObject.FindProperty ("m_CaptureTime");

			m_ResScale = serializedObject.FindProperty ("m_ResolutionScale");
			m_ResType = serializedObject.FindProperty ("m_ResolutionType");

			m_CaptureKey = serializedObject.FindProperty ("m_CaptureKey");
			m_CaptureAudio = serializedObject.FindProperty ("m_CaptureAudio");

			m_SaveToGif = serializedObject.FindProperty ("m_SaveGif");
			m_SaveToMp4 = serializedObject.FindProperty ("m_SaveMp4");
			m_SaveToOgg = serializedObject.FindProperty ("m_SaveOgg");

			m_selectedResolution = m_ResType.enumValueIndex;

			m_selectedCaptureOption = 1;
			if (m_ContinuousCapture.boolValue)
				m_selectedCaptureOption = 0;
		}

		public override void OnInspectorGUI(){

			serializedObject.Update();

			if (Application.isPlaying) {
				EditorGUILayout.HelpBox("Video capture properties cannot be changed in the inspector at runtime since this may result in instability or file output problems. Instead, use the UpdateCaptureSettings(...) or UpdateOutputSettings(...) methods to change the FlashbackRecorder behavior.", MessageType.Info);
				GUI.enabled = false;
			}
				
			EditorGUILayout.Space ();

			EditorGUILayout.PropertyField(m_FrameRate, new GUIContent("Framerate", "The number of frames captured each second. Higher values require more memory, processing time, and will result in larger output files."));

			m_selectedCaptureOption = EditorGUILayout.Popup(new GUIContent("Capture Type", "Should recording happen continously, or start/stop on the Capture Key press?"), m_selectedCaptureOption, m_captureOptions);
			if (m_selectedCaptureOption == 0) {
				m_ContinuousCapture.boolValue = true;
				EditorGUILayout.PropertyField (m_RecordTime, new GUIContent ("Capture History", "How much video capture history (in seconds) to store in memory."));
			} else {
				m_ContinuousCapture.boolValue = false;
			}

			EditorGUILayout.Space ();

			EditorGUILayout.PropertyField (m_CaptureKey, new GUIContent ("Capture Key", string.Format("Press this key to write the last {0} seconds of captured video to disk in the specified formats. Select 'None' to control capture yourself programatically with the 'SaveCapturedFrames' methods.", m_RecordTime.floatValue)));

			EditorGUILayout.Space ();

			m_selectedResolution = EditorGUILayout.Popup(new GUIContent("Resolution", ""), m_selectedResolution, m_resolutionOptions);
			m_ResType.enumValueIndex = m_selectedResolution;

			if (m_selectedResolution == 1) {
				EditorGUILayout.PropertyField (m_ResScale, new GUIContent ("Scale", "The amount to scale the native resolution."), GUILayout.ExpandWidth (false));
			} else if (m_selectedResolution == 2) {
				SetDimensions ();
				EditorGUILayout.PropertyField (m_Height, new GUIContent ("Height", "The height of the captured video in pixels. Must be a positive even value (Height % 2 == 0)."), GUILayout.ExpandWidth (false));
				EditorGUILayout.BeginHorizontal ();

				if (m_AutoWidth.boolValue) {
					EditorGUILayout.LabelField (new GUIContent ("Width", "The width based on the height and aspect ratio of the current camera. Will be calculated again at runtime. Uncheck the \"Auto\" box to set the width manually."), new GUIContent (m_Width.intValue.ToString ()), GUILayout.ExpandWidth (false));
				} else {
					EditorGUILayout.PropertyField (m_Width, new GUIContent ("Width", "The width of the captured video in pixels. Must be a positive even value (Width % 2 == 0). Check the \"Auto\" box to set the width based on the height and aspect ratio."), GUILayout.ExpandWidth (false));
				}

				m_AutoWidth.boolValue = EditorGUILayout.ToggleLeft ("Auto", m_AutoWidth.boolValue, GUILayout.MaxWidth (50));

				EditorGUILayout.EndHorizontal ();
			}

			EditorGUILayout.Space ();

			//m_selectedOutput = EditorGUILayout.Popup(new GUIContent("Output Formats", "The file type(s) that will be written to disk when the Capture Key is pressed."), m_selectedOutput, m_outputOptions);
			EditorGUILayout.LabelField (new GUIContent ("Output Formats", "."));
			m_SaveToGif.boolValue = EditorGUILayout.ToggleLeft ("GIF", m_SaveToGif.boolValue, GUILayout.MaxWidth (50));
			m_SaveToMp4.boolValue = EditorGUILayout.ToggleLeft ("MP4", m_SaveToMp4.boolValue, GUILayout.MaxWidth (50));
			m_SaveToOgg.boolValue = EditorGUILayout.ToggleLeft ("OGG", m_SaveToOgg.boolValue, GUILayout.MaxWidth (50));


			if (m_SaveToMp4.boolValue || m_SaveToOgg.boolValue) {
				EditorGUILayout.PropertyField(m_CaptureAudio, new GUIContent("Capture Audio", "Should the audio be recorded to the video output."));
			}

			EditorGUILayout.Space ();


			SerializedProperty watermarks = serializedObject.FindProperty ("m_watermarks");
			EditorGUILayout.PropertyField(watermarks, true);


			EditorGUILayout.Space ();

			if (GUILayout.Button ("Prepare Windows Standalone Build")) {
				PrepareStandaloneBuild (true);
			}

			if (GUILayout.Button ("Prepare Mac OSX Standalone Build")) {
				PrepareStandaloneBuild (false);
			}

			serializedObject.ApplyModifiedProperties();


			GUI.enabled = true;
		}

		void SetDimensions(){

			if (m_Height.intValue % 2 == 1)
				m_Height.intValue--;

			m_Height.intValue = Mathf.Max (m_Height.intValue, 2);
			if (m_AutoWidth.boolValue) {
				m_Width.intValue = Mathf.CeilToInt (1.778f * m_Height.intValue);
				if(Camera.main != null)
					m_Width.intValue = Mathf.CeilToInt (Camera.main.aspect * m_Height.intValue);
			}

			if (m_Width.intValue % 2 == 1)
				m_Width.intValue--;

			m_Width.intValue = Mathf.Max (m_Width.intValue, 2);
		}

		void PrepareStandaloneBuild(bool forWin){

			if(Directory.Exists(FlashbackRecorder.FFmpegStandaloneDir()))
				Directory.Delete (FlashbackRecorder.FFmpegStandaloneDir(), true);
			Directory.CreateDirectory (FlashbackRecorder.FFmpegStandaloneDir());

			if (forWin) {
				string source = FlashbackRecorder.FFmpegPackageDir() + FlashbackRecorder.FFmpegWin;
				string dest = FlashbackRecorder.FFmpegStandaloneDir() + FlashbackRecorder.FFmpegWin;
				if (!File.Exists (dest)) {
					File.Copy (source, dest);
				}
			} else {
				string source = FlashbackRecorder.FFmpegPackageDir() + FlashbackRecorder.FFmpegMac;
				string dest = FlashbackRecorder.FFmpegStandaloneDir() + FlashbackRecorder.FFmpegMac;
				if (!File.Exists (dest)) {
					File.Copy (source, dest);
				}
			}

			AssetDatabase.Refresh ();
		}
	}
}