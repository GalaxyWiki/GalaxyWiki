using GalaxyWiki.Core.Entities;
using Spectre.Console;
using System.Text;
using System.Text.RegularExpressions;

namespace GalaxyWiki.CLI
{
    public class NavigationState
    {
        // Current celestial body
        public CelestialBodies? CurrentBody { get; set; }
        
        // Path stack to track navigation history
        public Stack<CelestialBodies> PathStack { get; set; } = new Stack<CelestialBodies>();
        
        // Cache for children of bodies to avoid repeated API calls
        public Dictionary<int, List<CelestialBodies>> ChildrenCache { get; set; } = new Dictionary<int, List<CelestialBodies>>();
    }

    public static class CommandLogic
    {
        private static NavigationState _state = new NavigationState();

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
                path.Append($"\\{body.BodyName}");
            }
            
            path.Append(">");
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
        private static string TrimQuotes(string input)
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

            return await ApiClient.GetRevisionAsync($"http://localhost:5216/api/revision/{_state.CurrentBody.ActiveRevision}");
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
            }
            else
            {
                // Find the universe root
                var bodies = await ApiClient.GetCelestialBodiesMap();
                rootBody = bodies.Values.FirstOrDefault(b => b.Orbits == null);
                
                if (rootBody == null)
                {
                    TUI.Err("TREE", "Could not find root celestial body (Universe).");
                    return;
                }
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
    }
} 