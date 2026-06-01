#region

using System.Collections.Generic;
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

namespace ApophisSoftware.LuaObjects;

public partial class MetaData : RefCounted{
	internal Dictionary<string, string> metaData = new();

	public Variant[] lua_fields(){
		return new Variant[]{ "metaData" };
	}

	public void setstring(string key, string value){
		// stack:get_meta():set_string("description", "My worn out pick")

		if (metaData.ContainsKey(key))
			metaData[key] = value;
		else
			metaData.Add(key, value);
	}

	public string getstring(string key, string defaultval){
		if (metaData.ContainsKey(key))
			return metaData[key];
		else
			return defaultval;
	}
}