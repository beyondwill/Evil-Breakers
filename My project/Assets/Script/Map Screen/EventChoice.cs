using System;
using System.Collections.Generic;

[Serializable]
public class EventChoice
{
    public string choiceText;

    public List<EventEffect> effects;

    public EventInfo nextEvent;
}