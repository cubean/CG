using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using Irony.Parsing;

namespace STLCompiler
{
    public class AddressTerm : Terminal
    {

        #region Nested classes
        [Flags]
        public enum AreaTypes : int
        {
            None = 0,
            DiscreteInput = eCOMM200_MEMORY_AREA_MASK.eCOMM200_I_MEM_ONLY,
            DiscreteOutput = eCOMM200_MEMORY_AREA_MASK.eCOMM200_Q_MEM_ONLY,
            AnalogInput = eCOMM200_MEMORY_AREA_MASK.eCOMM200_AI_MEM_ONLY,
            AnalogOutput = eCOMM200_MEMORY_AREA_MASK.eCOMM200_AQ_MEM_ONLY,
            DbMemory = eCOMM200_MEMORY_AREA_MASK.eCOMM200_SD_MEM_ONLY,
            VariableMemory = eCOMM200_MEMORY_AREA_MASK.eCOMM200_V_MEM_ONLY,
            FlagMemory = eCOMM200_MEMORY_AREA_MASK.eCOMM200_M_MEM_ONLY,
            SystemMemory = eCOMM200_MEMORY_AREA_MASK.eCOMM200_SM_MEM_ONLY,
            StageMemory = eCOMM200_MEMORY_AREA_MASK.eCOMM200_SCR_MEM_ONLY,
            Accumulator = eCOMM200_MEMORY_AREA_MASK.eCOMM200_AC_MEM_ONLY,
            Timer = eCOMM200_MEMORY_AREA_MASK.eCOMM200_T_MEM_ONLY,
            Counter = eCOMM200_MEMORY_AREA_MASK.eCOMM200_C_MEM_ONLY,
            HighspeedCount = eCOMM200_MEMORY_AREA_MASK.eCOMM200_HC_MEM_ONLY,
            LocalMemory = eCOMM200_MEMORY_AREA_MASK.eCOMM200_L_MEM_ONLY,
            SubroutineMemory = eCOMM200_MEMORY_AREA_MASK.eCOMM200_SBR_MEM_ONLY,
        }
        //nested helper class
        protected class AreaTable : Dictionary<string, AreaTypes> { }
        protected class AreaFlagTable : Dictionary<AreaTypes, ushort> { }
        protected class AreaTypeList : List<AreaTypes> { }
        public class AddressDetails
        {
            public string Prefix;
            public string DbNumber;
            public string Area;
            public int AreaTypeCode;
            public string SizePrefix;
            public string Offset;
            public string BitPoint;
            public string Error;
            public object Value;
            public string Text { get { return Prefix + DbNumber + Area + SizePrefix + Offset + BitPoint; } }
        }
        #endregion

        #region Public Consts
        #endregion

        #region constructors and initialization
        public AddressTerm(string name)
            : base(name, TokenCategory.Content, TermFlags.IsLiteral)
        {
            Priority = IdentifierTerminal.ReservedWordsPriority - 1;
        }
        public void AddArea(string area, AreaTypes type)
        {
            AreaTypeMaps.Add(area, type);
            Areas.Add(area);
        }
        #endregion

        #region Public fields/properties: ExponentSymbols, Suffixes
        #endregion

        #region Private fields:
        StringList Areas = new StringList();
        readonly AreaTable AreaTypeMaps = new AreaTable();
        readonly AreaFlagTable AreaFlags = new AreaFlagTable();
        string _areasFirsts;
        string _sizePrefixes = "BbWwDd";
        AreaTypeList _directPrefixes = new AreaTypeList { 
            AreaTypes.DiscreteInput,
            AreaTypes.DiscreteOutput,
            AreaTypes.VariableMemory,
            AreaTypes.FlagMemory,
            AreaTypes.SystemMemory,
            AreaTypes.StageMemory,
        };
        AreaTypeList _peripheralPrefixes = new AreaTypeList { 
            AreaTypes.AnalogInput,
            AreaTypes.AnalogOutput,
            AreaTypes.SubroutineMemory,
        };
        AreaTypeList _timerCounterPrefixes = new AreaTypeList { 
            AreaTypes.Timer,
            AreaTypes.Counter,
        };
        AreaTypeList _addressOfPrefixes = new AreaTypeList { 
            AreaTypes.VariableMemory,
            AreaTypes.DiscreteInput,
            AreaTypes.DiscreteOutput,
            AreaTypes.Timer,
            AreaTypes.Counter,
            AreaTypes.FlagMemory,
            AreaTypes.SystemMemory,
            AreaTypes.DbMemory,
        };

        #endregion

        #region overrides: Init GetFirsts TryMatch
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);

            AddArea("I", AreaTypes.DiscreteInput);
            AddArea("E", AreaTypes.DiscreteInput);
            AddArea("Q", AreaTypes.DiscreteOutput);
            AddArea("A", AreaTypes.DiscreteOutput);
            AddArea("AI", AreaTypes.AnalogInput);
            AddArea("AE", AreaTypes.AnalogInput);
            AddArea("AQ", AreaTypes.AnalogOutput);
            AddArea("AA", AreaTypes.AnalogOutput);
            AddArea("DB", AreaTypes.DbMemory);
            AddArea("V", AreaTypes.VariableMemory);
            AddArea("M", AreaTypes.FlagMemory);
            AddArea("SM", AreaTypes.SystemMemory);
            AddArea("S", AreaTypes.StageMemory);
            AddArea("AC", AreaTypes.Accumulator);
            AddArea("T", AreaTypes.Timer);
            AddArea("C", AreaTypes.Counter);
            AddArea("Z", AreaTypes.Counter);
            AddArea("HC", AreaTypes.HighspeedCount);
            AddArea("HZ", AreaTypes.HighspeedCount);
            AddArea("L", AreaTypes.LocalMemory);
            AddArea("SBR", AreaTypes.SubroutineMemory);

            Areas.Sort(StringList.LongerFirst);
            _areasFirsts = string.Empty;
            foreach (string area in Areas)
                _areasFirsts += area[0];

            if (this.EditorInfo == null)
                this.EditorInfo = new TokenEditorInfo(TokenType.Literal, TokenColor.Number, TokenTriggers.None);
        }

        public override IList<string> GetFirsts()
        {
            StringList result = new StringList();
            result.Add("*");
            result.Add("&");
            result.AddRange(Areas);
            return result;
        }

        protected virtual string ReadNumber(ISourceStream source, string digits)
        {
            int begin = source.PreviewPosition;
            while (!source.EOF() && digits.IndexOf(source.PreviewChar) >= 0)
            {
                source.PreviewPosition++;
            }
            if (source.PreviewPosition > begin)
                return source.Text.Substring(begin, source.PreviewPosition - begin);
            else
                return null;
        }

        /*Addressing
        discrete input	I | E	Note: Case insensitive
        discrete output	Q | A	Note: Case insensitive
        analog input	AI | AE	Note: Case insensitive
        analog output	AQ | AA	Note: Case insensitive
        db memory	    DB	Note: Case insensitive
        variable memory	V	Note: Case insensitive
        flag memory	    M	Note: Case insensitive
        system memory	SM	Note: Case insensitive
        Stage memory	S	Note: Case insensitive
        accumulator	    AC	Note: Case insensitive
        Timer	        T	Note: Case insensitive
        counter	        C | Z	Note: Case insensitive
        high speed counter	HC | HZ	Note: Case insensitive
        data block	    DB	Note: Case insensitive
        local memory	L	Note: Case insensitive
        bit point	    unsigned number . 0 | 1 | ?| 7
        size prefix	    B | W | D                                       Note: Case insensitive
        Direct prefix	        discrete input | discrete output | variable memory | flag memory | system memory | stage memory
        peripheral prefix	    analog input | analog output
        timer counter prefix	timer | counter
        bit address	            direct prefix [X] bit point
        byte word dword address	direct prefix size prefix unsigned number
        peripheral address	    peripheral prefix [W] unsigned number
        timer counter address	timer counter prefix ((X unsigned number) | unsigned number)
        high speed counter address	high speed counter  unsigned number
        accumulator address	    accumulator [size prefix] unsigned number
        db number	            DB unsigned number
        db address	            [db number .] data block ([X] bit point) | (size prefix  unsigned number)
        Direct address	        bit address | byte word dword address | peripheral address |timer counter address | high speed counter address | accumulator address |db address
        indirect address	    * ((accumulator [ D| d ]) | (variable memory D | d ) | (local memory D | d) | ([db number .] data block D | d)) unsigned number
        address of	            & (variable memory | discrete input | discrete output | timer | counter | flag memory | system memory ([db number.] data block) size prefix unsigned number
        */
        public override Token TryMatch(ParsingContext context, ISourceStream source)
        {
            AddressDetails details = new AddressDetails();

            //1. Optional address prefix
            if (source.PreviewChar == '*' || source.PreviewChar == '&')
            {
                details.Prefix = source.PreviewChar.ToString();
                source.PreviewPosition++;
            }

            //2. Read area type
            AreaTypes areaType = AreaTypes.None;
            if (_areasFirsts.IndexOf(source.PreviewChar) >= 0)
                foreach (string area in Areas)
                {
                    if (source.MatchSymbol(area, true))
                    {
                        //We found prefix
                        source.PreviewPosition += area.Length;
                        AreaTypeMaps.TryGetValue(area, out areaType);
                        details.Area = area;
                        break;
                    };
                }
            if (areaType == AreaTypes.None)
                return null;
            details.AreaTypeCode = (int)areaType;

            //3. Check db number if Db memory
            if (areaType == AreaTypes.DbMemory)
            {
                int pos = source.PreviewPosition;
                string DbNumber = ReadNumber(source, Strings.DecimalDigits);
                if (DbNumber != null &&
                    source.MatchSymbol(".DB", true))
                {
                    details.DbNumber = "DB"+DbNumber+".";
                    source.PreviewPosition += 3;
                }
                else
                {
                    source.PreviewPosition = pos;
                }
            }

            //4. Read size prefix, check optional or required behind.
            if (_sizePrefixes.IndexOf(source.PreviewChar) >= 0 ||
                char.ToUpper(source.PreviewChar) == 'X')
            {
                details.SizePrefix = char.ToUpper(source.PreviewChar).ToString();
                source.PreviewPosition++;
            }

            //5. Address offset, include byte-word-dword and bit
            details.Offset = ReadNumber(source, Strings.DecimalDigits);
            if (details.Offset == null)
                return null;
            if (source.PreviewChar == '.')
            {
                source.PreviewPosition++;
                details.BitPoint = ReadNumber(source, Strings.OctalDigits);
                if (details.BitPoint == null)
                    return null;
            }

            sBLOCK200_ADDRESS_STRUCT value = new sBLOCK200_ADDRESS_STRUCT();
            if (ValidateSyntax(details, ref value))
            {
                var t = source.CreateToken(this.OutputTerminal, value); 
                t.Details = details;
                return t;
            }
            return source.CreateErrorToken("Error Address");
        }

        protected bool ValidateSyntax(AddressDetails details, ref sBLOCK200_ADDRESS_STRUCT value)
        {
            AreaTypes areaType = (AreaTypes)details.AreaTypeCode;
            if (details.Prefix == "*")
            {
                if (areaType != AreaTypes.Accumulator &&
                    areaType != AreaTypes.VariableMemory &&
                    areaType != AreaTypes.LocalMemory &&
                    areaType != AreaTypes.DbMemory)
                    return false;

                if (areaType != AreaTypes.Accumulator &&
                    details.SizePrefix != "D")
                    return false;

                if (details.BitPoint != null)
                    return false;
            }
            else if (details.Prefix == "&")
            {
                if (details.BitPoint != null)
                    return false;

                if (!_addressOfPrefixes.Contains(areaType))
                    return false;
            }
            else
            {
                if (_directPrefixes.Contains(areaType) || areaType == AreaTypes.DbMemory)
                {
                    if (details.BitPoint != null && 
                        details.SizePrefix != null && details.SizePrefix != "X")
                        return false;

                    if (details.BitPoint == null && 
                        (details.SizePrefix == null || details.SizePrefix == "X"))
                        return false;
                }
                if (_peripheralPrefixes.Contains(areaType))
                    if (details.BitPoint != null || 
                        details.SizePrefix != null && details.SizePrefix != "W")
                        return false;

                if (_timerCounterPrefixes.Contains(areaType))
                    if (details.BitPoint != null || 
                        details.SizePrefix != null && details.SizePrefix != "W")
                        return false;

                if (areaType == AreaTypes.HighspeedCount)
                    if (details.BitPoint != null || details.SizePrefix != null)
                        return false;

                if (areaType == AreaTypes.Accumulator)
                    if (details.BitPoint != null || details.SizePrefix == "X")
                        return false;
            }

            if (details.Prefix == "*")
                value.mode = eBLOCK200_ADDRESSING_MODE.eBLOCK200_ADDRESSING_MODE_INDIRECT_ADDRESS;
            else if (details.Prefix == "&")
                value.mode = eBLOCK200_ADDRESSING_MODE.eBLOCK200_ADDRESSING_MODE_ADDRESS_OF;
            else
                value.mode = eBLOCK200_ADDRESSING_MODE.eBLOCK200_ADDRESSING_MODE_DIRECT_ADDRESS;

            value.size = Descriptor.GetSizeByDescriptor(details.SizePrefix);
            value.memoryArea = details.AreaTypeCode;
            value.nOffset = int.Parse(details.Offset);
            if (details.BitPoint != null)
            {
                value.size = eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_BIT;
                value.nOffset <<= 3;
                value.nOffset += int.Parse(details.BitPoint);
            }
            switch (areaType)
            {
                case AreaTypes.SubroutineMemory:
                    value.size = eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_WORD;
                    break;
                case AreaTypes.Timer:
                case AreaTypes.Counter:
                    value.size = eBLOCK200_OPERAND_SIZE.eBLOCK200_OPERAND_SIZE_BIT;
                    break;
            }

            return true;
        }
        #endregion

        #region private utilities
        #endregion
    }
}
