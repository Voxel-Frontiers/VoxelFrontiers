#region

using ApophisSoftware;
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

public partial class MenuController : Control {
	[ExportGroup("Main Menu Properties")] [ExportCategory("Main Menu")] [Export]
	public Control MainMenu;

	[Export] public Control SettingsMenu;

	[Export] public Panel        MenuBackground;
	[Export] public Control      UpdateMessage;
	[Export] public AcceptDialog DownloadError;
	[Export] public Control      LogDisplayer;

	[Export] public SCC SourceControl;

	[ExportCategory("GameState vars")] [Export]
	public bool InGame;

	[Export] public bool InGameScreen;
	[Export] public bool InMainMenu = true;
	[Export] public bool InSettings;
	[Export] public bool InUpdate;
	[Export] public bool InLog;

	[ExportCategory("Button Settings")] [Export]
	public Button btnSettings;

	[Export] public Button btnUpdate;
	[Export] public Button btnExit;
	[Export] public Button BtnExitSettings;
	[Export] public Button btnDisplayLog;

	[ExportCategory("Debugging")] [Export] public bool DEBUG;

	public override void _Ready() {
		Visible = true; // prep everything for initial game start.
		// set up the menus
		MainMenu.Visible = true;
		SettingsMenu.Visible = false;
		MenuBackground.Visible = true;
		LogDisplayer.Visible = false;

		InMainMenu = true;
		InSettings = false;
		InGame = false;

		LocalizeMenuItems();

		btnUpdate.Pressed += SourceControl.DownloadGameSource;
		btnSettings.Pressed += BtnSettingsOnPressed;
		btnExit.Pressed += BtnExitOnPressed;
		BtnExitSettings.Pressed += BtnExitSettingsOnPressed;
		btnDisplayLog.Pressed += BtnDisplayLogOnPressed;
		base._Ready();
	}

	private void LocalizeMenuItems() {
		btnUpdate.Text = S("Update Game");
		btnSettings.Text = S("Settings");
		btnExit.Text = S("Exit Game");
		btnDisplayLog.Text = S("View Log");
		DownloadError.DialogText =
			S("An error has occurred trying to download the source code. Please try again later.");
	}

	private string S(string strText) {
		return Utils.S(strText);
	}

	private void BtnDisplayLogOnPressed() {
		MainMenu.Visible = false;
		InMainMenu = false;
		InLog = true;
		LogDisplayer.Visible = true;
	}

	private void BtnExitSettingsOnPressed() {
		AcceptEvent();
		if (DEBUG) Logging.Log("DisplayLog Button Pressed.");

		SourceControl.GatherAndSaveSettings();

		SettingsMenu.Visible = false;
		if (!InGame) {
			MainMenu.Visible = true;
			InMainMenu = true;
		}

		InSettings = false;
	}

	internal void BtnExitLogDisplayerOnPressed() {
		AcceptEvent();
		if (DEBUG) Logging.Log("Exit DisplayLog Button Pressed.");

		LogDisplayer.Visible = false;
		if (!InGame) {
			MainMenu.Visible = true;
			InMainMenu = true;
		}

		InLog = false;
	}

	private void BtnSettingsOnPressed() {
		AcceptEvent();
		if (btnSettings.Visible == false || InMainMenu == false) return;
		if (InUpdate) return;

		InMainMenu = false;
		InSettings = true;

		if (DEBUG) Logging.Log("Settings Button Pressed.");

		if (MainMenu != null)
			MainMenu.Visible = false;

		if (SettingsMenu != null) SettingsMenu.Visible = true;
	}

	public override void _Input(InputEvent @event) {
		if (Visible == false) return;

		if (@event.IsActionPressed("ui_cancel")) { // this is mapped to the Esc key.
			AcceptEvent();                         // clear out the event so nothing else receives it. 
			if (InMainMenu) {
				BtnExitOnPressed(); // do the exit button pressed.
			} else if (InSettings) {
				BtnExitSettingsOnPressed();
			} else if (InGame) {
				// show settings
				BtnSettingsOnPressed();
			} else if (InGameScreen) {
				// close game screen
			}
		}

		base._Input(@event);
	}

	public override void _Notification(int what) {
		if (what == NotificationWMCloseRequest) GetTree().Quit(); // default behavior
	}

	private void BtnExitOnPressed() {
		if (Visible == false) return; //do nothing.
		if (InUpdate) return;

		if (DEBUG) Logging.Log("Exit Button Pressed.");

		// notify everything of Closing Time. 
		GetTree().Root.PropagateNotification((int) NotificationWMCloseRequest);
	}

	internal void ShowUpdateNotice() {
		btnUpdate.Disabled = true;
		UpdateMessage.Visible = true;
		InUpdate = true;
	}

	internal void HideUpdateNotice() {
		btnUpdate.Disabled = false;
		UpdateMessage.Visible = false;
		InUpdate = false;
	}

	internal void ShowDownloadError(string message) {
		DownloadError.DialogText = message;
		DownloadError.Show();
	}
}