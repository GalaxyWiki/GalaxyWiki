using GalaxyWiki.Core.ResponseBodies;
using GalaxyWiki.Core.Entities;
using Spectre.Console;
using System.Text;

namespace GalaxyWiki.CLI
{
    public class NavigationState
    {
        // Current celestial body
        public CelestialBodies? CurrentBody { get; set; }

        // Path stack to track navigation history
        public Stack<CelestialBodies> PathStack { get; set; } = new Stack<CelestialBodies>();

        // Cache for children of bodies to avoid repeated API calls
        public Dictionary<int, List<CelestialBodies>> ChildrenCache { get; set; } = [];
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
        private static readonly NavigationState _state = new();
        private static readonly List<BodyTypeInfo> _bodyTypes =
        [
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
        ];

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
                return input[1..^1];
            }

            return input;
        }

        // List children of current directory
        public static async Task<PaginatedCelestialBodiesResponse?> ListDirectory(int page = 1, int size = 10)
        {
            if (_state.CurrentBody == null)
            {
                TUI.Err("LS", "Navigation system not initialized.");
                return null;
            }

            try
            {
                // Call API
                string endpoint = $"/celestial-body/{_state.CurrentBody.Id}/children?pageNumber={page}&pageSize={size}";
                PaginatedCelestialBodiesResponse paginatedResponse = await ApiClient.GetDeserialized<PaginatedCelestialBodiesResponse>(endpoint);

                return paginatedResponse;
            }
            catch (Exception ex)
            {
                TUI.Err("LS", "Failed to list directory.", ex.Message);
                return null;
            }
        }

        // Helper to get children with caching
        private static async Task<List<CelestialBodies>> GetChildren(int parentId)
        {
            // Check cache first
            if (_state.ChildrenCache.TryGetValue(parentId, out var cachedChildren)) { return cachedChildren; }

            // Call API
            string endpoint = $"/celestial-body/{parentId}/children";
            var childrenData = await ApiClient.GetDeserialized<List<CelestialBodies>>(endpoint);
            childrenData.Sort((a, b) => a.Id.CompareTo(b.Id));

            // Add to cache
            _state.ChildrenCache[parentId] = childrenData;

            return childrenData;
        }

        // Get the active revision content for the current body
        public static async Task<Revision?> GetCurrentRevision()
        {
            if (_state.CurrentBody == null || !_state.CurrentBody.ActiveRevision.HasValue) { return null; }
            return await ApiClient.GetRevisionAsync($"/api/revision/{_state.CurrentBody.ActiveRevision}");
        }

        // Display a tree view of celestial bodies
        public static async Task DisplayTree(bool useCurrentAsRoot = false)
        {
            if (_state.CurrentBody == null) { TUI.Err("TREE", "Navigation system not initialized."); return; }

            // Determine root node for the tree
            CelestialBodies rootBody;

            // Use a loading spinner while retrieving and building the tree
            await AnsiConsole.Status()
                .AutoRefresh(true)
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("cyan"))
                .StartAsync("Loading celestial body tree...", async ctx => 
                {
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
                        ctx.Status("Retrieving universe data...");
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
                    ctx.Status("Building celestial hierarchy...");
                    var tree = new Tree(FormatCelestialBodyLabel(rootBody));
                    await BuildTreeRecursively(rootBody.Id, tree);

                    // Update status before displaying
                    ctx.Status("Rendering tree...");
                    
                    // Display tree with some styling
                    tree.Guide = TreeGuide.BoldLine;
                    tree.Style = Style.Parse("blue");
                    AnsiConsole.Write(tree);
                });
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
            var (DisplayLabel, Body) = items.FirstOrDefault(i => i.DisplayLabel == selection);

            if (Body == null)
            {
                TUI.Err("WARP", "Selected celestial body not found.");
                return;
            }

            // Navigate to the selected body
            await WarpToBody(Body);
        }

        // Navigate to a specific celestial body by following the path from root
        private static async Task WarpToBody(CelestialBodies targetBody)
        {
            if (targetBody == null) return;

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
                        ctx.Status($"Passing through {pathParts[i - 1]}...");
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
                return [.. allBodies.Where(b => b.BodyType == bodyTypeId)];
            }
            catch (Exception ex)
            {
                TUI.Err("LIST", "Failed to list celestial bodies by type.", ex.Message);
                return [];
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
                return [];
            }

            var comments = await ApiClient.GetCommentsByCelestialBodyAsync(_state.CurrentBody.Id);

            // Sort comments
            if (sortOrder.Equals("newest", StringComparison.OrdinalIgnoreCase))
            {
                comments = [.. comments.OrderByDescending(c => c.CreatedDate)];
            }
            else if (sortOrder.Equals("oldest", StringComparison.OrdinalIgnoreCase))
            {
                comments = [.. comments.OrderBy(c => c.CreatedDate)];
            }

            // Apply limit if specified
            if (limit.HasValue && limit.Value > 0 && limit.Value < comments.Count)
            {
                comments = [.. comments.Take(limit.Value)];
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
                return [];
            }

            var comments = await ApiClient.GetCommentsByCelestialBodyAsync(targetBody.Id);

            // Sort comments
            if (sortOrder.Equals("newest", StringComparison.OrdinalIgnoreCase))
            {
                comments = [.. comments.OrderByDescending(c => c.CreatedDate)];
            }
            else if (sortOrder.Equals("oldest", StringComparison.OrdinalIgnoreCase))
            {
                comments = [.. comments.OrderBy(c => c.CreatedDate)];
            }

            // Apply limit if specified
            if (limit.HasValue && limit.Value > 0 && limit.Value < comments.Count)
            {
                comments = [.. comments.Take(limit.Value)];
            }

            return comments;
        }

        // Get comments for current body by date range
        public static async Task<List<Comment>> GetCommentsByDateRange(DateTime startDate, DateTime endDate, int? limit = null, string sortOrder = "newest")
        {
            if (_state.CurrentBody == null)
            {
                TUI.Err("COMMENT", "Navigation system not initialized.");
                return [];
            }

            var comments = await ApiClient.GetCommentsByDateRangeAsync(startDate, endDate, _state.CurrentBody.Id);

            // Sort comments
            if (sortOrder.Equals("newest", StringComparison.OrdinalIgnoreCase))
            {
                comments = [.. comments.OrderByDescending(c => c.CreatedDate)];
            }
            else if (sortOrder.Equals("oldest", StringComparison.OrdinalIgnoreCase))
            {
                comments = [.. comments.OrderBy(c => c.CreatedDate)];
            }

            // Apply limit if specified
            if (limit.HasValue && limit.Value > 0 && limit.Value < comments.Count)
            {
                comments = [.. comments.Take(limit.Value)];
            }

            return comments;
        }

        // Create a new comment for the current celestial body
        public static async Task<Comment?> CreateComment(string commentText)
        {
            if (_state.CurrentBody == null)
            {
                TUI.Err("COMMENT", "No celestial body selected.");
                return null;
            }

            // Create the comment via API
            return await ApiClient.CreateCommentAsync(commentText, _state.CurrentBody.Id);
        }

        // Delete a comment by its ID
        public static async Task<bool> DeleteComment(int commentId)
        {
            // Check if user is logged in (authenticated users only)
            if (string.IsNullOrEmpty(ApiClient.JWT))
            {
                TUI.Err("AUTH", "You must be logged in to delete comments.");
                return false;
            }

            // Delete the comment via API
            return await ApiClient.DeleteCommentAsync(commentId);
        }
        public static async Task<Comment> UpdateComment(int commentId, string commentText)
        {
            try { return await ApiClient.UpdateCommentAsync(commentId, commentText); }
            catch (Exception ex)
            {
                TUI.Err("COMMENT", "Failed to update comment.", ex.Message);
                return null;
            }
        }

        public static async Task<ContentRevisions> CreateRevision(string celestialBodyPath, string newContent)
        {
            try { return await ApiClient.CreateRevisionAsync(celestialBodyPath, newContent); }
            catch (Exception ex)
            {
                TUI.Err("REVISION", "Failed to create content revision.", ex.Message);
                return null;
            }
        }


        public static async Task<Comment?> GetCommentById(int commentId)
        {
            try
            {
                // Get all comments and find the one with matching ID
                var comments = await ApiClient.GetCommentsByCelestialBodyAsync(_state.CurrentBody.Id);
                return comments.FirstOrDefault(c => c.CommentId == commentId);
            }
            catch (Exception ex)
            {
                TUI.Err("COMMENT", "Failed to get comment.", ex.Message);
                return null;
            }
        }

        // Create a new celestial body
        public static async Task<CelestialBodies?> CreateCelestialBody(string bodyName, int bodyTypeId, int? orbitsId = null)
        {
            // Check if user is logged in (authenticated users only)
            if (string.IsNullOrEmpty(ApiClient.JWT))
            {
                TUI.Err("AUTH", "You must be logged in to create celestial bodies.");
                return null;
            }

            // Validate body type
            var bodyType = GetBodyTypeInfo(bodyTypeId.ToString());
            if (bodyType == null)
            {
                TUI.Err("BODY", $"Invalid body type ID: {bodyTypeId}");
                return null;
            }

            // Create the celestial body via API
            var newBody = await ApiClient.CreateCelestialBodyAsync(bodyName, bodyTypeId, orbitsId);

            if (newBody != null)
            {
                // Clear cache to ensure the new body appears in subsequent listings
                _state.ChildrenCache.Clear();
            }

            return newBody;
        }

        // Update an existing celestial body
        public static async Task<CelestialBodies?> UpdateCelestialBody(int bodyId, string bodyName, int bodyTypeId, int? orbitsId = null)
        {
            // Check if user is logged in (authenticated users only)
            if (string.IsNullOrEmpty(ApiClient.JWT))
            {
                TUI.Err("AUTH", "You must be logged in to update celestial bodies.");
                return null;
            }

            // Validate body type
            var bodyType = GetBodyTypeInfo(bodyTypeId.ToString());
            if (bodyType == null)
            {
                TUI.Err("BODY", $"Invalid body type ID: {bodyTypeId}");
                return null;
            }

            // Update the celestial body via API
            var updatedBody = await ApiClient.UpdateCelestialBodyAsync(bodyId, bodyName, bodyTypeId, orbitsId);
            if (updatedBody != null) { RefreshCurrentBodyState(updatedBody); }

            return updatedBody;
        }

        public static async Task QuickRefreshBody()
        {
            if (_state.CurrentBody == null) return;
            var newBody = await ApiClient.GetCelestialBodyAsync(_state.CurrentBody.Id);
            if (newBody != null) { RefreshCurrentBodyState(newBody); }
        }

        static void RefreshCurrentBodyState(CelestialBodies updatedBody)
        {
            // Clear cache to ensure the updated body appears correctly in subsequent listings
            _state.ChildrenCache.Clear();

            // If we updated the current body, update the state
            if (_state.CurrentBody != null && _state.CurrentBody.Id == updatedBody.Id)
            {
                _state.CurrentBody = updatedBody;

                // Update the path stack
                var newStack = new Stack<CelestialBodies>();
                foreach (var body in _state.PathStack.Reverse())
                {
                    if (body.Id == updatedBody.Id) { newStack.Push(updatedBody); }
                    else { newStack.Push(body); }
                }

                _state.PathStack = new Stack<CelestialBodies>(newStack.Reverse());
            }
        }

        // Delete a celestial body
        public static async Task<bool> DeleteCelestialBody(int bodyId)
        {
            // Check if user is logged in (authenticated users only)
            if (string.IsNullOrEmpty(ApiClient.JWT))
            {
                TUI.Err("AUTH", "You must be logged in to delete celestial bodies.");
                return false;
            }

            // Check if trying to delete the current body
            if (_state.CurrentBody != null && _state.CurrentBody.Id == bodyId)
            {
                TUI.Err("BODY", "Cannot delete the celestial body you're currently in.",
                    "Navigate to the parent body first using 'cd ..'");
                return false;
            }

            // Delete the celestial body via API
            bool success = await ApiClient.DeleteCelestialBodyAsync(bodyId);

            if (success)
            {
                // Clear cache to ensure deleted body doesn't appear in subsequent listings
                _state.ChildrenCache.Clear();
            }

            return success;
        }

        // Get a list of child celestial body names for autocomplete
        public static async Task<string[]> GetAvailableDestinations()
        {
            if (_state.CurrentBody == null)
            {
                return [];
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
                return [];
            }
        }
    }
}