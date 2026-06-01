#region

using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

#endregion

#region License / Copyright

/*
The MIT License (MIT)

Copyright © 2023 Michieal of Apophis Software.
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

#endregion

namespace ApophisSoftware;

public partial class CoroutineManager : Node{
	[ExportGroup("CoRoutine Manager Properties")] [ExportCategory("CoRoutine Manager Settings")] [Export]
	public bool DEBUG = false;

	private List<Coroutine> activeCoroutines = new();

	// Singleton instance
	private static CoroutineManager instance;
	public static CoroutineManager Instance => instance;

	public override void _Ready(){
		if (instance == null)
			instance = this;
		else
			QueueFree(); // Ensure there's only one instance
	}

	public override void _Process(double delta){
		base._Process(delta);
		UpdateCoroutines(delta);
	}

	public Coroutine StartCoroutine(IEnumerator coroutine, Action onCompleteCallback = null){
		var customCoroutine = new Coroutine(coroutine, onCompleteCallback);
		customCoroutine.Start();
		activeCoroutines.Add(customCoroutine);
		return customCoroutine;
	}

	public void StopCoroutine(Coroutine customCoroutine){
		if (activeCoroutines.Contains(customCoroutine)){
			activeCoroutines.Remove(customCoroutine);
			customCoroutine.InvokeCleanup();
		}
	}

	public void StopAllCoroutines(){
		foreach (var customCoroutine in activeCoroutines) customCoroutine.InvokeCleanup();

		activeCoroutines.Clear();
	}

	public void UpdateCoroutines(double delta){
		for (int i = activeCoroutines.Count - 1; i >= 0; i--){
			var customCoroutine = activeCoroutines[i];
			if (customCoroutine.IsRunning){
				if (customCoroutine.Enumerator.Current is WaitForSeconds){
					var waitForSeconds = (WaitForSeconds)customCoroutine.Enumerator.Current;
					if (waitForSeconds.Started){
						if (waitForSeconds.Update(delta))
							// if it's done waiting, then continue processing and move it to the next step.
							waitForSeconds.Stop();
						else
							continue; // if it's not done waiting, then skip this one. 
					}
				}

				if (customCoroutine.Enumerator.MoveNext()){
					if (customCoroutine.Enumerator.Current is WaitForSeconds){
						var waitForSeconds = (WaitForSeconds)customCoroutine.Enumerator.Current;
						if (!waitForSeconds.Started) waitForSeconds.Start(); // Start timing
					}
					else if (customCoroutine.Enumerator.Current is WaitOneFrame){
						var waitOneFrame = (WaitOneFrame)customCoroutine.Enumerator.Current;
						if (waitOneFrame.Update())
							// The WaitOneFrame has completed, remove it
							customCoroutine.Enumerator.MoveNext(); // Move to the next line of the coroutine
					}
				}
				else{
					// The Coroutine has completed, remove it
					if (DEBUG)
						GD.Print("Removing Completed CoRoutine.");
					activeCoroutines.RemoveAt(i);
					if (DEBUG)
						GD.Print("Invoking CoRoutine Cleanup.");
					customCoroutine.InvokeCleanup();
				}
			}
			else{
				// Coroutine was manually stopped, remove it
				if (DEBUG)
					GD.Print("Removing Manually Stopped CoRoutine.");
				activeCoroutines.RemoveAt(i);
				if (DEBUG)
					GD.Print("Invoking CoRoutine Cleanup.");
				customCoroutine.InvokeCleanup();
			}
		}
	}

	public override void _Notification(int what){
		if (what == NotificationWMCloseRequest) StopAllCoroutines(); // Stop all coroutines on exit.
	}
}

//-----------------------------------------------------------------------------------------
public class WaitForSeconds{
	internal double duration;
	private double startTime;

	internal bool Started;
	internal double CurrentTime = 0d;

	internal double Duration => duration;

	public WaitForSeconds(double seconds){
		duration = seconds;
		startTime = 0; // Initialize to 0
	}

	public void Start(){
		CurrentTime = 0d; // reset to zero.
		Started = true;
		startTime = CurrentTime; // Start timing when called
	}

	public void Stop(){
		Started = false;
	}

	public bool Update(double delta){
		CurrentTime += delta; // Get current time in seconds
		return CurrentTime - startTime >= duration;
	}
}

public class WaitOneFrame{
	private bool hasWaited = false;

	public bool Update(){
		if (!hasWaited){
			hasWaited = true;
			return false; // Continue processing in the next frame
		}
		else{
			return true; // Waiting is done; continue processing
		}
	}
}