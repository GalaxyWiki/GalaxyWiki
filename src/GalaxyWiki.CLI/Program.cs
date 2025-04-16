using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using dotenv.net;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.CLI;
using System.Text.RegularExpressions;
using System.Text;

namespace GalaxyWiki.Cli
{
    public static class Program
    {
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
                
                var inp = AnsiConsole.Ask<string>($"[lightcyan1]{promptPath}[/] [springgreen3_1]❯❯[/]");
                
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

                    case "cal": AnsiConsole.Write(TUI.Calendar()); break;

                    case "search": AnsiConsole.WriteLine("TODO: Search wiki pages"); break;

                    case "pwd": AnsiConsole.Write(TUI.Path(CommandLogic.GetCurrentPath())); break;

                    case "cd": await HandleCdCommand(dat); break;

                    case "ls": await HandleLsCommand(); break;

                    case "list":
                    case "find": await HandleListCommand(dat); break;

                    case "show":
                    case "info": await HandleShowCommand(dat); break;

                    case "render": AnsiConsole.Write(TUI.Image("../../assets/earth.png")); break;

                    case "chat": LaunchChatbot(); break;

                    case "login": await Login(); break;

                    default: HandleUnknownCommand(cmd); break;
                }
            }
            
            return 0;
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
        
        static async Task ShowInfoForCurrentLocation()
        {
            var body = CommandLogic.GetCurrentBody();
            var revision = await CommandLogic.GetCurrentRevision();
            
            if (revision == null)
            {
                TUI.Err("INFO", "No content available for this celestial body.", 
                    "This celestial body might not have an active revision.");
                return;
            }
            
            List<Comment> comments = new List<Comment>();
            if (revision.CelestialBodyName != null) {
                comments = await CommandLogic.GetCommentsForNamedBody(revision.CelestialBodyName);
            }

            // AnsiConsole.Write(TUI.Article(revision.CelestialBodyName ?? "Unknown", revision.Content));
            // AnsiConsole.Write(TUI.AuthorInfo(revision.AuthorDisplayName ?? "Unknown", revision.CreatedAt));
            AnsiConsole.Write(TUI.WikiPage(revision, body.BodyType, comments));
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

        static async Task LaunchChatbot() {
            var header = new Rule("[cyan] Galaxy Bot :robot: :sparkles: [/]");
            AnsiConsole.Write(header);

            // Ensure environment variables are loaded
            DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { ".env" }));
            var apiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
            
            if (string.IsNullOrEmpty(apiKey))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] CLAUDE_API_KEY environment variable is not set.");
                AnsiConsole.MarkupLine("Please check that your .env file contains a valid CLAUDE_API_KEY.");
                return;
            }

            bool chatMode = true;
            while(chatMode) {
                var msg = AnsiConsole.Ask<string>("[lightcyan1]Enter a message[/] [orange1]❯❯[/]");
                if (msg.ToLower() == "quit" || msg.ToLower() == "exit") { 
                    chatMode = false; 
                }
                else { 
                    await AnsiConsole.Status()
                        .StartAsync("Thinking...", async ctx => {
                            var response = await ClaudeClient.GetResponse(msg);
                            AnsiConsole.MarkupLine($"[green]Bot:[/] {response}");
                        });
                }
            }
        }
        
        // static void LaunchChatbot() {
        //     var header = new Rule("[cyan] Galaxy Bot :robot: :sparkles: [/]");
        //     AnsiConsole.Write(header);

        //     bool chatMode = true;
        //     while(chatMode) {
        //         var msg = AnsiConsole.Ask<string>("[lightcyan1]Enter a message[/] [gold3]❯❯[/]");
        //         if (msg.ToLower() == "quit" || msg.ToLower() == "exit") { chatMode = false; }
        //         else { AnsiConsole.WriteLine("TODO: Bot response"); }
        //     }
        // }

        static async Task Login() {
            Console.Write(new Rule("[gold3]Obtaining[/] JWT"));
            await GoogleAuthenticator.GetIdTokenAsync();

            Console.Write(new Rule("[green]JWT Obtained[/]"));
            Console.WriteLine(GoogleAuthenticator.JWT);

            Console.Write(new Rule("[cyan]Logging in[/] with API"));
            await ApiClient.LoginAsync(GoogleAuthenticator.JWT);
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
            
            if (string.IsNullOrWhiteSpace(commentText)) {
                TUI.Err("COMMENT", "Comment text is empty.");
                return;
            }
            
            var comment = await CommandLogic.CreateComment(commentText);
            
            if (comment != null) {
                AnsiConsole.MarkupLine("[green]Comment added successfully![/]");
                
                // Display the newly added comment
                var newComments = new List<Comment> { comment };
                AnsiConsole.Write(TUI.CommentsPanel(newComments, "Your New Comment"));
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
    }
}