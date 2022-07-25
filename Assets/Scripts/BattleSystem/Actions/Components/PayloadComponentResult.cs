using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Result of applying a payload component.
// When applying payloads, it may be passed further on to access related data and set off triggers.
public class PayloadComponentResult
{
    public Payload Payload;             // Information about the payload
    public PayloadComponent PayloadComponent;   // Information about the component
    public float ResultValue;                   // Some components can evaluate to a value, such as damage dealt. 
    public List<string> ResultFlags;            // Some components can generate flags. 

    public PayloadComponentResult(Payload payloadInfo, PayloadComponent payloadComponent)
    {
        Payload = payloadInfo;
        ResultFlags = new List<string>();
        PayloadComponent = payloadComponent;
    }
}
