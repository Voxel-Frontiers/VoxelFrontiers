#region

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

public partial class ItemStack : Item {
	// 
	// Full item identifier ("item name")
	// Optional amount
	// Optional wear value
	// Optional item metadata

	// local stack = ItemStack("default:pick_wood")
	// stack:set_wear(21323)
	// stack:get_meta():set_string("description", "My worn out pick")
	// local itemstring = stack:to_string()

	/// <summary>
	/// Sets the Name of the Item.
	/// Note: Be Careful of setting the identifier often; Hidden Cost. Will search for item definition...
	/// </summary>
	public string identifier {
		get { return name; }
		set {
			// try to find the named definition.
			FindResult x = VF.Instance.FindItem(value);
			if (x.success) {
				definition = x.definition;
				itemType = x.type;
				// TODO: add missing code for what can be done here.
			}

			name = value;
		}
	}

	public  int            amount = 1;
	public  int            wear   = 0;
	public  Variant        definition; //TODO: Link Definition to base item.
	private FindResultType itemType = FindResultType.Item;

	public override string ToString() {
		StringBuilder sb = new StringBuilder(identifier);
		if (amount != 1) sb.Append(" " + amount.ToString());

		if (wear != 0) {
			if (amount == 1) sb.Append(" " + amount.ToString());
			sb.Append(" " + wear.ToString());
		}

		return sb.ToString();
	}

	public string to_string() {
		return identifier;
	}

	// Used to take 1 of the Item.
	public ItemStack take_item() {
		amount -= 1;
		return this;
	}

	public void setwear(int value) {
		wear = value;
	}

	public string get_description() {
		return get_description();
	}

	#region CTOR

	public ItemStack() {
	}

	public ItemStack CreateItemStack(LuaTuple args) {
		ItemStack _itemStack = new ItemStack();

		if (args == null || args.IsEmpty()) {
			return _itemStack;
		}

		if (args.Size() >= 1) {
			_itemStack.identifier = _itemStack.name; // Note: Hidden Cost. Will search for item definition... 
		}

		if (args.Size() == 2) {
			_itemStack.amount = (int) args.ToArray()[1];
		}

		return _itemStack;
	}

	#endregion
}