using System;
using System.Collections.Generic;
using System.Text;

namespace SynDriver
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
            { ("ClassDecl", "class"), "class id OptInherits { RepMembers } ;"},
            { ("OptInherits", "inherits"), "inherits id RepExtraInherits"},
            { ("RepExtraInherits", ","), "RepExtraInherits , id"},
            { ("RepMembers", "public"), "Visibility MemberDecl RepMembers"}, { ("RepMembers", "private"), "Visibility MemberDecl RepMembers"},
            { ("Visibility", "public"), "public"}, { ("Visibility", "private"), "private"},
            { ("MemberDecl", "id"), "FuncDecl"}, { ("MemberDecl", "integer"), "VarDecl"}, { ("MemberDecl", "float"), "VarDecl"},
            { ("FuncDecl", "id"), "id ( FParams ) : VoidOrType ;"},
            { ("FuncHead", "id"), "id ( FParams ) : VoidOrType"},
            { ("OptId", "id"), "id sr"}, //challenge, what is sr
            { ("FuncDef", "id"), "FuncHead FuncBody"},
            { ("FuncBody", "local"), "OptLocalVD do RepStatement end"}, { ("FuncBody", "do"), "OptLocalVD do RepStatement end"},
            { ("OptLocalVD", "local"), "local RepVarDecl"},
            { ("VarDecl", "integer"), "Type id RepArraySize ;"}, { ("VarDecl", "float"), "Type id RepArraySize ;"}, { ("VarDecl", "id"), "Type id RepArraySize ;"},
            { ("RepVarDecl", "integer"), "VarDecl RepVarDecl"}, { ("RepVarDecl", "float"), "VarDecl RepVarDecl"}, { ("RepVarDecl", "id"), "VarDecl RepVarDecl"},
            { ("Statement", "if"), "if ( RelExpr ) then StatBlock else StatBlock ;"}, { ("Statement", "while"), "while ( RelExpr ) StatBlock ;"}, { ("Statement", "read"), "read ( Variable ) ;"}, { ("Statement", "write"), "write ( Expr ) ;"}, { ("Statement", "return"), "return ( Expr ) ;"}, { ("Statement", "id"), "RepIdnest id AssignOrFuncCall ;"}, //Assignstat or functioncall is still ambiguous
            { ("RepStatement", "if"), "Statement RepStatement"}, { ("RepStatement", "while"), "Statement RepStatement"}, { ("RepStatement", "read"), "Statement RepStatement"}, { ("RepStatement", "write"), "Statement RepStatement"}, { ("RepStatement", "return"), "Statement RepStatement"}, { ("RepStatement", "id"), "Statement RepStatement"}, 
            { ("AssignOrFuncCall", "("), "( AParams )"}, { ("AssignOrFuncCall", "["), "RepIndice AssignOp Expr"}, { ("AssignOrFuncCall", "="), "AssignOp Expr"},
            { ("StatBlock", "do"), "do RepStatement end"}, { ("StatBlock", "if"), "Statement"}, { ("StatBlock", "while"), "Statement"}, { ("StatBlock", "read"), "Statement"}, { ("StatBlock", "write"), "Statement"}, { ("StatBlock", "return"), "Statement"}, { ("StatBlock", "id"), "Statement"},
            { ("Expr", "intnum"), "ArithExpr"}, { ("Expr", "floatnum"), "ArithExpr"}, { ("Expr", "("), "RelExpr"}, { ("Expr", "not"), "RelExpr"}, { ("Expr", "+"), "ArithExpr"}, { ("Expr", "-"), "ArithExpr"}, { ("Expr", "id"), "ArithExpr"},
            { ("RelExpr", "intnum"), "ArithExpr RelOp ArithExpr"}, { ("RelExpr", "floatnum"), "ArithExpr RelOp ArithExpr"}, { ("RelExpr", "("), "ArithExpr RelOp ArithExpr"}, { ("RelExpr", "not"), "ArithExpr RelOp ArithExpr"}, { ("RelExpr", "+"), "ArithExpr RelOp ArithExpr"}, { ("RelExpr", "-"), "ArithExpr RelOp ArithExpr"}, { ("RelExpr", "id"), "ArithExpr RelOp ArithExpr"},
            { ("ArithExpr", "intnum"), "Term"}, { ("ArithExpr", "floatnum"), "Term"}, { ("ArithExpr", "("), "ArithExpr AddOp Term"}, { ("ArithExpr", "not"), "Term"}, { ("ArithExpr", "+"), "ArithExpr AddOp Term"}, { ("ArithExpr", "-"), "ArithExpr AddOp Term"}, { ("ArithExpr", "id"), "Term"},
            { ("Sign", "+"), "+"}, { ("Sign", "-"), "-"},
            { ("Term", "intnum"), "Factor"}, { ("Term", "floatnum"), "Factor"}, { ("Term", "("), "Factor"}, { ("Term", "not"), "Factor"}, { ("Term", "+"), "Factor"}, { ("Term", "-"), "Factor"}, { ("Term", "id"), "Factor"},
            { ("Factor", "intnum"), "intnum"}, { ("Factor", "floatnum"), "floatnum"}, { ("Factor", "("), "( ArithExpr )"}, { ("Factor", "not"), "not Factor"}, { ("Factor", "+"), "Sign Factor"}, { ("Factor", "-"), "Sign Factor"}, { ("Factor", "id"), "Variable"},
            { ("Variable", "id"), "id RepIdnest RepIndice"},
            { ("FunctionCall", "id"), "id RepIdnest ( AParams )"},
            { ("Idnest", "id"), "id OptNest"},
            { ("OptNest", "["), "RepIndice ."}, { ("OptNest", "."), "RepIndice ."},
            { ("RepIdnest", "id"), "Idnest RepIdnest"},
            { ("Indice", "["), "[ ArithExpr ]"},
            { ("RepIndice", "["), "Indice RepIndice"},
            { ("ArraySize", "["), "[ OptIntNum ]"}, 
            { ("RepArraySize", "["), "ArraySize RepArraySize"},
            { ("Type", "integer"), "integer"}, { ("Type", "float"), "float"}, { ("Type", "id"), "id"},
            { ("FParams", "integer"), "Type id RepArraySize RepFParamsTail"}, { ("FParams", "float"), "Type id RepArraySize RepFParamsTail"}, { ("FParams", "id"), "Type id RepArraySize RepFParamsTail"},
            { ("AParams", "intnum"), "Expr RepAParamsTail"},{ ("AParams", "floatNum"), "Expr RepAParamsTail"}, { ("AParams", "("), "Expr RepAParamsTail"}, { ("AParams", "not"), "Expr RepAParamsTail"}, { ("AParams", "+"), "Expr RepAParamsTail"}, { ("AParams", "-"), "Expr RepAParamsTail"}, { ("AParams", "id"), "Expr RepAParamsTail"},
            { ("AParamsTail", ","), ", Expr"},
            { ("RepAParamsTail", ","), "AParamsTail RepAParamsTail"},
            { ("FParamsTail", ","), ", Type id RepArraySize"},
            { ("RepFParamsTail", ","), "FParamsTail RepFParamsTail"},
            { ("AssignOp", "="), "="},
            { ("RelOp", "eq"), "eq"},{ ("RelOp", "neq"), "neq"}, { ("RelOp", "lt"), "lt"}, { ("RelOp", "gt"), "gt"}, { ("RelOp", "leq"), "leq"}, { ("RelOp", "geq"), "geq"},
            { ("AddOp", "+"), "+"},{ ("AddOp", "-"), "-"}, { ("AddOp", "or"), "or"},
            { ("MultOp", "*"), "*"}, { ("MultOp", "/"), "/"}, { ("MultOp", "and"), "and"},
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
            //{ ("AssignOrFuncCall", ";"), "EPSILON"}, { ("AssignOrFuncCall", "else"), "EPSILON"},
            //{ ("Expr", ")"), "Expr → ArithExpr"}, { ("Expr", ";"), "Expr → ArithExpr"}, { ("Expr", ","), "Expr → ArithExpr"},
            //{ ("RelExpr", ")"), "RelExpr → ArithExpr RelOp ArithExpr"}, { ("RelExpr", ";"), "RelExpr → ArithExpr RelOp ArithExpr"}, { ("RelExpr", ","), "RelExpr → ArithExpr RelOp ArithExpr"},
            { ("ArithExpr", ")"), "ArithExpr AddOp Term"}, //{ ("ArithExpr", ";"), "ArithExpr → Term"}, { ("ArithExpr", "eq"), "ArithExpr → Term"}, { ("ArithExpr", "neq"), "ArithExpr → Term"}, { ("lt", "+"), "ArithExpr -> Term"}, { ("ArithExpr", "gt"), "ArithExpr → Term"}, { ("ArithExpr", "or"), "ArithExpr → ArithExpr AddOp Term"}, { ("ArithExpr", "]"), "ArithExpr → Term"}, { ("ArithExpr", ","), "ArithExpr → Term"},
            //{ ("Sign", "intnum"), "Sign → +"}, { ("Sign", "floatnum"), "Sign → -"},
            //{ ("Term", "intnum"), "Term → Factor"}, { ("Term", "floatnum"), "Term → Factor"}, { ("Term", "("), "Term → Factor"}, { ("Term", "not"), "Term → Factor"}, { ("Term", "+"), "Term -> Factor"}, { ("Term", "-"), "Term → Factor"}, { ("Term", "id"), "Term → Factor"},
            //{ ("Factor", "intnum"), "Factor → intnum"}, { ("Factor", "floatnum"), "Factor → Factor"}, { ("Factor", "("), "Factor → ( ArithExpr )"}, { ("Factor", "not"), "Factor → not Factor"}, { ("Factor", "+"), "Factor -> Sign Factor"}, { ("Factor", "-"), "Factor → Sign Factor"}, { ("Factor", "id"), "Factor → Variable"},
            //{ ("Variable", "id"), "Variable → RepIdnest id RepIndice"},
            //{ ("FunctionCall", "id"), "FunctionCall → RepIdnest id ( AParams )"},
            //{ ("Idnest", "id"), "Idnest → id RepIndice ."},
            { ("OptNest", "id"), "EPSILON"},
            { ("RepIdnest", "["), "EPSILON"}, { ("RepIdnest", ")"), "EPSILON"}, { ("RepIdnest", "="), "EPSILON"}, { ("RepIdnest", ";"), "EPSILON"}, { ("RepIdnest", "eq"), "EPSILON"}, { ("RepIdnest", "neq"), "EPSILON"}, { ("RepIdnest", "lt"), "EPSILON"}, { ("RepIdnest", "gt"), "EPSILON"}, { ("RepIdnest", "leq"), "EPSILON"}, { ("RepIdnest", "geq"), "EPSILON"}, { ("RepIdnest", "+"), "EPSILON"}, { ("RepIdnest", "-"), "EPSILON"}, { ("RepIdnest", "or"), "EPSILON"}, { ("RepIdnest", "*"), "EPSILON"}, { ("RepIdnest", "/"), "EPSILON"}, { ("RepIdnest", "and"), "EPSILON"}, { ("RepIdnest", "("), "EPSILON"}, { ("RepIdnest", "]"), "EPSILON"}, { ("RepIdnest", ","), "EPSILON"},
            //{ ("Indice", "["), "Indice → [ ArithExpr ]"},
            { ("RepIndice", ")"), "EPSILON"}, { ("RepIndice", "="), "EPSILON"},{ ("RepIndice", ";"), "EPSILON"}, { ("RepIndice", "eq"), "EPSILON"},{ ("RepIndice", "new"), "EPSILON"}, { ("RepIndice", "lt"), "EPSILON"},{ ("RepIndice", "gt"), "EPSILON"}, { ("RepIndice", "leq"), "EPSILON"},{ ("RepIndice", "geq"), "EPSILON"}, { ("RepIndice", "+"), "EPSILON"},{ ("RepIndice", "-"), "EPSILON"}, { ("RepIndice", "or"), "EPSILON"},{ ("RepIndice", "*"), "EPSILON"}, { ("RepIndice", "/"), "EPSILON"},{ ("RepIndice", "and"), "EPSILON"}, { ("RepIndice", "."), "EPSILON"}, { ("RepIndice", "]"), "EPSILON"},{ ("RepIndice", ","), "EPSILON"},
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
            //{ ("AddOp", "+"), "AddOp → +"},{ ("AddOp", "-"), "AddOp → -"}, { ("AddOp", "or"), "AddOp → or"},
            //{ ("MultOp", "*"), "MultOp → *"}, { ("MultOp", "/"), "MultOp -> /"}, { ("MultOp", "and"), "MultOp → and"},
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
            "OptIntNum", "AssignOrFuncCall", "OptNest"

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
