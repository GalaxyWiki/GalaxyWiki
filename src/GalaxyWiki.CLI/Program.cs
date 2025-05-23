﻿using Spectre.Console;
using dotenv.net;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.CLI;
using System.Text;
using GalaxyWiki.Core.ResponseBodies;
using System.Reflection.Metadata;
using Npgsql.Replication.PgOutput.Messages;
using System.Linq.Expressions;
using GalaxyWiki.Core.Services;
using System.Net.Http.Json;

namespace GalaxyWiki.CLI
{
    public static class Program
    {
        // Command history to store previous commands
        private static List<string> _commandHistory = [];
        private static int _historyIndex = -1;
        private static readonly SearchService _searchService = new SearchService();

        public static async Task<int> Main(string[] args)
        {
            //==================== Main command loop ====================//

            // AnsiConsole.Live(layout);
            DotEnv.Load();
            TUI.ShowBanner();

            // Initialize the navigation system
            await CommandLogic.Initialize();

            bool running = true;
            while (running)
            {
                // Get the current path to display in prompt
                string promptPath = CommandLogic.GetCurrentPath();
                AnsiConsole.Markup($"[lightcyan1]{promptPath}[/] [springgreen3_1]❯❯[/] ");
                //var inp = AnsiConsole.Ask<string>("[lightcyan1]Enter a command[/] [springgreen3_1]❯❯[/]");
                // Use custom input method with command history support
                var inp = ReadLineWithHistory();
                var parts = inp.Trim().Split(' ', 2);
                //var cmd = parts[0].ToLower();
                var searchTerm = parts.Length > 1 ? parts[1] : string.Empty;

                // Skip empty input
                if (string.IsNullOrWhiteSpace(inp))
                    continue;

                // Add command to history if not empty and not a duplicate of the most recent command
                if (!string.IsNullOrWhiteSpace(inp) &&
                    (_commandHistory.Count == 0 || inp != _commandHistory[^1]))
                {
                    _commandHistory.Add(inp);
                }

                // Reset history navigation index
                _historyIndex = -1;

                // Parse command and arguments, handling quoted strings
                var (cmd, dat) = ParseCommand(inp);

                switch (cmd)
                {
                    case "banner": TUI.ShowBanner(); break;
                    case "cal": AnsiConsole.Write(TUI.Calendar()); break;
                    case "cd": await HandleCdCommand(dat); break;
                    case "chat": await LaunchChatbot(); break;
                    case "clear":
                    case "cls": AnsiConsole.Clear(); break;
                    case "comment": await HandleCommentCommand(dat); break;
                    case "create-body": await HandleCreateBodyCommand(dat); break;
                    case "delete-body": await HandleDeleteBodyCommand(dat); break;
                    case "edit": await HandleEditCurrentRevision(); break;
                    case "go": await ShowCdAutocomplete(); break;
                    case "help": PrintHelp(); break;
                    case "search":
                        if (string.IsNullOrWhiteSpace(searchTerm))
                        {
                            AnsiConsole.MarkupLine("[yellow]Please provide a search term after 'search'.[/]");
                        }
                        else { await PerformSearch(searchTerm); }
                        break;
                    case "list":
                    case "find": await HandleListCommand(dat); break;
                    case "login": await ApiClient.LoginAsync(); break;
                    case "ls": await HandleLsCommand(dat); break;
                    case "pwd": AnsiConsole.Write(TUI.Path(CommandLogic.GetCurrentPath())); break;
                    case "quit":
                    case "exit": running = false; break;
                    case "render": await HandleRenderCommand(); break;
                    case "revision": await HandleRevisionCommand(dat); break;
                    case "show":
                    case "info": await HandleShowCommand(dat); break;
                    case "tree": await HandleTreeCommand(dat); break;
                    case "update-body": await HandleUpdateBodyCommand(dat); break;
                    case "warp": await CommandLogic.WarpToSelectedBody(); break;

                    default: HandleUnknownCommand(cmd); break;
                }
            }

            return 0;
        }

        // Custom input method that supports command history with up/down arrows
        private static string ReadLineWithHistory()
        {
            StringBuilder input = new();
            int cursorPos = 0;

            // Starting position for editable area
            int startLeft = Console.CursorLeft;
            int startTop = Console.CursorTop;

            while (true)
            {
                try
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
                catch { }
            }
        }

        static async Task PerformSearch(string searchTerm)
        {
            AnsiConsole.MarkupLine($"[cyan]Searching for:[/] '{searchTerm}'...");

            try
            {
                // Fetch data
                var celestialBodiesMap = await GetAllCelestialBodies();

                var bodyResults = _searchService.SearchCelestialBodies(celestialBodiesMap.Values, searchTerm);

                var table = new Table().Expand();
                // table.AddColumn(new TableColumn("[yellow]Type[/]").Width(15));
                table.AddColumn(new TableColumn("[yellow]Celestial Body Name[/]"));


                bool foundResults = false;

                foreach (var result in bodyResults)
                {
                    table.AddRow(
                        // "Celestial Body",
                        Markup.Escape(result.Item.BodyName)
                    );
                    foundResults = true;
                }

                // foreach (var result in systemResults)
                // {
                //     table.AddRow(
                //         "Star System",
                //         Markup.Escape(result.Item.Name),
                //         Markup.Escape(result.MatchType),
                //         $"{result.MatchRatio}%"
                //     );
                //     foundResults = true;
                // }

                // Uncomment to display comment results
                // foreach (var result in commentResults)
                // {
                //     table.AddRow(
                //         "Comment",
                //         Markup.Escape($"On \"{result.Item.CelestialBody.BodyName}\": {result.Item.CommentText.Substring(0, Math.Min(50, result.Item.CommentText.Length))}..."),
                //         Markup.Escape(result.MatchType),
                //         $"{result.MatchRatio}%"
                //     );
                //     foundResults = true;
                // }

                if (foundResults)
                {
                    AnsiConsole.Write(table);
                }
                else
                {
                    AnsiConsole.MarkupLine($"[grey]No results found for '{Markup.Escape(searchTerm)}'.[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred during search:[/] {ex.Message}");
                // Optionally log the full exception ex.ToString()
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
            string cmd = input[..firstSpaceIndex].ToLower();

            // Extract arguments (everything after the first space)
            string args = input[(firstSpaceIndex + 1)..].Trim();

            return (cmd, args);
        }




        //==================== Commands ====================//

        static void PrintHelp()
        {
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
            grid.AddRow(new Text("ls --page-number <number>"), new Text("List celestial bodies in current location"));
            grid.AddRow(new Text("cd <name>"), new Text("Navigate to a celestial body"));
            grid.AddRow(new Text("cd 'Name with spaces'"), new Text("Navigate to a celestial body with spaces in the name"));
            grid.AddRow(new Text("cd .."), new Text("Navigate to parent celestial body"));
            grid.AddRow(new Text("cd /"), new Text("Navigate to Universe (root)"));
            grid.AddRow(new Text("go"), new Text("Interactive navigation with autocomplete"));
            grid.AddRow(new Text("tree"), new Text("Display full celestial body hierarchy"));
            grid.AddRow(new Text("tree -h"), new Text("Display hierarchy from current location"));
            grid.AddRow(new Text("warp"), new Text("Show interactive tree and warp to any celestial body"));
            grid.AddRow(new Text("search <term>"), new Text("Search for celestial bodies by name"));
            grid.AddRow(new Text("list/find"), new Text("List all celestial body types"));
            grid.AddRow(new Text("list/find -t <type>"), new Text("List all celestial bodies of a specific type (by name or ID)"));
            grid.AddRow(new Text("show/info"), new Text("Display wiki content for current celestial body"));
            grid.AddRow(new Text("show/info -n <name>"), new Text("Display wiki content for specified celestial body by name"));
            grid.AddRow(new Text("comment"), new Text("View comments for current celestial body"));
            grid.AddRow(new Text("comment \"text\""), new Text("Add a new comment to current celestial body"));
            grid.AddRow(new Text("comment --help"), new Text("Show detailed comment command options"));
            grid.AddRow(new Text("render"), new Text("Render the current celestial body"));
            grid.AddRow(new Text("revision"), new Text("Show revision history for current celestial body"));
            grid.AddRow(new Text("create-body"), new Text("Create a new celestial body"));
            grid.AddRow(new Text("update-body"), new Text("Update an existing celestial body"));
            grid.AddRow(new Text("delete-body"), new Text("Delete a celestial body"));
            grid.AddRow(new Text("pwd"), new Text("Display current location path"));
            grid.AddRow(new Text("clear/cls"), new Text("Clear the screen"));
            grid.AddRow(new Text("exit/quit"), new Text("Exit the application"));

            AnsiConsole.Write(grid);
            AnsiConsole.WriteLine();
        }

        static void HandleUnknownCommand(string cmd)
        {
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
            var argParts = args.Split([' '], 2, StringSplitOptions.RemoveEmptyEntries);

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

        static async Task HandleLsCommand(string args)
        {
            var pageNumber = 1;

            var argParts = args.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (argParts.Length < 2 || !argParts[0].Equals("--page-number", StringComparison.OrdinalIgnoreCase))
            {
                pageNumber = 1;
            }
            else
            {
                try
                {
                    pageNumber = Convert.ToInt32(argParts[1]);
                }
                catch
                {
                    TUI.Err("LIST", "Invalid page number: " + argParts[1]);
                    return;
                }
            }

            PaginatedCelestialBodiesResponse? response = await CommandLogic.ListDirectory(pageNumber);

            if (response.HasValue && !(response.Value.TotalCount == 0))
            {
                if (pageNumber > response.Value.TotalPages)
                {
                    AnsiConsole.MarkupLine("[yellow]Page number does not exist.[/]");
                    return;
                }

                var table = new Table();
                table.AddColumn("Type");
                table.AddColumn("Name");

                foreach (var child in response.Value.Items)
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
                        new Markup(name)
                    );
                }

                AnsiConsole.Write(table);
                AnsiConsole.Write($"Showing: {Math.Min((response.Value.PageSize * (response.Value.PageNumber - 1)) + 1, response.Value.TotalCount)} to {Math.Min(response.Value.PageSize * response.Value.PageNumber, response.Value.TotalCount)} of {response.Value.TotalCount}\n");
                AnsiConsole.Write($"Page: {response.Value.PageNumber} of {response.Value.TotalPages}\n");
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]No celestial bodies found in this location.[/]");
                return;
            }
        }

        static async Task HandleShowCommand(string args)
        {
            // Parse arguments to check for -n or --name flag
            var argParts = args.Split([' '], 2, StringSplitOptions.RemoveEmptyEntries);

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
            if (body == null) { TUI.Err("INFO", "No celestial body found at current location."); return; }

            var rev = await CommandLogic.GetCurrentRevision();

            if (rev == null)
            {
                TUI.Err("REV", "No content available for this celestial body.", "This celestial body might not have an active revision.");
                return;
            }

            List<Comment> comments = [];
            if (rev.CelestialBodyName != null)
            {
                comments = await CommandLogic.GetCommentsForNamedBody(rev.CelestialBodyName);
            }

            AnsiConsole.Write(TUI.WikiPage(rev, body.BodyType, comments));
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

        static async Task<IdMap<CelestialBodies>> GetAllCelestialBodies()
        {
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

            if (selectedItem.Body == null)
            {
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
                    await AddComment(JoinArgs([.. argsList.Skip(1)]));
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
                        if ((argsList[i] == "-l" || argsList[i] == "--limit") && int.TryParse(argsList[i + 1], out int bodyLimitVal))
                        {
                            bodyLimit = bodyLimitVal;
                            i++; // Skip the next arg as we've consumed it
                        }
                        else if ((argsList[i] == "-s" || argsList[i] == "--sort") && i + 1 < argsList.Count)
                        {
                            bodySortOrder = argsList[i + 1].ToLower();
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
                        if ((argsList[i] == "-l" || argsList[i] == "--limit") && int.TryParse(argsList[i + 1], out int dateLimitVal))
                        {
                            dateLimit = dateLimitVal;
                            i++; // Skip the next arg as we've consumed it
                        }
                        else if ((argsList[i] == "-s" || argsList[i] == "--sort") && i + 1 < argsList.Count)
                        {
                            dateSortOrder = argsList[i + 1].ToLower();
                            i++; // Skip the next arg as we've consumed it
                        }
                    }

                    await ViewCommentsByDateRange(startDate, endDate, dateLimit, dateSortOrder);
                    break;

                case "-r":
                case "--remove":
                    if (argsList.Count < 2 || !int.TryParse(argsList[1], out int commentId))
                    {
                        TUI.Err("COMMENT", "Invalid comment ID.", "Usage: comment -r <commentId>");
                        return;
                    }
                    await DeleteComment(commentId);
                    break;

                case "-u":
                case "--update":
                    if (argsList.Count < 2 || !int.TryParse(argsList[1], out int updateCommentId))
                    {
                        TUI.Err("COMMENT", "Invalid arguments.", "Usage: comment -u <commentId>");
                        return;
                    }
                    var comment = await CommandLogic.GetCommentById(updateCommentId);
                    (string updatedText, bool changed) = TUI.OpenExternalEditor(comment.CommentText ?? "");
                    if (!changed)
                    {
                        TUI.Err("COMMENT", "No change in the comment.");
                        return;
                    }

                    await UpdateComment(updateCommentId, updatedText);
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
            grid.AddRow(new Text("comment -r <commentId>"), new Text("Remove a comment"));
            grid.AddRow(new Text("comment -u <commentId> \"Text\""), new Text("Update a comment"));
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
            bool success = await CommandLogic.DeleteComment(commentId);

            if (success)
            {
                AnsiConsole.MarkupLine("[green]Comment deleted successfully![/]");
            }
            else
            {
                TUI.Err("COMMENT", "Failed to delete comment.");
            }
        }

        static async Task UpdateComment(int commentId, string commentText)
        {

            var updatedComment = await CommandLogic.UpdateComment(commentId, commentText);

            if (updatedComment != null)
            {
                AnsiConsole.MarkupLine("[green]Comment updated successfully![/]");
                var updatedComments = new List<Comment> { updatedComment };
                AnsiConsole.Write(TUI.CommentsPanel(updatedComments, "Updated Comment"));
            }
            else
            {
                TUI.Err("COMMENT", "Failed to update comment.");
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

        static async Task HandleEditCurrentRevision()
        {
            if (string.IsNullOrEmpty(ApiClient.JWT))
            {
                TUI.Err("AUTH", "You must be logged in to edit pages.");
                AnsiConsole.MarkupLine("[grey]Use the 'login' command to authenticate first.[/]");
                return;
            }

            var body = CommandLogic.GetCurrentBody();
            if (body == null) { TUI.Err("INFO", "No celestial body found at current location."); return; }

            var rev = await CommandLogic.GetCurrentRevision();

            if (rev == null)
            {
                TUI.Err("REV", "No content available for this celestial body.", "This celestial body might not have an active revision.");
                return;
            }

            (string newContent, bool changed) = TUI.OpenExternalEditor(rev.Content ?? "");
            if (!changed) { AnsiConsole.Markup($"[purple]Nothing was changed[/]\n\n"); return; }

            await CommandLogic.CreateRevision(body.BodyName, newContent);
            await CommandLogic.QuickRefreshBody();
            AnsiConsole.Markup($"[green]Content updated successfully[/]\n\n");
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
                    .AddColumn(new TableColumn("[bold]ID[/]").LeftAligned())
                    .AddColumn(new TableColumn("[bold]Created At[/]").LeftAligned())
                    .AddColumn(new TableColumn("[bold]Author[/]").LeftAligned())
                    .AddColumn(new TableColumn("[bold]Content[/]").LeftAligned());

                foreach (var revision in revisions.OrderByDescending(r => r.CreatedAt))
                {
                    var previewContent = revision.Content ?? "";

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

        static async Task HandleCreateBodyCommand(string args)
        {
            // Check if user is logged in
            if (string.IsNullOrEmpty(ApiClient.JWT))
            {
                TUI.Err("AUTH", "You must be logged in to create celestial bodies.");
                AnsiConsole.MarkupLine("[grey]Use the 'login' command to authenticate first.[/]");
                return;
            }

            // Interactive mode if no arguments are provided
            if (string.IsNullOrWhiteSpace(args))
            {
                await CreateBodyInteractive();
                return;
            }

            // Parse arguments (name, type, orbits)
            var argParts = SplitArgumentsRespectingQuotes(args);

            if (argParts.Count < 2)
            {
                TUI.Err("BODY", "Insufficient arguments.",
                    "Usage: create-body \"Name\" <body-type-id> [parent-id]");
                return;
            }

            string bodyName = TrimQuotes(argParts[0]);

            if (!int.TryParse(argParts[1], out int bodyTypeId))
            {
                TUI.Err("BODY", "Invalid body type ID.",
                    "Body type ID must be a number. Use 'list' command to see available types.");
                return;
            }

            int? orbitsId = null;
            if (argParts.Count >= 3 && int.TryParse(argParts[2], out int parsedOrbitsId))
            {
                orbitsId = parsedOrbitsId;
            }
            else if (argParts.Count >= 3)
            {
                TUI.Err("BODY", "Invalid parent ID.",
                    "Parent ID must be a number or omitted.");
                return;
            }
            else if (CommandLogic.GetCurrentBody() != null)
            {
                // If no parent ID is specified, use the current body as parent
                orbitsId = CommandLogic.GetCurrentBody().Id;
            }

            var newBody = await CommandLogic.CreateCelestialBody(bodyName, bodyTypeId, orbitsId);

            if (newBody != null)
            {
                AnsiConsole.MarkupLine($"[green]Successfully created celestial body:[/] [cyan]{newBody.BodyName}[/] (ID: {newBody.Id})");
            }
        }

        static async Task CreateBodyInteractive()
        {
            // Get body name
            string bodyName = AnsiConsole.Ask<string>("Enter celestial body [cyan]name[/]:");

            // List available body types for selection
            var bodyTypes = CommandLogic.GetBodyTypes();
            var bodyTypeOptions = bodyTypes.Select(t => $"{t.Id}: {t.Emoji} {t.Name}").ToArray();

            var selectedTypeOption = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the [cyan]body type[/]:")
                    .PageSize(20)
                    .AddChoices(bodyTypeOptions)
            );

            int bodyTypeId = int.Parse(selectedTypeOption.Split(':')[0]);

            // Determine parent (orbits) options
            int? orbitsId = null;

            bool useCurrentAsParent = AnsiConsole.Confirm("Use current location as parent?", true);
            if (useCurrentAsParent && CommandLogic.GetCurrentBody() != null)
            {
                orbitsId = CommandLogic.GetCurrentBody().Id;
            }
            else
            {
                // Get all bodies
                var allBodies = await ApiClient.GetCelestialBodies();

                if (allBodies.Count > 0)
                {
                    // Create options for selection
                    var bodyOptions = allBodies.Select(b => $"{b.Id}: {TUI.BodyTypeToEmoji(b.BodyType)} {b.BodyName}").ToArray();
                    bodyOptions = bodyOptions.Prepend("0: None (Root level)").ToArray();

                    var selectedBodyOption = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select the [cyan]parent body[/]:")
                            .PageSize(20)
                            .AddChoices(bodyOptions)
                    );

                    int selectedId = int.Parse(selectedBodyOption.Split(':')[0]);
                    orbitsId = selectedId > 0 ? selectedId : null;
                }
            }

            // Create the celestial body
            var newBody = await CommandLogic.CreateCelestialBody(bodyName, bodyTypeId, orbitsId);

            if (newBody != null)
            {
                AnsiConsole.MarkupLine($"[green]Successfully created celestial body:[/] [cyan]{newBody.BodyName}[/] (ID: {newBody.Id})");
            }
        }

        static async Task HandleUpdateBodyCommand(string args)
        {
            // Check if user is logged in
            if (string.IsNullOrEmpty(ApiClient.JWT))
            {
                TUI.Err("AUTH", "You must be logged in to update celestial bodies.");
                AnsiConsole.MarkupLine("[grey]Use the 'login' command to authenticate first.[/]");
                return;
            }

            // Interactive mode if no arguments are provided
            if (string.IsNullOrWhiteSpace(args))
            {
                await UpdateBodyInteractive();
                return;
            }

            // Parse arguments (id, name, type, orbits)
            var argParts = SplitArgumentsRespectingQuotes(args);

            if (argParts.Count < 3)
            {
                TUI.Err("BODY", "Insufficient arguments.",
                    "Usage: update-body <id> \"Name\" <body-type-id> [parent-id]");
                return;
            }

            if (!int.TryParse(argParts[0], out int bodyId))
            {
                TUI.Err("BODY", "Invalid body ID.",
                    "Body ID must be a number.");
                return;
            }

            string bodyName = TrimQuotes(argParts[1]);

            if (!int.TryParse(argParts[2], out int bodyTypeId))
            {
                TUI.Err("BODY", "Invalid body type ID.",
                    "Body type ID must be a number. Use 'list' command to see available types.");
                return;
            }

            int? orbitsId = null;
            if (argParts.Count >= 4 && int.TryParse(argParts[3], out int parsedOrbitsId))
            {
                orbitsId = parsedOrbitsId;
            }
            else if (argParts.Count >= 4)
            {
                TUI.Err("BODY", "Invalid parent ID.",
                    "Parent ID must be a number or omitted.");
                return;
            }

            var updatedBody = await CommandLogic.UpdateCelestialBody(bodyId, bodyName, bodyTypeId, orbitsId);

            if (updatedBody != null)
            {
                AnsiConsole.MarkupLine($"[green]Successfully updated celestial body:[/] [cyan]{updatedBody.BodyName}[/] (ID: {updatedBody.Id})");
            }
        }

        static async Task UpdateBodyInteractive()
        {
            // First, select the body to update
            var allBodies = await ApiClient.GetCelestialBodies();

            if (allBodies.Count == 0)
            {
                TUI.Err("BODY", "No celestial bodies found to update.");
                return;
            }

            // Create options for selection
            var bodyOptions = allBodies.Select(b => $"{b.Id}: {TUI.BodyTypeToEmoji(b.BodyType)} {b.BodyName}").ToArray();

            var selectedBodyOption = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the celestial body to [cyan]update[/]:")
                    .PageSize(20)
                    .AddChoices(bodyOptions)
            );

            int bodyId = int.Parse(selectedBodyOption.Split(':')[0]);
            var bodyToUpdate = allBodies.FirstOrDefault(b => b.Id == bodyId);

            if (bodyToUpdate == null)
            {
                TUI.Err("BODY", "Selected celestial body not found.");
                return;
            }

            // Get new name (use current as default)
            string bodyName = AnsiConsole.Ask("Enter new name:", bodyToUpdate.BodyName);

            // List available body types for selection
            var bodyTypes = CommandLogic.GetBodyTypes();
            var bodyTypeOptions = bodyTypes.Select(t => $"{t.Id}: {t.Emoji} {t.Name}").ToArray();

            var currentBodyType = bodyTypes.FirstOrDefault(t => t.Id == bodyToUpdate.BodyType);
            string currentBodyTypeOption = $"{currentBodyType?.Id}: {currentBodyType?.Emoji} {currentBodyType?.Name}";

            var selectedTypeOption = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the new [cyan]body type[/]:")
                    .PageSize(20)
                    .AddChoices(bodyTypeOptions)
                    .HighlightStyle(new Style(Color.Green))
            );

            int bodyTypeId = int.Parse(selectedTypeOption.Split(':')[0]);

            // Determine parent (orbits) options
            int? orbitsId = bodyToUpdate.Orbits?.Id;

            bool changeParent = AnsiConsole.Confirm("Change parent body?", false);
            if (changeParent)
            {
                // Get all potential parent bodies (excluding self and children)
                var potentialParents = allBodies.Where(b => b.Id != bodyId).ToList();

                // Create options for selection
                var parentOptions = potentialParents.Select(b => $"{b.Id}: {TUI.BodyTypeToEmoji(b.BodyType)} {b.BodyName}").ToArray();
                parentOptions = parentOptions.Prepend("0: None (Root level)").ToArray();

                var selectedParentOption = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select the new [cyan]parent body[/]:")
                        .PageSize(20)
                        .AddChoices(parentOptions)
                );

                int selectedId = int.Parse(selectedParentOption.Split(':')[0]);
                orbitsId = selectedId > 0 ? selectedId : null;
            }

            // Update the celestial body
            var updatedBody = await CommandLogic.UpdateCelestialBody(bodyId, bodyName, bodyTypeId, orbitsId);

            if (updatedBody != null)
            {
                AnsiConsole.MarkupLine($"[green]Successfully updated celestial body:[/] [cyan]{updatedBody.BodyName}[/] (ID: {updatedBody.Id})");
            }
        }

        static async Task HandleDeleteBodyCommand(string args)
        {
            // Check if user is logged in
            if (string.IsNullOrEmpty(ApiClient.JWT))
            {
                TUI.Err("AUTH", "You must be logged in to delete celestial bodies.");
                AnsiConsole.MarkupLine("[grey]Use the 'login' command to authenticate first.[/]");
                return;
            }

            // Interactive mode if no arguments are provided
            if (string.IsNullOrWhiteSpace(args))
            {
                await DeleteBodyInteractive();
                return;
            }

            // Parse arguments (id)
            if (!int.TryParse(args.Trim(), out int bodyId))
            {
                TUI.Err("BODY", "Invalid body ID.",
                    "Usage: delete-body <id>");
                return;
            }

            // Confirm deletion
            bool confirmed = AnsiConsole.Confirm($"Are you sure you want to delete celestial body with ID {bodyId}?", false);
            if (!confirmed)
            {
                AnsiConsole.MarkupLine("[grey]Deletion cancelled.[/]");
                return;
            }

            bool success = await CommandLogic.DeleteCelestialBody(bodyId);

            if (success)
            {
                AnsiConsole.MarkupLine($"[green]Successfully deleted celestial body with ID {bodyId}[/]");
            }
        }

        static async Task DeleteBodyInteractive()
        {
            // First, select the body to delete
            var allBodies = await ApiClient.GetCelestialBodies();

            if (allBodies.Count == 0)
            {
                TUI.Err("BODY", "No celestial bodies found to delete.");
                return;
            }

            // Create options for selection
            var bodyOptions = allBodies.Select(b => $"{b.Id}: {TUI.BodyTypeToEmoji(b.BodyType)} {b.BodyName}").ToArray();

            var selectedBodyOption = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the celestial body to [red]delete[/]:")
                    .PageSize(20)
                    .AddChoices(bodyOptions)
            );

            int bodyId = int.Parse(selectedBodyOption.Split(':')[0]);
            var bodyToDelete = allBodies.FirstOrDefault(b => b.Id == bodyId);

            if (bodyToDelete == null)
            {
                TUI.Err("BODY", "Selected celestial body not found.");
                return;
            }

            // Check if this is the current body
            var currentBody = CommandLogic.GetCurrentBody();
            if (currentBody != null && currentBody.Id == bodyId)
            {
                TUI.Err("BODY", "Cannot delete the celestial body you're currently in.",
                    "Navigate to the parent body first using 'cd ..'");
                return;
            }

            // Confirm deletion with more details
            bool confirmed = AnsiConsole.Confirm(
                $"Are you sure you want to delete [bold red]{bodyToDelete.BodyName}[/] (ID: {bodyId})?", false);

            if (!confirmed)
            {
                AnsiConsole.MarkupLine("[grey]Deletion cancelled.[/]");
                return;
            }

            bool success = await CommandLogic.DeleteCelestialBody(bodyId);

            if (success)
            {
                AnsiConsole.MarkupLine($"[green]Successfully deleted celestial body:[/] [grey]{bodyToDelete.BodyName}[/] (ID: {bodyId})");
            }
        }

        // Helper to trim quotes
        private static string TrimQuotes(string input)
        {
            return CommandLogic.TrimQuotes(input);
        }

        static async Task LaunchChatbot()
        {
            // Get current path and body before starting chat
            string currentPath = CommandLogic.GetCurrentPath();
            var currentBody = CommandLogic.GetCurrentBody();
            string currentContext = currentBody?.BodyName ?? "the Universe";
            string apiUrl = Environment.GetEnvironmentVariable("API_URL")!;

            var header = new Rule($"[cyan] Galaxy Bot :robot: :sparkles:[/] [dim]at {currentPath}[/]");
            AnsiConsole.Write(header);

            // Initialize chat
            var chat = new List<ChatMessage>();
            string systemMessage = $"You are a helpful assistant with expertise in astronomy and space science. The user is currently exploring {currentContext}.";
            string initialMessage = $"Hey there! What would you like to know about {currentContext}?";
            chat.Add(new ChatMessage { Role = "assistant", Content = initialMessage });

            AnsiConsole.MarkupLine($"[green]Bot:[/] {initialMessage}");

            // Setup HTTP client
            using var httpClient = new HttpClient();

            bool chatMode = true;
            while (chatMode)
            {
                var msg = AnsiConsole.Ask<string>("[lightcyan1]Enter a message[/] [orange1]❯❯[/]");

                if (msg.ToLower() == "quit" || msg.ToLower() == "exit") { chatMode = false; }
                else
                {
                    // Add user message to history
                    chat.Add(new ChatMessage { Role = "user", Content = msg });

                    await AnsiConsole.Status()
                    .StartAsync("[yellow]Thinking...[/]", async ctx =>
                    {
                        try
                        {
                            // Create request for the chat API
                            var chatRequest = new ChatRequest
                            {
                                Messages = chat,
                                System = systemMessage,
                                MaxTokens = 1024
                            };

                            // Send request to our API endpoint
                            var response = await httpClient.PostAsJsonAsync($"{apiUrl}/api/chat", chatRequest);

                            if (response.IsSuccessStatusCode)
                            {
                                var chatResponse = await response.Content.ReadFromJsonAsync<ChatResponse>();
                                if (chatResponse != null)
                                {
                                    // Display the response
                                    AnsiConsole.MarkupLine($"[green]Bot:[/] {chatResponse.Message}");

                                    // Add assistant's response to history
                                    chat.Add(new ChatMessage { Role = "assistant", Content = chatResponse.Message });
                                }
                            }
                            else
                            {
                                var errorContent = await response.Content.ReadAsStringAsync();
                                AnsiConsole.MarkupLine($"[red]Error:[/] {response.StatusCode} - {errorContent}");
                            }
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
                        }
                    });
                }
            }
        }
    }
}