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

	// rotation information for each subobject's rigidbody
	// (physics-resultant changes between the completion of one event and the start of the next must be brought into line)
	//public Dictionary<string,Vector3> startEventRotations = new Dictionary<string, Vector3> ();
	//public Dictionary<string,Vector3> endEventRotations = new Dictionary<string, Vector3> ();
	public Dictionary<string,Vector3> displacement = new Dictionary<string, Vector3> ();
	public Dictionary<string,Vector3> rotationalDisplacement = new Dictionary<string, Vector3> ();

	Rigging rigging;

	public Queue<Vector3> interTargetPositions = new Queue<Vector3> ();
	public Vector3 targetPosition;
	public Queue<Vector3> interTargetRotations = new Queue<Vector3> ();
	public Vector3 targetRotation;
	public Vector3 targetScale;
	public float moveSpeed = 1.0f;
	public float turnSpeed = 5.0f;

	public float minYBound;

	public GameObject supportingSurface = null;

	public bool isGrasped = false;
	public Transform graspTracker = null;
	public Transform grasperCoord = null;

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

						if (offset.sqrMagnitude <= Constants.EPSILON) {
							transform.position = targetPosition;
						}
					}
				}
				else {
					GraspScript graspController = grasperCoord.root.gameObject.GetComponent<GraspScript> ();
					if (graspTracker.transform.position != targetPosition+graspController.graspTrackerOffset) {
						Vector3 offset = MoveToward (targetPosition+graspController.graspTrackerOffset);

						if (offset.sqrMagnitude <= Constants.EPSILON) {
							graspTracker.transform.position = targetPosition;//+graspController.graspTrackerOffset;
						}
					}
				}
			}
			else {	// cannot execute motion
				OutputHelper.PrintOutput(OutputController.Role.Affector,"I'm sorry, I can't do that.");
				GameObject.Find ("BehaviorController").GetComponent<EventManager> ().SendMessage("AbortEvent");
				targetPosition = transform.position;
			}
		}
		else {
			Vector3 interimTarget = interTargetPositions.Peek ();
			if (!isGrasped) {
				//if (transform.position != interimTarget) {
					Vector3 offset = MoveToward (interimTarget);

					if (offset.sqrMagnitude <= Constants.EPSILON) {
						transform.position = interimTarget;
						interTargetPositions.Dequeue ();
					}
				//}
			}
			else {
				GraspScript graspController = grasperCoord.root.gameObject.GetComponent<GraspScript> ();
				//if (graspTracker.transform.position != interimTarget+graspController.graspTrackerOffset) {
				Vector3 offset = MoveToward (interimTarget+graspController.graspTrackerOffset);

					if (offset.sqrMagnitude <= Constants.EPSILON) {
						graspTracker.transform.position = interimTarget;//+graspController.graspTrackerOffset;
						interTargetPositions.Dequeue ();
					}
				//}
			}
		}
			
		if (interTargetRotations.Count == 0) {	// no queued sequence
			if (!Helper.VectorIsNaN (targetRotation)) {	// has valid target
				if (!isGrasped) {
					if (transform.rotation != Quaternion.Euler (targetRotation)) {
						float offset = RotateToward (targetRotation);

						if ((Mathf.Deg2Rad * offset) < 0.01f) {
							transform.rotation = Quaternion.Euler (targetRotation);
						}
					}
				}
				else {	// grasp tracking
				}
			}
			else {	// cannot execute motion
				OutputHelper.PrintOutput(OutputController.Role.Affector,"I'm sorry, I can't do that.");
				GameObject.Find ("BehaviorController").GetComponent<EventManager> ().SendMessage("AbortEvent");
				targetRotation = transform.eulerAngles;
			}
		}
		else {
			Vector3 interimTarget = interTargetRotations.Peek ();
			if (!isGrasped) {
				if (transform.rotation != Quaternion.Euler (interimTarget)) {
					//Debug.Log (transform.rotation == Quaternion.Euler (targetRotation));
					float offset = RotateToward (interimTarget);
					//Debug.Log (offset);
					//Debug.Log (Quaternion.Angle(transform.rotation,Quaternion.Euler (interimTarget)));
					//if ((Mathf.Deg2Rad * Quaternion.Angle (transform.rotation, Quaternion.Euler (interimTarget))) < 0.01f) {
					if ((Mathf.Deg2Rad * offset) < 0.01f) {
						transform.rotation = Quaternion.Euler (interimTarget);
						//Debug.Log (interimTarget);
						interTargetRotations.Dequeue ();
						//Debug.Log (interTargetRotations.Peek ());
					}
				}
			}
			else {	// grasp tracking
			}
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
				if ((hit.collider.gameObject.GetComponent<BoxCollider> ().enabled) &&
					(!hit.collider.gameObject.transform.IsChildOf(gameObject.transform))){
					supportingSurface = hit.collider.gameObject;
					break;
				}
			}
		}

		if (rigging != null) {
			if (rigging.usePhysicsRig) {
				return;
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

			if (targetPosition.y >= minYBound) {
				if (transform.position.y < transform.position.y + (minYBound - objectBounds.min.y)) {
					transform.position = new Vector3 (transform.position.x,
						transform.position.y + (minYBound - objectBounds.min.y),
						transform.position.z);
				}
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
			Vector3 offset = graspTracker.transform.position - target;
			Vector3 normalizedOffset = Vector3.Normalize (offset);

			/*if (rigging.usePhysicsRig) {
				Rigidbody[] rigidbodies = gameObject.GetComponentsInChildren<Rigidbody> ();
				foreach (Rigidbody rigidbody in rigidbodies) {
					rigidbody.MovePosition (new Vector3 (transform.position.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
						transform.position.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
						transform.position.z - normalizedOffset.z * Time.deltaTime * moveSpeed));
				}
			}*/

			graspTracker.transform.position = new Vector3 (graspTracker.transform.position.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
				graspTracker.transform.position.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
				graspTracker.transform.position.z - normalizedOffset.z * Time.deltaTime * moveSpeed);

			return offset;
		}
	}

	float RotateToward(Vector3 target) {
		float offset = 0.0f;
		if (!isGrasped) {
			//Quaternion offset = Quaternion.FromToRotation (transform.eulerAngles, targetRotation);
			//Vector3 normalizedOffset = Vector3.Normalize (offset);

			float angle = Quaternion.Angle (transform.rotation, Quaternion.Euler (target));
			float timeToComplete = angle / turnSpeed;
			float donePercentage = Mathf.Min (1.0f, Time.deltaTime / timeToComplete);
			Quaternion rot = Quaternion.Slerp (transform.rotation, Quaternion.Euler (target), donePercentage * 100.0f);
			//Quaternion resolve = Quaternion.identity;

			if (rigging.usePhysicsRig) {
				float displacementAngle = 360.0f;
				Rigidbody[] rigidbodies = gameObject.GetComponentsInChildren<Rigidbody> ();
				foreach (Rigidbody rigidbody in rigidbodies) {
					rigidbody.MoveRotation (rot);

					// check and see if rigidbody orientations and main body orientations are getting out of sync
					// due to physics effects

					// find the smallest displacement angle between an axis on the main body and an axis on this rigidbody
//					foreach (Vector3 mainBodyAxis in Constants.Axes.Values) {
//						foreach (Vector3 rigidbodyAxis in Constants.Axes.Values) {
//							if (Vector3.Angle (transform.rotation * mainBodyAxis, rigidbody.rotation * rigidbodyAxis) < displacementAngle) {
//								displacementAngle = Vector3.Angle (transform.rotation * mainBodyAxis, rigidbody.rotation * rigidbodyAxis);
//							}
//						}
//					}
//
//					Debug.Log (displacementAngle);
//
//					if (displacementAngle > Constants.EPSILON) {
//						//Debug.Break ();
//					}

					// compute displacement between rigidbody's orientation at the start of event and now
					// rotate the main body by that displacement
					// rotate the rigidbody back to start
//					if (startEventRotations.ContainsKey (rigidbody.name)) {
//						Debug.Log (rigidbody.name);
//						// initial = rotation to get to where we were at start of this event
//						Quaternion initial = Quaternion.Euler (gameObject.GetComponent<Voxeme> ().startEventRotations [rigidbody.name]);
//						Debug.Log (initial.eulerAngles);
//						// final = rotation to get to where we are now due to any physics effects
//						Quaternion final = rigidbody.rotation;
//						Debug.Log (final.eulerAngles);
//						// resolve = rotation to get from initial orientation to final orientation after physics effects
//						// (i.e. movement from initial state to final state)
//						resolve = final * Quaternion.Inverse (initial);
//						Debug.Log (resolve.eulerAngles);
//						//Debug.Log ((initial * resolve).eulerAngles);
//						Debug.Log ((resolve * initial).eulerAngles);
//						// resolveInv = rotation to get from final (current rigidbody) rotation back to initial (aligned with main obj) rotation
//						//resolveInv = initial * Quaternion.Inverse (final);
//						//Debug.Log (resolveInv.eulerAngles);
//						rigidbody.transform.rotation = initial;
//					}
				}
			}

			transform.rotation = rot;
			//transform.rotation = resolve * rot;
			//(args [0] as GameObject).transform.rotation = resolve * (args [0] as GameObject).transform.rotation;
			//Debug.Log ((args [0] as GameObject).transform.rotation.eulerAngles);

			//GameObject.Find ("ReachObject").transform.position = transform.position;

			offset = Quaternion.Angle (rot, Quaternion.Euler (target));
			//Debug.Log (offset);
		}
		else {
			//float offset = Quaternion.FromToRotation (transform.eulerAngles, targetRotation);//graspTracker.transform.position - target;
			//Vector3 normalizedOffset = Vector3.Normalize (offset);

			/*if (rigging.usePhysicsRig) {
					Rigidbody[] rigidbodies = gameObject.GetComponentsInChildren<Rigidbody> ();
					foreach (Rigidbody rigidbody in rigidbodies) {
						rigidbody.MovePosition (new Vector3 (transform.position.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
							transform.position.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
							transform.position.z - normalizedOffset.z * Time.deltaTime * moveSpeed));
					}
				}*/

			/*graspTracker.transform.position = new Vector3 (graspTracker.transform.position.x - normalizedOffset.x * Time.deltaTime * moveSpeed,
				graspTracker.transform.position.y - normalizedOffset.y * Time.deltaTime * moveSpeed,
				graspTracker.transform.position.z - normalizedOffset.z * Time.deltaTime * moveSpeed);*/

		}

		// resolve subobject rigidbody rotations
		// TODO: ResolveSubObjectRigidbodyRotations()
//		Debug.Log (gameObject.name);
//		Debug.Log (gameObject.transform.rotation.eulerAngles);
//		Rigidbody[] rigidbodies = gameObject.GetComponentsInChildren<Rigidbody> ();
//		Quaternion resolve = Quaternion.identity;
//		Quaternion resolveInv = Quaternion.identity;
//		Quaternion mainBodyResolve = Quaternion.identity;
//		foreach (Rigidbody rigidbody in rigidbodies) {
//			if (endEventRotations.ContainsKey (rigidbody.name)) {
//				Debug.Log (rigidbody.name);
//				// initial = rotation to get to where we were at satisfaction of previous event
//				Quaternion initial = Quaternion.Euler (gameObject.GetComponent<Voxeme> ().endEventRotations [rigidbody.name]);
//				Debug.Log (initial.eulerAngles);
//				// final = rotation to get to where we are now due to any physics effects
//				Quaternion final = rigidbody.rotation;
//				Debug.Log (final.eulerAngles);
//				// resolve = rotation to get from initial orientation to final orientation after physics effects
//				// (i.e. movement from initial state to final state)
//				resolve = final * Quaternion.Inverse (initial);
//				Debug.Log (resolve.eulerAngles);
//				//Debug.Log ((initial * resolve).eulerAngles);
//				Debug.Log ((resolve * initial).eulerAngles);
//				// resolveInv = rotation to get from final (current rigidbody) rotation back to initial (aligned with main obj) rotation
//				//resolveInv = initial * Quaternion.Inverse (final);
//				//Debug.Log (resolveInv.eulerAngles);
//				rigidbody.transform.rotation = initial;
//			}
//		}

//		(args [0] as GameObject).transform.rotation = resolve * (args [0] as GameObject).transform.rotation;
//		Debug.Log ((args [0] as GameObject).transform.rotation.eulerAngles);
//		Debug.Break ();

		return offset;
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

		// set symmetry info
		string[] rotsym = voxml.Type.RotatSym.Split (',');
		foreach (string sym in rotsym) {
			opVox.Type.RotatSym.Add (sym);
		}

		string[] reflsym = voxml.Type.ReflSym.Split (',');
		foreach (string sym in reflsym) {
			opVox.Type.ReflSym.Add (sym);
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

#if UNITY_EDITOR
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
			file.WriteLine("SYMMETRY");
			file.Write("ROT\t");
			foreach (string s in opVox.Type.RotatSym) {
				file.Write(String.Format("{0}\t",s));
			}
			file.Write("REFL\t");
			foreach (string s in opVox.Type.ReflSym) {
				file.Write(String.Format("{0}\t",s));
			}
			file.WriteLine("\n");
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
#endif
	}
}
