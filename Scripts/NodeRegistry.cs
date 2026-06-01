using Godot;
using System.Collections.Generic;
using ApophisSoftware.LuaObjects; // Assuming NodeBlock is in this namespace

public class NodeRegistry : Node
{
    public class NodeDefinition
    {
        public int Id;
        public string Name;
        public string[] TexturePaths; // Paths to the texture images for each face
        public bool IsTransparent;
        public NodeBlock OriginalNodeBlock; // Reference to the original NodeBlock for full access
    }

    private Dictionary<int, NodeDefinition> _nodesById = new Dictionary<int, NodeDefinition>();
    private Dictionary<string, NodeDefinition> _nodesByName = new Dictionary<string, NodeDefinition>();
    private int _nextId = 1; // Start ID from 1, 0 can be reserved for 'air' or 'empty'

    public override void _Ready()
    {
        // This NodeRegistry will be populated by MCLPP's registered nodes.
        // It's crucial that MCLPP.Instance is ready and has registered its nodes
        // before this _Ready method attempts to access them.
        // If MCLPP is an Autoload, its _Ready will typically run before other nodes.
        PopulateFromMCLPP();
        GD.Print("NodeRegistry initialized and populated from MCLPP.");
    }

    /// <summary>
    /// Populates this NodeRegistry with definitions from MCLPP.Instance.registered_nodes.
    /// This should be called after MCLPP has finished registering all its Lua-defined nodes.
    /// </summary>
    public void PopulateFromMCLPP()
    {
        if (MCLPP.Instance == null)
        {
            GD.PrintErr("NodeRegistry: MCLPP.Instance is null. Cannot populate nodes.");
            return;
        }

        if (MCLPP.Instance.registered_nodes == null || MCLPP.Instance.registered_nodes.Count == 0)
        {
            GD.Print("NodeRegistry: MCLPP.Instance.registered_nodes is empty. No nodes to populate.");
            // We should at least register 'air' if it's not coming from Lua
            RegisterInternalNode("Air", System.Array.Empty<string>(), true);
            return;
        }

        // Clear existing definitions to avoid duplicates if called multiple times
        _nodesById.Clear();
        _nodesByName.Clear();
        _nextId = 1; // Reset ID counter

        // Register 'Air' as ID 0 if it's not already registered by Lua
        // Assuming 'Air' is a fundamental block that might not always be explicitly registered in Lua
        if (!MCLPP.Instance.registered_nodes.ContainsKey("air")) // Minetest uses lowercase for node names
        {
            RegisterInternalNode("Air", System.Array.Empty<string>(), true);
        }

        foreach (var entry in MCLPP.Instance.registered_nodes)
        {
            string nodeName = entry.Key;
            NodeBlock nodeBlock = entry.Value;

            // Ensure 'Air' is registered with ID 0 if it came from Lua
            if (nodeName.ToLower() == "air" && _nodesById.ContainsKey(0))
            {
                // If 'Air' was already registered internally with ID 0, update its properties
                // or skip if it's already correctly set.
                // For now, we'll assume the internal registration is sufficient if Lua also defines it.
                // If Lua defines 'air', we should use its properties.
                _nodesById.Remove(0); // Remove the internally registered 'Air'
                _nodesByName.Remove("Air");
                _nextId = 1; // Reset _nextId to 1 after registering 'Air' as 0
            }
            
            // Determine transparency based on NodeBlock's properties
            bool isTransparent = nodeBlock.use_texture_alpha != "opaque";

            // Use the internal registration method to assign an ID and store the definition
            RegisterInternalNode(nodeName, nodeBlock.Tiles, isTransparent, nodeBlock);
        }
    }

    /// <summary>
    /// Internal method to register a node definition.
    /// </summary>
    private int RegisterInternalNode(string name, string[] texturePaths, bool isTransparent, NodeBlock originalNodeBlock = null)
    {
        if (_nodesByName.ContainsKey(name))
        {
            GD.PrintErr($"Node with name '{name}' already registered in NodeRegistry.");
            return _nodesByName[name].Id;
        }

        NodeDefinition def = new NodeDefinition
        {
            Id = _nextId++,
            Name = name,
            TexturePaths = texturePaths,
            IsTransparent = isTransparent,
            OriginalNodeBlock = originalNodeBlock
        };

        // Special handling for 'Air' to ensure it gets ID 0 if not already taken
        if (name.ToLower() == "air" && !_nodesById.ContainsKey(0))
        {
            def.Id = 0;
            _nextId = 1; // Ensure next ID starts from 1 after 0 is used
        }
        else if (name.ToLower() == "air" && _nodesById.ContainsKey(0) && _nodesById[0].Name.ToLower() != "air")
        {
            // If ID 0 is taken by something else, and we're trying to register 'air',
            // assign it the next available ID. This scenario should ideally not happen
            // if 'Air' is consistently registered first or with ID 0.
            GD.PrintErr($"NodeRegistry: 'Air' node could not be assigned ID 0 as it's already taken by '{_nodesById[0].Name}'. Assigning next available ID.");
        }


        _nodesById[def.Id] = def;
        _nodesByName[name] = def;
        GD.Print($"Registered node in C# NodeRegistry: {name} (ID: {def.Id}) with {texturePaths.Length} texture(s). Transparent: {isTransparent}");
        return def.Id;
    }

    public NodeDefinition GetNodeDefinition(int id)
    {
        _nodesById.TryGetValue(id, out NodeDefinition def);
        return def;
    }

    public NodeDefinition GetNodeDefinition(string name)
    {
        _nodesByName.TryGetValue(name, out NodeDefinition def);
        return def;
    }

    public IEnumerable<NodeDefinition> GetAllNodeDefinitions()
    {
        return _nodesById.Values;
    }
}
