using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Global;
using Satisfaction;

public class EventManager : MonoBehaviour {
	public List<String> events = new List<String>();
	public OrderedDictionary eventsStatus = new OrderedDictionary();
	string skolemized, evaluated;
	MethodInfo methodToCall;
	public Predicates preds;
	String nextQueuedEvent = "";
	int argVarIndex = 0;
	Hashtable skolems = new Hashtable();
	string argVarPrefix = @"_ARG";
	Regex r = new Regex(@".*\(.*\)");
	String nextIncompleteEvent;

	// Use this for initialization
	void Start () {
		preds = gameObject.GetComponent<Predicates> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (events.Count > 0) {
			if (SatisfactionTest.IsSatisfied (events [0]) == true) {
				GameObject.Find ("BlocksWorld").GetComponent<AStarSearch> ().plannedPath.Clear ();
				Debug.Log ("Satisfied " + events [0]);
				for (int i = 0; i < events.Count - 1; i++) {
					events [i] = events [i + 1];
				}
				events.RemoveAt (events.Count - 1);
				//Debug.Log (events.Count);

				if (events.Count > 0) {
					ExecuteNextCommand ();
				}
				else {
					OutputHelper.PrintOutput ("OK, I did it.");
				}
			}
		}
		/*if (eventsStatus.Keys.Count > 0) {
			//Debug.Log (eventsStatus.Keys.Count);
			String nextIncompleteEvent = GetNextIncompleteEvent ();
			//Debug.Log ("Next incomplete event: " + nextIncompleteEvent);
			//Debug.Log ("Next queued event: " + nextQueuedEvent);

			if (nextIncompleteEvent != "") {
				if (SatisfactionTest.IsSatisfied (nextIncompleteEvent) == true) {
					GameObject.Find ("BlocksWorld").GetComponent<AStarSearch> ().plannedPath.Clear ();
					Debug.Log ("Satisfied " + nextIncompleteEvent);
					eventsStatus[nextIncompleteEvent] = true;
					if (nextQueuedEvent != "") {
						//SatisfactionTest.ComputeSatisfactionConditions(nextQueuedEvent);
						Debug.Log ("eventsStatus.Keys.Count:" + eventsStatus.Keys.Count.ToString());
						//ExecuteCommand (nextQueuedEvent);
					}
					else {
						nextIncompleteEvent = "";
					}
				}
			}
		}*/
	}

	public void QueueEvent(String commandString) {
		// not using a queue because I'm horrible
		events.Add(commandString);
	}

	public void ExecuteNextCommand() {
		EvaluateCommand (events [0]);
		if (events.Count > 0) {
			if (SatisfactionTest.ComputeSatisfactionConditions (events [0])) {
				ExecuteCommand (events [0]);
			}
		}
	}

	public void EvaluateCommand(String command) {
		ClearRDFTriples ();
		ClearSkolems ();
		ParseCommand (command);
		FinishSkolemization ();
		skolemized = Skolemize (command);
		Debug.Log ("Skolemized command: " + skolemized);
		//EvaluateSkolemizedCommand(skolemized);
		if (EvaluateSkolemConstants ()) {
			evaluated = ApplySkolems (skolemized);
			Debug.Log ("Evaluated command: " + evaluated);
			events [events.IndexOf (command)] = evaluated;
			//nextIncompleteEvent = evaluated;
		}
		else {
			events.RemoveAt (events.Count - 1);
		}
	}

	public void ExecuteCommand(String evaluatedCommand) {
		Hashtable predArgs = Helper.ParsePredicate (evaluatedCommand);
		String pred = Helper.GetTopPredicate (evaluatedCommand);

		if (predArgs.Count > 0) {
			Queue<String> argsStrings = new Queue<String> (((String)predArgs [pred]).Split (new char[] { ',' }));
			List<object> objs = new List<object> ();
		
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
						GameObject go = GameObject.Find (arg as String);
						if (go == null) {
							OutputHelper.PrintOutput ("What is a " + "\"" + (arg as String) + "\"?");
							return;	// abort
						}
						objs.Add (GameObject.Find (arg as String));
					}
				}
			}

			objs.Add (true);
			methodToCall = preds.GetType ().GetMethod (pred.ToUpper());

			if ((methodToCall != null) &&  (preds.rdfTriples.Count > 0)) {
				Debug.Log ("ExecuteCommand: invoke " + methodToCall.Name);
				object obj = methodToCall.Invoke (preds, new object[]{ objs.ToArray () });
			}
		}
	}

	public void ClearEvents() {
		events.Clear ();
	}

	String GetNextIncompleteEvent() {
		String[] keys = new String[eventsStatus.Keys.Count];
		bool[] values = new bool[eventsStatus.Keys.Count];

		eventsStatus.Keys.CopyTo (keys,0);
		eventsStatus.Values.CopyTo (values,0);

		String nextIncompleteEvent = "";
		for (int i = 0; i < keys.Length; i++) {
			if ((bool)eventsStatus[keys[i]] == false) {
				nextIncompleteEvent = (String)keys[i];
				if (i < events.Count-1) {
					SatisfactionTest.ComputeSatisfactionConditions(events[i+1]);
					eventsStatus.Keys.CopyTo (keys,0);
					eventsStatus.Values.CopyTo (values,0);
					nextQueuedEvent = (String)keys[i+1];
				}
				else {
					nextQueuedEvent = "";
					//put(spoon,on(mug));put(apple,on(plate))
					//put(block1,on(block2));put(block3,on(block1))
				}
				break;
			}
		}

		return nextIncompleteEvent;

		/*SatisfactionTest.ComputeSatisfactionConditions(events[eventsStatus.Keys.Count-1]);

		eventsStatus.Keys.CopyTo (keys,0);
		eventsStatus.Values.CopyTo (values,0);

		String nextIncompleteEvent = keys [keys.Length - 1];

		if (events.Count > eventsStatus.Keys.Count) {
			nextQueuedEvent = SatisfactionTest.ComputeSatisfactionConditions(events[eventsStatus.Keys.Count]);

			eventsStatus.Keys.CopyTo (keys,0);
			eventsStatus.Values.CopyTo (values,0);
			
			nextQueuedEvent = keys [keys.Length - 1];
		}
		else {
			nextQueuedEvent = "";
		}

		return nextIncompleteEvent;*/
	}

	public void ClearSkolems() {
		argVarIndex = 0;
		skolems.Clear ();
	}

	public void ClearRDFTriples () {
		preds.rdfTriples.Clear ();
	}

	public void ParseCommand(String command) {
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

	public void FinishSkolemization() {
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

	public String Skolemize(String inString) {
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

	public String ApplySkolems(String inString) {
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

	public bool EvaluateSkolemConstants() {
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

						if (Helper.v.IsMatch ((String)arg)) {	// if arg is vector form
							objs.Add (Helper.ParsableToVector ((String)arg));
						} else if (arg is String) {	// if arg is String
							if ((arg as String).Count (f => f == '(') +	// not a predicate
							    (arg as String).Count (f => f == ')') == 0) {
								if (preds.GetType ().GetMethod (pred.ToUpper ()).ReturnType != typeof(String)) {	// if predicate not going to return string (as in "AS")
									GameObject go = GameObject.Find (arg as String);
									if (go == null) {
										OutputHelper.PrintOutput ("What is a " + "\"" + (arg as String) + "\"?");
										return false;	// abort
									}
								}
							}

							Regex q = new Regex("\".*\"");
							if (q.IsMatch(arg as String)) {
								objs.Add (arg);
							}
							else {
								objs.Add (GameObject.Find (arg as String));
							}
						}
					}
					methodToCall = preds.GetType ().GetMethod (pred.ToUpper());

					if (methodToCall == null) {
						OutputHelper.PrintOutput ("Sorry, what does " + "\"" + pred + "\" mean?");
						return false;
					}

					Debug.Log ("EvaluateSkolemConstants: invoke " + methodToCall.Name);
					object obj = methodToCall.Invoke (preds, new object[]{ objs.ToArray () });
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

		return true;
	}
}
