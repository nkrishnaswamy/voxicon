/*
 * Flashback Video Recorder
 * VideoCaptureWorker.cs
 * 
 * Creates a new thread that first writes a temporary file to disk containing raw video,
 * then executes FFmpeg, which encodes the raw video data to the formats specified. 
 * This will work on Windows and Mac desktop platforms (standalone or in editor).
 * 
 * 
 * Copyright 2016 LaunchPoint Games
 * One license per seat. For all terms and conditions, see included 
 * documentation, or visit http://www.launchpointgames.com/unity/flashback.html
 */


using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace FlashbackVideoRecorder{

	internal class VideoCaptureWorker {

		//The thread that will launch FFmpeg executable
		Thread m_thread;

		//Stores the FFmpeg directory and filename
		string m_ffmpegFilename = "";

		//The input and output file paths
		string m_sourceFilePath = "";			//Temp file path = "m_baseFilePath/video.tmp"
		string m_gifFilePath = "";				//GIF file path = "m_baseFilePath/temp.gif"
		string m_mp4FilePath = "";				//MP4 file path = "m_baseFilePath/temp.mp4"
		string m_oggFilePath = "";				//OGG file path = "m_baseFilePath/temp.ogg"
		string m_paletteFilePath = "";			//Temporary file used to store the GIF color palette

		//Variables set by the FlashbackRecorder class
		internal byte[][] m_frames;				//The byte texture associated with each frame
		internal string m_filedir;				//The output directory
		internal int m_inputWidth;				//Resolution width of the captured video
		internal int m_inputHeight;				//Resolution height of the captured video
		internal int m_outputWidth;				//Resolution width of video output
		internal int m_outputHeight;			//Resolution height of video output
		internal int m_framerate;				//Frames per second
		internal bool m_outputToGif;			//If true, will output a GIF file
		internal bool m_outputToMp4;			//If true, will output an MP4 file
		internal bool m_outputToOog;			//If true, will output to an OGG file
		internal string m_audioFile;			//Path to associated audio file
		internal FlashbackRecorder m_recorder;	//The recorder that started this process
		internal Watermark[] m_watermarks;


		internal VideoCaptureWorker(){

			switch (Application.platform) {

			//On OSX, launch the Mac specific version of the executable
			//The ".exe" extension helps ensure campatibility
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXPlayer:
				m_ffmpegFilename = FlashbackRecorder.FFmpegMac;
				break;

				//On Windows, launch the PC specific version of the executable
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsPlayer:
				m_ffmpegFilename = FlashbackRecorder.FFmpegWin;
				break;

			default:
				m_ffmpegFilename = "";
				break;
			}

			//When the thread starts, execute the "Run" method
			m_thread = new Thread (Run);
		}


		//Makes sure the directory structure is configured correctly and starts the thread
		internal void WriteToDisk(){
			if (!Directory.Exists (m_filedir + "~"))
				Directory.CreateDirectory (m_filedir + "~");

			m_sourceFilePath = m_filedir + "~/video.tmp";

			m_gifFilePath = m_filedir + "~/temp.gif";
			DeleteExistingFile (m_gifFilePath);

			m_mp4FilePath = m_filedir + "~/temp.mp4";
			DeleteExistingFile (m_mp4FilePath);

			m_oggFilePath = m_filedir + "~/temp.ogg";
			DeleteExistingFile (m_oggFilePath);

			m_paletteFilePath = m_filedir + "~/palette.png";
			DeleteExistingFile (m_paletteFilePath);

			m_thread.Priority = System.Threading.ThreadPriority.AboveNormal;
			m_thread.Start ();
		}


		//If the file exists, delete it from disk
		void DeleteExistingFile(string filename){
			if (filename != null && File.Exists (filename)) {
				File.Delete (filename);
			}
		}


		//Writes a temporary file with the frame data, then executes the appropriate FFmpeg commands 
		//to generate the specified output files
		void Run(){

			//Write the frame data into a temporary file
			FileStream h264stream = new FileStream(m_sourceFilePath, FileMode.Create, FileAccess.Write);
			foreach (byte[] source in m_frames) {
				//Flip each frame vertically, otherwise FFmpeg will write each frame "upside down"
				byte[] frameData = FlipVertical (source );
				h264stream.Write (frameData, 0, frameData.Length);

				//Sleep between each frame
				Thread.Sleep (1);
			}

			h264stream.Close();

			RunFfmpegProcess (GetMp4Arguments (m_sourceFilePath, m_mp4FilePath));

			string tmp = m_filedir + "~/temp_1.mp4";
			foreach(Watermark m in m_watermarks){
				
				RunFfmpegProcess(GetWatermarkArguments(m_mp4FilePath, tmp, m.GetImagePath(), m.GetHorizontalString(), m.GetVerticalString()));
				Thread.Sleep (1);
				File.Delete (m_mp4FilePath);
				File.Move (tmp, m_mp4FilePath);
			}

			DeleteExistingFile (tmp);
				
			//If saving to a GIF, execute the FFmpeg process with the appropriate parameters
			if (m_outputToGif) {
				RunFfmpegProcess(GetPaletteArguments(m_mp4FilePath, m_paletteFilePath));
				RunFfmpegProcess (GetGifArguments (m_mp4FilePath, m_paletteFilePath, m_gifFilePath));

				File.Move (m_gifFilePath, m_filedir + ".gif");
				if(m_recorder != null)
					m_recorder.OnFileCreated (m_filedir + ".gif");
			}

			if (m_outputToOog) {
				RunFfmpegProcess (GetOggArguments (m_mp4FilePath, m_oggFilePath));
				File.Move (m_oggFilePath, m_filedir + ".ogg");
				if(m_recorder != null)
					m_recorder.OnFileCreated (m_filedir + ".ogg");
			}

			//If saving to an MP4, rename the temp file to the final name
			if (m_outputToMp4) {
				File.Move (m_mp4FilePath, m_filedir + ".mp4");

				if (m_recorder != null)
					m_recorder.OnFileCreated (m_filedir + ".mp4");
			} else {
				DeleteExistingFile (m_mp4FilePath);
			}


			//Delete the temporary output
			if(Directory.Exists(m_filedir + "~"))
				Directory.Delete(m_filedir + "~", true);
		}


		//Producing a high quality GIF is a two step process. First we need to generate a color palette
		string GetPaletteArguments(string source, string png){
			string arguments = "";

			arguments += string.Format("-r {0} ", m_framerate);
			arguments += string.Format("-i \"{0}\" -vf palettegen \"{1}\"", source, png);

			return arguments;
		}


		//Use the raw data and the palette to produce the GIF
		string GetGifArguments(string source, string palette, string dest){
			string arguments = "";

			arguments += string.Format("-i \"{0}\" ", source);
			arguments += string.Format("-i \"{0}\" ", palette);
			arguments += "-an ";
			arguments += "-loop 0 ";
			arguments += string.Format("-lavfi \"scale={0}:{1}:flags=lanczos,paletteuse=dither=bayer:bayer_scale=4\" ", m_outputWidth, m_outputHeight);
			arguments += "-y ";
			arguments += string.Format("\"{0}\" ", dest);

			return arguments;
		}


		//MP4 specific parameters for FFmpeg
		string GetMp4Arguments(string source, string dest){
			string arguments = "";

			arguments += "-f rawvideo ";
			arguments += string.Format ("-s {0}x{1} ", m_inputWidth, m_inputHeight);
			arguments += "-pix_fmt rgb24 ";
			arguments += string.Format("-r {0} ", m_framerate);
			arguments += string.Format("-i \"{0}\" ", source);

			if (m_audioFile == null)
				arguments += "-an ";
			else
				arguments += string.Format ("-i \"{0}\" -c:a ac3 -shortest ", m_audioFile);
					
			arguments += "-pix_fmt yuv420p ";
			arguments += string.Format("-vf scale={0}:{1} ", m_outputWidth, m_outputHeight);
			arguments += "-nostdin ";
			arguments += "-y ";
			arguments += string.Format("\"{0}\"", dest);

			return arguments;
		}


		string GetOggArguments(string source, string dest){
			string arguments = "";

			arguments += string.Format("-i \"{0}\" ", source);
			arguments += "-acodec libvorbis ";
			arguments += string.Format("\"{0}\"", dest);


			return arguments;
		}


		//Allows image overlay on 
		string GetWatermarkArguments(string source, string video, string image, string horizontal, string vertical){
			string arguments = "";

			arguments += string.Format("-i \"{0}\" ", source);
			arguments += string.Format ("-i \"{0}\" ", image);
			arguments += string.Format ("-filter_complex \"overlay={0}:{1}\" ", horizontal, vertical);
			arguments += string.Format("-y \"{0}\" ", video);

			return arguments;
		}


		//Spawns the FFmpeg process (ensuring the window is hidden)
		string RunFfmpegProcess(string arguments){

			string ffmpeg = FlashbackRecorder.FFmpegStandaloneDir() + m_ffmpegFilename;
			#if UNITY_EDITOR
			ffmpeg = FlashbackRecorder.FFmpegPackageDir() + m_ffmpegFilename;
			#endif

			Process process = new Process ();
			process.StartInfo = new ProcessStartInfo (ffmpeg, arguments);
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardError = true;
			process.Start ();

			StreamReader reader = process.StandardError;
			String output = reader.ReadToEnd ();

			while (!process.HasExited) {
				Thread.Sleep (100);
			}

			reader.Close ();
			process.Close ();

			return output;
		}


		//Takes a byte array and flips it vertically because Unity and FFmpeg store frame data in opposite directions
		byte[] FlipVertical(byte[] textureBuffer){

			//Based on the height and number of bytes, determine each row width (each pixel will be stored in 3 bytes)
			int rowWidth = textureBuffer.Length / m_inputHeight;

			//Start at the top and bottom of the array, swap their values, then step towards the middle of the array
			//Stop when we hit the halfway point (otherwise we would undo the process)
			for (int i = 0; i < m_inputHeight / 2; i++) {

				//Store the "upper" and "lower" rows
				byte[] urow = new byte[rowWidth];
				byte[] lrow = new byte[rowWidth];

				//Copy the values from the lower row into the upper row
				Array.Copy (textureBuffer, i * rowWidth, urow, 0, rowWidth);
				Array.Copy (textureBuffer, (m_inputHeight - i - 1) * rowWidth, lrow, 0, rowWidth);

				//Copy the values from the upper row to the lower row
				Array.Copy (urow, 0, textureBuffer, (m_inputHeight - i - 1) * rowWidth, rowWidth);
				Array.Copy (lrow, 0, textureBuffer, i * rowWidth, rowWidth);
			}

			return textureBuffer;
		}
	}
}