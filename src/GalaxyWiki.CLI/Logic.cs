using GalaxyWiki.Core.Entities;
using Spectre.Console;
using System.Text;
using System.Text.RegularExpressions;

public class NavigationState
{
    // Current celestial body
    public CelestialBodies? CurrentBody { get; set; }
    
    // Path stack to track navigation history
    public Stack<CelestialBodies> PathStack { get; set; } = new Stack<CelestialBodies>();
    
    // Cache for children of bodies to avoid repeated API calls
    public Dictionary<int, List<CelestialBodies>> ChildrenCache { get; set; } = new Dictionary<int, List<CelestialBodies>>();
}

// Class to store body type information
public class BodyTypeInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Emoji => TUI.BodyTypeToEmoji(Id);
}

public static class CommandLogic
{
    private static NavigationState _state = new NavigationState();
    private static List<BodyTypeInfo> _bodyTypes = new List<BodyTypeInfo>
    {
        new BodyTypeInfo { Id = 1, Name = "Galaxy", Description = "A vast system of stars, gas, and dust held together by gravity" },
        new BodyTypeInfo { Id = 2, Name = "Star", Description = "A luminous ball of plasma held together by its own gravity" },
        new BodyTypeInfo { Id = 3, Name = "Planet", Description = "A celestial body orbiting a star with sufficient mass for gravity to make it round" },
        new BodyTypeInfo { Id = 4, Name = "Moon", Description = "A natural satellite orbiting a planet or other celestial body" },
        new BodyTypeInfo { Id = 5, Name = "Satellite", Description = "An artificial object placed in orbit around a celestial body" },
        new BodyTypeInfo { Id = 6, Name = "Black Hole", Description = "A region of spacetime where gravity is so strong that nothing can escape from it" },
        new BodyTypeInfo { Id = 7, Name = "Dwarf Planet", Description = "A celestial body orbiting the Sun that is massive enough to be rounded by its own gravity" },
        new BodyTypeInfo { Id = 8, Name = "Asteroid", Description = "A minor rocky body orbiting the Sun, smaller than a planet" },
        new BodyTypeInfo { Id = 9, Name = "Comet", Description = "A small, icy object that, when close to the Sun, displays a visible coma and tail" },
        new BodyTypeInfo { Id = 10, Name = "Nebula", Description = "A cloud of gas and dust in outer space" },
        new BodyTypeInfo { Id = 11, Name = "Universe", Description = "All of space and time and their contents" }
    };

    // Initialize the navigation state with the universe root
    public static async Task Initialize()
    {
        var bodies = await ApiClient.GetCelestialBodiesMap();
        
        // Find the root (Universe) node - it's the one with null Orbits
        var root = bodies.Values.FirstOrDefault(b => b.Orbits == null);
        if (root == null)
        {
            TUI.Err("INIT", "Could not find root celestial body (Universe).");
            return;
        }

        _state.CurrentBody = root;
        _state.PathStack.Clear();
        _state.PathStack.Push(root);
    }

    // Get the current path as a formatted string
    public static string GetCurrentPath()
    {
        if (_state.CurrentBody == null)
            return "GW>";

        var path = new StringBuilder("GW");
        var reversedPath = _state.PathStack.Reverse().ToList();
        
        foreach (var body in reversedPath)
        {
            path.Append($" \\ {body.BodyName}");
        }
        
        // path.Append(">");
        return path.ToString();
    }

    // Change directory
    public static async Task<bool> ChangeDirectory(string target)
    {
        // Sanity check
        if (_state.CurrentBody == null)
        {
            await Initialize();
            if (_state.CurrentBody == null)
            {
                TUI.Err("CD", "Navigation system not initialized.");
                return false;
            }
        }

        // Remove any surrounding quotes from the target
        target = TrimQuotes(target);

        // Handle special navigation commands
        if (string.IsNullOrWhiteSpace(target) || target == ".")
        {
            // Stay in the current directory
            return true;
        }
        else if (target == "/")
        {
            // Go to root
            await Initialize();
            return true;
        }
        else if (target == "..")
        {
            // Go up one level
            if (_state.CurrentBody.Orbits == null)
            {
                TUI.Warn("CD", "Already at root level.", "Cannot go up from Universe.");
                return false;
            }

            _state.PathStack.Pop(); // Remove current from path
            _state.CurrentBody = _state.CurrentBody.Orbits;
            return true;
        }
        
        // Navigate to a specific celestial body
        try
        {
            // Get children of current body
            var children = await GetChildren(_state.CurrentBody.Id);
            
            // Find the child by name (case-insensitive)
            var targetChild = children.FirstOrDefault(c => 
                c.BodyName.Equals(target, StringComparison.OrdinalIgnoreCase));
            
            if (targetChild == null)
            {
                TUI.Err("CD", $"Celestial body '{target}' not found.", 
                    "Use 'ls' to see available celestial bodies.");
                return false;
            }
            
            // Update current body and path
            _state.CurrentBody = targetChild;
            _state.PathStack.Push(targetChild);
            return true;
        }
        catch (Exception ex)
        {
            TUI.Err("CD", "Failed to change directory.", ex.Message);
            return false;
        }
    }

    // Helper method to remove surrounding quotes from a string
    public static string TrimQuotes(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Remove surrounding double or single quotes if present
        if ((input.StartsWith("\"") && input.EndsWith("\"")) || 
            (input.StartsWith("'") && input.EndsWith("'")))
        {
            return input.Substring(1, input.Length - 2);
        }

        return input;
    }

    // List children of current directory
    public static async Task<List<CelestialBodies>> ListDirectory()
    {
        if (_state.CurrentBody == null)
        {
            TUI.Err("LS", "Navigation system not initialized.");
            return new List<CelestialBodies>();
        }

        try
        {
            var children = await GetChildren(_state.CurrentBody.Id);
            return children;
        }
        catch (Exception ex)
        {
            TUI.Err("LS", "Failed to list directory.", ex.Message);
            return new List<CelestialBodies>();
        }
    }

    // Helper to get children with caching
    private static async Task<List<CelestialBodies>> GetChildren(int parentId)
    {
        // Check cache first
        if (_state.ChildrenCache.TryGetValue(parentId, out var cachedChildren))
        {
            return cachedChildren;
        }

        // Call API
        string endpoint = $"/celestial-body/{parentId}/children";
        var childrenData = await ApiClient.GetDeserialized<List<CelestialBodies>>(endpoint);
        
        // Add to cache
        _state.ChildrenCache[parentId] = childrenData;
        
        return childrenData;
    }

    // Get the active revision content for the current body
    public static async Task<Revision?> GetCurrentRevision()
    {
        if (_state.CurrentBody == null || !_state.CurrentBody.ActiveRevision.HasValue)
        {
            return null;
        }

        return await ApiClient.GetRevisionAsync($"/api/revision/{_state.CurrentBody.ActiveRevision}");
    }

    // Display a tree view of celestial bodies
    public static async Task DisplayTree(bool useCurrentAsRoot = false)
    {
        if (_state.CurrentBody == null)
        {
            TUI.Err("TREE", "Navigation system not initialized.");
            return;
        }

        // Determine root node for the tree
        CelestialBodies rootBody;
        
        if (useCurrentAsRoot)
        {
            // Use current location as root
            rootBody = _state.CurrentBody;
            
            if (rootBody == null)
            {
                TUI.Err("TREE", "No current celestial body to use as root.");
                return;
            }
        }
        else
        {
            // Find the universe root
            var bodies = await ApiClient.GetCelestialBodiesMap();
            var possibleRoot = bodies.Values.FirstOrDefault(b => b.Orbits == null);
            
            if (possibleRoot == null)
            {
                TUI.Err("TREE", "Could not find root celestial body (Universe).");
                return;
            }
            
            rootBody = possibleRoot;
        }

        // Create tree and build it
        var tree = new Tree(FormatCelestialBodyLabel(rootBody));
        await BuildTreeRecursively(rootBody.Id, tree);
        
        // Display tree with some styling
        tree.Guide = TreeGuide.BoldLine;
        tree.Style = Style.Parse("blue");
        AnsiConsole.Write(tree);
    }

    // Helper to recursively build the tree
    private static async Task BuildTreeRecursively(int parentId, IHasTreeNodes parentNode)
    {
        try
        {
            // Get children of this node
            var children = await GetChildren(parentId);
            
            // Add each child to the tree
            foreach (var child in children)
            {
                var childNode = parentNode.AddNode(FormatCelestialBodyLabel(child));
                await BuildTreeRecursively(child.Id, childNode);
            }
        }
        catch (Exception ex)
        {
            TUI.Err("TREE", $"Error building tree for node {parentId}", ex.Message);
        }
    }

    // Format celestial body for display in the tree
    private static string FormatCelestialBodyLabel(CelestialBodies body)
    {
        string emoji = TUI.BodyTypeToEmoji(body.BodyType);
        return $"{emoji} {body.BodyName}";
    }

    // Get the current celestial body
    public static CelestialBodies? GetCurrentBody()
    {
        return _state.CurrentBody;
    }

    // Warp to a selected celestial body using an interactive tree
    public static async Task WarpToSelectedBody()
    {
        // Get all celestial bodies
        var bodies = await ApiClient.GetCelestialBodiesMap();
        
        // Find the root (Universe) node
        var root = bodies.Values.FirstOrDefault(b => b.Orbits == null);
        if (root == null)
        {
            TUI.Err("WARP", "Could not find root celestial body (Universe).");
            return;
        }
        
        // Create an empty list of selectable items
        var items = new List<(string DisplayLabel, CelestialBodies Body)>();
        
        // Show the selection prompt with a loading indicator
        await AnsiConsole.Status()
            .StartAsync("Building universe map...", ctx => 
            {
                // Build a list of selectable items with proper indentation
                TUI.RecBuildSelectableTree(bodies, root.Id, items, 0);
                return Task.CompletedTask;
            });
        
        if (items.Count == 0)
        {
            TUI.Err("WARP", "No celestial bodies found in the universe.");
            return;
        }
        
        // Display the selection prompt
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a celestial body to warp to:")
                .PageSize(30)
                .HighlightStyle(new Style(Color.SpringGreen3_1, Color.Black, Decoration.Underline))
                .AddChoices(items.Select(i => i.DisplayLabel))
        );
        
        // Find the selected body
        var selectedItem = items.FirstOrDefault(i => i.DisplayLabel == selection);
        
        if (selectedItem.Body == null)
        {
            TUI.Err("WARP", "Selected celestial body not found.");
            return;
        }
        
        // Navigate to the selected body
        await WarpToBody(selectedItem.Body);
    }

    // Navigate to a specific celestial body by following the path from root
    private static async Task WarpToBody(CelestialBodies targetBody)
    {
        if (targetBody == null)
            return;

        // First go to root
        await ChangeDirectory("/");
        
        // Build the path from the selected body back to root
        var pathParts = new List<string>();
        var current = targetBody;
        
        while (current != null)
        {
            pathParts.Insert(0, current.BodyName);
            current = current.Orbits;
        }
        
        // Navigate to each part of the path
        AnsiConsole.Status()
            .Start("Warping through space...", ctx => 
            {
                // Skip the first part (Universe) as we're already there
                for (int i = 1; i < pathParts.Count; i++)
                {
                    ctx.Status($"Passing through {pathParts[i-1]}...");
                    ctx.Spinner(Spinner.Known.Star);
                    Thread.Sleep(300); // Short delay for visual effect
                    ChangeDirectory(pathParts[i]).Wait();
                }
                
                ctx.Status($"Arrived at {targetBody.BodyName}!");
                Thread.Sleep(500); // Pause briefly to show arrival message
                return;
            });
        
        AnsiConsole.MarkupLine($"[green]Successfully warped to[/] [cyan]{targetBody.BodyName}[/]");
    }
    
    // List all available body types
    public static List<BodyTypeInfo> GetBodyTypes()
    {
        return _bodyTypes;
    }
    
    // Get body type info by ID or name
    public static BodyTypeInfo? GetBodyTypeInfo(string typeIdentifier)
    {
        // Try to parse as number first
        if (int.TryParse(typeIdentifier, out int typeId))
        {
            return _bodyTypes.FirstOrDefault(t => t.Id == typeId);
        }
        
        // Otherwise search by name (case-insensitive)
        return _bodyTypes.FirstOrDefault(t => 
            t.Name.Equals(typeIdentifier, StringComparison.OrdinalIgnoreCase));
    }
    
    // List all celestial bodies of a specific type
    public static async Task<List<CelestialBodies>> ListCelestialBodiesByType(int bodyTypeId)
    {
        try
        {
            // Get all celestial bodies
            var allBodies = await ApiClient.GetCelestialBodies();
            
            // Filter by body type
            return allBodies.Where(b => b.BodyType == bodyTypeId).ToList();
        }
        catch (Exception ex)
        {
            TUI.Err("LIST", "Failed to list celestial bodies by type.", ex.Message);
            return new List<CelestialBodies>();
        }
    }

    // Find a celestial body by name and get its revision
    public static async Task<Revision?> GetRevisionByBodyName(string bodyName)
    {
        // Get all celestial bodies
        var bodies = await ApiClient.GetCelestialBodiesMap();
        
        // Find the body by name (case-insensitive)
        var targetBody = bodies.Values.FirstOrDefault(b => 
            b.BodyName.Equals(bodyName, StringComparison.OrdinalIgnoreCase));
        
        if (targetBody == null || !targetBody.ActiveRevision.HasValue)
        {
            return null;
        }
        
        try
        {
            // Get the active revision
            return await ApiClient.GetRevisionAsync($"/api/revision/{targetBody.ActiveRevision.Value}");
        }
        catch (Exception ex)
        {
            TUI.Err("INFO", $"Failed to get revision for '{bodyName}'", ex.Message);
            return null;
        }
    }
    
    // Get comments for the current celestial body
    public static async Task<List<Comment>> GetCommentsForCurrentBody(int? limit = null, string sortOrder = "newest")
    {
        if (_state.CurrentBody == null)
        {
            TUI.Err("COMMENT", "Navigation system not initialized.");
            return new List<Comment>();
        }
        
        var comments = await ApiClient.GetCommentsByCelestialBodyAsync(_state.CurrentBody.Id);
        
        // Sort comments
        if (sortOrder.Equals("newest", StringComparison.OrdinalIgnoreCase))
        {
            comments = comments.OrderByDescending(c => c.CreatedDate).ToList();
        }
        else if (sortOrder.Equals("oldest", StringComparison.OrdinalIgnoreCase))
        {
            comments = comments.OrderBy(c => c.CreatedDate).ToList();
        }
        
        // Apply limit if specified
        if (limit.HasValue && limit.Value > 0 && limit.Value < comments.Count)
        {
            comments = comments.Take(limit.Value).ToList();
        }

        // Get display names for all comments
        foreach (var comment in comments)
        {
            try
            {
                var user = await ApiClient.GetUserByIdAsync(comment.UserId);
                if (user != null)
                {
                    comment.DisplayName = user.DisplayName;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user display name: {ex.Message}");
                comment.DisplayName = "Unknown User";
            }
        }
        
        return comments;
    }
    
    // Get comments for a celestial body by name
    public static async Task<List<Comment>> GetCommentsForNamedBody(string bodyName, int? limit = null, string sortOrder = "newest")
    {
        // Get all celestial bodies
        var bodies = await ApiClient.GetCelestialBodiesMap();
        
        // Find the body by name (case-insensitive)
        var targetBody = bodies.Values.FirstOrDefault(b => 
            b.BodyName.Equals(bodyName, StringComparison.OrdinalIgnoreCase));
        
        if (targetBody == null)
        {
            TUI.Err("COMMENT", $"Celestial body '{bodyName}' not found.");
            return new List<Comment>();
        }
        
        var comments = await ApiClient.GetCommentsByCelestialBodyAsync(targetBody.Id);
        
        // Sort comments
        if (sortOrder.Equals("newest", StringComparison.OrdinalIgnoreCase))
        {
            comments = comments.OrderByDescending(c => c.CreatedDate).ToList();
        }
        else if (sortOrder.Equals("oldest", StringComparison.OrdinalIgnoreCase))
        {
            comments = comments.OrderBy(c => c.CreatedDate).ToList();
        }
        
        // Apply limit if specified
        if (limit.HasValue && limit.Value > 0 && limit.Value < comments.Count)
        {
            comments = comments.Take(limit.Value).ToList();
        }
        
        return comments;
    }
    
    // Get comments for current body by date range
    public static async Task<List<Comment>> GetCommentsByDateRange(DateTime startDate, DateTime endDate, int? limit = null, string sortOrder = "newest")
    {
        if (_state.CurrentBody == null)
        {
            TUI.Err("COMMENT", "Navigation system not initialized.");
            return new List<Comment>();
        }
        
        var comments = await ApiClient.GetCommentsByDateRangeAsync(startDate, endDate, _state.CurrentBody.Id);
        
        // Sort comments
        if (sortOrder.Equals("newest", StringComparison.OrdinalIgnoreCase))
        {
            comments = comments.OrderByDescending(c => c.CreatedDate).ToList();
        }
        else if (sortOrder.Equals("oldest", StringComparison.OrdinalIgnoreCase))
        {
            comments = comments.OrderBy(c => c.CreatedDate).ToList();
        }
        
        // Apply limit if specified
        if (limit.HasValue && limit.Value > 0 && limit.Value < comments.Count)
        {
            comments = comments.Take(limit.Value).ToList();
        }
        
        return comments;
    }
    
    // Create a new comment for the current celestial body
    public static async Task<Comment?> CreateComment(string commentText)
    {
        if (_state.CurrentBody == null)
        {
            TUI.Err("COMMENT", "Navigation system not initialized.");
            return null;
        }
        
        return await ApiClient.CreateCommentAsync(commentText, _state.CurrentBody.Id);
    }

    // Delete a comment by ID
    public static async Task<bool> DeleteComment(int commentId)
    {
        try
        {
            return await ApiClient.DeleteCommentAsync(commentId);
        }
        catch (Exception ex)
        {
            TUI.Err("COMMENT", "Failed to delete comment.", ex.Message);
            return false;
        }
    }

    public static async Task<Comment> UpdateComment(int commentId, string commentText)
    {
        try
        {
            return await ApiClient.UpdateCommentAsync(commentId, commentText);
        }
        catch (Exception ex)
        {
            TUI.Err("COMMENT", "Failed to update comment.", ex.Message);
            return null;
        }
    }

    // Get a list of child celestial body names for autocomplete
    public static async Task<string[]> GetAvailableDestinations()
    {
        if (_state.CurrentBody == null)
        {
            return Array.Empty<string>();
        }
        
        try
        {
            var children = await GetChildren(_state.CurrentBody.Id);
            var destinations = children.Select(c => c.BodyName).ToList();
            
            // Add special navigation options
            destinations.Add("..");
            destinations.Add("/");
            
            return destinations.ToArray();
        }
        catch (Exception ex)
        {
            TUI.Err("CD", "Failed to get available destinations.", ex.Message);
            return Array.Empty<string>();
        }
    }
}
