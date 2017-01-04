using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(Builder))]
public class Builder : Editor {

	public string buildName = "VoxSim_Build7";
	List<string> scenes = new List<string>(){"Assets/Scenes/VoxSimMenu.unity"};

	public override void OnInspectorGUI () 
	{
		base.OnInspectorGUI();

		if (GUILayout.Button ("Build", GUILayout.Height (30))) {
			using (System.IO.StreamWriter file = 
			       new System.IO.StreamWriter(@"Assets/Resources/ScenesList.txt"))
			{
				string scenesDirPath = Application.dataPath + "/Scenes/";
				string [] fileEntries = Directory.GetFiles(Application.dataPath+"/Scenes/");
				foreach (string s in fileEntries) {
					if (!s.EndsWith(".meta")) {
						string sceneName = s.Remove(0,Application.dataPath.Length-"Assets".Length);
						if (!scenes.Contains(sceneName)) {
							scenes.Add(sceneName);
							file.WriteLine(sceneName.Split ('/')[2].Replace (".unity",""));
						}
					}
				}
			}
			BuildPipeline.BuildPlayer(scenes.ToArray(),"Build/mac/"+buildName,BuildTarget.StandaloneOSXUniversal,BuildOptions.None);
			//BuildPipeline.BuildPlayer(scenes.ToArray(),"Build/web/"+buildName,BuildTarget.WebPlayer,BuildOptions.None);
		}
	}
}
