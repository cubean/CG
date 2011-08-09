using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;
using Irony.Ast;

namespace STLCompiler
{
    [Language("STL", "1.0", "PLC STL data format")]
    public class STLGrammar : Grammar
    {
        public STLGrammar() : base(false)
        {
            #region Letters and Digits
            /*  	
            letter	        A | B | .. | Z | a | b | .. |z
            digit	        0 | 1 | .. | 9
            bit digit	    0|1| .. |7
            binary digit	0 | 1 | _
            hex digit	    digit | A | .. | F | a | .. | f | _
            character	    letter | ASCII 128 | .. | ASCII 255 | _
            first identifier character	character | space
            other identifier character	character | digit | space
            Local Variables (Local ToTerms)	[ # ] identifier
            */
            var semi = ToTerm(";", "semi");
            var semi_opt = new NonTerminal("semi?", Empty | semi);
            var dot = ToTerm(".", "dot");
            var comma = ToTerm(",", "comma");
            var comma_opt = new NonTerminal("comma_opt", Empty | comma);
            var equal = ToTerm("=", "equal");

            var digits = new NumberLiteral("decimal", NumberOptions.IntOnly | NumberOptions.DisableQuickParse);
            var binarys = new NumberLiteral("binary", NumberOptions.IntOnly | NumberOptions.DisableQuickParse | NumberOptions.Binary | NumberOptions.AllowUnderscore);
            var octals = new NumberLiteral("octal", NumberOptions.IntOnly | NumberOptions.DisableQuickParse | NumberOptions.Octal);
            var hexs = new NumberLiteral("hex", NumberOptions.IntOnly | NumberOptions.DisableQuickParse | NumberOptions.Hex | NumberOptions.AllowUnderscore);
            var identifier = TerminalFactory.CreateCSharpIdentifier("identifier");
            var global_variable = new NonTerminal("global_variable", typeof(Variable));
            var local_variable = new NonTerminal("local_variable", typeof(Variable));
            KeyTerm colon = ToTerm(":", "colon");

            binarys.AddPrefix("2#", NumberOptions.Binary);
            hexs.AddPrefix("16#", NumberOptions.Hex);
            global_variable.SetFlag(TermFlags.IsConstant);
            local_variable.SetFlag(TermFlags.IsConstant);

            global_variable.Rule = identifier;
            local_variable.Rule = "#" + identifier;
            #endregion

            #region Program Comments
            /*
            comment characters	printable character CR
            comment	            // { comment characters } CR
            */
            var comment = new CommentTerminal("comment", "//", "\r", "\n");
            var comment_opt = new NonTerminal("comment?");
            var line_comment = new NonTerminal("line_comment");
            var multi_line_comment = new NonTerminal("multi_line_comment");
            var empty_line = new NonTerminal("empty_line");
            var empty_lines = new NonTerminal("empty_line+");
            var empty_lines_opt = new NonTerminal("empty_line*");

            multi_line_comment.SetFlag(TermFlags.IsList);
            empty_lines.SetFlag(TermFlags.IsList);
            empty_lines_opt.SetFlag(TermFlags.IsListContainer);

            comment_opt.Rule = comment | Empty;
            line_comment.Rule = comment + NewLine;
            multi_line_comment.Rule = multi_line_comment + line_comment | line_comment;
            empty_line.Rule = NewLine | line_comment;
            empty_lines.Rule = empty_lines + empty_line | empty_line;
            empty_lines_opt.Rule = empty_lines | Empty;
            #endregion

            #region Instruction mnemonic terminal
            // Bit logic
            var bit_logic_mnemonic = new NonTerminal("bit_logic_mnemonic");
            var clock_mnemonic = new NonTerminal("clock_mnemonic");
            var communication_mnemonic = new NonTerminal("communication_mnemonic");
            var compare_mnemonic = new NonTerminal("compare_mnemonic");
            var convert_mnemonic = new NonTerminal("convert_mnemonic");
            var counter_mnemonic = new NonTerminal("counter_mnemonic");
            var floatpoint_math_mnemonic = new NonTerminal("floatpoint_math_mnemonic");
            var integer_math_mnemonic = new NonTerminal("integer_math_mnemonic");
            var logical_mnemonic = new NonTerminal("logical_mnemonic");
            var move_mnemonic = new NonTerminal("move_mnemonic");
            var program_control_mnemonic = new NonTerminal("program_control_mnemonic");
            var shift_rotate_mnemonic = new NonTerminal("shift_rotate_mnemonic");
            var string_mnemonic = new NonTerminal("string_mnemonic");
            var table_mnemonic = new NonTerminal("table_mnemonic");
            var timer_mnemonic = new NonTerminal("timer_mnemonic");
            var subroutine_mnemonic = new NonTerminal("subroutine_mnemonic");

            bit_logic_mnemonic.Rule =
                ToTerm("LD") | "A" | "O" |
                "LDN" | "AN" | "ON" |
                "LDI" | "AI" | "OI" |
                "LDNI" | "ANI" | "ONI" |
                "NOT" | "EU" | "ED" | "ALD" | "OLD" |
                "LPS" | "LDS" | "LRD" | "LPP" | "=" | "=I" |
                "S" | "SI" | "R" | "RI" | "AENO" | "NOP";
            clock_mnemonic.Rule =
                ToTerm("TODR") | "TODW" | "TODRX" | "TODWX";
            communication_mnemonic.Rule =
                ToTerm("XMT") | "RCV" | "NETR" | "NETW" | "GPA" | "SPA";
            compare_mnemonic.Rule =
                ToTerm("LDB=") | "AB=" | "OB=" |
                "LDB<>" | "AB<>" | "OB<>" |
                "LDB>=" | "AB>=" | "OB>=" |
                "LDB<=" | "AB<=" | "OB<=" |
                "LDB>" | "AB>" | "OB>" |
                "LDB<" | "AB<" | "OB<" |
                "LDW=" | "AW=" | "OW=" |
                "LDW<>" | "AW<>" | "OW<>" |
                "LDW>=" | "AW>=" | "OW>=" |
                "LDW<=" | "AW<=" | "OW<=" |
                "LDW>" | "AW>" | "OW>" |
                "LDW<" | "AW<" | "OW<" |
                "LDD=" | "AD=" | "OD=" |
                "LDD<>" | "AD<>" | "OD<>" |
                "LDD>=" | "AD>=" | "OD>=" |
                "LDD<=" | "AD<=" | "OD<=" |
                "LDD>" | "AD>" | "OD>" |
                "LDD<" | "AD<" | "OD<" |
                "LDR=" | "AR=" | "OR=" |
                "LDR<>" | "AR<>" | "OR<>" |
                "LDR>=" | "AR>=" | "OR>=" |
                "LDR<=" | "AR<=" | "OR<=" |
                "LDR>" | "AR>" | "OR>" |
                "LDR<" | "AR<" | "OR<" |
                "LDS=" | "AS=" | "OS=" |
                "LDS<>" | "AS<>" | "OS<>";
            convert_mnemonic.Rule =
                ToTerm("BTI") | "ITB" | "ITD" | "ITS" |
                "DTI" | "DTR" | "DTS" |
                "ROUND" | "TRUNC" | "RTS" |
                "BCDI" | "IBCD" |
                "ITA" | "DTA" | "RTA" | "ATH" | "HTA" |
                "STI" | "STD" | "STR" |
                "DECO" | "Encoder" | "SEG";
            counter_mnemonic.Rule =
                ToTerm("CTU") | "CTD" | "CTUD" | "HDEF" | "HSC" | "PLS";
            floatpoint_math_mnemonic.Rule =
                ToTerm("+R") | "-R" | "*R" | "/R" |
                "SQRT" | "SIN" | "COS" | "TAN" | "LN" | "EXP" | "PID";
            integer_math_mnemonic.Rule =
                ToTerm("+I") | "-I" | "*I" | "/I" |
                "+D" | "-D" | "*D" | "/D" |
                "MUL" | "DIV" | 
                "INCB" | "INCW" | "INCD" |
                "DECB" | "DECW" | "DECD";
            logical_mnemonic.Rule =
                ToTerm("INVB") | "INVW" | "INVD" |
                "ANDB" | "ANDW" | "ANDD" |
                "ORB" | "ORW" | "ORD" |
                "XORB" | "XORW" | "XORD";
            move_mnemonic.Rule =
                ToTerm("MOVB") | "MOVW" | "MOVD" | "MOVR" |
                "BMB" | "BMW" | "BMD" |
                "SWAP" | "BIR" | "BIW";
            program_control_mnemonic.Rule =
                ToTerm("FOR") | "NEXT" | "JMP" | "LBL" |
                "LSCR" | "SCRT" | "SCRE" | "CSCRE" |
                "END" | "STOP" | "WDR" | "DLED";
            shift_rotate_mnemonic.Rule =
                ToTerm("SLB") | "SLW" | "SLD" |
                "SRB" | "SRW" | "SRD" |
                "RLB" | "RLW" | "RLD" |
                "RRB" | "RRW" | "RRD" | "SHRB";
            string_mnemonic.Rule =
                ToTerm("SLEN") | "SCPY" | "SSCPY" | "SCAT" | "SFND" | "CFND";
            table_mnemonic.Rule =
                ToTerm("FILL") | "ATT" | "FND=" | "FND<>" | "FND<" | "FND>" | "LIFO" | "FIFO";
            timer_mnemonic.Rule =
                ToTerm("TON") | "TONR" | "TOF" | "BITIM" | "CITIM";
            subroutine_mnemonic.Rule =
                ToTerm("CALL") | "ENI" | "ENS";
            #endregion

            #region Constants
            /* 
            Boolean constant	        TRUE | FALSE | ON | OFF                        Note: Case insensitive
            constant descriptor     	B# | BYTE# | W# | WORD# | DW# | DWORD#         Note: Case insensitive
            unsigned integer	        [constant descriptor]digit {digit}
            number	                    digit {digit}
            signed number	            [ + | - ] number
            signed integer	            [constant descriptor]signed number
            binary integer	            [constant descriptor]2# binary digit { binary digit }
            hex integer	                [constant descriptor]16# hex digit { hex digit }
            exponent	                ( E | e ) signed number
            real constant	            signed number . {exponent}
            real constant	            number {exponent}
            real constant	            signed number . number {exponent}
            printable character	        ASCII 32 | .. | ASCII 255 excluding DEL and ?
            two character hex	        hex digit  hex digit
            character representation	printable character | $$ | $ | $L | $l | $N | $n | $P | $p | $R | $r | $T | $t | $?| $" 
                $two character hex
                    Note: The leading $ sign is a control character          (Also known as an escape code)
                Escape Code	    Interpretation
                $$	            Single dollar sign
                $?              Single quotation mark character
                $"	            Double quotation mark character
                $L or $l	    A line feed character
                $N or $n	    A new line character
                $P or $p	    Form feed, new page
                $R or $r	    Carriage return character
                $T or $t	    Tab character
            
            ASCII character constant	[constant descriptor]'character representation'
            ASCII string constant	    "character representation" 
            constant	                Boolean constant | binary digit | unsigned integer | 
                                        signed integer | binary integer | hex integer | 
                                        real constant | ASCII constant
            */
            var real = new NumberLiteral("Real", NumberOptions.AllowSign);
            var bool_constant = new NonTerminal("bool_constant", typeof(BoolConstrant));
            var constant_descriptor = new NonTerminal("constant_descriptor", typeof(Descriptor));
            var unsigned_integer = new NonTerminal("unsigned_integer", typeof(NumberConstrant));
            var signed_number = new NumberLiteral("decimal", NumberOptions.IntOnly|NumberOptions.AllowSign);
            var signed_integer = new NonTerminal("signed_integer", typeof(NumberConstrant));
            var binary_integer = new NonTerminal("binary_integer", typeof(NumberConstrant));
            var hex_integer = new NonTerminal("hex_integer", typeof(NumberConstrant));
            var real_constant = new NonTerminal("real_constant", typeof(NumberConstrant));
            var character_representation = new StringLiteral("character_representation", "'");
            var ascii_constant = new NonTerminal("ascii_constant");
            var string_constant = new StringLiteral("string_constant", "\"");
            var constant = new NonTerminal("Constant");

            bool_constant.Rule = ToTerm("TRUE") | "FALSE" | "ON" | "OFF";
            constant_descriptor.Rule = (ToTerm("B") | "BYTE" | "W" | "WORD" | "D" | "DWORD") + "#" | Empty;
            unsigned_integer.Rule = constant_descriptor + digits;
            signed_integer.Rule = constant_descriptor + signed_number;
            binary_integer.Rule = constant_descriptor + binarys;
            hex_integer.Rule = constant_descriptor + hexs;
            real_constant.Rule = real;
            ascii_constant.Rule = constant_descriptor + character_representation;
            constant.Rule = bool_constant |  unsigned_integer | signed_integer | 
                            binary_integer | hex_integer | real_constant | 
                            ascii_constant | string_constant;
            #endregion

            #region Addressing
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
            var address_term = new AddressTerm("address_term");
            var address = new NonTerminal("address", typeof(Address));
            
            address.Rule = address_term;
            #endregion

            #region Local Variable Declaration
            /* 
            ob declaration	        [var declaration]
            int declaration	        [var declaration]
            sbr declaration	        [io var declaration] ob declarations
            io var declaration	    [var input declaration] [var in out declaration] [var output declaration]
            var input declaration	VAR_INPUT  CR decl_list END_VAR  CR
            var output declaration	VAR_OUTPUT  CR decl_list END_VAR CR
            var in out declaration	VAR_IN_OUT  CR decl_list END_VAR  CR
            var declaration	        VAR  CR decl_list END_VAR  CR
            decl list	            {identifier : complex type }; {comment}
            Type	                Base type | user type | string type
            complex type	        Base type | array type | struct type | user type | string type
            init value	            constant | integer ( init value ) | init value , init value
            base type	            BOOL | BYTE | WORD | DWORD | CHAR | INT | DINT | REAL |
            */
            var base_type = new NonTerminal("base_type");
            var init_value = new NonTerminal("init_value");
            var complex_type = new NonTerminal("complex_type");
            var type = new NonTerminal("type");
            var decl = new NonTerminal("decl", typeof(Declaration));
            var decl_line = new NonTerminal("decl_line");
            var decl_list = new NonTerminal("decl+");
            var decl_list_opt = new NonTerminal("decl*");
            var var_input_decl = new NonTerminal("var_input_decl", typeof(VarDecl));
            var var_output_decl = new NonTerminal("var_output_decl", typeof(VarDecl));
            var var_in_out_decl = new NonTerminal("var_in_out_decl", typeof(VarDecl));
            var var_decl = new NonTerminal("var_decl", typeof(VarDecl));
            var var_input_decl_opt = new NonTerminal("var_input_decl?");
            var var_output_decl_opt = new NonTerminal("var_output_decl?");
            var var_in_out_decl_opt = new NonTerminal("var_in_out_decl?");
            var var_decl_opt = new NonTerminal("var_decl?");
            var var_decl_suffix = new NonTerminal("var_decl_suffix");
            var io_var_decl = new NonTerminal("io_var_decl");

            decl_list.SetFlag(TermFlags.IsList);
            decl_list_opt.SetFlag(TermFlags.IsListContainer);
            MarkTransient(decl_line, complex_type, base_type,
                var_input_decl_opt, var_output_decl_opt, var_in_out_decl_opt);

            base_type.Rule = ToTerm("BOOL") | "BYTE" | "WORD" | "DWORD" | "CHAR" | "INT" | "DINT" | "REAL";
            init_value.Rule = init_value + comma + constant | constant | ToTerm("integer") + "(" + constant + ")";
            complex_type.Rule = base_type;
            type.Rule = base_type;
            decl.Rule = identifier + ":" + complex_type + ";" + comment_opt + NewLine;
            decl_line.Rule = decl | empty_line;
            decl_list.Rule = decl_list + decl_line | decl_line;
            decl_list_opt.Rule = decl_list | Empty;
            var_decl_suffix.Rule = NewLine + decl_list_opt + "END_VAR" + NewLine;
            var_input_decl.Rule = "VAR_INPUT" + var_decl_suffix;
            var_output_decl.Rule = "VAR_OUTPUT" + var_decl_suffix;
            var_in_out_decl.Rule = "VAR_IN_OUT" + var_decl_suffix;
            var_decl.Rule = "VAR" + var_decl_suffix;
            var_input_decl_opt.Rule = var_input_decl | Empty;
            var_output_decl_opt.Rule = var_output_decl | Empty;
            var_in_out_decl_opt.Rule = var_in_out_decl | Empty;
            var_decl_opt.Rule = var_decl | Empty;
            io_var_decl.Rule = var_input_decl_opt + var_output_decl_opt + var_in_out_decl_opt;
            #endregion

            #region Program Structure
            /*Note:
            Keywords are case insensitive unless otherwise noted. 

            Block title	    { white space } (TITLE =  comment characters CR | comment)
            Block comment	{comment}
            network title	{ white space } (TITLE =  comment characters CR | comment)
            network comment	{comment}
            line comment	{comment}
            code comment	{comment}
            network number	{ space | unsigned number }
            operand	        { white space } global variable | local variable | constant | direct address | indirect address | address of
            instruction mnemonic	(letter | digit | _ ) { letter | digit | _ }
            instruction	            instruction mnemonic [(white space) { white space } operand {(,| white space) { white space } operand}] ; code comment
            statement list	        {instruction | line comment}
            network	                NETWORK network number (CR | [network title] [network comment]) statement list
            ob number	            OB unsigned number
            organization block	    ORGANIZATION_BLOCK (ob number) (CR | [block title] [block comment])ob declarations BEGIN [network] END_ORGANIZATION_BLOCK CR
            sbr number	            SBR unsigned number
            subroutine block	    SUBROUTINE_BLOCK (sbr number) (CR [block title] [block comment])sbr declarations BEGIN [network] END_SUBROUTINE_BLOCK CR
            int number	            INT unsigned_number
            interrupt block	        INTERRUPT_BLOCK (int number) (CR [block title] [block comment]) int declarations BEGIN [network] END_INTERRUPT_BLOCK CR
             */
            var title_content = new FreeTextLiteral("Title", "\r", "\n");

            var title = new NonTerminal("title");
            var title_opt = new NonTerminal("title?");
            var network_number = new NonTerminal("network_number");
            var mnemonic = new NonTerminal("mnemonic");
            var operand = new NonTerminal("operand");
            var operand_list = new NonTerminal("operand+");
            var operand_list_opt = new NonTerminal("operand*");
            var instruction = new NonTerminal("instruction", typeof(Instruction));
            var statement = new NonTerminal("statement");
            var statement_list = new NonTerminal("statement_list");
            var statement_list_opt = new NonTerminal("statement*");
            var network = new NonTerminal("network", typeof(Network));
            var network_list = new NonTerminal("network+");
            var network_list_opt = new NonTerminal("network*", typeof(NodeList));
            var ob_number = new NonTerminal("ob_number");
            var block_1 = new NonTerminal("block_1");
            var block_2 = new NonTerminal("block_2");
            var organization_block = new NonTerminal("organization_block", typeof(POU));
            var sbr_number = new NonTerminal("sbr_number");
            var subroutine_block = new NonTerminal("subroutine_block", typeof(POU));
            var int_number = new NonTerminal("int_number");
            var interrupt_block = new NonTerminal("interrupt_block", typeof(POU));
            var subroutine_block_list = new NonTerminal("subroutine_block+");
            var interrupt_block_list = new NonTerminal("interrupt_block+");
            var subroutine_block_list_opt = new NonTerminal("subroutine_block*", typeof(NodeList));
            var interrupt_block_list_opt = new NonTerminal("interrupt_block*", typeof(NodeList));
            var program = new NonTerminal("program", typeof(Block));

            operand_list.SetFlag(TermFlags.IsList);
            statement_list.SetFlag(TermFlags.IsList);
            network_list.SetFlag(TermFlags.IsList);
            subroutine_block_list.SetFlag(TermFlags.IsList);
            interrupt_block_list.SetFlag(TermFlags.IsList);
            operand_list_opt.SetFlag(TermFlags.IsListContainer);
            statement_list_opt.SetFlag(TermFlags.IsListContainer);
            network_list_opt.SetFlag(TermFlags.IsListContainer);
            subroutine_block_list_opt.SetFlag(TermFlags.IsListContainer);
            interrupt_block_list_opt.SetFlag(TermFlags.IsListContainer);
            MarkTransient(constant, mnemonic, operand, statement);

            mnemonic.Rule = bit_logic_mnemonic | clock_mnemonic | communication_mnemonic |
                compare_mnemonic | convert_mnemonic | counter_mnemonic | floatpoint_math_mnemonic |
                integer_math_mnemonic | logical_mnemonic | move_mnemonic | program_control_mnemonic |
                shift_rotate_mnemonic | string_mnemonic | table_mnemonic | timer_mnemonic | subroutine_mnemonic;
            title.Rule = ToTerm("TITLE") + "=" + title_content + NewLine;
            title_opt.Rule = title | Empty;
            operand.Rule = global_variable | local_variable | constant | address;
            operand_list.Rule = operand_list + comma + operand | operand;
            operand_list_opt.Rule = operand_list | Empty;
            instruction.Rule = mnemonic + operand_list_opt + semi_opt + comment_opt + NewLine;
            statement.Rule = instruction | line_comment | NewLine;
            statement_list.Rule = statement_list + statement | statement;
            statement_list_opt.Rule = statement_list | Empty;
            network_number.Rule = digits | Empty;
            network.Rule = "NETWORK" + network_number + comment_opt + NewLine + title_opt + statement_list_opt;
            network_list.Rule = network_list + network | network;
            network_list_opt.Rule = network_list | Empty;
            block_1.Rule = identifier + ":" + identifier + NewLine + title_opt + empty_lines_opt;
            block_2.Rule = "BEGIN" + NewLine + network_list_opt;
            organization_block.Rule = 
                "ORGANIZATION_BLOCK" + block_1 + var_decl_opt + block_2 + "END_ORGANIZATION_BLOCK" + NewLine;
            subroutine_block.Rule =
                "SUBROUTINE_BLOCK" + block_1 + io_var_decl + var_decl_opt + block_2 + "END_SUBROUTINE_BLOCK" + NewLine;
            interrupt_block.Rule =
                "INTERRUPT_BLOCK" + block_1 + var_decl_opt + block_2 + "END_INTERRUPT_BLOCK" + NewLine;
            subroutine_block_list.Rule = subroutine_block_list + subroutine_block | subroutine_block;
            interrupt_block_list.Rule = interrupt_block_list + interrupt_block | interrupt_block;
            subroutine_block_list_opt.Rule = subroutine_block_list | Empty;
            interrupt_block_list_opt.Rule = interrupt_block_list | Empty;
            program.Rule = organization_block + subroutine_block_list_opt + interrupt_block_list_opt;
            #endregion

            //Set grammar root
            this.Root = program;
            this.WhitespaceChars = " \t\v";

            MarkPunctuation("(", ")", ":", ";", ".", ",");
            MarkPunctuation(semi_opt, comma_opt, 
                comment, comment_opt, line_comment, empty_line, empty_lines, empty_lines_opt);


            this.LanguageFlags = LanguageFlags.NewLineBeforeEOF | 
                LanguageFlags.CreateAst;
        }//constructor
    }//class
}
