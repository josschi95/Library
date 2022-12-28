using System.Collections;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
 
/// <summary>
/// This script will handle the on-screen elements for creating and displaying nodes on the graph view
/// </summary>
public class DialogueGraphView : GraphView
{
    public delegate void OnChange();
    public OnChange onChange;

    public readonly Vector2 defaultNodeSize = new Vector2(150, 200);
    public readonly Vector2 maxNodeSize = new Vector2(185, 90);

    private NodeSearchWindow searchWindow;

    public DialogueGraphView(DialogueGraphWindow editorWindow)
    {
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        AddElement(GenerateEntryPointNode());
        AddSearchWindow(editorWindow);
    }

    #region - Nodes -
    //Create the static entry point node for the dialogue
    private DialogueNode GenerateEntryPointNode()
    {
        var node = new DialogueNode
        {
            title = "START",
            GUID = Guid.NewGuid().ToString(),
            dialogueText = "Entry Point",
            entryPoint = true
        };

        var generatedPort = GeneratePort(node, Direction.Output);
        generatedPort.portName = "Next";
        node.outputContainer.Add(generatedPort);

        //node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

        node.SetPosition(new Rect(100, 200, 150, 150));

        node.Refresh();
        return node;
    }

    //Creates a new dialogue node and adds it to the graph
    public void CreateNode(string nodeName, Vector2 position, bool player = false)
    {
        AddElement(CreateDialogueNode(nodeName, position, player));
    }

    //Creates a new dialogue node, but does not add the element by default
    public DialogueNode CreateDialogueNode(string nodeName, Vector2 position, bool player = false)
    {
        var dialogueNode = new DialogueNode
        {
            title = nodeName,
            dialogueText = nodeName,
            isPlayerNode = player,
            GUID = Guid.NewGuid().ToString(),
        };

        var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        dialogueNode.inputContainer.Add(inputPort);

        if (player) dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node_Player"));
        else dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node_NPC"));

        //Add a button to add further ports
        var button = new Button(() =>
        {
            AddChoicePort(dialogueNode);
        });
        button.text = "New Choice";
        dialogueNode.titleContainer.Add(button);

        //Adds field to enter main dialogue for this node
        var textField = new TextField(string.Empty);
        textField.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.dialogueText = evt.newValue;
            dialogueNode.title = evt.newValue;
        });
        textField.SetValueWithoutNotify(dialogueNode.title);
        dialogueNode.SetTextField(textField);
        dialogueNode.mainContainer.Add(textField);

        //dialogueNode.style.maxHeight = defaultNodeSize.y;
        //dialogueNode.style.maxWidth = defaultNodeSize.x;        

        dialogueNode.Refresh();
        dialogueNode.SetPosition(new Rect(position, defaultNodeSize));

        onChange?.Invoke();
        return dialogueNode;
    }
    #endregion

    #region - Ports -
    private Port GeneratePort(DialogueNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float)); //Arbitray type
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach((port) =>
       {
           if (startPort != port && startPort.node!= port.node)
               compatiblePorts.Add(port);
       });

        return compatiblePorts;
    }

    //Add a new output port onto the node, which allows it to branch to other nodes
    public void AddChoicePort(DialogueNode node, string overriddenPortName = "")
    {
        var generatedPort = GeneratePort(node, Direction.Output);

        generatedPort.CapturePointer(0);

        //Remove previous lable to reduce clutter/redundancy
        var oldLabel = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(oldLabel);

        //Set the name of the port, to a default or a given one
        var outputPortCount = node.outputContainer.Query("connector").ToList().Count;
        var choicePortName = string.IsNullOrEmpty(overriddenPortName) 
            ? $"Choice {outputPortCount + 1}" 
            : overriddenPortName;

        //Add a text field to set the dialogue text of the port
        var textField = new TextField
        {
            name = string.Empty,
            value = choicePortName,
            multiline = true,            
        };
        textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
        generatedPort.contentContainer.Add(new Label("  "));
        generatedPort.contentContainer.Add(textField);

        //Add a delete button to remove the port
        var deleteButton = new Button(() => RemovePort(node, generatedPort)) { text = "X" };
        generatedPort.contentContainer.Add(deleteButton);

        //Add the port and update the node
        generatedPort.portName = choicePortName;
        node.outputContainer.Add(generatedPort);
        node.Refresh();

        onChange?.Invoke();
    }

    //Remove an output port from the node and any associated edge
    private void RemovePort(DialogueNode dialogueNode, Port generatedPort)
    {
        var targetEdge = edges.ToList().Where(x => x.output.portName == generatedPort.portName 
            && x.output.node == generatedPort.node);

        if (!targetEdge.Any()) return;
        var edge = targetEdge.First();
        edge.input.Disconnect(edge);
        RemoveElement(targetEdge.First());

        dialogueNode.outputContainer.Remove(generatedPort);
        dialogueNode.Refresh();

        onChange?.Invoke();
    }
    #endregion

    private void AddSearchWindow(DialogueGraphWindow editorWindow)
    {
        searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        searchWindow.Init(editorWindow, this);
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
    }

    #region - Graph Reconstruction
    //Clears the current graph and re-creates the given dialogueContainer graph
    public void ReconstructGraph(DialogueContainer containerCache)
    {
        ClearGraph(containerCache);
        CreateNodes(containerCache);
        ConnectNodes(containerCache);

        EditorWindow.GetWindow<DialogueGraphWindow>().OnFileOpened();
        EditorWindow.GetWindow<DialogueInspector>().OnFileOpened(containerCache);
    }

    //Clears all nodes and edges from the current graphview
    private void ClearGraph(DialogueContainer containerCache)
    {
        //Set entry points guid back from the save. Discard existing grid
        var tempNodes = nodes.ToList().Cast<DialogueNode>().ToList();
        tempNodes.Find(x => x.entryPoint).GUID = containerCache.nodeLinks[0].baseNodeGuid;

        foreach(var node in tempNodes)
        {
            if (node.entryPoint) continue;

            //Remove edges that connected to this node
            edges.Where(x => x.input.node == node).ToList() //List<Edge>
                .ForEach(edge => RemoveElement(edge));

            //Then remove the ndoe
            RemoveElement(node);
        }
    }

    //Re-creates all nodes in the given dialogueContainer
    private void CreateNodes(DialogueContainer containerCache)
    {
        foreach (var nodeData in containerCache.dialogueNodeData)
        {
            //Passing vector2.zero since position will be set later
            var tempNode = CreateDialogueNode(nodeData.dialogueText, Vector2.zero);
            tempNode.GUID = nodeData.Guid;
            AddElement(tempNode);

            var nodePorts = containerCache.nodeLinks.Where(x => x.baseNodeGuid == nodeData.Guid).ToList();
            nodePorts.ForEach(x => AddChoicePort(tempNode, x.portname));
        }
    }

    //Connects the nodes
    private void ConnectNodes(DialogueContainer containerCache)
    {
        var tempNodes = nodes.ToList().Cast<DialogueNode>().ToList();
        tempNodes.Find(x => x.entryPoint).GUID = containerCache.nodeLinks[0].baseNodeGuid;

        for (int i = 0; i < tempNodes.Count; i++)
        {
            var connections = containerCache.nodeLinks.Where(x => x.baseNodeGuid == tempNodes[i].GUID).ToList();
            for (int j = 0; j < connections.Count; j++)
            {
                var targetNodeGuid = connections[j].targetNodeGuid;
                var targetNode = tempNodes.First(x => x.GUID == targetNodeGuid);
                LinkNodes(tempNodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                targetNode.SetPosition(new Rect(containerCache.dialogueNodeData.First(x =>
                    x.Guid == targetNodeGuid).position, defaultNodeSize));
            }
        }
    }

    //Creates the edges linking the nodes
    private void LinkNodes(Port output, Port input)
    {
        var tempEdge = new Edge
        {
            output = output,
            input = input
        };

        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);

        Add(tempEdge);
    }
    #endregion
}
