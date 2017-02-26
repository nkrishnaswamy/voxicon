using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Timers;

using Global;
using FlashbackVideoRecorder;
using SQLite4Unity3d;

namespace VideoCapture {
	public enum VideoCaptureMode {
		PerEvent,
		FullTime,
		Manual
	};

	public enum VideoCaptureFilenameType {
		FlashbackDefault,
		EventString,
		Custom
	};

	public class VideoDBEntry  {
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }
		public string Filename { get; set; }
		public string InputString { get; set; }
		public string Parse { get; set; }
		public string EventPredicate { get; set; }
		public byte[] Objects { get; set; }
		public byte[] ParameterValues { get; set; }
		//public Dictionary<string,string> ParameterValues { get; set; }

		public override string ToString ()
		{
			string head = string.Format ("[VideoDBEntry: Id={0}," +
				" Filename={1}," +
				" InputString={2}," +
				" Parse={3}," +
				" Predicate={4},", Id, Filename, InputString, Parse, EventPredicate);
			string valueContent = "";
			string tail = "]";
			return head + valueContent + tail;
		}
	}

	public class VideoAutoCapture : MonoBehaviour {
		public KeyCode startCaptureKey;
		public KeyCode stopCaptureKey;

		FlashbackRecorder recorder;
		InputController inputController;
		EventManager eventManager;
		PluginImport commBridge;

		public double eventTimeoutTime = 15000.0f;
		Timer eventTimeoutTimer;

		public double eventCompleteWaitTime = 1000.0f;
		Timer eventCompleteWaitTimer;

		bool capture;

		bool capturing = false;
		bool writingFile = false;
		bool stopCaptureFlag = false;

		VideoCaptureMode captureMode;
		VideoCaptureFilenameType filenameScheme;
		string filenamePrefix;
		string dbFile;

		SQLiteConnection dbConnection;
		VideoDBEntry dbEntry;
		string outFileName = string.Empty;

		public event EventHandler FileWritten;

		public void OnFileWritten(object sender, EventArgs e)
		{
			if (FileWritten != null)
			{
				FileWritten(this, e);
			}
		}

		int eventIndex = 0;

		// Use this for initialization
		void Start () {
			recorder = gameObject.GetComponent<FlashbackRecorder> ();
			inputController = GameObject.Find ("IOController").GetComponent<InputController> ();
			eventManager = GameObject.Find ("BehaviorController").GetComponent<EventManager> ();
			commBridge = GameObject.Find ("CommunicationsBridge").GetComponent<PluginImport> ();

			capture = (PlayerPrefs.GetInt ("Capture Video") == 1);
			captureMode = (VideoCaptureMode)PlayerPrefs.GetInt ("Video Capture Mode");
			filenameScheme = (VideoCaptureFilenameType)PlayerPrefs.GetInt ("Video Capture Filename Type");
			filenamePrefix = PlayerPrefs.GetString ("Custom Video Filename Prefix");
			dbFile = PlayerPrefs.GetString ("Video Capture DB");

			if (captureMode == VideoCaptureMode.PerEvent) {
				inputController.InputReceived += StartCapture;
				inputController.InputReceived += InputStringReceived;
				inputController.ParseComplete += ParseReceived;

				eventManager.QueueEmpty += EventComplete;

				eventTimeoutTimer = new Timer (eventTimeoutTime);
				eventTimeoutTimer.Enabled = false;
				eventTimeoutTimer.Elapsed += StopCapture;

				eventCompleteWaitTimer = new Timer (eventCompleteWaitTime);
				eventCompleteWaitTimer.Enabled = false;
				eventCompleteWaitTimer.Elapsed += StopCapture;

				FileWritten += CaptureComplete;
			}
		}
		
		// Update is called once per frame
		void Update () {
			if (stopCaptureFlag) {
				SaveCapture ();
				stopCaptureFlag = false;
			}

			if ((writingFile) && (recorder.GetNumberOfPendingFiles () == 0)) {
				Debug.Log ("File written to disk.");
				OnFileWritten (this, null);
				writingFile = false;
			}

			if (captureMode == VideoCaptureMode.Manual) {
				if ((!capturing) && (!writingFile)) {
					if (Input.GetKeyDown (startCaptureKey)) {
						StartCapture (null, null);
					}
				}

				if (!writingFile) {
					if (Input.GetKeyDown (stopCaptureKey)) {
						StopCapture(null, null);
					}
				}
			}
		}

		void StartCapture(object sender, EventArgs e) {
			if (!capture) {
				return;
			}

			if (filenameScheme == VideoCaptureFilenameType.EventString) {
				outFileName = string.Format ("{0}-{1}", (((InputEventArgs)e).InputString).Replace (" ", "_"), DateTime.Now.ToString ("yyyy-MM-dd-HHmmss"));
			}
			else {
				outFileName = string.Format ("{0}-{1}", filenamePrefix, DateTime.Now.ToString ("yyyy-MM-dd-HHmmss"));
			}

			if (dbFile != string.Empty) {
				OpenDB ();
				dbEntry = new VideoDBEntry ();
				dbEntry.Filename = outFileName;
			}

			if (captureMode == VideoCaptureMode.PerEvent) {
				eventManager.InsertEvent ("wait()", 0);
				eventTimeoutTimer.Enabled = true;
			}

			recorder.StartCapture ();
			Debug.Log ("Starting video capture...");

			capturing = true;
			stopCaptureFlag = false;
		}

		void SaveCapture () {
			if (captureMode == VideoCaptureMode.PerEvent) {
				eventTimeoutTimer.Enabled = false;
				eventTimeoutTimer.Interval = eventTimeoutTime;

				eventCompleteWaitTimer.Enabled = false;
				eventCompleteWaitTimer.Interval = eventCompleteWaitTime;
			}

			recorder.StopCapture ();

			if (filenameScheme == VideoCaptureFilenameType.FlashbackDefault) {
				recorder.SaveCapturedFrames ();

				if (dbFile != string.Empty) {
					dbEntry.Filename = "Flashback_" + DateTime.Now.ToString ("yyyy-MM-dd-HHmmss");
				}
			}
			else {
				recorder.SaveCapturedFrames (outFileName);
			}

			if (dbFile != string.Empty) {
				WriteToDB ();
			}

			capturing = false;
			writingFile = true;

			Debug.Log ("Stopping video capture.");
		}

		void StopCapture(object sender, EventArgs e) {
			if (capturing) {
				stopCaptureFlag = true;
			}
		}

		void EventComplete(object sender, EventArgs e) {
			if (capturing) {
				eventCompleteWaitTimer.Enabled = true;
			}
		}

		void InputStringReceived(object sender, EventArgs e) {
			dbEntry.InputString = ((InputEventArgs)e).InputString;
		}

		void ParseReceived(object sender, EventArgs e) {
			dbEntry.Parse = ((InputEventArgs)e).InputString;
			dbEntry.EventPredicate = Helper.GetTopPredicate (dbEntry.Parse);

			Regex r = new Regex (@"\([^()]*\)");

			MatchCollection matches = r.Matches (dbEntry.Parse);
			List<String> argsStrings = matches.Cast<Match> ().Select (m => m.Value.Replace("(","").Replace(")","")).ToList ();

			dbEntry.Objects = Helper.SerializeObject (argsStrings);
		}

		void ParametersCalculated(object sender, EventArgs e) {
		}

		void CaptureComplete(object sender, EventArgs e) {
			if (eventIndex != -1) {
				string[] args = new string[]{ eventIndex.ToString (), PlayerPrefs.GetString ("Listener Port") };
				string result = Marshal.PtrToStringAuto (PluginImport.PythonCall (Application.dataPath + "/Externals/python/", "auto_event_script", "send_next_event_to_port", args, args.Length));
				Debug.Log (result);
				eventIndex = System.Convert.ToInt32 (result);
			}
		}

		void OpenDB() {
			dbConnection = new SQLiteConnection (string.Format ("{0}.db", dbFile),
				SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

			dbConnection.CreateTable<VideoDBEntry> ();
		}

		void WriteToDB() {
			dbConnection.InsertAll (new[]{ dbEntry });
		}
	}
}