using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Globalization;
using CG.Data.DataSource;

namespace CG.Communication.PLC.MITSUBISH.Service
{
    public struct NumberParseResult
    {
        public bool IsSuccess{get;set;}
        public int Value{get;set;}
    }
    public class FXComplier
    {
        private static SourceCodeLine m_nowProcessLine;         //当前处理的代码行

        public static List<SourceCodeLine> ConvertSourceToLineList(string wholeSourceCodeStr)
        {
            List<SourceCodeLine> resultList = new List<SourceCodeLine>();
            string[] sourceLines = wholeSourceCodeStr.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < sourceLines.Length; i++)
            {
                SourceCodeLine codeLine = new SourceCodeLine();
                codeLine.BasicLineStr = sourceLines[i].ToUpper();
                string[] lineElements = codeLine.BasicLineStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int index = 0;
                bool result = int.TryParse(lineElements[0], out index);
                codeLine.Index = index;
                int startIndex = result ? 1 : 0;
                codeLine.Cmd = lineElements[startIndex];
                for (int j = startIndex + 1; j < lineElements.Length; j++)
                {
                    codeLine.Operands.Add(lineElements[j]);
                }
                resultList.Add(codeLine);
            }
            return resultList;
        }

        public static string ComplieCmdStr(List<SourceCodeLine> sourceCodeLineList, string fxModel)
        {
            Dictionary<string, IFXCmd> cmdList = FXConfigReader.Cmds[fxModel];
            Dictionary<string, FXRegister> registerList = FXConfigReader.Registers[fxModel];
            List<OperandMap> mapList = FXConfigReader.OperandMaps[fxModel];
            string result = "";
            for (int i = 0; i < sourceCodeLineList.Count; i++)
            {
                //if (line.Index == 522)
                //    Console.WriteLine("");
                //Console.WriteLine(line.Index.ToString());
                SourceCodeLine line = sourceCodeLineList[i];
                FXComplier.m_nowProcessLine = line;
                IFXCmd iCmd = FXComplier.GetCmdFromDictionary(ref line, cmdList);
                string oneLineCmdStr = GetOneLineCmdStr(iCmd, line.Operands, registerList, mapList);
                if (oneLineCmdStr != null && oneLineCmdStr != string.Empty)
                {
                    string[] oneLineCmdParts = oneLineCmdStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string oneLineCmdPart in oneLineCmdParts)
                    {
                        result = result + " " + Utils.Switch2StringParts(oneLineCmdPart, 2);
                    }
                }
                else
                    throw new Exception("Complie Error. At " + FXComplier.m_nowProcessLine.BasicLineStr);
            }
            return result.Substring(1, result.Length - 1);
        }

        /// <summary>
        ///  将一个软元件符号分解为"类型"+"编号"的形式，如M800=>M,800
        ///  也有可能是K4M100之类的组合操作数，这时候类型依然为M，编号依然为100，但OperandDetails.Bits=4*4
        /// </summary>
        /// <param name="operandStr">形如Y001的软元件编号</param>
        /// <returns>返回OperandDetails实例</returns>
        private static OperandDetails GetOperandDetails(string operandStr, List<OperandMap> mapList)
        {
            List<int> letterIndex = new List<int>();
            letterIndex.Add(0);
            for (int i = 1; i < operandStr.Length; i++)
            {
                char tempChar = operandStr[i];
                if (!((0x30 <= tempChar && 0x39 >= tempChar) || 0x2D == tempChar))
                    letterIndex.Add(i);
            }
            OperandDetails details = new OperandDetails();
            if (letterIndex.Count == 1)         //单一操作数
            {
                details.Name = operandStr.Substring(0, letterIndex[0] + 1);
                details.Value = operandStr.Substring(letterIndex[0] + 1, operandStr.Length - letterIndex[0] - 1);
                //int value = GetKHConstantValue(details.Name, details.Value);
                //if (value != -1)
                //    details.Value = value.ToString("X");
                details.Bits = -1;
            }
            else if (letterIndex.Count == 2)       //iron
            {
                string constName = operandStr.Substring(0, letterIndex[0] + 1);
                string constValue = operandStr.Substring(letterIndex[0] + 1, letterIndex[1] - letterIndex[0] - 1);
                string operandName = operandStr.Substring(letterIndex[1], 1);
                string operandValue = operandStr.Substring(letterIndex[1] + 1, operandStr.Length - letterIndex[1] - 1);
                NumberParseResult bitsResult = GetKHConstantValue(constName, constValue);
                details.Bits = bitsResult.Value * 4;
                details.Name = operandName;
                details.Value = operandValue;
            }
            DoRegisterMapping(mapList, ref details);
            return details;
        }
        private static void DoRegisterMapping(List<OperandMap> mapLst, ref OperandDetails details)
        {
            foreach (OperandMap map in mapLst)
            {
                if ((!details.Name.Equals(FXConfigReader.HCONSTANT, StringComparison.CurrentCultureIgnoreCase)) && (!details.Name.Equals(FXConfigReader.KCONSTANT, StringComparison.CurrentCultureIgnoreCase)))
                {
                    int detailsValue = Convert.ToInt32(details.Value, 16);
                    bool isValueInRange = (detailsValue >= map.From) & (detailsValue <= map.To);
                    bool isSameName = details.Name.Equals(map.Name);
                    if (isSameName && isValueInRange)
                    {
                        details.Name = map.ReName;
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 获得基本指令的操作数的绝对地址，以10进制的数字表示
        /// </summary>
        /// <param name="details">操作数的具体信息</param>
        /// <param name="registerList">寄存器对照表</param>
        /// <returns></returns>
        private static NumberParseResult GetBasicCmdOperandAbsAddr(OperandDetails details, Dictionary<string, FXRegister> registerList)
        {
            NumberParseResult result = GetKHConstantValue(details.Name, details.Value);
            if (!result.IsSuccess)
            {
                if (registerList.ContainsKey(details.Name))
                {
                    FXRegister register = registerList[details.Name];
                    int relative = Convert.ToInt32(details.Value, (int)register.NameFormatType);
                    result.Value = relative - register.From + register.AddrFrom + register.Offset;
                    result.IsSuccess = true;
                }
                else
                    throw new Exception("Operand not exist. At " + FXComplier.m_nowProcessLine.BasicLineStr);
            }
            return result;
        }
        /// <summary>
        /// 根据指定的命令名称从命令字典中取出相应的命令内容
        /// 判断的顺序为：存在于字典中，如果以D开头则去掉D存在于字典中，如果以P结尾则去掉P存在与字典中，如果以D开头并以P结尾则分别去掉DP后是否存在于字典
        /// </summary>
        /// <param name="cmdName">命令名称</param>
        /// <param name="cmdList">命令字典</param>
        /// <returns></returns>
        private static IFXCmd GetCmdFromDictionary(ref SourceCodeLine line, Dictionary<string, IFXCmd> cmdList)
        {
            if (cmdList.ContainsKey(line.Cmd))           //如果已经存在于字典中，那么直接返回
                return cmdList[line.Cmd];
            else
            {
                if (line.Cmd.StartsWith(FXConfigReader.PCMDPREFIX))       //如果命令以P开头，那么为指针命令
                {
                    string numStr = line.Cmd.Substring(1, line.Cmd.Length - 1);
                    if (cmdList.ContainsKey(FXConfigReader.PCMDPREFIX))
                    {
                        FXBasicCmd cmd = (FXBasicCmd)cmdList[FXConfigReader.PCMDPREFIX];
                        line.Operands.Add(FXConfigReader.KCONSTANT + numStr);
                        return cmd;
                    }
                }
                if (line.Cmd.StartsWith(FXConfigReader.DCMDPREFIX))      //如果命令由字母D开头，那么尝试去掉D后是否存在于字典中
                {
                    string tempName = line.Cmd.Substring(1, line.Cmd.Length - 1);
                    if (cmdList.ContainsKey(tempName))
                    {
                        FXAppCmd cmd = (FXAppCmd)cmdList[tempName];
                        cmd.AppCmdType = FXAppCmdType.D;
                        return cmd;
                    }
                }
                if (line.Cmd.EndsWith(FXConfigReader.PCMDSUFFIX))       //如果命令由字母P结尾，那么尝试去掉P后是否存在于字典中
                {
                    string tempName = line.Cmd.Substring(0, line.Cmd.Length - 1);
                    if (cmdList.ContainsKey(tempName))
                    {
                        FXAppCmd cmd = (FXAppCmd)cmdList[tempName];
                        cmd.AppCmdType = FXAppCmdType.P;
                        return cmd;
                    }
                }
                if (line.Cmd.StartsWith(FXConfigReader.DCMDPREFIX) && line.Cmd.EndsWith(FXConfigReader.PCMDSUFFIX))         //如果命令既由D开头，又有P结尾，那么尝试都去掉后是否存在于字典中
                {
                    string tempName = line.Cmd.Substring(1, line.Cmd.Length - 2);
                    if (cmdList.ContainsKey(tempName))
                    {
                        FXAppCmd cmd = (FXAppCmd)cmdList[tempName];
                        cmd.AppCmdType = FXAppCmdType.DP;
                        return cmd;
                    }
                }
                //如果到这还没返回，说明只剩最后一种可能情况，即是D类型的接点比较命令(以>,<,=,<=,>=结尾，这种命令没有P类型的)，这类的命令D标识存在于原命令后，大小比较符前
                string tryContactCompareCmd = line.Cmd.Replace(">", "").Replace("<", "").Replace("=", "");
                if (tryContactCompareCmd.LastIndexOf(FXConfigReader.DCMDPREFIX) == tryContactCompareCmd.Length - 1)  //如果去掉可能的比较符号后的字符串的最后字符为D
                {
                    string tempName = line.Cmd.Substring(0, tryContactCompareCmd.Length - 1) + line.Cmd.Substring(tryContactCompareCmd.Length, line.Cmd.Length - tryContactCompareCmd.Length);
                    if (cmdList.ContainsKey(tempName))
                    {
                        FXAppCmd cmd = (FXAppCmd)cmdList[tempName];
                        cmd.AppCmdType = FXAppCmdType.D;
                        return cmd;
                    }
                }
                throw new Exception("No such command " + line.Cmd + ". At " + FXComplier.m_nowProcessLine.BasicLineStr);
            }
        }

        private static string GetOneLineCmdStr(IFXCmd cmd, List<string> paramArray, Dictionary<string, FXRegister> registerList, List<OperandMap> mapList)
        {
            string result = null;
            OperandDetails[] operandDetails = new OperandDetails[paramArray.Count];
            string[] paramNameArray = new string[paramArray.Count];
            for (int i = 0; i < paramArray.Count; i++)
            {
                operandDetails[i] = GetOperandDetails(paramArray[i], mapList);
                paramNameArray[i] = operandDetails[i].Name;
            }
            if (cmd.CmdType == FXCmdType.Basic)         //如果是基本型的命令
            {
                FXBasicCmd basicCmd = (FXBasicCmd)cmd;
                FXBasicCmdCharacter character = GetBasicCmdCharacter(basicCmd, paramNameArray);
                result = ComplieBasicCmdStr(character, operandDetails, registerList);
            }
            else        //如果是应用型的命令
            {
                FXAppCmd appCmd = (FXAppCmd)cmd;
                result = ComplieAppCmdStr(appCmd, operandDetails, registerList);
            }
            return result;
        }

        private static string ComplieBasicCmdStr(FXBasicCmdCharacter character, OperandDetails[] detailsArray, Dictionary<string, FXRegister> registerList)
        {
            if (character.BasicCmdType == FXBasicCmdType.PureSingle)        //如果是纯单字指令，那么需要判断是否有ppp占位符
            {
                return character.Value;
            }
            if (character.BasicCmdType == FXBasicCmdType.Single || character.BasicCmdType == FXBasicCmdType.Double)
            {
                int absAddr = GetBasicCmdOperandAbsAddr(detailsArray[0], registerList).Value;
                int indexOfLastBlank = character.Value.LastIndexOf(FXConfigReader.BLANKSTR);
                int indexOfPPP = character.Value.IndexOf(FXConfigReader.PPPSTR);
                //int indexOfN = character.Value.IndexOf(FXComplier.NSTR);
                if (indexOfPPP > 0)                                     //如果是ppp表达式，那么从最后一个空格到+ppp之间的就是需要相加的数
                {
                    int index = indexOfPPP;// > 0 ? indexOfPPP : indexOfN;
                    string toBeAddStr = character.Value.Substring(indexOfLastBlank + 1, index - indexOfLastBlank - 1);
                    int resultInt = Convert.ToInt32(toBeAddStr, 16) + absAddr;
                    string resultStr = resultInt.ToString("X");
                    resultStr = Utils.FillStringWithChar(resultStr, FXConfigReader.ZEROCHAR, 4, true);
                    //return indexOfPPP > 0 ? character.Value.Replace(toBeAddStr + FXComplier.PPPSTR, resultStr) : character.Value.Replace(toBeAddStr + FXComplier.NSTR, resultStr);
                    return character.Value.Replace(toBeAddStr + FXConfigReader.PPPSTR, resultStr);
                }
            }
            if (character.BasicCmdType == FXBasicCmdType.Triple || character.BasicCmdType == FXBasicCmdType.Five)
            {
                int[] absAddrArray = new int[detailsArray.Length];
                for (int i = 0; i < detailsArray.Length; i++)
                {
                    absAddrArray[i] = GetBasicCmdOperandAbsAddr(detailsArray[i], registerList).Value;
                }
                string cmdReplaced = ReplacePlaceHolder(character.Value, absAddrArray);
                cmdReplaced = ReplaceDigitIndecaterAndDoAddition(cmdReplaced);
                return cmdReplaced;
            }
            return null;
        }

        private static string ComplieAppCmdStr(FXAppCmd cmd, OperandDetails[] detailsArray, Dictionary<string, FXRegister> registerList)
        {
            int funcValue = (cmd.FuncNO + 8) * 2;           //命令的值
            bool is32bit = false;
            if (cmd.AppCmdType == FXAppCmdType.D)
            {
                funcValue = funcValue + 1;
                is32bit = true;
            }
            if (cmd.AppCmdType == FXAppCmdType.P)
            {
                funcValue = funcValue + 0x1000;
            }
            if (cmd.AppCmdType == FXAppCmdType.DP)
            {
                funcValue = funcValue + 0x1001;
                is32bit = true;
            }
            string valueStr = Utils.FillStringWithChar(funcValue.ToString("X"), FXConfigReader.ZEROCHAR, 4, true);
            foreach (OperandDetails details in detailsArray)
            {
                int valueValue = 0;
                FXRegisterMN mn = GetRegisterMN(details, registerList, ref valueValue);
                if (!is32bit)       //如果是16bit的
                {
                    int valueLow = 0x8000 + 0x100 * mn.M + (valueValue & 0xFF);
                    int valueHigh = 0x8000 + 0x100 * mn.N + ((valueValue & 0xFF00) >> 8);
                    valueStr = valueStr + " " + valueLow.ToString("X") + " " + valueHigh.ToString("X");
                }
                else
                {
                    int valueLowest = 0x8000 + 0x100 * mn.M + (valueValue & 0xFF);
                    int valueLow = 0x8000 + 0x100 * mn.N + ((valueValue & 0xFF00) >> 8);
                    int valueHigh = 0x8000 + ((valueValue & 0xFF0000) >> 16);
                    int valueHighest = 0x8000 + (int)((valueValue & 0xFF000000) >> 24);
                    valueStr = valueStr + " " + valueLowest.ToString("X") + " " + valueLow.ToString("X") + " " + valueHigh.ToString("X") + " " + valueHighest.ToString("X");
                }
            }
            return valueStr;
        }

        private static FXRegisterMN GetRegisterMN(OperandDetails details, Dictionary<string, FXRegister> registerList, ref int valueValue)
        {
            if (details.Name.Equals(FXConfigReader.KCONSTANT, StringComparison.CurrentCultureIgnoreCase))          //如果是K常数
            {
                FXRegisterMN mn = new FXRegisterMN();
                mn.M = 0;
                mn.N = 0;
                mn.From = 0;
                mn.To = 0;
                valueValue = Convert.ToInt32(details.Value, 10);
                return mn;
            }
            if (details.Name.Equals(FXConfigReader.HCONSTANT, StringComparison.CurrentCultureIgnoreCase))          //如果是H常数
            {
                FXRegisterMN mn = new FXRegisterMN();
                mn.M = 2;
                mn.N = 0;
                mn.From = 0;
                mn.To = 0;
                valueValue = Convert.ToInt32(details.Value, 16);
                return mn;
            }
            if (registerList.ContainsKey(details.Name))
            {
                FXRegister register = registerList[details.Name];
                int detailsValue = Convert.ToInt32(details.Value, (int)register.NameFormatType);
                foreach (FXRegisterMN mn in register.MN)
                {
                    if (detailsValue >= mn.From && detailsValue <= mn.To)
                    {
                        if (!mn.IsPoint)
                        {
                            //valueValue = (detailsValue - mn.From + mn.Offset) * 2;
                            valueValue = ((detailsValue - mn.From) * mn.Step + mn.Offset) * 2;
                            return mn;
                        }
                        else
                        {
                            valueValue = detailsValue - register.From + register.AddrFrom + register.Offset;
                            FXRegisterMN tempMN = new FXRegisterMN();
                            tempMN.From = mn.From;
                            tempMN.To = mn.To;
                            tempMN.IsPoint = mn.IsPoint;
                            tempMN.Offset = mn.Offset;
                            tempMN.M = 4;
                            tempMN.N = details.Bits / 2;
                            return tempMN;
                        }
                    }
                }
                throw new Exception("No Such Operand Value! " + details.Value + ". At " + FXComplier.m_nowProcessLine.BasicLineStr);
            }
            else
                throw new Exception("No Such Operand Name! " + details.Name + ". At " + FXComplier.m_nowProcessLine.BasicLineStr);
        }

        private static FXBasicCmdCharacter GetBasicCmdCharacter(FXBasicCmd cmd, string[] paramNameArray)
        {
            if (cmd.Characters.Count == 1)              //如果只有一个Character，那么不用遍历了，直接返回
                return cmd.Characters[0];
            foreach (FXBasicCmdCharacter character in cmd.Characters)         //遍历这个命令下的所有Character
            {
                if (character.BasicCmdType == FXBasicCmdType.Single || character.BasicCmdType == FXBasicCmdType.Double)
                {
                    if (paramNameArray.Length == 1)
                    {
                        bool isFit = IsParamFitAdapter(character, paramNameArray);
                        if (isFit)
                            return character;
                    }
                }
                if (character.BasicCmdType == FXBasicCmdType.Triple || character.BasicCmdType == FXBasicCmdType.Five)
                {
                    bool isFit = IsParamFitAdapter(character, paramNameArray);
                    if (isFit)
                        return character;
                }
            }
            throw new Exception("can not find character that meet the param. At " + FXComplier.m_nowProcessLine.BasicLineStr);
        }
        /// <summary>
        /// 将规则表达式中的占位符，例如{0}之类的替换成参数的值
        /// 参数的个数和占位符的个数必须相等，并且索引必须正确
        /// </summary>
        /// <param name="cmdValue">含有占位符的命令值表达式</param>
        /// <param name="detailsArray">参数的值</param>
        /// <returns></returns>
        private static string ReplacePlaceHolder(string cmdValue, int[] valueArray)
        {
            string[] formatParams = new string[valueArray.Length];
            for (int i = 0; i < valueArray.Length; i++)
                formatParams[i] = valueArray[i].ToString("X");
            return string.Format(cmdValue, formatParams);
        }

        private static string ReplaceDigitIndecaterAndDoAddition(string cmdValue)
        {
            string[] cmdElementArray = cmdValue.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (cmdElementArray == null || cmdElementArray.Length == 0)
                cmdElementArray = new string[] { cmdValue };
            for (int i = 0; i < cmdElementArray.Length; i++)
            {
                int variable = 0;
                int indexOfAdd = cmdElementArray[i].IndexOf(FXConfigReader.SIMBOLADD);
                int constNumber = Convert.ToInt32(cmdElementArray[i].Substring(0, indexOfAdd), 16);         //常数操作数
                int indexOfDot = cmdElementArray[i].IndexOf(FXConfigReader.SIMBOLDOT);
                if (indexOfDot > -1)            //如果存在"."，那么需要替换高低位占位符
                {
                    string digitIndicater = cmdElementArray[i].Substring(indexOfDot + 1, cmdElementArray[i].Length - indexOfDot - 1);
                    variable = Convert.ToInt32(cmdElementArray[i].Substring(indexOfAdd + 1, indexOfDot - indexOfAdd - 1), 16);
                    if (digitIndicater.Equals(FXConfigReader.DIGITINDICATERx))         //最低8位
                        variable = variable & 0xFF;
                    if (digitIndicater.Equals(FXConfigReader.DIGITINDICATERy))
                        variable = (variable >> 8) & 0xFF;
                    if (digitIndicater.Equals(FXConfigReader.DIGITINDICATERz))
                        variable = (variable >> 16) & 0xFF;
                    if (digitIndicater.Equals(FXConfigReader.DIGITINDICATERw))         //最高8位
                        variable = (variable >> 24) & 0xFF;
                }
                else        //如果不存在"."，那么直接相加
                {
                    variable = Convert.ToInt32(cmdElementArray[i].Substring(indexOfAdd + 1, cmdElementArray[i].Length - indexOfAdd - 1), 16);
                }
                int total = constNumber + variable;
                cmdElementArray[i] = Utils.FillStringWithChar(total.ToString("X"), FXConfigReader.ZEROCHAR, 4, true);
            }
            string result = "";
            for (int i = 0; i < cmdElementArray.Length; i++)
                result = result + FXConfigReader.BLANKSTR + cmdElementArray[i];
            return result.Substring(FXConfigReader.BLANKSTR.Length, result.Length - FXConfigReader.BLANKSTR.Length);
        }

        private static bool IsParamFitAdapter(FXBasicCmdCharacter character, string[] paramNameArray)
        {
            string[] adapterArray = character.Adapter.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (character.AdapterType == FXBasicCmdAdapterType.Exclude)
                return !Utils.IsArrayItemInAnotherArray(adapterArray, paramNameArray);
            if (character.AdapterType == FXBasicCmdAdapterType.Only)
                return Utils.IsArrayItemInAnotherArray(adapterArray, paramNameArray);
            if (character.AdapterType == FXBasicCmdAdapterType.NULL)
                return true;
            if (character.AdapterType == FXBasicCmdAdapterType.N)
                return true;
            if (character.AdapterType == FXBasicCmdAdapterType.With)
            {
                if (paramNameArray.Length == adapterArray.Length)
                {
                    for (int i = 0; i < paramNameArray.Length; i++)
                    {
                        if (!paramNameArray[i].Equals(adapterArray[i]))
                            return false;
                    }
                    return true;
                }
                else
                    return false;
            }
            return false;
        }

        private static NumberParseResult GetKHConstantValue(string constName, string constValue)
        {
            NumberParseResult result = new NumberParseResult();
            result.IsSuccess = false;
            int value = 0;
            if (constName.Equals(FXConfigReader.KCONSTANT, StringComparison.CurrentCultureIgnoreCase))
                result.IsSuccess = int.TryParse(constValue, NumberStyles.Number, null, out value);
            if (constName.Equals(FXConfigReader.HCONSTANT, StringComparison.CurrentCultureIgnoreCase))
                result.IsSuccess = int.TryParse(constValue, NumberStyles.HexNumber, null, out value);
            result.Value = value;
            return result;
        }
    }
}
