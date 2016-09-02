using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Global;
using MajorAxes;
using Vox;

public class Voxeme : MonoBehaviour {

	[HideInInspector]
	public VoxML voxml = new VoxML();

	public OperationalVox opVox = new OperationalVox ();

	Rigging rigging;

	public Queue<Vector3> interTargetPositions = new Queue<Vector3> ();
	public Vector3 targetPosition;
	public Vector3 targetRotation;
	public Vector3 targetScale;
	public float moveSpeed = 1.0f;
	public float turnSpeed = 2.5f;
	//public MajorAxis majorAxis;

	public float minYBound;

	public GameObject supportingSurface = null;

	public bool isGrasped = false;

	// Use this for initialization
	void Start () {
		// load in VoxML knowledge
		TextAsset markup = Resources.Load (gameObject.name) as TextAsset;
		if (markup != null) {
			voxml = VoxML.LoadFromText (markup.text);
		}

		// populate operational voxeme structure
		PopulateOperationalVoxeme();

		// get movement blocking
		minYBound = Helper.GetObjectWorldSize(gameObject).min.y;

		// get rigging components
		rigging = gameObject.GetComponent<Rigging> ();

		targetPosition = transform.position;
		targetRotation = transform.eulerAngles;
		targetScale = transform.localScale;
	}
		
	// Update is called once per frame
	void Update () {
		if (interTargetPositions.Count == 0) {	// no queued path
			if (!Helper.VectorIsNaN (targetPosition)) {	// has valid destination
				if (!isGrasped) {
					if (transform.position != targetPosition) {
						Vector3 offset = MoveToward (targetPosition);

						if (offset.sqrMagnitude <= 0.01f) {
							transform.position = targetPosition;
						}
					}
				}
				else {
					GameObject reachObj = GameObject.Find ("ReachObject");
					if (reachObj.transform.position != targetPosition) {
						Vector3 offset = MoveToward (targetPosition);

						if (offset.sqrMagnitude <= 0.01f) {
							reachObj.transform.position = targetPosition;
						}
					}
				}
			}
			else {	// cannot execute motion
				OutputHelper.PrintOutput("I'm sorry, I can't do that.");
				targetPosition = transform.position;
			}
		}
		else {
			Vector3 interimTarget = interTargetPositions.Peek ();
			if (!isGrasped) {
				if (transform.position != interimTarget) {
					Vector3 offset = MoveToward (interimTarget);

					if (offset.sqrMagnitude <= 0.001f) {
						transform.position = interimTarget;
						interTargetPositions.Dequeue ();
					}
				}
			}
			else {
				GameObject reachObj = GameObject.Find ("ReachObject");
				if (reachObj.transform.position != interimTarget) {
					Vector3 offset = MoveToward (interimTarget);

					if (offset.sqrMagnitude <= 0.01f) {
						reachObj.transform.position = interimTarget;
						interTargetPositions.Dequeue ();
					}
				}
			}
		}

		if (rigging != null) {
			if (rigging.usePhysicsRig) {
				return;
			}
		}

		if (!Helper.VectorIsNaN (targetRotation)) {	// has valid target
			if (!isGrasped) {
				if (transform.rotation != Quaternion.Euler (targetRotation)) {
					if ((Mathf.Deg2Rad * Quaternion.Angle (transform.rotation, Quaternion.Euler (targetRotation))) > 0.01f) {
						//transform.eulerAngles = Vector3.MoveTowards (transform.eulerAngles, targetRotation, Time.deltaTime * turnSpeed);
						//transform.eulerAngles = Vector3.Slerp (transform.eulerAngles, targetRotation, Time.deltaTime * turnSpeed);
						transform.rotation = Quaternion.Slerp (transform.rotation, 
							Quaternion.Euler (targetRotation), Time.deltaTime * turnSpeed);
					} else {
						//transform.eulerAngles = targetRotation;
						transform.rotation = Quaternion.Euler (targetRotation);
					}
				}
			}
		}
		else {	// cannot execute motion
			OutputHelper.PrintOutput("I'm sorry, I can't do that.");
			targetPosition = transform.position;
		}

		if ((transform.localScale != targetScale) && (!isGrasped)) {
			Vector3 offset = transform.localScale - targetScale;
			Vector3 normalizedOffset = Vector3.Normalize (offset);
	
			transform.localScale = new Vector3 (transform.localScale.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
				transform.localScale.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
				transform.localScale.z - normalizedOffset.z * Time.deltaTime * moveSpeed);
	
			if (offset.sqrMagnitude <= 0.01f) {
				transform.localScale = targetScale;
			}
		}

		RaycastHit[] hits;

		hits = Physics.RaycastAll (transform.position, AxisVector.negYAxis);
		List<RaycastHit> hitList = new List<RaycastHit> ((RaycastHit[])hits);
		hits = hitList.OrderBy (h => h.distance).ToArray ();
		foreach (RaycastHit hit in hits) {
			if (hit.collider.gameObject.GetComponent<BoxCollider> () != null) {
				if (hit.collider.gameObject.GetComponent<BoxCollider> ().enabled) {
					supportingSurface = hit.collider.gameObject;
					break;
				}
			}
		}

		if (supportingSurface != null) {
			//Debug.Log (supportingSurface.name);
			// add check for SupportingSurface component
			Renderer[] renderers = supportingSurface.GetComponentsInChildren<Renderer> ();
			Bounds surfaceBounds = new Bounds ();
			foreach (Renderer renderer in renderers) {
				if (renderer.bounds.max.y > surfaceBounds.max.y) {
					surfaceBounds = renderer.bounds;
				}
			}

			Vector3 currentMin = gameObject.transform.position;
			renderers = gameObject.GetComponentsInChildren<Renderer> ();
			Bounds objectBounds = new Bounds ();
			foreach (Renderer renderer in renderers) {
				if (renderer.bounds.max.y > objectBounds.max.y) {
					objectBounds = renderer.bounds;
				}

				if (renderer.bounds.min.y < currentMin.y) {
					currentMin = renderer.bounds.min;
				}
			}

			if (transform.position.y < transform.position.y + (minYBound - objectBounds.min.y)) {
				transform.position = new Vector3 (transform.position.x,
					transform.position.y + (minYBound - objectBounds.min.y),
					transform.position.z);
			}

			/*if (supportingSurface.GetComponent<SupportingSurface> ().surfaceType == SupportingSurface.SupportingSurfaceType.Concave) {
				/*if (objectBounds.min.y < surfaceBounds.min.y) {
				transform.position = new Vector3 (transform.position.x,
				                                  transform.position.y + (surfaceBounds.min.y - objectBounds.min.y),
				                                  transform.position.z);
			}*/
				/*if (currentMin.y < surfaceBounds.min.y) {
					transform.position = new Vector3 (transform.position.x,
						transform.position.y + (surfaceBounds.min.y - currentMin.y),
						transform.position.z);
				}
			} else {
				/*if (objectBounds.min.y < surfaceBounds.max.y) {
				transform.position = new Vector3 (transform.position.x,
			                         transform.position.y + (surfaceBounds.max.y - objectBounds.min.y),
			                         transform.position.z);
			}*/
				/*if (currentMin.y < surfaceBounds.max.y) {
					transform.position = new Vector3 (transform.position.x,
						transform.position.y + (surfaceBounds.max.y - currentMin.y),
						transform.position.z);
				}
			}*/
		}

		// check relationships

	}

	Vector3 MoveToward(Vector3 target) {
		if (!isGrasped) {
			Vector3 offset = transform.position - target;
			Vector3 normalizedOffset = Vector3.Normalize (offset);

			if (rigging.usePhysicsRig) {
				Rigidbody[] rigidbodies = gameObject.GetComponentsInChildren<Rigidbody> ();
				foreach (Rigidbody rigidbody in rigidbodies) {
					rigidbody.MovePosition (new Vector3 (transform.position.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
						transform.position.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
						transform.position.z - normalizedOffset.z * Time.deltaTime * moveSpeed));
				}
			}

			transform.position = new Vector3 (transform.position.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
				transform.position.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
				transform.position.z - normalizedOffset.z * Time.deltaTime * moveSpeed);

			//GameObject.Find ("ReachObject").transform.position = transform.position;

			return offset;
		}
		else {
			GameObject reachObj = GameObject.Find ("ReachObject");
			GameObject grasperCoord = GameObject.Find ("GrasperCoord");


			Vector3 offset = reachObj.transform.position - target;
			Vector3 normalizedOffset = Vector3.Normalize (offset);

			/*if (rigging.usePhysicsRig) {
				Rigidbody[] rigidbodies = gameObject.GetComponentsInChildren<Rigidbody> ();
				foreach (Rigidbody rigidbody in rigidbodies) {
					rigidbody.MovePosition (new Vector3 (transform.position.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
						transform.position.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
						transform.position.z - normalizedOffset.z * Time.deltaTime * moveSpeed));
				}
			}*/

			reachObj.transform.position = new Vector3 (reachObj.transform.position.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
				reachObj.transform.position.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
				reachObj.transform.position.z - normalizedOffset.z * Time.deltaTime * moveSpeed);

			return offset;
		}
	}

	void OnCollisionEnter(Collision other) {
		if (other.gameObject.tag == "MainCamera") {
			return;
		}
	}

	void PopulateOperationalVoxeme() {
		// set entity type
		opVox.VoxemeType = voxml.Entity.Type;

		// set lex
		opVox.Lex.Pred = voxml.Lex.Pred;
		//Debug.Log (opVox.Lex.Pred);

		// set type info

		// find component objects
		foreach (VoxTypeComponent c in voxml.Type.Components) {
			string[] s = c.Value.Split('[');
			Transform obj = null;
			int index = -1;
			obj = gameObject.transform.FindChild(gameObject.name+"*/"+s[0]);
			if (s.Length > 1) {
				index = Helper.StringToInt (s[1].Remove (s[1].IndexOf (']')));
			}

			if (obj != null) {
				//Debug.Log (s[0]);
				//Debug.Log (obj);
				//Debug.Log (index);
				opVox.Type.Components.Add (new Triple<string,GameObject,int> (s[0], obj.gameObject, index));
			}
		}

		// set component as semantic head
		string[] str = voxml.Type.Head.Split('[');
		int i = Helper.StringToInt (str[1].Remove (str[1].IndexOf (']')));
		if (opVox.Type.Components.FindIndex (c => c.Item3 == i) != -1) {
			opVox.Type.Head = opVox.Type.Components.First (c => c.Item3 == i);
		}
		// if none, add entire game object as semantic head for voxeme
		else {
			opVox.Type.Head = new Triple<string,GameObject,int> (gameObject.name, gameObject, i);
			opVox.Type.Components.Add (new Triple<string,GameObject,int> (gameObject.name, gameObject, i));
		}


		// set habitat info
		foreach (VoxHabitatIntr ih in voxml.Habitat.Intrinsic) {
			string[] s = ih.Name.Split ('[');
			int index = Helper.StringToInt (s [1].Remove (s [1].IndexOf (']')));
			//Debug.Log(index);
			//Debug.Log (s[0] + " = {" + ih.Value + "}");

			if (!opVox.Habitat.IntrinsicHabitats.ContainsKey (index)) {
				opVox.Habitat.IntrinsicHabitats.Add (index, new List<string> (){ s[0] + " = {" + ih.Value + "}" });
			}
			else {
				opVox.Habitat.IntrinsicHabitats [index].Add (s [0] + " = {" + ih.Value + "}");
			}
		}

		foreach (VoxHabitatExtr eh in voxml.Habitat.Extrinsic) {
			string[] s = eh.Name.Split ('[');
			int index = System.Convert.ToInt16 (s[1].Remove (s[1].IndexOf(']')));
			//Debug.Log(index);
			//Debug.Log (s[0] + " = {" + ih.Value + "}");

			if (!opVox.Habitat.ExtrinsicHabitats.ContainsKey (index)) {
				opVox.Habitat.ExtrinsicHabitats.Add (index, new List<string> (){ s[0] + " = {" + eh.Value + "}" });
			}
			else {
				opVox.Habitat.ExtrinsicHabitats [index].Add (s [0] + " = {" + eh.Value + "}");
			}
		}

		/*foreach (KeyValuePair<int,List<string>> kv in opVox.Habitat.IntrinsicHabitats) {
					Debug.Log (kv.Key);
					foreach (string s in kv.Value) {
						Debug.Log (s);
					}
				}*/

		// set affordance info
		foreach (VoxAffordAffordance a in voxml.Afford_Str.Affordances) {
			//Debug.Log (a.Formula);
			Regex reentrancyForm = new Regex (@"\[[0-9]+\]");
			Regex numericalForm = new Regex (@"[0-9]+");
			string[] s = a.Formula.Split (new string[]{ "->" }, StringSplitOptions.None);
			string[] conditions = s [0].Split (new char[]{ ',' }, 2);
			MatchCollection reentrancies = reentrancyForm.Matches (s [1]);
			string aff = "";
			string cHabitat = "";
			string cFormula = "";
			string events = "";
			string result = "";
			cHabitat = conditions [0]; // split into habitat and non-habitat condition (if any)
			cFormula = conditions.Length > 1 ? conditions [1] : ""; // split into habitat and non-habitat condition (if any)
			int index = (cHabitat.Split ('[').Length > 1) ? 
				Helper.StringToInt (cHabitat.Split ('[') [1].Remove (cHabitat.Split ('[') [1].IndexOf (']'))) : 0;

			//Debug.Log ("Habitat index: " + index.ToString ());
			foreach (Match match in reentrancies) {
				GroupCollection groups = match.Groups;
				foreach (Group group in groups) {
					aff = s[1].Replace (group.Value, group.Value.Trim (new char[]{ '[', ']' }));
				}
			}

			//if (cFormula != "") {
			//	Debug.Log ("Formula: " + cFormula);
			//}
			//Debug.Log ("Affordance: " + aff);

			events = aff.Split (']')[0].Trim ('[');
			MatchCollection numerical = numericalForm.Matches (events);
			foreach (Match match in numerical) {
				GroupCollection groups = match.Groups;
				foreach (Group group in groups) {
					events = events.Replace (group.Value, '['+group.Value+']');
				}
			}
			//Debug.Log ("Events: " + events);

			result = aff.Split (']') [1];
			numerical = numericalForm.Matches (result);
			foreach (Match match in numerical) {
				GroupCollection groups = match.Groups;
				foreach (Group group in groups) {
					result = result.Replace (group.Value, '[' + group.Value + ']');
				}
			}
			//Debug.Log ("Result: " + result);

			Pair<string,string> affordance = new Pair<string, string> (events, result);
			if (!opVox.Affordance.Affordances.ContainsKey (index)) {
				opVox.Affordance.Affordances.Add (index, new List<Pair<string,Pair<string,string>>> (){ new Pair<string,Pair<string,string>>(cFormula,affordance) });
			}
			else {
				opVox.Affordance.Affordances[index].Add (new Pair<string,Pair<string,string>>(cFormula,affordance));
			}
		}

		using (System.IO.StreamWriter file = 
			new System.IO.StreamWriter(gameObject.name+@".txt"))
		{
			file.WriteLine("PRED");
			file.WriteLine("{0,-20}",opVox.Lex.Pred);
			file.WriteLine("\n");
			file.WriteLine("TYPE");
			file.WriteLine("COMPONENTS");
			foreach (Triple<string, GameObject, int> component in opVox.Type.Components) {
				file.Write (String.Format("{0,-20}{1,-20}{2,-20}{3,-20}{4,-20}\n",
					"Name: " + component.Item1,
					"\t",
					"GameObject name: " + component.Item2.name,
					"\t",
					"Index: " + component.Item3));
			}
			file.WriteLine("HABITATS");
			file.WriteLine("INTRINSIC");
			foreach (KeyValuePair<int,List<string>> kv in opVox.Habitat.IntrinsicHabitats) {
				file.Write ("Index: " + kv.Key);
				foreach (string formula in kv.Value) {
					file.Write ("\t\tFormula: " + formula + "\n");
				}
			}
			file.WriteLine("EXTRINSIC");
			foreach (KeyValuePair<int,List<string>> kv in opVox.Habitat.ExtrinsicHabitats) {
				file.Write ("Index: " + kv.Key);
				foreach (string formula in kv.Value) {
					file.Write ("\t\tFormula: " + formula + "\n");
				}
			}
			file.WriteLine("\n");
			file.WriteLine("AFFORDANCES");
			foreach (KeyValuePair<int, List<Pair<string, Pair<string, string>>>> kv in opVox.Affordance.Affordances) {
				file.Write ("Habitat index: " + kv.Key);
				foreach (Pair<string, Pair<string, string>> affordance in kv.Value) {
					file.Write ("\t\tCondition: " + ((affordance.Item1 != "") ? affordance.Item1 : "None") +
						"\t\tEvents: " + affordance.Item2.Item1 + "\t\tResult: " + ((affordance.Item2.Item2 != "") ? affordance.Item2.Item2 : "None") + "\n");
				}
			}
		}
	}
}
