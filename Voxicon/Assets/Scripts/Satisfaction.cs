using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Global;
using RCC;

namespace Satisfaction {
	public static class SatisfactionTest {
		public static bool IsSatisfied (String test) {
			bool satisfied = false;
			Hashtable predArgs = Helper.ParsePredicate (test);
			String predString = "";
			String[] argsStrings = null;

			foreach (DictionaryEntry entry in predArgs) {
				predString = (String)entry.Key;
				argsStrings = ((String)entry.Value).Split (new char[] {','});
			}

			//Debug.Log (test);

			if (predString == "put") {	// satisfy put
				GameObject obj = GameObject.Find (argsStrings [0] as String);
				if (obj != null) {
					//Debug.Log(Helper.ConvertVectorToParsable(obj.transform.position) + " " + (String)argsStrings[1]);
					//Debug.Log(obj.transform.position);
					//Debug.Log (Helper.ParsableToVector ((String)argsStrings [1]));
					if (obj.transform.position == Helper.ParsableToVector ((String)argsStrings [1])) {
						satisfied = true;
						//obj.GetComponent<Rigging> ().ActivatePhysics (true);
						ReevaluateRelationships (predString, obj);	// we need to talk (do physics reactivation in here?)
					}
				}
			}
			else if (predString == "flip") {	// satisfy flip
				GameObject obj = GameObject.Find (argsStrings [0] as String);
				if (obj != null) {
					//Debug.Log(Helper.ConvertVectorToParsable(obj.transform.position) + " " + (String)argsStrings[1]);
					//Debug.Log(obj.transform.position);
					//Debug.Log (Quaternion.Angle(obj.transform.rotation,Quaternion.Euler(Helper.ParsableToVector((String)argsStrings[1]))));
					if (Quaternion.Angle (obj.transform.rotation, Quaternion.Euler (Helper.ParsableToVector ((String)argsStrings [1]))) == 0.0f) {
						satisfied = true;
						obj.GetComponent<Rigging> ().ActivatePhysics (true);
					}
				}
			}
			else if (predString == "bind") {	// satisfy bind
				satisfied = true;
			}

			return satisfied;
		}

		public static bool ComputeSatisfactionConditions(String command) {
			Hashtable predArgs = Helper.ParsePredicate (command);
			String pred = Helper.GetTopPredicate (command);
			
			if (predArgs.Count > 0) {
				Queue<String> argsStrings = new Queue<String> (((String)predArgs [pred]).Split (new char[] { ',' }));
				List<object> objs = new List<object> ();
				MethodInfo methodToCall;
				Predicates preds = GameObject.Find ("BehaviorController").GetComponent<Predicates> ();
			
				while (argsStrings.Count > 0) {
					object arg = argsStrings.Dequeue ();
				
					if (Helper.v.IsMatch ((String)arg)) {	// if arg is vector form
						objs.Add (Helper.ParsableToVector ((String)arg));
					} else if (arg is String) {	// if arg is String
						Regex q = new Regex("\".*\"");
						if (q.IsMatch (arg as String)) {
							objs.Add (arg as String);
						}
						else {
							if ((arg as String).Count (f => f == '(') +
						    (arg as String).Count (f => f == ')') == 0) {
								GameObject go = GameObject.Find (arg as String);
								if (go == null) {
									OutputHelper.PrintOutput ("What is a " + "\"" + (arg as String) + "\"?");
									return false;	// abort
								}
							}
							objs.Add (GameObject.Find (arg as String));
						}
					}
				}

				objs.Add (false);
				methodToCall = preds.GetType ().GetMethod (pred.ToUpper());

				if (methodToCall != null) {
					Debug.Log ("ComputeSatisfactionConditions: invoke " + methodToCall.Name);
					object obj = methodToCall.Invoke (preds, new object[]{ objs.ToArray () });
				}
				else {
					OutputHelper.PrintOutput ("Sorry, what does " + "\"" + pred + "\" mean?");
					return false;
				}
			}
			else {
				OutputHelper.PrintOutput ("Sorry, I don't understand \"" + command + ".\"");
				return false;
			}

			return true;
		}

		public static void ReevaluateRelationships(String program, GameObject obj) {
			// get object bounds
			Bounds objBounds = Helper.GetObjectWorldSize(obj);

			// get all objects
			GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

			if (program == "put") {
				Bounds testBounds = new Bounds ();
				Voxeme[] voxemes;
				RelationTracker relationTracker = (RelationTracker)GameObject.Find ("BehaviorController").GetComponent("RelationTracker");
				foreach (GameObject test in allObjects) {
					if (test != obj) {
						voxemes = test.GetComponentsInChildren<Voxeme> ();
						foreach (Voxeme voxeme in voxemes) {
							if (voxeme != null) {
								if (!voxeme.gameObject.name.Contains("*")) {	// hacky fix to filter out unparented objects w/ disabled voxeme components
									testBounds = Helper.GetObjectWorldSize (test);
									// bunch of underspecified RCC relations
									if (voxeme.voxml.Afford_Str.Affordances.Any (p => p.Formula.Contains("support"))) {
										if ((voxeme.voxml.Type.Concavity == "Concave") &&
										   (Helper.FitsIn (objBounds, testBounds))) {	// if test object is concave and placed object would fit inside
											if (RCC8.PO (objBounds, Helper.GetObjectWorldSize (test))) {	// interpenetration = support
												RiggingHelper.RigTo (obj, test);	// setup parent-child rig
												relationTracker.AddNewRelation(new List<GameObject>{test,obj},"support");
												Debug.Log (test.name + " supports " + obj.name);
											}
										}
										else {
											if (RCC8.EC (objBounds, Helper.GetObjectWorldSize (test))) {	// otherwise EC = support
												if (voxeme.enabled) {
													obj.GetComponent<Rigging> ().ActivatePhysics (true);
												}
												obj.GetComponent<Voxeme>().minYBound = Helper.GetObjectWorldSize(test).max.y;
												RiggingHelper.RigTo (obj, test);	// setup parent-child rig
												relationTracker.AddNewRelation(new List<GameObject>{test,obj},"support");
												Debug.Log (test.name + " supports " + obj.name);
											}
										}
									}

									if (voxeme.voxml.Afford_Str.Affordances.Any (p => p.Formula.Contains("contain"))) {
										if (Helper.FitsIn (objBounds, Helper.GetObjectWorldSize (test))) {
											if (RCC8.PO (objBounds, Helper.GetObjectWorldSize (test))) {	// interpenetration = containment
												obj.GetComponent<Voxeme> ().minYBound = Helper.GetObjectWorldSize (obj).min.y;
												RiggingHelper.RigTo (obj, test);	// setup parent-child rig
												relationTracker.AddNewRelation (new List<GameObject>{test,obj}, "contain");
												Debug.Log (test.name + " contains " + obj.name);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
