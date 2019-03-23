using System;
using System.Collections.Generic;

public class Common
{
    public static void Assert(bool cond, string errormsg)
    {
        if (!cond)
        {
            Console.WriteLine("[ASSERT!] " + errormsg);
        }
    }
}

public class ASWakeman
{
    ASWakeman m_top = null;
    ASWakeman m_bottom = null;
    private int m_myGateSize;
    private int m_mySize;
    private bool[] m_inputs;
    private bool[] m_outputs;
    private bool[] m_myGates;

    public int myGateSize()
    {
        return m_myGateSize;
    }

    public int mySize()
    {
        return m_mySize;
    }

    public string export()
    {
        calculate();
        string returnValue = "";
        for (int i = 0; i < m_mySize; ++i)
        {
            if (m_outputs[i] == false)
            { returnValue += '0'; }
            else
            { returnValue += '1'; }
        }
        return returnValue;
    }

    public void calculate()
    {
        calculate_internal();
    }

    public void SetInputs(bool[] inputs)
    {
        Common.Assert(inputs.Length == m_inputs.Length, "Set Inputs does not match in length");
    }

    public bool[] GetOutputs()
    {
        return m_outputs;
    }

    public bool[] calculate_internal()
    {
        // edge cases for like 0 and 1
        if (mySize() == 0)
        {
            return m_outputs;
        }

        if (mySize() == 1)
        {
            m_outputs[0] = m_inputs[0];
            return m_outputs;
        }

        if (mySize() == 2)
        {
            Common.Assert(m_myGates.Length == 1, "2 inputs and many gates error!");
            if (m_myGates[0] == true)
            {
                m_outputs[0] = m_inputs[1];
                m_outputs[1] = m_inputs[0];
            }
            else
            {
                m_outputs[0] = m_inputs[0];
                m_outputs[1] = m_inputs[1];
            }
            return m_outputs;
        }

        if (mySize() == 3)
        {
            Common.Assert(m_myGates.Length == 3, "3 inputs and wrong gates error!");
            if (m_myGates[0] == true)
            {
                m_outputs[0] = m_inputs[1];
                m_outputs[1] = m_inputs[0];
            }
            else
            {
                m_outputs[0] = m_inputs[0];
                m_outputs[1] = m_inputs[1];
            }

            m_outputs[2] = m_inputs[2];

            return m_outputs;
        }

        // first pass
        int numOfGates = (m_mySize / 2);
        bool[] tmpinput = new bool[m_mySize];
        /* // hardcoded test
        tmpinput[0] = m_inputs[0];
        tmpinput[1] = m_inputs[4];
        tmpinput[2] = m_inputs[1];
        tmpinput[3] = m_inputs[5];

        tmpinput[4] = m_inputs[2];
        tmpinput[5] = m_inputs[6];
        tmpinput[6] = m_inputs[3];
        tmpinput[7] = m_inputs[7];
        tmpinput[8] = m_inputs[8];
        */
        // wiring
        for (int i = 0; i < m_mySize; ++i)
        {
            if (i % 2 == 0) // if its even
            {
                tmpinput[i] = m_inputs[i / 2]; // copy it 
            }
            else if (i == m_mySize - 1 && i % 2 == 1) //todo: else edge odd case
            {
                tmpinput[i] = m_inputs[i]; // copy it 
            }
            else // odd case
            {
                int halfsize = m_mySize / 2;
                int finalcal = m_mySize - halfsize + i - 1;
                if (finalcal >= m_mySize) break; // overshot again
                tmpinput[i] = m_inputs[finalcal]; // copy it 
            }
        }

        // first gate check
        for (int i = 0; i < numOfGates; ++i)
        {
            if (m_myGates[i] == true)
            {
                bool tmp = tmpinput[i * 2];
                tmpinput[i * 2] = tmpinput[i * 2 + 1];
                tmpinput[i * 2 + 1] = tmp;
            }
        }

        // todo: handle the odd gate flip and single passthrough middle gate check

        // sending it down into recusive
        if (m_top != null)
        {
            bool[] tmpinputtop = new bool[m_top.mySize()];
            for (int i = 0; i < m_top.mySize(); ++i)
            {
                tmpinputtop[i] = tmpinput[i];
            }
            m_top.SetInputs(tmpinputtop);
            m_top.calculate_internal();
        }

        if (m_bottom != null)
        {
            bool[] tmpinputbot = new bool[m_bottom.mySize()];
            for (int i = 0; i < m_bottom.mySize(); ++i)
            {
                int topsize = 0;
                if (m_top != null) topsize = m_top.mySize();
                int offset = i + topsize;
                if (offset >= tmpinput.Length) break;
                tmpinputbot[i] = tmpinput[offset];
            }
            m_bottom.SetInputs(tmpinputbot);
            m_bottom.calculate_internal();
        }

        // extraction of data from internal structures
        if (m_top != null)
        {
            bool[] tmpoutputtop = m_top.GetOutputs();
            for (int i = 0; i < m_top.mySize(); ++i)
            {
                tmpinput[i] = tmpoutputtop[i];
            }
        }
        else
        {
            Common.Assert(false, "top failed? " + mySize());
        }

        if (m_bottom != null)
        {
            bool[] tmpoutputbot = m_bottom.GetOutputs();
            int topsize = 0;
            if (m_top != null) topsize = m_top.mySize();
            for (int i = 0; i < m_bottom.mySize(); ++i)
            {
                tmpinput[i + topsize] = tmpoutputbot[i];
            }
        }
        else
        {
            Common.Assert(false, "btm failed? " + mySize());
        }

        // last gate check
        for (int i = 0; i < numOfGates; ++i)
        {
            if (m_myGates[i + numOfGates - 1] == true)
            {
                bool tmp = tmpinput[i * 2];
                tmpinput[i * 2] = tmpinput[i * 2 + 1];
                tmpinput[i * 2 + 1] = tmp;
            }
        }

        // copy to outputs
        for (int i = 0; i < m_outputs.Length; ++i)
        {
            m_outputs[i] = tmpinput[i];
        }

        return m_outputs;
    }

    public int recalc(int a)
    {
        if (a < 2)
        {
            return 0;
        }

        bool isOdd = (a % 2 != 0);
        if (isOdd)
        {
            return (recalc((a / 2) + 1) + recalc(a / 2) + a - 1);
        }
        return (recalc(a / 2) + recalc(a / 2) + a - 1);
    }

    private void init(int a, string inputs, string gates)
    {
        m_myGateSize = recalc(a);
        m_mySize = a;

        m_myGates = new bool[m_myGateSize];
        m_inputs = new bool[m_mySize];
        m_outputs = new bool[m_mySize];

        if (inputs.Length != 0)
        {
            Common.Assert(a == inputs.Length, "inputs length is wrong");
            for (int i = 0; i < inputs.Length; ++i)
            {
                if (inputs[i] == '0')
                { m_inputs[i] = false; }
                else
                { m_inputs[i] = true; }
            }
        }

        if (gates.Length != 0)
        {
            Common.Assert(m_myGateSize == gates.Length, "" + a + " gates length is wrong " + m_myGateSize);
            for (int i = 0; i < gates.Length; ++i)
            {
                if (gates[i] == '0')
                { m_myGates[i] = false; }
                else
                { m_myGates[i] = true; }
            }
        }

        // recursive creation
        int topsize = a / 2;
        int botsize = (a % 2 == 0) ? a / 2 : a / 2 + 1;
        int roundeddowngates = ((a / 2) * 2);
        int leftovergates = m_myGateSize - roundeddowngates;
        if (leftovergates > 0)
        {
            // gates
            //Console.WriteLine("gazie: " + m_myGateSize + " " + a);
            //Console.WriteLine("create: " + a);
            int topgatesize = recalc(topsize);
            bool[] topgates = new bool[topgatesize];
            if (topgatesize == 0)
            {
                // todo: nothing?
                //Console.WriteLine("topsize end:" + botsize);
            }
            else
            {
                for (int i = 0; i < topgatesize; ++i)
                {
                    int offsettedcount = i + roundeddowngates - 1;
                    //Console.WriteLine("top: " + offsettedcount + " into " + i);
                    topgates[i] = m_myGates[offsettedcount];
                }
            }

            int bottomgatesize = recalc(botsize);
            bool[] bottomgates = new bool[bottomgatesize];
            if (bottomgatesize == 0)
            {
                // todo: nothing?
                //Console.WriteLine("botsize end:" + botsize);
            }
            else
            {
                for (int i = 0; i < bottomgatesize; ++i)
                {
                    int offsettedcount = i + roundeddowngates - 1 + topgatesize;
                    //Console.WriteLine("bottom: " + offsettedcount + " into " + i);
                    bottomgates[i] = m_myGates[offsettedcount];
                }
            }


            // inputs
            bool[] topinputs = new bool[topsize];
            if (topsize == 0)
            {
                // todo: nothing?
            }
            else
            {
                for (int i = 0; i < topsize; ++i)
                {
                    int offset = i * 2;
                    topinputs[i] = m_inputs[offset];
                }
            }

            bool[] bottominputs = new bool[botsize];
            if (botsize == 0)
            {
                // todo: nothing?
            }
            else
            {
                for (int i = 0; i < botsize; ++i)
                {
                    int offset = i * 2 + 1;
                    if (offset >= m_inputs.Length) break; // IMPT: do not overshot the array
                    bottominputs[i] = m_inputs[offset];
                }
            }

            if (topsize > 1) // todo: is this correct logic?
            {
                m_top = new ASWakeman(topsize, topinputs, topgates);
            }
            if (botsize > 1) // todo: is this correct logic?
            {
                m_bottom = new ASWakeman(botsize, bottominputs, bottomgates);
            }
        }
    }
    public ASWakeman(int a)
    {
        init(a, "", "");
    }

    public ASWakeman(int a, string inputs)
    {
        init(a, inputs, "");
    }

    public ASWakeman(int a, bool[] inputs, bool[] gates)
    {
        string inputstring = "";
        for (int i = 0; i < inputs.Length; ++i)
        {
            if (inputs[i] == false)
            { inputstring += '0'; }
            else
            { inputstring += '1'; }
        }

        string gatestring = "";
        for (int i = 0; i < gates.Length; ++i)
        {
            if (gates[i] == false)
            { gatestring += '0'; }
            else
            { gatestring += '1'; }
        }
        init(a, inputstring, gatestring);
    }

    public ASWakeman(int a, string inputs, string gates)
    {
        init(a, inputs, gates);
    }
}

class MainClass
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello World");
        int[] arrayoftest = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 16, 32, 40 };
        for (int i = 0; i < arrayoftest.Length; ++i)
        {
            ASWakeman as0 = new ASWakeman(arrayoftest[i]);
            Console.WriteLine("as " + arrayoftest[i] + " " + as0.mySize() + " " + as0.export());
        }

        ASWakeman hard1 = new ASWakeman(8, "00001111", "11111111111111111");
        Console.WriteLine("usecase: " + hard1.export());

        ASWakeman hard2 = new ASWakeman(8, "01010101", "00000000000000000");
        Console.WriteLine("usecase: " + hard2.export());

        ASWakeman hard3 = new ASWakeman(8, "00001111", "11110000111111110");
        Console.WriteLine("usecase: " + hard3.export());

        Console.WriteLine("all good!");

        string a = Console.ReadLine();
    }
}
