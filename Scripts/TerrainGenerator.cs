using Godot;
using System; // Required for Random
using System.Collections.Generic;
using ApophisSoftware.LuaObjects; // Assuming NodeBlock is in this namespace

public partial class TerrainGenerator : Node3D
{
    // --- Configuration ---
    [Export] public int ChunkSize = 16; // Size of a chunk in blocks (e.g., 16x16x16)
    [Export] public int RenderDistance = 4; // How many chunks in each direction to render

    // --- Dependencies ---
    // This would be a reference to your NodeRegistry or similar system
    // that provides information about registered block types (e.g., their textures).
    public NodeRegistry NodeRegistry { get; private set; } // Changed to public property

    // --- Internal State ---
    private Dictionary<Vector3I, Chunk> _loadedChunks = new Dictionary<Vector3I, Chunk>();
    private Material _terrainMaterial; // The material using the texture atlas
    private Dictionary<string, Rect2> _textureUVRects = new Dictionary<string, Rect2>(); // Maps texture path to its UV rect in the atlas

    // --- Methods ---

    public override void _Ready()
    {
        // Initialize NodeRegistry (you'll need to implement this part)
        // Ensure NodeRegistry is set up as an Autoload or accessible in your scene tree
        NodeRegistry = GetNode<NodeRegistry>("/root/NodeRegistry"); // Example path, adjust as needed
        if (NodeRegistry == null)
        {
            GD.PrintErr("TerrainGenerator: NodeRegistry not found! Please ensure it's an Autoload or correctly instanced.");
            return;
        }

        // Populate the NodeRegistry with definitions from MCLPP
        NodeRegistry.PopulateFromMCLPP();

        // 1. Prepare the texture atlas and material
        GenerateTextureAtlasAndMaterial();
        
        // 2. Start generating initial terrain around the player/origin
        GenerateInitialTerrain();
    }

    /// <summary>
    /// Generates a texture atlas from all registered node textures and creates a material.
    /// </summary>
    private void GenerateTextureAtlasAndMaterial()
    {
        GD.Print("Generating texture atlas and material...");

        List<string> uniqueTexturePaths = new List<string>();
        foreach (var nodeDef in NodeRegistry.GetAllNodeDefinitions())
        {
            foreach (string path in nodeDef.TexturePaths)
            {
                if (!string.IsNullOrEmpty(path) && !uniqueTexturePaths.Contains(path))
                {
                    uniqueTexturePaths.Add(path);
                }
            }
        }

        if (uniqueTexturePaths.Count == 0)
        {
            GD.PrintErr("No unique textures found to build atlas.");
            return;
        }

        // For simplicity, let's assume a simple horizontal atlas for now.
        // A more robust solution would pack textures efficiently (e.g., using a bin packing algorithm).
        int atlasWidth = 0;
        int maxHeight = 0;
        List<Image> images = new List<Image>();

        foreach (string path in uniqueTexturePaths)
        {
            Image img = ImageLoader.LoadImage(path); // Custom helper to load image
            if (img != null)
            {
                images.Add(img);
                atlasWidth += img.GetWidth();
                if (img.GetHeight() > maxHeight)
                {
                    maxHeight = img.GetHeight();
                }
            }
            else
            {
                GD.PrintErr($"Failed to load texture: {path}");
            }
        }

        if (images.Count == 0)
        {
            GD.PrintErr("No images successfully loaded for atlas.");
            return;
        }

        Image atlasImage = Image.Create(atlasWidth, maxHeight, false, Image.Format.Rgba8);
        int currentX = 0;
        for (int i = 0; i < images.Count; i++)
        {
            Image img = images[i];
            atlasImage.BlitRect(img, new Rect2I(0, 0, img.GetWidth(), img.GetHeight()), new Vector2I(currentX, 0));
            
            // Store UV rect for this texture
            _textureUVRects[uniqueTexturePaths[i]] = new Rect2(
                (float)currentX / atlasWidth,
                0,
                (float)img.GetWidth() / atlasWidth,
                (float)img.GetHeight() / maxHeight
            );
            currentX += img.GetWidth();
        }

        ImageTexture atlasTexture = ImageTexture.CreateFromImage(atlasImage);

        StandardMaterial3D material = new StandardMaterial3D();
        material.AlbedoTexture = atlasTexture;
        material.TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest; // Pixel art style
        _terrainMaterial = material;

        GD.Print($"Texture atlas generated. Size: {atlasWidth}x{maxHeight}.");
    }

    /// <summary>
    /// Generates the initial set of terrain chunks.
    /// </summary>
    private void GenerateInitialTerrain()
    {
        GD.Print("Generating initial terrain...");
        for (int x = -RenderDistance; x <= RenderDistance; x++)
        {
            for (int z = -RenderDistance; z <= RenderDistance; z++)
            {
                // Assuming terrain is mostly on the Y=0 plane for initial generation
                LoadChunk(new Vector3I(x, 0, z)); // Now uses LoadChunk
            }
        }
    }

    /// <summary>
    /// Helper to get block ID at a specific world coordinate.
    /// </summary>
    /// <param name="worldBlockPos">The world coordinates of the block.</param>
    /// <returns>The block ID, or 'Air' ID if out of bounds or chunk not loaded.</returns>
    public int GetBlockId(Vector3I worldBlockPos) // Changed to public
    {
        // Convert world position to chunk coordinates and local block coordinates
        Vector3I chunkCoords = new Vector3I(
            Mathf.FloorToInt((float)worldBlockPos.X / ChunkSize),
            Mathf.FloorToInt((float)worldBlockPos.Y / ChunkSize),
            Mathf.FloorToInt((float)worldBlockPos.Z / ChunkSize)
        );
        
        Vector3I localPos = new Vector3I(
            worldBlockPos.X % ChunkSize,
            worldBlockPos.Y % ChunkSize,
            worldBlockPos.Z % ChunkSize
        );
        // Handle negative modulo results
        if (localPos.X < 0) localPos.X += ChunkSize;
        if (localPos.Y < 0) localPos.Y += ChunkSize;
        if (localPos.Z < 0) localPos.Z += ChunkSize;


        if (_loadedChunks.TryGetValue(chunkCoords, out Chunk chunk))
        {
            // Check if local position is within bounds of this chunk
            if (localPos.X >= 0 && localPos.X < ChunkSize &&
                localPos.Y >= 0 && localPos.Y < ChunkSize &&
                localPos.Z >= 0 && localPos.Z < ChunkSize)
            {
                return chunk.BlockData[localPos.X, localPos.Y, localPos.Z];
            }
        }
        // If chunk is not loaded or local position is out of bounds for the found chunk,
        // assume it's air.
        return NodeRegistry.GetNodeDefinition("Air")?.Id ?? 0;
    }

    /// <summary>
    /// Generates or regenerates the mesh for a given chunk.
    /// This method only handles mesh creation/update, not block data generation or chunk management.
    /// </summary>
    /// <param name="chunk">The Chunk object containing the block data.</param>
    /// <param name="meshInstance">The MeshInstance3D to update, or null to create a new one.</param>
    private void _GenerateChunkMesh(Chunk chunk, MeshInstance3D meshInstance)
    {
        if (meshInstance == null)
        {
            meshInstance = new MeshInstance3D();
            AddChild(meshInstance); // Add to scene tree immediately if new
        }

        meshInstance.Name = $"Chunk_{chunk.ChunkCoords.X}_{chunk.ChunkCoords.Y}_{chunk.ChunkCoords.Z}";
        meshInstance.Position = new Vector3(chunk.ChunkCoords.X * ChunkSize, chunk.ChunkCoords.Y * ChunkSize, chunk.ChunkCoords.Z * ChunkSize);
        
        ArrayMesh arrayMesh = new ArrayMesh();
        
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();
        int vertexIndex = 0;

        int airId = NodeRegistry.GetNodeDefinition("Air")?.Id ?? 0;

        // Iterate through each block in the chunk
        for (int x = 0; x < ChunkSize; x++)
        {
            for (int y = 0; y < ChunkSize; y++)
            {
                for (int z = 0; z < ChunkSize; z++)
                {
                    int blockId = chunk.BlockData[x, y, z];
                    if (blockId == airId) continue; // Don't render air blocks

                    NodeRegistry.NodeDefinition blockDef = NodeRegistry.GetNodeDefinition(blockId);
                    if (blockDef == null) continue; // Should not happen if IDs are managed correctly

                    // Calculate world position of the current block's origin
                    Vector3I currentBlockWorldPos = new Vector3I(
                        chunk.ChunkCoords.X * ChunkSize + x,
                        chunk.ChunkCoords.Y * ChunkSize + y,
                        chunk.ChunkCoords.Z * ChunkSize + z
                    );

                    // Define face normals and neighbor offsets
                    // Order: +Y (Top), -Y (Bottom), +X (Right), -X (Left), +Z (Front), -Z (Back)
                    Vector3[] faceNormals = {
                        Vector3.Up, Vector3.Down, Vector3.Right, Vector3.Left, Vector3.Forward, Vector3.Back
                    };

                    Vector3I[] neighborOffsets = {
                        new Vector3I(0, 1, 0),  // Top
                        new Vector3I(0, -1, 0), // Bottom
                        new Vector3I(1, 0, 0),  // Right
                        new Vector3I(-1, 0, 0), // Left
                        new Vector3I(0, 0, 1),  // Front
                        new Vector3I(0, 0, -1)  // Back
                    };

                    // Base UVs for a square face (bottom-left, bottom-right, top-right, top-left)
                    // This order is common for quad rendering and matches the vertex order below.
                    Vector2[] baseUVs = {
                        new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0)
                    };

                    // Vertices for each face, relative to block's local origin (x,y,z)
                    // These are ordered to match the baseUVs and form two triangles (0,1,2) and (0,2,3)
                    Vector3[][] faceVertices = {
                        // Top (+Y)
                        new Vector3[] { new Vector3(x, y + 1, z), new Vector3(x + 1, y + 1, z), new Vector3(x + 1, y + 1, z + 1), new Vector3(x, y + 1, z + 1) },
                        // Bottom (-Y)
                        new Vector3[] { new Vector3(x, y, z + 1), new Vector3(x + 1, y, z + 1), new Vector3(x + 1, y, z), new Vector3(x, y, z) },
                        // Right (+X)
                        new Vector3[] { new Vector3(x + 1, y, z + 1), new Vector3(x + 1, y, z), new Vector3(x + 1, y + 1, z), new Vector3(x + 1, y + 1, z + 1) },
                        // Left (-X)
                        new Vector3[] { new Vector3(x, y, z), new Vector3(x, y, z + 1), new Vector3(x, y + 1, z + 1), new Vector3(x, y + 1, z) },
                        // Front (+Z)
                        new Vector3[] { new Vector3(x, y, z + 1), new Vector3(x + 1, y, z + 1), new Vector3(x + 1, y + 1, z + 1), new Vector3(x, y + 1, z + 1) },
                        // Back (-Z)
                        new Vector3[] { new Vector3(x + 1, y, z), new Vector3(x, y, z), new Vector3(x, y + 1, z), new Vector3(x + 1, y + 1, z) }
                    };

                    for (int faceIdx = 0; faceIdx < 6; faceIdx++)
                    {
                        Vector3I neighborWorldPos = currentBlockWorldPos + neighborOffsets[faceIdx];
                        int neighborBlockId = GetBlockId(neighborWorldPos);
                        NodeRegistry.NodeDefinition neighborDef = NodeRegistry.GetNodeDefinition(neighborBlockId);

                        // Render face if neighbor is air or transparent
                        if (neighborDef == null || neighborDef.IsTransparent)
                        {
                            // Get texture path for this face
                            string texturePath = "";
                            if (blockDef.TexturePaths.Length > faceIdx)
                            {
                                texturePath = blockDef.TexturePaths[faceIdx];
                            }
                            else if (blockDef.TexturePaths.Length > 0)
                            {
                                texturePath = blockDef.TexturePaths[0]; // Fallback to first texture if not enough are provided
                            }

                            if (string.IsNullOrEmpty(texturePath) || !_textureUVRects.ContainsKey(texturePath))
                            {
                                // Fallback if texture path is invalid or not in atlas
                                // Maybe use a default missing texture or skip face
                                GD.PrintErr($"Missing texture or UV rect for path: {texturePath} for block {blockDef.Name}. Skipping face.");
                                continue; 
                            }

                            Rect2 uvRect = _textureUVRects[texturePath];

                            // Add vertices, normals, and UVs for this face
                            for (int i = 0; i < 4; i++)
                            {
                                vertices.Add(faceVertices[faceIdx][i]);
                                normals.Add(faceNormals[faceIdx]);
                                
                                // Adjust UVs based on the texture atlas rect
                                uvs.Add(new Vector2(
                                    uvRect.Position.X + baseUVs[i].X * uvRect.Size.X,
                                    uvRect.Position.Y + baseUVs[i].Y * uvRect.Size.Y
                                ));
                            }

                            // Add indices for two triangles (quad)
                            indices.Add(vertexIndex + 0);
                            indices.Add(vertexIndex + 1);
                            indices.Add(vertexIndex + 2);

                            indices.Add(vertexIndex + 0);
                            indices.Add(vertexIndex + 2);
                            indices.Add(vertexIndex + 3);

                            vertexIndex += 4;
                        }
                    }
                }
            }
        }

        if (vertices.Count > 0)
        {
            Godot.Collections.Array arrays = new Godot.Collections.Array();
            arrays.Resize((int)Mesh.ArrayType.Max);
            arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
            arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
            arrays[(int)Mesh.ArrayType.TexUV] = uvs.ToArray(); 
            arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();
            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        }
        else
        {
            GD.Print($"Chunk {chunk.ChunkCoords} generated no mesh (all blocks air or hidden).");
        }

        meshInstance.Mesh = arrayMesh;
        meshInstance.MaterialOverride = _terrainMaterial; // Apply the shared material
    }

    // --- Chunk Loading/Unloading/Updating ---

    /// <summary>
    /// Loads a chunk at the specified coordinates. If the chunk doesn't exist, it generates its block data.
    /// </summary>
    /// <param name="chunkCoords">The integer coordinates of the chunk to load.</param>
    public void LoadChunk(Vector3I chunkCoords)
    {
        if (_loadedChunks.ContainsKey(chunkCoords))
        {
            GD.Print($"Chunk {chunkCoords} already loaded.");
            return;
        }

        GD.Print($"Loading chunk at {chunkCoords}...");

        // 1. Create a 3D map of blocks for this chunk.
        Chunk newChunk = new Chunk(ChunkSize, chunkCoords);
        newChunk.GenerateBlockData(NodeRegistry); // Fill with block types

        // Store the chunk data immediately so GetBlockId can access it for neighbor checks
        _loadedChunks[chunkCoords] = newChunk; 

        // 2. Create the Godot MeshInstance3D and generate its mesh.
        MeshInstance3D meshInstance = new MeshInstance3D();
        _GenerateChunkMesh(newChunk, meshInstance); // Use the refactored mesh generation
    }

    /// <summary>
    /// Unloads a chunk at the specified coordinates, removing it from the scene and memory.
    /// </summary>
    /// <param name="chunkCoords">The integer coordinates of the chunk to unload.</param>
    public void UnloadChunk(Vector3I chunkCoords)
    {
        if (!_loadedChunks.TryGetValue(chunkCoords, out Chunk chunkToUnload))
        {
            GD.Print($"Attempted to unload chunk {chunkCoords} which is not loaded.");
            return;
        }

        GD.Print($"Unloading chunk at {chunkCoords}...");

        // Find the MeshInstance3D associated with this chunk
        string meshInstanceName = $"Chunk_{chunkCoords.X}_{chunkCoords.Y}_{chunkCoords.Z}";
        Node meshNode = GetNodeOrNull(meshInstanceName);

        if (meshNode is MeshInstance3D meshInstance)
        {
            meshInstance.QueueFree(); // Free the node and its children
        }
        else
        {
            GD.PrintErr($"Could not find MeshInstance3D for chunk {chunkCoords} to unload.");
        }

        _loadedChunks.Remove(chunkCoords);
    }

    /// <summary>
    /// Rebuilds the mesh for a specific chunk.
    /// </summary>
    /// <param name="chunkCoords">The coordinates of the chunk to rebuild.</param>
    private void _RebuildChunkMesh(Vector3I chunkCoords)
    {
        if (_loadedChunks.TryGetValue(chunkCoords, out Chunk chunk))
        {
            string meshInstanceName = $"Chunk_{chunkCoords.X}_{chunkCoords.Y}_{chunkCoords.Z}";
            if (GetNodeOrNull(meshInstanceName) is MeshInstance3D meshInstance)
            {
                _GenerateChunkMesh(chunk, meshInstance);
                GD.Print($"Rebuilt mesh for chunk {chunkCoords}.");
            }
            else
            {
                GD.PrintErr($"Could not find MeshInstance3D for chunk {chunkCoords} to rebuild mesh.");
            }
        }
        else
        {
            GD.Print($"Attempted to rebuild mesh for chunk {chunkCoords} which is not loaded.");
        }
    }

    /// <summary>
    /// Updates a block at a specific world position and regenerates affected chunk meshes.
    /// </summary>
    /// <param name="worldBlockPos">The world coordinates of the block to update.</param>
    /// <param name="newBlockId">The new ID of the block.</param>
    public void UpdateBlock(Vector3I worldBlockPos, int newBlockId)
    {
        // 1. Determine which chunk the worldBlockPos belongs to
        Vector3I affectedChunkCoords = new Vector3I(
            Mathf.FloorToInt((float)worldBlockPos.X / ChunkSize),
            Mathf.FloorToInt((float)worldBlockPos.Y / ChunkSize),
            Mathf.FloorToInt((float)worldBlockPos.Z / ChunkSize)
        );

        // 2. Get the chunk and update its block data
        if (_loadedChunks.TryGetValue(affectedChunkCoords, out Chunk affectedChunk))
        {
            Vector3I localPos = new Vector3I(
                worldBlockPos.X % ChunkSize,
                worldBlockPos.Y % ChunkSize,
                worldBlockPos.Z % ChunkSize
            );
            // Handle negative modulo results
            if (localPos.X < 0) localPos.X += ChunkSize;
            if (localPos.Y < 0) localPos.Y += ChunkSize;
            if (localPos.Z < 0) localPos.Z += ChunkSize;

            // Ensure localPos is within bounds before updating
            if (localPos.X >= 0 && localPos.X < ChunkSize &&
                localPos.Y >= 0 && localPos.Y < ChunkSize &&
                localPos.Z >= 0 && localPos.Z < ChunkSize)
            {
                affectedChunk.BlockData[localPos.X, localPos.Y, localPos.Z] = newBlockId;
                GD.Print($"Updated block at {worldBlockPos} to ID {newBlockId}.");

                // 3. Rebuild the mesh for the affected chunk
                _RebuildChunkMesh(affectedChunkCoords);

                // 4. Check and rebuild meshes for neighboring chunks if the updated block is on a chunk boundary
                // Only need to check neighbors if the updated block is on the edge of the chunk
                bool onXBoundary = localPos.X == 0 || localPos.X == ChunkSize - 1;
                bool onYBoundary = localPos.Y == 0 || localPos.Y == ChunkSize - 1;
                bool onZBoundary = localPos.Z == 0 || localPos.Z == ChunkSize - 1;

                if (onXBoundary || onYBoundary || onZBoundary)
                {
                    Vector3I[] neighborOffsets = {
                        new Vector3I(0, 1, 0),  // Top
                        new Vector3I(0, -1, 0), // Bottom
                        new Vector3I(1, 0, 0),  // Right
                        new Vector3I(-1, 0, 0), // Left
                        new Vector3I(0, 0, 1),  // Front
                        new Vector3I(0, 0, -1)  // Back
                    };

                    HashSet<Vector3I> chunksToRebuild = new HashSet<Vector3I>();
                    // No need to add affectedChunkCoords here, it's already rebuilt above.

                    foreach (Vector3I offset in neighborOffsets)
                    {
                        Vector3I neighborWorldBlockPos = worldBlockPos + offset;
                        Vector3I neighborChunkCoords = new Vector3I(
                            Mathf.FloorToInt((float)neighborWorldBlockPos.X / ChunkSize),
                            Mathf.FloorToInt((float)neighborWorldBlockPos.Y / ChunkSize),
                            Mathf.FloorToInt((float)neighborWorldBlockPos.Z / ChunkSize)
                        );

                        if (neighborChunkCoords != affectedChunkCoords && _loadedChunks.ContainsKey(neighborChunkCoords))
                        {
                            chunksToRebuild.Add(neighborChunkCoords);
                        }
                    }

                    foreach (Vector3I coords in chunksToRebuild)
                    {
                        _RebuildChunkMesh(coords);
                    }
                }
            }
            else
            {
                GD.PrintErr($"Local position {localPos} for world block {worldBlockPos} is out of bounds for chunk {affectedChunkCoords}.");
            }
        }
        else
        {
            GD.PrintErr($"Attempted to update block at {worldBlockPos}, but chunk {affectedChunkCoords} is not loaded.");
        }
    }


    // --- Perlin Noise Implementation ---
    // This is a simple 2D Perlin Noise implementation. For a more robust solution,
    // consider using a dedicated noise library or a more advanced algorithm.
    private static Random _perlinRandom = new Random();
    private static int[] _p = new int[512]; // Permutation table

    static TerrainGenerator() // Static constructor to initialize permutation table
    {
        int[] permutation = new int[256];
        for (int i = 0; i < 256; i++)
        {
            permutation[i] = i;
        }
        // Shuffle the permutation table
        for (int i = 0; i < 256; i++)
        {
            int swapIndex = _perlinRandom.Next(256);
            int temp = permutation[i];
            permutation[i] = permutation[swapIndex];
            permutation[swapIndex] = temp;
        }

        for (int i = 0; i < 256; i++)
        {
            _p[i] = permutation[i];
            _p[i + 256] = permutation[i];
        }
    }

    private static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
    private static float Lerp(float t, float a, float b) => a + t * (b - a);
    private static float Grad(int hash, float x, float y)
    {
        int h = hash & 15;
        float u = h < 8 ? x : y;
        float v = h < 4 ? y : (h == 12 || h == 14 ? x : 0);
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    public static float PerlinNoise(float x, float y)
    {
        int X = Mathf.FloorToInt(x) & 255;
        int Y = Mathf.FloorToInt(y) & 255;

        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);

        float u = Fade(x);
        float v = Fade(y);

        int A = _p[X] + Y;
        int AA = _p[A];
        int AB = _p[A + 1];
        int B = _p[X + 1] + Y;
        int BA = _p[B];
        int BB = _p[B + 1];

        float res = Lerp(v, Lerp(u, Grad(_p[AA], x, y),
                                 Grad(_p[BA], x - 1, y)),
                         Lerp(u, Grad(_p[AB], x, y - 1),
                                 Grad(_p[BB], x - 1, y - 1)));
        
        // Normalize to 0-1 range (Perlin noise typically returns -1 to 1)
        return (res + 1.0f) / 2.0f;
    }
}

// You'll need a simple class to hold chunk data
public class Chunk
{
    public int[,,] BlockData; // Stores the Node ID for each block
    public Vector3I ChunkCoords;
    public int Size;

    public Chunk(int size, Vector3I coords)
    {
        Size = size;
        ChunkCoords = coords;

        BlockData = new int[size, size, size];
    }

    public void GenerateBlockData(NodeRegistry registry)
    {
        // TODO: Implement noise-based generation here.
        // For example, fill with a "dirt" node ID up to a certain height,
        // and "air" above it, using Perlin noise for height variation.
        // Use registry to get node IDs.

        // For now, let's fill with a simple pattern: dirt below a certain height, air above.
        int dirtId = registry.GetNodeDefinition("Dirt")?.Id ?? 0;
        int grassId = registry.GetNodeDefinition("Grass")?.Id ?? 0; // Get grass ID
        int stoneId = registry.GetNodeDefinition("Stone")?.Id ?? 0; // Get stone ID
        int airId = registry.GetNodeDefinition("Air")?.Id ?? 0;

        // Simple flat terrain for now
        int baseGroundLevel = Size / 2; 

        for (int x = 0; x < Size; x++)
        {
            for (int z = 0; z < Size; z++)
            {
                // Use the custom PerlinNoise function
                float noiseScale = 0.1f;
                float noiseValue = TerrainGenerator.PerlinNoise( // Using the new PerlinNoise function
                    (float)(ChunkCoords.X * Size + x) * noiseScale, 
                    (float)(ChunkCoords.Z * Size + z) * noiseScale
                );
                int heightOffset = Mathf.RoundToInt(noiseValue * 5); // Vary height by up to 5 blocks

                int currentGroundHeight = baseGroundLevel + heightOffset;

                for (int y = 0; y < Size; y++)
                {
                    if (y < currentGroundHeight - 3) // Stone layer
                    {
                        BlockData[x, y, z] = stoneId;
                    }
                    else if (y < currentGroundHeight - 1) // Dirt layer
                    {
                        BlockData[x, y, z] = dirtId;
                    }
                    else if (y == currentGroundHeight - 1) // Top layer is grass
                    {
                        BlockData[x, y, z] = grassId;
                    }
                    else
                    {
                        BlockData[x, y, z] = airId;
                    }
                }
            }
        }
    }
}

// Helper class to load images, as Godot's ResourceLoader.Load<Image>() might not work directly for all paths
public static class ImageLoader
{
    public static Image LoadImage(string path)
    {
        if (ResourceLoader.Exists(path))
        {
            Texture2D texture = ResourceLoader.Load<Texture2D>(path);
            if (texture != null)
            {
                return texture.GetImage();
            }
        }
        return null;
    }
}
