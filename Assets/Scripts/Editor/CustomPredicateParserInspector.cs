using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PredicateParser))]
public class CustomPredicateParserInspector : Editor {
	List<string> prompts = new List<string> ();

	public override void OnInspectorGUI () 
	{
		base.OnInspectorGUI();

		if (!prompts.Contains("slide(Box1)"))
			prompts.Add ("slide(Box1)");
		if (!prompts.Contains("roll(Ball1)"))
			prompts.Add ("roll(Ball1)");
		if (!prompts.Contains("cross(Box1, Floor)"))
			prompts.Add ("cross(Box1, Floor)");
		if (!prompts.Contains("cross(Ball1, Floor)"))
			prompts.Add ("cross(Ball1, Floor)");
		if (!prompts.Contains("cross(Ball3, Floor)"))
			prompts.Add ("cross(Ball3, Floor)");
		if (!prompts.Contains("reach(Ball1, Left)"))
			prompts.Add ("reach(Ball1, Left)");
		if (!prompts.Contains("reach(Ball1, Center)"))
			prompts.Add ("reach(Ball1, Center)");
		if (!prompts.Contains("reach(Ball2, Center)"))
			prompts.Add ("reach(Ball2, Center)");
		
		if (GUILayout.Button("Prompt", GUILayout.Height(30)))
		{
			if(target.GetType() == typeof(PredicateParser))
			{
				PredicateParser getterSetter = (PredicateParser)target;
				getterSetter.InputStringProperty = getterSetter.inputString;
			}
		}

		foreach (string prompt in prompts)
		{
			if (GUILayout.Button(prompt))
			{
				if(target.GetType() == typeof(PredicateParser))
				{
					PredicateParser getterSetter = (PredicateParser)target;
					getterSetter.InputStringProperty = prompt;
				}
			}
		}

		if (GUILayout.Button("Reset", GUILayout.Height(30)))
		{
			if(target.GetType() == typeof(PredicateParser))
			{
				PredicateParser getterSetter = (PredicateParser)target;
				getterSetter.DynamicIntroduction = false;
				getterSetter.ShowTrails = false;

				object[] entities = Resources.FindObjectsOfTypeAll (typeof(OldEntityClass));
				foreach(OldEntityClass entity in entities)
				{
					entity.Reset();
				}
			}
		}
	}
}
