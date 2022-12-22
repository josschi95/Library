/// <summary>
/// Interface to be used.... I'll come back to this
/// </summary>

public interface DialogueNodeVisitor
{
    void Visit(BasicDialogueNode node);
    void Visit(ChoiceDialogueNode node);
}
