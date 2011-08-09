using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CG.Data.DataSource;

namespace CG.Communication.PLC.MITSUBISH.Service
{
    public class FXCommander1
    {
        /// <summary>
        /// create command which is specified by parameters
        /// </summary>
        /// <param name="control"></param>
        /// <param name="cmd"></param>
        /// <param name="addrStr">the relative address of the first data, in string format, just like "D123\Y0", ignore cases</param>
        /// <param name="countStr">the number of the data which should be read, in string format and the length must be 2</param>
        /// <param name="dataStrArray">the data which should be written, in string array format, each element's length must be 2 and the total length of the array must equals with "countStr"</param>
        /// <returns>the command, in byte array format</returns>
        public static byte[] CreateCommand(FXSeriesControl control,FXSeriesCmd cmd,int addrFrom,int len,string[] dataStrArray)
        {
            string addrStr = Utils.FillStringWithChar(Convert.ToString(addrFrom, 16).ToUpper(), '0', 4, true);
            string lenStr = Utils.FillStringWithChar(Convert.ToString(len, 16).ToUpper(), '0', 2, true);
            if (control == FXSeriesControl.ENQ)
                return new byte[] { (byte)FXSeriesControl.ENQ };
            else if (control == FXSeriesControl.STX)
            {
                byte[] cmdBytes = GetCmdByte(cmd);
                if (cmd == FXSeriesCmd.Read || cmd == FXSeriesCmd.ReadConfig || cmd == FXSeriesCmd.ReadProgram)
                    return ReadCommand(addrStr, lenStr, cmdBytes);
                else if (cmd == FXSeriesCmd.Write || cmd == FXSeriesCmd.WriteConfig || cmd == FXSeriesCmd.WriteProgram)
                    return WriteCommand(addrStr, lenStr, dataStrArray, cmdBytes);
                else if (cmd == FXSeriesCmd.ON || cmd == FXSeriesCmd.OFF)
                    return ForceOnOffCommand(cmd, addrStr);
                else
                    throw new NotImplementedException();
            }
            else
                throw new Exception("PC only can send command ENQ or STX");
        }
        public static byte[] CreateCommandEx(FXSeriesControl control, FXSeriesCmd cmd, int addrFrom, int len, byte[] dataBytes)
        {
            string addrStr = Utils.FillStringWithChar(Convert.ToString(addrFrom, 16).ToUpper(), '0', 4, true);
            string lenStr = Utils.FillStringWithChar(Convert.ToString(len, 16).ToUpper(), '0', 2, true);
            if (control == FXSeriesControl.ENQ)
                return new byte[] { (byte)FXSeriesControl.ENQ };
            else if (control == FXSeriesControl.STX)
            {
                byte[] cmdBytes = GetCmdByte(cmd);
                if (cmd == FXSeriesCmd.Read || cmd == FXSeriesCmd.ReadConfig || cmd == FXSeriesCmd.ReadProgram)
                    return ReadCommand(addrStr, lenStr, cmdBytes);
                else if (cmd == FXSeriesCmd.Write || cmd == FXSeriesCmd.WriteConfig || cmd == FXSeriesCmd.WriteProgram)
                    return WriteCommand(addrStr, lenStr, dataBytes, cmdBytes);
                else if (cmd == FXSeriesCmd.ON || cmd == FXSeriesCmd.OFF)
                    return ForceOnOffCommand(cmd, addrStr);
                else
                    throw new NotImplementedException();
            }
            else
                throw new Exception("PC only can send command ENQ or STX");
        }
        /// <summary>
        /// convert the byte array like command to obvious ascii format
        /// </summary>
        /// <param name="cmd">command in byte array format</param>
        /// <returns>ascii format command</returns>
        public static string ConvertToASCIIFormat(byte[] cmd)
        {
            string result = "";
            foreach (byte data in cmd)
            {
                if ((data >= 0x30 && data <= 0x39) || (data >= 0x41 && data <= 0x46))       //in 0~9 or A~F
                    result = result + (char)data+", ";
                else
                    result = result + "(" + data.ToString("X") + "), ";
            }
            return result;
        }
        /// <summary>
        /// create command to read from "addrStr" with the length "countStr"
        /// </summary>
        /// <param name="addrStr">the absolute address of the first data, in string format and the length must be 4</param>
        /// <param name="countStr">the number of the data which should be read, in string format and the length must be 2</param>
        /// <param name="cmd">the real command data, in byte array format</param>
        /// <returns>the read command, in byte array format</returns>
        private static byte[] ReadCommand(string addrStr, string countStr,byte[] cmd)
        {
            if (addrStr != null && countStr.Length == 2)
            {
                byte[] result = new byte[10+cmd.Length];
                result[0] = (byte)FXSeriesControl.STX;
                Array.Copy(cmd, 0, result, 1, cmd.Length);
                result[cmd.Length + 1] = (byte)addrStr[0];
                result[cmd.Length + 2] = (byte)addrStr[1];
                result[cmd.Length + 3] = (byte)addrStr[2];
                result[cmd.Length + 4] = (byte)addrStr[3];
                result[cmd.Length + 5] = (byte)countStr[0];
                result[cmd.Length + 6] = (byte)countStr[1];
                result[cmd.Length + 7] = (byte)FXSeriesControl.ETX;
                byte[] checkBytes = Utils.CheckSum(result, 1, cmd.Length + 7, 2, HighOrLow.KeepLow);
                result[cmd.Length + 8] = checkBytes[0];
                result[cmd.Length + 9] = checkBytes[1];
                return result;
            }
            else
                throw new Exception("addrStr is null or countStr's length is not 2");
        }
        /// <summary>
        /// create command to write from "addrStr" with the length "countStr"
        /// </summary>
        /// <param name="addrStr">the absolute address of the first data, in string format and the length must be 4</param>
        /// <param name="countStr">the number of the data which should be write, in string format and the length must be 2</param>
        /// <param name="dataStrArray">the data which should be written, in string array format, each element's length must be 2 and the total length of the array must equals with "countStr"</param>
        /// <param name="cmd">the real command data, in byte array format</param>
        /// <returns>the write command, in byte array format</returns>
        private static byte[] WriteCommand(string addrStr, string countStr, string[] dataStrArray,byte[] cmd)
        {
            if (addrStr != null && countStr.Length == 2)
            {
                int count = Convert.ToInt32(countStr,16);
                if (dataStrArray.Length == count)
                {
                    int resultLen = 10 + cmd.Length + count * 2;
                    byte[] result = new byte[resultLen];
                    result[0] = (byte)FXSeriesControl.STX;
                    Array.Copy(cmd, 0, result, 1, cmd.Length);
                    result[cmd.Length + 1] = (byte)addrStr[0];
                    result[cmd.Length + 2] = (byte)addrStr[1];
                    result[cmd.Length + 3] = (byte)addrStr[2];
                    result[cmd.Length + 4] = (byte)addrStr[3];
                    result[cmd.Length + 5] = (byte)countStr[0];
                    result[cmd.Length + 6] = (byte)countStr[1];
                    int index = cmd.Length + 6;
                    for (int i = 0; i < count;i++ )             //fill the data part of the write command
                    {
                        string temp = dataStrArray[i].ToUpper();
                        result[++index] = (byte)temp[0];
                        result[++index] = (byte)temp[1];
                    }
                    result[++index] = (byte)FXSeriesControl.ETX;
                    byte[] checkBytes = Utils.CheckSum(result, 1, index, 2, HighOrLow.KeepLow);
                    result[++index] = checkBytes[0];
                    result[++index] = checkBytes[1];
                    return result;
                }
                else
                    throw new Exception("countStr is not equal to the length of dataStrArray");
            }
            else
                throw new Exception("addrStr is null or countStr's length is not 2");
        }

        private static byte[] WriteCommand(string addrStr,string countStr,byte[] dataBytes,byte[] cmd)
        {
            if (addrStr != null && countStr.Length == 2)
            {
                int count = Convert.ToInt32(countStr, 16);
                if (dataBytes.Length == count)
                {
                    int resultLen = 10 + cmd.Length + count * 2;
                    byte[] result = new byte[resultLen];
                    result[0] = (byte)FXSeriesControl.STX;
                    Array.Copy(cmd, 0, result, 1, cmd.Length);
                    result[cmd.Length + 1] = (byte)addrStr[0];
                    result[cmd.Length + 2] = (byte)addrStr[1];
                    result[cmd.Length + 3] = (byte)addrStr[2];
                    result[cmd.Length + 4] = (byte)addrStr[3];
                    result[cmd.Length + 5] = (byte)countStr[0];
                    result[cmd.Length + 6] = (byte)countStr[1];
                    int index = cmd.Length + 6;
                    for (int i = 0; i < count; i++)             //fill the data part of the write command
                    {
                        index++;
                        result[index] = (byte)(dataBytes[i] >> 4);
                        result[index] = (byte)(result[index] > 0x09 ? result[index] + 0x37 : result[index] + 0x30);
                        index++;
                        result[index] = (byte)(dataBytes[i] & 0x0F);
                        result[index] = (byte)(result[index] > 0x09 ? result[index] + 0x37 : result[index] + 0x30);
                    }
                    result[++index] = (byte)FXSeriesControl.ETX;
                    byte[] checkBytes = Utils.CheckSum(result, 1, index, 2, HighOrLow.KeepLow);
                    result[++index] = checkBytes[0];
                    result[++index] = checkBytes[1];
                    return result;
                }
                else
                    throw new Exception("countStr is not equal to the length of dataStrArray");
            }
            else
                throw new Exception("addrStr is null or countStr's length is not 2");
        }
        /// <summary>生成强制On/Off命令
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static byte[] ForceOnOffCommand(FXSeriesCmd cmd, string addrStr)
        {
            byte[] result = new byte[9];
            result[0] = 0x02;
            if (cmd == FXSeriesCmd.ON)
                result[1] = 0x37;
            else
                result[1] = 0x38;
            result[2] = (byte)addrStr[2];
            result[3] = (byte)addrStr[3];
            result[4] = (byte)addrStr[0];
            result[5] = (byte)addrStr[1];
            result[6] = 0x03;
            byte[] checkBytes = Utils.CheckSum(result, 1, 6, 2, HighOrLow.KeepLow);
            result[7] = checkBytes[0];
            result[8] = checkBytes[1];
            return result;
        }
        /// <summary>
        ///  mapping the relative address to absolute address
        /// </summary>
        /// <param name="addrStr">the relative address, in string format, just like "D123\Y0", ignore cases</param>
        /// <returns>the absolute address which the relative address specified, if success the length of return value is 4, others null</returns>
        private static string ConvertToAbsAddr(string addrStr)
        {
            return addrStr;
        }
        /// <summary>
        /// get real command data from command type
        /// </summary>
        /// <param name="cmd">command type</param>
        /// <returns>real command data</returns>
        private static byte[] GetCmdByte(FXSeriesCmd cmd)
        {
            byte[] result = null;
            switch (cmd)
            {
                case FXSeriesCmd.ON:
                    result = new byte[] { (byte)'7'};
                    break;
                case FXSeriesCmd.OFF:
                    result = new byte[] { (byte)'8'};
                    break;
                case FXSeriesCmd.Read:
                    result = new byte[] { (byte)'0'};
                    break;
                case FXSeriesCmd.Write:
                    result = new byte[] { (byte)'1'};
                    break;
                case FXSeriesCmd.ReadConfig:
                    result = new byte[] { (byte)'E',(byte)'0',(byte)'0'};
                    break;
                case FXSeriesCmd.WriteConfig:
                    result = new byte[] { (byte)'E',(byte)'1',(byte)'0'};
                    break;
                case FXSeriesCmd.ReadProgram:
                    result = new byte[] { (byte)'E',(byte)'0',(byte)'1'};
                    break;
                case FXSeriesCmd.WriteProgram:
                    result = new byte[] { (byte)'E',(byte)'1',(byte)'1'};
                    break;
            }
            return result;
        }
    }
}
