using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Event/Event")]
public class EventInfo : ScriptableObject
{
    public string event_title;

    [TextArea]
    public string event_script;

    public Sprite event_image;

    public List<EventChoice> choices;
}