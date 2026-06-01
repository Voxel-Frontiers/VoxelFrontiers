#region

using System.Collections.Generic;
using System.Text;
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

namespace ApophisSoftware.LuaObjects;

/// <summary>
///     Base Class for Items, such as Nodes, Tools, CraftedItem, etc.
///     description = "",
///     -- Can contain new lines. "\n" has to be used as new line character.
///     -- See also: `get_description` in [`ItemStack`]
///     short_description = "",
///     -- Must not contain new lines.
///     -- Defaults to nil.
///     -- Use an [`ItemStack`] to get the short description, e.g.:
///     --   ItemStack(itemname):get_short_description()
///     groups = {},
///     -- key = name, value = rating; rating = &lt;number&gt;.
///     -- If rating not applicable, use 1.
///     -- e.g. {wool = 1, fluffy = 3}
///     --      {soil = 2, outerspace = 1, crumbly = 1}
///     --      {bendy = 2, snappy = 1},
///     --      {hard = 1, metal = 1, spikes = 1}
///     inventory_image = "",
///     -- Texture shown in the inventory GUI
///     -- Defaults to a 3D rendering of the node if left empty.
///     inventory_overlay = "",
///     -- An overlay texture which is not affected by colorization
///     wield_image = "",
///     -- Texture shown when item is held in hand
///     -- Defaults to a 3D rendering of the node if left empty.
///     wield_overlay = "",
///     -- Like inventory_overlay but only used in the same situation as wield_image
///     wield_scale = {x = 1, y = 1, z = 1},
///     -- Scale for the item when held in hand
///     palette = "",
///     -- An image file containing the palette of a node.
///     -- You can set the currently used color as the "palette_index" field of
///     -- the item stack metadata.
///     -- The palette is always stretched to fit indices between 0 and 255, to
///     -- ensure compatibility with "colorfacedir" (and similar) nodes.
///     color = "#ffffffff",
///     -- Color the item is colorized with. The palette overrides this.
///     stack_max = 99,
///     -- Maximum amount of items that can be in a single stack.
///     -- The default can be changed by the setting `default_stack_max`
///     range = 4.0,
///     -- Range of node and object pointing that is possible with this item held
///     liquids_pointable = false,
///     -- If true, item can point to all liquid nodes (`liquidtype ~= "none"`),
///     -- even those for which `pointable = false`
///     light_source = 0,
///     -- When used for nodes: Defines amount of light emitted by node.
///     -- Otherwise: Defines texture glow when viewed as a dropped item
///     -- To set the maximum (14), use the value 'minetest.LIGHT_MAX'.
///     -- A value outside the range 0 to minetest.LIGHT_MAX causes undefined
///     -- behavior.
///     -- See "Tool Capabilities" section for an example including explanation
///     tool_capabilities = {
///     full_punch_interval = 1.0,
///     max_drop_level = 0,
///     groupcaps = {
///     -- For example:
///     choppy = {times = {2.50, 1.40, 1.00}, uses = 20, maxlevel = 2},
///     },
///     damage_groups = {groupname = damage},
///     -- Damage values must be between -32768 and 32767 (2^15)
///     punch_attack_uses = nil,
///     -- Amount of uses this tool has for attacking players and entities
///     -- by punching them (0 = infinite uses).
///     -- For compatibility, this is automatically set from the first
///     -- suitable groupcap using the formula "uses * 3^(maxlevel - 1)".
///     -- It is recommend to set this explicitly instead of relying on the
///     -- fallback behavior.
///     },
///     node_placement_prediction = nil,
///     -- If nil and item is node, prediction is made automatically.
///     -- If nil and item is not a node, no prediction is made.
///     -- If "" and item is anything, no prediction is made.
///     -- Otherwise should be name of node which the client immediately places
///     -- on ground when the player places the item. Server will always update
///     -- with actual result shortly.
///     node_dig_prediction = "air",
///     -- if "", no prediction is made.
///     -- if "air", node is removed.
///     -- Otherwise should be name of node which the client immediately places
///     -- upon digging. Server will always update with actual result shortly.
///     sound = {
///     -- Definition of item sounds to be played at various events.
///     -- All fields in this table are optional.
///     breaks = &lt;SimpleSoundSpec&gt;,
///     -- When tool breaks due to wear. Ignored for non-tools
///     eat = &lt;SimpleSoundSpec&gt;,
///     -- When item is eaten with `minetest.do_item_eat`
///     punch_use = &lt;SimpleSoundSpec&gt;,
///     -- When item is used with the 'punch/mine' key pointing at a node or entity
///     punch_use_air = &lt;SimpleSoundSpec&gt;,
///     -- When item is used with the 'punch/mine' key pointing at nothing (air)
///     },
///     on_place = function(itemstack, placer, pointed_thing),
///     -- When the 'place' key was pressed with the item in hand
///     -- and a node was pointed at.
///     -- Shall place item and return the leftover itemstack
///     -- or nil to not modify the inventory.
///     -- The placer may be any ObjectRef or nil.
///     -- default: minetest.item_place
///     on_secondary_use = function(itemstack, user, pointed_thing),
///     -- Same as on_place but called when not pointing at a node.
///     -- Function must return either nil if inventory shall not be modified,
///     -- or an itemstack to replace the original itemstack.
///     -- The user may be any ObjectRef or nil.
///     -- default: nil
///     on_drop = function(itemstack, dropper, pos),
///     -- Shall drop item and return the leftover itemstack.
///     -- The dropper may be any ObjectRef or nil.
///     -- default: minetest.item_drop
///     on_pickup = function(itemstack, picker, pointed_thing, time_from_last_punch, ...),
///     -- Called when a dropped item is punched by a player.
///     -- Shall pick-up the item and return the leftover itemstack or nil to not
///     -- modify the dropped item.
///     -- Parameters:
///     -- * `itemstack`: The `ItemStack` to be picked up.
///     -- * `picker`: Any `ObjectRef` or `nil`.
///     -- * `pointed_thing` (optional): The dropped item (a `"__builtin:item"`
///     --   luaentity) as `type="object"` `pointed_thing`.
///     -- * `time_from_last_punch, ...` (optional): Other parameters from
///     --   `luaentity:on_punch`.
///     -- default: `minetest.item_pickup`
///     on_use = function(itemstack, user, pointed_thing),
///     -- default: nil
///     -- When user pressed the 'punch/mine' key with the item in hand.
///     -- Function must return either nil if inventory shall not be modified,
///     -- or an itemstack to replace the original itemstack.
///     -- e.g. itemstack:take_item(); return itemstack
///     -- Otherwise, the function is free to do what it wants.
///     -- The user may be any ObjectRef or nil.
///     -- The default functions handle regular use cases.
///     after_use = function(itemstack, user, node, digparams),
///     -- default: nil
///     -- If defined, should return an itemstack and will be called instead of
///     -- wearing out the item (if tool). If returns nil, does nothing.
///     -- If after_use doesn't exist, it is the same as:
///     --   function(itemstack, user, node, digparams)
///     --     itemstack:add_wear(digparams.wear)
///     --     return itemstack
///     --   end
///     -- The user may be any ObjectRef or nil.
///     _custom_field = whatever,
///     -- Add your own custom fields. By convention, all custom field names
///     -- should start with `_` to avoid naming collisions with future engine
///     -- usage.
/// </summary>
public partial class Item : RefCounted {
	#region CTOR

	public Item(string _Name) {
		if (_Name == "")
			name = "unknown";
		else
			name = _Name;
	}

	public Item() {
		name = "unknown";
	}

	public Item CreateItem(LuaTuple args) {
		Item _item = new Item();

		if (args != null && !args.IsEmpty()) {
			_item.name = (string) args.ToArray()[0];
		}

		return _item;
	}

	public Variant __index(LuaApi _lua, Variant index) {
		if (Get((string) index).Obj != null)
			// is true if it is return self.Get(index) else return the dictionary index
			return Get((string) index);

		if (user_def.ContainsKey((string) index))
			return user_def[(string) index];
		else
			return "Unknown property: " + index;
	}

	private LuaError __newindex(LuaApi _lua, Variant index, Variant value) {
		LuaError error;

		if (Get((string) index).Obj != null) {
			// is true if it is return self.Get(index) else return the dictionary index
			Set((string) index, value);
			return null;
		}

		if (user_def.ContainsKey((string) index)) {
			string errmsg = "Error: Cannot add existing index key: " + (string) index;
			errmsg += " on object: " + name;
			errmsg += "\nUse .set_custom_field({\"field_name\", value}) instead.";
			error = LuaError.NewError(errmsg, LuaError.ErrorType.Runtime);
		} else {
			user_def.Add((string) index, value); // add in the "index" with the value.
			error = null;
		}

		return error;
	}

	#endregion

	public string  name;
	public string  inventory_image   = "";
	public string  inventory_overlay = "";
	public string  wield_image       = "";
	public string  wield_overlay     = "";
	public Vector3 wield_scale       = new(1, 1, 1); // default to 1.0 scale on all axes.
	public string  palette           = "";
	public string  color             = "#ffffffff"; // hex-code color representation. aka ColorSpec

	public string description {
		get => metaData.getstring("description", "");
		set => metaData.setstring("description", value);
	}

	public string   short_description = "";
	public string[] tiles             = new[] {"", "", "", "", "", ""}; // +Y, -Y, +X, -X, +Z, -Z
	public string[] overlay_tiles     = new[] {"", "", "", "", "", ""};

	public string[]
		special_tiles = new[] {"", "", "", "", "", ""}; //special_tiles = {tile definition 1, Tile definition 2}

	public Godot.Collections.Dictionary<string, int>
		groups =
			new Godot.Collections.Dictionary<string, int>(); // ex. groups = {handy = 1, axey = 1, choppy = 1, dig_by_piston = 1, plant = 1, non_mycelium_plant = 1, flammable = 3},

	/* sound = {
	   -- Definition of item sounds to be played at various events.
	   -- All fields in this table are optional.

	   breaks = <SimpleSoundSpec>,
	   -- When tool breaks due to wear. Ignored for non-tools

	   eat = <SimpleSoundSpec>,
	   -- When item is eaten with `minetest.do_item_eat`

	   punch_use = <SimpleSoundSpec>,
	   -- When item is used with the 'punch/mine' key pointing at a node or entity

	   punch_use_air = <SimpleSoundSpec>,
	   -- When item is used with the 'punch/mine' key pointing at nothing (air)
	   },
	*/
	public string[] sounds; // TODO: Implement this.

	// images are by default, png files. this is their placeholders.
	public int stack_max = 99;
	public float range = 4.0f;
	public bool liquids_pointable = false;
#nullable enable // used to allow node_placement_prediction to be null, and not garbage collected.
	public string? node_placement_prediction = null; // NYI
#nullable disable
	public string                                        node_dig_prediction = "air"; // NYI
	public LuaFunctionRef                                on_place;                    // On_Place Function Code.
	public LuaFunctionRef                                on_secondary_use;
	public LuaFunctionRef                                on_drop;
	public LuaFunctionRef                                on_pickup;
	public LuaFunctionRef                                on_use;
	public LuaFunctionRef                                after_use;
	public ToolsCap                                      tool_capabilities = new();
	public Godot.Collections.Dictionary<string, Variant> user_def          = new();

	public string liquid_alternative_flowing = ""; // liquid_alternative_flowing = "example:water_flowing"
	public string liquid_alternative_source  = ""; // liquid_alternative_source = "example:water_source"


	public void set_custom_field(Variant _KeyValPair) {
		Godot.Collections.Dictionary<string, Variant> KVP = (Godot.Collections.Dictionary<string, Variant>) _KeyValPair;
		foreach (KeyValuePair<string, Variant> keyValuePair in KVP)
			if (user_def.ContainsKey(keyValuePair.Key))
				user_def[keyValuePair.Key] =
					keyValuePair.Value; // assign the new value to the key. ie, do an overwrite.
			else
				user_def.Add(keyValuePair.Key, keyValuePair.Value);
	}

	public Variant get_custom_field(string Key) {
		if (user_def.ContainsKey(Key))
			return user_def[Key];
		else
			return new Variant();
	}


// Private Property field variables / backing.
	private int?   _move_resistance = 0;
	private int    _lightsource     = 0;
	private string _liquidtype      = "none"; //  -- specifies liquid flowing physics
	private bool   _floodable       = false;

	public bool floodable {
		get => _floodable;
		set {
			if (_liquidtype != "none")
				_floodable = false;
			else
				_floodable = value;
		}
	}

	/// <summary>
	///     -- * "none":    no liquid flowing physics
	///     -- * "source":  spawns flowing liquid nodes at all 4 sides and below;
	///     --              recommended drawtype: "liquid".
	///     -- * "flowing": spawned from source, spawns more flowing liquid nodes
	///     --              around it until `liquid_range` is reached;
	///     --              will drain out without a source;
	///     --              recommended drawtype: "flowingliquid".
	///     -- If it's "source" or "flowing", then the
	///     -- `liquid_alternative_*` fields _must_ be specified
	/// </summary>
	public string liquidtype {
		get => _liquidtype;
		set {
			switch (value.ToLower()) {
				case "none":
					_liquidtype = "none";
					break;
				case "source":
					_liquidtype = "source";
					floodable = false;
					break;
				case "flowing":
					_liquidtype = "flowing";
					floodable = false;
					break;
				default:
					_liquidtype = "none";
					break;
			}
		}
	}

	public int light_source {
		get => _lightsource;
		set {
			_lightsource = value;
			if (_lightsource < 0)
				_lightsource = 0;
			if (_lightsource > VF.Instance.LIGHT_MAX) _lightsource = VF.Instance.LIGHT_MAX;
		}
	}

	public int? move_resistance {
		get => _move_resistance;
		set {
			if (value == null) {
				_move_resistance = 1; //TODO: Make this set to flow resistance.
			} else {
				_move_resistance = value;
				if (_move_resistance > 7) _move_resistance = 7;
			}
		}
	}

	// ---------------------------------------------
	private MetaData metaData = new();

	public MetaData getmeta() {
		return metaData;
	}

	public string get_short_description() {
		short_description = metaData.getstring("short_description", "");
		return short_description;
	}

	public override string ToString() {
		StringBuilder sb = new StringBuilder();
		sb.Append("Item Class Object: ");
		sb.Append(name);
		sb.Append(" / ");
		sb.AppendLine(base.ToString());
		sb.AppendLine("====================================");
		sb.Append("description: ");
		sb.AppendLine(description.ToString());
		sb.Append("short_description: ");
		sb.AppendLine(short_description.ToString());
		sb.Append("inventory_image: ");
		sb.AppendLine(inventory_image.ToString());
		sb.Append("inventory_overlay: ");
		sb.AppendLine(inventory_overlay.ToString());
		sb.Append("wield_image: ");
		sb.AppendLine(wield_image.ToString());
		sb.Append("wield_overlay: ");
		sb.AppendLine(wield_overlay.ToString());
		sb.Append("wield_scale: ");
		sb.AppendLine(wield_scale.ToString());
		sb.Append("palette:");
		sb.AppendLine(palette.ToString());
		sb.Append("color: ");
		sb.AppendLine(color.ToString());

		sb.AppendLine("groups: {");
		foreach (var val in groups) {
			sb.Append("\t\"");
			sb.Append(val.Key + "\" = ");
			sb.AppendLine("\"" + val.Value + "\"");
		}

		sb.AppendLine("}");

		sb.AppendLine("tiles: {");
		foreach (string str in tiles) {
			sb.Append("\"");
			sb.Append(str);
			sb.Append("\",");
		}

		sb.AppendLine();
		sb.AppendLine("}");

		sb.AppendLine("overlay_tiles: {");
		foreach (string str in overlay_tiles) {
			sb.Append("\"");
			sb.Append(str);
			sb.Append("\",");
		}

		sb.AppendLine();
		sb.AppendLine("}");

		sb.AppendLine("special_tiles: {");
		foreach (string str in special_tiles) {
			sb.Append("\"");
			sb.Append(str);
			sb.Append("\",");
		}

		sb.AppendLine();
		sb.AppendLine("}");
		sb.AppendLine("sounds: {");
		/*
		foreach (string s in sounds) {
			sb.Append("\"");
			sb.AppendLine(s);
			sb.Append("\"");
		}
		*/
		sb.AppendLine("NYI.");
		sb.AppendLine("}");
		sb.Append("stack_max: ");
		sb.AppendLine(stack_max.ToString());
		sb.Append("range: ");
		sb.AppendLine(range.ToString());
		sb.Append("liquids_pointable: ");
		sb.AppendLine(liquids_pointable.ToString());
		sb.Append("node_placement_prediction: ");
		sb.AppendLine(node_placement_prediction);
		sb.Append("node_dig_prediction: ");
		sb.AppendLine(node_dig_prediction);
		sb.Append("floodable: ");
		sb.AppendLine(floodable.ToString());
		sb.Append("liquidtype: ");
		sb.AppendLine(liquidtype);
		sb.Append("light_source: ");
		sb.AppendLine(light_source.ToString());
		sb.Append("move_resistance: ");
		sb.AppendLine(move_resistance.ToString());
		sb.Append("liquid_alternative_flowing: ");
		sb.AppendLine(liquid_alternative_flowing);
		sb.Append("liquid_alternative_source: ");
		sb.AppendLine(liquid_alternative_source);

		sb.Append("[callback-function]on_place: ");
		if (on_place == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_place.ToString());

		sb.Append("[callback-function]on_secondary_use: ");
		if (on_secondary_use == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_secondary_use.ToString());

		sb.Append("[callback-function]on_drop: ");
		if (on_drop == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_drop.ToString());

		sb.Append("[callback-function]on_pickup: ");
		if (on_pickup == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_pickup.ToString());

		sb.Append("[callback-function]on_use: ");
		if (on_use == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_use.ToString());

		sb.Append("[callback-function]after_use: ");
		if (after_use == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(after_use.ToString());

		sb.Append("tool_capabilities: ");
		sb.AppendLine(tool_capabilities.ToString());

		sb.AppendLine("[Custom Properties Dictionary]: {");
		foreach (var kvp in user_def) {
			sb.Append("\"");
			sb.Append(kvp.Key);
			sb.Append(" = ");
			sb.Append(kvp.Value.ToString());
			sb.AppendLine("\"");
		}

		sb.AppendLine("}");
		sb.AppendLine("Meta Data: {");
		foreach (var kvp in metaData.metaData) {
			sb.Append("\"");
			sb.Append(kvp.Key);
			sb.Append(" = ");
			sb.Append(kvp.Value.ToString());
			sb.AppendLine("\"");
		}

		sb.AppendLine("}");

		sb.AppendLine("====================================");
		sb.AppendLine("[API Functions] ");
		sb.AppendLine("Item(name_of_item) : Constructor; Returns a new Item of (name). ");
		sb.AppendLine("get_short_description(): returns the short description value (string).");
		sb.AppendLine("getmeta() : returns the metadata object for this object.");
		sb.AppendLine("set_custom_field({key,value}) : {string, string}.");
		sb.AppendLine("get_custom_field(key) : string; returns value of the field.");

		return sb.ToString();
	}
}