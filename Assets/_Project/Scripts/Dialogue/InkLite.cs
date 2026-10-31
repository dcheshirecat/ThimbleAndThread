using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// Reads and plays dialogue written in a subset of Ink (inklestudios.com/ink), so the .ink files in
/// Assets/_Project/Dialogue can be written and previewed in Inky. Supported:
///   === knot_name ===            a scene
///   Mara: Hello there. # happy   a line, with an optional speaker and tags (# happy sets the portrait)
///   Narration without a speaker.
///   * [Choice text] Said after   choices (* or +), nested with * * ; "[...]" is shown only on the button
///   * {flag} [Text]              a choice shown only when the game says the condition holds ({not flag} too)
///   - Gathered again.            a gather: where the choices meet up again
///   -> other_knot / -> END       jumps
///   ~ friendship(5)              a command for the game (see DialogueRunner's callbacks)
///   # fits: scarf, sweater       tags before the first line belong to the whole knot
///   // a comment
/// Anything else from Ink (variables, stitches, logic) isn't supported.
public class InkLite
{
    public class Knot
    {
        public string name;
        public List<string> tags = new List<string>();     // knot-level tags, e.g. "fits: scarf"
        public List<Instr> code = new List<Instr>();
    }

    public enum Op { Say, Call, Choices, Jump, Divert, End }

    public class Option
    {
        public string text;           // on the button
        public string condition;      // shown only if the game agrees, or null
        public int target;            // where the choice's own content starts
    }

    public class Instr
    {
        public Op op;
        public string speaker, text, call;   // Say: speaker (or null) and text; Call: the command
        public List<string> tags = new List<string>();
        public List<Option> options;         // Choices
        public int target;                   // Jump
        public string knot;                  // Divert
    }

    readonly Dictionary<string, Knot> knots = new Dictionary<string, Knot>();

    public bool Has(string knot) => knot != null && knots.ContainsKey(knot);
    public Knot Get(string knot) => knots.TryGetValue(knot, out var k) ? k : null;
    public IEnumerable<string> KnotNames => knots.Keys;

    static readonly Regex KnotHeader = new Regex(@"^={2,}\s*([A-Za-z0-9_]+)\s*=*\s*$");
    static readonly Regex Speaker = new Regex(@"^([A-Z][A-Za-z' .]{0,24}):\s+(.+)$");
    static readonly Regex Condition = new Regex(@"^\{([^}]*)\}\s*");

    public void Load(string source)
    {
        Knot knot = null;
        var groups = new List<Group>();
        foreach (var raw in source.Replace("\r", "").Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("//")) continue;
            var header = KnotHeader.Match(line);
            if (header.Success)
            {
                if (knot != null) Close(knot, groups, 0);
                knot = new Knot { name = header.Groups[1].Value };
                knots[knot.name] = knot;
                continue;
            }
            if (knot == null) continue;   // text before the first knot is ignored

            if (line.StartsWith("#") && knot.code.Count == 0 && groups.Count == 0)
            {
                foreach (var t in line.Split('#')) if (t.Trim().Length > 0) knot.tags.Add(t.Trim());
                continue;
            }
            if (line[0] == '*' || line[0] == '+') { Choice(knot, groups, line); continue; }
            if (line[0] == '-' && !line.StartsWith("->"))
            {
                int depth = 0;
                while (line.Length > 0 && (line[0] == '-' && !line.StartsWith("->") || line[0] == ' ')) { if (line[0] == '-') depth++; line = line.Substring(1); }
                Gather(knot, groups, depth);
                line = line.Trim();
                if (line.Length == 0) continue;
            }
            Content(knot, line);
        }
        if (knot != null) Close(knot, groups, 0);
    }

    // ---- Compiling choices ----

    class Group
    {
        public int depth;
        public Instr choices;
        public List<Instr> jumps = new List<Instr>();   // ends of choice bodies, jumping on to the gather
    }

    void Choice(Knot knot, List<Group> groups, string line)
    {
        int depth = 0;
        while (line.Length > 0 && (line[0] == '*' || line[0] == '+' || line[0] == ' ')) { if (line[0] != ' ') depth++; line = line.Substring(1); }
        CloseDeeper(knot, groups, depth);
        var top = groups.Count > 0 ? groups[groups.Count - 1] : null;
        if (top != null && top.depth == depth)
        {
            var jump = new Instr { op = Op.Jump, target = -1 };   // the previous choice's content ends: on to the gather
            knot.code.Add(jump);
            top.jumps.Add(jump);
        }
        else
        {
            top = new Group { depth = depth, choices = new Instr { op = Op.Choices, options = new List<Option>() } };
            knot.code.Add(top.choices);
            groups.Add(top);
        }

        line = line.Trim();
        string condition = null;
        var cond = Condition.Match(line);
        if (cond.Success) { condition = cond.Groups[1].Value.Trim(); line = line.Substring(cond.Length); }
        string divert = null;
        int arrow = line.IndexOf("->", StringComparison.Ordinal);
        if (arrow >= 0) { divert = line.Substring(arrow + 2).Trim(); line = line.Substring(0, arrow).Trim(); }

        string shown = line.Replace("[", "").Replace("]", "").Trim();
        string said = Regex.Replace(line, @"\[[^\]]*\]", "").Trim();
        top.choices.options.Add(new Option { text = shown, condition = condition, target = knot.code.Count });
        if (said.Length > 0) knot.code.Add(new Instr { op = Op.Say, text = said });   // told as narration
        if (divert != null) knot.code.Add(new Instr { op = Op.Divert, knot = divert });
    }

    void Gather(Knot knot, List<Group> groups, int depth)
    {
        CloseDeeper(knot, groups, depth);
        if (groups.Count > 0 && groups[groups.Count - 1].depth == depth)
        {
            var g = groups[groups.Count - 1];
            groups.RemoveAt(groups.Count - 1);
            foreach (var j in g.jumps) j.target = knot.code.Count;
        }
    }

    /// Ends choice groups deeper than "depth": their choices carry on to the enclosing group's gather.
    void CloseDeeper(Knot knot, List<Group> groups, int depth)
    {
        while (groups.Count > 0 && groups[groups.Count - 1].depth > depth)
        {
            var inner = groups[groups.Count - 1];
            groups.RemoveAt(groups.Count - 1);
            var jump = new Instr { op = Op.Jump, target = -1 };   // the last inner choice's content ends here too
            knot.code.Add(jump);
            inner.jumps.Add(jump);
            if (groups.Count > 0) groups[groups.Count - 1].jumps.AddRange(inner.jumps);   // else they stay -1: the end
        }
    }

    /// Ends a knot: any choices still open carry on to its end.
    void Close(Knot knot, List<Group> groups, int depth)
    {
        CloseDeeper(knot, groups, -1);
        int end = knot.code.Count;
        knot.code.Add(new Instr { op = Op.End });
        foreach (var i in knot.code) if (i.op == Op.Jump && i.target == -1) i.target = end;
        groups.Clear();
    }

    void Content(Knot knot, string line)
    {
        string divert = null;
        int arrow = line.IndexOf("->", StringComparison.Ordinal);
        if (arrow >= 0) { divert = line.Substring(arrow + 2).Trim(); line = line.Substring(0, arrow).Trim(); }
        if (line.StartsWith("~"))
        {
            knot.code.Add(new Instr { op = Op.Call, call = line.Substring(1).Trim() });
        }
        else if (line.Length > 0)
        {
            var tags = new List<string>();
            int hash = line.IndexOf('#');
            if (hash >= 0)
            {
                foreach (var t in line.Substring(hash).Split('#')) if (t.Trim().Length > 0) tags.Add(t.Trim());
                line = line.Substring(0, hash).Trim();
            }
            var m = Speaker.Match(line);
            knot.code.Add(m.Success
                ? new Instr { op = Op.Say, speaker = m.Groups[1].Value, text = m.Groups[2].Value, tags = tags }
                : new Instr { op = Op.Say, text = line, tags = tags });
        }
        if (divert != null) knot.code.Add(new Instr { op = Op.Divert, knot = divert });
    }
}

/// Plays one knot of InkLite dialogue, line by line. The game supplies how to check conditions and
/// what commands do.
public class DialogueRunner
{
    public class Line
    {
        public string speaker, text;
        public List<string> tags;
        public List<InkLite.Option> choices;   // set when the player must choose
    }

    readonly InkLite ink;
    readonly Func<string, bool> condition;
    readonly Action<string> command;
    InkLite.Knot knot;
    int pc;
    List<InkLite.Option> waiting;

    public bool Finished { get; private set; }

    public DialogueRunner(InkLite ink, string knotName, Func<string, bool> condition, Action<string> command)
    {
        this.ink = ink;
        this.condition = condition;
        this.command = command;
        knot = ink.Get(knotName);
        Finished = knot == null;
    }

    /// The next line or set of choices, or null when the knot is over.
    public Line Next()
    {
        if (waiting != null) return new Line { choices = waiting };
        for (int guard = 0; guard < 10000 && !Finished; guard++)
        {
            if (pc < 0 || pc >= knot.code.Count) { Finished = true; break; }
            var i = knot.code[pc++];
            switch (i.op)
            {
                case InkLite.Op.Say:
                    if (string.IsNullOrWhiteSpace(i.text)) break;   // a blank line would need an extra Continue
                    return new Line { speaker = i.speaker, text = i.text, tags = i.tags };
                case InkLite.Op.Call: command?.Invoke(i.call); break;
                case InkLite.Op.Jump: pc = i.target; break;
                case InkLite.Op.End: Finished = true; break;
                case InkLite.Op.Divert:
                    if (i.knot == "END" || i.knot == "DONE" || !ink.Has(i.knot)) Finished = true;
                    else { knot = ink.Get(i.knot); pc = 0; }
                    break;
                case InkLite.Op.Choices:
                    waiting = i.options.FindAll(o => o.condition == null || condition == null || condition(o.condition));
                    if (waiting.Count == 0) { waiting = null; break; }
                    return new Line { choices = waiting };
            }
        }
        return null;
    }

    public void Choose(int index)
    {
        if (waiting == null || index < 0 || index >= waiting.Count) return;
        pc = waiting[index].target;
        waiting = null;
    }
}
