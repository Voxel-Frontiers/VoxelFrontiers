#region

using System.Collections.Generic;
using System.Text;
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

public struct GroupCap{
	public string name = "";
	public double[] times = new[]{ 0.0d };
	public int uses = 0;
	public int maxlevel = 0;

	public GroupCap(){
	}

	public override string ToString(){
		StringBuilder sb = new StringBuilder();

		sb.Append(name);
		sb.Append(" = {");
		sb.Append("times = {");
		foreach (double d in times){
			sb.Append(d.ToString());
			sb.Append(", ");
		}

		sb.Append("}, ");
		sb.Append("uses = ");
		sb.Append(uses.ToString());
		sb.Append(", maxlevel = ");
		sb.Append(maxlevel.ToString());
		sb.AppendLine("},");
		return sb.ToString();
	}
}

public partial class ToolsCap : RefCounted{
	/* from Luanti.
	tool_capabilities = {
		full_punch_interval = 1.0,
		max_drop_level = 0,
		groupcaps = {
			-- For example:
			choppy = {times = {2.50, 1.40, 1.00}, uses = 20, maxlevel = 2},
		},
		damage_groups = {groupname = damage},
		-- Damage values must be between -32768 and 32767 (2^15)

		punch_attack_uses = nil,
		-- Amount of uses this tool has for attacking players and entities
		-- by punching them (0 = infinite uses).
			-- For compatibility, this is automatically set from the first
			-- suitable groupcap using the formula "uses * 3^(maxlevel - 1)".
			-- It is recommended to set this explicitly instead of relying on the
		-- fallback behavior.
	},
	*/

	public double full_punch_interval = 1.0d;
	public int max_drop_level = 0;
	public List<GroupCap> groupcaps = new();
	public Godot.Collections.Dictionary<string, int> damage_groups = new Godot.Collections.Dictionary<string, int>();

	public int? punch_attack_uses{
		get => _punch_attack_uses;
		set => _punch_attack_uses = value;
	}

	private int? _punch_attack_uses = null;

	public override string ToString(){
		StringBuilder sb = new StringBuilder();
		sb.Append("full_punch_interval: ");
		sb.AppendLine(full_punch_interval.ToString());
		sb.Append("max_drop_level: ");
		sb.AppendLine(max_drop_level.ToString());
		sb.Append("groupcaps: ");
		sb.AppendLine(groupcaps.ToString());
		sb.Append("damage_groups: ");
		sb.AppendLine(damage_groups.ToString());
		sb.Append("punch_attack_uses: ");
		sb.AppendLine(_punch_attack_uses.ToString());

		sb.AppendLine(base.ToString());
		return sb.ToString();
	}
}