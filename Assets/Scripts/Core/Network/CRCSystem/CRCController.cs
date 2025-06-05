using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CRCController : MonoBehaviour
{
    [Header("CRC 输入")]

    [Tooltip("提示按钮")][SerializeField] private Button toolTipButton;

    [Tooltip("输入：数据 Input")][SerializeField] private TMP_InputField inputDataField;
    [Tooltip("输入：选择的生成多项式")][SerializeField] private TMP_Dropdown polynomialDropdown;

    [Tooltip("输入： 用户输入的CRC校验的答案作答")][SerializeField] private TMP_InputField userAnswerDataField;


    [Tooltip("按钮：点击提交输入，查看计算的结果与步骤")][SerializeField] private Button CommitButton;

    [Tooltip("按钮：点击校验答案")][SerializeField] private Button TryButton;

    [Header("步骤展示区")]
    [Tooltip("输出： 系统输出的计算CRC校验的步骤")][SerializeField] private TextMeshProUGUI StepStringOutput;
    [Tooltip("输入：系统输出的计算CRC校验的答案")][SerializeField] private TextMeshProUGUI ComputedAnswerOutput;

    private CRCSystem crcSystem; // Reference to the CRCSystem

    string InputData
    {
        get { return inputDataField.text.Trim(); }
    }
    string UserAnswerData
    {
        get { return userAnswerDataField.text.Trim(); }
    }

    string SelectedPolynomial
    {
        get
        {
            if (polynomialDropdown.options.Count > 0)
            {
                return polynomialDropdown.options[polynomialDropdown.value].text;
            }
            return "";
        }
    }

    private void Awake()
    {
        Setup();
        crcSystem = FindObjectOfType<CRCSystem>();
        if (crcSystem == null)
        {
            Debug.LogError("CRCSystem not found in the scene! Please add it to a GameObject or add component dynamically.");
            crcSystem = gameObject.AddComponent<CRCSystem>();
        }
    }

    #region 界面配置
    private void Setup()
    {
        toolTipButton.onClick.AddListener(() =>
        {
             UIManager.Instance.OpenPopup("CRC-KnowledgeExplain");
        });

        // Populate polynomial dropdown
        polynomialDropdown.ClearOptions();
        List<string> polynomialOptions = new List<string>
        {
            "x8+x2+x+1",
            "x³+x²+x+1", // Corresponds to EGeneratingPolynomial.X3_X2_X1_1
            "x³+1"       // Corresponds to EGeneratingPolynomial.X3_1
        };
        polynomialDropdown.AddOptions(polynomialOptions);

        CommitButton.onClick.AddListener(() =>
        {
            if (!VerifyInputIsBinary(InputData))
            {
                // UIManager.Instance.OpenPopup("BinaryErrorPopUp");
                Debug.LogError("输入数据包含非二进制字符！请只输入 '0' 或 '1'。");
                StepStringOutput.text = "<color=red>错误: 输入数据必须是二进制 (只包含 '0' 和 '1')。</color>";
                ComputedAnswerOutput.text = "N/A";
                return;
            }

            StartCoroutine(ComputedStepCRC());
        });

        TryButton.onClick.AddListener(() =>
        {
            ValidateUserAnswer();
        });
    }
    #endregion

    bool VerifyInputIsBinary(string input)
    {
        return input.All(c => c == '0' || c == '1');
    }

    IEnumerator ComputedStepCRC()
    {
        StepStringOutput.text = "计算中...";
        ComputedAnswerOutput.text = "计算中...";

        string computeData = InputData;
        string polyData = SelectedPolynomial;

        List<string> steps = crcSystem.getCRCSteps(computeData, polyData);

        // 从步骤列表中提取最终计算出的CRC码
        // 假设CRCSystem.getCRCSteps 的最后一行是最终CRC码
        string computedCRC = "Error";
        string lastStep = steps.LastOrDefault();
        if (lastStep != null && lastStep.Contains("最终余数 (CRC码):"))
        {
            computedCRC = lastStep.Split(':')[1].Trim();
            computedCRC = System.Text.RegularExpressions.Regex.Replace(computedCRC, "<.*?>", string.Empty); // Remove color tags
        }

        StepStringOutput.text = ""; // Clear for display
        foreach (string step in steps)
        {
            StepStringOutput.text += step + "\n";
            yield return new WaitForSeconds(0.05f); // Reduced delay for faster display
        }

        ComputedAnswerOutput.text = $"<color=green>{computedCRC}</color>";

        yield return null;
    }

    private void ValidateUserAnswer()
    {
        string inputData = InputData;
        string userAnswer = UserAnswerData;
        string selectedPoly = SelectedPolynomial;

        if (!VerifyInputIsBinary(inputData) || !VerifyInputIsBinary(userAnswer))
        {
            Debug.LogError("输入数据或用户答案包含非二进制字符！");
            StepStringOutput.text = "<color=red>错误: 输入数据或用户答案必须是二进制。</color>";
            ComputedAnswerOutput.text = "N/A";
            return;
        }

        // 拼接原始数据和用户提交的CRC答案
        string dataToValidate = inputData + userAnswer;

        // 获取多项式的二进制形式和阶数
        string binaryPolynomial = crcSystem.ConvertPolynomialToBinary(selectedPoly);
        if (string.IsNullOrEmpty(binaryPolynomial))
        {
            Debug.LogError("无法获取多项式二进制形式进行验证。");
            StepStringOutput.text = "<color=red>错误: 无法解析生成多项式。</color>";
            return;
        }

        int polynomialDegree = binaryPolynomial.Length - 1;

        // 进行校验计算，并获取校验过程的步骤
        List<string> validationSteps = new List<string>();
        // 调用PerformCRCDivision直接进行校验计算
        string remainder = crcSystem.PerformCRCDivision(dataToValidate, binaryPolynomial, validationSteps);

        bool isValid = remainder.All(c => c == '0'); // 余数全为0则校验通过

        StepStringOutput.text += "\n\n--- 校验您的答案的计算过程 ---";
        foreach (string step in validationSteps)
        {
            StepStringOutput.text += "\n" + step;
        }

        if (isValid)
        {
            ComputedAnswerOutput.text = $"<color=green>√ 正确！CRC校验通过。</color>";
            StepStringOutput.text += $"\n<b><color=orange>校验最终余数:</color> <color=green>{remainder}</color></b>";
            StepStringOutput.text += $"\n<b><color=orange>校验结果:</color> <color=green>通过</color></b>";
        }
        else
        {
            ComputedAnswerOutput.text = $"<color=red>× 错误！</color>";
            StepStringOutput.text += $"\n<b><color=orange>校验最终余数:</color> <color=red>{remainder}</color></b>";
            StepStringOutput.text += $"\n<b><color=orange>校验结果:</color> <color=red>失败</color></b>";
        }
    }
}