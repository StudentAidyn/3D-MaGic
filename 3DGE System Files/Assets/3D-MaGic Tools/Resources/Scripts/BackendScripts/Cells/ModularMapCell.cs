using UnityEngine;

public class ModularMapCell
{
    // + Variables +

    // Module Type:
    public int Module = -1;
    public Bitset Options;

    // + Constructor +
    public ModularMapCell(int bitsetSize)
    {
        Options = new Bitset(bitsetSize);
        Options.SetAllBits();
    }
}
