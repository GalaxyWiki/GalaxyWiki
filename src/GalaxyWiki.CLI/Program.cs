using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using dotenv.net;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.CLI;
using System.Text.RegularExpressions;

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

                    case "comment": AnsiConsole.WriteLine($"TODO: Comment\n{dat}"); break;

                    case "tree": await HandleTreeCommand(dat); break;

                    case "warp": await CommandLogic.WarpToSelectedBody(); break;

                    case "cal": AnsiConsole.Write(TUI.Calendar()); break;

                    case "search": AnsiConsole.WriteLine("TODO: Search wiki pages"); break;

                    case "pwd": AnsiConsole.Write(TUI.Path(CommandLogic.GetCurrentPath())); break;

                    case "cd": await HandleCdCommand(dat); break;

                    case "ls": await HandleLsCommand(); break;

                    case "show":
                    case "info": await HandleShowCommand(); break;

                    case "render": AnsiConsole.Write(TUI.Image("../../assets/earth.png")); break;

                    case "chat": LaunchChatbot(); break;

                    case "login": await Login(); break;
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
            grid.AddRow(new Text("cd [name]"), new Text("Navigate to a celestial body"));
            grid.AddRow(new Text("cd 'Name with spaces'"), new Text("Navigate to a celestial body with spaces in the name"));
            grid.AddRow(new Text("cd .."), new Text("Navigate to parent celestial body"));
            grid.AddRow(new Text("cd /"), new Text("Navigate to Universe (root)"));
            grid.AddRow(new Text("tree"), new Text("Display full celestial body hierarchy"));
            grid.AddRow(new Text("tree -h"), new Text("Display hierarchy from current location"));
            grid.AddRow(new Text("warp"), new Text("Show interactive tree and warp to any celestial body"));
            grid.AddRow(new Text("show/info"), new Text("Display wiki content for current celestial body"));
            grid.AddRow(new Text("pwd"), new Text("Display current location path"));
            grid.AddRow(new Text("clear/cls"), new Text("Clear the screen"));
            grid.AddRow(new Text("exit/quit"), new Text("Exit the application"));
            
            AnsiConsole.Write(grid);
            AnsiConsole.WriteLine();
        }

        static async Task HandleCdCommand(string target)
        {
            await CommandLogic.ChangeDirectory(target);
        }

        static async Task HandleTreeCommand(string args)
        {
            // Check for the "-h" or "--here" flag
            bool useCurrentAsRoot = args.Trim().Equals("-h", StringComparison.OrdinalIgnoreCase) || 
                                   args.Trim().Equals("--here", StringComparison.OrdinalIgnoreCase);
            
            await CommandLogic.DisplayTree(useCurrentAsRoot);
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
                
                table.AddRow(emoji, name, id);
            }
            
            AnsiConsole.Write(table);
        }

        static async Task HandleShowCommand()
        {
            var revision = await CommandLogic.GetCurrentRevision();
            
            if (revision == null)
            {
                TUI.Err("INFO", "No content available for this celestial body.", 
                    "This celestial body might not have an active revision.");
                return;
            }
            
            AnsiConsole.Write(TUI.Article(revision.CelestialBodyName ?? "Unknown", revision.Content));
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
                var msg = AnsiConsole.Ask<string>("[lightcyan1]Enter a message[/] [orange1]❯❯[/]");
                if (msg.ToLower() == "quit" || msg.ToLower() == "exit") { chatMode = false; }
                else { AnsiConsole.WriteLine("TODO: Bot response"); }
            }
        }

        static async Task Login() {
            Console.Write(new Rule("[orange]Obtaining[/] JWT"));
            await GoogleAuthenticator.GetIdTokenAsync();

            Console.Write(new Rule("[green]JWT Obtained[/]"));
            Console.WriteLine(GoogleAuthenticator.JWT);

            Console.Write(new Rule("[cyan]Logging in[/] with API"));
            await ApiClient.LoginAsync(GoogleAuthenticator.JWT);
        }
    }
}