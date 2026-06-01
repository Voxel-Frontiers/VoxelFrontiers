#region

using System;
using System.Collections;

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

public class Coroutine{
	private bool isRunning = false;

	public event Action OnCleanup;

	public bool IsRunning => isRunning;

	internal IEnumerator Enumerator;

	public Coroutine(IEnumerator enumerator, Action onCompleteCallback = null){
		Enumerator = enumerator;
		OnCleanup = onCompleteCallback;
	}

	internal void Start(){
		isRunning = true;
	}

	public void InvokeCleanup(){
		isRunning = false;
		OnCleanup?.Invoke();
	}

	// Called from the caller code, not the manager code, to stop the coroutine. Extra, and probably over-engineering.
	public void Stop(){
		isRunning = false;
	}
}