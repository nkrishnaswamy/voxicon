using UnityEngine;
using UnityEditor;
using System.Collections;
using FlashbackVideoRecorder;


namespace FlashbackVideoRecorder{

	//[CustomPropertyDrawer (typeof (Watermark))]
	public class WatermarkEditor : PropertyDrawer {

		bool foldout;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			
			foldout = EditorGUI.Foldout(position, foldout, label);
			if (foldout) {
				EditorGUI.BeginProperty (position, label, property);
				Debug.Log(property.CountInProperty());

				EditorGUILayout.PropertyField (property.FindPropertyRelative ("m_Image"), new GUIContent ("Capture Audio", "Should the audio be recorded to the video output."));
				EditorGUILayout.PropertyField (property.FindPropertyRelative ("m_horizontalAlignment"), new GUIContent ("Capture Audio", "Should the audio be recorded to the video output."));


				EditorGUI.EndProperty ();
			}

		}
	}
}