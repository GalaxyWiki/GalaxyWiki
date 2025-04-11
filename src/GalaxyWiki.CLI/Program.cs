using System.Data;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
using dotenv.net;
using System.Threading.Tasks;

namespace GalaxyWiki.Cli
{
    public static class Program
    {
        public static async Task Main(string[] args)
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

                    case "tree": AnsiConsole.Write(GetUniverseTree()); break;

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

        static IRenderable GetUniverseTree() {
            // TODO: Populate via API fetch
            var universe = new Tree("Universe");

            var sagA = universe.AddNode("Sagittarius A*");

            var sun = sagA.AddNode("Sun");                  sun.AddNodes("Mercury", "Venus");
            var earth = sun.AddNode("Earth");               earth.AddNodes("Moon", "ISS");
            var mars = sun.AddNode("Mars");                 mars.AddNodes("Phobos", "Deimos");
            var jupiter = sun.AddNode("Jupiter");           jupiter.AddNodes("Io", "Europa", "Ganymede", "Callisto", "Amalthea", "Thebe", "Himalia", "Elara", "Lysithea");
            var saturn = sun.AddNode("Saturn");             saturn.AddNodes("[red]Titan[/]", "Enceladus", "Iapetus", "Rhea", "Dione", "Tethys", "Mimas", "Hyperion", "Phoebe", "Janus", "Epimetheus");
            var uranus = sun.AddNode("Uranus");             uranus.AddNodes("Miranda", "Ariel", "Umbriel", "Titania", "Oberon");
            var neptune = sun.AddNode("Neptune");           neptune.AddNodes("Triton", "Nereid", "Proteus", "Larissa");
            var pluto = sun.AddNode("Pluto");               pluto.AddNodes("Charon", "Hydra", "Nix", "Kerberos", "Styx");            

            return universe;
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