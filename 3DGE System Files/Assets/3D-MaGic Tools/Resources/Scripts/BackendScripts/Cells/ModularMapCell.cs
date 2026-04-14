using UnityEngine;

public struct ModularMapCell
{
    // + Variables +

    // Module m_type:
    public int Module;
    public Bitset Options;

    // + Constructor +
    public ModularMapCell(int bitsetSize)
    {
        Module = -1;
        Options = new Bitset(bitsetSize);
        Options.SetAllBits();
    }
}
