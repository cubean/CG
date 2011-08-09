using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CG.Communication.PLC.MITSUBISH.Service
{
    [ServiceContract]
    public interface IFXService
    {
        #region 功能部分
        [OperationContract]
        string OpenPort();

        [OperationContract]
        string ClosePort();

        [OperationContract]
        FXComplieResult Complie(string sourceCode);

        [OperationContract]
        FXDeComplieResult Decompile(string readSourceCode);

        [OperationContract]
        FXENQResult ENQ();

        [OperationContract]
        string Connect(string fxModel,string comStr);

        [OperationContract]
        string DisConnect();

        #endregion

        #region 调试部分
        [OperationContract]
        FXRemoteControlResult RemoteControl(FXStatus status);

        [OperationContract]
        string ForceOn(FXRegisterBasicInfo info);

        [OperationContract]
        string ForceOff(FXRegisterBasicInfo info);

        [OperationContract]
        string WriteRegister(FXRegisterBasicInfo info, byte[] value);
        #endregion

        #region 读取部分
        [OperationContract]
        FXRegionInfoResult ReadParamRegion();

        [OperationContract]
        FXReadCodeResult ReadCode();

        [OperationContract]
        FXDeComplieResult ReadDecompiledCode();

        [OperationContract]
        FXReadCommentResult ReadComment();

        [OperationContract]
        FXReadStatusResult ReadPLCStatus();

        [OperationContract]
        FXRegisterReadResult ReadRegisters(List<FXRegisterReadInfo> infos);
        #endregion

        #region 写入部分
        [OperationContract]
        string WriteComment(FXCommentInfo[] infos);

        [OperationContract]
        string WriteCode(string codeStr);

        [OperationContract]
        string WriteCodeInFile(string path);

        [OperationContract]
        string WriteParamRegion(FXRegionInfo[] infos);
        #endregion

        #region 获得配置部分
        [OperationContract]
        Dictionary<string, FXBasicCmd> GetBasicCmdConfig();

        [OperationContract]
        Dictionary<string, FXAppCmd> GetAppCmdConfig();

        [OperationContract]
        Dictionary<string, FXRegister> GetRegisterConfig();

        [OperationContract]
        List<OperandMap> GetRegisterMappingConfig();
        #endregion
    }
}
