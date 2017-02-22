using UnityEngine;
using System;
using System.Collections;
using System.Timers;

using FlashbackVideoRecorder;

public class VideoAutoCapture : MonoBehaviour {
	FlashbackRecorder recorder;
	InputController inputController;
	EventManager eventManager;

	public double eventTimeoutTime = 15000.0f;
	Timer eventTimeoutTimer;

	public double eventCompleteWaitTime = 1000.0f;
	Timer eventCompleteWaitTimer;

	bool capture;

	bool capturing = false;
	bool writingFile = false;
	bool stopCaptureFlag = false;

	public event EventHandler FileWritten;

	public void OnFileWritten(object sender, EventArgs e)
	{
		if (FileWritten != null)
		{
			FileWritten(this, e);
		}
	}

	// Use this for initialization
	void Start () {
		recorder = gameObject.GetComponent<FlashbackRecorder> ();
		inputController = GameObject.Find ("IOController").GetComponent<InputController> ();
		eventManager = GameObject.Find ("BehaviorController").GetComponent<EventManager> ();

		capture = (PlayerPrefs.GetInt ("Capture Video") == 1);

		inputController.InputReceived += StartCapture;
		eventManager.QueueEmpty += EventComplete;

		eventTimeoutTimer = new Timer (eventTimeoutTime);
		eventTimeoutTimer.Enabled = false;
		eventTimeoutTimer.Elapsed += StopCapture;

		eventCompleteWaitTimer = new Timer (eventCompleteWaitTime);
		eventCompleteWaitTimer.Enabled = false;
		eventCompleteWaitTimer.Elapsed += StopCapture;
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
	}

	void StartCapture(object sender, EventArgs e) {
		if (!capture) {
			return;
		}

		Debug.Log (((InputEventArgs)e).InputString);
		eventManager.InsertEvent ("wait()", 0);
		eventTimeoutTimer.Enabled = true;

		recorder.StartCapture ();
		Debug.Log ("Starting video capture...");

		capturing = true;
		stopCaptureFlag = false;
	}

	void SaveCapture () {
		eventTimeoutTimer.Enabled = false;
		eventTimeoutTimer.Interval = eventTimeoutTime;

		eventCompleteWaitTimer.Enabled = false;
		eventCompleteWaitTimer.Interval = eventCompleteWaitTime;

		recorder.StopCapture ();
		recorder.SaveCapturedFrames ();
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
}
