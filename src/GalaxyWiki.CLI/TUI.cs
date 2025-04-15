using FluentNHibernate.Conventions;
using GalaxyWiki.Core.Entities;
using NHibernate.Criterion;
using NHibernate.Util;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Text.RegularExpressions;

public class IdMap<T> : Dictionary<int, T> {}

public static class TUI {
    //==================== Utils ====================//

    // Wrap an arbitrary element in a TUI box
    public static Panel Boxed(IRenderable elem, String title = "") {
        return new Panel(Align.Center(elem, VerticalAlignment.Middle))
        .RoundedBorder()
        .Header(title)
        .Expand();
    }

    static string BodyToString(CelestialBodies body) {
        // TODO: Potentially append star-system group as tag
        return $"{BodyTypeToEmoji(body.BodyType)} ({body.Id}) {body.BodyName}";
    }

    public static string BodyTypeToEmoji(int bodyType) {
        return bodyType switch {
            1 => "🌌",  // Galaxy
            2 => "⭐",  // Star
            3 => "🪐",  // Planet
            4 => "🌙",  // Moon
            5 => "🛰️",  // Satellite
            6 => "⚫",  // Black Hole
            7 => "🧊",  // Dwarf Planet
            8 => "☄️",  // Asteroid
            9 => "☄️",  // Comet
            10 => "☁️", // Nebula
            11 => "🌠", // Universe
            _ => "🔭"   // Default
        };
    }



    //==================== Elements ====================//

    //-------------------- Static --------------------//

    //---------- Banner ----------//
    public static void ShowBanner() {
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
    //---------- Error ----------//
    public static void Err(string name, string desc, string info = "") {
        AnsiConsole.Markup($"[[[bold red]{name.ToUpper()} ERR[/]]]: [red]{desc}[/]{(info.Trim().IsEmpty() ? "\n\t" + info.Replace("\n", "\n\t") : "")}\n\n");
    }
    public static void Warn(string name, string desc, string info = "") {
        AnsiConsole.Markup($"[[[bold orange]{name.ToUpper()} WARN[/]]]: [orange]{desc}[/]{(info.Trim().IsEmpty() ? "\n\t" + info.Replace("\n", "\n\t") : "")}\n\n");
    }

    //---------- Path ----------//
    public static Panel Path(string path) {
        var elem = new TextPath(path);

        elem.RootStyle = new Style(foreground: Color.Red);
        elem.SeparatorStyle = new Style(foreground: Color.Green);
        elem.StemStyle = new Style(foreground: Color.Blue);
        elem.LeafStyle = new Style(foreground: Color.Yellow);

        return Boxed(elem);
    }

    //---------- Calendar ----------//
    public static Panel Calendar() {
        var cal = new Calendar(DateTime.Today)
            .AddCalendarEvent(DateTime.Today)
            .HeaderStyle(Style.Parse("bold"))
            .HighlightStyle(Style.Parse("yellow bold"))
            .RoundedBorder();

        return Boxed(cal, "[cyan] Calendar :calendar: [/]");
    }

    //---------- Image ----------//
    public static Panel Image(string path) {
        return Boxed(
            new CanvasImage(path).MaxWidth(12),
            "[cyan] Image :camera: [/]"
        );
    }

    //---------- Article ----------//
    public static Panel Article(string bodyName, string? content) {
        if (content == null) {
            return new Panel(
                Align.Left(new Markup("No content available"))
            )
            .BorderColor(Color.SpringGreen3_1)
            .RoundedBorder()
            .Header($"[bold cyan] {bodyName} [/]")
            .HeaderAlignment(Justify.Center);
        }

        // Parse content with Spectre markup
        var formattedContent = FormatContentWithSpectre(content);

        return new Panel(
            Align.Left(new Markup(formattedContent))
        )
        .BorderColor(Color.SpringGreen3_1)
        .RoundedBorder()
        .Header($"[bold cyan] {bodyName} [/]")
        .HeaderAlignment(Justify.Center);
    }

    // Format content with Spectre markup
    private static string FormatContentWithSpectre(string content) {
        // Replace literal '\n' with actual newline characters
        content = content.Replace("\\n", "\n");
        
        // Split text into paragraphs
        var paragraphs = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>();

        foreach (var paragraph in paragraphs) {
            // Format names of celestial bodies with color
            // This assumes names start with capital letters and have at least 3 characters
            var formattedParagraph = Regex.Replace(
                paragraph, 
                @"\b([A-Z][a-z]{2,}(?:\s[A-Z][a-z]*)*)\b", 
                "[cyan]$1[/]"
            );
            
            // Format scientific terms
            formattedParagraph = Regex.Replace(
                formattedParagraph,
                @"\b(galaxy|star|planet|moon|universe|Big Bang|gravity|light-year|atom|dark matter|dark energy)\b",
                match => "[yellow]" + match.Value + "[/]",
                RegexOptions.IgnoreCase
            );

            // Format numbers and measurements
            formattedParagraph = Regex.Replace(
                formattedParagraph,
                @"\b(\d+(?:\.\d+)?(?:\s*(?:billion|million|trillion|light-year|ly|kg|m|km))?)\b",
                "[green]$1[/]"
            );

            result.Add(formattedParagraph);
        }

        // Join paragraphs with line breaks between them
        return string.Join("\n\n", result);
    }

    //---------- Author Info ----------//
    public static Panel AuthorInfo(string displayName, DateTime date) {
        var formattedDate = date.ToString("MMMM d, yyyy 'at' h:mm tt");
        return new Panel(
            Align.Right(
                new Markup($"[italic grey]Written by [/][bold]{displayName}[/] [italic grey]on {formattedDate}[/]")
            )
        )
        .NoBorder()
        .Padding(0, 0, 0, 1);
    }


    //---------- Comment ----------//
    public static Panel Comment(string username, string content){
        var layout = new Layout("Comment")
            .SplitRows(
                new Layout("User"),
                new Layout("Content")
            );
        
        layout["User"].Update(new Text(username));
        layout["Content"].Update(new Text(content));

        return Boxed(layout);
    }

    //---------- Wiki Page ----------//
    public static Panel WikiPage() {
        var layout = new Layout("Page")
            .SplitRows(
                new Layout("Content").SplitColumns(
                    new Layout("Article"),
                    new Layout("Meta")
                ),
                new Layout("Comments")
            );
        
        layout["Article"].Update(new Text("PAGE CONTENT GOES HERE"));
        layout["Meta"].Update(Image("../../assets/earth.png"));

        return Boxed(layout);
    }

    //---------- Celestial Tree ----------//
    public static void CelestialTree(IdMap<CelestialBodies> bodyMap, Tree rootTree, int rootId, IdMap<TreeNode> nodeMap) {
        RecBuildTree(bodyMap, rootTree, rootId, nodeMap);
    }

    static void RecBuildTree(
        IdMap<CelestialBodies> bodyMap,
        object parentNode, 
        int bodyId, 
        IdMap<TreeNode> nodeMap
    ) {
        // Get body by ID
        if (!bodyMap.TryGetValue(bodyId, out var body)) { return; }
        
        // Create node
        string label = BodyToString(body);

        TreeNode node;
        switch(parentNode) {
            case Tree t:        node = t.AddNode(label);    break;
            case TreeNode tn:   node = tn.AddNode(label);   break;
            default:            return; // Unsupported parent type
        }

        nodeMap[body.Id] = node;
        
        // Find all children that orbit this body
        var children = bodyMap.Values.Where(b => b.Orbits != null && b.Orbits.Id == body.Id).ToList();
        
        // Recursively add children
        foreach (var child in children) { RecBuildTree(bodyMap, node, child.Id, nodeMap); }
    }


    //-------------------- Interactive --------------------//

    //---------- Celestial Tree Selectable ----------//
    public static string? CelestialTreeSelectable(
        IdMap<CelestialBodies> bodyMap,
        int rootId,
        List<(string DisplayLabel, CelestialBodies Body)> items
    ) {
        RecBuildSelectableTree(bodyMap, rootId, items, 0);
            
        if (items.Count == 0) { AnsiConsole.WriteLine("No celestial bodies found."); return null; }
        
        // Display the selection prompt
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a celestial body to view details:")
                .PageSize(30)
                .HighlightStyle(new Style(Color.SpringGreen3_1, Color.Black, Decoration.Underline))
                .AddChoices(items.Select(i => i.DisplayLabel))
        );

        return selection;
    }

    public static void RecBuildSelectableTree(
        IdMap<CelestialBodies> bodyMap, 
        int bodyId, 
        List<(string DisplayLabel, CelestialBodies Body)> items, 
        int level
    ) {
        // Get body by ID
        if (!bodyMap.TryGetValue(bodyId, out var body)) { return; }

        // Create text display
        string indent = new string(' ', level * 2) + (level > 0 ? "└─ " : "");
        string label = indent + BodyToString(body);
        
        // Add to the list of items
        items.Add((label, body));
        
        // Find all children that orbit this body
        var children = bodyMap.Values.Where(b => b.Orbits != null && b.Orbits.Id == body.Id).ToList();
        
        // Recursively add children
        foreach (var child in children) {
            RecBuildSelectableTree(bodyMap, child.Id, items, level + 1);
        }
    }


}