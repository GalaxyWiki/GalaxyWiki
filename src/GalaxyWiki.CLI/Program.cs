using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using dotenv.net;
using GalaxyWiki.Core.Entities;

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

            int? workDir = null;
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

                    case "cal": AnsiConsole.Write(TUI.Calendar()); break;

                    case "search": AnsiConsole.WriteLine("TODO: Search wiki pages"); break;

                    case "pwd": AnsiConsole.Write(TUI.Path("Example > Path > Here")); break; // TODO: Get path

                    case "cd": AnsiConsole.Write("TODO: If no argument provided, open path radio button selector"); break;

                    case "show": AnsiConsole.Write("TODO: Show wiki page content"); break;

                    case "render": AnsiConsole.Write(TUI.Image("../../assets/earth.png")); break;

                    case "chat": LaunchChatbot(); break;

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
            AnsiConsole.Write(TUI.Article(rev.CelestialBodyName ?? "Unknown", rev.Content));
            AnsiConsole.Write(TUI.AuthorInfo(rev.AuthorDisplayName ?? "Unknown", rev.CreatedAt));
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