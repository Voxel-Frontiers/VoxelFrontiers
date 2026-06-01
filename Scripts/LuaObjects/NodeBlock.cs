#region Usings

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
 * GNU General Public License as more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

#endregion


namespace ApophisSoftware.LuaObjects;

public partial class NodeBlock : Item{
	public BlockBox node_box = new();
	public BlockBox collision_box = new();
	public BlockBox selection_box = new();

	public string use_texture_alpha = "opaque";
	public string post_effect_color = "#00000000";
	public int place_param2 = 0;
	public string paramtype = "light";
	public string paramtype2 = "none";
	public float visual_scale = 1.0f;
	public bool is_ground_content = true;
	public bool sunlight_propagates = false;
	public bool walkable = true; // -- If true, objects collide with node
	public bool pointable = true; // -- If true, can be pointed at
	public bool diggable = true; //  -- If false, can never be dug
	public bool climbable = false; //  -- If true, can be climbed on like a ladder
	public bool buildable_to = false; // Is this node replaceable, when another node is placed?
	public string drawtype = "normal";
	public string[] Tiles = System.Array.Empty<string>(); // Textures of node; +Y, -Y, +X, -X, +Z, -Z
	public string MeshPath = ""; // Path to a custom 3D model resource

	// Function References.
	public LuaFunctionRef on_construct;
	public LuaFunctionRef on_destruct;
	public LuaFunctionRef after_destruct;
	public LuaFunctionRef on_flood;
	public LuaFunctionRef preserve_metadata;
	public LuaFunctionRef after_place_node;
	public LuaFunctionRef after_dig_node;
	public LuaFunctionRef can_dig;
	public LuaFunctionRef on_punch;
	public LuaFunctionRef on_rightclick;
	public LuaFunctionRef on_dig;
	public LuaFunctionRef on_timer;
	public LuaFunctionRef on_receive_fields;
	public LuaFunctionRef allow_metadata_inventory_move;
	public LuaFunctionRef allow_metadata_inventory_put;
	public LuaFunctionRef allow_metadata_inventory_take;
	public LuaFunctionRef on_metadata_inventory_move;
	public LuaFunctionRef on_metadata_inventory_put;
	public LuaFunctionRef on_metadata_inventory_take;
	public LuaFunctionRef on_blast;

	public string mod_origin = "??"; // "modname"

	#region CTOR

	public NodeBlock() : base(){
	}

	public NodeBlock(string name) : base(name){
		this.name = name;
	}

	public NodeBlock CreateNodeBlock(LuaTuple args){
		NodeBlock nb = new NodeBlock("unknown");

		if (args != null && !args.IsEmpty()){
			nb.name = (string)args.ToArray()[0];
		}

		return nb;
	}

	#endregion

	public override string ToString(){
		StringBuilder sb = new StringBuilder();

		sb.Append("node_box: ");
		sb.AppendLine(node_box.ToString());
		sb.Append("collision_box:");
		sb.AppendLine(collision_box.ToString());
		sb.Append("selection_box:");
		sb.AppendLine(selection_box.ToString());
		sb.Append("use_texture_alpha: ");
		sb.AppendLine(use_texture_alpha);
		sb.Append("post_effect_color: ");
		sb.AppendLine(post_effect_color);
		sb.Append("place_param2: ");
		sb.AppendLine(place_param2.ToString());
		sb.Append("paramtype: ");
		sb.AppendLine(paramtype);
		sb.Append("paramtype2: ");
		sb.AppendLine(paramtype2);
		sb.Append("visual_scale: ");
		sb.AppendLine(visual_scale.ToString());
		sb.Append("is_ground_content: ");
		sb.AppendLine(is_ground_content.ToString());
		sb.Append("sunlight_propagates: ");
		sb.AppendLine(sunlight_propagates.ToString());
		sb.Append("walkable: ");
		sb.AppendLine(walkable.ToString());
		sb.Append("pointable: ");
		sb.AppendLine(pointable.ToString());
		sb.Append("diggable: ");
		sb.AppendLine(diggable.ToString());
		sb.Append("climbable: ");
		sb.AppendLine(climbable.ToString());
		sb.Append("buildable_to: ");
		sb.AppendLine(buildable_to.ToString());
		sb.Append("drawtype: ");
		sb.AppendLine(drawtype.ToString());
		sb.Append("Tiles: ");
		sb.AppendLine(string.Join(", ", Tiles));
		sb.Append("MeshPath: ");
		sb.AppendLine(MeshPath);


		sb.Append("[callback-function]on_construct: ");
		if (on_construct == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_construct.ToString());

		sb.Append("[callback-function]on_destruct: ");
		if (on_destruct == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_destruct.ToString());

		sb.Append("[callback-function]after_destruct: ");
		if (after_destruct == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(after_destruct.ToString());

		sb.Append("[callback-function]on_flood: ");
		if (on_flood == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_flood.ToString());

		sb.Append("[callback-function]preserve_metadata: ");
		if (preserve_metadata == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(preserve_metadata.ToString());

		sb.Append("[callback-function]after_place_node: ");
		if (after_place_node == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(after_place_node.ToString());

		sb.Append("[callback-function]after_dig_node: ");
		if (after_dig_node == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(after_dig_node.ToString());

		sb.Append("[callback-function]can_dig: ");
		if (can_dig == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(can_dig.ToString());

		sb.Append("[callback-function]on_punch: ");
		if (on_punch == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_punch.ToString());

		sb.Append("[callback-function]on_rightclick: ");
		if (on_rightclick == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_rightclick.ToString());

		sb.Append("[callback-function]on_dig: ");
		if (on_dig == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_dig.ToString());

		sb.Append("[callback-function]on_timer: ");
		if (on_timer == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_timer.ToString());

		sb.Append("[callback-function]on_receive_fields: ");
		if (on_receive_fields == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_receive_fields.ToString());

		sb.Append("[callback-function]allow_metadata_inventory_move: ");
		if (allow_metadata_inventory_move == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(allow_metadata_inventory_move.ToString());

		sb.Append("[callback-function]allow_metadata_inventory_put: ");
		if (allow_metadata_inventory_put == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(allow_metadata_inventory_put.ToString());

		sb.Append("[callback-function]allow_metadata_inventory_take: ");
		if (allow_metadata_inventory_take == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(allow_metadata_inventory_take.ToString());

		sb.Append("[callback-function]on_metadata_inventory_move: ");
		if (on_metadata_inventory_move == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_metadata_inventory_move.ToString());

		sb.Append("[callback-function]on_metadata_inventory_put: ");
		if (on_metadata_inventory_put == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_metadata_inventory_put.ToString());

		sb.Append("[callback-function]on_metadata_inventory_take: ");
		if (on_metadata_inventory_take == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_metadata_inventory_take.ToString());

		sb.Append("[callback-function]on_blast: ");
		if (on_blast == null)
			sb.AppendLine("Not Defined.");
		else
			sb.AppendLine(on_blast.ToString());

		sb.Append("mod_origin: ");
		sb.AppendLine(mod_origin.ToString());

		sb.AppendLine(base.ToString());
		return sb.ToString();
	}

	/*
	 From the Luanti documentation...
	 * {
    -- <all fields allowed in item definitions>

    drawtype = "normal",  -- See "Node drawtypes"

    visual_scale = 1.0,
    -- Supported for drawtypes "plantlike", "signlike", "torchlike",
    -- "firelike", "mesh", "nodebox", "allfaces".
    -- For plantlike and firelike, the image will start at the bottom of the
    -- node. For torchlike, the image will start at the surface to which the
    -- node "attaches". For the other drawtypes the image will be centered
    -- on the node.

    tiles = {tile definition 1, def2, def3, def4, def5, def6},
    -- Textures of node; +Y, -Y, +X, -X, +Z, -Z
    -- List can be shortened to needed length.

    overlay_tiles = {tile definition 1, def2, def3, def4, def5, def6},
    -- Same as `tiles`, but these textures are drawn on top of the base
    -- tiles. You can use this to colorize only specific parts of your
    -- texture. If the texture name is an empty string, that overlay is not
    -- drawn. Since such tiles are drawn twice, it is not recommended to use
    -- overlays on very common nodes.

    special_tiles = {tile definition 1, Tile definition 2},
    -- Special textures of node; used rarely.
    -- List can be shortened to needed length.

    color = ColorSpec,
    -- The node's original color will be multiplied with this color.
    -- If the node has a palette, then this setting only has an effect in
    -- the inventory and on the wield item.

    use_texture_alpha = ...,
    -- Specifies how the texture's alpha channel will be used for rendering.
    -- possible values:
    -- * "opaque": Node is rendered opaque regardless of alpha channel
    -- * "clip": A given pixel is either fully see-through or opaque
    --           depending on the alpha channel being below/above 50% in value
    -- * "blend": The alpha channel specifies how transparent a given pixel
    --            of the rendered node is
    -- The default is "opaque" for drawtypes normal, liquid and flowingliquid;
    -- "clip" otherwise.
    -- If set to a boolean value (deprecated): true either sets it to blend
    -- or clip, false sets it to clip or opaque mode depending on the drawtype.

    palette = "",
    -- The node's `param2` is used to select a pixel from the image.
    -- Pixels are arranged from left to right and from top to bottom.
    -- The node's color will be multiplied with the selected pixel's color.
    -- Tiles can override this behavior.
    -- Only when `paramtype2` supports palettes.

    post_effect_color = "#00000000",
    -- Screen tint if player is inside node, see "ColorSpec"

    paramtype = "none",  -- See "Nodes"

    paramtype2 = "none",  -- See "Nodes"

    place_param2 = 0,
    -- Value for param2 that is set when player places node

    is_ground_content = true,
    -- If false, the cave generator and dungeon generator will not carve
    -- through this node.
    -- Specifically, this stops mod-added nodes being removed by caves and
    -- dungeons when those generate in a neighbor mapchunk and extend out
    -- beyond the edge of that mapchunk.

    sunlight_propagates = false,
    -- If true, sunlight will go infinitely through this node

    walkable = true,  -- If true, objects collide with node

    pointable = true,  -- If true, can be pointed at

    diggable = true,  -- If false, can never be dug

    climbable = false,  -- If true, can be climbed on like a ladder

    move_resistance = 0,
    -- Slows down movement of players through this node (max. 7).
    -- If this is nil, it will be equal to liquid_viscosity.
    -- Note: If liquid movement physics apply to the node
    -- (see `liquid_move_physics`), the movement speed will also be
    -- affected by the `movement_liquid_*` settings.

    buildable_to = false,  -- If true, placed nodes can replace this node

    floodable = false,
    -- If true, liquids flow into and replace this node.
    -- Warning: making a liquid node 'floodable' will cause problems.

    liquidtype = "none",  -- specifies liquid flowing physics
    -- * "none":    no liquid flowing physics
    -- * "source":  spawns flowing liquid nodes at all 4 sides and below;
    --              recommended drawtype: "liquid".
    -- * "flowing": spawned from source, spawns more flowing liquid nodes
    --              around it until `liquid_range` is reached;
    --              will drain out without a source;
    --              recommended drawtype: "flowingliquid".
    -- If it's "source" or "flowing", then the
    -- `liquid_alternative_*` fields _must_ be specified

    liquid_alternative_flowing = "",
    liquid_alternative_source = "",
    -- These fields may contain node names that represent the
    -- flowing version (`liquid_alternative_flowing`) and
    -- source version (`liquid_alternative_source`) of a liquid.
    --
    -- Specifically, these fields are required if any of these is true:
    -- * `liquidtype ~= "none" or
    -- * `drawtype == "liquid" or
    -- * `drawtype == "flowingliquid"
    --
    -- Liquids consist of up to two nodes: source and flowing.
    --
    -- There are two ways to define a liquid:
    -- 1) Source node and flowing node. This requires both fields to be
    --    specified for both nodes.
    -- 2) Standalone source node (cannot flow). `liquid_alternative_source`
    --    must be specified and `liquid_range` must be set to 0.
    --
    -- Example:
    --     liquid_alternative_flowing = "example:water_flowing",
    --     liquid_alternative_source = "example:water_source",

    liquid_viscosity = 0,
    -- Controls speed at which the liquid spreads/flows (max. 7).
    -- 0 is fastest, 7 is slowest.
    -- By default, this also slows down movement of players inside the node
    -- (can be overridden using `move_resistance`)

    liquid_renewable = true,
    -- If true, a new liquid source can be created by placing two or more
    -- sources nearby

    liquid_move_physics = nil, -- specifies movement physics if inside node
    -- * false: No liquid movement physics apply.
    -- * true: Enables liquid movement physics. Enables things like
    --   ability to "swim" up/down, sinking slowly if not moving,
    --   smoother speed change when falling into, etc. The `movement_liquid_*`
    --   settings apply.
    -- * nil: Will be treated as true if `liquidtype ~= "none"`
    --   and as false otherwise.

    leveled = 0,
    -- Only valid for "nodebox" drawtype with 'type = "leveled"'.
    -- Allows defining the nodebox height without using param2.
    -- The nodebox height is 'leveled' / 64 nodes.
    -- The maximum value of 'leveled' is `leveled_max`.

    leveled_max = 127,
    -- Maximum value for `leveled` (0-127), enforced in
    -- `minetest.set_node_level` and `minetest.add_node_level`.
    -- Values above 124 might causes collision detection issues.

    liquid_range = 8,
    -- Maximum distance that flowing liquid nodes can spread around
    -- source on flat land;
    -- maximum = 8; set to 0 to disable liquid flow

    drowning = 0,
    -- Player will take this amount of damage if no bubbles are left

    damage_per_second = 0,
    -- If player is inside node, this damage is caused

    node_box = {type = "regular"},  -- See "Node boxes"

    connects_to = {},
    -- Used for nodebox nodes with the type == "connected".
    -- Specifies to what neighboring nodes connections will be drawn.
    -- e.g. `{"group:fence", "default:wood"}` or `"default:stone"`

    connect_sides = {},
    -- Tells connected nodebox nodes to connect only to these sides of this
    -- node. possible: "top", "bottom", "front", "left", "back", "right"

    mesh = "",
    -- File name of mesh when using "mesh" drawtype

    selection_box = {
        -- see [Node boxes] for possibilities
    },
    -- Custom selection box definition. Multiple boxes can be defined.
    -- If "nodebox" drawtype is used and selection_box is nil, then node_box
    -- definition is used for the selection box.

    collision_box = {
        -- see [Node boxes] for possibilities
    },
    -- Custom collision box definition. Multiple boxes can be defined.
    -- If "nodebox" drawtype is used and collision_box is nil, then node_box
    -- definition is used for the collision box.

	[MICHIEAL: Not Supported.]
    -- Support maps made in and before January 2012
    legacy_facedir_simple = false,
    legacy_wallmounted = false,

    waving = 0,
    -- Valid for drawtypes:
    -- mesh, nodebox, plantlike, allfaces_optional, liquid, flowingliquid.
    -- 1 - wave node like plants (node top moves side-to-side, bottom is fixed)
    -- 2 - wave node like leaves (whole node moves side-to-side)
    -- 3 - wave node like liquids (whole node moves up and down)
    -- Not all models will properly wave.
    -- plantlike drawtype can only wave like plants.
    -- allfaces_optional drawtype can only wave like leaves.
    -- liquid, flowingliquid drawtypes can only wave like liquids.

    sounds = {
        -- Definition of node sounds to be played at various events.
        -- All fields in this table are optional.

        footstep = <SimpleSoundSpec>,
        -- If walkable, played when object walks on it. If node is
        -- climbable or a liquid, played when object moves through it

        dig = <SimpleSoundSpec> or "__group",
        -- While digging node.
        -- If `"__group"`, then the sound will be
        -- `{name = "default_dig_<groupname>", gain = 0.5}` , where `<groupname>` is the
        -- name of the item's digging group with the fastest digging time.
        -- In case of a tie, one of the sounds will be played (but we
        -- cannot predict which one)
        -- Default value: `"__group"`

        dug = <SimpleSoundSpec>,
        -- Node was dug

        place = <SimpleSoundSpec>,
        -- Node was placed. Also played after falling

        place_failed = <SimpleSoundSpec>,
        -- When node placement failed.
        -- Note: This happens if the _built-in_ node placement failed.
        -- This sound will still be played if the node is placed in the
        -- `on_place` callback manually.

        fall = <SimpleSoundSpec>,
        -- When node starts to fall or is detached
    },

    drop = "",
    -- Name of dropped item when dug.
    -- Default dropped item is the node itself.

    -- Using a table allows multiple items, drop chances and item filtering:
    drop = {
        max_items = 1,
        -- Maximum number of item lists to drop.
        -- The entries in 'items' are processed in order. For each:
        -- Item filtering is applied, chance of drop is applied, if both are
        -- successful the entire item list is dropped.
        -- Entry processing continues until the number of dropped item lists
        -- equals 'max_items'.
        -- Therefore, entries should progress from low to high drop chance.
        items = {
            -- Examples:
            {
                -- 1 in 1000 chance of dropping a diamond.
                -- Default rarity is '1'.
                rarity = 1000,
                items = {"default:diamond"},
            },
            {
                -- Only drop if using an item whose name is identical to one
                -- of these.
                tools = {"default:shovel_mese", "default:shovel_diamond"},
                rarity = 5,
                items = {"default:dirt"},
                -- Whether all items in the dropped item list inherit the
                -- hardware coloring palette color from the dug node.
                -- Default is 'false'.
                inherit_color = true,
            },
            {
                -- Only drop if using an item whose name contains
                -- "default:shovel_" (this item filtering by string matching
                -- is deprecated, use tool_groups instead).
                tools = {"~default:shovel_"},
                rarity = 2,
                -- The item list dropped.
                items = {"default:sand", "default:desert_sand"},
            },
            {
                -- Only drop if using an item in the "magicwand" group, or
                -- an item that is in both the "pickaxe" and the "lucky"
                -- groups.
                tool_groups = {
                    "magicwand",
                    {"pickaxe", "lucky"}
                },
                items = {"default:coal_lump"},
            },
        },
    },

    on_construct = function(pos),
    -- Node constructor; called after adding node.
    -- Can set up metadata and stuff like that.
    -- Not called for bulk node placement (i.e. schematics and VoxelManip).
    -- Note: Within an on_construct callback, minetest.set_node can cause an
    -- infinite loop if it invokes the same callback.
    --  Consider using minetest.swap_node instead.
    -- default: nil

    on_destruct = function(pos),
    -- Node destructor; called before removing node.
    -- Not called for bulk node placement.
    -- default: nil

    after_destruct = function(pos, oldnode),
    -- Node destructor; called after removing node.
    -- Not called for bulk node placement.
    -- default: nil

    on_flood = function(pos, oldnode, newnode),
    -- Called when a liquid (newnode) is about to flood oldnode, if it has
    -- `floodable = true` in the nodedef. Not called for bulk node placement
    -- (i.e. schematics and VoxelManip) or air nodes. If return true the
    -- node is not flooded, but on_flood callback will most likely be called
    -- over and over again every liquid update interval.
    -- Default: nil
    -- Warning: making a liquid node 'floodable' will cause problems.

    preserve_metadata = function(pos, oldnode, oldmeta, drops),
    -- Called when `oldnode` is about be converted to an item, but before the
    -- node is deleted from the world or the drops are added. This is
    -- generally the result of either the node being dug or an attached node
    -- becoming detached.
    -- * `pos`: node position
    -- * `oldnode`: node table of node before it was deleted
    -- * `oldmeta`: metadata of node before it was deleted, as a metadata table
    -- * `drops`: a table of `ItemStack`s, so any metadata to be preserved can
    --   be added directly to one or more of the dropped items. See
    --   "ItemStackMetaRef".
    -- default: `nil`

    after_place_node = function(pos, placer, itemstack, pointed_thing),
    -- Called after constructing node when node was placed using
    -- minetest.item_place_node / minetest.place_node.
    -- If return true no item is taken from itemstack.
    -- `placer` may be any valid ObjectRef or nil.
    -- default: nil

    after_dig_node = function(pos, oldnode, oldmetadata, digger),
    -- Called after destructing the node when node was dug using
    -- `minetest.node_dig` / `minetest.dig_node`.
    -- * `pos`: node position
    -- * `oldnode`: node table of node before it was dug
    -- * `oldmetadata`: metadata of node before it was dug,
    --                  as a metadata table
    -- * `digger`: ObjectRef of digger
    -- default: nil

    can_dig = function(pos, [player]),
    -- Returns true if node can be dug, or false if not.
    -- default: nil

    on_punch = function(pos, node, puncher, pointed_thing),
    -- default: minetest.node_punch
    -- Called when puncher (an ObjectRef) punches the node at pos.
    -- By default calls minetest.register_on_punchnode callbacks.

    on_rightclick = function(pos, node, clicker, itemstack, pointed_thing),
    -- default: nil
    -- Called when clicker (an ObjectRef) used the 'place/build' key
    -- (not necessarily an actual rightclick)
    -- while pointing at the node at pos with 'node' being the node table.
    -- itemstack will hold clicker's wielded item.
    -- Shall return the leftover itemstack.
    -- Note: pointed_thing can be nil, if a mod calls this function.
    -- This function does not get triggered by clients <=0.4.16 if the
    -- "formspec" node metadata field is set.

    on_dig = function(pos, node, digger),
    -- default: minetest.node_dig
    -- By default checks privileges, wears out item (if tool) and removes node.
    -- return true if the node was dug successfully, false otherwise.
    -- Deprecated: returning nil is the same as returning true.

    on_timer = function(pos, elapsed),
    -- default: nil
    -- called by NodeTimers, see minetest.get_node_timer and NodeTimerRef.
    -- elapsed is the total time passed since the timer was started.
    -- return true to run the timer for another cycle with the same timeout
    -- value.

    on_receive_fields = function(pos, formname, fields, sender),
    -- fields = {name1 = value1, name2 = value2, ...}
    -- Called when an UI form (e.g. sign text input) returns data.
    -- See minetest.register_on_player_receive_fields for more info.
    -- default: nil

    allow_metadata_inventory_move = function(pos, from_list, from_index, to_list, to_index, count, player),
    -- Called when a player wants to move items inside the inventory.
    -- Return value: number of items allowed to move.

    allow_metadata_inventory_put = function(pos, listname, index, stack, player),
    -- Called when a player wants to put something into the inventory.
    -- Return value: number of items allowed to put.
    -- Return value -1: Allow and don't modify item count in inventory.

    allow_metadata_inventory_take = function(pos, listname, index, stack, player),
    -- Called when a player wants to take something out of the inventory.
    -- Return value: number of items allowed to take.
    -- Return value -1: Allow and don't modify item count in inventory.

    on_metadata_inventory_move = function(pos, from_list, from_index, to_list, to_index, count, player),
    on_metadata_inventory_put = function(pos, listname, index, stack, player),
    on_metadata_inventory_take = function(pos, listname, index, stack, player),
    -- Called after the actual action has happened, according to what was
    -- allowed.
    -- No return value.

    on_blast = function(pos, intensity),
    -- intensity: 1.0 = mid range of regular TNT.
    -- If defined, called when an explosion touches the node, instead of
    -- removing the node.

    mod_origin = "modname",
    -- stores which mod actually registered a node
    -- If the source could not be determined it contains "??"
    -- Useful for getting which mod truly registered something
    -- example: if a node is registered as ":othermodname:nodename",
    -- nodename will show "othermodname", but mod_origin will say "modname"
}
	 */
}