#region

using System;
using System.IO;
using Godot;

#endregion

#region License / Copyright

/*
 * Copyright © 2023-2026, Michieal.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

#endregion

namespace ApophisSoftware;

public static class Logging{
	private static string LogFile = "";
	private static StreamWriter swLogFile;

	private static void LogFileInit(string Log_File){
		LogFile = Log_File;
		swLogFile = new StreamWriter(LogFile, true);
	}

	public static void Log(object message){
		string timestamp = DateTime.Now.ToString("u");
		swLogFile.WriteLine(timestamp.ToString() + " - [INFO]: " + message);
		// swLogFile.WriteLine(); // put a blank line in there... 
		swLogFile.Flush();
		GD.Print(message);
	}

	public static void LogStartup(object message){
		string timestamp = DateTime.Now.ToString("u");
		swLogFile.WriteLine("\n\r------------------------------");
		swLogFile.WriteLine(timestamp.ToString() + " - [SYSTEM]: " + message);
		swLogFile.WriteLine("------------------------------");
		swLogFile.Flush();
		GD.Print(message);
	}

	public static void Log(string loglevel, object message){
		string timestamp = DateTime.Now.ToString("u");
		string loglevelpretty = "";

		switch (loglevel.ToLower()){
			case "info":
				loglevelpretty = "[INFO]: ";
				break;
			case "verbose":
				loglevelpretty = "[VERBOSE]: ";
				break;
			case "action":
				loglevelpretty = "[ACTION]: ";
				break;
			case "warning":
				loglevelpretty = "[WARNING]: ";
				break;
			case "error":
				loglevelpretty = "[ERROR]: ";
				break;
			case "system":
				loglevelpretty = "[SYSTEM]: ";
				break;
			default:
				loglevelpretty = "[INFO]: ";
				break;
		}

		swLogFile.WriteLine(timestamp.ToString() + " - " + loglevelpretty + message);
		swLogFile.WriteLine(); // put a blank line in there... 
		swLogFile.Flush();
		GD.Print(message);
	}

	// CTOR
	static Logging(){
		if (LogFile != "")
			LogFileInit(LogFile);
		else
			LogFileInit(Utils.GetStoragePath() + "/debug.txt");
	}

	public static void CloseLogFile(){
		swLogFile?.Flush();
		swLogFile?.Dispose();
		swLogFile = null;
	}
}