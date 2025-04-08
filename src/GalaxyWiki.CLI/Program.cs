using Spectre.Console;

namespace GalaxyWiki.Cli
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Layout layout = new Layout("Root")
                .SplitColumns(
                    new Layout("Left"),
                    new Layout("Middle")
                        .SplitRows(
                            new Layout("Header"),
                            new Layout("Terminal")
                        ),
                    new Layout("Right")
                        .SplitRows(
                            new Layout("Top"),
                            new Layout("Mid"),
                            new Layout("Bottom")
                        )
                );

            // Left panel
            // Universe Tree

            var universe = new Tree("Universe");

            var sagA = universe.AddNode("Sagittarius A*");

            var sun = sagA.AddNode("Sun");
            sun.AddNode("Mercury");
            sun.AddNode("Venus");

            var earth = sun.AddNode("Earth");
            earth.AddNode("Moon");
            earth.AddNode("ISS");

            var mars = sun.AddNode("Mars");
            mars.AddNode("Phobos");
            mars.AddNode("Deimos");

            var jupiter = sun.AddNode("Jupiter");
            jupiter.AddNode("Io");
            jupiter.AddNode("Europa");
            jupiter.AddNode("Ganymede");
            jupiter.AddNode("Callisto");
            jupiter.AddNode("Amalthea");
            jupiter.AddNode("Thebe");
            jupiter.AddNode("Himalia");
            jupiter.AddNode("Elara");
            jupiter.AddNode("Lysithea");

            var saturn = sun.AddNode("Saturn");
            saturn.AddNode("Titan");
            saturn.AddNode("Enceladus");
            saturn.AddNode("Iapetus");
            saturn.AddNode("Rhea");
            saturn.AddNode("Dione");
            saturn.AddNode("Tethys");
            saturn.AddNode("Mimas");
            saturn.AddNode("Hyperion");
            saturn.AddNode("Phoebe");
            saturn.AddNode("Janus");
            saturn.AddNode("Epimetheus");

            var uranus = sun.AddNode("Uranus");
            uranus.AddNode("Miranda");
            uranus.AddNode("Ariel");
            uranus.AddNode("Umbriel");
            uranus.AddNode("Titania");
            uranus.AddNode("Oberon");

            var neptune = sun.AddNode("Neptune");
            neptune.AddNode("Triton");
            neptune.AddNode("Nereid");
            neptune.AddNode("Proteus");
            neptune.AddNode("Larissa");

            var pluto = sun.AddNode("Pluto");
            pluto.AddNode("Charon");
            pluto.AddNode("Hydra");
            pluto.AddNode("Nix");
            pluto.AddNode("Kerberos");
            pluto.AddNode("Styx");            

            layout["Left"].Update(
                new Panel(
                    universe
                )
                .RoundedBorder()
                .Header("[cyan] Universe Tree :deciduous_tree: [/]")
                .Expand()
            );

            CanvasImage image = new CanvasImage("../../assets/earth.png").MaxWidth(12);
            
            layout["Right"]["Mid"].Update(
                new Panel(
                    Align.Center(
                        image,
                        VerticalAlignment.Middle
                    )
                )
                .RoundedBorder()
                .Header("[cyan] Image :camera: [/]")
                .Expand()
            );

            var command = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose your command:")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                    .AddChoices([
                        "info",
                        "comment",
                        "cd"
                    ])
            );

            layout["Middle"]["Terminal"].Update(
                new Panel(
                    command
                )
                .RoundedBorder()
                .Header("[cyan] Terminal [/]")
                .Expand()
            )
            .Ratio(2);

            layout["Middle"].Ratio(2);

            layout["Middle"]["Header"].Update(
                new Panel(
                    Align.Left(
                        new FigletText(
                            FigletFont.Load("../../assets/starwars.flf"),
                            "Galaxy Wiki"
                        )
                        .Centered()
                        .Color(Color.Aqua),
                        VerticalAlignment.Bottom
                    )
                )
                .NoBorder()
                .Expand()
            );

            layout["Right"]["Bottom"].Update(
                new Panel(
                    Align.Center(
                        new Markup("This is where the chatbot will be"),
                        VerticalAlignment.Middle
                    )
                )
                .RoundedBorder()
                .Header("[cyan] Galaxy Bot :robot::sparkles: [/]")
                .Expand()
            );

            layout["Right"]["Top"].Update(
                new Panel(
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
                .Expand()
            );

            AnsiConsole.Write(layout);
        }
    }
}