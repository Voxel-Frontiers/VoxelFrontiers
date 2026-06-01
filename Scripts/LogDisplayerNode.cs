#region

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

public partial class LogDisplayerNode : Control {
	[Export] public Button         btnCloseLogView;
	[Export] public CodeEdit       LogViewDisplay;
	[Export] public MenuController MainMenuController;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		VisibilityChanged += OnVisibilityChanged;
		btnCloseLogView.Pressed += BtnCloseLogViewOnPressed;
	}

	private void BtnCloseLogViewOnPressed() {
		LogViewDisplay.Text = "";
		MainMenuController.BtnExitLogDisplayerOnPressed();
	}

	private void OnVisibilityChanged() {
		if (!Visible) {
			LogViewDisplay.Text = "";
			return;
		}

		string LogFile = Utils.GetStoragePath() + "/debug.txt";
		if (!File.Exists(LogFile)) {
			LogViewDisplay.Text = "Logfile Not Found.";
			return;
		}

		StreamReader srLogFile = new StreamReader(LogFile);
		LogViewDisplay.Text = srLogFile.ReadToEnd();
		srLogFile.Close();
	}
}