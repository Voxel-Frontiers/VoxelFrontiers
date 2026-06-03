#region

using System;
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

public partial class UISetting : Node{
	[ExportGroup("UI Setting Properties")] [ExportCategory("Linkages")] [Export]
	public Label SettingName;

	[Export] public Label Category;
	[Export] public Label Description;
	[Export] public Label DefaultValue;
	[Export] public LineEdit ValueInput;
	[Export] public OptionButton ValueDropdown;
	[Export] public CheckBox ValueToggle;
	[Export] public Label ValueRange;

	[ExportCategory("Settings")] [Export] public bool DEBUG = false;
	internal Setting ThisSetting;

	public UISetting(){
	}

	private string settingType;
	private string[] values;
	private bool togVal = false;
	private bool UseRange;

	internal void InitializeSetting(Setting SettingDefault){
		ThisSetting = SettingDefault;

		settingType = ThisSetting.SettingType;
		SettingName.Text = ThisSetting.DisplayName.ToUpper();
		Category.Text = ThisSetting.Category.ToUpper();

		Description.Text = ""; // Clear out the placeholder text, from designing the prefab.
		Description.TooltipText = "";
		foreach (string s in ThisSetting.Description){
			Description.Text += s + "\n";
			Description.TooltipText += s + "\n";
		}

		// hide the edit area.
		ValueDropdown.Visible = false;
		ValueInput.Visible = false;
		ValueToggle.Visible = false;

		// tooltips
		ValueDropdown.TooltipText = ThisSetting.ToString();
		ValueInput.TooltipText = ThisSetting.ToString();
		ValueToggle.TooltipText = ThisSetting.ToString();

		if (ThisSetting.minValue.Equals(ThisSetting.maxValue))
			UseRange = false;
		else
			UseRange = true;

		if (UseRange){
			ValueRange.TooltipText += "Minimum Value = " + ThisSetting.minValue + "\n";
			ValueRange.TooltipText += "Maximum Value = " + ThisSetting.maxValue + "\n";
			ValueRange.Text = "Range: " + ThisSetting.minValue;
			var x = ThisSetting.minValue.ToString().ToFloat();
			var y = ThisSetting.maxValue.ToString().ToFloat();
			if (x > y){
				ValueRange.Text += " - ∞"; // handle when min is set, but max is null.
				ThisSetting.maxValue = 100000; //grant a max, as it is used to handle validation. 
			}
			else if (y > x){
				ValueRange.Text += " - " + ThisSetting.maxValue;
			}
			else{
				ValueRange.Visible = false; // if it is the same, then it shouldn't be shown.
				UseRange = false;
			}
		}

		// set the current value in case we need to edit it.
		switch (ThisSetting.SettingType){
			case "bool":
				if (ValueToggle != null)
					ValueToggle.Toggled += ValueToggleOnToggled;

				if (ThisSetting.CurrentValue.ToString().ToLower().Trim() == "false"){
					togVal = false;
					ValueToggle.Text = "FALSE";
					ValueToggle.ButtonPressed = false;
				}
				else{
					togVal = true;
					ValueToggle.Text = "TRUE";
					ValueToggle.ButtonPressed = true;
				}

				break;
			case "enum":
				if (ValueDropdown == null) return;

				ValueDropdown.TooltipText = ThisSetting.DisplayName + "\n";
				ValueDropdown.TooltipText += "Default Value = " + ThisSetting.DefaultValue + "\n";
				ValueDropdown.TooltipText += "Options:\n";
				values = ThisSetting.EnumerationValues;
				foreach (string opts in values){
					ValueDropdown.AddItem(opts.Trim());
					ValueDropdown.TooltipText += opts + "\n";
				}

				int index = 0;
				for (int i = 0; i < values.Length; i++)
					if (values[i] == ThisSetting.CurrentValue.ToString()){
						index = i;
						break;
					}

				ValueDropdown.Selected = index;
				ValueDropdown.ItemSelected += ValueDropdownOnItemSelected;

				break;
			default:
				if (ValueInput != null){
					ValueInput.Text = ThisSetting.CurrentValue.ToString();
					ValueInput.TextSubmitted += ValueInputOnTextSubmitted;
					ValueInput.SelectAllOnFocus = true;
					ValueInput.FocusExited += ValueInputOnFocusExit;
					ValueInput.TooltipText = ThisSetting.DisplayName + "\n";
					ValueInput.TooltipText = "Any Changes will be validated and saved automatically.\n";
					ValueInput.TooltipText += "Default Value = " + ThisSetting.DefaultValue + "\n";
					ValueInput.TooltipText += "Current Value = " + ThisSetting.CurrentValue + "\n";
					if (UseRange){
						ValueInput.TooltipText += "Minimum Value = " + ThisSetting.minValue + "\n";
						ValueInput.TooltipText += "Maximum Value = " + ThisSetting.maxValue + "\n";
					}
				}

				break;
		}

		DefaultValue.Text = ThisSetting.DefaultValue.ToString();

		DisplaySetting();
	}

	private void ValueInputOnFocusExit(){
		ValueInputOnTextSubmitted(ValueInput.Text);
	}

	private void ValueInputOnTextSubmitted(string newtext){
		ValueInput.TooltipText = "Any Changes will be validated and saved automatically.\n";
		ValueInput.TooltipText += "Default Value = " + ThisSetting.DefaultValue + "\n";
		ValueInput.TooltipText += "Current Value = " + ThisSetting.CurrentValue + "\n";

		if (UseRange){
			ValueInput.TooltipText += "Minimum Value = " + ThisSetting.minValue + "\n";
			ValueInput.TooltipText += "Maximum Value = " + ThisSetting.maxValue + "\n";
			UseRange = true;
		}

		switch (ThisSetting.SettingType){
			case "int":
				try{
					int val = newtext.ToInt();
					if (UseRange){
						if (val <= (int)ThisSetting.maxValue && val >= (int)ThisSetting.minValue)
							ThisSetting.CurrentValue = val;
						else
							ValueInput.Text = ThisSetting.CurrentValue.ToString();
					}
					else{
						ThisSetting.CurrentValue = val;
					}
				}
				catch (Exception e){
					Logging.Log(ThisSetting.SettingName + ": Setting Value Conversion error for INT Value.\n" +
					            e.InnerException);
					ValueInput.Text = ThisSetting.CurrentValue.ToString();
				}

				break;
			case "float":
				try{
					float val = newtext.ToInt();
					if (UseRange){
						if (val <= (float)ThisSetting.maxValue && val >= (float)ThisSetting.minValue)
							ThisSetting.CurrentValue = val;
						else
							ValueInput.Text = ThisSetting.CurrentValue.ToString();
					}
					else{
						ThisSetting.CurrentValue = val;
					}
				}
				catch (Exception e){
					Logging.Log(ThisSetting.SettingName + ": Setting Value Conversion error for FLOAT Value.\n" +
					            e.InnerException);
					ValueInput.Text = ThisSetting.CurrentValue.ToString();
				}

				break;
			case "string":
				ThisSetting.CurrentValue = newtext.ToLower();
				break;
		}
	}

	private void ValueToggleOnToggled(bool buttonpressed){
		togVal = buttonpressed;
		if (togVal){
			ThisSetting.CurrentValue = "true";
			ValueToggle.Text = "TRUE";
		}
		else{
			ThisSetting.CurrentValue = "false";
			ValueToggle.Text = "FALSE";
		}
	}

	private void ValueDropdownOnItemSelected(long index){
		ThisSetting.CurrentValue = values[index];
		if (DEBUG) Logging.Log("Dropdown Value changed to: " + ThisSetting.CurrentValue);
	}

	public void DisplaySetting(){
		// this is simple - hide the display value and show the editable version.

		if (UseRange)
			ValueRange.Visible = true;
		else
			ValueRange.Visible = false;

		switch (ThisSetting.SettingType){
			case "bool":
				ValueToggle.Visible = true;
				break;
			case "enum":
				ValueDropdown.Visible = true;
				break;
			default:
				ValueInput.Visible = true;
				break;
		}
	}

	internal void Remove(){
		QueueFree();
	}
}