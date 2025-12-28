
// Bitset information - https://www.youtube.com/watch?v=Q_Apap8Dfbk&ab_channel=LevelUp
using System;
using System.Drawing;

[System.Serializable]
public struct BitsetData
{
    public int int_size;
    public int[] arr_int_data;
}

public class Bitset
{
    // VARIABLES **********************************************************************************
    // ********************************************************************************************

    int[] bitset;
    int bitset_size = 0;
    public int Size() => bitset_size;

    // ********************************************************************************************
    // ********************************************************************************************


    // CONSTRUCTORS *******************************************************************************
    // ********************************************************************************************
    public Bitset()
    {

    }

    public Bitset(int size)
    {
        Setup(size);
    }

    public Bitset(Bitset bitset_to_copy)
    {
        Setup(bitset_to_copy.bitset_size);
        Copy(bitset_to_copy);
    }

    // ********************************************************************************************
    // ********************************************************************************************



    // FUNCTIONS **********************************************************************************
    // ********************************************************************************************
    private void Setup(int size)
    {
        bitset = new int[(size / 32) + 1];
        bitset_size = size;
    }

    public void bits_set(int[] value)
    {
        bitset = value;
    }

    public bool Copy(Bitset other_bits)
    {
        if (other_bits.bitset_size != bitset_size) return false;

        for (int i = 0; i < bitset_size / 32 + 1; i++)
        {
            bitset[i] = other_bits.bitset[i];
        }

        return true;
    }

    // ********************************************************************************************
    // ********************************************************************************************

    public int[] bits_get()
    {
        return bitset;
    }

    public void AllSet()
    {
        for(int i = 0; i < bitset.Length; i++)
        {
            bitset[i] = -1;
        } 
    }

    public void AllReset()
    {
        for (int i = 0; i < bitset.Length; i++)
        {
            bitset[i] = 0;
        }
    }

    public bool IsAllReset()
    {
        bool result = true;
        foreach(int bit in bitset)
        {
            if (bit != 0) result = false;
        }
        return result;
    }


    public void set(int index)
    {
        if (index > bitset_size) return;
        bitset[index / 32] |= (1 << (index % 32));
    }

    // Resets the bit on the index entered
    public void reset(int index)
    {
        if (index > bitset_size) return;
        bitset[index / 32] &= ~(1 << (index % 32));
    }

    public int ifset(int index)
    {
        if (index > bitset_size) return -1;

        return (bitset[index / 32] & (1 << index)) >> index;
    }

    public bool bifset(int index)
    {
        if (index > bitset_size) return false;
        return (bitset[index / 32] & (1 << index)) != 0;
    }

    public bool this[int index]
    {
        get { return bifset(index); }
    }

    public void toggle(int index)
    {
        if (index > bitset_size) return;
        bitset[index / 32] ^= (1 << index);
    }


    public string Print()
    {
        string str = "";

        for (int i = 0; i < (bitset_size / 32 + 1); i++)
        {

            str += Convert.ToString(bitset[i], 2);
        }
        return str;
    }

    // ********************************************************************************************
    // ********************************************************************************************



    // OPERATORS **********************************************************************************
    // ********************************************************************************************
    public static Bitset operator &(Bitset A_bits, Bitset B_bits)
    {
        if (A_bits.bitset_size != B_bits.bitset_size) return A_bits;

        for (int i = 0; i < A_bits.bits_get().Length; i++)
        {
            A_bits.bits_get()[i] &= B_bits.bits_get()[i];
        }
        return A_bits;
    }

    public static Bitset operator |(Bitset A_bits, Bitset B_bits)
    {
        if (A_bits.bitset_size != B_bits.bitset_size) return null;

        for (int i = 0; i < A_bits.bits_get().Length; i++)
        {
            A_bits.bits_get()[i] |= B_bits.bits_get()[i];
        }
        return A_bits;
    }

    // STATIC FUNCTIONS ***************************************************************************
    // ********************************************************************************************

    public static bool Compare_IsSame(Bitset bitset_A, Bitset bitset_B)
    {
        if (bitset_A.bitset_size != bitset_B.bitset_size) return false;

        for (int i = 0; i < bitset_A.bitset_size / 32 + 1; i++)
        {
            if(bitset_A[i] != bitset_B[i])
            {
                return false;
            }
        }

        return true;
    }


    // ********************************************************************************************
    // ********************************************************************************************



    // SAVE/LOAD **********************************************************************************
    // ********************************************************************************************
    
    public void Save(ref BitsetData data)
    {
        data.int_size = bitset_size;
        data.arr_int_data = bitset;
    }

    public void Load(BitsetData data)
    {
        bitset_size = data.int_size;
        bitset = data.arr_int_data;
    }

    // ********************************************************************************************
    // ********************************************************************************************
}
