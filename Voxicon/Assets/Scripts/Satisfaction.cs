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
				GameObject theme = GameObject.Find (argsStrings [0] as String);
				if (theme != null) {
					//Debug.Log(Helper.VectorToParsable(theme.transform.position) + " " + (String)argsStrings[1]);
					//Debug.Log(obj.transform.position);
					if (Helper.CloseEnough(theme.transform.position,Helper.ParsableToVector ((String)argsStrings [1]))) {
						satisfied = true;
						//obj.GetComponent<Rigging> ().ActivatePhysics (true);
						//ReevaluateRelationships (predString, theme);	// we need to talk (do physics reactivation in here?)
						ReasonFromAffordances (predString, theme.GetComponent<Voxeme>());	// we need to talk (do physics reactivation in here?) // replace ReevaluateRelationships
					}
				}
			}
			else if (predString == "flip") {	// satisfy flip
				GameObject theme = GameObject.Find (argsStrings [0] as String);
				if (theme != null) {
					//Debug.Log(Helper.ConvertVectorToParsable(obj.transform.position) + " " + (String)argsStrings[1]);
					//Debug.Log(obj.transform.position);
					//Debug.Log (Quaternion.Angle(obj.transform.rotation,Quaternion.Euler(Helper.ParsableToVector((String)argsStrings[1]))));
					if (Helper.CloseEnough(theme.transform.rotation, Quaternion.Euler (Helper.ParsableToVector ((String)argsStrings [1])))) {
						satisfied = true;
						ReasonFromAffordances (predString, theme.GetComponent<Voxeme>());	// we need to talk (do physics reactivation in here?) // replace ReevaluateRelationships
						theme.GetComponent<Rigging> ().ActivatePhysics (true);
					}
				}
			}
			else if (predString == "bind") {	// satisfy bind
				satisfied = true;
			}
			else if (predString == "reach") {	// satisfy reach
				satisfied = true;
			}

			return satisfied;
		}

		public static bool ComputeSatisfactionConditions(String command) {
			Hashtable predArgs = Helper.ParsePredicate (command);
			String pred = Helper.GetTopPredicate (command);
			ObjectSelector objSelector = GameObject.Find ("BlocksWorld").GetComponent<ObjectSelector> ();
			
			if (predArgs.Count > 0) {
				Queue<String> argsStrings = new Queue<String> (((String)predArgs [pred]).Split (new char[] { ',' }));
				List<object> objs = new List<object> ();
				MethodInfo methodToCall;
				Predicates preds = GameObject.Find ("BehaviorController").GetComponent<Predicates> ();

				while (argsStrings.Count > 0) {
					object arg = argsStrings.Dequeue ();
				
					if (Helper.v.IsMatch ((String)arg)) {	// if arg is vector form
						objs.Add (Helper.ParsableToVector ((String)arg));
					}
					else if (arg is String) {	// if arg is String
						Regex q = new Regex("\".*\"");
						if (q.IsMatch (arg as String)) {
							objs.Add (arg as String);
						}
						else {
							if ((arg as String).Count (f => f == '(') +
						    	(arg as String).Count (f => f == ')') == 0) {
								List<GameObject> matches = new List<GameObject> ();
								foreach (Voxeme voxeme in objSelector.allVoxemes) {
									if (voxeme.voxml.Lex.Pred.Equals(arg)) {
										matches.Add (voxeme.gameObject);
									}
								}

								if (matches.Count <= 1) {
									GameObject go = GameObject.Find (arg as String);
									if (go == null) {
										OutputHelper.PrintOutput (string.Format ("What is a \"{0}\"?", (arg as String)));
										return false;	// abort
									}
								}
								else {
									//Debug.Log (string.Format ("Which {0}?", (arg as String)));
									//OutputHelper.PrintOutput (string.Format("Which {0}?", (arg as String)));
									//return false;	// abort
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

		public static void ReasonFromAffordances(String program, Voxeme obj) {
			Regex reentrancyForm = new Regex (@"\[[0-9]+\]");
			List<string> supportedRelations = new List<string> (new string[]{ @"on\(.*\)",	// list of supported relations
				@"in\(.*\)"});	// to do: move externally, draw from voxeme database
		
			// get relation tracker
			RelationTracker relationTracker = (RelationTracker)GameObject.Find ("BehaviorController").GetComponent("RelationTracker");

			// get bounds of theme object of program
			Bounds objBounds = Helper.GetObjectWorldSize(obj.gameObject);

			// get list of all voxeme entities
			Voxeme[] allVoxemes = UnityEngine.Object.FindObjectsOfType<Voxeme>();

			// reactivate physics by default
			bool reactivatePhysics = true;
			obj.minYBound = objBounds.min.y;

			// reasoning from affordances
			foreach (Voxeme test in allVoxemes) {
				// foreach voxeme
				// get bounds of object being tested against
				Bounds testBounds = Helper.GetObjectWorldSize(test.gameObject);
				if (test.enabled) {	// if voxeme is active
					OperationalVox.OpAfford_Str affStr = test.opVox.Affordance;
					foreach (int h in affStr.Affordances.Keys) {
						for (int i = 0; i < affStr.Affordances[h].Count; i++) {	// condition/event/result list for this habitat index
							//if (test.opVox.Habitat condition blah blah)	// ignore habitat conditionals for now
							string ev = affStr.Affordances[h][i].Item2.Item1;
							if (ev.Contains (program)) {
								//Debug.Log (test.opVox.Lex.Pred);
								//Debug.Log (program);

								foreach (string rel in supportedRelations) {
									Regex r = new Regex (rel);
									if (r.Match (ev).Length > 0) {	// found a relation that might apply between these objects
										string relation = r.Match (ev).Groups[0].Value.Split('(')[0];

										MatchCollection matches = reentrancyForm.Matches (ev);
										foreach (Match m in matches) {
											foreach (Group g in m.Groups) {
												int componentIndex = Helper.StringToInt (
													                     g.Value.Replace (g.Value, g.Value.Trim (new char[]{ '[', ']' })));
												//Debug.Log (componentIndex);
												if (test.opVox.Type.Components.FindIndex (c => c.Item3 == componentIndex) != -1) {
													Triple<string, GameObject, int> component = test.opVox.Type.Components.First (c => c.Item3 == componentIndex);
													//Debug.Log (ev.Replace(g.Value,component.Item2.name));
													Debug.Log (string.Format("Is {0} {1} {2}?", obj.gameObject.name, relation, component.Item2.name));
													if (TestRelation (obj.gameObject, relation, test.gameObject)) {
														string result = affStr.Affordances[h][i].Item2.Item2;

														// things are getting a little ad hoc here
														if (relation == "on") {
															if (!((Helper.GetMostImmediateParentVoxeme (test.gameObject).GetComponent<Voxeme> ().voxml.Type.Concavity == "Concave") &&
															    (Helper.FitsIn (objBounds, testBounds)))) {
																//if (obj.enabled) {
																//	obj.gameObject.GetComponent<Rigging> ().ActivatePhysics (true);
																//}
																obj.minYBound = testBounds.max.y;
															}
														}
														else if (relation == "in") {
															reactivatePhysics = false;
															obj.minYBound = objBounds.min.y;
														}

														if (result != "") {
															result = result.Replace ("x", obj.gameObject.name);
															// any component reentrancy ultimately inherits from the parent voxeme itself
															result = reentrancyForm.Replace (result, test.gameObject.name);
															result = Helper.GetTopPredicate (result);
															Debug.Log (string.Format("{0}: {1} {2}s {3}",
																affStr.Affordances[h][i].Item2.Item1,test.gameObject.name,result,obj.gameObject.name));
															relationTracker.AddNewRelation(new List<GameObject>{test.gameObject,obj.gameObject},result);

															if (result == "support") {
																RiggingHelper.RigTo (obj.gameObject, test.gameObject);
															}
															else if (result == "contain") {
																RiggingHelper.RigTo (obj.gameObject, test.gameObject);
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
			}

			if (reactivatePhysics) {
				if (obj.enabled) {
					obj.gameObject.GetComponent<Rigging> ().ActivatePhysics (true);
				}
			}
		}

		public static bool TestRelation(GameObject obj1, string relation, GameObject obj2) {
			bool r = false;
			Bounds bounds1 = Helper.GetObjectWorldSize (obj1);
			Bounds bounds2 = Helper.GetObjectWorldSize (obj2);

			Regex align = new Regex(@"align\(.+,.+\)");
			List<string> habitats = new List<string> ();
			foreach (int i in Helper.GetMostImmediateParentVoxeme(obj2).GetComponent<Voxeme>().opVox.Habitat.IntrinsicHabitats.Keys) {
				habitats.AddRange(Helper.GetMostImmediateParentVoxeme(obj2).GetComponent<Voxeme>().
					opVox.Habitat.IntrinsicHabitats [i].Where ((h => align.IsMatch (h))));
			}

			for (int i = 0; i < habitats.Count; i++) {
				habitats[i] = align.Match(habitats[i]).Value.Replace("align(","").Replace(")","").Split(',')[0];
			}

			// (default to Y-alignment if no encoding exists)
			if (habitats.Count == 0) {
				habitats.Add ("Y");
			}

			if (relation == "on") {
				foreach (string axis in habitats) {
					if ((Helper.GetMostImmediateParentVoxeme (obj2.gameObject).GetComponent<Voxeme> ().voxml.Type.Concavity == "Concave") &&
					    (Helper.FitsIn (bounds1, bounds2))) {	// if test object is concave and placed object would fit inside
						switch (axis) {
						case "X":
							r = (Vector3.Distance(
								new Vector3(obj2.gameObject.transform.position.x, obj1.gameObject.transform.position.y, obj1.gameObject.transform.position.z),
								obj2.gameObject.transform.position) <= Constants.EPSILON);
							break;

						case "Y":
							r = (Vector3.Distance(
								new Vector3(obj1.gameObject.transform.position.x, obj2.gameObject.transform.position.y, obj1.gameObject.transform.position.z),
								obj2.gameObject.transform.position) <= Constants.EPSILON);
							break;

						case "Z":
							r = (Vector3.Distance(
								new Vector3(obj1.gameObject.transform.position.x, obj1.gameObject.transform.position.y, obj2.gameObject.transform.position.z),
								obj2.gameObject.transform.position) <= Constants.EPSILON);
							break;

						default:
							break;
						}
						r &= RCC8.PO (bounds1, bounds2);
					}
					else {
						switch (axis) {
						case "X":
							r = (Vector3.Distance(
								new Vector3(obj2.gameObject.transform.position.x, obj1.gameObject.transform.position.y, obj1.gameObject.transform.position.z),
								obj2.gameObject.transform.position) <= Constants.EPSILON);
							break;

						case "Y":
							r = (Vector3.Distance(
								new Vector3(obj1.gameObject.transform.position.x, obj2.gameObject.transform.position.y, obj1.gameObject.transform.position.z),
								obj2.gameObject.transform.position) <= Constants.EPSILON);
							break;

						case "Z":
							r = (Vector3.Distance(
								new Vector3(obj1.gameObject.transform.position.x, obj1.gameObject.transform.position.y, obj2.gameObject.transform.position.z),
								obj2.gameObject.transform.position) <= Constants.EPSILON);
							break;

						default:
							break;
						}
						r &= RCC8.EC (bounds1, bounds2);
					}
				}
			}
			else if (relation == "in") {
				if (Helper.FitsIn (bounds1, bounds2)) {
					r = RCC8.PO (bounds1, bounds2) || RCC8.ProperPart (bounds1, bounds2);
				}
			}
			else if (relation == "behind") {
				r = RCC8.EC(bounds1, bounds2) || RCC8.DC(bounds1, bounds2);
			}
			else {
			}

			return r;
		}

		public static void ReevaluateRelationships(String program, GameObject obj) {
			// get object bounds
			Bounds objBounds = Helper.GetObjectWorldSize(obj);

			// get all objects
			GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

			// reasoning from habitats
			// for each object
			// for each habitat in object
			// for each affordance by habitat

			// e.g. with object obj: H->[put(x, on([1]))]support([1], x)
			//	if (program == "put" && obj is on test) then test supports obj
			//	H[2]->[put(x, in([1]))]contain(y, x)
			// if obj is in configuration [2], if (program == "put" && obj is in test) then test contains obj

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
										// **check for support configuration here
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
