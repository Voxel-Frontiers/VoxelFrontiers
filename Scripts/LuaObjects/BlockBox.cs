namespace ApophisSoftware.LuaObjects;

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

/* NodeBox definition...
 * {
   -- A normal cube; the default in most things
   type = "regular"
   }
   {
   -- A fixed box (or boxes) (facedir param2 is used, if applicable)
   type = "fixed",
   fixed = box OR {box1, box2, ...}
   }
   {
   -- A variable height box (or boxes) with the top face position defined
   -- by the node parameter 'leveled = ', or if 'paramtype2 == "leveled"'
   -- by param2.
   -- Other faces are defined by 'fixed = {}' as with 'type = "fixed"'.
   type = "leveled",
   fixed = box OR {box1, box2, ...}
   }
   {
   -- A box like the selection box for torches
   -- (wallmounted param2 is used, if applicable)
   type = "wallmounted",
   wall_top = box,
   wall_bottom = box,
   wall_side = box
   }
   {
   -- A node that has optional boxes depending on neighboring nodes'
   -- presence and type. See also `connects_to`.
   type = "connected",
   fixed = box OR {box1, box2, ...}
   connect_top = box OR {box1, box2, ...}
   connect_bottom = box OR {box1, box2, ...}
   connect_front = box OR {box1, box2, ...}
   connect_left = box OR {box1, box2, ...}
   connect_back = box OR {box1, box2, ...}
   connect_right = box OR {box1, box2, ...}
   -- The following `disconnected_*` boxes are the opposites of the
   -- `connect_*` ones above, i.e. when a node has no suitable neighbor
   -- on the respective side, the corresponding disconnected box is drawn.
   disconnected_top = box OR {box1, box2, ...}
   disconnected_bottom = box OR {box1, box2, ...}
   disconnected_front = box OR {box1, box2, ...}
   disconnected_left = box OR {box1, box2, ...}
   disconnected_back = box OR {box1, box2, ...}
   disconnected_right = box OR {box1, box2, ...}
   disconnected = box OR {box1, box2, ...} -- when there is *no* neighbor
   disconnected_sides = box OR {box1, box2, ...} -- when there are *no*
   -- neighbors to the sides
   }
 */

public class BlockBox{
	private string _type;

	/// <summary>
	/// Type can be: "regular", "fixed", "wallmounted", or "connected".
	/// </summary>
	public string type{
		get{ return _type; }
		set{
			switch (value.ToLower()){
				case "regular":
					break;
				case "fixed":
					break;
				case "wallmounted":
					break;
				case "connected":
					break;
				default:
					Logging.Log("error", "Invalid 'type' given to BlockBox.");
					return;
		//			break;
			}

			_type = value;
		}
	}

	public BoxDef[] _fixed;

	public BoxDef wall_top;
	public BoxDef wall_bottom;
	public BoxDef wall_side;

	public BoxDef[] connect_top;
	public BoxDef[] connect_bottom;
	public BoxDef[] connect_front;
	public BoxDef[] connect_left;
	public BoxDef[] connect_back;
	public BoxDef[] connect_right;
	public BoxDef[] disconnected_top;
	public BoxDef[] disconnected_bottom;
	public BoxDef[] disconnected_front;
	public BoxDef[] disconnected_left;
	public BoxDef[] disconnected_back;
	public BoxDef[] disconnected_right;
	public BoxDef[] disconnected;
	public BoxDef[] disconnected_sides;
}

public class BoxDef{
	public float[] sides; // Changed from private to public
}