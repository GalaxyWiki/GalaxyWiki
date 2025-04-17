using FluentNHibernate.Conventions;
using GalaxyWiki.CLI;
using GalaxyWiki.Core.Entities;
using NHibernate.Criterion;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace GalaxyWiki.CLI
{
    public class IdMap<T> : Dictionary<int, T> { }

    public static class TUI
    {
        //==================== Utils ====================//

        static int Mod(int x, int m) => (x % m + m) % m;

        static string Sanitize(string s) => s.Replace("[", "[[").Replace("]", "]]");

        static Color Shade(Color col, float amt) => Color.Default.Blend(col, amt).Blend(Color.White, 0.2f * (float)Math.Pow(amt, 8));

        // Wrap an arbitrary element in a TUI box
        public static Panel Boxed(IRenderable elem, String title = "", Color? color = null, Justify headAlign = Justify.Center)
        {
            return new Panel(Align.Center(elem, VerticalAlignment.Middle))
            .RoundedBorder()
            .Header(title)
            .BorderColor(color ?? Color.Default)
            .HeaderAlignment(headAlign)
            .Expand();
        }

        static string BodyToString(CelestialBodies body)
        {
            // TODO: Potentially append star-system group as tag
            return $"{BodyTypeToEmoji(body.BodyType)} ({body.Id}) {body.BodyName}";
        }

        public static string BodyTypeToEmoji(int bodyType)
        {
            return bodyType switch
            {
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

        // Format content with Spectre markup
        private static string FormatContentWithSpectre(string content)
        {
            // Decode & sanitize
            content = content
                .Replace("\\n", "\n")
                .Replace("[", "[[")
                .Replace("]", "]]");

            // Split text into paragraphs
            var paragraphs = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var result = new List<string>();

            foreach (var paragraph in paragraphs)
            {
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


        // Format comment text with Spectre markup
        private static string FormatCommentWithSpectre(string content)
        {
            // Replace literal '\n' with actual newline characters
            content = content.Replace("\\n", "\n");

            // Format names of celestial bodies with color
            var formattedContent = Regex.Replace(
                content,
                @"\b([A-Z][a-z]{2,}(?:\s[A-Z][a-z]*)*)\b",
                "[cyan]$1[/]"
            );

            return formattedContent;
        }



        //==================== Elements ====================//

        //-------------------- Static --------------------//

        //---------- Banner ----------//
        public static void ShowBanner()
        {
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
        public static void Err(string name, string desc, string info = "")
        {
            AnsiConsole.Markup($"[[[bold red]{name.ToUpper()} ERR[/]]]: [red]{desc}[/]");
            if (!info.Trim().IsEmpty()) { AnsiConsole.Markup("\n\t" + Sanitize(info).Replace("\n", "\n\t")); }
            AnsiConsole.Write("\n\n");
        }
        public static void Warn(string name, string desc, string info = "")
        {
            AnsiConsole.Markup($"[[[bold darkorange3]{name.ToUpper()} WARN[/]]]: [gold3]{desc}[/]");
            if (!info.Trim().IsEmpty()) { AnsiConsole.Markup("\n\t" + Sanitize(info).Replace("\n", "\n\t") + "\n\n"); }
            AnsiConsole.Write("\n\n");
        }

        //---------- Path ----------//
        public static Panel Path(string path)
        {
            var elem = new TextPath(path);
            elem.RootStyle = new Style(foreground: Color.Red);
            elem.SeparatorStyle = new Style(foreground: Color.Green);
            elem.StemStyle = new Style(foreground: Color.Blue);
            elem.LeafStyle = new Style(foreground: Color.Yellow);

            return Boxed(elem);
        }

        //---------- Calendar ----------//
        public static Panel Calendar()
        {
            var cal = new Calendar(DateTime.Today)
                .AddCalendarEvent(DateTime.Today)
                .HeaderStyle(Style.Parse("bold"))
                .HighlightStyle(Style.Parse("yellow bold"))
                .RoundedBorder();

            return Boxed(cal, "[cyan] Calendar :calendar: [/]");
        }

        //---------- Image ----------//
        public static Panel Image(string path, string? title = null)
        {
            return Boxed(
                new CanvasImage(path).MaxWidth(40),
                title ?? "[cyan] :camera: Image [/]",
                Color.SkyBlue1
            );
        }

        public static async Task RenderCelestialBody(string bodyName, int bodyType, int canvasW = 50)
        {
            bool isAnim = false;
            bool glow = false;
            string img;

            switch (bodyType)
            {
                case 4: // Moon
                case 2: // Star
                case 3: // Planet
                case 7: // Dwarf planet
                    string fileName = Regex.Replace(bodyName.ToLower(), @"\s+", "_");
                    string filePath = $"..\\..\\assets\\maps\\{fileName}.png";
                    img = File.Exists(filePath) ? filePath : "<generate>";
                    glow = bodyType == 2; // Star
                    isAnim = true;
                    break;

                case 1: img = "..\\..\\assets\\sprites\\galaxy.png"; break;
                case 5: img = "..\\..\\assets\\sprites\\satellite.png"; break;
                case 6: img = "..\\..\\assets\\sprites\\black_hole.png"; break;
                case 8: img = "..\\..\\assets\\sprites\\asteroid.png"; break;
                case 9: img = "..\\..\\assets\\sprites\\comet.png"; break;
                case 10: img = "..\\..\\assets\\sprites\\nebula.png"; break;
                case 11: img = "..\\..\\assets\\sprites\\universe.png"; break;
                default: img = "..\\..\\assets\\sprites\\default.png"; break;
            }
            ;

            string title = $"[cyan] {BodyTypeToEmoji(bodyType)} {bodyName} [/]";
            if (isAnim)
            { // Display anim sphere
                Color[,] tex;
                if (img == "<generate>") { tex = TextureUtils.GenerateTextureFromSeed(bodyName); }      // Generate texture
                else { tex = TextureUtils.LoadSphericalTexture(img, 200, 100); }    // Load texture

                await AnimSphere(tex, title, glow, canvasW);
            }
            else { AnsiConsole.Write(Align.Center(Image(img, title))); } // Display static image
        }

        public static async Task AnimSphere(Color[,] tex, string title, bool glow = false, int canvasW = 50)
        {
            int tw = tex.GetLength(0), th = tex.GetLength(1);
            var c = new Canvas(canvasW, canvasW);

            // Render
            await AnsiConsole.Live(Boxed(c, title)).StartAsync(async ctx =>
            {
                for (float frame = 0.0f; frame < 10.0f; frame += 0.1f)
                {
                    for (int px = 0; px < canvasW; px++)
                        for (int py = 0; py < canvasW; py++)
                        {
                            double xn = 2.0 * (px / (double)canvasW) - 1.0;
                            double yn = 2.0 * (py / (double)canvasW) - 1.0;
                            double d2 = xn * xn + yn * yn;
                            if (d2 >= 1.0)
                            { // Background
                                if (glow)
                                {
                                    float bg = (float)Math.Pow(d2, 2) / 2.0f;
                                    c.SetPixel(px, py, Color.Orange1.Blend(Color.Black, Math.Clamp(bg, 0.0f, 1.0f)));
                                }
                                continue;
                            }

                            // Compute z so that (xn, yn, z) lies on a unit sphere
                            double z = Math.Sqrt(1.0 - xn * xn - yn * yn);

                            // Apply rotation matrix
                            double cosF = Math.Cos(frame);
                            double sinF = Math.Sin(frame);
                            double rx = cosF * xn + sinF * z;
                            double ry = yn;
                            double rz = -sinF * xn + cosF * z;

                            // Compute spherical coords
                            double u = 0.5 + Math.Atan2(rz, rx) / (2 * Math.PI);    // Theta about y-axis
                            double v = Math.Acos(ry) / Math.PI;                     // Phi around y-axis

                            // Map uv to tex
                            int tx = ((int)(u * tw)) % tw;
                            int ty = th - ((int)(v * th)) % th;
                            if (tx < 0) tx += tw;
                            if (ty < 0) ty += th;

                            // Lighting
                            double lx = -0.25, ly = -0.4 + 0.2 * Math.Sin(frame), lz = 1;
                            double len = Math.Sqrt(lx * lx + ly * ly + lz * lz);
                            lx /= len; ly /= len; lz /= len;
                            double dot = xn * lx + yn * ly + z * lz;
                            double light = glow ? 1 : Math.Sqrt(Math.Max(0.05, dot));

                            Color p = Shade(tex[tx, ty], (float)light);
                            c.SetPixel(px, py, p);
                        }

                    // Update
                    ctx.Refresh();
                    await Task.Delay(10);
                }
            });
        }


        //---------- Article ----------//
        public static Panel Article(string bodyName, int? bodyType, string? content)
        {
            string title = $"[bold cyan] {BodyTypeToEmoji(bodyType ?? -1)} {bodyName} [/]";
            if (content == null)
            {
                return Boxed(
                    Align.Left(new Markup("No content available")),
                    title,
                    Color.SpringGreen3_1
                );
            }

            return Boxed(
                Align.Left(new Markup(FormatContentWithSpectre(content))),
                $"[bold cyan] {BodyTypeToEmoji(bodyType ?? -1)} {bodyName} [/]",
                Color.SpringGreen3_1
            );
        }

        //---------- Author Info ----------//
        public static Panel AuthorInfo(string displayName, DateTime date)
        {
            var formattedDate = date.ToString("MMMM d, yyyy 'at' h:mm tt");
            return new Panel(
                Align.Right(
                    new Markup($"[italic grey]Last updated by [/][bold]{displayName}[/] [italic grey]on {formattedDate}[/]")
                )
            )
            .NoBorder()
            .Padding(0, 0, 0, 1);
        }


        //---------- Comment ----------//
        public static Panel Comment(string username, string content)
        {
            var layout = new Layout("Comment")
                .SplitRows(
                    new Layout("User"),
                    new Layout("Content")
                );

            layout["User"].Update(new Text(username));
            layout["Content"].Update(new Text(content));

            return Boxed(layout);
        }

        //---------- Comments Panel ----------//
        public static Panel CommentsPanel(List<Comment> comments, string title = "Comments")
        {
            if (comments.Count == 0)
            {
                return Boxed(
                    Align.Center(new Markup("[grey]No comments available[/]")),
                    $"[bold cyan] {title} [/]"
                );
            }

            var table = new Table()
                .Border(TableBorder.None)
                .BorderColor(Color.Grey50)
                .HideHeaders()
                .Expand();

            // Single column for simplified layout
            table.AddColumn(new TableColumn("").Width(100));

            foreach (var comment in comments)
            {
                // Format the comment text - the main focus
                string formattedComment = FormatCommentWithSpectre(comment.CommentText);
                
                // Format date in a simple style
                string formattedDate = FormatRelativeTime(comment.CreatedDate);
                
                // Create the author info for the bottom right
                string author = string.IsNullOrEmpty(comment.DisplayName) ?
                    "Anonymous" :
                    comment.DisplayName;
                
                // Create footer with author and date
                string footer = $"— [bold]{author}[/] • [dim]{formattedDate}[/] [grey](#{comment.CommentId})[/]";
                
                // Create a layout for the comment with aligned content
                var commentContent = new Markup(formattedComment);
                var footerContent = Align.Right(new Markup(footer));
                
                // Add the comment to the table with minimal padding for a compact view
                var panel = new Panel(
                    new Grid()
                        .AddColumn(new GridColumn())
                        .AddRow(commentContent)
                        .AddRow(footerContent)
                        .Collapse() // Make grid as compact as possible
                )
                .RoundedBorder()
                .BorderColor(Color.Grey50)
                .Padding(1, 0, 1, 0); // Horizontal padding only
                
                table.AddRow(panel);
                
                // No extra space between comments for a more compact view
            }

            return Boxed(table, $"[bold cyan] {title} ({comments.Count}) [/]", Color.Grey);
        }

        // Format relative time similar to YouTube
        private static string FormatRelativeTime(DateTime date)
        {
            var span = DateTime.Now - date;

            if (span.TotalDays > 365)
            {
                int years = (int)(span.TotalDays / 365);
                return years == 1 ? "1 year ago" : $"{years} years ago";
            }
            if (span.TotalDays > 30)
            {
                int months = (int)(span.TotalDays / 30);
                return months == 1 ? "1 month ago" : $"{months} months ago";
            }
            if (span.TotalDays > 7)
            {
                int weeks = (int)(span.TotalDays / 7);
                return weeks == 1 ? "1 week ago" : $"{weeks} weeks ago";
            }
            if (span.TotalDays >= 1)
            {
                int days = (int)span.TotalDays;
                return days == 1 ? "1 day ago" : $"{days} days ago";
            }
            if (span.TotalHours >= 1)
            {
                int hours = (int)span.TotalHours;
                return hours == 1 ? "1 hour ago" : $"{hours} hours ago";
            }
            if (span.TotalMinutes >= 1)
            {
                int minutes = (int)span.TotalMinutes;
                return minutes == 1 ? "1 minute ago" : $"{minutes} minutes ago";
            }

            return "just now";
        }

        //---------- Wiki Page ----------//
        public static Panel WikiPage(Revision rev, int? bodyType, List<Comment> comments)
        {
            int w = Console.BufferWidth;
            int h = Console.BufferHeight;

            var layout = new Layout("Page")
                .SplitRows(
                    new Layout("Title").Ratio(1),
                    new Layout("Article").Ratio(10),
                    new Layout("Comments").Ratio(10)
                );

            layout["Title"].Update(AuthorInfo(rev.AuthorDisplayName ?? "Unknown", rev.CreatedAt));
            layout["Article"].Update(Article(rev.CelestialBodyName ?? "Unknown", bodyType, rev.Content));
            layout["Comments"].Update(CommentsPanel(comments));

            return Boxed(layout);
        }

        //---------- Celestial Tree ----------//
        public static void CelestialTree(IdMap<CelestialBodies> bodyMap, Tree rootTree, int rootId, IdMap<TreeNode> nodeMap)
        {
            RecBuildTree(bodyMap, rootTree, rootId, nodeMap);
        }

        static void RecBuildTree(
            IdMap<CelestialBodies> bodyMap,
            object parentNode,
            int bodyId,
            IdMap<TreeNode> nodeMap
        )
        {
            // Get body by ID
            if (!bodyMap.TryGetValue(bodyId, out var body)) { return; }

            // Create node
            string label = BodyToString(body);

            TreeNode node;
            switch (parentNode)
            {
                case Tree t: node = t.AddNode(label); break;
                case TreeNode tn: node = tn.AddNode(label); break;
                default: return; // Unsupported parent type
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
        )
        {
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
        )
        {
            // Get body by ID
            if (!bodyMap.TryGetValue(bodyId, out var body)) { return; }

            // Create text display
            string indent = new string(' ', level * 2) + (level > 0 ? "└─ " : "");
            string label = indent + BodyToString(body);

            // Add to the list of items
            items.Add((label, body));

            // Find all children that orbit this body
            var children = bodyMap.Values.Where(b => b.Orbits != null && b.Orbits.Id == body.Id).ToList();
            children.Sort((a, b) => a.Id.CompareTo(b.Id));

            // Recursively add children
            foreach (var child in children)
            {
                RecBuildSelectableTree(bodyMap, child.Id, items, level + 1);
            }
        }

        //---------- Interactive CD Selector ----------//
        public static string? DestinationSelector(string[] destinations, string title = "Select destination:")
        {
            if (destinations.Length == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No destinations available.[/]");
                return null;
            }

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(title)
                    .PageSize(20)
                    .HighlightStyle(new Style(Color.SpringGreen3_1, Color.Black, Decoration.Underline))
                    .AddChoices(destinations)
            );

            return selection;
        }

        // Returns (newContent, changed) tuple
        public static (string, bool) OpenExternalEditor(string originalContent)
        {
            string tempFilePath = System.IO.Path.GetTempFileName();
            File.WriteAllText(tempFilePath, originalContent);

            var startInfo = new ProcessStartInfo
            {
                FileName = "explorer",
                Arguments = $"\"{tempFilePath}\""
            };

            Process? editorProcess = null;
            try { editorProcess = Process.Start(startInfo); }
            catch (Exception ex) { Err("LAUNCH", "Failed to launch editor", ex.Message); }
            if (editorProcess == null) return (originalContent, false);

            var rule = new Rule("[green bold]Content opened in default editor[/]");
            rule.Style = Style.Parse("green");
            AnsiConsole.Write(rule);

            bool done = false;
            while (!done) { done = AnsiConsole.Prompt(new ConfirmationPrompt("Are you done editing?")); }

            // Try to close the editor if it's still open
            try
            {
                if (editorProcess != null && !editorProcess.HasExited)
                {
                    editorProcess.Kill(true);
                    editorProcess.WaitForExit();
                }
            }
            catch { }

            string newContent = File.ReadAllText(tempFilePath);
            try { File.Delete(tempFilePath); } catch { } // Cleanup

            bool contentChanged = !string.Equals(originalContent, newContent, StringComparison.Ordinal);
            return (newContent, contentChanged);
        }

        public static void ShowHelp()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("GalaxyWiki CLI").Centered());
            AnsiConsole.WriteLine();

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .AddColumn(new TableColumn("Command").LeftAligned())
                .AddColumn(new TableColumn("Description").LeftAligned());

            table.AddRow("cd <path>", "Navigate to a celestial body");
            table.AddRow("ls", "List bodies in current location");
            table.AddRow("info, show", "Show info about current location");
            table.AddRow("show -n <name>", "Show info about a specific body");
            table.AddRow("render", "Show visual representation of current body");
            table.AddRow("pwd", "Print current path");
            table.AddRow("comment <text>", "Add a comment to the current body");
            table.AddRow("revision", "Show revision history for current body");
            table.AddRow("revision -n <name>", "Show revision history for a specific body");
            table.AddRow("find <term>", "Search for celestial bodies");
            table.AddRow("create", "Create a new revision for current body");
            table.AddRow("edit", "Edit the current body's content");
            table.AddRow("login", "Login to the system");
            table.AddRow("exit, quit", "Exit the application");

            AnsiConsole.Write(table);
        }
    }
}