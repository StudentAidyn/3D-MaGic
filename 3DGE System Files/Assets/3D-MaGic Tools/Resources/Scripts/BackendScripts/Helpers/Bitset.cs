
// Bitset information - https://www.youtube.com/watch?v=Q_Apap8Dfbk&ab_channel=LevelUp
using System;
using System.Drawing;
using System.Text;

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
        int bitsetArraySize = (size / 32) + 1;
        BitsetArray = new int[bitsetArraySize];
        m_bitsetSize = size;
    }

    public void SetBits(int[] value)
    {
        BitsetArray = value;
    }

    public bool Copy(Bitset otherBitset)
    {
        if (otherBitset.m_bitsetSize != m_bitsetSize) return false;

        int bitsetArraySize = (m_bitsetSize / 32) + 1;
        for (int i = 0; i < bitsetArraySize; i++)
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
        string bitsetString = "";

        for (int i = 0; i < (m_bitsetSize / 32 + 1); i++)
        {

            bitsetString += Convert.ToString(BitsetArray[i], 2);
        }
        
        StringBuilder sb = new StringBuilder();
        for (int i = bitsetString.Length - 1; i >= 0; i--)
        {

            if (i > 0 && (i + 1) % 4 == 0) { sb.Append(' '); }
            sb.Append(bitsetString[(bitsetString.Length - 1) - i]);
        }
        string result = sb.ToString();

        return result;
    }

    // ********************************************************************************************
    // ********************************************************************************************



    // OPERATORS **********************************************************************************
    // ********************************************************************************************
    public static Bitset operator &(Bitset left, Bitset right)
    {
        if (left.m_bitsetSize != right.m_bitsetSize) return left;

        for (int i = 0; i < left.GetBitset().Length; i++)
        {
            left.GetBitset()[i] &= right.GetBitset()[i];
        }
        return left;
    }

    public static Bitset operator |(Bitset bits, Bitset other)
    {
        if (bits.m_bitsetSize != other.m_bitsetSize) return null;

        for (int i = 0; i < bits.GetBitset().Length; i++)
        {
            bits.GetBitset()[i] |= other.GetBitset()[i];
        }
        return bits;
    }

    // STATIC FUNCTIONS ***************************************************************************
    // ********************************************************************************************

    public static bool DoesBitsetMatchOther(Bitset bitset, Bitset other)
    {
        if (bitset.m_bitsetSize != other.m_bitsetSize) return false;

        for (int i = 0; i < bitset.m_bitsetSize / 32 + 1; i++)
        {
            if(bitset[i] != other[i])
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
