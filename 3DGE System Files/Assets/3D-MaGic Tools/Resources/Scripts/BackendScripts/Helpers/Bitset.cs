
// Bitset information - https://www.youtube.com/watch?v=Q_Apap8Dfbk&ab_channel=LevelUp
using System;
using System.Drawing;

[System.Serializable]
public struct BitsetData
{
    public int Size;
    public int[] Data;
}

public class Bitset
{
    // VARIABLES **********************************************************************************
    // ********************************************************************************************

    private int[] BitsetArray;
    private int m_bitsetSize = 0;
    public int Size() => m_bitsetSize;

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

    public Bitset(Bitset bitsetToCopy)
    {
        Setup(bitsetToCopy.m_bitsetSize);
        Copy(bitsetToCopy);
    }

    // ********************************************************************************************
    // ********************************************************************************************



    // FUNCTIONS **********************************************************************************
    // ********************************************************************************************
    private void Setup(int size)
    {
        BitsetArray = new int[(size / 32) + 1];
        m_bitsetSize = size;
    }

    public void SetBits(int[] value)
    {
        BitsetArray = value;
    }

    public bool Copy(Bitset otherBitset)
    {
        if (otherBitset.m_bitsetSize != m_bitsetSize) return false;

        for (int i = 0; i < m_bitsetSize / 32 + 1; i++)
        {
            BitsetArray[i] = otherBitset.BitsetArray[i];
        }

        return true;
    }

    // ********************************************************************************************
    // ********************************************************************************************

    public int[] GetBitset()
    {
        return BitsetArray;
    }

    public void SetAllBits()
    {
        for(int i = 0; i < BitsetArray.Length; i++)
        {
            BitsetArray[i] = -1;
        } 
    }

    public void ResetAllBits()
    {
        for (int i = 0; i < BitsetArray.Length; i++)
        {
            BitsetArray[i] = 0;
        }
    }

    public bool IsAllReset()
    {
        bool result = true;
        foreach(int bit in BitsetArray)
        {
            if (bit != 0) result = false;
        }
        return result;
    }


    public void SetBitAtIndex(int index)
    {
        if (index > m_bitsetSize) return;
        BitsetArray[index / 32] |= (1 << (index % 32));
    }

    // Resets the bit on the index entered
    public void ResetBitAtIndex(int index)
    {
        if (index > m_bitsetSize) return;
        BitsetArray[index / 32] &= ~(1 << (index % 32));
    }

    public int IfSetAtIndex(int index)
    {
        if (index > m_bitsetSize) return -1;

        return (BitsetArray[index / 32] & (1 << index)) >> index;
    }

    public bool IfResetAtIndex(int index)
    {
        if (index > m_bitsetSize) return false;
        return (BitsetArray[index / 32] & (1 << index)) != 0;
    }

    public bool this[int index]
    {
        get { return IfResetAtIndex(index); }
    }

    public void ToggleBitAtIndex(int index)
    {
        if (index > m_bitsetSize) return;
        BitsetArray[index / 32] ^= (1 << index);
    }


    public string Print()
    {
        string str = "";

        for (int i = 0; i < (m_bitsetSize / 32 + 1); i++)
        {

            str += Convert.ToString(BitsetArray[i], 2);
        }
        return str;
    }

    // ********************************************************************************************
    // ********************************************************************************************



    // OPERATORS **********************************************************************************
    // ********************************************************************************************
    public static Bitset operator &(Bitset A_bits, Bitset B_bits)
    {
        if (A_bits.m_bitsetSize != B_bits.m_bitsetSize) return A_bits;

        for (int i = 0; i < A_bits.GetBitset().Length; i++)
        {
            A_bits.GetBitset()[i] &= B_bits.GetBitset()[i];
        }
        return A_bits;
    }

    public static Bitset operator |(Bitset A_bits, Bitset B_bits)
    {
        if (A_bits.m_bitsetSize != B_bits.m_bitsetSize) return null;

        for (int i = 0; i < A_bits.GetBitset().Length; i++)
        {
            A_bits.GetBitset()[i] |= B_bits.GetBitset()[i];
        }
        return A_bits;
    }

    // STATIC FUNCTIONS ***************************************************************************
    // ********************************************************************************************

    public static bool DoesBitsetMatchOther(Bitset bitset_A, Bitset bitset_B)
    {
        if (bitset_A.m_bitsetSize != bitset_B.m_bitsetSize) return false;

        for (int i = 0; i < bitset_A.m_bitsetSize / 32 + 1; i++)
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
        data.Size = m_bitsetSize;
        data.Data = BitsetArray;
    }

    public void Load(BitsetData data)
    {
        m_bitsetSize = data.Size;
        BitsetArray = data.Data;
    }

    // ********************************************************************************************
    // ********************************************************************************************
}
