using System.Data;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using dotenv.net;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

using System.Net.Http;
using System.Threading.Tasks;
using GalaxyWiki.Core.Entities;
using FluentNHibernate.Testing.Values;

namespace GalaxyWiki.Cli
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            //==================== Layout definition ====================//
            // REMOVED: Old approach

            // Layout layout = new Layout("Root")
            //     .SplitColumns(
            //         new Layout("Left"),
            //         new Layout("Middle").SplitRows( new Layout("Header"),   new Layout("Terminal")                          ),
            //         new Layout("Right").SplitRows(  new Layout("Top"),      new Layout("Mid"),      new Layout("Bottom")    )
            //     );

            // //---------- Add prompt ----------//
            // var command = AnsiConsole.Prompt(
            //     new SelectionPrompt<string>()
            //         .Title("Choose your command:")
            //         .PageSize(10)
            //         .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
            //         .AddChoices([ "info", "comment", "cd" ])
            // );



            //==================== Main command loop ====================//

            // AnsiConsole.Live(layout);
            DotEnv.Load();
            ShowBanner();

            bool running = true;
            while(running) {
                var inp = AnsiConsole.Ask<string>("[lightcyan1]Enter a command[/] [springgreen3_1]❯❯[/]");
                var parts = inp.Trim().Split();
                var cmd = parts[0].ToLower();
                var dat = string.Join(" ", parts[1..]);

                switch(cmd) {
                    case "quit":
                    case "exit": running = false; break;

                    case "help": PrintHelp(); break;

                    case "clear":
                    case "cls": AnsiConsole.Clear(); break;

                    case "comment": AnsiConsole.WriteLine($"TODO: Comment\n{dat}"); break;

                    case "tree": await DisplayInteractiveUniverseTree(); break;

                    case "cal": AnsiConsole.Write(GetCalendar()); break;

                    case "search": AnsiConsole.WriteLine("TODO: Search wiki pages"); break;

                    case "pwd": AnsiConsole.Write(GetCurrentWorkingDirectoryPath()); break;

                    case "cd": AnsiConsole.Write("TODO: If no argument provided, open path radio button selector"); break;

                    case "show": AnsiConsole.Write("TODO: Show wiki page content"); break;

                    case "render": AnsiConsole.Write(GetRenderedCelestialBody()); break;

                    case "chat": LaunchChatbot(); break;

                    case "login": await Login(); break;
                }
            }
            
            return 0;
        }


        //==================== Commands ====================//

        static void PrintHelp() { AnsiConsole.WriteLine("HELP MENU"); }

        static void ShowBanner() {
            var banner = new Panel(
                Align.Left(
                    new FigletText(FigletFont.Load("../../assets/starwars.flf"), "Galaxy Wiki")
                    .Centered()
                    .Color(Color.Aqua),
                    VerticalAlignment.Bottom
                )
            )
            .NoBorder()
            .Expand();

            AnsiConsole.WriteLine("\n     Welcome to\n");
            AnsiConsole.Write(banner);
        }

        static IRenderable GetCurrentWorkingDirectoryPath() {
            var path = new TextPath("Universe > Sagittarius A* > Sun");

            path.RootStyle = new Style(foreground: Color.Red);
            path.SeparatorStyle = new Style(foreground: Color.Green);
            path.StemStyle = new Style(foreground: Color.Blue);
            path.LeafStyle = new Style(foreground: Color.Yellow);
            return path;
        }

        static Panel GetRenderedCelestialBody() {
            // Get image
            CanvasImage image = new CanvasImage("../../assets/earth.png").MaxWidth(12);
            
            return new Panel(Align.Center(image, VerticalAlignment.Middle))
            .RoundedBorder()
            .Header("[cyan] Image :camera: [/]")
            .Expand();
        }

        static Panel GetCalendar() {
            return new Panel(
                Align.Center(
                    new Calendar(DateTime.Today)
                        .AddCalendarEvent(DateTime.Today)
                        .HeaderStyle(Style.Parse("bold"))
                        .HighlightStyle(Style.Parse("yellow bold"))
                        .RoundedBorder(),
                    VerticalAlignment.Middle
                )
            )
            .RoundedBorder()
            .Header("[cyan] Calendar :calendar: [/]")
            .Expand();
        }

        static async Task<List<CelestialBodies>> GetAllCelestialBodies() {
            string apiUrl = "http://localhost:5216/api/celestial-body";

            try
            {
                return await ApiClient.GetCelestialBodiesAsync(apiUrl);
            }
            catch (Exception ex) { AnsiConsole.WriteLine("[red]An error occurred[/]: " + ex.Message); }

            return new List<CelestialBodies>();
        }

        static async Task DisplayInteractiveUniverseTree()
        {
            // Get all celestial bodies
            List<CelestialBodies> bodies = await GetAllCelestialBodies();
            
            // Create an empty list of selectable items
            var items = new List<(string DisplayLabel, CelestialBodies Body)>();
            
            // Find the root (Universe) node
            var root = bodies.FirstOrDefault(b => b.Orbits == null);
            if (root == null)
            {
                AnsiConsole.WriteLine("Could not find root celestial body.");
                return;
            }
            
            // Build a list of selectable items with proper indentation
            BuildSelectableItems(bodies, root.Id, items, 0);
            
            if (items.Count == 0)
            {
                AnsiConsole.WriteLine("No celestial bodies found.");
                return;
            }
            
            // Display the selection prompt
            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a celestial body to view details:")
                    .PageSize(30)
                    .HighlightStyle(new Style(Color.SpringGreen3_1, Color.Black, Decoration.Underline))
                    .AddChoices(items.Select(i => i.DisplayLabel))
            );
            
            // Find the selected body
            var selectedItem = items.FirstOrDefault(i => i.DisplayLabel == selection);
            if (selectedItem.Body != null && selectedItem.Body.ActiveRevision.HasValue)
            {
                await ShowRevisionContent(selectedItem.Body.ActiveRevision.Value);
            }
            else
            {
                AnsiConsole.WriteLine("No active revision found for this celestial body.");
            }
        }
        
        static void BuildSelectableItems(
            List<CelestialBodies> allBodies, 
            int bodyId, 
            List<(string DisplayLabel, CelestialBodies Body)> items, 
            int level)
        {
            // Create lookup for better performance
            var bodiesById = allBodies.ToDictionary(b => b.Id);
            
            // Get the body by ID
            if (!bodiesById.TryGetValue(bodyId, out var body))
            {
                return;
            }
            
            // Get emoji based on body type
            string emoji = GetCelestialBodyEmoji(body.BodyType);
            
            // Create indentation
            string indent = new string(' ', level * 2) + (level > 0 ? "└─ " : "");
            
            // Create the display label
            string displayLabel = $"{indent}{emoji} ({body.Id}) {body.BodyName}";
            
            // Add to the list of items
            items.Add((displayLabel, body));
            
            // Find all children that orbit this body
            var children = allBodies.Where(b => b.Orbits != null && b.Orbits.Id == body.Id).ToList();
            
            // Recursively add children
            foreach (var child in children)
            {
                BuildSelectableItems(allBodies, child.Id, items, level + 1);
            }
        }
        
        static async Task ShowRevisionContent(int revisionId)
        {
            try
            {
                var revision = await ApiClient.GetRevisionAsync($"http://localhost:5216/api/revision/{revisionId}");
                if (revision != null)
                {
                    // Create a rich panel for the content
                    var panel = new Panel(
                        Align.Left(
                            new Markup(revision.Content ?? "No content available")
                        )
                    )
                    .BorderColor(Color.SpringGreen3_1)
                    .RoundedBorder()
                    .Header($"[bold cyan] {revision.CelestialBodyName} [/]")
                    .HeaderAlignment(Justify.Center);
                    
                    AnsiConsole.Write(panel);
                    
                    // Add author information in a separate footer panel
                    var formattedDate = revision.CreatedAt.ToString("MMMM d, yyyy 'at' h:mm tt");
                    AnsiConsole.Write(
                        new Panel(
                            Align.Right(
                                new Markup($"[italic grey]Written by [/][bold]{revision.AuthorDisplayName}[/] [italic grey]on {formattedDate}[/]")
                            )
                        )
                        .NoBorder()
                        .Padding(0, 0, 0, 1)
                    );
                }
                else
                {
                    AnsiConsole.WriteLine($"Could not retrieve revision #{revisionId}");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error retrieving revision:[/] {ex.Message}");
            }
        }

        static async Task<IRenderable> GetUniverseTree() {
            // Get all celestial bodies from API
            List<CelestialBodies> bodies = await GetAllCelestialBodies();
            
            // Create root tree with an empty label to hide the initial "Universe" text
            var universe = new Tree("");
            
            // Find the root (Universe) node
            var root = bodies.FirstOrDefault(b => b.Orbits == null);
            if (root == null) {
                // Fallback if we don't find a proper root
                return universe;
            }
            
            // Build tree recursively starting from root
            Dictionary<int, TreeNode> nodeMap = new Dictionary<int, TreeNode>();
            BuildCelestialBodyTree(bodies, universe, root.Id, nodeMap);
            
            return universe;
        }

        static void BuildCelestialBodyTree(List<CelestialBodies> bodies, Tree rootTree, int rootId, Dictionary<int, TreeNode> nodeMap) {
            // Create a lookup dictionary for better performance
            var bodiesById = bodies.ToDictionary(b => b.Id);
            
            // Recursively add nodes to the tree
            AddCelestialBodyToTree(bodies, bodiesById, rootTree, rootId, nodeMap);
        }
        
        static void AddCelestialBodyToTree(
            List<CelestialBodies> allBodies, 
            Dictionary<int, CelestialBodies> bodiesById,
            object parentNode, 
            int bodyId, 
            Dictionary<int, TreeNode> nodeMap) {
            
            // Get the body by ID
            if (!bodiesById.TryGetValue(bodyId, out var body)) {
                return;
            }
            
            // Get emoji based on body type
            string emoji = GetCelestialBodyEmoji(body.BodyType);
            
            // Create node for this body with emoji
            string nodeLabel = $"{emoji} ({body.Id}) {body.BodyName}";
            TreeNode node;
            
            if (parentNode is Tree tree) {
                node = tree.AddNode(nodeLabel);
            } 
            else if (parentNode is TreeNode treeNode) {
                node = treeNode.AddNode(nodeLabel);
            }
            else {
                return; // Unsupported parent type
            }
            
            nodeMap[body.Id] = node;
            
            // Find all children that orbit this body
            var children = allBodies.Where(b => b.Orbits != null && b.Orbits.Id == body.Id).ToList();
            
            // Recursively add children
            foreach (var child in children) {
                AddCelestialBodyToTree(allBodies, bodiesById, node, child.Id, nodeMap);
            }
        }
        
        static string GetCelestialBodyEmoji(int bodyType) {
            // Map body types to emojis based on the ID
            return bodyType switch {
                1 => "🌌", // Galaxy
                2 => "⭐", // Star
                3 => "🪐", // Planet
                4 => "🌙", // Moon
                5 => "🛰️", // Satellite
                6 => "⚫", // Black Hole
                7 => "🧊", // Dwarf Planet
                8 => "☄️", // Asteroid
                9 => "☄️", // Comet
                10 => "☁️", // Nebula
                11 => "🌠", // Universe
                _ => "🔭"  // Default for unknown types
            };
        }

        static void LaunchChatbot() {
            var header = new Spectre.Console.Rule("[cyan] Galaxy Bot :robot: :sparkles: [/]");
            AnsiConsole.Write(header);

            bool chatMode = true;
            while(chatMode) {
                var msg = AnsiConsole.Ask<string>("[lightcyan1]Enter a message[/] [orange1]❯❯[/]");
                if (msg.ToLower() == "quit" || msg.ToLower() == "exit") { chatMode = false; }
                else { AnsiConsole.WriteLine("TODO: Bot response"); }
            }
        }

        static async Task Login() {
            Console.WriteLine("Obtaining JWT");
            await GoogleAuthenticator.GetIdTokenAsync();

            Console.WriteLine("JWT Obtained.");
            Console.WriteLine(GoogleAuthenticator.JWT);

            Console.WriteLine("Logging in with API");
            await ApiClient.LoginAsync(GoogleAuthenticator.JWT);
        }
    }
}

class Asdf : ICommand
{
    public Task<int> Execute(CommandContext context, CommandSettings settings) { throw new NotImplementedException(); }

    public ValidationResult Validate(CommandContext context, CommandSettings settings) { throw new NotImplementedException(); }
}
