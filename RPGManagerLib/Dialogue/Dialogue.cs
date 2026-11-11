using RPGManagerLib.UI;

namespace RPGManagerLib.Dialogue;

/// <summary>
/// One thing the player can say. Conditions and effects are small functions so each option
/// can check flags, reputation or class and change the world without the runner knowing how.
/// </summary>
public class DialogueOption
{
    public string Text { get; }

    /// <summary>Node to go to next; null ends the conversation.</summary>
    public string? Next { get; }

    /// <summary>The option only appears when this returns true (null = always).</summary>
    public Func<bool>? Condition { get; init; }

    /// <summary>Runs when the option is chosen, before moving on.</summary>
    public Action? Effect { get; init; }

    public DialogueOption(string text, string? next)
    {
        Text = text;
        Next = next;
    }
}

/// <summary>
/// One beat of a conversation: something is said, then the player picks a reply.
/// </summary>
public class DialogueNode
{
    public string Id { get; }

    /// <summary>Prints what the NPC says or does at this node.</summary>
    public Action Speak { get; }

    public List<DialogueOption> Options { get; } = new();

    public DialogueNode(string id, Action speak, params DialogueOption[] options)
    {
        Id = id;
        Speak = speak;
        Options.AddRange(options);
    }
}

/// <summary>
/// A conversation as a dictionary of nodes, navigated by node id.
/// </summary>
public class DialogueTree
{
    private readonly Dictionary<string, DialogueNode> nodes = new();

    /// <summary>Adds a node and returns the tree for chaining.</summary>
    public DialogueTree Add(DialogueNode node)
    {
        nodes[node.Id] = node;
        return this;
    }

    /// <summary>Walks the tree from a starting node until an option ends the conversation.</summary>
    public void Run(string startId)
    {
        string? current = startId;
        while (current != null && nodes.TryGetValue(current, out var node))
        {
            Ui.Line();
            node.Speak();

            var available = node.Options.Where(o => o.Condition == null || o.Condition()).ToList();
            if (available.Count == 0) return;

            int choice = Ui.Choose(available.Select(o => o.Text).ToList());
            var chosen = available[choice];
            chosen.Effect?.Invoke();
            current = chosen.Next;
        }
    }
}
