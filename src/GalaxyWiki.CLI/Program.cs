using Spectre.Console;

namespace GalaxyWiki.Cli
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //==================== Layout definition ====================//
            Layout layout = new Layout("Root")
                .SplitColumns(
                    new Layout("Left"),
                    new Layout("Middle").SplitRows( new Layout("Header"),   new Layout("Terminal")                          ),
                    new Layout("Right").SplitRows(  new Layout("Top"),      new Layout("Mid"),      new Layout("Bottom")    )
                );


            //==================== Left panel ====================//

            //---------- Construct universe tree ----------//
            // TODO: Populate via API fetch
            var universe = new Tree("Universe");

            var sagA = universe.AddNode("Sagittarius A*");

            var sun = sagA.AddNode("Sun");                  sun.AddNodes("Mercury", "Venus");
            var earth = sun.AddNode("Earth");               earth.AddNodes("Moon", "ISS");
            var mars = sun.AddNode("Mars");                 mars.AddNodes("Phobos", "Deimos");
            var jupiter = sun.AddNode("Jupiter");           jupiter.AddNodes("Io", "Europa", "Ganymede", "Callisto", "Amalthea", "Thebe", "Himalia", "Elara", "Lysithea");
            var saturn = sun.AddNode("Saturn");             saturn.AddNodes("Titan", "Enceladus", "Iapetus", "Rhea", "Dione", "Tethys", "Mimas", "Hyperion", "Phoebe", "Janus", "Epimetheus");
            var uranus = sun.AddNode("Uranus");             uranus.AddNodes("Miranda", "Ariel", "Umbriel", "Titania", "Oberon");
            var neptune = sun.AddNode("Neptune");           neptune.AddNodes("Triton", "Nereid", "Proteus", "Larissa");
            var pluto = sun.AddNode("Pluto");               pluto.AddNodes("Charon", "Hydra", "Nix", "Kerberos", "Styx");            

            layout["Left"].Update(
                new Panel(universe)
                .RoundedBorder()
                .Header("[cyan] Universe Tree :deciduous_tree: [/]")
                .Expand()
            );


            //==================== Middle panel ====================//

            //---------- Add banner ----------//
            layout["Middle"]["Header"].Update(
                new Panel(
                    Align.Left(
                        new FigletText(FigletFont.Load("../../assets/starwars.flf"), "Galaxy Wiki")
                        .Centered()
                        .Color(Color.Aqua),
                        VerticalAlignment.Bottom
                    )
                )
                .NoBorder()
                .Expand()
            );

            //---------- Add prompt ----------//
            var command = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose your command:")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                    .AddChoices([ "info", "comment", "cd" ])
            );

            layout["Middle"]["Terminal"].Update(
                new Panel(command)
                .RoundedBorder()
                .Header("[cyan] Terminal [/]")
                .Expand()
            )
            .Ratio(2);

            layout["Middle"].Ratio(2);


            //==================== Right panel ====================//

            //---------- Add image ----------//
            CanvasImage image = new CanvasImage("../../assets/earth.png").MaxWidth(12);
            
            layout["Right"]["Mid"].Update(
                new Panel(Align.Center(image, VerticalAlignment.Middle))
                .RoundedBorder()
                .Header("[cyan] Image :camera: [/]")
                .Expand()
            );


            //---------- Add chatbot ----------//
            layout["Right"]["Bottom"].Update(
                new Panel(
                    Align.Center(new Markup("This is where the chatbot will be"), VerticalAlignment.Middle)
                )
                .RoundedBorder()
                .Header("[cyan] Galaxy Bot :robot::sparkles: [/]")
                .Expand()
            );

            //---------- Add useless calendar ----------//
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


            //==================== Output ====================//

            AnsiConsole.Write(layout);
        }
    }
}