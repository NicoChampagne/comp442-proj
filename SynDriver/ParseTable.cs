using System;
using System.Collections.Generic;
using System.Text;

namespace SynSemDriver
{
    public static class ParseTable
    {
        public static Dictionary<(string, string), string> NonTerminalSymbolToProductionDictionary = new Dictionary<(string, string), string>()
        {
            // First Sets
            { ("START", "main"), "Prog"}, { ("START", "class"), "Prog"}, { ("START", "id"), "Prog"},
            { ("Prog", "main"), "RepClassDecl RepFuncDef main FuncBody"}, { ("Prog", "class"), "RepClassDecl RepFuncDef main FuncBody"}, { ("Prog", "id"), "RepClassDecl RepFuncDef main FuncBody"},
            { ("RepFuncDef", "id"), "FuncDef RepFuncDef"}, //{ ("RepFuncDef", "EPSILON"), "EPSILON"},
            { ("RepClassDecl", "class"), "ClassDecl RepClassDecl"}, //{ ("RepClassDecl", "EPSILON"), "EPSILON"},
            { ("ClassDecl", "class"), "class Id OptInherits { RepMembers } ;"},
            { ("Id", "id"), "id"},
            { ("OptInherits", "inherits"), "inherits Id RepExtraInherits"},
            { ("RepExtraInherits", ","), "RepExtraInherits , Id"},
            { ("RepMembers", "public"), "Visibility MemberDecl RepMembers"}, { ("RepMembers", "private"), "Visibility MemberDecl RepMembers"},
            { ("Visibility", "public"), "public"}, { ("Visibility", "private"), "private"},
            { ("MemberDecl", "id"), "Id VarOrFunc ;"}, { ("MemberDecl", "integer"), "VarDecl"}, { ("MemberDecl", "float"), "VarDecl"},
            { ("VarOrFunc", "("), "FuncDeclEnd"}, { ("VarOrFunc", "id"), "VarDeclEnd"},
            { ("VarDeclEnd", "id"), "Id RepArraySize"},
            { ("FuncDeclEnd", "("), "( FParams ) : VoidOrType"},
            { ("FuncHead", "id"), "Id ( FParams ) : VoidOrType"},
            { ("OptId", "id"), "Id sr"}, //challenge, what is sr
            { ("FuncDef", "id"), "FuncHead FuncBody"},
            { ("FuncBody", "local"), "OptLocalVD do RepStatement end"}, { ("FuncBody", "do"), "OptLocalVD do RepStatement end"},
            { ("OptLocalVD", "local"), "local RepVarDecl"},
            { ("VarDecl", "integer"), "Type Id RepArraySize ;"}, { ("VarDecl", "float"), "Type Id RepArraySize ;"}, { ("VarDecl", "id"), "Type Id RepArraySize ;"},
            { ("RepVarDecl", "integer"), "VarDecl RepVarDecl"}, { ("RepVarDecl", "float"), "VarDecl RepVarDecl"}, { ("RepVarDecl", "id"), "VarDecl RepVarDecl"},
            { ("Statement", "if"), "if ( RelExpr ) then StatBlock else StatBlock ;"}, { ("Statement", "while"), "while ( RelExpr ) StatBlock ;"}, { ("Statement", "read"), "read ( Variable ) ;"}, { ("Statement", "write"), "write ( Expr ) ;"}, { ("Statement", "return"), "return ( Expr ) ;"}, { ("Statement", "id"), "RepIdnest AssignOrFuncCall ;"}, //Assignstat or functioncall is still ambiguous
            { ("RepStatement", "if"), "Statement RepStatement"}, { ("RepStatement", "while"), "Statement RepStatement"}, { ("RepStatement", "read"), "Statement RepStatement"}, { ("RepStatement", "write"), "Statement RepStatement"}, { ("RepStatement", "return"), "Statement RepStatement"}, { ("RepStatement", "id"), "Statement RepStatement"}, 
            { ("AssignOrFuncCall", "("), "( AParams )"}, { ("AssignOrFuncCall", "["), "RepIndice AssignOp StartFuncOrExpr"}, { ("AssignOrFuncCall", "="), "AssignOp StartFuncOrExpr"},
            { ("StartFuncOrExpr", "id"), "Variable ExprOrFunc"}, { ("StartFuncOrExpr", "intnum"), "Expr"}, { ("StartFuncOrExpr", "floatnum"), "Expr"}, { ("StartFuncOrExpr", "not"), "Expr"}, { ("StartFuncOrExpr", "+"), "Expr"}, { ("StartFuncOrExpr", "-"), "Expr"},
            { ("ExprOrFunc", "("), "( AParams )"}, { ("ExprOrFunc", "intnum"), "Expr"}, { ("ExprOrFunc", "floatnum"), "Expr"}, { ("ExprOrFunc", "not"), "Expr"}, { ("ExprOrFunc", "+"), "AddOrMultOp Term"}, { ("ExprOrFunc", "-"), "AddOrMultOp Term"}, 
            { ("StatBlock", "do"), "do RepStatement end"}, { ("StatBlock", "if"), "Statement"}, { ("StatBlock", "while"), "Statement"}, { ("StatBlock", "read"), "Statement"}, { ("StatBlock", "write"), "Statement"}, { ("StatBlock", "return"), "Statement"}, { ("StatBlock", "id"), "Statement"},
            { ("Expr", "intnum"), "ArithExpr"}, { ("Expr", "floatnum"), "ArithExpr"}, { ("Expr", "("), "RelExpr"}, { ("Expr", "not"), "RelExpr"}, { ("Expr", "+"), "ArithExpr"}, { ("Expr", "-"), "ArithExpr"}, { ("Expr", "id"), "ArithExpr"},
            { ("RelExpr", "intnum"), "ArithExpr RelOp ArithExpr"}, { ("RelExpr", "floatnum"), "ArithExpr RelOp ArithExpr"}, { ("RelExpr", "("), "ArithExpr RelOp ArithExpr"}, { ("RelExpr", "not"), "ArithExpr RelOp ArithExpr"}, { ("RelExpr", "+"), "ArithExpr RelOp ArithExpr"}, { ("RelExpr", "-"), "ArithExpr RelOp ArithExpr"}, { ("RelExpr", "id"), "ArithExpr RelOp ArithExpr"},
            { ("ArithExpr", "intnum"), "Term OptMultArith"}, { ("ArithExpr", "floatnum"), "Term OptMultArith"}, { ("ArithExpr", "("), "Term OptMultArith"}, { ("ArithExpr", "not"), "Term OptMultArith"}, { ("ArithExpr", "id"), "Term OptMultArith"},
            { ("OptMultArith", "+"), "AddOrMultOp ArithExpr"}, { ("OptMultArith", "-"), "AddOrMultOp ArithExpr"}, { ("OptMultArith", "*"), "AddOrMultOp ArithExpr"}, { ("OptMultArith", "/"), "AddOrMultOp ArithExpr"},
            { ("Sign", "+"), "+"}, { ("Sign", "-"), "-"},
            { ("Term", "intnum"), "Factor"}, { ("Term", "floatnum"), "Factor"}, { ("Term", "("), "Factor"}, { ("Term", "not"), "Factor"}, { ("Term", "+"), "Factor"}, { ("Term", "-"), "Factor"}, { ("Term", "id"), "Factor"},
            { ("Factor", "intnum"), "intnum"}, { ("Factor", "floatnum"), "floatnum"}, { ("Factor", "("), "( ArithExpr )"}, { ("Factor", "not"), "not Factor"}, { ("Factor", "+"), "Sign Factor"}, { ("Factor", "-"), "Sign Factor"}, { ("Factor", "id"), "Variable"},
            { ("Variable", "id"), "RepIdnest RepIndice"},
            { ("FunctionCall", "id"), "RepIdnest ( AParams )"},
            { ("Idnest", "id"), "Id OptNest"},
            { ("OptNest", "."), "."},
            { ("RepIdnest", "id"), "Idnest RepIdnest"},
            { ("Indice", "["), "[ ArithExpr ]"},
            { ("RepIndice", "["), "Indice RepIndice"},
            { ("ArraySize", "["), "[ OptIntNum ]"}, 
            { ("RepArraySize", "["), "ArraySize RepArraySize"},
            { ("Type", "integer"), "integer"}, { ("Type", "float"), "float"}, { ("Type", "id"), "Id"},
            { ("FParams", "integer"), "Type Id RepArraySize RepFParamsTail"}, { ("FParams", "float"), "Type Id RepArraySize RepFParamsTail"}, { ("FParams", "id"), "Type Id RepArraySize RepFParamsTail"},
            { ("AParams", "intnum"), "Expr RepAParamsTail"},{ ("AParams", "floatNum"), "Expr RepAParamsTail"}, { ("AParams", "("), "Expr RepAParamsTail"}, { ("AParams", "not"), "Expr RepAParamsTail"}, { ("AParams", "+"), "Expr RepAParamsTail"}, { ("AParams", "-"), "Expr RepAParamsTail"}, { ("AParams", "id"), "Expr RepAParamsTail"},
            { ("AParamsTail", ","), ", Expr"},
            { ("RepAParamsTail", ","), "AParamsTail RepAParamsTail"},
            { ("FParamsTail", ","), ", Type Id RepArraySize"},
            { ("RepFParamsTail", ","), "FParamsTail RepFParamsTail"},
            { ("AssignOp", "="), "="},
            { ("RelOp", "eq"), "eq"},{ ("RelOp", "neq"), "neq"}, { ("RelOp", "lt"), "lt"}, { ("RelOp", "gt"), "gt"}, { ("RelOp", "leq"), "leq"}, { ("RelOp", "geq"), "geq"},
            { ("AddOp", "+"), "+"},{ ("AddOp", "-"), "-"}, { ("AddOp", "or"), "or"},
            { ("MultOp", "*"), "*"}, { ("MultOp", "/"), "/"}, { ("MultOp", "and"), "and"},
            { ("AddOrMultOp", "*"), "MultOp"}, { ("AddOrMultOp", "/"), "MultOp"}, { ("AddOrMultOp", "+"), "AddOp"}, { ("AddOrMultOp", "-"), "AddOp"},
            { ("VoidOrType", "integer"), "Type"}, { ("VoidOrType", "float"), "Type"}, { ("VoidOrType", "id"), "Type"}, { ("VoidOrType", "void"), "void"},
            { ("OptIntNum", "intnum"), "intnum"},
            // Follow Sets
            { ("RepFuncDef", "main"), "EPSILON"},
            { ("RepClassDecl", "id"), "EPSILON"}, { ("RepClassDecl", "main"), "EPSILON"},
            { ("OptInherits", "{"), "EPSILON"},
            { ("RepExtraInherits", "{"), "EPSILON"},
            { ("RepMembers", "}"), "EPSILON"},
            { ("OptLocalVD", "do"), "EPSILON"},
            //{ ("VarDecl", "public"), "VarDecl -> Type id RepArraySize ;"}, { ("VarDecl", "private"), "VarDecl -> Type id RepArraySize ;"}, { ("VarDecl", "}"), "VarDecl -> Type id RepArraySize ;"}, { ("VarDecl", "do"), "VarDecl -> Type id RepArraySize ;"},
            { ("RepVarDecl", "do"), "EPSILON"},
            //{ ("Statement", "end"), "Statement → if ( RelExpr ) then StatBlock else StatBlock ;"}, { ("Statement", "else"), "Statement -> if ( RelExpr ) then StatBlock else StatBlock ;"}, { ("Statement", ";"), "Statement -> AssignStat ;"},
            { ("RepStatement", "end"), "EPSILON"},
            //{ ("AssignStat", ";"), "AssignStat → Variable AssignOp Expr"},
            { ("StatBlock", ";"), "EPSILON"}, { ("StatBlock", "else"), "EPSILON"},
            { ("AssignOrFuncCall", ";"), "EPSILON"},
            { ("ExprOrFunc", ";"), "EPSILON"},
            //{ ("Expr", ")"), "Expr → ArithExpr"}, { ("Expr", ";"), "Expr → ArithExpr"}, { ("Expr", ","), "Expr → ArithExpr"},
            //{ ("RelExpr", ")"), "RelExpr → ArithExpr RelOp ArithExpr"}, { ("RelExpr", ";"), "RelExpr → ArithExpr RelOp ArithExpr"}, { ("RelExpr", ","), "RelExpr → ArithExpr RelOp ArithExpr"},
            //{ ("ArithExpr", ")"), "ArithExpr AddOp Term"}, //{ ("ArithExpr", ";"), "ArithExpr → Term"}, { ("ArithExpr", "eq"), "ArithExpr → Term"}, { ("ArithExpr", "neq"), "ArithExpr → Term"}, { ("lt", "+"), "ArithExpr -> Term"}, { ("ArithExpr", "gt"), "ArithExpr → Term"}, { ("ArithExpr", "or"), "ArithExpr → ArithExpr AddOp Term"}, { ("ArithExpr", "]"), "ArithExpr → Term"}, { ("ArithExpr", ","), "ArithExpr → Term"},
            { ("OptMultArith", ")"), "EPSILON"}, { ("OptMultArith", ";"), "EPSILON"}, { ("OptMultArith", "eq"), "EPSILON"}, { ("OptMultArith", "neq"), "EPSILON"}, { ("OptMultArith", "lt"), "EPSILON"}, { ("OptMultArith", "gt"), "EPSILON"}, { ("OptMultArith", "leq"), "EPSILON"}, { ("OptMultArith", "geq"), "EPSILON"}, { ("OptMultArith", "]"), "EPSILON"}, { ("OptMultArith", ","), "EPSILON"},
            //{ ("Sign", "intnum"), "Sign → +"}, { ("Sign", "floatnum"), "Sign → -"},
            //{ ("Term", "intnum"), "Term → Factor"}, { ("Term", "floatnum"), "Term → Factor"}, { ("Term", "("), "Term → Factor"}, { ("Term", "not"), "Term → Factor"}, { ("Term", "+"), "Term -> Factor"}, { ("Term", "-"), "Term → Factor"}, { ("Term", "id"), "Term → Factor"},
            //{ ("Factor", "intnum"), "Factor → intnum"}, { ("Factor", "floatnum"), "Factor → Factor"}, { ("Factor", "("), "Factor → ( ArithExpr )"}, { ("Factor", "not"), "Factor → not Factor"}, { ("Factor", "+"), "Factor -> Sign Factor"}, { ("Factor", "-"), "Factor → Sign Factor"}, { ("Factor", "id"), "Factor → Variable"},
            //{ ("Variable", "id"), "Variable → RepIdnest id RepIndice"},
            //{ ("FunctionCall", "id"), "FunctionCall → RepIdnest id ( AParams )"},
            { ("Idnest", "["), "EPSILON"}, { ("Idnest", ")"), "EPSILON"}, { ("Idnest", "="), "EPSILON"}, { ("Idnest", ";"), "EPSILON"}, { ("Idnest", "eq"), "EPSILON"}, { ("Idnest", "neq"), "EPSILON"}, { ("Idnest", "lt"), "EPSILON"}, { ("Idnest", "gt"), "EPSILON"}, { ("Idnest", "leq"), "EPSILON"}, { ("Idnest", "geq"), "EPSILON"}, { ("Idnest", "+"), "EPSILON"}, { ("Idnest", "-"), "EPSILON"}, { ("Idnest", "or"), "EPSILON"}, { ("Idnest", "*"), "EPSILON"}, { ("Idnest", "/"), "EPSILON"}, { ("Idnest", "and"), "EPSILON"}, { ("Idnest", "("), "EPSILON"}, { ("Idnest", "]"), "EPSILON"}, { ("Idnest", ","), "EPSILON"},
            { ("OptNest", "id"), "EPSILON"}, { ("OptNest", "["), "EPSILON"}, { ("OptNest", ")"), "EPSILON"}, { ("OptNest", "="), "EPSILON"}, { ("OptNest", ";"), "EPSILON"}, { ("OptNest", "eq"), "EPSILON"}, { ("OptNest", "neq"), "EPSILON"}, { ("OptNest", "lt"), "EPSILON"}, { ("OptNest", "gt"), "EPSILON"}, { ("OptNest", "leq"), "EPSILON"}, { ("OptNest", "geq"), "EPSILON"}, { ("OptNest", "+"), "EPSILON"}, { ("OptNest", "-"), "EPSILON"}, { ("OptNest", "or"), "EPSILON"}, { ("OptNest", "*"), "EPSILON"}, { ("OptNest", "/"), "EPSILON"}, { ("OptNest", "and"), "EPSILON"}, { ("OptNest", "("), "EPSILON"}, { ("OptNest", "]"), "EPSILON"}, { ("OptNest", ","), "EPSILON"},
            { ("RepIdnest", "["), "EPSILON"}, { ("RepIdnest", ")"), "EPSILON"}, { ("RepIdnest", "="), "EPSILON"}, { ("RepIdnest", ";"), "EPSILON"}, { ("RepIdnest", "eq"), "EPSILON"}, { ("RepIdnest", "neq"), "EPSILON"}, { ("RepIdnest", "lt"), "EPSILON"}, { ("RepIdnest", "gt"), "EPSILON"}, { ("RepIdnest", "leq"), "EPSILON"}, { ("RepIdnest", "geq"), "EPSILON"}, { ("RepIdnest", "+"), "EPSILON"}, { ("RepIdnest", "-"), "EPSILON"}, { ("RepIdnest", "or"), "EPSILON"}, { ("RepIdnest", "*"), "EPSILON"}, { ("RepIdnest", "/"), "EPSILON"}, { ("RepIdnest", "and"), "EPSILON"}, { ("RepIdnest", "("), "EPSILON"}, { ("RepIdnest", "]"), "EPSILON"}, { ("RepIdnest", ","), "EPSILON"},
            { ("Indice", ")"), "EPSILON"}, { ("Indice", "="), "EPSILON"},{ ("Indice", ";"), "EPSILON"}, { ("Indice", "eq"), "EPSILON"},{ ("Indice", "neq"), "EPSILON"}, { ("Indice", "lt"), "EPSILON"},{ ("Indice", "gt"), "EPSILON"}, { ("Indice", "leq"), "EPSILON"},{ ("Indice", "geq"), "EPSILON"}, { ("Indice", "+"), "EPSILON"},{ ("Indice", "-"), "EPSILON"}, { ("Indice", "or"), "EPSILON"},{ ("Indice", "*"), "EPSILON"}, { ("Indice", "/"), "EPSILON"},{ ("Indice", "and"), "EPSILON"}, { ("Indice", "]"), "EPSILON"},{ ("Indice", ","), "EPSILON"},
            { ("RepIndice", ")"), "EPSILON"}, { ("RepIndice", "="), "EPSILON"},{ ("RepIndice", ";"), "EPSILON"}, { ("RepIndice", "eq"), "EPSILON"},{ ("RepIndice", "neq"), "EPSILON"}, { ("RepIndice", "lt"), "EPSILON"},{ ("RepIndice", "gt"), "EPSILON"}, { ("RepIndice", "leq"), "EPSILON"},{ ("RepIndice", "geq"), "EPSILON"}, { ("RepIndice", "+"), "EPSILON"},{ ("RepIndice", "-"), "EPSILON"}, { ("RepIndice", "or"), "EPSILON"},{ ("RepIndice", "*"), "EPSILON"}, { ("RepIndice", "/"), "EPSILON"},{ ("RepIndice", "and"), "EPSILON"}, { ("RepIndice", "]"), "EPSILON"},{ ("RepIndice", ","), "EPSILON"},
            //{ ("ArraySize", "["), "ArraySize → [ intNum ]"}, // possible intnum or epsilon
            { ("RepArraySize", ";"), "EPSILON"}, { ("RepArraySize", ","), "EPSILON"}, { ("RepArraySize", ")"), "EPSILON"},
            //{ ("Type", "intNum"), "Type -> integer"}, { ("Type", "floatNum"), "Type -> float"}, { ("Type", "id"), "Type -> id"},
            //{ ("FParams", "intNum"), "FParams -> Type id RepArraySize RepFParamsTail"}, { ("FParams", "floatNum"), "FParams -> Type id RepArraySize RepFParamsTail"}, { ("FParams", "id"), "FParams -> Type id RepArraySize RepFParamsTail"}, { ("FParams", "EPSILON"), "FParams -> EPSILON"},
            //{ ("AParams", "intNum"), "AParams → Expr RepAParamsTail"},{ ("AParams", "floatNum"), "AParams → Expr RepAParamsTail"}, { ("AParams", "("), "AParams → Expr RepAParamsTail"}, { ("AParams", "not"), "AParams → Expr RepAParamsTail"}, { ("AParams", "+"), "AParams -> Expr RepAParamsTail"}, { ("AParams", "-"), "AParams → Expr RepAParamsTail"}, { ("AParams", "id"), "AParams -> Expr RepAParamsTail"}, { ("AParams", "EPSILON"), "AParams -> EPSILON"},
            //{ ("AParamsTail", ","), "AParamsTail → , Type id RepArraySize"},
            { ("RepAParamsTail", ")"), "EPSILON"},
            //{ ("FParamsTail", ","), "FParamsTail → , Expr"},
            { ("RepFParamsTail", ")"), "EPSILON"},
            //{ ("AssignOp", "="), "AssignOp → ="},
            //{ ("RelOp", "eq"), "RelOp → eq"},{ ("RelOp", "neq"), "RelOp → neq"}, { ("RelOp", "lt"), "RelOp → lt"}, { ("RelOp", "gt"), "RelOp → gt"}, { ("RelOp", "leq"), "RelOp -> leq"}, { ("RelOp", "geq"), "RelOp → geq"},
            { ("AddOp", ";"), "EPSILON"},{ ("AddOp", ")"), "AddOp → EPSILON"}, //{ ("AddOp", "or"), "AddOp → or"},
            { ("MultOp", ";"), "EPSILON"},{ ("MultOp", ")"), "AddOp → EPSILON"},// { ("MultOp", "and"), "MultOp → and"},
            //{ ("VoidOrType", "intNum"), "VoidOrType → Type"}, { ("VoidOrType", "floatNum"), "VoidOrType -> Type"}, { ("VoidOrType", "id"), "VoidOrType → Type"}, { ("VoidOrType", "void"), "VoidOrType → void"},
            { ("OptIntNum", "]"), "EPSILON"},
        };

        public static bool ProductionExists(string nonterminal, string terminal)
        {
            var key = (nonterminal, terminal);
            return NonTerminalSymbolToProductionDictionary.ContainsKey(key);
        }

        public static HashSet<string> NonTerminalSymbolsSet = new HashSet<string>()
        {
            "START", "AParams", "AParamsTail", "AddOp", "ArithExpr", "ArraySize",
            "AssignOp", "AssignStat", "ClassDecl", "Expr", "FParams", "FParamsTail",
            "Factor", "FuncBody", "FuncDecl", "FuncDef", "FuncHead", "FunctionCall",
            "Idnest", "Indice", "MemberDecl", "MultOp", "Prog", "RelExpr", "Sign",
            "StatBlock", "Statement", "Term", "Type","VarDecl", "Variable", "Visibility",
            "RepFuncDef", "RepClassDecl", "OptInherits", "RepExtraInherits", "RepMembers",
            "OptId", "OptLocalVD", "RepVarDecl", "RepStatement", "RepExpr", "RepIdnest",
            "RepIndice", "RepArraySize", "RepAParamsTail", "RepFParamsTail", "VoidOrType",
            "OptIntNum", "AssignOrFuncCall", "OptNest", "VarOrFunc", "FuncDeclEnd" ,
            "VarDeclEnd", "Id", "OptMultArith", "RelOp", "AddOrMultOp", "StartFuncOrExpr",
            "ExprOrFunc"

        };

        public static HashSet<string> TerminalSymbolsSet = new HashSet<string>()
        {
            ",", "+", "-", "or", "[", "intnum", "]", "=", "class", "id",
            "{", "}", ";", "(", ")", "floatnum", "not", "do", "end", ":",
            "void", ".", "*", "/", "and", "inherits", "local", "sr", "main",
            "eq", "geq", "gt", "leq", "lt", "neq", "if", "then", "else", "read",
            "return", "while", "write", "float", "integer", "private", "public",
            "EPSILON", "="
        };

        public static HashSet<string> TerminalExpression = new HashSet<string>()
        {
            "+", "-", "or", "=", "!=", "*", "/", "and",
            "eq", "geq", "gt", "leq", "lt", "neq"
        };
    }
}
