using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using dotenv.net;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.CLI;
using System.Text.RegularExpressions;
using System.Text;

namespace GalaxyWiki.CLI
{
    public static class Program
    {
        // Command history to store previous commands
        private static List<string> _commandHistory = new List<string>();
        private static int _historyIndex = -1;

        public static async Task<int> Main(string[] args)
        {
            //==================== Main command loop ====================//

            // AnsiConsole.Live(layout);
            DotEnv.Load();
            TUI.ShowBanner();

            // Initialize the navigation system
            await CommandLogic.Initialize();
            
            bool running = true;
            while(running) {
                // Get the current path to display in prompt
                string promptPath = CommandLogic.GetCurrentPath();
                
                // Display prompt
                AnsiConsole.Markup($"[lightcyan1]{promptPath}[/] [springgreen3_1]❯❯[/] ");
                
                // Use custom input method with command history support
                var inp = ReadLineWithHistory();
                
                // Skip empty input
                if (string.IsNullOrWhiteSpace(inp))
                    continue;
                
                // Add command to history if not empty and not a duplicate of the most recent command
                if (!string.IsNullOrWhiteSpace(inp) && 
                    (_commandHistory.Count == 0 || inp != _commandHistory[_commandHistory.Count - 1])) {
                    _commandHistory.Add(inp);
                }
                
                // Reset history navigation index
                _historyIndex = -1;
                
                // Parse command and arguments, handling quoted strings
                var (cmd, dat) = ParseCommand(inp);

                switch(cmd) {
                    case "quit":
                    case "exit": running = false; break;

                    case "help": PrintHelp(); break;

                    case "banner": TUI.ShowBanner(); break;

                    case "clear":
                    case "cls": AnsiConsole.Clear(); break;

                    case "comment": await HandleCommentCommand(dat); break;

                    case "tree": await HandleTreeCommand(dat); break;

                    case "warp": await CommandLogic.WarpToSelectedBody(); break;

                    case "go": await ShowCdAutocomplete(); break;

                    case "cd": await HandleCdCommand(dat); break;

                    case "cal": AnsiConsole.Write(TUI.Calendar()); break;

                    case "search": AnsiConsole.WriteLine("TODO: Search wiki pages"); break;

                    case "pwd": AnsiConsole.Write(TUI.Path(CommandLogic.GetCurrentPath())); break;

                    case "ls": await HandleLsCommand(); break;

                    case "list":
                    case "find": await HandleListCommand(dat); break;

                    case "show":
                    case "info": await HandleShowCommand(dat); break;

                    case "render": await HandleRenderCommand(); break;

                    case "chat": LaunchChatbot(); break;

                    case "login": await ApiClient.LoginAsync(); break;

                    case "revision": await HandleRevisionCommand(dat); break;

                    default: HandleUnknownCommand(cmd); break;
                }
            }
            
            return 0;
        }

        // Custom input method that supports command history with up/down arrows
        private static string ReadLineWithHistory()
        {
            StringBuilder input = new StringBuilder();
            int cursorPos = 0;
            
            // Starting position for editable area
            int startLeft = Console.CursorLeft;
            int startTop = Console.CursorTop;
            
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine(); // Move to next line after Enter
                    return input.ToString();
                }
                else if (keyInfo.Key == ConsoleKey.Backspace && cursorPos > 0)
                {
                    input.Remove(cursorPos - 1, 1);
                    cursorPos--;
                    
                    // Redraw the input line
                    Console.SetCursorPosition(startLeft, startTop);
                    Console.Write(new string(' ', input.Length + 1)); // Clear the line
                    Console.SetCursorPosition(startLeft, startTop);
                    Console.Write(input.ToString());
                    Console.SetCursorPosition(startLeft + cursorPos, startTop);
                }
                else if (keyInfo.Key == ConsoleKey.Delete && cursorPos < input.Length)
                {
                    input.Remove(cursorPos, 1);
                    
                    // Redraw the input line
                    Console.SetCursorPosition(startLeft, startTop);
                    Console.Write(new string(' ', input.Length + 1)); // Clear the line
                    Console.SetCursorPosition(startLeft, startTop);
                    Console.Write(input.ToString());
                    Console.SetCursorPosition(startLeft + cursorPos, startTop);
                }
                else if (keyInfo.Key == ConsoleKey.LeftArrow && cursorPos > 0)
                {
                    cursorPos--;
                    Console.SetCursorPosition(startLeft + cursorPos, startTop);
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow && cursorPos < input.Length)
                {
                    cursorPos++;
                    Console.SetCursorPosition(startLeft + cursorPos, startTop);
                }
                else if (keyInfo.Key == ConsoleKey.Home)
                {
                    cursorPos = 0;
                    Console.SetCursorPosition(startLeft, startTop);
                }
                else if (keyInfo.Key == ConsoleKey.End)
                {
                    cursorPos = input.Length;
                    Console.SetCursorPosition(startLeft + cursorPos, startTop);
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    // Navigate backward in history
                    if (_commandHistory.Count > 0)
                    {
                        // Move to the previous command in history (if possible)
                        _historyIndex = Math.Min(_commandHistory.Count - 1, _historyIndex + 1);
                        string historyCommand = _commandHistory[_commandHistory.Count - 1 - _historyIndex];
                        
                        // Clear current input
                        Console.SetCursorPosition(startLeft, startTop);
                        Console.Write(new string(' ', input.Length));
                        Console.SetCursorPosition(startLeft, startTop);
                        
                        // Replace with command from history
                        input.Clear();
                        input.Append(historyCommand);
                        Console.Write(input.ToString());
                        cursorPos = input.Length;
                        Console.SetCursorPosition(startLeft + cursorPos, startTop);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    // Navigate forward in history
                    if (_historyIndex > 0)
                    {
                        _historyIndex--;
                        string historyCommand = _commandHistory[_commandHistory.Count - 1 - _historyIndex];
                        
                        // Clear current input
                        Console.SetCursorPosition(startLeft, startTop);
                        Console.Write(new string(' ', input.Length));
                        Console.SetCursorPosition(startLeft, startTop);
                        
                        // Replace with command from history
                        input.Clear();
                        input.Append(historyCommand);
                        Console.Write(input.ToString());
                        cursorPos = input.Length;
                        Console.SetCursorPosition(startLeft + cursorPos, startTop);
                    }
                    else if (_historyIndex == 0)
                    {
                        // Clear the input when navigating past the newest history entry
                        _historyIndex = -1;
                        Console.SetCursorPosition(startLeft, startTop);
                        Console.Write(new string(' ', input.Length));
                        Console.SetCursorPosition(startLeft, startTop);
                        input.Clear();
                        cursorPos = 0;
                    }
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    // Insert character at cursor position
                    input.Insert(cursorPos, keyInfo.KeyChar);
                    cursorPos++;
                    
                    // Redraw the input line
                    Console.SetCursorPosition(startLeft, startTop);
                    Console.Write(input.ToString());
                    Console.SetCursorPosition(startLeft + cursorPos, startTop);
                }
            }
        }

        // Parse command and arguments, handling quoted strings
        private static (string cmd, string args) ParseCommand(string input)
        {
            input = input.Trim();
            
            if (string.IsNullOrEmpty(input))
                return ("", "");
                
            // Handle the case where the entire input is just the command
            int firstSpaceIndex = input.IndexOf(' ');
            if (firstSpaceIndex < 0)
                return (input.ToLower(), "");
                
            // Extract the command (everything before the first space)
            string cmd = input.Substring(0, firstSpaceIndex).ToLower();
            
            // Extract arguments (everything after the first space)
            string args = input.Substring(firstSpaceIndex + 1).Trim();
            
            return (cmd, args);
        }

        //==================== Commands ====================//

        static void PrintHelp() { 
            AnsiConsole.WriteLine("Available Commands:");
            AnsiConsole.WriteLine();
            
            // Create a simple grid without using markup
            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();
            
            // Add headers
            grid.AddRow(
                new Text("Command", Style.Parse("bold")), 
                new Text("Description", Style.Parse("bold"))
            );
            
            // Add rows with plain Text objects to avoid markup parsing
            grid.AddRow(new Text("ls"), new Text("List celestial bodies in current location"));
            grid.AddRow(new Text("cd <name>"), new Text("Navigate to a celestial body"));
            grid.AddRow(new Text("cd 'Name with spaces'"), new Text("Navigate to a celestial body with spaces in the name"));
            grid.AddRow(new Text("cd .."), new Text("Navigate to parent celestial body"));
            grid.AddRow(new Text("cd /"), new Text("Navigate to Universe (root)"));
            grid.AddRow(new Text("go"), new Text("Interactive navigation with autocomplete"));
            grid.AddRow(new Text("tree"), new Text("Display full celestial body hierarchy"));
            grid.AddRow(new Text("tree -h"), new Text("Display hierarchy from current location"));
            grid.AddRow(new Text("warp"), new Text("Show interactive tree and warp to any celestial body"));
            grid.AddRow(new Text("list/find"), new Text("List all celestial body types"));
            grid.AddRow(new Text("list/find -t <type>"), new Text("List all celestial bodies of a specific type (by name or ID)"));
            grid.AddRow(new Text("show/info"), new Text("Display wiki content for current celestial body"));
            grid.AddRow(new Text("show/info -n <name>"), new Text("Display wiki content for specified celestial body by name"));
            grid.AddRow(new Text("comment"), new Text("View comments for current celestial body"));
            grid.AddRow(new Text("comment \"text\""), new Text("Add a new comment to current celestial body"));
            grid.AddRow(new Text("comment --help"), new Text("Show detailed comment command options"));
            grid.AddRow(new Text("render"), new Text("Render the current celestial body"));
            grid.AddRow(new Text("pwd"), new Text("Display current location path"));
            grid.AddRow(new Text("clear/cls"), new Text("Clear the screen"));
            grid.AddRow(new Text("exit/quit"), new Text("Exit the application"));
            
            AnsiConsole.Write(grid);
            AnsiConsole.WriteLine();
        }

        static void HandleUnknownCommand(string cmd) {
            TUI.Err("CMD", $"Unknown command [bold italic cyan]{cmd}[/]", "Run [bold italic blue]help[/] for options");
        }

        static async Task HandleCdCommand(string target)
        {
            // If no target is provided, show autocomplete prompt
            if (string.IsNullOrWhiteSpace(target))
            {
                await ShowCdAutocomplete();
                return;
            }
            
            await CommandLogic.ChangeDirectory(target);
        }
        
        static async Task ShowCdAutocomplete()
        {
            // Get available destinations
            var destinations = await CommandLogic.GetAvailableDestinations();
            
            if (destinations.Length == 0)
            {
                TUI.Err("CD", "No destinations available.");
                return;
            }
            
            // Show destination selector
            var selectedDestination = TUI.DestinationSelector(destinations, "Navigate to:");
            
            if (selectedDestination != null)
            {
                // Navigate to the selected destination
                await CommandLogic.ChangeDirectory(selectedDestination);
            }
        }

        static async Task HandleTreeCommand(string args)
        {
            // Check for the "-h" or "--here" flag
            bool useCurrentAsRoot = args.Trim().Equals("-h", StringComparison.OrdinalIgnoreCase) || 
                                   args.Trim().Equals("--here", StringComparison.OrdinalIgnoreCase);
            
            await CommandLogic.DisplayTree(useCurrentAsRoot);
        }

        static async Task HandleListCommand(string args)
        {
            // Parse arguments
            var argParts = args.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            
            // If no arguments or not the right format, show all body types
            if (argParts.Length < 2 || !argParts[0].Equals("-t", StringComparison.OrdinalIgnoreCase))
            {
                // Display all body types
                DisplayBodyTypes();
                return;
            }
            
            // Get the type identifier (name or ID)
            string typeIdentifier = CommandLogic.TrimQuotes(argParts[1]);
            
            // Get body type info
            var bodyType = CommandLogic.GetBodyTypeInfo(typeIdentifier);
            if (bodyType == null)
            {
                TUI.Err("LIST", $"Unknown body type: {typeIdentifier}", 
                    "Use 'list' without arguments to see available types.");
                return;
            }
            
            // Get and display bodies of the specified type
            await DisplayBodiesByType(bodyType);
        }

        static void DisplayBodyTypes()
        {
            var bodyTypes = CommandLogic.GetBodyTypes();
            
            var table = new Table();
            table.AddColumn("ID");
            table.AddColumn("Type");
            table.AddColumn("Symbol");
            table.AddColumn("Description");
            
            foreach (var type in bodyTypes)
            {
                table.AddRow(
                    type.Id.ToString(),
                    type.Name,
                    type.Emoji,
                    type.Description
                );
            }
            
            table.Title = new TableTitle("Celestial Body Types");
            table.BorderColor(Color.Aqua);
            AnsiConsole.Write(table);
            
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Use[/] [cyan]list -t <type>[/] [grey]to list all bodies of a specific type.[/]");
            AnsiConsole.MarkupLine("[grey]Example:[/] [cyan]list -t Planet[/] [grey]or[/] [cyan]list -t 3[/]");
        }

        static async Task DisplayBodiesByType(BodyTypeInfo bodyType)
        {
            // Show loading indicator
            var bodies = await AnsiConsole.Status()
                .StartAsync($"Searching for {bodyType.Name}s...", _ => 
                    CommandLogic.ListCelestialBodiesByType(bodyType.Id));
            
            if (bodies.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]No {bodyType.Name}s found in the database.[/]");
                return;
            }
            
            var table = new Table();
            table.AddColumn("ID");
            table.AddColumn("Name");
            table.AddColumn("Orbits");
            
            foreach (var body in bodies)
            {
                table.AddRow(
                    new Text(body.Id.ToString()),
                    new Text(body.BodyName),
                    new Text(body.Orbits?.BodyName ?? "None")
                );
            }
            
            table.Title = new TableTitle($"{bodyType.Emoji} {bodyType.Name}s ({bodies.Count})");
            table.BorderColor(Color.Aqua);
            AnsiConsole.Write(table);
            
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[grey]To navigate to a {bodyType.Name.ToLower()}, use[/] [cyan]cd \"Name\"[/] [grey]from its parent location.[/]");
            AnsiConsole.MarkupLine($"[grey]Or use[/] [cyan]warp[/] [grey]to select any {bodyType.Name.ToLower()} directly.[/]");
        }

        static async Task HandleLsCommand()
        {
            var children = await CommandLogic.ListDirectory();
            
            if (children.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No celestial bodies found in this location.[/]");
                return;
            }
            
            var table = new Table();
            table.AddColumn("Type");
            table.AddColumn("Name");
            table.AddColumn("ID");
            
            foreach (var child in children)
            {
                string emoji = TUI.BodyTypeToEmoji(child.BodyType);
                string id = child.Id.ToString();
                
                // If the name contains spaces, suggest using quotes
                string name = child.BodyName;
                if (name.Contains(" "))
                {
                    name = $"{name} [grey](use: cd '{name}')[/]";
                }
                
                table.AddRow(
                    new Text(emoji), 
                    new Markup(name), 
                    new Text(id)
                );
            }
            
            AnsiConsole.Write(table);
        }

        static async Task HandleShowCommand(string args)
        {
            // Parse arguments to check for -n or --name flag
            var argParts = args.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            
            // Check if a specific celestial body was requested
            if (argParts.Length == 2 && (argParts[0].Equals("-n", StringComparison.OrdinalIgnoreCase) || 
                                         argParts[0].Equals("--name", StringComparison.OrdinalIgnoreCase)))
            {
                string bodyName = CommandLogic.TrimQuotes(argParts[1]);
                await ShowInfoForNamedBody(bodyName);
                return;
            }
            
            // If no specific body was requested, show info for current location
            await ShowInfoForCurrentLocation();
        }

        static async Task HandleRenderCommand()
        {
            var body = CommandLogic.GetCurrentBody();
            await TUI.RenderCelestialBody(body?.BodyName ?? "", body?.BodyType ?? -1);
        }
        
        static async Task ShowInfoForCurrentLocation()
        {
            var body = CommandLogic.GetCurrentBody();
            
            if (body == null)
            {
                TUI.Err("INFO", "No celestial body found at current location.");
                return;
            }
            
            var revision = await CommandLogic.GetCurrentRevision();
            
            if (revision == null)
            {
                TUI.Err("INFO", "No content available for this celestial body.", 
                    "This celestial body might not have an active revision.");
                return;
            }
            
            List<Comment> comments = [];
            if (revision.CelestialBodyName != null) {
                comments = await CommandLogic.GetCommentsForNamedBody(revision.CelestialBodyName);
            }

            // AnsiConsole.Write(TUI.Article(revision.CelestialBodyName ?? "Unknown", revision.Content));
            // AnsiConsole.Write(TUI.AuthorInfo(revision.AuthorDisplayName ?? "Unknown", revision.CreatedAt));
            AnsiConsole.Write(TUI.WikiPage(revision, body.BodyType, comments));
            await TUI.RenderCelestialBody(body.BodyName, body.BodyType);
        }
        
        static async Task ShowInfoForNamedBody(string bodyName)
        {
            var revision = await CommandLogic.GetRevisionByBodyName(bodyName);
            
            if (revision == null)
            {
                TUI.Err("INFO", $"No content available for '{bodyName}'.", 
                    "The celestial body might not exist or doesn't have an active revision.");
                return;
            }
            
            AnsiConsole.Write(TUI.Article(revision.CelestialBodyName ?? bodyName, null, revision.Content));
            AnsiConsole.Write(TUI.AuthorInfo(revision.AuthorDisplayName ?? "Unknown", revision.CreatedAt));
        }

        static async Task<IdMap<CelestialBodies>> GetAllCelestialBodies() {
            try { return await ApiClient.GetCelestialBodiesMap(); }
            catch (Exception ex) { TUI.Err("GET", "Cannot get celestial bodies", ex.Message); }

            return new IdMap<CelestialBodies>();
        }

        static async Task DisplayInteractiveUniverseTree()
        {
            IdMap<CelestialBodies> bodies = await GetAllCelestialBodies();
            
            // Create an empty list of selectable items
            var items = new List<(string DisplayLabel, CelestialBodies Body)>();
            
            // Find the root (Universe) node
            var root = bodies.Values.FirstOrDefault(b => b.Orbits == null);
            if (root == null) { TUI.Err("DB", "Could not find root celestial body."); return; }
            
            // Build a list of selectable items with proper indentation
            string? selection = TUI.CelestialTreeSelectable(bodies, root.Id, items);
            
            // Find the selected body
            var selectedItem = items.FirstOrDefault(i => i.DisplayLabel == selection);

            if (selectedItem.Body == null) {
                return;
            }
            
            // Navigate to the selected body
            await CommandLogic.ChangeDirectory("/"); // First go to root
            
            // Then navigate to each level of the path
            var pathParts = new List<string>();
            var current = selectedItem.Body;
            
            // Build the path from the selected body back to root
            while (current != null)
            {
                pathParts.Insert(0, current.BodyName);
                current = current.Orbits;
            }
            
            // Navigate to each part of the path
            foreach (var part in pathParts)
            {
                await CommandLogic.ChangeDirectory(part);
            }
        }
        
        static void LaunchChatbot() {
            var header = new Rule("[cyan] Galaxy Bot :robot: :sparkles: [/]");
            AnsiConsole.Write(header);

            bool chatMode = true;
            while(chatMode) {
                AnsiConsole.Markup("[lightcyan1]Enter a message[/] [gold3]❯❯[/] ");
                var msg = ReadLineWithHistory();
                
                if (msg.ToLower() == "quit" || msg.ToLower() == "exit") { 
                    chatMode = false; 
                }
                else { 
                    AnsiConsole.WriteLine("TODO: Bot response"); 
                }
            }
        }

        static async Task HandleCommentCommand(string args)
        {
            // If no arguments, display all comments for current celestial body
            if (string.IsNullOrWhiteSpace(args))
            {
                await ViewCommentsForCurrentBody();
                return;
            }
            
            // Split arguments by spaces while respecting quotes
            var argsList = SplitArgumentsRespectingQuotes(args);
            
            if (argsList.Count == 0)
            {
                await ViewCommentsForCurrentBody();
                return;
            }
            
            // Check if the first argument is a flag
            string firstArg = argsList[0].ToLower();
            
            // Add a new comment if arguments don't start with a flag
            if (!firstArg.StartsWith("-"))
            {
                await AddComment(args);
                return;
            }
            
            // Parse flags
            switch (firstArg)
            {
                case "-a":
                case "--add":
                    if (argsList.Count < 2)
                    {
                        TUI.Err("COMMENT", "No comment text provided.", "Usage: comment -a \"Your comment text\"");
                        return;
                    }
                    await AddComment(JoinArgs(argsList.Skip(1).ToList()));
                    break;
                
                case "-l":
                case "--limit":
                    if (argsList.Count < 2 || !int.TryParse(argsList[1], out int limit))
                    {
                        TUI.Err("COMMENT", "Invalid limit value.", "Usage: comment -l <number>");
                        return;
                    }
                    await ViewCommentsForCurrentBody(limit);
                    break;
                
                case "-s":
                case "--sort":
                    if (argsList.Count < 2)
                    {
                        TUI.Err("COMMENT", "Sort order not specified.", "Usage: comment -s <newest|oldest>");
                        return;
                    }
                    string sortOrder = argsList[1].ToLower();
                    if (sortOrder != "newest" && sortOrder != "oldest")
                    {
                        TUI.Err("COMMENT", "Invalid sort order.", "Valid options: newest, oldest");
                        return;
                    }
                    await ViewCommentsForCurrentBody(null, sortOrder);
                    break;
                
                case "-n":
                case "--name":
                    if (argsList.Count < 2)
                    {
                        TUI.Err("COMMENT", "Celestial body name not provided.", "Usage: comment -n \"Body Name\"");
                        return;
                    }
                    
                    string bodyName = argsList[1];
                    
                    // Check for additional flags
                    int? bodyLimit = null;
                    string bodySortOrder = "newest";
                    
                    for (int i = 2; i < argsList.Count - 1; i++)
                    {
                        if ((argsList[i] == "-l" || argsList[i] == "--limit") && int.TryParse(argsList[i+1], out int bodyLimitVal))
                        {
                            bodyLimit = bodyLimitVal;
                            i++; // Skip the next arg as we've consumed it
                        }
                        else if ((argsList[i] == "-s" || argsList[i] == "--sort") && i+1 < argsList.Count)
                        {
                            bodySortOrder = argsList[i+1].ToLower();
                            i++; // Skip the next arg as we've consumed it
                        }
                    }
                    
                    await ViewCommentsForNamedBody(bodyName, bodyLimit, bodySortOrder);
                    break;
                
                case "-d":
                case "--dates":
                    if (argsList.Count < 3)
                    {
                        TUI.Err("COMMENT", "Date range not properly specified.", 
                            "Usage: comment -d <startDate> <endDate>\nDates should be in the format YYYY-MM-DD.");
                        return;
                    }
                    
                    if (!DateTime.TryParse(argsList[1], out DateTime startDate) || 
                        !DateTime.TryParse(argsList[2], out DateTime endDate))
                    {
                        TUI.Err("COMMENT", "Invalid date format.", "Dates should be in the format YYYY-MM-DD.");
                        return;
                    }
                    
                    // Check for additional flags
                    int? dateLimit = null;
                    string dateSortOrder = "newest";
                    
                    for (int i = 3; i < argsList.Count - 1; i++)
                    {
                        if ((argsList[i] == "-l" || argsList[i] == "--limit") && int.TryParse(argsList[i+1], out int dateLimitVal))
                        {
                            dateLimit = dateLimitVal;
                            i++; // Skip the next arg as we've consumed it
                        }
                        else if ((argsList[i] == "-s" || argsList[i] == "--sort") && i+1 < argsList.Count)
                        {
                            dateSortOrder = argsList[i+1].ToLower();
                            i++; // Skip the next arg as we've consumed it
                        }
                    }
                    
                    await ViewCommentsByDateRange(startDate, endDate, dateLimit, dateSortOrder);
                    break;
                
                case "-del":
                case "--delete":
                    if (argsList.Count < 2 || !int.TryParse(argsList[1], out int commentId))
                    {
                        TUI.Err("COMMENT", "Invalid comment ID.", "Usage: comment -del <id>");
                        return;
                    }
                    await DeleteComment(commentId);
                    break;
                
                case "-h":
                case "--help":
                    DisplayCommentHelp();
                    break;
                
                default:
                    TUI.Err("COMMENT", $"Unknown flag: {firstArg}", "Use 'comment --help' to see available options.");
                    break;
            }
        }
        
        static void DisplayCommentHelp()
        {
            AnsiConsole.WriteLine("Comment Command Usage:");
            AnsiConsole.WriteLine();
            
            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();
            
            grid.AddRow(
                new Text("Command", Style.Parse("bold")), 
                new Text("Description", Style.Parse("bold"))
            );
            
            grid.AddRow(new Text("comment"), new Text("View all comments for the current celestial body"));
            grid.AddRow(new Text("comment \"Your text here\""), new Text("Add a new comment to the current celestial body"));
            grid.AddRow(new Text("comment -a \"Comment text\""), new Text("Add a new comment to the current celestial body"));
            grid.AddRow(new Text("comment -l <number>"), new Text("Limit the number of comments displayed"));
            grid.AddRow(new Text("comment -s <sort>"), new Text("Sort comments (newest or oldest)"));
            grid.AddRow(new Text("comment -n \"Body Name\""), new Text("View comments for the specified celestial body"));
            grid.AddRow(new Text("comment -d <start> <end>"), new Text("View comments within date range (YYYY-MM-DD format)"));
            grid.AddRow(new Text("comment -n \"Body\" -l 5 -s oldest"), new Text("Combine flags for specific queries"));
            grid.AddRow(new Text("comment -del <id>"), new Text("Delete a comment by ID"));
            
            AnsiConsole.Write(grid);
            AnsiConsole.WriteLine();
        }
        
        static async Task ViewCommentsForCurrentBody(int? limit = null, string sortOrder = "newest")
        {
            var comments = await CommandLogic.GetCommentsForCurrentBody(limit, sortOrder);
            
            string title = CommandLogic.GetCurrentBody()?.BodyName ?? "Unknown";
            title = $"Comments for {title}";
            
            if (limit.HasValue)
            {
                title += $" (Showing {Math.Min(limit.Value, comments.Count)} of {comments.Count})";
            }
            
            AnsiConsole.Write(TUI.CommentsPanel(comments, title));
        }
        
        static async Task ViewCommentsForNamedBody(string bodyName, int? limit = null, string sortOrder = "newest")
        {
            var comments = await CommandLogic.GetCommentsForNamedBody(bodyName, limit, sortOrder);
            
            string title = $"Comments for {bodyName}";
            
            if (limit.HasValue && comments.Count > 0)
            {
                title += $" (Showing {Math.Min(limit.Value, comments.Count)} of {comments.Count})";
            }
            
            AnsiConsole.Write(TUI.CommentsPanel(comments, title));
        }
        
        static async Task ViewCommentsByDateRange(DateTime startDate, DateTime endDate, int? limit = null, string sortOrder = "newest")
        {
            var comments = await CommandLogic.GetCommentsByDateRange(startDate, endDate, limit, sortOrder);
            
            string bodyName = CommandLogic.GetCurrentBody()?.BodyName ?? "Unknown";
            string dateRange = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}";
            string title = $"Comments for {bodyName} ({dateRange})";
            
            if (limit.HasValue && comments.Count > 0)
            {
                title += $" (Showing {Math.Min(limit.Value, comments.Count)} of {comments.Count})";
            }
            
            AnsiConsole.Write(TUI.CommentsPanel(comments, title));
        }
        
        static async Task AddComment(string commentText)
        {
            // Trim any surrounding quotes
            commentText = CommandLogic.TrimQuotes(commentText);
            
            if (string.IsNullOrWhiteSpace(commentText))
            {
                TUI.Err("COMMENT", "Comment text is empty.");
                return;
            }
            
            var comment = await CommandLogic.CreateComment(commentText);
            
            if (comment != null)
            {
                AnsiConsole.MarkupLine("[green]Comment added successfully![/]");
                
                // Display the newly added comment
                var newComments = new List<Comment> { comment };
                AnsiConsole.Write(TUI.CommentsPanel(newComments, "Your New Comment"));
            }
            else
            {
                TUI.Err("COMMENT", "Failed to add comment.");
            }
        }
        
        static async Task DeleteComment(int commentId)
        {
            if (AnsiConsole.Confirm($"Are you sure you want to delete comment with ID {commentId}?"))
            {
                bool success = await CommandLogic.DeleteComment(commentId);
                
                if (success)
                {
                    AnsiConsole.MarkupLine($"[green]Comment with ID {commentId} deleted successfully![/]");
                    
                    await ViewCommentsForCurrentBody();
                }
                else
                {
                    TUI.Err("COMMENT", $"Failed to delete comment with ID {commentId}.", 
                        "You might not have permission to delete this comment, or it doesn't exist.");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[grey]Comment deletion cancelled.[/]");
            }
        }
        
        // Helper to split arguments while respecting quoted strings
        static List<string> SplitArgumentsRespectingQuotes(string args)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var currentArg = new StringBuilder();
            
            for (int i = 0; i < args.Length; i++)
            {
                char c = args[i];
                
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    // Don't include the quote characters
                }
                else if (c == ' ' && !inQuotes)
                {
                    // End of an argument
                    if (currentArg.Length > 0)
                    {
                        result.Add(currentArg.ToString());
                        currentArg.Clear();
                    }
                }
                else
                {
                    currentArg.Append(c);
                }
            }
            
            // Add the last argument if there is one
            if (currentArg.Length > 0)
            {
                result.Add(currentArg.ToString());
            }
            
            return result;
        }
        
        // Helper to join arguments back into a single string
        static string JoinArgs(List<string> args)
        {
            return string.Join(" ", args);
        }

        static async Task HandleRevisionCommand(string args)
        {
            // Parse arguments to check for -n or --name flag
            var argParts = args.Split([' '], 2, StringSplitOptions.RemoveEmptyEntries);
            
            // Check if a specific celestial body was requested
            if (argParts.Length == 2 && (argParts[0].Equals("-n", StringComparison.OrdinalIgnoreCase) || 
                                         argParts[0].Equals("--name", StringComparison.OrdinalIgnoreCase)))
            {
                string bodyName = CommandLogic.TrimQuotes(argParts[1]);
                await ShowRevisionsForNamedBody(bodyName);
                return;
            }
            
            // If no specific body was requested, show revisions for current location
            await ShowRevisionsForCurrentLocation();
        }
        
        static async Task ShowRevisionsForCurrentLocation()
        {
            var body = CommandLogic.GetCurrentBody();
            
            if (body == null)
            {
                TUI.Err("REVISION", "No celestial body found at current location.");
                return;
            }
            
            await ShowRevisionsForNamedBody(body.BodyName);
        }
        
        static async Task ShowRevisionsForNamedBody(string bodyName)
        {
            try
            {
                var revisions = await ApiClient.GetRevisionsByBodyNameAsync(bodyName);
                
                if (revisions == null || revisions.Count == 0)
                {
                    TUI.Err("REVISION", $"No revisions found for '{bodyName}'.");
                    return;
                }
                
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .BorderColor(Color.Grey)
                    .AddColumn(new TableColumn("ID").LeftAligned())
                    .AddColumn(new TableColumn("Created At").LeftAligned())
                    .AddColumn(new TableColumn("Author").LeftAligned())
                    .AddColumn(new TableColumn("Preview").LeftAligned());
                
                foreach (var revision in revisions.OrderByDescending(r => r.CreatedAt))
                {
                    var previewContent = revision.Content?.Length > 50 
                        ? revision.Content[..50] + "..." 
                        : revision.Content ?? "";
                    
                    table.AddRow(
                        revision.Id.ToString(),
                        revision.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                        revision.AuthorDisplayName ?? "Unknown",
                        previewContent
                    );
                }
                
                AnsiConsole.Write(new Rule($"[gold3]Revision History for {bodyName}[/]"));
                AnsiConsole.Write(table);
            }
            catch (Exception ex)
            {
                TUI.Err("REVISION", $"Failed to retrieve revisions for '{bodyName}'", ex.Message);
            }
        }
    }
}