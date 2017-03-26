using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OldEntityClass),true)]
public class CustomEntityInspector : Editor {
	public override void OnInspectorGUI () 
	{
		base.OnInspectorGUI();
		
		// Take out this if statement to set the value using setter when ever you change it in the inspector.
		// But then it gets called a couple of times when ever inspector updates
		// By having a button, you can control when the value goes through the setter and getter, your self.
		//if (GUILayout.Button("Use setters/getters"))
		//{
		if(target.GetType().IsSubclassOf(typeof(OldEntityClass)))
		{
			OldEntityClass getterSetter = (OldEntityClass)target;
			getterSetter.CurrentBehaviorTypeProperty = getterSetter.currentBehaviorType;
			//Debug.Log(getterSetter.CurrentBehaviorTypeProperty);
		}
		//}
	}
}
