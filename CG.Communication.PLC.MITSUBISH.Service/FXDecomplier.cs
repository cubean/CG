using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CG.Data.DataSource;

namespace CG.Communication.PLC.MITSUBISH.Service
{
    public class FXDecomplier
    {
        /// <summary>将机器码反编译成源码
        /// 如果输入的机器码字符串中没有空格，那么将自动的按4个一组分开
        /// </summary>
        /// <param name="codeStr">机器码字符串，每四个16进制字符一组，每组之间需要有空格隔开，例如8000 0ABC</param>
        /// <returns></returns>
        public static string DecomplieCmdCode(string codeStr, string fxModel)
        {
            Dictionary<string, FXBasicCmdCharacter> reverseBasicCmdDict = FXConfigReader.ReverseBasicCmds[fxModel];
            Dictionary<int, FXAppCmd> reverseAppCmdDict = FXConfigReader.ReverseAppCmds[fxModel];
            Dictionary<string, IFXCmd> cmdDict = FXConfigReader.Cmds[fxModel];
            Dictionary<string, FXRegister> registerDict = FXConfigReader.Registers[fxModel];
            string result = "";
            int indexOfSpace = codeStr.IndexOf(FXConfigReader.BLANKSTR);        //查看输入的源代码字符串中是否有空格
            if (indexOfSpace == -1)         //如果没有空格，那么自动每4个1组，每组间添加一个空格
                codeStr = Utils.InsertStringWithInterval(codeStr, 4, FXConfigReader.BLANKSTR);
            string[] codeStrArray = codeStr.Split(new string[] { FXConfigReader.BLANKSTR }, StringSplitOptions.RemoveEmptyEntries);
            bool lenVaild = IsEveryCodeEquals4(codeStrArray);
            if (lenVaild)
            {
                int index = 0;          //已反编译的源代码组索引
                while (index < codeStrArray.Length)
                {
                    string complieStr = DecomplieCmdLine(codeStrArray, ref index, reverseBasicCmdDict, reverseAppCmdDict, cmdDict, registerDict);
                    if (complieStr == null)
                        throw new Exception("Decomplie Cmd Error! Already Decomplied Str is \r\n" + result);
                    result = result + complieStr + "\r\n";
                }
                return result;
            }
            else
                throw new Exception("Source Code Length Error!");
        }

        /// <summary>将输入的机器码数组按照靠前对齐原则转换成一行源代码
        /// 
        /// </summary>
        /// <param name="codeArray">机器码数组，每次输入5个，不足的用FFFF代替</param>
        /// <returns></returns>
        private static string DecomplieCmdLine(string[] codeArray, ref int index, Dictionary<string, FXBasicCmdCharacter> reverseBasicCmdDict, Dictionary<int, FXAppCmd> reverseAppCmdDict, Dictionary<string, IFXCmd> cmdDict, Dictionary<string, FXRegister> registerDict)
        {
            string tryBasicStr = DecomplieBasicCmdLine(codeArray, ref index, reverseBasicCmdDict, cmdDict, registerDict);           //尝试用基本命令去编译
            if (tryBasicStr == null)           //如果不是基本命令，那么尝试用应用指令去解释
            {
                string tryAppStr = DecomplieAppCmdLine(codeArray, ref index, reverseAppCmdDict, registerDict);
                return tryAppStr;
            }
            else
                return tryBasicStr;
        }

        /// <summary>从开始索引位置尝试反编译一条基本指令及其操作数
        ///  
        /// </summary>
        /// <param name="codeArray">全部机器码</param>
        /// <param name="index">开始编译部分的索引引用</param>
        /// <param name="reverseBasicCmdDict"></param>
        /// <param name="cmdDict"></param>
        /// <param name="registerDict"></param>
        /// <returns>编译成功返回结果字符串，不成功返回null</returns>
        private static string DecomplieBasicCmdLine(string[] codeArray, ref int index, Dictionary<string, FXBasicCmdCharacter> reverseBasicCmdDict, Dictionary<string, IFXCmd> cmdDict, Dictionary<string, FXRegister> registerDict)
        {
            string result = null;
            FXBasicCmdCharacter[] basicCharacters = GetBasicCmdCharacter(codeArray, ref index, reverseBasicCmdDict, cmdDict);
            if (basicCharacters != null)       //可以找到，说明是基本指令
            {
                foreach (FXBasicCmdCharacter character in basicCharacters)
                {
                    if (character.BasicCmdType == FXBasicCmdType.PureSingle)
                    {
                        return character.Cmd;
                    }
                    if (character.BasicCmdType == FXBasicCmdType.Single)
                    {
                        FXRegisterBasicInfo[] registerInfos = GetRegisterByCharacterSingleAndDouble(codeArray, ref index, character, registerDict);
                        if (registerInfos != null)
                        {
                            result = character.Cmd;
                            foreach (FXRegisterBasicInfo info in registerInfos)
                            {
                                if (character.AdapterType == FXBasicCmdAdapterType.N)           //如果是N类型的适配器，直接加起来就行
                                    result = result + info.NO;
                                else
                                    result = result + " " + info.Register + info.NO;
                            }
                            return result;
                        }
                    }
                    if (character.BasicCmdType == FXBasicCmdType.Double)
                    {
                        FXRegisterBasicInfo[] registerInfos = GetRegisterByCharacterSingleAndDouble(codeArray, ref index, character, registerDict);
                        if (registerInfos != null)
                        {
                            result = character.Cmd;
                            foreach (FXRegisterBasicInfo info in registerInfos)
                                result = result + " " + info.Register + info.NO;
                            return result;
                        }
                    }
                    if (character.BasicCmdType == FXBasicCmdType.Triple)
                    {
                        FXRegisterBasicInfo[] registerInfos = GetRegisterByCharacterTriple(codeArray, ref index, character, registerDict);
                        if (registerInfos != null)
                        {
                            result = character.Cmd;
                            foreach (FXRegisterBasicInfo info in registerInfos)
                                result = result + " " + info.Register + info.NO;
                            return result;
                        }
                    }
                    if (character.BasicCmdType == FXBasicCmdType.Five)
                    {
                        FXRegisterBasicInfo[] registerInfos = GetRegisterByCharacterFive(codeArray, ref index, character, registerDict);
                        if (registerInfos != null)
                        {
                            result = character.Cmd;
                            foreach (FXRegisterBasicInfo info in registerInfos)
                                result = result + " " + info.Register + info.NO;
                            return result;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>从开始索引位置尝试反编译一条应用指令及其操作数
        /// 
        /// </summary>
        /// <param name="codeArray"></param>
        /// <param name="index"></param>
        /// <param name="reverseAppCmdDict"></param>
        /// <param name="registerDict"></param>
        /// <returns></returns>
        private static string DecomplieAppCmdLine(string[] codeArray, ref int index, Dictionary<int, FXAppCmd> reverseAppCmdDict, Dictionary<string, FXRegister> registerDict)
        {
            FXAppCmd appCmd = GetAppCmd(codeArray, ref index, reverseAppCmdDict);
            if (appCmd != null)             //如果找到了该应用指令，那么计算操作数
            {
                int totalLen = GetAppOperandCodeLen(codeArray, index);
                string operandStr = "";
                int len = 2;
                if (appCmd.AppCmdType == FXAppCmdType.D || appCmd.AppCmdType == FXAppCmdType.DP)
                    len = 4;
                if (totalLen % len != 0)        //操作数长度必须刚好
                    throw new Exception("Decomplie App Cmd Operand Len Error!");
                while (totalLen > 0)
                {
                    operandStr = operandStr + " " + GetAppOperandStr(codeArray, index, len, registerDict);
                    index = index + len;
                    totalLen = totalLen - len;
                }
                return GetAppCmdStrByCmdType(appCmd) + " " + operandStr;
            }
            else
                return null;
        }

        /// <summary>根据给定的源代码数组查找其对应的基本命令特性(如果是基本命令的话)
        /// 
        /// </summary>
        /// <param name="codeArray">全部机器码数组</param>
        /// <param name="index">当前执行到的索引的引用</param>
        /// <param name="reverseBasicCmdDict">基本指令反向对照字典</param>
        /// <param name="cmdDict">命令字典</param>
        /// <returns></returns>
        private static FXBasicCmdCharacter[] GetBasicCmdCharacter(string[] codeArray, ref int index, Dictionary<string, FXBasicCmdCharacter> reverseBasicCmdDict, Dictionary<string, IFXCmd> cmdDict)
        {
            string key = codeArray[index];
            if (reverseBasicCmdDict.ContainsKey(key))           //如果直接和第1组相等，那么说明是纯单字命令，直接返回
            {
                index++;
                return new FXBasicCmdCharacter[] { reverseBasicCmdDict[key] };
            }
            key = codeArray[index][0].ToString();
            if (reverseBasicCmdDict.ContainsKey(key))     //如果和第1组的第1位相等，那么说明是单字命令，直接返回
            {
                return new FXBasicCmdCharacter[] { reverseBasicCmdDict[key] };
            }
            key = codeArray[index] + " " + codeArray[index + 1][0].ToString();
            if (reverseBasicCmdDict.ContainsKey(key))    //如果和第1组的第1~4位，第2组的第1位相同，那么说明是双字指令，直接返回
            {
                index = index++;
                return new FXBasicCmdCharacter[] { reverseBasicCmdDict[key] };
            }
            //还没返回，那么要么不是基本指令，要么是三字，要么是五字，先找五字指令，不过要先判断以下够不够长
            if (codeArray.Length - index >= 5)
            {
                key = codeArray[index][0].ToString() + FXConfigReader.BLANKSTR + codeArray[index + 1].Substring(0, 2) + FXConfigReader.BLANKSTR + codeArray[index + 2].Substring(0, 2) + FXConfigReader.BLANKSTR + codeArray[index + 3].Substring(0, 2) + FXConfigReader.BLANKSTR + codeArray[index + 4].Substring(0, 2);
                if (reverseBasicCmdDict.ContainsKey(key))    //如果第1组的第1个，第2组的第1~2个，第3组的第1~2个，第4组的第1~2个，第5组的第1~2个相同，那么是五字指令，直接返回
                {
                    return new FXBasicCmdCharacter[] { reverseBasicCmdDict[key] };
                }
            }
            //还没返回，那么开始寻找是否是3字指令
            if (codeArray.Length - index >= 3)
            {
                key = codeArray[index][0].ToString() + FXConfigReader.BLANKSTR + codeArray[index + 1].Substring(0, 2) + FXConfigReader.BLANKSTR + codeArray[index + 2].Substring(0, 2);
                if (reverseBasicCmdDict.ContainsKey(key))     //如果第1组的第1个，第2组的第1~2个，第3组的第1~2个相同，那么是三字指令，从命令对字典中获得对应的命令后返回
                {
                    FXBasicCmdCharacter character = reverseBasicCmdDict[key];
                    FXBasicCmd cmd = (FXBasicCmd)cmdDict[character.Cmd];
                    List<FXBasicCmdCharacter> tempList = new List<FXBasicCmdCharacter>();
                    foreach (FXBasicCmdCharacter tempCharacter in cmd.Characters)
                    {
                        if (tempCharacter.BasicCmdType == FXBasicCmdType.Triple)
                            tempList.Add(tempCharacter);
                    }
                    FXBasicCmdCharacter[] result = new FXBasicCmdCharacter[tempList.Count];
                    tempList.CopyTo(result);
                    return result;
                }
            }
            //到这都没返回，那么就不是基本指令了，直接返回null
            return null;
        }

        /// <summary>根据给定的机器码找到对应的应用指令
        /// 
        /// </summary>
        /// <param name="codeArray">机器码数组</param>
        /// <param name="index">当前的索引</param>
        /// <param name="reverseAppCmdDict">应用指令功能编号和应用指令对照表</param>
        /// <returns></returns>
        private static FXAppCmd GetAppCmd(string[] codeArray, ref int index, Dictionary<int, FXAppCmd> reverseAppCmdDict)
        {
            int cmdInt = Convert.ToInt32(codeArray[index], 16);
            FXAppCmdType cmdType = FXAppCmdType.Normal;
            if (cmdInt > 0x1000)           //如果命令值大于0x1000，那么说明有脉冲成分
            {
                cmdInt = cmdInt - 0x1000;
                if (cmdInt % 2 == 0)        //如果是偶数，那么没有没有32位成分
                    cmdType = FXAppCmdType.P;
                else
                {
                    cmdType = FXAppCmdType.DP;
                    cmdInt--;
                }
            }
            else        //没有脉冲成分
            {
                if (cmdInt % 2 == 0)        //如果是偶数，那么没有没有32位成分
                    cmdType = FXAppCmdType.Normal;
                else
                {
                    cmdType = FXAppCmdType.D;
                    cmdInt--;
                }
            }
            cmdInt = cmdInt / 2 - 8;
            if (reverseAppCmdDict.ContainsKey(cmdInt))
            {
                FXAppCmd cmdInDict = reverseAppCmdDict[cmdInt];
                FXAppCmd cmd = new FXAppCmd();
                cmd.AppCmdType = cmdType;
                cmd.Comment = cmdInDict.Comment;
                cmd.FuncNO = cmdInDict.FuncNO;
                cmd.Group = cmdInDict.Group;
                cmd.Name = cmdInDict.Name;
                index++;
                return cmd;
            }
            else
                return null;
        }

        /// <summary>获得单字或多字指令的操作数信息
        /// 
        /// </summary>
        /// <param name="code">如果是Single的，那么就是4位的机器码；如果是Double的，那么就是8位机器码中的后四位</param>
        /// <param name="character"></param>
        /// <param name="registerDict"></param>
        /// <returns></returns>
        private static FXRegisterBasicInfo[] GetRegisterByCharacterSingleAndDouble(string[] codeArray, ref int index, FXBasicCmdCharacter character, Dictionary<string, FXRegister> registerDict)
        {
            FXRegisterBasicInfo[] infos = null;
            if (character.BasicCmdType == FXBasicCmdType.Double)
                index++;
            int specifiedAddr = Convert.ToInt32(codeArray[index].Substring(1, 3), 16);      //单字和双字命令的操作数地址为给定4位机器码的后三位
            index++;
            FXRegisterBasicInfo info = GetRegisterInfo(specifiedAddr, character.Adapter, -1, character.AdapterType, registerDict);
            infos = new FXRegisterBasicInfo[] { info };
            return infos;
        }

        /// <summary>获得三字指令的操作数信息
        /// 
        /// </summary>
        /// <param name="codes">机器码(至少3个长)</param>
        /// <param name="character"></param>
        /// <param name="registerDict"></param>
        /// <returns></returns>
        private static FXRegisterBasicInfo[] GetRegisterByCharacterTriple(string[] codeArray, ref int index, FXBasicCmdCharacter character, Dictionary<string, FXRegister> registerDict)
        {
            FXRegisterBasicInfo[] infos = null;
            int addr0 = Convert.ToInt32(codeArray[index].Substring(1, 3), 16);              //获取第一个操作数地址
            int addr1 = Convert.ToInt32(codeArray[index + 2].Substring(2, 2) + codeArray[index + 1].Substring(2, 2), 16);       //获取第二个操作数地址
            FXRegisterBasicInfo info0 = GetRegisterInfo(addr0, character.Adapter, 0, FXBasicCmdAdapterType.With, registerDict);
            if (info0 != null)
            {
                FXRegisterBasicInfo info1 = GetRegisterInfo(addr1, character.Adapter, 1, FXBasicCmdAdapterType.With, registerDict);
                if (info1 != null)
                {
                    infos = new FXRegisterBasicInfo[] { info0, info1 };
                    index = index + 3;
                }
            }
            return infos;
        }

        /// <summary>获得五字指令的操作数信息
        /// 
        /// </summary>
        /// <param name="codes">机器码</param>
        /// <param name="character"></param>
        /// <param name="registerDict"></param>
        /// <returns></returns>
        private static FXRegisterBasicInfo[] GetRegisterByCharacterFive(string[] codeArray, ref int index, FXBasicCmdCharacter character, Dictionary<string, FXRegister> registerDict)
        {
            FXRegisterBasicInfo[] infos = null;
            int addr0 = Convert.ToInt32(codeArray[index].Substring(1, 3), 16);              //获取第一个操作数地址
            int addr1 = Convert.ToInt32(codeArray[index + 4].Substring(2, 2) + codeArray[index + 3].Substring(2, 2) + codeArray[index + 2].Substring(2, 2) + codeArray[index + 1].Substring(2, 2), 16);       //获取第二个操作数地址
            FXRegisterBasicInfo info0 = GetRegisterInfo(addr0, character.Adapter, 0, FXBasicCmdAdapterType.With, registerDict);
            if (info0 != null)
            {
                FXRegisterBasicInfo info1 = GetRegisterInfo(addr1, character.Adapter, 1, FXBasicCmdAdapterType.With, registerDict);
                if (info1 != null)
                {
                    infos = new FXRegisterBasicInfo[] { info0, info1 };
                    index = index + 5;
                }
            }
            return infos;
        }

        /// <summary>从给定的地址中获得Register信息
        /// 当adapterType=With时，需要指定validAdapterIndex来指定需要具体哪一个适配器
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="adapterValue">适配器的值</param>
        /// <param name="validAdapterIndex">有效的适配器索引(仅当adapterType=With时有效，其余值忽略本参数)</param>
        /// <param name="adapterType">适配器类型</param>
        /// <param name="registerDict">Register字典</param>
        /// <returns></returns>
        private static FXRegisterBasicInfo GetRegisterInfo(int address, string adapterValue, int validAdapterIndex, FXBasicCmdAdapterType adapterType, Dictionary<string, FXRegister> registerDict)
        {
            FXRegisterBasicInfo info = null;
            if (adapterType == FXBasicCmdAdapterType.N)              //适配器类型是N，直接返回常数
            {
                info = new FXRegisterBasicInfo();
                info.Register = FXConfigReader.KCONSTANT;
                info.NO = address;
                return info;
            }
            string[] adapterItems = adapterValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);       //分出adapterValue中的所有Register
            if (adapterType == FXBasicCmdAdapterType.Exclude)     //计算除了adapterValue中的所有元素
            {
                foreach (FXRegister register in registerDict.Values)            //遍历所有Register
                {
                    if (!Utils.IsItemInAnotherArray(register.Name, adapterItems))        //只计算不相同的元素
                    {
                        string registerNOStr = register.GetRegisterNOStr(address);
                        if (registerNOStr != null)
                        {
                            info = new FXRegisterBasicInfo();
                            info.Register = register.Name;
                            info.NO = Convert.ToInt32(registerNOStr);
                            break;
                        }
                    }
                }
            }
            if (adapterType == FXBasicCmdAdapterType.Only)         //只计算adapterValue中指定的元素
            {
                foreach (string adapter in adapterItems)
                {
                    FXRegister register = registerDict[adapter];
                    string registerNOStr = register.GetRegisterNOStr(address);
                    if (registerNOStr != null)
                    {
                        info = new FXRegisterBasicInfo();
                        info.Register = register.Name;
                        info.NO = Convert.ToInt32(registerNOStr);
                        break;
                    }
                }
            }
            if (adapterType == FXBasicCmdAdapterType.With)           //只计算adapterValue中由validAdapterIndex指定的元素
            {
                string adapter = adapterItems[validAdapterIndex];
                if (adapter.Equals(FXConfigReader.KCONSTANT))        //如果是K的话说明是常数，不用算了
                {
                    info = new FXRegisterBasicInfo();
                    info.Register = FXConfigReader.KCONSTANT;
                    info.NO = address;
                }
                else
                {
                    FXRegister register = registerDict[adapter];
                    string registerNOStr = register.GetRegisterNOStr(address);
                    if (registerNOStr != null)
                    {
                        info = new FXRegisterBasicInfo();
                        info.Register = register.Name;
                        info.NO = Convert.ToInt32(registerNOStr);
                    }
                }
            }
            return info;
        }

        /// <summary>检查源代码字符串数组中是否每一项的长度都为4
        /// 
        /// </summary>
        /// <param name="codeStrArray"></param>
        /// <returns></returns>
        private static bool IsEveryCodeEquals4(string[] codeStrArray)
        {
            foreach (string codeStr in codeStrArray)
            {
                if (codeStr.Length != 4)
                    return false;
            }
            return true;
        }

        /// <summary>根据机器码获得对应的操作数细节
        /// 
        /// </summary>
        /// <param name="codeArray">全部机器码数组(每4个16进制字符为一组)</param>
        /// <param name="index">开始索引</param>
        /// <param name="len">操作数机器码长度(只能为2或者4)</param>
        /// <param name="registerDict"></param>
        /// <returns></returns>
        private static string GetAppOperandStr(string[] codeArray, int index, int len, Dictionary<string, FXRegister> registerDict)
        {
            if (len != 2 && len != 4)
                return null;
            int m = Convert.ToInt32(codeArray[index].Substring(1, 1));          //m是第1个机器码组的第2个字符
            int n = Convert.ToInt32(codeArray[index + 1].Substring(1, 1));      //n是第2个机器码组的第2个字符
            string operandValue;
            if (len == 2)
                operandValue = codeArray[index + 1].Substring(2, 2) + codeArray[index].Substring(2, 2);
            else
                operandValue = codeArray[index + 3].Substring(2, 2) + codeArray[index + 2].Substring(2, 2) + codeArray[index + 1].Substring(2, 2) + codeArray[index].Substring(2, 2);
            if (m == 0 && n == 0)              //只有K常数的操作数才是这样的
                return FXConfigReader.KCONSTANT + Convert.ToInt32(operandValue, 16);
            if (m == 2 && n == 0)              //只有H常数的操作数才是这样的
                return FXConfigReader.HCONSTANT + Convert.ToInt32(operandValue, 16).ToString("X");
            foreach (FXRegister register in registerDict.Values)        //遍历所有的Register
            {
                if (register.MN.Count > 0 && register.MN[0].M == m)       //只处理有标识MN 并且M相等的情况
                {
                    if (register.MN[0].IsPoint)          //如果是位元，那么可以看成是K+register的组合，K部分的大小为n/2
                    {
                        string registerPart = register.GetRegisterNOStr(Convert.ToInt32(operandValue, 16));
                        if (registerPart != null)
                        {
                            string kPart = FXConfigReader.KCONSTANT + Convert.ToString(n / 2);         //计算K的部分
                            if (n / 2 == 0)          //如果K小于0，那么没有意义，因此直接返回寄存器就成
                                return register.Name + registerPart;
                            else
                                return kPart + register.Name + registerPart;
                        }
                    }
                    else if (register.MN[0].N == n)        //如果是字元，那么只需要处理MN对上的情形
                    {
                        int operandValueInt = Convert.ToInt32(operandValue, 16);
                        operandValueInt = operandValueInt / 2 + register.From;          //除以2是偏移量，需要加上从哪里开始的
                        return register.Name + Convert.ToString(operandValueInt, (int)register.NameFormatType);
                    }
                }
            }
            return null;
        }

        /// <summary>找到一条应用指令操作数的长度
        ///  原理是所有应用指令操作数都是以8开头的，因此从开始索引处开始查找到第一个不是8开头的就ok
        /// </summary>
        /// <param name="codeArray"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static int GetAppOperandCodeLen(string[] codeArray, int index)
        {
            int len = 0;
            while (codeArray.Length > index + len)
            {
                if (codeArray[index + len].StartsWith("8"))
                    len++;
                else
                    break;
            }
            return len;
        }

        /// <summary>根据应用指令的类型生成对应的应用指令符号字符串
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static string GetAppCmdStrByCmdType(FXAppCmd cmd)
        {
            if (cmd.AppCmdType == FXAppCmdType.D)
                return "D" + cmd.Name;
            if (cmd.AppCmdType == FXAppCmdType.DP)
                return "D" + cmd.Name + "P";
            if (cmd.AppCmdType == FXAppCmdType.P)
                return cmd.Name + "P";
            return cmd.Name;
        }
    }
}
