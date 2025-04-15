using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using dotenv.net;
using GalaxyWiki.Core.Entities;
using GalaxyWiki.Core.Services;
using GalaxyWiki.Core.DTOs;

namespace GalaxyWiki.Cli
{
    public static class Program
    {
        private static readonly SearchService _searchService = new SearchService();

        public static async Task<int> Main(string[] args)
        {
            //==================== Main command loop ====================//

            // AnsiConsole.Live(layout);
            DotEnv.Load();
            TUI.ShowBanner();

            int? workDir = null;
            bool running = true;
            while(running) {
                var inp = AnsiConsole.Ask<string>("[lightcyan1]Enter a command[/] [springgreen3_1]❯❯[/]");
                var parts = inp.Trim().Split(' ', 2);
                var cmd = parts[0].ToLower();
                var searchTerm = parts.Length > 1 ? parts[1] : string.Empty;

                switch(cmd) {
                    case "quit":
                    case "exit": running = false; break;

                    case "help": PrintHelp(); break;

                    case "clear":
                    case "cls": AnsiConsole.Clear(); break;

                    case "comment": AnsiConsole.WriteLine($"TODO: Comment\n{searchTerm}"); break;

                    case "tree": await DisplayInteractiveUniverseTree(); break;

                    case "cal": AnsiConsole.Write(TUI.Calendar()); break;

                    case "search": 
                        if (string.IsNullOrWhiteSpace(searchTerm))
                        {
                            AnsiConsole.MarkupLine("[yellow]Please provide a search term after 'search'.[/]");
                        }
                        else
                        {
                           await PerformSearch(searchTerm);
                        }
                        break;

                    case "pwd": AnsiConsole.Write(TUI.Path("Example > Path > Here")); break; // TODO: Get path

                    case "cd": AnsiConsole.Write("TODO: If no argument provided, open path radio button selector"); break;

                    case "show": AnsiConsole.Write("TODO: Show wiki page content"); break;

                    case "render": AnsiConsole.Write(TUI.Image("../../assets/earth.png")); break;

                    case "chat": await LaunchChatbot(); break;

                    case "login": await Login(); break;
                }
            }
            
            return 0;
        }


        //==================== Commands ====================//

        static void PrintHelp() { AnsiConsole.WriteLine("HELP MENU"); }

        static async Task<IdMap<CelestialBodies>> GetAllCelestialBodies() {
            try { return await ApiClient.GetCelestialBodiesMap(); }
            catch (Exception ex) { AnsiConsole.Markup("[red]An error occurred[/]:\n" + ex.Message + "\n"); }

            return new IdMap<CelestialBodies>();
        }

        static async Task DisplayInteractiveUniverseTree()
        {
            IdMap<CelestialBodies> bodies = await GetAllCelestialBodies();
            
            // Create an empty list of selectable items
            var items = new List<(string DisplayLabel, CelestialBodies Body)>();
            
            // Find the root (Universe) node
            var root = bodies.Values.FirstOrDefault(b => b.Orbits == null);
            if (root == null) { AnsiConsole.WriteLine("Could not find root celestial body."); return; }
            
            // Build a list of selectable items with proper indentation
            string? selection = TUI.CelestialTreeSelectable(bodies, root.Id, items);
            
            // Find the selected body
            var selectedItem = items.FirstOrDefault(i => i.DisplayLabel == selection);

            if (selectedItem.Body == null || !selectedItem.Body.ActiveRevision.HasValue) {
                AnsiConsole.WriteLine("No active revision found for this celestial body."); 
                return;
            }

            await ShowRevisionContent(selectedItem.Body.ActiveRevision.Value);
        }
        
        static async Task ShowRevisionContent(int revisionId)
        {
            Revision? rev;
            try { rev = await ApiClient.GetRevisionAsync($"http://localhost:5216/api/revision/{revisionId}"); }
            catch (Exception ex) { AnsiConsole.MarkupLine($"[red]Error retrieving revision:[/] {ex.Message}"); return; }

            if (rev == null) { AnsiConsole.WriteLine($"Could not retrieve revision #{revisionId}"); return; }

            // Display
            AnsiConsole.Write(TUI.Article(rev.CelestialBodyName ?? "Unknown", null, rev.Content ?? "No content available"));
            AnsiConsole.Write(TUI.AuthorInfo(rev.AuthorDisplayName ?? "Unknown", rev.CreatedAt));
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

        static async Task Login() {
            Console.Write(new Rule("[orange]Obtaining[/] JWT"));
            await GoogleAuthenticator.GetIdTokenAsync();

            Console.Write(new Rule("[green]JWT Obtained[/]"));
            Console.WriteLine(GoogleAuthenticator.JWT);

            Console.Write(new Rule("[cyan]Logging in[/] with API"));
            await ApiClient.LoginAsync(GoogleAuthenticator.JWT);
        }

        static async Task PerformSearch(string searchTerm)
        {
            AnsiConsole.MarkupLine($"[cyan]Searching for:[/] '{searchTerm}'...");

            try
            {
                // Fetch data
                var celestialBodiesMap = await GetAllCelestialBodies();
                var starSystems = await ApiClient.GetAllStarSystems(); // Assuming this works or handles errors
                // var commentDtos = await ApiClient.GetAllCommentDtos(); // Uncomment if you want to search comments too

                var bodyResults = _searchService.SearchCelestialBodies(celestialBodiesMap.Values, searchTerm);
                var systemResults = _searchService.SearchStarSystems(starSystems, searchTerm);
                // var commentResults = _searchService.SearchCommentDtos(commentDtos, searchTerm); // Uncomment for comments

                var table = new Table().Expand();
                table.AddColumn(new TableColumn("[yellow]Type[/]").Width(15));
                table.AddColumn(new TableColumn("[yellow]Name[/]"));
                table.AddColumn(new TableColumn("[yellow]Match Type[/]").Width(15));
                table.AddColumn(new TableColumn("[yellow]Match %[/]").Width(10));

                bool foundResults = false;

                foreach (var result in bodyResults)
                {
                    table.AddRow(
                        "Celestial Body",
                        Markup.Escape(result.Item.BodyName),
                        Markup.Escape(result.MatchType),
                        $"{result.MatchRatio}%"
                    );
                    foundResults = true;
                }

                foreach (var result in systemResults)
                {
                    table.AddRow(
                        "Star System",
                        Markup.Escape(result.Item.Name),
                        Markup.Escape(result.MatchType),
                        $"{result.MatchRatio}%"
                    );
                    foundResults = true;
                }
                
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
    }
}