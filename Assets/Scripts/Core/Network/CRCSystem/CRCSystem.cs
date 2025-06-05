using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // For .All()

public enum EGeneratingPolynomial
{
    X8_X2_X1_1,
    X3_X2_X1_1, // "x³+x²+x+1"; Represents 1111 in binary
    X3_1,       // x³+1; Represents 1001 in binary
}

public class CRCSystem : MonoBehaviour
{
    private string _dataBytesStr; // Not directly used in the calculation methods here, but kept for consistency
    private string _generating_polynomial_str; // Store the polynomial string

    public string DataBytesStr
    {
        set { _dataBytesStr = value; }
    }

    void Awake()
    {
        _generating_polynomial_str = "x³+x²+x+1"; // Default, can be set via dropdown in CRCController
    }

    /// <summary>
    /// Converts a polynomial string (e.g., "x³+x²+x+1") to its binary representation (e.g., "1111").
    /// </summary>
    /// <param name="polynomialStr">The polynomial string.</param>
    /// <returns>The binary string representation of the polynomial.</returns>
    public string ConvertPolynomialToBinary(string polynomialStr) // Made public for CRCController to access
    {
        if (polynomialStr == "x8+x2+x+1")
        {
            return "100000111";
        }else 
        if (polynomialStr == "x³+x²+x+1")
        {
            return "1111"; // x^3 + x^2 + x^1 + x^0
        }
        else if (polynomialStr == "x³+1")
        {
            return "1001"; // x^3 + x^0
        }
        Debug.LogError($"未知生成多项式: {polynomialStr}");
        return "";
    }

    /// <summary>
    /// Performs a binary XOR operation on two binary strings of the same length.
    /// </summary>
    /// <param name="a">The first binary string.</param>
    /// <param name="b">The second binary string.</param>
    /// <returns>The result of the XOR operation as a binary string.</returns>
    private string BinaryXOR(string a, string b)
    {
        // IMPORTANT: Assume 'a' and 'b' are already of the same length (divisorLength)
        // This method should not handle padding, padding should be done before calling it.
        if (a.Length != b.Length)
        {
            Debug.LogError("BinaryXOR: Input strings must be of the same length.");
            return ""; // Or throw an exception
        }

        char[] result = new char[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            result[i] = (a[i] == b[i]) ? '0' : '1';
        }
        return new string(result);
    }

    /// <summary>
    /// Performs the CRC division (XOR operations) and records each step for display.
    /// This method is modified to capture the 'paper-like' steps.
    /// </summary>
    /// <param name="data">The data string (padded with zeros).</param>
    /// <param name="divisor">The binary representation of the polynomial.</param>
    /// <param name="steps">A list to store the formatted calculation steps.</param>
    /// <returns>The calculated CRC remainder.</returns>
    public string PerformCRCDivision(string data, string divisor, List<string> steps) // Made public for direct call from CRCController for validation
    {
        int divisorLength = divisor.Length;
        int dataLength = data.Length; // This is the total length of message + appended zeros
        string currentRemainder = data.Substring(0, divisorLength); // This will hold the current remainder/dividend window

        // Initial setup for display
        // The 'data' parameter here is already the padded message (message + zeros)
        string originalMessagePart = data.Substring(0, dataLength - (divisorLength - 1));
        string appendedZerosPart = data.Substring(dataLength - (divisorLength - 1));

        steps.Add($"<b>计算过程：</b>");
        steps.Add($"原始数据 (Message): <color=#00FF00>{originalMessagePart}</color>");
        steps.Add($"附加零: <color=#0000FF>{appendedZerosPart}</color>");
        steps.Add($"处理后的数据 (M(x) + 0s): <color=red>{data}</color>");
        steps.Add($"生成多项式 (Divisor) G(x): <color=yellow>{divisor}</color>");
        steps.Add($"多项式位数 (k): {divisorLength}");
        steps.Add("---");

        // Loop through the data bit by bit, from the point after the initial divisorLength chunk
        for (int i = 0; i < dataLength - (divisorLength - 1); i++)
        {
            steps.Add($"<color=orange>步骤 {i + 1}:</color>");

            // Display the current state of the data being processed, highlighting the window
            string fullDataDisplay = data;
            // The active window is 'currentRemainder' which is always 'divisorLength' long
            // Calculate the start index of the active window in the original data string
            int activeWindowStartIndex = i;
            // Pad the 'active window' with leading spaces to align with the 'fullDataDisplay'
            string leadingSpaces = new string(' ', activeWindowStartIndex);

            // Display the current 'window' of the data and divisor for visual clarity
            steps.Add($"  被除数 (当前窗口): {fullDataDisplay.Substring(0, activeWindowStartIndex)}<color=#00ffffff>{fullDataDisplay.Substring(activeWindowStartIndex, divisorLength)}</color>{fullDataDisplay.Substring(activeWindowStartIndex + divisorLength)}");
            steps.Add($"  当前余数 (处理前): <color=purple>{currentRemainder}</color>");


            if (currentRemainder[0] == '1')
            {
                // Perform XOR operation if the leading bit is 1
                string xorResult = BinaryXOR(currentRemainder, divisor);
                steps.Add($"  除数 (G(x)):     {leadingSpaces}<color=yellow>{divisor}</color>");
                steps.Add($"  ---");
                steps.Add($"  XOR结果:         {leadingSpaces}<color=white>{xorResult}</color>");
                currentRemainder = xorResult;
            }
            else
            {
                // If leading bit is 0, effectively XOR with all zeros
                string zeros = new string('0', divisorLength);
                steps.Add($"  除数 (0...0):    {leadingSpaces}<color=yellow>{zeros}</color>");
                steps.Add($"  ---");
                steps.Add($"  XOR结果:         {leadingSpaces}<color=white>{currentRemainder}</color>"); // Result is the same
            }

            // Remove leading zeros from the result of XOR
            int firstOneIndex = currentRemainder.IndexOf('1');
            string effectiveRemainder = "";
            if (firstOneIndex != -1)
            {
                effectiveRemainder = currentRemainder.Substring(firstOneIndex);
            }
            else
            {
                effectiveRemainder = "0"; // All zeros, so effective remainder is 0
            }

            steps.Add($"  当前余数 (XOR后): <color=green>{effectiveRemainder}</color>");

            // Append the next bit from the original data, if available
            // The next bit is at index (i + divisorLength) in the original 'data' string
            if (i + divisorLength < dataLength)
            {
                string nextBit = data[i + divisorLength].ToString();
                currentRemainder = effectiveRemainder + nextBit;
                // Pad with leading zeros to maintain 'divisorLength' for the next iteration
                // This is crucial for consistent division window size
                currentRemainder = currentRemainder.PadLeft(divisorLength, '0');
                steps.Add($"  附加下一个数据位 '{nextBit}'，新被除数: <color=yellow>{currentRemainder}</color>");
            }
            else
            {
                // This is the last iteration, the currentRemainder is the final CRC
                // Ensure it's padded to (divisorLength - 1) for the final result
                currentRemainder = currentRemainder.Substring(currentRemainder.Length - (divisorLength - 1)).PadLeft(divisorLength - 1, '0');
                steps.Add($"  已处理完所有数据位，最终余数准备就绪。");
            }
            steps.Add("---");
        }

        // The final currentRemainder should be the CRC, padded to (divisorLength - 1)
        // Ensure it's trimmed and padded correctly one last time
        string finalCRC = currentRemainder.TrimStart('0');
        if (finalCRC.Length < divisorLength - 1)
        {
            finalCRC = finalCRC.PadLeft(divisorLength - 1, '0');
        }
        else if (finalCRC.Length > divisorLength - 1)
        {
            // This should ideally not happen with correct logic, but as a safeguard
            finalCRC = finalCRC.Substring(finalCRC.Length - (divisorLength - 1));
        }

        steps.Add($" 最终余数 (CRC码): <b><color=red>{finalCRC}</color></b>");
        return finalCRC;
    }


    /// <summary>
    /// Gets the detailed steps of the CRC calculation for a message.
    /// </summary>
    /// <param name="baseData">The original binary data string.</param>
    /// <param name="polynomialStr">The polynomial string (e.g., "x³+x²+x+1").</param>
    /// <returns>A list of strings, each representing a step in the CRC calculation.</returns>
    public List<string> getCRCSteps(string baseData, string polynomialStr)
    {
        List<string> steps = new List<string>();

        string binaryPolynomial = ConvertPolynomialToBinary(polynomialStr);
        if (string.IsNullOrEmpty(binaryPolynomial))
        {
            steps.Add($"<color=red>错误: 未知的生成多项式 '{polynomialStr}'</color>");
            return steps;
        }

        int polynomialDegree = binaryPolynomial.Length - 1;

        steps.Add($"<size=24><b>CRC 校验步骤详情</b></size>");
        steps.Add($"原始数据 (Message): <color=green>{baseData}</color>");
        steps.Add($"生成多项式 (Polynomial): <color=yellow>{polynomialStr}</color> (二进制: <color=yellow>{binaryPolynomial}</color>)");
        steps.Add($"CRC 码位数 (多项式阶数): {polynomialDegree} 位");
        steps.Add("---");

        // Step 1: Append zeros to the original data
        string paddedData = baseData + new string('0', polynomialDegree);
        steps.Add($"步骤 1: 在原始数据 <color=green>'{baseData}'</color> 后附加 <color=blue>{polynomialDegree}</color> 个 '0'。");
        steps.Add($"   <color=#00ffffff>处理后的数据:</color> <color=green>{paddedData}</color>");
        steps.Add("---");

        // Step 2: Perform division and get remainder
        PerformCRCDivision(paddedData, binaryPolynomial, steps);

        // The final remainder is already added by PerformCRCDivision
        return steps;
    }

    /// <summary>
    /// Validates a received data stream (message + CRC) using the specified polynomial.
    /// </summary>
    /// <param name="receivedDataWithCRC">The complete data string including the message and the received CRC.</param>
    /// <param name="polynomialStr">The polynomial string.</param>
    /// <returns>True if the remainder is all zeros, indicating a valid transmission; otherwise, false.</returns>
    public bool validateCRC(string receivedDataWithCRC, string polynomialStr)
    {
        List<string> tempSteps = new List<string>(); // For validation, we don't need to log steps to UI
        string binaryPolynomial = ConvertPolynomialToBinary(polynomialStr);
        if (string.IsNullOrEmpty(binaryPolynomial)) return false;

        // When validating, the input 'receivedDataWithCRC' is already the complete data.
        // We do NOT append zeros here. Just directly divide.
        string remainder = PerformCRCDivision(receivedDataWithCRC, binaryPolynomial, tempSteps); // Use tempSteps

        // A valid CRC means the final remainder should be all zeros.
        bool isValid = remainder.All(c => c == '0');
        return isValid;
    }
}