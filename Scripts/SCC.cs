#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Godot;
using HttpClient = System.Net.Http.HttpClient;

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
// -------------------------------
// Design Flow: This loads up the code, and processes it.
// Then, it sends off to the Lua Controller, to execute the Lua code, and begin the game.

public partial class SCC : Node {
	[ExportGroup("Sourcecode Properties")] [ExportCategory("SourceCode Settings")] [Export]
	public Label StatusLabel;

	[Export] public Label          VersionLabel;
	[Export] public VBoxContainer  _engineSettings;
	[Export] public VBoxContainer  _gameSettings;
	[Export] public MenuController MenuHandler;

	[Export] public bool DEBUG;

	[Export] public PackedScene SettingsPrefab;
//--------------------------------------------------------------------------------

	internal List<string>                GlobalSourceDirs = new();
	internal string                      ModulePath;
	internal string                      MainPath;
	internal List<string>                LocaleDirs;
	internal List<string>                TexDirs;
	internal List<string>                SoundDirs;
	internal List<string>                ModelDirs;
	internal List<string>                MenuFiles;
	internal List<string>                TextureFiles;
	internal List<string>                SourceFiles;
	internal List<string>                SoundFiles;
	internal List<string>                ModelFiles;
	internal Dictionary<string, string>  LocaleFiles;
	internal Dictionary<string, string>  ModPaths;
	private  List<string>                SettingsFiles; // Released in SettingsProcessor.
	internal Dictionary<string, Setting> GameSettings;
	internal Dictionary<string, object>  CurVals;

	internal string SettingsConf = "";

	private readonly Utils     _utils  = new();
	private static   string    PathSep = "/";
	private          string    GamePath;
	private static   string    _ZipFile;
	private          Coroutine dirScanCoroutine;
	private          Coroutine SettingsProcCoroutine;

	// Private static instance field to hold the single instance of SCC.
	private static SCC instance;

	// Private constructor to prevent external instantiation.
	private SCC() {
	}

	// Public static property to get the Singleton instance.
	public static SCC Instance {
		get {
			// If the instance hasn't been created yet, create it.
			if (instance == null) instance = new SCC();

			return instance;
		}
	}


	// Http Net Access
	// HttpClient lifecycle management best practices:
	// https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines#recommended-use
	private static HttpClient SourceClient = new() {
		BaseAddress = new Uri("https://git.minetest.land")
	};

	public Utils Utils => _utils;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
#if android || mobile
		Utils.ANDROID = true;
#endif
#if !(mobile || android)
		if (OS.HasFeature("web"))
			Utils.WEBDEPLOY = true;
#endif

		// application/config/version

		GameVersion = (string) ProjectSettings.GetSetting("application/config/version");

		GamePath = Utils.GetStoragePath() + PathSep + "Game" + PathSep;
		if (Directory.Exists(GamePath) == false)
			Directory.CreateDirectory(GamePath);

		_ZipFile = GamePath + "GameZip.zip";
		StatusLabel.Text = Utils.S("Game Initializing.");
		Logging.LogStartup("System Start.");

		ScanDirectories();
	}

	internal string GameVersion { get; set; }

	// make sure that we're as fresh as new york snow!
	private void InitializeStorage() {
		GlobalSourceDirs = new List<string>();
		LocaleDirs = new List<string>();
		TexDirs = new List<string>();
		SoundDirs = new List<string>();
		ModelDirs = new List<string>();
		MenuFiles = new List<string>();
		TextureFiles = new List<string>();
		SourceFiles = new List<string>();
		SoundFiles = new List<string>();
		ModelFiles = new List<string>();
		LocaleFiles = new Dictionary<string, string>();
		ModPaths = new Dictionary<string, string>();
		SettingsFiles = new List<string>(); // Released in SettingsProcessor.
		GameSettings = new Dictionary<string, Setting>();
		CurVals = new Dictionary<string, object>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
	}

	public override void _ExitTree() {
		OnDisable();
		Logging.CloseLogFile();
		base._ExitTree();
	}

	private void OnDisable() {
		if (dirScanCoroutine != null)
			CoroutineManager.Instance.StopCoroutine(dirScanCoroutine);
		if (SettingsProcCoroutine != null)
			CoroutineManager.Instance.StopCoroutine(SettingsProcCoroutine);
	}

	public void ExitGame() {
		OnDisable();
		// Insert in App Quit code here
	}

	private string ProcessVersionCode() {
		//version=0.85.0-SNAPSHOT
		if (File.Exists(MainPath + "game.conf") == false) return "";

		return Utils.ProcessConfSettingFromFile(MainPath + "game.conf", "version");
	}

	internal Dictionary<string, UISetting> UIGameSettings = new();

	private string ProcessModName(string filename) {
		// updated to be universal, so now this is a wrapper function.
		return Utils.ProcessConfSettingFromFile(filename, "name");
	}

	private void ScanDirectories() {
		InitializeStorage();
		StatusLabel.Text = "Scanning Source. May take a while, please be patient.";
		if (DEBUG) Logging.Log("Scanning Game Source.");

		string[] dirs = Directory.GetDirectories(GamePath);
		if (dirs.Length < 1) {
			StatusLabel.Text = Utils.S("Game Source Not Found. Downloading.");
			if (DEBUG) Logging.Log("Game Source not found - Downloading.");

			DownloadGameSource();
			return;
		}

		if (DEBUG) Logging.Log("Game Source Exists.");

		if (dirs.Length > 0)
			foreach (string dir in dirs)
				if (File.Exists(dir + PathSep + "game.conf")) {
					dirs = Directory.GetDirectories(dir);
					MainPath = dir + PathSep;
					break;
				}

		if (MainPath != "")
			if (File.Exists(MainPath + "settingtypes.txt"))
				SettingsFiles.Add(MainPath + "settingtypes.txt");

		if (VersionLabel != null) VersionLabel.Text = "Version: " + GameVersion + ":" + ProcessVersionCode();
		// Example: 'Version 2023.9.20:0.85.0-SNAPSHOT'
		// handle actual Dir scanning code.
		// look for MODules, Textures, and Menu.
		foreach (string dir in dirs) {
			// handle Menu Files.
			if (dir.ToLower().EndsWith("menu")) {
				if (DEBUG) Logging.Log("Handling Game Menu.");

				string[] menu = Directory.GetFiles(dir);
				MenuFiles.Clear();
				MenuFiles.AddRange(menu);
			}

			// handle Root Texture Files.
			if (dir.ToLower().EndsWith("textures")) {
				StatusLabel.Text = Utils.S("Building Textures List.");
				if (DEBUG) Logging.Log("Building Textures List.");

				string[] texfiles = Directory.GetFiles(dir);
				TextureFiles.Clear();
				TextureFiles.AddRange(texfiles);
			}

			if (dir.ToLower().EndsWith("mods")) {
				ModulePath = dir;
				if (DEBUG) Logging.Log("Found Module Dirs.");
			}
		}

		StatusLabel.Text = Utils.S("Deep Scanning Game structure.");
		if (DEBUG) Logging.Log("Deep Scanning Game Structure.");

		dirScanCoroutine = CoroutineManager.Instance.StartCoroutine(DirScan());
	}

	private IEnumerator DirScan() {
		WaitOneFrame WaitAFrame = new WaitOneFrame();
		string[] dirs;
		// recursively scan all module directories.
		StatusLabel.Text = Utils.S("Building directory structure.");
		if (DEBUG) Logging.Log("Building Directory Modules Structure.");

		dirs = Directory.GetDirectories(ModulePath, "*.*", SearchOption.AllDirectories);
		yield return WaitAFrame; // yield one frame...
		GlobalSourceDirs.AddRange(dirs);

		string[] models = Directory.GetDirectories(ModulePath, "models", SearchOption.AllDirectories);
		yield return WaitAFrame;
		string[] sounds = Directory.GetDirectories(ModulePath, "sounds", SearchOption.AllDirectories);
		yield return WaitAFrame;
		string[] media = Directory.GetDirectories(ModulePath, "media", SearchOption.AllDirectories);
		yield return WaitAFrame;
		string[] texs = Directory.GetDirectories(ModulePath, "textures", SearchOption.AllDirectories);
		yield return WaitAFrame;
		string[] _locales = Directory.GetDirectories(ModulePath, "locale", SearchOption.AllDirectories);
		yield return WaitAFrame;

		ModelDirs.AddRange(models);
		SoundDirs.AddRange(sounds);
		TexDirs.AddRange(texs);
		LocaleDirs.AddRange(_locales);

		foreach (string dir in LocaleDirs) GlobalSourceDirs.Remove(dir);

		StatusLabel.Text = Utils.S("Building Textures List.");
		if (DEBUG) Logging.Log("Building Textures List.");

		int indCount = 0;
		foreach (string dir in TexDirs) {
			GlobalSourceDirs.Remove(dir);
			string[] textures = Directory.GetFiles(dir);
			foreach (string texture in textures)
				if (texture.ToLower().EndsWith("bmp") || texture.ToLower().EndsWith("png") ||
				    texture.ToLower().EndsWith("jpg") || texture.ToLower().EndsWith("jpeg")) {
					TextureFiles.Add(texture);
					indCount++;
					if (indCount % 10 == 0)
						yield return WaitAFrame; // release control because this operation will be huge.
				}
		}

		yield return WaitAFrame;

		StatusLabel.Text = Utils.S("Building Sounds List.");
		if (DEBUG) Logging.Log("Building Sounds List.");

		indCount = 0;
		foreach (string dir in SoundDirs) {
			GlobalSourceDirs.Remove(dir);
			string[] _sounds = Directory.GetFiles(dir);
			foreach (string sound in _sounds)
				if (sound.ToLower().EndsWith("ogg") || sound.ToLower().EndsWith("mp3") ||
				    sound.ToLower().EndsWith("aif") || sound.ToLower().EndsWith("wav")) {
					SoundFiles.Add(sound);
					indCount++;
					if (indCount % 10 == 0)
						yield return WaitAFrame; // release control because this operation will be many.
				}
		}

		yield return WaitAFrame;

		StatusLabel.Text = Utils.S("Building Models List.");
		if (DEBUG) Logging.Log("Indexing Models.");

		indCount = 0;
		foreach (string dir in ModelDirs) {
			GlobalSourceDirs.Remove(dir);
			string[] _models = Directory.GetFiles(dir);
			foreach (string model in _models)
				if (model.ToLower().EndsWith("obj") || model.ToLower().EndsWith("fbx")) {
					ModelFiles.Add(model);
					indCount++;
					if (indCount % 10 == 0)
						yield return WaitAFrame; // release control because this operation will be many.
				}
		}

		yield return WaitAFrame;

		StatusLabel.Text = Utils.S("Indexing Locale Files.");
		if (DEBUG) Logging.Log("Indexing Locale Files.");

		indCount = 0;
		foreach (string dir in LocaleDirs) {
			GlobalSourceDirs.Remove(dir);
			string[] _localefiles = Directory.GetFiles(dir);
			foreach (string item in _localefiles)
				if (item.ToLower().EndsWith("tr")) {
					LocaleFiles.Add(item, dir);
					indCount++;
					if (indCount % 10 == 0)
						yield return WaitAFrame; // release control because this operation will be many.
				}
		}

		yield return WaitAFrame;
		GC.Collect(); //Do a quick (Hopefully!) collect of any generated garbage.
		yield return WaitAFrame;

		// Now that we've done some pruning... 
		ProcessDirs();
		yield return WaitAFrame;
	}

	// process Game Source Settings..
	private IEnumerator SettingsProcessor() {
		WaitOneFrame WaitAFrame = new WaitOneFrame();

		StatusLabel.Text = Utils.S("Processing Settings.");
		if (DEBUG) Logging.Log("Starting the Processing of Settings.");

		foreach (string settingsFile in SettingsFiles) {
			Dictionary<string, Setting> temp = Utils.ProcessSettingFromTextFile(settingsFile);
			foreach (KeyValuePair<string, Setting> keyValuePair in temp)
				if (keyValuePair.Key != "") {
					if (GameSettings.ContainsKey(keyValuePair.Key))
						Logging.Log("error", "setting duplication: " +
						                     keyValuePair.Key + "\n" + keyValuePair.Value +
						                     "\nSetting Skipped.");
					else
						GameSettings.Add(keyValuePair.Key,
							keyValuePair.Value); // migrate the returned settings to the combined settings.
				}

			yield return WaitAFrame;
			GC.Collect(); // we've probably created a ton of garbage by now. Let's fix that.
			yield return WaitAFrame;
		}

		// handle setting the saved values... 
		SettingsConf = Utils.GetStoragePath() + "/settings.conf";
		if (File.Exists(SettingsConf) == false) // prepare first time use.
			ProcessGameMTC(SettingsConf);

		// TODO: Make CurVals useful, and have it hold the current settings' values like it is supposed to!
		CurVals = Utils.ProcessSavedSettingsFile(SettingsConf);

		string key;
		foreach (KeyValuePair<string, Setting> keyValuePair in GameSettings) {
			// handle keycheck
			key = keyValuePair.Key;
			if (CurVals.ContainsKey(key))
				// Handle setting the "Setting" to the CurrentValues' value.
				keyValuePair.Value.CurrentValue = CurVals[key];
			else
				// Handle setting the CurrentValues' value for the default setting.
				CurVals.Add(key, keyValuePair.Value);
		}

		SettingsFiles.Clear(); // Release settings files.
		StatusLabel.Text = Utils.S("Settings Processing Done.");
		if (DEBUG) Logging.Log("Done Processing Settings.");

		yield return WaitAFrame;
		BuildSettingsUI();
		yield return WaitAFrame;
	}

	internal void GatherAndSaveSettings() {
		string key;
		foreach (KeyValuePair<string, Setting> keyValuePair in GameSettings) {
			// handle keycheck
			key = keyValuePair.Key;
			if (CurVals.ContainsKey(key))
				CurVals[key] = keyValuePair.Value.CurrentValue;
			else
				// Handle setting the CurrentValues' value for the default setting.
				CurVals.Add(key, keyValuePair.Value);
		}

		Utils.SaveSettingsToFile(SettingsConf, CurVals);
	}

	private void SettingsPrefabInstantiate(VBoxContainer Parent, Setting thisSetting) {
		UISetting reference = SettingsPrefab.Instantiate<UISetting>(PackedScene.GenEditState.Disabled);
		reference.ThisSetting = thisSetting;
		reference.Name = thisSetting.SettingName;
		UIGameSettings.Add(thisSetting.SettingName, reference);
		reference.DEBUG = DEBUG;                  // Make SCC Control "Debug" for the settings UI.
		Parent.AddChild(reference);               // Add the uisetting to the appropriate parent.
		reference.InitializeSetting(thisSetting); // set up the newly created setting.
		if (DEBUG) Logging.Log("Created setting: " + reference.Name);
	}

	internal void BuildSettingsUI() {
		if (DEBUG) Logging.Log("Building Settings UI.");

		foreach (KeyValuePair<string, Setting> keyValuePair in GameSettings)
			SettingsPrefabInstantiate(_gameSettings, keyValuePair.Value);

		if (DEBUG) Logging.Log("Settings Build Completed.");

		StatusLabel.Text = Utils.S("Settings Build Completed.");
	}

	internal void DestroySettingsUI() {
		foreach (KeyValuePair<string, UISetting> uiGameSetting in UIGameSettings) uiGameSetting.Value.Remove();

		UIGameSettings.Clear();
	}

	private void ProcessGameMTC(string settingsConf) {
		if (File.Exists(MainPath + "minetest.conf") == false)
			return; // not there? exit.
		StreamReader srReader = new StreamReader(MainPath + "minetest.conf");
		StreamWriter swWriter = new StreamWriter(settingsConf);
		string data;
		while (srReader.EndOfStream == false) {
			data = srReader.ReadLine();
			if (data.Contains('=')) swWriter.WriteLine(data.Trim());
		}

		srReader.Close();
		swWriter.Flush();
		swWriter.Close();
	}

	private void ProcessDirs() {
		StatusLabel.Text = Utils.S("Building Source Code List.");
		if (DEBUG) Logging.Log("Building Source Code List.");

		List<string> RemoveDirs = new List<string>();
		foreach (string dir in GlobalSourceDirs) {
			string dirpath = dir + PathSep;
			if (File.Exists(dirpath + "mod.conf")) {
				string modname = ProcessModName(dirpath + "mod.conf");
				if (modname != "") ModPaths.Add(modname, dir);

				if (File.Exists(dirpath + "init.lua"))
					if (File.Exists(dirpath + "init.lua"))
						SourceFiles.Add(dirpath + "init.lua");

				if (File.Exists(dirpath + "settingtypes.txt")) {
					if (SettingsFiles.Contains(dirpath + "settingtypes.text"))
						Logging.Log("error",
							"Settings file found twice!\nFilename: " + dirpath + "settingtypes.text");
					else
						SettingsFiles.Add(dirpath + "settingtypes.txt");
				}
			} else {
				RemoveDirs.Add(dir);
			}
		}

		StatusLabel.Text = Utils.S("Source Scan Completed.");
		if (DEBUG) Logging.Log("Game Source Scanning Complete.");

		foreach (string dir in RemoveDirs) GlobalSourceDirs.Remove(dir); // clean up the source dirs. 

		RemoveDirs.Clear();
		StatusLabel.Text = Utils.S("Processing Settings Files.");
		if (DEBUG) Logging.Log("Processing Settings Files.");

		SettingsProcCoroutine = CoroutineManager.Instance.StartCoroutine(SettingsProcessor());
	}

	internal void DownloadGameSource() {
		if (MenuHandler.btnUpdate.Visible == false ||
		    MenuHandler.btnUpdate.Disabled)
			return; // if this is buried, don't allow the hot-key to force download the source code. 

		// Handle UI display info
		MenuHandler.ShowUpdateNotice();

		if (dirScanCoroutine != null) { // stop processing, if we are going to download / update the files.
			CoroutineManager.Instance.StopCoroutine(dirScanCoroutine);
			dirScanCoroutine = null;
		}

		if (File.Exists(_ZipFile))
			File.Delete(_ZipFile);        // start with a fresh download.
		if (Directory.Exists(GamePath)) { // clear out any remaining items in here, to install over.
			Directory.Delete(GamePath, true);
			Directory.CreateDirectory(GamePath); // recreate the game source storage dir.
		}

		// ------------------------------------------------------
		StatusLabel.Text = Utils.S("Downloading Game Source; Please wait.");
		if (DEBUG) Logging.Log("Downloading Game Source.");

		DestroySettingsUI();

#pragma warning disable CS4014
		DownloadFile(SourceClient);
#pragma warning restore
	}

	private async Task DownloadFile(HttpClient httpClient) {
		using HttpResponseMessage response = await
			httpClient.GetAsync("Michieal/MineClone2/archive/master.zip");

		if (response is {StatusCode: HttpStatusCode.OK}) {
			byte[] responseByteArray = await response.Content.ReadAsByteArrayAsync();
			using (FileStream fileStream = new FileStream(_ZipFile, FileMode.Create)) {
				// save the downloaded file out...
				await fileStream.WriteAsync(responseByteArray, 0, responseByteArray.Length);
			}

			ExtractSource();
		} else {
			Logging.Log("error", "Error retrieving Game Source: \n" + response.ReasonPhrase);
			MenuHandler.ShowDownloadError(
				"An error has occurred trying to download the source code. Please try again later.");
		}
	}

	private void ExtractSource() {
		StatusLabel.Text = Utils.S("Game Source Download Complete! Extracting.");
		using (ZipArchive GameSourceZip = ZipFile.Open(_ZipFile, ZipArchiveMode.Read)) {
			try {
				// This method should handle the extraction of the downloaded ZIP file.
				GameSourceZip.ExtractToDirectory(GamePath, true);
			} catch (Exception ex) {
				// Handle extraction errors
				Logging.Log("error", "Extraction Error: " + ex.Message);
				MenuHandler.ShowDownloadError("An error has occurred trying to extract the source code.\n" +
				                              "Trying to redownload the source code.");
				DownloadGameSource();
				return;
			}

			StatusLabel.Text = Utils.S("Game Source Extracted. Scanning Directories.");
		}


		if (File.Exists(_ZipFile))
			File.Delete(_ZipFile); // clean up the download to save space.

		MenuHandler.HideUpdateNotice();
		ScanDirectories();
	}
}