using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynDriver
{
    public class SemanticAnalyzer
    {
        public static (SymbolTable symbolTable, List<string> errorList) CreateSymbolTable(Tree<string> tree)
        {
            var globalTable = new SymbolTable("Global");

            TreeNode<string> root = tree.Root;
            var progStart = root.GetChild(0);

            var globalClasses = progStart.GetChild(0);
            SeparateClassDeclarations(globalClasses, globalTable);

            var globalFunctions = progStart.GetChild(1);
            SeparateFunctionDeclarations(globalFunctions, globalTable);

            var mainMethod = progStart.GetChild(3);
            BuildTableFromMainProgramNode(mainMethod, globalTable);

            // Analyze table and write out semantic errors or warnings
            var errorList = AnalyzeTableElements(globalTable);

            return (globalTable, errorList);
        }

        public static void SeparateClassDeclarations(TreeNode<string> currentNode, SymbolTable currentTable)
        {
            var nextNodes = Tree<string>.NextSubNodes(currentNode);

            // Separate each class node
            foreach (var node in nextNodes)
            {
                BuildTableFromClassNode(node, currentTable);
            }
        }

        public static void BuildTableFromClassNode(TreeNode<string> currentNode, SymbolTable currentTable)
        {
            var nodesInPreOrder = Tree<string>.PreOrderingOfSubNodes(currentNode, new List<string>());
            var classTable = new SymbolTable(name: nodesInPreOrder[2]);
            var count = nodesInPreOrder.Count;
            var index = 3;

            // Find a way to handle inheritance
            currentTable.Insert(new SymbolValue(name: nodesInPreOrder[2], kind: "Class", link: classTable));

            while (index != count - 1)
            {
                // Parse functions from class

                if (nodesInPreOrder[index].Equals("integer") || nodesInPreOrder[index].Equals("float"))
                {
                    // Parse variable names and types, then insert into class table.
                    if (nodesInPreOrder[index + 3].Equals("[") && (nodesInPreOrder[index + 4].Equals("]") || nodesInPreOrder[index + 5].Equals("]")) && (nodesInPreOrder[index + 6].Equals("[") || nodesInPreOrder[index + 7].Equals("[")) && (nodesInPreOrder[index + 7].Equals("]") || nodesInPreOrder[index + 8].Equals("]") || nodesInPreOrder[index + 9].Equals("]")))
                    {
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 1], kind: "Variable", type: nodesInPreOrder[index] + "[][]"));
                    }
                    else if (nodesInPreOrder[index + 3].Equals("[") && (nodesInPreOrder[index + 4].Equals("]") || nodesInPreOrder[index + 5].Equals("]")))
                    {
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 1], kind: "Variable", type: nodesInPreOrder[index] + "[]"));
                    }
                    else
                    {
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 1], kind: "Variable", type: nodesInPreOrder[index]));
                    }
                }

                index++;
            }
        }

        public static void SeparateFunctionDeclarations(TreeNode<string> currentNode, SymbolTable currentTable)
        {
            var nextNodes = Tree<string>.NextSubNodes(currentNode);

            // Separate each function node
            foreach (var node in nextNodes)
            {
                BuildTableFromFunctionNode(node, currentTable);
            }
        }

        public static void BuildTableFromFunctionNode(TreeNode<string> currentNode, SymbolTable currentTable)
        {
            var nodesInPreOrder = Tree<string>.PreOrderingOfSubNodes(currentNode, new List<string>());
            var funcTable = new SymbolTable(name: nodesInPreOrder[2]);
            var count = nodesInPreOrder.Count;
            var indexForFunction = 3;
            var index = 3;
            var funcParams = "";
            var funcReturnType = "";

            while (indexForFunction != count - 1)
            {
                // Get function parmeters
                if (nodesInPreOrder[indexForFunction].Equals("("))
                {
                    if (nodesInPreOrder[index + 4].Equals("[") && (nodesInPreOrder[index + 5].Equals("]") || nodesInPreOrder[index + 6].Equals("]")) && (nodesInPreOrder[index + 6].Equals("[") || nodesInPreOrder[index + 7].Equals("[")) && (nodesInPreOrder[index + 7].Equals("]") || nodesInPreOrder[index + 8].Equals("]") || nodesInPreOrder[index + 9].Equals("]")))
                    {
                        funcParams = nodesInPreOrder[indexForFunction + 1] + "[][]";
                    }
                    else if (nodesInPreOrder[index + 4].Equals("[") && (nodesInPreOrder[index + 5].Equals("]") || nodesInPreOrder[index + 6].Equals("]")))
                    {
                        funcParams = nodesInPreOrder[indexForFunction + 1] + "[]";
                    }
                    else
                    {
                        funcParams = nodesInPreOrder[indexForFunction + 1];
                    }
                }
                else if (nodesInPreOrder[indexForFunction].Equals(","))
                {
                    if (nodesInPreOrder[index + 3].Equals("[") && (nodesInPreOrder[index + 4].Equals("]") || nodesInPreOrder[index + 5].Equals("]")) && (nodesInPreOrder[index + 5].Equals("[") || nodesInPreOrder[index + 6].Equals("[")) && (nodesInPreOrder[index + 6].Equals("]") || nodesInPreOrder[index + 7].Equals("]") || nodesInPreOrder[index + 8].Equals("]")))
                    {
                        funcParams += ", " + nodesInPreOrder[indexForFunction + 1] + "[][]";
                    }
                    else if (nodesInPreOrder[index + 3].Equals("[") && (nodesInPreOrder[index + 4].Equals("]") || nodesInPreOrder[index + 5].Equals("]")))
                    {
                        funcParams += ", " + nodesInPreOrder[indexForFunction + 1] + "[]";
                    }
                    else
                    {
                        funcParams += ", " + nodesInPreOrder[indexForFunction + 1];
                    }
                }

                // Get function return type
                if (nodesInPreOrder[indexForFunction].Equals(":"))
                {
                    if (nodesInPreOrder[index + 3].Equals("[") && nodesInPreOrder[index + 4].Equals("]") && nodesInPreOrder[index + 5].Equals("[") && nodesInPreOrder[index + 6].Equals("]"))
                    {
                        funcReturnType = nodesInPreOrder[indexForFunction + 1] + "[][]";
                    }
                    else if (nodesInPreOrder[index + 3].Equals("[") && nodesInPreOrder[index + 4].Equals("]"))
                    {
                        funcReturnType = nodesInPreOrder[indexForFunction + 1] + "[]";
                    }
                    else
                    {
                        funcReturnType = nodesInPreOrder[indexForFunction + 1];
                    }

                    break;
                }

                indexForFunction++;
            }

            currentTable.Insert(new SymbolValue(name: nodesInPreOrder[2], kind: "Function", type: funcReturnType + ": " + funcParams, link: funcTable));

            while (index != count - 1)
            {
                if (nodesInPreOrder[index].Equals("integer") || nodesInPreOrder[index].Equals("float"))
                {
                    // Parse variable names and types, then insert into function table.
                    if (nodesInPreOrder[index + 3].Equals("[") && (nodesInPreOrder[index + 4].Equals("]") || nodesInPreOrder[index + 5].Equals("]")) && (nodesInPreOrder[index + 6].Equals("[") || nodesInPreOrder[index + 7].Equals("[")) && (nodesInPreOrder[index + 7].Equals("]") || nodesInPreOrder[index + 8].Equals("]") || nodesInPreOrder[index + 9].Equals("]")))
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 1], kind: "Variable", type: nodesInPreOrder[index] + "[][]"));
                    }
                    else if (nodesInPreOrder[index + 3].Equals("[") && (nodesInPreOrder[index + 4].Equals("]") || nodesInPreOrder[index + 5].Equals("]")))
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 1], kind: "Variable", type: nodesInPreOrder[index] + "[]"));
                    }
                    else
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 1], kind: "Variable", type: nodesInPreOrder[index]));
                    }
                }

                index++;
            }
        }

        public static void BuildTableFromMainProgramNode(TreeNode<string> currentNode, SymbolTable currentTable)
        {
            var nodesInPreOrder = Tree<string>.PreOrderingOfSubNodes(currentNode, new List<string>());
            var mainTable = new SymbolTable(name: "main");
            var count = nodesInPreOrder.Count;
            var index = 0;

            currentTable.Insert(new SymbolValue(name: "main", kind: "Function", link: mainTable));

            while (index != count - 1)
            {
                if (nodesInPreOrder[index].Equals("integer") || nodesInPreOrder[index].Equals("float"))
                {
                    // Parse variable names and types, then insert into main table.
                    if (nodesInPreOrder[index + 3].Equals("[") && (nodesInPreOrder[index + 4].Equals("]") || nodesInPreOrder[index + 5].Equals("]")) && (nodesInPreOrder[index + 6].Equals("[") || nodesInPreOrder[index + 7].Equals("[")) && (nodesInPreOrder[index + 7].Equals("]") || nodesInPreOrder[index + 8].Equals("]") || nodesInPreOrder[index + 9].Equals("]")))
                    {
                        mainTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 1], kind: "Variable", type: nodesInPreOrder[index] + "[][]"));
                    }
                    else if (nodesInPreOrder[index + 3].Equals("[") && nodesInPreOrder[index + 5].Equals("]"))
                    {
                        mainTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 1], kind: "Variable", type: nodesInPreOrder[index] + "[]"));
                    }
                    else
                    {
                        mainTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 1], kind: "Variable", type: nodesInPreOrder[index]));
                    }
                }

                index++;
            }
        }

        public static List<string> AnalyzeTableElements(SymbolTable symbolTable)
        {
            var errorList = new List<string>();
            var allTables = symbolTable.GetTables();
            foreach (var table in allTables)
            {
                var allVariableSymbols = table.GetVariables();
                var allFunctionSymbols = table.GetFunctions();
                var allClassesSymbols = table.GetClassTables();

                foreach (var variableSymbol in allVariableSymbols)
                {
                    if (allVariableSymbols.Where(c => c.Name.Equals(variableSymbol.Name)).Count() > 1)
                    {
                        //Semantic Error: Multiples of a named variable in the same scope error
                        errorList.Add("Error: multiple variables with same name in the same scope, var: " + variableSymbol.Name);
                    }
                }

                foreach (var classSymbol in allClassesSymbols)
                {
                    if (allClassesSymbols.Where(c => c.Name.Equals(classSymbol.Name)).Count() > 1)
                    {
                        //Semantic Error: Multiples of a named class in the same scope error
                        errorList.Add("Error: multiple classes with same name, class name: " + classSymbol.Name);
                    }
                }

                foreach (var functionSymbols in allFunctionSymbols)
                {
                    var sameNamesInList = allFunctionSymbols.Where(c => c.Name.Equals(functionSymbols.Name));
                    if (sameNamesInList.Count() > 1)
                    {
                        foreach (var symbol in sameNamesInList)
                        {
                            if (sameNamesInList.Where(c => c.Type.Equals(symbol.Type)).Count() > 1)
                            {
                                // Semantic Error: Functions with same name and same type exist
                                errorList.Add("Error: function with same name and same type, function: " + functionSymbols.Name);
                            }
                        }

                        //Semantic Warning: Overloading of functions should return warning
                        errorList.Add("Warning: function overloaded, function: " + functionSymbols.Name);
                    }
                }
            }

            var classTables = symbolTable.GetClassTables();
            foreach (var table in classTables)
            {
                if (table.IsInherited)
                {
                    var parentTable = classTables.Find(t => t.Name.Equals(table.InheritedTable));
                    foreach (var parentSymbols in parentTable.TableEntries)
                    {
                        if (table.TableEntries.Where(t => t.Name.Equals(parentSymbols.Name) && t.Kind.Equals(parentSymbols.Kind) && !t.Type.Equals(parentSymbols.Type)).Count() > 0)
                        {
                            //Warning for shadowed inherited members (classes)
                            errorList.Add("Warning: shadowed inherited class member: " + parentSymbols.Name);
                        }
                    }
                }

                //Check circular class dependencies
                var classVariables = table.GetVariables().Where(c => c.Type != "integer" && c.Type != "float");
                foreach (var classVariable in classVariables)
                {
                    var getClassTable = symbolTable.GetClassTables().FirstOrDefault(c => c.Name.Equals(classVariable.Type));
                    if (getClassTable != null)
                    {
                        var getOtherClassVariables = getClassTable.GetVariables().Where(c => c.Type != "integer" && c.Type != "float");
                        if (getOtherClassVariables.Any(c => c.Type.Equals(table.Name)))
                        {
                            //Warning for circular class dependency semantic error
                            errorList.Add("Warning: Circular depencies for classes: " + table.Name + " and " + getClassTable.Name);
                        }
                    }
                }
            }

            return errorList;
        }

        public static void SemanticAnalysis(Tree<string> tree, SymbolTable symbolTable, List<string> errorList)
        {
            TreeNode<string> root = tree.Root;
            var listOfSubScopes = new List<(List<string>, SymbolTable)>();
            var progStart = root.GetChild(0);

            var classes = progStart.GetChild(0);
            var nextClassNodes = Tree<string>.NextSubNodes(classes);
            var allCalledFunctions = new List<(string fName, string fParams)>();
            var allFunctions = symbolTable.GetFunctions();

            // Separate each class node
            foreach (var classNode in nextClassNodes)
            {
                var subscopeNodes = Tree<string>.PreOrderingOfSubNodes(classNode, new List<string>());
                var classTable = symbolTable.TableEntries.First(c => c.Name.Equals(subscopeNodes[1])).Link;

                var subScopeEntry = (subscopeNodes, classTable);
                listOfSubScopes.Add(subScopeEntry);
            }

            var functions = progStart.GetChild(1);
            var nextFunctionNodes = Tree<string>.NextSubNodes(functions);

            // Separate each class node
            foreach (var functionNode in nextFunctionNodes)
            {
                var subscopeNodes = Tree<string>.PreOrderingOfSubNodes(functionNode, new List<string>());
                var functionTable = symbolTable.TableEntries.FirstOrDefault(c => c.Name.Equals(subscopeNodes[2]) && c.Kind.Equals("Function"))?.Link;
                //if time check params too

                if (functionTable != null)
                {
                    var subScopeEntry = (subscopeNodes, functionTable);
                    listOfSubScopes.Add(subScopeEntry);
                }
            }

            var main = progStart.GetChild(3);
            var subScopeNodes = Tree<string>.PreOrderingOfSubNodes(main, new List<string>());
            var mainTable = symbolTable.TableEntries.First(c => c.Name.Equals("main")).Link;

            var subscopeEntry = (subScopeNodes, mainTable);
            listOfSubScopes.Add(subscopeEntry);

            foreach (var subScope in listOfSubScopes)
            {
                var list = subScope.Item1;
                var index = 0;
                var count = list.Count();
                var isaFunc = list[0].Equals("Function:");

                while (index != count - 1)
                {
                    // Id must by defined in scope
                    if (list[index].Equals("id"))
                    {
                        var idName = list[index + 1];

                        // Check tables
                        var checkGlobalTable = symbolTable.TableEntries.Where(c => c.Name.Equals(idName));
                        var checkLocalTable = subScope.Item2.TableEntries.Where(c => c.Name.Equals(idName));

                        if (checkGlobalTable.Count() == 0 && checkLocalTable.Count() == 0)
                        {
                            errorList.Add("Error: use of undeclared local variable, variable: " + idName);
                        }
                    }

                    //Type checking for all expressions
                    if (list[index].Equals("=") || list[index].Equals("+") || list[index].Equals("-") || list[index].Equals("*") || list[index].Equals("/") || list[index].Equals("eq") || list[index].Equals("neq"))
                    {
                        // get first operand
                        var firstOperand = "";
                        var firstOperandType = "";

                        if (list[index - 1].Equals("]"))
                        {
                            // array index type checking
                            if (int.TryParse(list[index - 2], out _))
                            {
                                if (list[index - 4].Equals("]"))
                                {
                                    if (int.TryParse(list[index - 5], out _))
                                    {
                                        firstOperand = list[index - 7];
                                    }
                                    else
                                    {
                                        firstOperand = list[index - 8];

                                        var arrayVar = list[index - 5];
                                        if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar)).Type.Equals("integer"))
                                        {
                                            errorList.Add("Error: array index not an integer, array: " + firstOperand);
                                        }
                                    }
                                }
                                else
                                {
                                    firstOperand = list[index - 4];
                                }
                            }
                            else
                            {
                                if (list[index - 5].Equals("]"))
                                {
                                    if (int.TryParse(list[index - 6], out _))
                                    {
                                        firstOperand = list[index - 8];

                                        var arrayVar = list[index - 2];
                                        if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar)).Type.Equals("integer"))
                                        {
                                            errorList.Add("Error: array index not an integer, array: " + firstOperand);
                                        }
                                    }
                                    else
                                    {
                                        firstOperand = list[index - 9];

                                        var arrayVar1 = list[index - 2];
                                        var arrayVar2 = list[index - 6];
                                        if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar1)).Type.Equals("integer") || !subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar2)).Type.Equals("integer"))
                                        {
                                            errorList.Add("Error: array index not an integer, array: " + firstOperand);
                                        }
                                    }
                                }
                                else
                                {
                                    firstOperand = list[index - 5];

                                    var arrayVar = list[index - 2];
                                    if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar)).Type.Equals("integer"))
                                    {
                                        errorList.Add("Error: array index not an integer, array: " + firstOperand);
                                    }
                                }
                            }

                            firstOperandType = subScope.Item2.TableEntries.First(c => c.Name.Equals(firstOperand)).Type;
                        }
                        else if (int.TryParse(list[index - 1], out _))
                        {
                            firstOperand = list[index - 1];
                            firstOperandType = "integer";
                        }
                        else if (float.TryParse(list[index - 1], out _))
                        {
                            firstOperand = list[index - 1];
                            firstOperandType = "float";
                        }
                        else
                        {
                            firstOperand = list[index - 1];
                            firstOperandType = subScope.Item2.TableEntries.FirstOrDefault(c => c.Name.Equals(firstOperand))?.Type;
                        }

                        // get second operand
                        var secondOperand = "";
                        var secondOperandType = "";

                        if (list[index + 3].Equals("["))
                        {
                            secondOperand = list[index + 2];
                            secondOperandType = subScope.Item2.TableEntries.First(c => c.Name.Equals(firstOperand)).Type;

                            // array index type checking
                            if (int.TryParse(list[index + 4], out _))
                            {
                                if (list[index + 6].Equals("["))
                                {
                                    if (int.TryParse(list[index + 7], out _))
                                    {
                                        var arrayVar = list[index + 8];
                                        if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar)).Type.Equals("integer"))
                                        {
                                            errorList.Add("Error: array index not an integer, array: " + secondOperand);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (list[index + 7].Equals("["))
                                {
                                    if (int.TryParse(list[index + 8], out _))
                                    {
                                        var arrayVar1 = list[index + 5];
                                        var arrayVar2 = list[index + 9];
                                        if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar1)).Type.Equals("integer") || !subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar2)).Type.Equals("integer"))
                                        {
                                            errorList.Add("Error: array index not an integer, array: " + secondOperand);
                                        }
                                    }
                                }
                            }
                        }
                        else if (int.TryParse(list[index + 1], out _))
                        {
                            secondOperand = list[index + 1];
                            secondOperandType = "integer";
                        }
                        else if (float.TryParse(list[index + 1], out _))
                        {
                            secondOperand = list[index + 1];
                            secondOperandType = "float";
                        }
                        else
                        {
                            secondOperand = list[index + 2];
                            secondOperandType = subScope.Item2.TableEntries.First(c => c.Name.Equals(firstOperand)).Type;
                        }

                        if (!firstOperandType.Equals(secondOperandType))
                        {
                            errorList.Add("Error: expression types are incompatible, first operand: " + firstOperand + ", second operand: " + secondOperand);
                        }
                    }
                    else if (list[index].Equals("geq") || list[index].Equals("gt") || list[index].Equals("leq") || list[index].Equals("lt"))
                    {
                        // get first operand
                        var firstOperand = "";
                        var firstOperandType = "";

                        if (list[index - 2].Equals("]"))
                        {
                            // array index type checking
                            if (int.TryParse(list[index - 3], out _))
                            {
                                if (list[index - 4].Equals("]"))
                                {
                                    if (int.TryParse(list[index - 6], out _))
                                    {
                                        firstOperand = list[index - 6];
                                    }
                                    else
                                    {
                                        firstOperand = list[index - 9];

                                        var arrayVar = list[index - 6];
                                        if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar)).Type.Equals("integer"))
                                        {
                                            errorList.Add("Error: array index not an integer, array: " + firstOperand);
                                        }
                                    }
                                }
                                else
                                {
                                    firstOperand = list[index - 5];
                                }
                            }
                            else
                            {
                                if (list[index - 6].Equals("]"))
                                {
                                    if (int.TryParse(list[index - 7], out _))
                                    {
                                        firstOperand = list[index - 9];

                                        var arrayVar = list[index - 3];
                                        if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar)).Type.Equals("integer"))
                                        {
                                            errorList.Add("Error: array index not an integer, array: " + firstOperand);
                                        }
                                    }
                                    else
                                    {
                                        firstOperand = list[index - 10];

                                        var arrayVar1 = list[index - 3];
                                        var arrayVar2 = list[index - 7];
                                        if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar1)).Type.Equals("integer") || !subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar2)).Type.Equals("integer"))
                                        {
                                            errorList.Add("Error: array index not an integer, array: " + firstOperand);
                                        }
                                    }
                                }
                                else
                                {
                                    firstOperand = list[index - 6];

                                    var arrayVar = list[index - 3];
                                    if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar)).Type.Equals("integer"))
                                    {
                                        errorList.Add("Error: array index not an integer, array: " + firstOperand);
                                    }
                                }
                            }

                            firstOperandType = subScope.Item2.TableEntries.First(c => c.Name.Equals(firstOperand)).Type;
                        }
                        else if (int.TryParse(list[index - 2], out _))
                        {
                            firstOperand = list[index - 2];
                            firstOperandType = "integer";
                        }
                        else if (float.TryParse(list[index - 2], out _))
                        {
                            firstOperand = list[index - 2];
                            firstOperandType = "float";
                        }
                        else
                        {
                            firstOperand = list[index - 2];
                            firstOperandType = subScope.Item2.TableEntries.FirstOrDefault(c => c.Name.Equals(firstOperand))?.Type;
                        }

                        // get second operand
                        var secondOperand = "";
                        var secondOperandType = "";

                        if (list[index + 3].Equals("["))
                        {
                            secondOperand = list[index + 2];
                            secondOperandType = subScope.Item2.TableEntries.First(c => c.Name.Equals(firstOperand)).Type;

                            // array index type checking
                            if (int.TryParse(list[index + 4], out _))
                            {
                                if (list[index + 6].Equals("["))
                                {
                                    if (int.TryParse(list[index + 7], out _))
                                    {
                                        var arrayVar = list[index + 8];
                                        if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar)).Type.Equals("integer"))
                                        {
                                            errorList.Add("Error: array index not an integer, array: " + secondOperand);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (list[index + 7].Equals("["))
                                {
                                    if (int.TryParse(list[index + 8], out _))
                                    {
                                        var arrayVar1 = list[index + 5];
                                        var arrayVar2 = list[index + 9];
                                        if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar1)).Type.Equals("integer") || !subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar2)).Type.Equals("integer"))
                                        {
                                            errorList.Add("Error: array index not an integer, array: " + secondOperand);
                                        }
                                    }
                                }
                            }
                        }
                        else if (int.TryParse(list[index + 1], out _))
                        {
                            secondOperand = list[index + 1];
                            secondOperandType = "integer";
                        }
                        else if (float.TryParse(list[index + 1], out _))
                        {
                            secondOperand = list[index + 1];
                            secondOperandType = "float";
                        }
                        else
                        {
                            secondOperand = list[index + 2];
                            secondOperandType = subScope.Item2.TableEntries.First(c => c.Name.Equals(firstOperand)).Type;
                        }

                        if (!firstOperandType.Equals(secondOperandType))
                        {
                            errorList.Add("Error: expression types are incompatible, first operand: " + firstOperand + ", second operand: " + secondOperand);
                        }
                    }

                    //Type checking function return types
                    if (list[index].Equals("return") && isaFunc)
                    {
                        var returnVariable = "";
                        var returnType = "";

                        if (list[index + 3].Equals("["))
                        {
                            returnVariable = list[index + 2];
                            returnType = subScope.Item2.TableEntries.First(c => c.Name.Equals(returnVariable)).Type;

                            // array index type checking
                            if (int.TryParse(list[index + 4], out _))
                            {
                                if (list[index + 6].Equals("["))
                                {
                                    if (int.TryParse(list[index + 7], out _))
                                    {
                                        var arrayVar = list[index + 8];
                                        if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar)).Type.Equals("integer"))
                                        {
                                            errorList.Add("Error: array index not an integer, array: " + returnVariable);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (list[index + 7].Equals("["))
                                {
                                    if (int.TryParse(list[index + 8], out _))
                                    {
                                        var arrayVar1 = list[index + 5];
                                        var arrayVar2 = list[index + 9];
                                        if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar1)).Type.Equals("integer") || !subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar2)).Type.Equals("integer"))
                                        {
                                            errorList.Add("Error: array index not an integer, array: " + returnVariable);
                                        }
                                    }
                                }
                            }
                        }
                        else if (int.TryParse(list[index + 1], out _))
                        {
                            returnVariable = list[index + 1];
                            returnType = "integer";
                        }
                        else if (float.TryParse(list[index + 1], out _))
                        {
                            returnVariable = list[index + 1];
                            returnType = "float";
                        }
                        else
                        {
                            returnVariable = list[index + 2];
                            returnType = subScope.Item2.TableEntries.First(c => c.Name.Equals(returnVariable)).Type;
                        }

                        var functionName = list[2];
                        var tableReturnType = symbolTable.TableEntries.FirstOrDefault(c => c.Name.Equals(functionName) && c.Kind.Equals("Function") && c.Type.Equals(returnType));

                        if (tableReturnType == null)
                        {
                            errorList.Add("Error: returned value type does not match function return type, function: " + functionName);
                        }
                    }

                    //'.' dot operator used properly
                    if (list[index].Equals("."))
                    {
                        var classNameContaining = list[index - 1];
                        var functionOrVariableCalled = list[index + 2];

                        // find class in global table
                        var classEntry = symbolTable.TableEntries.Where(c => c.Name.Equals(classNameContaining) && c.Kind.Equals("Class"));

                        if (classEntry.Count() == 1)
                        {
                            var classTable = classEntry.First().Link;
                            var countCorrect = classTable.TableEntries.Where(c => c.Name.Equals(functionOrVariableCalled));

                            if (countCorrect.Count() == 0)
                            {
                                // Error using dot operator
                                errorList.Add("Error: Dot Operator error, variable/function not found in class: " + classNameContaining + " for variable/function: " + functionOrVariableCalled);
                            }

                        }
                        else
                        {
                            // Error using dot operator
                            errorList.Add("Error: Dot Operator error, class not found, for class: " + classNameContaining);
                        }

                    }

                    var functionsCalled = new List<(string fName, string fParams)>();

                    //Function call parameter
                    if (list[index].Equals("(") && list[index - 2].Equals("id") && !list[index - 3].Equals("Function:"))
                    {
                        var functionName = list[index - 1];
                        var parameters = new List<string>();
                        var paramIndex = 1;

                        if (list[index + 1].Equals(")"))
                        {
                            parameters.Add("void");
                        }

                        var paramType = "";
                        bool newParam = true;

                        // get params
                        while (!list[index + paramIndex].Equals(")"))
                        {
                            if (newParam)
                            {
                                if (list[index + paramIndex].Equals("id"))
                                {
                                    var varName = list[index + paramIndex + 1];
                                    var varType = subScope.Item2.TableEntries.First(c => c.Name.Equals(varName) && c.Kind.Equals("Variable")).Type;
                                    paramType += varType;
                                }
                                else if (int.TryParse(list[index + paramIndex], out _) || float.TryParse(list[index + paramIndex], out _))
                                {
                                    paramType += list[index + paramIndex];
                                }

                                newParam = false;
                            }

                            if (list[index + paramIndex].Equals(","))
                            {
                                parameters.Add(paramType);
                                newParam = true;
                            }

                            paramIndex++;
                        }

                        // concat
                        var concatenatedParams = "";
                        bool firstParam = true;
                        foreach (var param in parameters)
                        {
                            if (firstParam)
                            {
                                concatenatedParams += param;
                                firstParam = false;
                            }
                            else
                            {
                                concatenatedParams += ", " + param;
                            }
                        }

                        var functionCalled = (functionName, concatenatedParams);
                        functionsCalled.Add(functionCalled);
                        allCalledFunctions.Add(functionCalled);

                        if (!symbolTable.GetFunctions().Any(c => c.Name.Equals(functionName) && c.Type.Equals(concatenatedParams)))
                        {
                            // Error function parameters do not match function definition
                            errorList.Add("Error: function call parameters do not match function definition, function: " + functionName);
                        }
                    }

                    // Every called function has a definition
                    foreach (var functionThatWasCalled in functionsCalled)
                    {
                        if (!allFunctions.Any(c => c.Name.Equals(functionThatWasCalled.fName) && c.Type.Equals(functionThatWasCalled.fParams)))
                        {
                            errorList.Add("Error: function called does not contain definition, function: " + functionThatWasCalled.fName + ", with parameters: " + functionThatWasCalled.fParams);
                        }
                    }

                    index++;
                }
            }
            
            // Every function definition has a function call
            foreach (var aFunc in allFunctions)
            {
                if (!allCalledFunctions.Any(c => c.fName.Equals(aFunc.Name) && c.fParams.Equals(aFunc.Type)) && !aFunc.Name.Equals("main"))
                {
                    errorList.Add("Warning: function definition is not used, function: " + aFunc.Name + ", with parameters: " + aFunc.Type);
                }
            }
        }
    }
}
