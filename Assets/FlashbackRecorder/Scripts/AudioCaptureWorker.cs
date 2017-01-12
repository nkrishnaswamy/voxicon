/*
 * Flashback Video Recorder
 * AudioCaptureWorker.cs v1.2
 * 
 * Stores and writes any captured audio to disk for use in MP4 file creation.
 * 
 * Copyright 2016 LaunchPoint Games
 * One license per seat. For all terms and conditions, see included 
 * documentation, or visit http://www.launchpointgames.com/unity/flashback.html
 */

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace FlashbackVideoRecorder{

	public class AudioCaptureWorker {

		//True if audio is being captured
		private bool m_captureAudio = true;

		//Used to generate the output file
		private int m_outputRate = 48000;
		private int m_headerSize = 44; //default for uncompressed wav

		//Used to store audio data in real-time
		private double lastTime = 0.0f;
		private float delta = 40;

		//How much audio history to capture
		float m_captureTime;

		//Stores the audio in a queue
		Queue<float[]> m_audioQueue;


		//Constructor creates the queue
		public AudioCaptureWorker(float captureTime){
			m_captureTime = captureTime;

			AudioConfiguration c = AudioSettings.GetConfiguration ();
			m_outputRate = c.sampleRate;

			m_audioQueue = new Queue<float[]> ();
		}


		//Turn on/off audio capture
		public void CaptureAudio(bool capture){
			m_captureAudio = capture;
		}


		//Clears the audio queue
		public void ClearAudioData(){
			if (m_audioQueue != null)
				m_audioQueue.Clear ();
		}


		//Takes in the audio data and stores it in the queue
		public void CacheAudioData(float[] data, int channels) {

			delta = (float)(1.0f / (AudioSettings.dspTime - lastTime));
			lastTime = AudioSettings.dspTime;

			if(m_captureAudio) {
				while (m_audioQueue.Count > Mathf.CeilToInt(m_captureTime * delta))
					m_audioQueue.Dequeue ();

				float[] mydata = new float[data.Length];
				Array.Copy (data, mydata, data.Length);
				m_audioQueue.Enqueue (mydata);
			}
		}


		//Saves the audio data to a file
		//Returns the file name
		public string WriteAudioToFile(string path){

			if (m_audioQueue == null || m_audioQueue.Count == 0)
				return null;

			float[][] audio = m_audioQueue.ToArray ();

			if (!Directory.Exists (path + "~"))
				Directory.CreateDirectory (path + "~");

			string filePath = path + "~/audio.tmp";

			FileStream fs = GenerateFileStream (filePath);
			WriteContent (fs, audio);
			WriteHeader (fs);

			return filePath;
		}


		//Prepare the filestream and allocate space for the header
		private FileStream GenerateFileStream(String name) {
			FileStream fileStream = new FileStream(name, FileMode.Create);
			byte emptyByte = new byte();

			for(int i = 0; i < m_headerSize; i++) //preparing the header
			{
				fileStream.WriteByte(emptyByte);
			}

			return fileStream;
		}


		//Writes each file chunk to the file stream
		private void WriteContent(FileStream fileStream, float[][] audio){
			
			for (int i = 0; i < audio.GetLength(0); i++){
				WriteAudioBlock (fileStream, audio [i]);
			}
		}


		//Writes a single chunk of data to the filestream
		void WriteAudioBlock(FileStream fileStream, float[] dataSource) {

			Int16[] intData = new Int16[dataSource.Length];
			//converting in 2 steps : float[] to Int16[], //then Int16[] to Byte[]

			Byte[] bytesData = new Byte[dataSource.Length*2];
			//bytesData array is twice the size of
			//dataSource array because a float converted in Int16 is 2 bytes.

			int rescaleFactor = 32767; //to convert float to Int16

			for (int i = 0; i < dataSource.Length; i++) {
				intData[i] = (Int16)(dataSource[i]*rescaleFactor);
				Byte[] byteArr = new Byte[2];
				byteArr = BitConverter.GetBytes(intData[i]);
				byteArr.CopyTo(bytesData,i*2);
			}

			fileStream.Write(bytesData,0,bytesData.Length);
		}


		//Writes the file header to disk after the file properties have been calculated
		void WriteHeader(FileStream fileStream) {

			fileStream.Seek(0, SeekOrigin.Begin);

			Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
			fileStream.Write(riff,0,4);

			Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length-8);
			fileStream.Write(chunkSize,0,4);

			Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
			fileStream.Write(wave,0,4);

			Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
			fileStream.Write(fmt,0,4);

			Byte[] subChunk1 = BitConverter.GetBytes(16);
			fileStream.Write(subChunk1,0,4);

			UInt16 two = 2;
			UInt16 one = 1;

			Byte[] audioFormat = BitConverter.GetBytes(one);
			fileStream.Write(audioFormat,0,2);

			Byte[] numChannels = BitConverter.GetBytes(two);
			fileStream.Write(numChannels,0,2);

			Byte[] sampleRate = BitConverter.GetBytes(m_outputRate);
			fileStream.Write(sampleRate,0,4);

			Byte[] byteRate = BitConverter.GetBytes(m_outputRate*4);

			fileStream.Write(byteRate,0,4);

			UInt16 four = 4;
			Byte[] blockAlign = BitConverter.GetBytes(four);
			fileStream.Write(blockAlign,0,2);

			UInt16 sixteen = 16;
			Byte[] bitsPerSample = BitConverter.GetBytes(sixteen);
			fileStream.Write(bitsPerSample,0,2);

			Byte[] dataString = System.Text.Encoding.UTF8.GetBytes("data");
			fileStream.Write(dataString,0,4);

			Byte[] subChunk2 = BitConverter.GetBytes(fileStream.Length-m_headerSize);
			fileStream.Write(subChunk2,0,4);

			fileStream.Close();
		}
	}
}