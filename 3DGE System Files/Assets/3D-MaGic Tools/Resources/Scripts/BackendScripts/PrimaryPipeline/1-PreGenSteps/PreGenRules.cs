using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PreGenRules : MonoBehaviour 
{
    public abstract void PreGen();
}

public class LayerController : PreGenRules
{
    public override void PreGen()
    {
        // Do something to layers
    }
}