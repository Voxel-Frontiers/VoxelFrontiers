#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ApophisSoftware.LuaObjects;
using Godot;
using Environment = System.Environment;

#endregion

#region License / Copyright

/*
 * Copyright © 2023, Michieal.
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

public class Utils {
	/*public static Texture2D LoadImageToTexture2D(string filename) {
		Texture2D RetImage = new Texture2D(1, 1);

		if (File.Exists(filename) == false)
			return null;

		byte[] imagedata = File.ReadAllBytes(filename);

		RetImage.LoadImage(imagedata);

		return RetImage;
	}*/

	internal static bool ANDROID   { get; set; }
	internal static bool WEBDEPLOY { get; set; }

	// --------------------------------------------
	// Localization dictionaries.		
	// format: key = English, Value = Translated.
	private static Dictionary<string, string> EngineLangDE = new();
	private static Dictionary<string, string> EngineLangFR = new();
	private static Dictionary<string, string> EngineLangJA = new();
	private static Dictionary<string, string> EngineLangES = new();
	private static Dictionary<string, string> GameLangDE   = new();
	private static Dictionary<string, string> GameLangFR   = new();
	private static Dictionary<string, string> GameLangJA   = new();
	private static Dictionary<string, string> GameLangES   = new();

	// --------------------------------------------

	public static T ConvertTo<T>(object Source) {
		return (T) Convert.ChangeType(Source, typeof(T));
	}

	public static string GetStoragePath() {
		string PathString = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		if (!ANDROID && !WEBDEPLOY) {
			PathString += "/.Apophis Software/.mineclonepp";
			Directory.CreateDirectory(PathString); // make the path string, if it doesn't exist.
			return PathString;
		} else {
			// When not ran on a Desktop system, use the built in user data dir. Which, points to the data path for
			// the storage device. For Android, it's the app's storage; for WebGL, it's the section of the Browser
			// storage that the webgl app has access to.
			PathString = OS.GetUserDataDir();
			PathString += "/mineclonepp";
			Directory.CreateDirectory(PathString); // make the path string, if it doesn't exist.

			return PathString;
		}
	}

	public static string ProcessConfSettingFromFile(string Filename, string Setting) {
		StreamReader srStream = File.OpenText(Filename);
		string value = "";

		while (!srStream.EndOfStream) {
			value = srStream.ReadLine().ToLower();
			if (value.StartsWith(Setting)) { // if found, strip out the bs. should end up with a "mcl_" something.
				value = value.Replace(Setting, ""); // "=\"{}#"
				value = value.Replace(" ", "");
				value = value.Replace("=", "");
				value = value.Replace("\"", "");
				value = value.Replace("#", "");

				break;
			}
		}

		return value;
	}

	public static void SaveSettingsToFile(string SettingsConf, Dictionary<string, object> SettingsDict) {
		StreamWriter swWriter = new StreamWriter(SettingsConf);
		string data;

		foreach (KeyValuePair<string, object> setting in SettingsDict) {
			data = setting.Key + "=" + setting.Value.ToString();
			swWriter.WriteLine(data.Trim());
		}

		swWriter.Flush();
		swWriter.Close();
		GC.Collect();
	}

	public static Dictionary<string, object> ProcessSavedSettingsFile(string ConfFilename) {
		if (File.Exists(ConfFilename) == false) {
			Logging.Log("warning", "Tried to get settings from a non-existent file:\n" + ConfFilename);

			return null; // not there? exit.
		}

		Dictionary<string, object> settings = new Dictionary<string, object>();

		StreamReader srReader = new StreamReader(ConfFilename);
		string data;
		string[] setdata;
		while (srReader.EndOfStream == false) {
			data = srReader.ReadLine();
			if (data != null) {
				if (data.Contains('=') && data.StartsWith("#") == false) {
					setdata = data.Split("=");
					if (!settings.ContainsKey(setdata[0].ToLower().Trim()))
						settings.Add(setdata[0].ToLower().Trim(), setdata[1].ToLower().Trim());
				}
			}
		}

		srReader.Close();

		return settings;
	}

	public static Dictionary<string, Setting> ProcessSettingFromTextFile(string Filename) {
		Dictionary<string, Setting> settings = new Dictionary<string, Setting>();
		Setting setting = new Setting();
		StreamReader srStream = File.OpenText(Filename);
		string value = "";

		string SettingsCategory = "General";

		while (!srStream.EndOfStream) {
			setting.SettingsFile = Filename;
			value = srStream.ReadLine().ToLower().Trim();

			if (value == "") // handle a new setting.
				if (settings.ContainsKey(setting.SettingName) == false) {
					// prevent adding 2 of the same settings. Also, no blank settings.
					settings.Add(setting.SettingName, setting); // Add to existing setting. 
					if (settings.ContainsKey("")) settings.Remove("");

					setting = new Setting();
				}

			if (value.StartsWith("[")) { // Handle Category Specifier.
				setting.Category = value.Remove(0, 1).Replace("]", "").ToUpper();
				value = ""; // A nothing value.
				SettingsCategory = setting.Category;
			} else {
				setting.Category = SettingsCategory;
			}

			if (value.StartsWith("#")) {    // handle Description
				value = value.Remove(0, 1); // Remove the first char (#) from the string.
				value = value.Trim();
				value = value.Replace("Minetest", "[old engine]");
				value = value.Replace("minetest", "crap");
				if (value != "")                          // don't put in blank lines. 
					setting.Description.Add(value + " "); // add the new line to the string description.
			} else if (value.StartsWith("#") == false && value != "") {
				// here is the bulk of the setting...
				char[] splitter = {'(', ')'};
				string[] setstr = value.Split(splitter);
				string[] setstrData = {""};
				try {
					setstrData = setstr[2].Trim().Split(' ');
				} catch (Exception e) {
					Logging.Log("error", "Malformed Setting.\n\r" + e);
					return null;
				}

				// Example Format: "tsm_railcorridors_probability_chest (Chest probability) float 0.05 0.0 1.0"
				// extract the setting name.
				setting.SettingName = setstr[0].Trim(); // first part of the line.
				Logging.Log("Processing Setting: " + setting.SettingName);
				setting.DisplayName = setstr[1].Trim(); // second part of the line.
				setting.SettingType = setstrData[0].Trim();

				// Defining Defaults:
				// mcl_weather_rain_particles (Rain particles) int 500 0
				// the 500 is the Default value, 0 is the minimum value.

				switch (setting.SettingType.ToLower()) {
					case "enum":
						Logging.Log(value);
						//[INFO]: mcl_node_particles (block particles detail level) enum none high,medium,low,none
						setting.DefaultValue = ConvertTo<string>(setstrData[1]) as object;
						setting.EnumerationValues = setstrData[2].Trim().Split(',');
						break;
					case "string":
						break;
					case "bool":
						setting.DefaultValue = ConvertTo<bool>(setstrData[1].Trim()) as object;
						break;
					case "int":
						setting.DefaultValue = ConvertTo<int>(setstrData[1].Trim()) as object;
						// check for, and handle min/max values.
						if (setstrData.Length > 2)
							setting.minValue = ConvertTo<int>(setstrData[2].Trim()) as object;

						if (setstrData.Length > 3)
							setting.maxValue = ConvertTo<int>(setstrData[3].Trim()) as object;

						break;
					case "float":
						setting.DefaultValue = ConvertTo<float>(setstrData[1].Trim()) as object;
						// check for, and handle min/max values.
						if (setstrData.Length > 2)
							setting.minValue = ConvertTo<float>(setstrData[2].Trim()) as object;

						if (setstrData.Length > 3)
							setting.maxValue = ConvertTo<float>(setstrData[3].Trim()) as object;

						break;
					default:
						Logging.Log("error",
							"Unknown Setting Type DefaultValue in Settings File: " + setting.SettingsFile +
							"; Setting:" +
							setting.SettingName + "\n " + value);
						break;
				} // switch

				setting.CurrentValue = setting.DefaultValue;
			} // if
		}     // while.

		// Handle the last (or in some cases the ONLY) setting.
		if (settings.ContainsKey(setting.SettingName) == false) {
			// prevent adding 2 of the same settings. Also, no blank settings.

			settings.Add(setting.SettingName, setting); // Add to existing setting. 

			if (settings.ContainsKey("")) settings.Remove("");
		}

		return settings; // return the defined settings from this file.
	}                    // function.

	public static bool TestForError(Variant X) {
		bool isError = false;

		if (X.Obj == null) return isError;

		try {
			if (X.Obj.GetType() == typeof(LuaError)) {
				var z = (LuaError) X;
				if (z.Message != "") {
					isError = true;
					Logging.Log("error", "LUA Runtime Error Catch.");
					Logging.Log("error", "ERROR " + z.Type + ": " + z.Message);
				}
			}
		} catch (Exception e) {
			Logging.Log(e);
			return false;
		}

		return isError;
	}

	public Utils() {
		// Have a basic Ctor.
	}

	static Utils() {
		GetLocalization(); // initialize the language.
	}

	internal static string GetRawLocalizationCode() {
		// Get the current culture
		CultureInfo currentCulture = CultureInfo.CurrentCulture;

		// Get the current language code (two-letter ISO language name)
		string languageCode = currentCulture.TwoLetterISOLanguageName;

		return languageCode.ToUpper();
	}

	internal static string GetLocalization() {
		// Get the current language code (two-letter ISO language name)
		string languageCode = GetRawLocalizationCode();

		switch (languageCode) {
			case "EN":
				CurrentLocale = LanguageCodes.en;
				break;
			case "ES":
				CurrentLocale = LanguageCodes.es;
				LoadLocalizationFile("res://locale/locale.es.tr", LanguageCodes.en);
				break;
			case "FR":
				LoadLocalizationFile("res://locale/locale.fr.tr", LanguageCodes.fr);
				CurrentLocale = LanguageCodes.fr;
				break;
			case "DE":
				LoadLocalizationFile("res://locale/locale.de.tr", LanguageCodes.de);
				CurrentLocale = LanguageCodes.de;
				break;
			case "JA":
				LoadLocalizationFile("res://locale/locale.ja.tr", LanguageCodes.ja);
				CurrentLocale = LanguageCodes.ja;
				break;
		}

		return languageCode;
	}

	public static LanguageCodes CurrentLocale = LanguageCodes.en;

	public static string S(string StrDisplay) {
		return LocalizeString(StrDisplay, CurrentLocale);
	}

	public static string GsTranslateString(string StrDisplay) {
		return LocalizeString(StrDisplay, CurrentLocale, true);
	}

	public static string LocalizeString(string DisplayString, LanguageCodes Lang, bool ForGame = false) {
		// This switch block uses fall-through logic. 
		switch (Lang) {
			case LanguageCodes.de:
				if (ForGame) {
					if (GameLangDE.TryGetValue(DisplayString, out var s)) return s;
				} else {
					if (EngineLangDE.TryGetValue(DisplayString, out var s)) return s;
				}

				break;
			case LanguageCodes.fr:
				if (ForGame) {
					if (GameLangFR.TryGetValue(DisplayString, out var s)) return s;
				} else {
					if (EngineLangFR.TryGetValue(DisplayString, out var s)) return s;
				}

				break;
			case LanguageCodes.es:
				if (ForGame) {
					if (GameLangES.TryGetValue(DisplayString, out var s)) return s;
				} else {
					if (EngineLangES.TryGetValue(DisplayString, out var s)) return s;
				}

				break;
			case LanguageCodes.ja:
				if (ForGame) {
					if (GameLangJA.TryGetValue(DisplayString, out var s)) return s;
				} else {
					if (EngineLangJA.TryGetValue(DisplayString, out var s)) return s;
				}

				break;

			default:
				break;
		}

		return DisplayString;
	}

	public static void LoadLocalizationFile(string Filename, LanguageCodes LangCode, bool ForGame = false) {
		if (File.Exists(Filename) == false)
			return;

		StreamReader srTRFile = new StreamReader(Filename);

		string[] data;
		string temp;
		while (srTRFile.EndOfStream == false) {
			temp = srTRFile.ReadLine();
			if (temp.StartsWith("#"))
				continue; // skip lines with # as the starting character. aka, skip remarks.

			data = temp.Split("=");

			switch (LangCode) {
				case LanguageCodes.de:
					if (ForGame) {
						if (GameLangDE.ContainsKey(data[0].Trim()) == false)
							GameLangDE.Add(data[0].Trim(), data[1].Trim());
					} else {
						if (EngineLangDE.ContainsKey(data[0].Trim()) == false)
							EngineLangDE.Add(data[0].Trim(), data[1].Trim());
					}

					break;
				case LanguageCodes.es:
					if (ForGame) {
						if (GameLangES.ContainsKey(data[0].Trim()) == false)
							GameLangES.Add(data[0].Trim(), data[1].Trim());
					} else {
						if (EngineLangES.ContainsKey(data[0].Trim()) == false)
							EngineLangES.Add(data[0].Trim(), data[1].Trim());
					}

					break;
				case LanguageCodes.fr:
					if (ForGame) {
						if (GameLangFR.ContainsKey(data[0].Trim()) == false)
							GameLangFR.Add(data[0].Trim(), data[1].Trim());
					} else {
						if (EngineLangFR.ContainsKey(data[0].Trim()) == false)
							EngineLangFR.Add(data[0].Trim(), data[1].Trim());
					}

					break;
				case LanguageCodes.ja:
					if (ForGame) {
						if (GameLangJA.ContainsKey(data[0].Trim()) == false)
							GameLangJA.Add(data[0].Trim(), data[1].Trim());
					} else {
						if (EngineLangJA.ContainsKey(data[0].Trim()) == false)
							EngineLangJA.Add(data[0].Trim(), data[1].Trim());
					}

					break;
				default:
					Logging.Log("error", "Unknown Language file");
					break;
			}
		}

		srTRFile.Close();
	}


	// ---------------------------------------------------------------------------------------------
	// Lua Section.
	private static LuaController _luaController;

	internal static void SetLuaController(LuaController LC) {
		_luaController = LC;
	}

	internal static LuaController GetLuaController() {
		return _luaController;
	}

	// Design Note: (TODO:) make where this is called from instantiate the Lua Controller, so that it's not running from
	// the start of the program. 
	internal static void LoadLuaScripts(Dictionary<string, string> Scripts) {
		foreach (var kvp in Scripts) {
			LUA.lua.DoString("_exec_file(\"" + kvp.Value + "\", " + kvp.Key + ")");
		}
	}
} // class Utils.

public enum LanguageCodes {
	en = 0, // project default language.
	de = 1,
	fr = 2,
	ja = 3,
	es = 4,
	br = 5
}
// Namespace.