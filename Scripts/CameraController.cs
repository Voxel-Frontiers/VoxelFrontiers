#region

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

public enum ViewCamera{
	CameraStateNormal = 0,
	CameraStateFront = 1,
	CameraStateBehind = 2
}

public partial class CameraController : Node3D{
	[ExportGroup("Character Camera Properties")] [ExportCategory("Camera Controller Properties")] [Export]
	public Camera3D FirstPersonCamera;

	[Export] public Camera3D ThirdPersonCameraFront;
	[Export] public Camera3D ThirdPersonCameraBehind;

	[Export] public ViewCamera CurrentState;


	//-----------------------------------------------------------------------
	// Private static instance to hold the singleton instance
	private static CameraController _instance;

	// Public property to access the singleton instance
	public static CameraController Instance{
		get{
			if (_instance == null) _instance = new CameraController();

			return _instance;
		}
	}

	// Private constructor to prevent external instantiation
	private CameraController(){
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		FirstPersonCamera.Visible = true;
		ThirdPersonCameraBehind.Visible = false;
		ThirdPersonCameraFront.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta){
	}

	internal void ChangeCurrentCamera(ViewCamera NewState){
		switch (NewState){
			case ViewCamera.CameraStateNormal:
				FirstPersonCamera.Visible = true;
				ThirdPersonCameraBehind.Visible = false;
				ThirdPersonCameraFront.Visible = false;
				break;

			case ViewCamera.CameraStateFront:
				FirstPersonCamera.Visible = false;
				ThirdPersonCameraBehind.Visible = false;
				ThirdPersonCameraFront.Visible = true;
				break;

			case ViewCamera.CameraStateBehind:
				FirstPersonCamera.Visible = false;
				ThirdPersonCameraBehind.Visible = true;
				ThirdPersonCameraFront.Visible = false;
				break;

			default:
				FirstPersonCamera.Visible = true;
				ThirdPersonCameraBehind.Visible = false;
				ThirdPersonCameraFront.Visible = false;
				break;
		}

		CurrentState = NewState;
	}

	internal void ChangeCurrentCamera(){
		switch (CurrentState){
			case ViewCamera.CameraStateNormal:
				FirstPersonCamera.Visible = true;
				ChangeCurrentCamera(ViewCamera.CameraStateBehind);
				CurrentState = ViewCamera.CameraStateBehind;
				break;

			case ViewCamera.CameraStateFront:
				ChangeCurrentCamera(ViewCamera.CameraStateNormal);
				CurrentState = ViewCamera.CameraStateNormal;
				break;

			case ViewCamera.CameraStateBehind:
				ChangeCurrentCamera(ViewCamera.CameraStateFront);
				CurrentState = ViewCamera.CameraStateFront;
				break;

			default:
				ChangeCurrentCamera(ViewCamera.CameraStateNormal);
				break;
		}
	}
}