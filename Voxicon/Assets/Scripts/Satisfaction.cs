using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Global;

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

			if (predString == "put") {	// satisfy put
				GameObject obj = GameObject.Find(argsStrings[0] as String);
				//Debug.Log(Helper.ConvertVectorToParsable(obj.transform.position) + " " + (String)argsStrings[1]);
				if (obj.transform.position == Helper.ParsableToVector((String)argsStrings[1])) {
					satisfied = true;
				}
			}

			return satisfied;
		}

		public static void ComputeSatisfactionConditions(String command) {
			Hashtable predArgs = Helper.ParsePredicate (command);
			String pred = Helper.GetTopPredicate (command);
			Queue<String> argsStrings = new Queue<String> (((String)predArgs [pred]).Split (new char[] {','}));
			List<object> objs = new List<object>();
			MethodInfo methodToCall;
			Predicates preds = GameObject.Find ("BehaviorController").GetComponent<Predicates> ();
			
			while (argsStrings.Count > 0) {
				object arg = argsStrings.Dequeue ();
				
				if (Helper.v.IsMatch((String)arg)) {	// if arg is vector form
					objs.Add (Helper.ParsableToVector((String)arg));
				}
				else if (arg is String) {	// if arg is String
					objs.Add (GameObject.Find (arg as String));
				}
			}

			objs.Add (false);
			methodToCall = preds.GetType ().GetMethod (pred);
			Debug.Log ("ExecuteCommand: invoke " + methodToCall.Name);
			object obj = methodToCall.Invoke (preds, new object[]{objs.ToArray ()});
		}
	}
}
