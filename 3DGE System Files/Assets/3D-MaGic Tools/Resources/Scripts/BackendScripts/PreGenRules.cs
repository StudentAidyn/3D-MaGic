using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PreGenRules : MonoBehaviour 
{
    public abstract void PreGen(Map _map);
}

public class LayerController : PreGenRules
{
    public override void PreGen(Map _map)
    {
        // Do something to layers
    }
}