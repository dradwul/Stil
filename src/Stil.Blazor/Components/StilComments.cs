using Microsoft.AspNetCore.Components.Forms;

namespace Stil.Blazor.Components;

/// <summary>
/// A comment thread: <c>StilChat</c> without sides, oldest first, growing with the thread. The
/// composer is taller and Ctrl+Enter sends, since comments often have several lines.
/// </summary>
public class StilComments<TItem> : StilChat<TItem>
{
    protected override bool Thread => true;
}

/// <summary>A message the user sends from <c>StilChat</c> or <c>StilComments</c>.</summary>
/// <param name="Text">The text, trimmed. It can be empty when files are attached.</param>
/// <param name="Files">The attached files; read them before the next send.</param>
public sealed record StilChatMessage(string Text, IReadOnlyList<IBrowserFile> Files);
