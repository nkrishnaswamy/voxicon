using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Global;
using Satisfaction;

public class InputController : MonoBehaviour {
	public String inputString;
	String[] commands;
	List<String> evaluatedCommands = new List<String>();
	Regex r = new Regex(@".*\(.*\)");
	string argVarPrefix = @"_ARG";
	int argVarIndex = 0;
	Hashtable skolems = new Hashtable();
	string skolemized, evaluated;
	MethodInfo methodToCall;
	Predicates preds;
	EventManager eventManager;
	Macros macros;

	void Start() {
		GameObject bc = GameObject.Find ("BehaviorController");
		preds = bc.GetComponent<Predicates> ();
		eventManager = bc.GetComponent<EventManager> ();
		macros = bc.GetComponent<Macros> ();
	}

	void Update() {
	}

	void OnGUI() {
		Event e = Event.current;
		if (e.keyCode == KeyCode.Return) {
			if (inputString != "") {
				if (inputString.Count (x => x == '(') == inputString.Count (x => x == ')')) {
					eventManager.ClearEvents ();
					foreach (KeyValuePair<String,String> kv in macros.commandMacros) {
						if (inputString == kv.Key) {
							inputString = kv.Value;
							break;
						}
					}
					Debug.Log ("User entered: " + inputString);
					commands = inputString.Split (';');
					//String commandString = inputString;
					foreach (String commandString in commands) {
						ClearRDFTriples ();
						ClearSkolems ();
						ParseCommand (commandString);
						FinishSkolemization ();
						skolemized = Skolemize (commandString);
						Debug.Log ("Skolemized command: " + skolemized);
						//EvaluateSkolemizedCommand(skolemized);
						EvaluateSkolemConstants ();
						evaluated = ApplySkolems (skolemized);
						Debug.Log ("Evaluated command: " + evaluated);
						eventManager.events.Add (evaluated);
					}

					//foreach (String ev in eventManager.events) {
					SatisfactionTest.ComputeSatisfactionConditions (eventManager.events [0]);
					//}

					eventManager.ExecuteNextCommand ();
				}
				inputString = GUI.TextField (new Rect (0, 0, 600, 25), "");
			}
		}
		else
			inputString = GUI.TextField (new Rect (0, 0, 600, 25), inputString);
	}

	void ClearSkolems() {
		argVarIndex = 0;
		skolems.Clear ();
	}

	void ClearRDFTriples () {
		preds.rdfTriples.Clear ();
	}

	void ParseCommand(String command) {
		Hashtable predArgs;
		String predString = null;
		Queue<String> argsStrings = null;

		if (r.IsMatch (command)) {	// if command matches predicate form
			//Debug.Log ("ParseCommand: " + command);
			Triple<String,String,String> triple = Helper.MakeRDFTriples(command);
			if (triple.Item1 != "" && triple.Item2 != "" && triple.Item3 != "") {
				preds.rdfTriples.Add(triple);
				Helper.PrintRDFTriples(preds.rdfTriples);
			}
			else {
				Debug.Log ("Failed to make RDF triple");
			}
			predArgs = Helper.ParsePredicate(command);
			foreach (DictionaryEntry entry in predArgs) {
				predString = (String)entry.Key;
				argsStrings = new Queue<String>(((String)entry.Value).Split (new char[] {','}));

				StringBuilder sb = new StringBuilder("[");
				foreach(String arg in argsStrings) {
					sb.Append (arg + ",");
				}
				sb.Remove(sb.Length-1,1);
				sb.Append("]");
				String argsList = sb.ToString();
				//Debug.Log(predString + " : " + argsList);

				for(int i = 0; i < argsStrings.Count; i++) {
					if (r.IsMatch (argsStrings.ElementAt (i))) {
						String v = argVarPrefix+argVarIndex.ToString();
						skolems[v] = argsStrings.ElementAt (i);
						//Debug.Log (v + " : " + skolems[v]);
						argVarIndex++;

						sb = new StringBuilder(sb.ToString());
						foreach(DictionaryEntry kv in skolems) {
							argsList = argsList.Replace((String)kv.Value, (String)kv.Key);
							//Debug.Log(predString + " : " + argsList);
							//foreach(KeyValuePair<String,String> kkv in skolems) {
							//	if (kkv.Key != kv.Key) {
							//		skolems[kkv.Key] = kkv.Value.Replace(kv.Value, kv.Key);
							//	}
							//}
							//move(mug,to(center(square_table)))
							//move(mug,ARG0)
							//move(mug,to(ARG1))
						}
						//Debug.Log(predString + " : " + argsList);

					}
					ParseCommand (argsStrings.ElementAt (i));
				}
			}
		}
		
		//if (r.IsMatch(arguments))	Debug.Log (arguments);
		//Debug.Log (predicate + ' ' + arguments);

		/*GameObject bc = GameObject.Find ("BehaviorController");
		List<GameObject> objs = new List<GameObject>();
		while (argsStrings.Count > 0) {
			objs.Add(GameObject.Find(argsStrings.Dequeue()));
		}

		Predicates preds = bc.GetComponent<Predicates> ();
		MethodInfo method = preds.GetType().GetMethod(predString);
		object obj = method.Invoke(preds, new object[]{objs.ToArray<GameObject>()});
		Debug.Log (obj);*/
	}

	void FinishSkolemization() {
		Hashtable temp = new Hashtable ();

		foreach (DictionaryEntry kv in skolems) {
			foreach (DictionaryEntry kkv in skolems) {
				if (kkv.Key != kv.Key) {
					//Debug.Log ("FinishSkolemization: "+kv.Key+ " " +kkv.Key);
					if (((String)kkv.Value).Contains((String)kv.Value)) {
						//Debug.Log ("FinishSkolemization: "+kv.Value + " found in " + kkv.Value);
						//Debug.Log ("FinishSkolemization: "+kkv.Key + " : " + kkv.Value.Replace(kv.Value, kv.Key));
						temp [kkv.Key] = ((String)kkv.Value).Replace((String)kv.Value, (String)kv.Key);
					}
				}
			}
		}

		foreach (DictionaryEntry kv in temp) {
			skolems[kv.Key] = temp[kv.Key];
		}

		Helper.PrintKeysAndValues(skolems);
	}

	String Skolemize(String inString) {
		String outString = inString;
		String temp = inString;

		int parenCount = temp.Count(f => f == '(') + 
							temp.Count(f => f == ')');
		//Debug.Log ("Skolemize: parenCount = " + parenCount.ToString ());

		do{
			foreach (DictionaryEntry kv in skolems) {
				outString = (String)outString.Replace((String)kv.Value,(String)kv.Key);
				//Debug.Log (outString);
			}
			temp = outString;
			parenCount = temp.Count(f => f == '(') + 
							temp.Count(f => f == ')');
			//Debug.Log ("Skolemize: parenCount = " + parenCount.ToString ());
			//move(mug,from(edge(table)),to(edge(table)))
		}while(parenCount > 2);

		return outString;
	}

	String ApplySkolems(String inString) {
		String outString = inString;
		String temp = inString;
		
		int parenCount = temp.Count(f => f == '(') + 
			temp.Count(f => f == ')');
		//Debug.Log ("Skolemize: parenCount = " + parenCount.ToString ());
		
		//do{
			foreach (DictionaryEntry kv in skolems) {
				if (kv.Value is Vector3) {
					outString = (String)outString.Replace((String)kv.Key,Helper.VectorToParsable((Vector3)kv.Value));
					//Debug.Log (outString);
				}
				else {
					outString = (String)outString.Replace((String)kv.Key,(String)kv.Value);
				}
			}
			temp = outString;
			parenCount = temp.Count(f => f == '(') + 
				temp.Count(f => f == ')');
			//Debug.Log ("Skolemize: parenCount = " + parenCount.ToString ());
			//move(mug,from(edge(table)),to(edge(table)))
		//}while(parenCount > 6);
		
		return outString;
	}

	void EvaluateSkolemConstants() {
		Hashtable temp = new Hashtable ();
		Regex regex = new Regex (argVarPrefix+@"[0-9]+");
		Match argsMatch;
		Hashtable predArgs;
		List<object> objs = new List<object>();
		Queue<String> argsStrings;

		foreach (DictionaryEntry kv in skolems) {
			objs.Clear ();
			if (kv.Value is String) {
				argsMatch = regex.Match ((String)kv.Value);
				if (argsMatch.Groups [0].Value.Length == 0) {	// matched an empty string = no match
					Debug.Log (kv.Value);
					predArgs = Helper.ParsePredicate ((String)kv.Value);
					String pred = Helper.GetTopPredicate ((String)kv.Value);
					argsStrings = new Queue<String> (((String)predArgs [pred]).Split (new char[] {','}));
					while (argsStrings.Count > 0) {
						object arg = argsStrings.Dequeue ();

						if (Helper.v.IsMatch((String)arg)) {	// if arg is vector form
							objs.Add (Helper.ParsableToVector((String)arg));
						}
						else if (arg is String) {	// if arg is String
							objs.Add (GameObject.Find (arg as String));
						}
					}
					methodToCall = preds.GetType ().GetMethod (pred);
					Debug.Log ("EvaluateSkolemConstants: invoke " + methodToCall.Name);
					object obj = methodToCall.Invoke (preds, new object[]{objs.ToArray ()});
					Debug.Log (obj);
					temp [kv.Key] = obj;
				}
				else {
					temp [kv.Key] = skolems [kv.Key];
				}
			}
		}

		foreach (DictionaryEntry kv in temp) {
			//Debug.Log (kv.Value);
			String matchVal = kv.Value as String;
			if (matchVal == null) {
				matchVal = @"DEADBEEF";
			}
			argsMatch = regex.Match (matchVal);
			if (argsMatch.Groups [0].Value.Length > 0) {
				Debug.Log (argsMatch.Groups [0]);
				if (temp.ContainsKey (argsMatch.Groups [0].Value)) {
					object replaceWith = temp [(String)argsMatch.Groups [0].Value];
					Debug.Log (replaceWith.GetType ());
					//String replaced = ((String)skolems [kv.Key]).Replace ((String)argsMatch.Groups [0].Value,
					//	replaceWith.ToString ().Replace (',', ';').Replace ('(', '<').Replace (')', '>'));
					String replaced = ((String)skolems [kv.Key]).Replace ((String)argsMatch.Groups [0].Value, Helper.VectorToParsable((Vector3)replaceWith));
					Debug.Log (replaced);
					//if (replace is Vector3) {
					skolems [kv.Key] = replaced;
					//}
				}
			} else {
				skolems [kv.Key] = temp [kv.Key];
			}
		}

		Helper.PrintKeysAndValues(skolems);

		int newEvaluations = 0;
		foreach (DictionaryEntry kv in skolems) {
			//Debug.Log(kv.Key + " : " + kv.Value.GetType());
			if (kv.Value is String) {
				argsMatch = r.Match ((String)kv.Value);

				if (argsMatch.Groups [0].Value.Length > 0) {
					newEvaluations++;
				}
			}
		}

		//Debug.Log (newEvaluations);
		if (newEvaluations > 0)
			EvaluateSkolemConstants ();
	}
}
