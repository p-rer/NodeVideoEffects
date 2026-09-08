using Node.Graph.Port;

namespace Node.Graph.Events;

public class NeedToReinitializeInputPortsEvent : EventArgs
{
    public NeedToReinitializeInputPortsEvent(string propName, InputsContainer newContainer)
    {
        PropName = propName;
        NewContainer = newContainer;
    }

    public string PropName { get; }
    public InputsContainer NewContainer { get; }
}