#region Usings

using System;
using System.IO;
using ApophisSoftware.LuaObjects;
using Godot;

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

public partial class LuaController : Node {
	public  LuaApi   lua = LUA.lua;
	private Callable print;

	// API Objects
	private MCLPP Mclpp = MCLPP.Instance;

	#region Ctor / Dtor

	public LuaController() {
		InitializeThis();
	}

	public override void _ExitTree() {
		lua.Dispose();
		base._ExitTree();
	}

	#endregion

	private void InitializeThis() {
		// define libraries that the lua code has access to. 
		Godot.Collections.Array<string> libraries = new() {
			"base",  // Base Lua commands
			"debug", // Debug.
			"math",  // Math Functions.
			"utf8",  // UTF-8 specific.
			"table", // Table functionality.
			"string" // String Specific functionality.
		};
		lua.BindLibraries(libraries);

		//override the built in lua 'print' function to use the logging.
		print = new Callable(this, MethodName.LuaPrint);
		lua.PushVariant("print", print);

		Logging.Log("info", "Initializing Game API.");
		// try to register the api for the game.
		RegisterAPI();
	}

	private void LuaPrint(string Message) {
		Logging.Log("[LUA]: " + Message);
	}

	internal void Dofile(string Filename) {
		lua.DoFile(Filename);
	}

	internal void CreateGlobalVar(string VariableName, string VariableValue) {
		// Assign a named Lua Variable and give it a value.
		lua.PushVariant(VariableName, VariableValue);
	}

	internal void CreateGlobalVar(string VariableName, GodotObject VariableValue) {
		// Assign a named Lua Variable and give it a value.
		lua.PushVariant(VariableName, VariableValue);
	}

	internal void RegisterAPI() {
		// Removed in favor of just aliasing it in the builtin.lua code.
		// CreateGlobalVar("minetest", Minetest);
		CreateGlobalVar("mclpp", Mclpp);
		Item _item = new Item();
		var _item_ = new Callable(_item, Item.MethodName.CreateItem);
		LuaCallableExtra item_wtuple = LuaCallableExtra.WithTuple(_item_, 0);

		CreateGlobalVar("Item", _item);
		LuaError error = lua.PushVariant("Item", item_wtuple);
		if (Utils.TestForError(error)) {
			Logging.Log("error", "Couldn't Push Item(name) creation function in Lua Code.");
		}

		NodeBlock _NodeBlock = new NodeBlock();
		var _NodeBlock_ = new Callable(_NodeBlock, NodeBlock.MethodName.CreateNodeBlock);
		LuaCallableExtra nb_wtuple = LuaCallableExtra.WithTuple(_NodeBlock_, 1);

		CreateGlobalVar("Node", _NodeBlock);
		error = lua.PushVariant("Node", nb_wtuple);
		if (Utils.TestForError(error)) {
			Logging.Log("error", "Couldn't Push Node(name) creation function in Lua Code.");
		}

		ItemStack _itemStack = new();
		var _itemStack_ = new Callable(_itemStack, ItemStack.MethodName.CreateItemStack);
		LuaCallableExtra is_wtuple = LuaCallableExtra.WithTuple(_itemStack_, 1);

		CreateGlobalVar("ItemStack", _itemStack);
		error = lua.PushVariant("ItemStack", is_wtuple);
		if (Utils.TestForError(error)) {
			Logging.Log("error", "Couldn't Push ItemStack(name) creation function in Lua Code.");
		}
	}

	public override void _Ready() {
		Variant error = lua.DoString(@"
					mclpp.log (""system"" , ""Lua System Ready."")
				");
		try {
			LuaError _Error = (LuaError) error;

			if (_Error != null && _Error.Message != "") {
				Logging.Log("error", "An error occurred calling DoString.");
				Logging.Log("error", "ERROR " + _Error.Type + ": " + _Error.Message);
			}
		} catch (Exception e) {
			// Do Nothing. This is just to make sure that casting works, and nothing throws a real error. 
			// Done because of the changes made to DoString and DoFile in v2.1-beta11. -MRO
		}

		lua.UseCallables = false;
		Variant BuiltInVar = lua.DoFile("res://Scripts/LuaObjects/Lua/builtin.lua");
		try {
			LuaError BuiltInError = (LuaError) BuiltInVar;

			if (BuiltInError != null && BuiltInError.Message != "") {
				Logging.Log("system", "Error: " + BuiltInError.Type);
				Logging.Log("system", "Error: " + BuiltInError.Message);
			}
		} catch (Exception e) {
			// Do Nothing. This is just to make sure that casting works, and nothing throws a real error. 
			// Done because of the changes made to DoString and DoFile in v2.1-beta11. -MRO
		}
	}

	public override void _Process(double delta) {
		base._Process(delta);
	}

	internal bool IsInPath(string RelativePath) {
		string basePath = Utils.GetStoragePath();
		string fullPath = Path.Combine(basePath, RelativePath);

		// Get the relative path from the base path to the full path
		string relative = Path.GetRelativePath(basePath, fullPath);

		// Check if the relative path is not ".." or starts with "..\"
		bool isInSubdirectory = !relative.Equals("..") && !relative.StartsWith("..\\");

		return isInSubdirectory;
	}
}
