using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CG.Communication.PLC.Siemens.S7_200.Service
{
    [ServiceContract]
    public interface IFXService
    {
        #region 功能部分
        [OperationContract]
        bool OpenPort(string port, string baud, char parity);

        [OperationContract]
        void ClosePort();

        [OperationContract]
        bool Complie(string sourceCode, out byte[] block);

        [OperationContract]
        bool Decompile(byte[] block, out string readSourceCode);

        [OperationContract]
        bool Connect(int address);

        [OperationContract]
        void DisConnect();
        #endregion

        #region 调试部分
        //[OperationContract]
        //string ForceOn(FXRegisterBasicInfo info);

        //[OperationContract]
        //string ForceOff(FXRegisterBasicInfo info);
        #endregion

        #region 读取部分
        [OperationContract]
        bool ReadProgramBlock(out byte[] block);
        #endregion

        #region 写入部分
        [OperationContract]
        bool WriteProgramBlock(byte[] block);
        #endregion
    }
}
