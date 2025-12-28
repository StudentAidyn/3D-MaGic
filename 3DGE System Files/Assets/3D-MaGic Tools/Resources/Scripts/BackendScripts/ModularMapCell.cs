using UnityEngine;

public class ModularMapCell
{
    // + Variables +

    // Module Type:
    public int _int_module = -1;
    public Bitset _btst_options;

    // + Constructor +
    public ModularMapCell(int _bitsetSize)
    {
        _btst_options = new Bitset(_bitsetSize);
        _btst_options.AllSet();
    }
}
