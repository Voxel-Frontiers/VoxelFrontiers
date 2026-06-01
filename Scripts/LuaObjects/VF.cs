#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

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

public partial class VF : RefCounted {
	private string _mod_name = "";

	private readonly System.Collections.Generic.Dictionary<string, Coroutine> ABMs = new();

	private bool DEBUG = SCC.Instance.DEBUG;

	private readonly System.Collections.Generic.Dictionary<string, lbm> LBMs = new();

	public int LIGHT_MAX = 15;

	private Godot.Collections.Dictionary<string, string> registered_aliases = new();

	public Godot.Collections.Dictionary<string, Variant> registered_crafts = new();

	public Godot.Collections.Dictionary<string, Item> registered_items = new();

	public Godot.Collections.Dictionary<string, NodeBlock> registered_nodes = new();

	public Godot.Collections.Dictionary<string, Variant>
		registered_tools = new(); // TODO: Change this once Tools are made.

	// --- New: Reference to the TerrainGenerator ---
	public TerrainGenerator TerrainGeneratorInstance { get; set; }
	// ---------------------------------------------

	public bool log(string text) {
		Logging.Log(text);
		return true;
		/*
		minetest.debug(...)
		Equivalent to minetest.log(table.concat({...}, "\t"))
		minetest.log([level,] text)
		level is one of "none", "error", "warning", "action", "info", or "verbose". Default is "none".
		*/
	}

	public bool log(string level, string text) {
		if (level.ToLower() == "none") {
			log(text);
			return true;
		}

		Logging.Log(level, text);
		return true;
	}

	public void debug(string text) {
		Logging.Log(text);
	}

	public string get_modpath(string module_name) {
		if (SCC.Instance.ModPaths.ContainsKey(module_name) == false) return null;

		return SCC.Instance.ModPaths[module_name];
	}

	public void set_current_modname(string _ModName) {
		_mod_name = _ModName;
	}

	public string get_current_modname() {
		return _mod_name;
	}

	/*
	 *    label = "Lava cooling",
	   -- Descriptive label for profiling purposes (optional).
	   -- Definitions with identical labels will be listed as one.

	   nodenames = {"default:lava_source"},
	   -- Apply `action` function to these nodes.
	   -- `group:groupname` can also be used here.

	   neighbors = {"default:water_source", "default:water_flowing"},
	   -- Only apply `action` to nodes that have one of, or any
	   -- combination of, these neighbors.
	   -- If left out or empty, any neighbor will do.
	   -- `group:groupname` can also be used here.

	   interval = 10.0,
	   -- Operation interval in seconds

	   chance = 50,
	   -- Chance of triggering `action` per-node per-interval is 1.0 / chance

	   min_y = -32768,
	   max_y = 32767,
	   -- min and max height levels where ABM will be processed (inclusive)
	   -- can be used to reduce CPU usage

	   catch_up = true,
	   -- If true, catch-up behavior is enabled: The `chance` value is
	   -- temporarily reduced when returning to an area to simulate time lost
	   -- by the area being unattended. Note that the `chance` value can often
	   -- be reduced to 1.

	   action = function(pos, node, active_object_count, active_object_count_wider),
	   -- Function triggered for each qualifying node.
	   -- `active_object_count` is number of active objects in the node's
	   -- mapblock.
	   -- `active_object_count_wider` is number of active objects in the node's
	   -- mapblock plus all 26 neighboring mapblocks. If any neighboring
	   -- mapblocks are unloaded an estimate is calculated for them based on
	   -- loaded mapblocks.
	 */
	public void register_abm(Godot.Collections.Dictionary<string, Variant> table) {
		string label;
		string[] nodenames;
		var interval = 1.0d;
		var chance = 1;
		LuaFunctionRef action;
		var min_y = -32768;
		var max_y = 32768;
		var catchup = true;
		string[] neighbors = {"default:water_source", "default:water_flowing"};

		Array<Variant> _params = new();

		try {
			label = (string) table["label"];
			nodenames = (string[]) table["nodenames"];
			interval = (double) table["interval"];
			chance = (int) table["chance"];
			action = (LuaFunctionRef) table["action"];
		} catch (Exception e) {
			Logging.Log("error", e);
			return;
		}

		if (table.ContainsKey("min_y")) min_y = (int) table["min_y"];

		if (table.ContainsKey("max_y")) max_y = (int) table["max_y"];

		if (table.ContainsKey("catchup")) catchup = (bool) table["catchup"];

		if (table.ContainsKey("neighbors")) neighbors = (string[]) table["neighbors"];

		// main parameters needed for the coroutine.
		_params.Add(interval);
		_params.Add(chance);
		_params.Add(action);
		_params.Add(label);

		_params.Add(min_y);
		_params.Add(max_y);
		_params.Add(catchup);
		_params.Add(neighbors);

		if (DEBUG) {
			var sbLog = new StringBuilder();
			sbLog.AppendLine("Register ABM Called.");
			sbLog.Append("label: ");
			sbLog.AppendLine(label);
			sbLog.Append("nodename: ");
			foreach (var nodename in nodenames) sbLog.AppendLine(nodename);

			sbLog.Append("interval: ");
			sbLog.AppendLine(interval.ToString());
			sbLog.Append("chance: ");
			sbLog.AppendLine(chance.ToString());
			sbLog.Append("action: ");
			sbLog.AppendLine(action.ToString());

			Logging.Log(sbLog.ToString());
		}

		// ---- Create the coroutine for the abm.
		if (!ABMs.ContainsKey(label)) {
			var x = CoroutineManager.Instance.StartCoroutine(ABMCoRoutine(_params), CleanUpABM);
			ABMs.Add(label, x);
		} else {
			Logging.Log("warning", "Lua Code tried to create an already defined ABM. Name: " + label);
		}
	}

	internal void CleanUpABM() {
		var removeme = new List<string>();

		// scan and remove completed coroutines, so that they can be GC'd.
		foreach (var abm in ABMs)
			if (abm.Value.IsRunning == false)
				removeme.Add(abm.Key);
		foreach (var remove in removeme) ABMs.Remove(remove);

		GC.Collect(); // finally, take out the trash.
	}

	public void register_alias(string alias, string item) {
		if (registered_aliases.ContainsKey(item)) {
			var x = FindItem(registered_aliases[item]);
			switch (x.type) {
				case FindResultType.Node:
					registered_nodes.Remove(registered_aliases[item]);
					registered_nodes.Add(alias, (NodeBlock) x.definition);
					registered_aliases[item] = alias;
					break;
				case FindResultType.Item:
					registered_items.Remove(registered_aliases[item]);
					registered_items.Add(alias, (Item) x.definition);
					registered_aliases[item] = alias;
					break;
				case FindResultType.Tool:
					registered_tools.Remove(registered_aliases[item]);
					registered_tools.Add(alias, x.definition);
					registered_aliases[item] = alias;
					break;
			}

			return;
		}

		//Search Items, Nodes and Tools for the correct entry, then duplicate said entry with the alias' name.
		var def = FindItem(item);
		if (def.success == false) {
			log("error", "Couldn't make alias: " + alias + " for item:" + item);
			return;
		}

		if (def.type == FindResultType.Node)
			registered_nodes.Add(alias, (NodeBlock) def.definition);
		else if (def.type == FindResultType.Item)
			registered_items.Add(alias, (Item) def.definition);
		else if (def.type == FindResultType.Tool) registered_tools.Add(alias, def.definition);

		registered_aliases.Add(item, alias);
	}

	public FindResult FindItem(string itemStackName) {
		var fr = new FindResult();

		foreach (var kvp in registered_nodes)
			if (kvp.Key == itemStackName) {
				fr.success = true;
				fr.identifier = kvp.Key;
				fr.definition = kvp.Value;
				fr.type = FindResultType.Node;
				return fr;
			}

		foreach (var kvp in registered_items)
			if (kvp.Key == itemStackName) {
				fr.success = true;
				fr.identifier = kvp.Key;
				fr.definition = kvp.Value;
				fr.type = FindResultType.Item;
				return fr;
			}

		foreach (var kvp in registered_tools)
			if (kvp.Key == itemStackName) {
				fr.success = true;
				fr.identifier = kvp.Key;
				fr.definition = kvp.Value;
				fr.type = FindResultType.Tool;
				return fr;
			}

		return fr;
	}

/*
 * label = "Upgrade legacy doors",
   -- Descriptive label for profiling purposes (optional).
   -- Definitions with identical labels will be listed as one.

   name = "modname:replace_legacy_door",
   -- Identifier of the LBM, should follow the modname:<whatever> convention

   nodenames = {"default:lava_source"},
   -- List of node names to trigger the LBM on.
   -- Names of non-registered nodes and groups (as group:groupname)
   -- will work as well.

   run_at_every_load = false,
   -- Whether to run the LBM's action every time a block gets activated,
   -- and not only the first time the block gets activated after the LBM
   -- was introduced.

   action = function(pos, node, dtime_s),
   -- Function triggered for each qualifying node.
   -- `dtime_s` is the in-game time (in seconds) elapsed since the block
   -- was last active
 */
	public void register_lbm(Godot.Collections.Dictionary<string, Variant> table) {
		string label;
		string[] nodenames;
		string name;
		bool RunAtEveryLoad;
		var action = new LuaFunctionRef();

		label = (string) table["label"];
		nodenames = (string[]) table["nodenames"];
		name = (string) table["name"];
		RunAtEveryLoad = (bool) table["run_at_every_load"];
		action = (LuaFunctionRef) table["action"];

		var LBM = new lbm();
		LBM.RunAtEveryLoad = RunAtEveryLoad;
		LBM.name = name;
		LBM.action = action;
		LBM.label = label;
		LBM.nodenames = nodenames;

		if (!LBMs.TryAdd(name, LBM)) {
			Logging.Log("warning", "Couldn't add in additional LBM: " + name);
		} else {
			if (DEBUG) Logging.Log("system", "LBM Added: " + name);
		}
	}

/*
 * Item definition

   Used by minetest.register_node, minetest.register_craftitem, and minetest.register_tool.

   {
   description = "",
   -- Can contain new lines. "\n" has to be used as new line character.
   -- See also: `get_description` in [`ItemStack`]

   short_description = "",
   -- Must not contain new lines.
   -- Defaults to nil.
   -- Use an [`ItemStack`] to get the short description, e.g.:
   --   ItemStack(itemname):get_short_description()

   groups = {},
   -- key = name, value = rating; rating = <number>.
   -- If rating not applicable, use 1.
   -- e.g. {wool = 1, fluffy = 3}
   --      {soil = 2, outerspace = 1, crumbly = 1}
   --      {bendy = 2, snappy = 1},
   --      {hard = 1, metal = 1, spikes = 1}

   inventory_image = "",
   -- Texture shown in the inventory GUI
   -- Defaults to a 3D rendering of the node if left empty.

   inventory_overlay = "",
   -- An overlay texture which is not affected by colorization

   wield_image = "",
   -- Texture shown when item is held in hand
   -- Defaults to a 3D rendering of the node if left empty.

   wield_overlay = "",
   -- Like inventory_overlay but only used in the same situation as wield_image

   wield_scale = {x = 1, y = 1, z = 1},
   -- Scale for the item when held in hand

   palette = "",
   -- An image file containing the palette of a node.
   -- You can set the currently used color as the "palette_index" field of
   -- the item stack metadata.
   -- The palette is always stretched to fit indices between 0 and 255, to
   -- ensure compatibility with "colorfacedir" (and similar) nodes.

   color = "#ffffffff",
   -- Color the item is colorized with. The palette overrides this.

   stack_max = 99,
   -- Maximum amount of items that can be in a single stack.
   -- The default can be changed by the setting `default_stack_max`

   range = 4.0,
   -- Range of node and object pointing that is possible with this item held

   liquids_pointable = false,
   -- If true, item can point to all liquid nodes (`liquidtype ~= "none"`),
   -- even those for which `pointable = false`

   light_source = 0,
   -- When used for nodes: Defines amount of light emitted by node.
   -- Otherwise: Defines texture glow when viewed as a dropped item
   -- To set the maximum (14), use the value 'minetest.LIGHT_MAX'.
   -- A value outside the range 0 to minetest.LIGHT_MAX causes undefined
   -- behavior.

   -- See "Tool Capabilities" section for an example including explanation
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
   -- It is recommend to set this explicitly instead of relying on the
   -- fallback behavior.
   },

   node_placement_prediction = nil,
   -- If nil and item is node, prediction is made automatically.
   -- If nil and item is not a node, no prediction is made.
   -- If "" and item is anything, no prediction is made.
   -- Otherwise should be name of node which the client immediately places
   -- on ground when the player places the item. Server will always update
   -- with actual result shortly.

   node_dig_prediction = "air",
   -- if "", no prediction is made.
   -- if "air", node is removed.
   -- Otherwise should be name of node which the client immediately places
   -- upon digging. Server will always update with actual result shortly.

   sound = {
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

   on_place = function(itemstack, placer, pointed_thing),
   -- When the 'place' key was pressed with the item in hand
   -- and a node was pointed at.
   -- Shall place item and return the leftover itemstack
   -- or nil to not modify the inventory.
   -- The placer may be any ObjectRef or nil.
   -- default: minetest.item_place

   on_secondary_use = function(itemstack, user, pointed_thing),
   -- Same as on_place but called when not pointing at a node.
   -- Function must return either nil if inventory shall not be modified,
   -- or an itemstack to replace the original itemstack.
   -- The user may be any ObjectRef or nil.
   -- default: nil

   on_drop = function(itemstack, dropper, pos),
   -- Shall drop item and return the leftover itemstack.
   -- The dropper may be any ObjectRef or nil.
   -- default: minetest.item_drop

   on_pickup = function(itemstack, picker, pointed_thing, time_from_last_punch, ...),
   -- Called when a dropped item is punched by a player.
   -- Shall pick-up the item and return the leftover itemstack or nil to not
   -- modify the dropped item.
   -- Parameters:
   -- * `itemstack`: The `ItemStack` to be picked up.
   -- * `picker`: Any `ObjectRef` or `nil`.
   -- * `pointed_thing` (optional): The dropped item (a `"__builtin:item"`
   --   luaentity) as `type="object"` `pointed_thing`.
   -- * `time_from_last_punch, ...` (optional): Other parameters from
   --   `luaentity:on_punch`.
   -- default: `minetest.item_pickup`

   on_use = function(itemstack, user, pointed_thing),
   -- default: nil
   -- When user pressed the 'punch/mine' key with the item in hand.
   -- Function must return either nil if inventory shall not be modified,
   -- or an itemstack to replace the original itemstack.
   -- e.g. itemstack:take_item(); return itemstack
   -- Otherwise, the function is free to do what it wants.
   -- The user may be any ObjectRef or nil.
   -- The default functions handle regular use cases.

   after_use = function(itemstack, user, node, digparams),
   -- default: nil
   -- If defined, should return an itemstack and will be called instead of
   -- wearing out the item (if tool). If returns nil, does nothing.
   -- If after_use doesn't exist, it is the same as:
   --   function(itemstack, user, node, digparams)
   --     itemstack:add_wear(digparams.wear)
   --     return itemstack
   --   end
   -- The user may be any ObjectRef or nil.

   _custom_field = whatever,
   -- Add your own custom fields. By convention, all custom field names
   -- should start with `_` to avoid naming collisions with future engine
   -- usage.
   }

 */
	public void register_node(string node_name, Variant NodeDefinition) {
		var x = FindItem(node_name);
		if (x.success) {
			log("system",
				"Error processing register_node item: " + node_name + " in module: " + _mod_name +
				".\nNode already registered. Please use `.override_item` to change the node definition.\nRegister_Node failed.");
			return;
		}

		// process table and fill out the node def.
		NodeBlock NewNode = ProcessNode(node_name, NodeDefinition);

		if (NewNode == null) {
			log("error", "Invalid node was attempted to be registered.");
			return;
		}

		// add to dictionary.
		registered_nodes.Add(node_name, NewNode);

		log("Node Registered Successfully:\n", NewNode.ToString());  // test for verifying NodeBlock Changes.
	}

	public void register_craftitem(string item_name, Variant ItemDefinition) {
		var x = FindItem(item_name);
		if (x.success) {
			log("system",
				"Error processing register_craftitem item: " + item_name + " in module: " + _mod_name +
				".\nItem already registered. Please use `.override_item` to change the item definition.\nRegister_Craftitem failed.");
			return;
		}

		// process table and fill out the node def.
		Item NewItem = ProcessItem(item_name, ItemDefinition);

		// add to dictionary.
		registered_items.Add(item_name, NewItem);
	}

	public void register_tool(string tool_name, Variant ToolDefinition) {
		var x = FindItem(tool_name);
		if (x.success) {
			log("system",
				"Error processing register_tool item: " + tool_name + " in module: " + _mod_name +
				".\nTool already registered. Please use `.override_item` to change the tool definition.\nRegister_Tool failed.");
			return;
		}

		// process table and fill out the node def.
		//TODO: seriously clean up tools section
		Item NewItem = ProcessItem(tool_name, ToolDefinition);

		// add to dictionary.
		registered_tools.Add(tool_name, NewItem);
	}

	public void override_item(string item_name, Variant Redefinition) {
		var x = FindItem(item_name);
		if (!x.success) {
			log("system",
				"Error overriding item: " + item_name + " in module: " + _mod_name +
				".\nItem Not Registered. Please use the appropriate register function instead.\nOverride_Item failed.");
			return;
		}

		switch (x.type) {
			case FindResultType.Item:
				registered_items[item_name] = ProcessItem(item_name, Redefinition);
				break;
			case FindResultType.Node:
				registered_nodes[item_name] = ProcessNode(item_name, Redefinition);
				break;
			case FindResultType.Tool:
				registered_tools[item_name] = ProcessTool(item_name, Redefinition);
				break;
			default:
				break;
		}
	}

	private Variant ProcessTool(string itemName, Variant redefinition) {
		throw new NotImplementedException();
	}

	private BlockBox ProcessBox(string NodeName, Godot.Collections.Dictionary<string, Variant> BoxDictionary) {
		BlockBox this_box = new BlockBox();
		if (BoxDictionary.ContainsKey("type") == false) {
			log("error",
				"Node Definition: " + NodeName +
				" In Register Node contains a collision box, but doesn't have `type` defined.");
			return null;
		} else {
			this_box.type = ((string) BoxDictionary["type"]).ToLower();
		}

		switch (this_box.type.ToLower()) {
			case "regular": // do nothing else.
				break;
			case "fixed":
				if (BoxDictionary.ContainsKey("fixed") == false) {
					log("error",
						"Node Definition: " + NodeName +
						" In Register Node contains a fixed collision box, but doesn't have `fixed` defined.");
					return null;
				}

				break;
			case "wallmounted":
				if (BoxDictionary.ContainsKey("wall_top") == false) {
					log("error",
						"Node Definition: " + NodeName +
						" In Register Node contains a fixed collision box, but doesn't have `wall_top` defined.");
					return null;
				}

				if (BoxDictionary.ContainsKey("wall_bottom") == false) {
					log("error",
						"Node Definition: " + NodeName +
						" In Register Node contains a fixed collision box, but doesn't have `wall_bottom` defined.");
					return null;
				}

				if (BoxDictionary.ContainsKey("wall_side") == false) {
					log("error",
						"Node Definition: " + NodeName +
						" In Register Node contains a fixed collision box, but doesn't have `wall_side` defined.");
					return null;
				}

				break;
			case "connected":
				if (BoxDictionary.ContainsKey("fixed") == false) {
					log("error",
						"Node Definition: " + NodeName +
						" In Register Node contains a fixed collision box, but doesn't have `fixed` defined.");
					return null;
				}

				break;
			default:
				Logging.Log("error", "Invalid 'type' given to BlockBox.");
				return null;
				break;
		}


		return this_box;
	}

	private NodeBlock ProcessNode(string NodeName, Variant NodeDef) {
		NodeBlock nodeBlock = new NodeBlock(NodeName);
		Godot.Collections.Dictionary<string, Variant> Nodedef = (Godot.Collections.Dictionary<string, Variant>) NodeDef;

		if (Nodedef.TryGetValue("node_box", out var nvalue)) {
			var _nodebox = (Godot.Collections.Dictionary<string, Variant>) nvalue;

			BlockBox nodeBox = ProcessBox(NodeName, _nodebox);

			if (nodeBox == null) {
				return null;
			}

			nodeBlock.node_box = nodeBox;
		}

		if (Nodedef.TryGetValue("collision_box", out var cvalue)) {
			var collBox = (Godot.Collections.Dictionary<string, Variant>) cvalue;

			BlockBox collisionBox = ProcessBox(NodeName, collBox);

			if (collisionBox == null) {
				return null;
			}

			nodeBlock.collision_box = collisionBox;
		} else {
			if (nodeBlock.node_box != null) {
				nodeBlock.collision_box = nodeBlock.node_box;
			}
		}

		if (Nodedef.TryGetValue("selection_box", out var svalue)) {
			var _selectionbox = (Godot.Collections.Dictionary<string, Variant>) svalue;

			BlockBox selectionBox = ProcessBox(NodeName, _selectionbox);

			if (selectionBox == null) {
				return null;
			}

			nodeBlock.selection_box = selectionBox;
		} else {
			if (nodeBlock.collision_box != null) {
				nodeBlock.selection_box = nodeBlock.collision_box;
			}
		}

		if (Nodedef.TryGetValue("after_destruct", out var value)) {
			nodeBlock.after_destruct = (LuaFunctionRef) value;
		}

		if (Nodedef.TryGetValue("after_dig_node", out value)) {
			nodeBlock.after_dig_node = (LuaFunctionRef) value;
		}

		if (Nodedef.TryGetValue("after_place_node", out value)) {
			nodeBlock.after_place_node = (LuaFunctionRef) value;
		}

		if (Nodedef.TryGetValue("after_use", out value)) {
			nodeBlock.after_use = (LuaFunctionRef) value;
		}

		if (Nodedef.TryGetValue("allow_metadata_inventory_move", out value)) {
			nodeBlock.allow_metadata_inventory_move = (LuaFunctionRef) value;
		}

		if (Nodedef.TryGetValue("allow_metadata_inventory_put", out value)) {
			nodeBlock.allow_metadata_inventory_put = (LuaFunctionRef) value;
		}

		if (Nodedef.TryGetValue("allow_metadata_inventory_move", out value)) {
			nodeBlock.allow_metadata_inventory_move = (LuaFunctionRef) value;
		}

		if (Nodedef.TryGetValue("allow_metadata_inventory_take", out value)) {
			nodeBlock.allow_metadata_inventory_take = (LuaFunctionRef) value;
		}

		if (Nodedef.TryGetValue("buildable_to", out value)) {
			nodeBlock.buildable_to = (bool) value;
		}

		if (Nodedef.TryGetValue("can_dig", out value)) {
			nodeBlock.can_dig = (LuaFunctionRef) value;
		}

		if (Nodedef.TryGetValue("climbable", out value)) {
			nodeBlock.climbable = (bool) value;
		}

		if (Nodedef.TryGetValue("color", out value)) {
			nodeBlock.color = (string) value;
		}

		if (Nodedef.TryGetValue("description", out value)) {
			nodeBlock.description = (string) value;
		}

		if (Nodedef.TryGetValue("diggable", out value)) {
			nodeBlock.diggable = (bool) value;
		}

		if (Nodedef.TryGetValue("drawtype", out value)) {
			nodeBlock.drawtype = (string) value;
		}

		if (Nodedef.TryGetValue("floodable", out value)) {
			nodeBlock.floodable = (bool) value;
		}

		if (Nodedef.TryGetValue("groups", out value)) {
			nodeBlock.groups = (Godot.Collections.Dictionary<string, int>) value;
		}

		if (Nodedef.TryGetValue("inventory_image", out value)) {
			nodeBlock.inventory_image = (string) value;
		}

		if (Nodedef.TryGetValue("inventory_overlay", out value)) {
			nodeBlock.inventory_overlay = (string) value;
		}

		if (Nodedef.TryGetValue("is_ground_content", out value)) {
			nodeBlock.is_ground_content = (bool) value;
		}

		if (Nodedef.TryGetValue("light_source", out value)) {
			nodeBlock.light_source = (int) value;
		}

		if (Nodedef.TryGetValue("liquidtype", out value)) {
			nodeBlock.liquidtype = (string) value;
		}

		if (Nodedef.TryGetValue("liquids_pointable", out value)) {
			nodeBlock.liquids_pointable = (bool) value;
		}

		if (Nodedef.TryGetValue("liquid_alternative_flowing", out value)) {
			nodeBlock.liquid_alternative_flowing = (string) value;
		}

		if (Nodedef.TryGetValue("liquid_alternative_source", out value)) {
			nodeBlock.liquid_alternative_source = (string) value;
		}
		ProcessProperty(Nodedef, "node_box", value => ProcessBox(NodeName, (Godot.Collections.Dictionary<string, Variant>) value), box => nodeBlock.node_box = box);
		ProcessProperty(Nodedef, "collision_box", value => ProcessBox(NodeName, (Godot.Collections.Dictionary<string, Variant>) value), box => nodeBlock.collision_box = box);
		ProcessProperty(Nodedef, "selection_box", value => ProcessBox(NodeName, (Godot.Collections.Dictionary<string, Variant>) value), box => nodeBlock.selection_box = box);
		ProcessProperty(Nodedef, "after_destruct", value => (LuaFunctionRef) value, refValue => nodeBlock.after_destruct = refValue);
		ProcessProperty(Nodedef, "after_dig_node", value => (LuaFunctionRef) value, refValue => nodeBlock.after_dig_node = refValue);
		ProcessProperty(Nodedef, "after_place_node", value => (LuaFunctionRef) value, refValue => nodeBlock.after_place_node = refValue);
		ProcessProperty(Nodedef, "after_use", value => (LuaFunctionRef) value, refValue => nodeBlock.after_use = refValue);
		ProcessProperty(Nodedef, "allow_metadata_inventory_move", value => (LuaFunctionRef) value, refValue => nodeBlock.allow_metadata_inventory_move = refValue);
		ProcessProperty(Nodedef, "allow_metadata_inventory_put", value => (LuaFunctionRef) value, refValue => nodeBlock.allow_metadata_inventory_put = refValue);
		ProcessProperty(Nodedef, "allow_metadata_inventory_take", value => (LuaFunctionRef) value, refValue => nodeBlock.allow_metadata_inventory_take = refValue);
		ProcessProperty(Nodedef, "buildable_to", value => (bool) value, refValue => nodeBlock.buildable_to = refValue);
		ProcessProperty(Nodedef, "can_dig", value => (LuaFunctionRef) value, refValue => nodeBlock.can_dig = refValue);
		ProcessProperty(Nodedef, "climbable", value => (bool) value, refValue => nodeBlock.climbable = refValue);
		ProcessProperty(Nodedef, "color", value => (string) value, refValue => nodeBlock.color = refValue);
		ProcessProperty(Nodedef, "description", value => (string) value, refValue => nodeBlock.description = refValue);
		ProcessProperty(Nodedef, "diggable", value => (bool) value, refValue => nodeBlock.diggable = refValue);
		ProcessProperty(Nodedef, "drawtype", value => (string) value, refValue => nodeBlock.drawtype = refValue);
		ProcessProperty(Nodedef, "floodable", value => (bool) value, refValue => nodeBlock.floodable = refValue);
		ProcessProperty(Nodedef, "groups", value => (Godot.Collections.Dictionary<string, int>) value, refValue => nodeBlock.groups = refValue);
		ProcessProperty(Nodedef, "inventory_image", value => (string) value, refValue => nodeBlock.inventory_image = refValue);
		ProcessProperty(Nodedef, "inventory_overlay", value => (string) value, refValue => nodeBlock.inventory_overlay = refValue);
		ProcessProperty(Nodedef, "is_ground_content", value => (bool) value, refValue => nodeBlock.is_ground_content = refValue);
		ProcessProperty(Nodedef, "light_source", value => (int) value, refValue => nodeBlock.light_source = refValue);
		ProcessProperty(Nodedef, "liquidtype", value => (string) value, refValue => nodeBlock.liquidtype = refValue);
		ProcessProperty(Nodedef, "liquids_pointable", value => (bool) value, refValue => nodeBlock.liquids_pointable = refValue);
		ProcessProperty(Nodedef, "liquid_alternative_flowing", value => (string) value, refValue => nodeBlock.liquid_alternative_flowing = refValue);
		ProcessProperty(Nodedef, "liquid_alternative_source", value => (string) value, refValue => nodeBlock.liquid_alternative_source = refValue);
#nullable enable
		ProcessProperty(Nodedef, "move_resistance", value => (int?) value, refValue => nodeBlock.move_resistance = refValue);
		ProcessProperty(Nodedef, "node_dig_prediction", value => (string) value, refValue => nodeBlock.node_dig_prediction = refValue);
		ProcessProperty(Nodedef, "node_placement_prediction", value => (string?) value, refValue => nodeBlock.node_placement_prediction = refValue);
#nullable disable
		ProcessProperty(Nodedef, "on_blast", value => (LuaFunctionRef) value, refValue => nodeBlock.on_blast = refValue);
		ProcessProperty(Nodedef, "on_construct", value => (LuaFunctionRef) value, refValue => nodeBlock.on_construct = refValue);
		ProcessProperty(Nodedef, "on_destruct", value => (LuaFunctionRef) value, refValue => nodeBlock.on_destruct = refValue);
		ProcessProperty(Nodedef, "on_dig", value => (LuaFunctionRef) value, refValue => nodeBlock.on_dig = refValue);
		ProcessProperty(Nodedef, "on_drop", value => (LuaFunctionRef) value, refValue => nodeBlock.on_drop = refValue);
		ProcessProperty(Nodedef, "on_flood", value => (LuaFunctionRef) value, refValue => nodeBlock.on_flood = refValue);
		ProcessProperty(Nodedef, "on_metadata_inventory_move", value => (LuaFunctionRef) value, refValue => nodeBlock.on_metadata_inventory_move = refValue);
		ProcessProperty(Nodedef, "on_metadata_inventory_put", value => (LuaFunctionRef) value, refValue => nodeBlock.on_metadata_inventory_put = refValue);
		ProcessProperty(Nodedef, "on_metadata_inventory_take", value => (LuaFunctionRef) value, refValue => nodeBlock.on_metadata_inventory_take = refValue);
		ProcessProperty(Nodedef, "on_punch", value => (LuaFunctionRef) value, refValue => nodeBlock.on_punch = refValue);
		ProcessProperty(Nodedef, "on_receive_fields", value => (LuaFunctionRef) value, refValue => nodeBlock.on_receive_fields = refValue);
		ProcessProperty(Nodedef, "on_rightclick", value => (LuaFunctionRef) value, refValue => nodeBlock.on_rightclick = refValue);
		ProcessProperty(Nodedef, "on_secondary_use", value => (LuaFunctionRef) value, refValue => nodeBlock.on_secondary_use = refValue);
		ProcessProperty(Nodedef, "on_timer", value => (LuaFunctionRef) value, refValue => nodeBlock.on_timer = refValue);
		ProcessProperty(Nodedef, "on_use", value => (LuaFunctionRef) value, refValue => nodeBlock.on_use = refValue);
		ProcessProperty(Nodedef, "overlay_tiles", value => (string[]) value, refValue => nodeBlock.overlay_tiles = refValue);
		ProcessProperty(Nodedef, "paramtype", value => (string) value, refValue => nodeBlock.paramtype = refValue);
		ProcessProperty(Nodedef, "paramtype2", value => (string) value, refValue => nodeBlock.paramtype2 = refValue);
		ProcessProperty(Nodedef, "place_param2", value => (int) value, refValue => nodeBlock.place_param2 = refValue);
		ProcessProperty(Nodedef, "pointable", value => (bool) value, refValue => nodeBlock.pointable = refValue);
		ProcessProperty(Nodedef, "post_effect_color", value => (string) value, refValue => nodeBlock.post_effect_color = refValue);
		ProcessProperty(Nodedef, "preserve_metadata", value => (LuaFunctionRef) value, refValue => nodeBlock.preserve_metadata = refValue);
		ProcessProperty(Nodedef, "short_description", value => (string) value, refValue => nodeBlock.short_description = refValue);
		ProcessProperty(Nodedef, "sounds", value => (string[]) value, refValue => nodeBlock.sounds = refValue);
		ProcessProperty(Nodedef, "special_tiles", value => (string[]) value, refValue => nodeBlock.special_tiles = refValue);
		ProcessProperty(Nodedef, "stack_max", value => (int) value, refValue => nodeBlock.stack_max = refValue);
		ProcessProperty(Nodedef, "sunlight_propagates", value => (bool) value, refValue => nodeBlock.sunlight_propagates = refValue);
		ProcessProperty(Nodedef, "tiles", value => (string[]) value, refValue => nodeBlock.tiles = refValue);
		ProcessProperty(Nodedef, "tool_capabilities", value => (ToolsCap) value, refValue => nodeBlock.tool_capabilities = refValue);
		ProcessProperty(Nodedef, "use_texture_alpha", value => (string) value, refValue => nodeBlock.use_texture_alpha = refValue);
		ProcessProperty(Nodedef, "user_def", value => (Godot.Collections.Dictionary<string, Variant>) value, refValue => nodeBlock.user_def = refValue);
		ProcessProperty(Nodedef, "visual_scale", value => (float) value, refValue => nodeBlock.visual_scale = refValue);
		ProcessProperty(Nodedef, "walkable", value => (bool) value, refValue => nodeBlock.walkable = refValue);
		ProcessProperty(Nodedef, "wield_image", value => (string) value, refValue => nodeBlock.wield_image = refValue);
		ProcessProperty(Nodedef, "wield_scale", value => (Vector3) value, refValue => nodeBlock.wield_scale = refValue);
		ProcessProperty(Nodedef, "wield_overlay", value => (string) value, refValue => nodeBlock.wield_overlay = refValue);

		nodeBlock.mod_origin = _mod_name;
		return nodeBlock;
	}

	private void ProcessProperty<T>(Godot.Collections.Dictionary<string, Variant> Nodedef, string key, Func<Variant, T> processValue, Action<T> assignValue) {
		if (Nodedef.TryGetValue(key, out var value)) {
			T processedValue = processValue(value);
			if (processedValue != null) {
				assignValue(processedValue);
			} else {
				GD.Print($"The processed value for the key '{key}' is null.");
			}
		}
	}

	private Item ProcessItem(string itemName, Variant redefinition) {
		throw new NotImplementedException();
	}

	public void unregister_node(string node_name) {
	}

	public void get_translator(string modname) {
	}

	/// <summary>
	/// Retrieves the NodeBlock definition at the specified world coordinates.
	/// This method is intended to be called from Lua.
	/// </summary>
	/// <param name="x">World X coordinate.</param>
	/// <param name="y">World Y coordinate.</param>
	/// <param name="z">World Z coordinate.</param>
	/// <returns>The NodeBlock object at the given coordinates, or null if no TerrainGenerator is set or block is not found.</returns>
	public NodeBlock get_node(int x, int y, int z)
	{
		if (TerrainGeneratorInstance == null)
		{
			log("error", "MCLPP.TerrainGeneratorInstance is not set. Cannot get node data.");
			return null;
		}

		Vector3I worldPos = new Vector3I(x, y, z);
		int blockId = TerrainGeneratorInstance.GetBlockId(worldPos);
		
		// Access NodeRegistry directly from TerrainGeneratorInstance
		NodeRegistry nodeRegistry = TerrainGeneratorInstance.NodeRegistry;
		if (nodeRegistry == null)
		{
			log("error", "TerrainGeneratorInstance.NodeRegistry is null. Cannot get node data.");
			return null;
		}

		NodeRegistry.NodeDefinition nodeDef = nodeRegistry.GetNodeDefinition(blockId);
		if (nodeDef != null)
		{
			return nodeDef.OriginalNodeBlock;
		}
		
		return null; // Block not found or is air
	}


	internal IEnumerator ABMCoRoutine(Array<Variant> _params) {
		var rng = new Random(DateTime.UtcNow.Millisecond);

		if (DEBUG) Logging.Log(_params.ToString());

		var Interval = (double) _params[0];
		var chance = (int) _params[1];
		var action = (LuaFunctionRef) _params[2];
		var label = (string) _params[3];

		var delay = new WaitForSeconds((float) Interval);

		// action = function(pos, node, active_object_count, active_object_count_wider)

		var pos = new Vector3(0, 0, 0);
		var node = new NodeBlock();
		var ActiveObjectCount = 0;
		var ActiveObjectCountWider = 0;

		var Params = new Array();

		Params.Add(pos);
		Params.Add(node);
		Params.Add(ActiveObjectCount); // number of Entities / Objects.
		Params.Add(ActiveObjectCountWider);

		while (true) {
			var val = rng.Next(1, chance);

			if (val == 1) {
				var error = action.Invoke(Params);
				if (Utils.TestForError(error))
					Logging.Log("Error", "Error executing ABM function callback: " + label);
			}

			yield return delay;
		}
	}

	#region CTOR

	private static VF _instance;

	public static VF Instance {
		get {
			if (_instance == null) _instance = new VF();

			return _instance;
		}
	}

	private VF() {
		// Private constructor to prevent external instantiation
	}

	#endregion
}

internal struct lbm {
	public string         label;
	public string[]       nodenames;
	public string         name;
	public bool           RunAtEveryLoad;
	public LuaFunctionRef action = null;

	public lbm() {
		label = "";
		nodenames = new[] {""};
		name = "";
		RunAtEveryLoad = false;
		action = null;
	}
}

public enum FindResultType {
	Item = 0,
	Node = 1,
	Tool = 2
}

public struct FindResult {
	public bool           success;
	public FindResultType type;
	public Variant        definition;
	public string         identifier;
}
