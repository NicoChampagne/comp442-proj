using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynSemDriver
{
    public class SemanticAnalyzer
    {
        public static List<string> allClassNames = new List<string>();

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
            var classTables = new List<SymbolTable>();

            // Separate each class node
            foreach (var node in nextNodes)
            {
                if (node.Value.Equals(""))
                {
                    return;
                }

                var nodesInPreOrder = Tree<string>.PreOrderingOfSubNodes(node, new List<string>());

                if (nodesInPreOrder.Where(c => c.Equals("Class:")).Count() > 1)
                {
                    var lastIndex = nodesInPreOrder.FindLastIndex(c => c.Equals("Class:"));
                    var firstList = nodesInPreOrder.GetRange(0, lastIndex);
                    var sencondList = nodesInPreOrder.GetRange(lastIndex, nodesInPreOrder.Count - firstList.Count);

                    classTables.Add(BuildTableFromClassNode(firstList, currentTable));
                    classTables.Add(BuildTableFromClassNode(sencondList, currentTable));
                }
                else
                {
                    classTables.Add(BuildTableFromClassNode(node, currentTable));
                }
            }

            // add inherited values to class that are inherited
            foreach (var classTable in classTables)
            {
                if (classTable.IsInherited)
                {
                    foreach (var inheritedClassName in classTable.InheritedTables)
                    {
                        var parentClass = classTables.Find(c => c.Name.Equals(inheritedClassName));

                        foreach (var entry in parentClass.TableEntries)
                        {
                            var newEntry = new SymbolValue("parent_" + entry.Name, entry.Kind, entry.Type, entry.ArrayType, entry.Offset, entry.ScopedOffset, entry.Link);
                            classTable.Insert(newEntry);
                        }

                        classTable.TableOffset += parentClass.TableOffset;
                    }
                }
            }
        }

        public static SymbolTable BuildTableFromClassNode(TreeNode<string> currentNode, SymbolTable currentTable)
        {
            var nodesInPreOrder = Tree<string>.PreOrderingOfSubNodes(currentNode, new List<string>());
            var classTable = new SymbolTable(name: nodesInPreOrder[3]);
            var count = nodesInPreOrder.Count;
            var index = 3;
            var isaFunc = false;

            var classSymbol = new SymbolValue(name: nodesInPreOrder[3], kind: "Class", link: classTable);
            allClassNames.Add(nodesInPreOrder[3]);

            if (nodesInPreOrder[4].Equals("inherits"))
            {
                classSymbol.Link.IsInherited = true;
                classSymbol.Link.InheritedTables.Add(nodesInPreOrder[6]);

                if (nodesInPreOrder[7].Equals("inherits"))
                {
                    classSymbol.Link.InheritedTables.Add(nodesInPreOrder[9]);

                    if (nodesInPreOrder[10].Equals("inherits"))
                    {
                        classSymbol.Link.InheritedTables.Add(nodesInPreOrder[12]);
                        if (nodesInPreOrder[13].Equals("inherits"))
                        {
                            classSymbol.Link.InheritedTables.Add(nodesInPreOrder[15]);
                            if (nodesInPreOrder[16].Equals("inherits"))
                            {
                                classSymbol.Link.InheritedTables.Add(nodesInPreOrder[18]);
                            }
                        }
                    }
                }
            }

            currentTable.Insert(classSymbol);

            while (index != count - 1)
            {
                // Todo: Parse class variables from classes?

                if (isaFunc || (nodesInPreOrder[index].Equals("id") && nodesInPreOrder[index + 2].Equals("(")))
                {
                    isaFunc = true;
                    // Don't operate on nodes until func is clear
                    if (nodesInPreOrder[index].Equals("}"))
                    {
                        isaFunc = false;
                    }
                    else if (index + 2 < count && nodesInPreOrder[index + 2].Equals("("))
                    {
                        var funcName = nodesInPreOrder[index + 1];
                        var innerCount = index + 2;
                        var funcParams = "";
                        var funcReturnType = "";

                        while (!nodesInPreOrder[innerCount].Equals("}"))
                        {
                            // Get function parmeters
                            if (nodesInPreOrder[innerCount].Equals("("))
                            {
                                if (nodesInPreOrder[innerCount + 4].Equals("[") && (nodesInPreOrder[innerCount + 5].Equals("]") || nodesInPreOrder[innerCount + 6].Equals("]")) && (nodesInPreOrder[innerCount + 6].Equals("[") || nodesInPreOrder[innerCount + 7].Equals("[")) && (nodesInPreOrder[innerCount + 7].Equals("]") || nodesInPreOrder[innerCount + 8].Equals("]") || nodesInPreOrder[innerCount + 9].Equals("]")))
                                {
                                    funcParams = nodesInPreOrder[innerCount + 1] + "[][]";
                                }
                                else if (nodesInPreOrder[innerCount + 4].Equals("[") && (nodesInPreOrder[innerCount + 5].Equals("]") || nodesInPreOrder[innerCount + 6].Equals("]")))
                                {
                                    funcParams = nodesInPreOrder[innerCount + 1] + "[]";
                                }
                                else
                                {
                                    funcParams = nodesInPreOrder[innerCount + 1];
                                }
                            }
                            else if (nodesInPreOrder[innerCount].Equals(","))
                            {
                                if (nodesInPreOrder[innerCount + 3].Equals("[") && (nodesInPreOrder[innerCount + 4].Equals("]") || nodesInPreOrder[innerCount + 5].Equals("]")) && (nodesInPreOrder[innerCount + 5].Equals("[") || nodesInPreOrder[innerCount + 6].Equals("[")) && (nodesInPreOrder[innerCount + 6].Equals("]") || nodesInPreOrder[innerCount + 7].Equals("]") || nodesInPreOrder[innerCount + 8].Equals("]")))
                                {
                                    funcParams += ", " + nodesInPreOrder[innerCount + 1] + "[][]";
                                }
                                else if (nodesInPreOrder[innerCount + 3].Equals("[") && (nodesInPreOrder[innerCount + 4].Equals("]") || nodesInPreOrder[innerCount + 5].Equals("]")))
                                {
                                    funcParams += ", " + nodesInPreOrder[innerCount + 1] + "[]";
                                }
                                else
                                {
                                    funcParams += ", " + nodesInPreOrder[innerCount + 1];
                                }
                            }

                            // Get function return type
                            if (nodesInPreOrder[innerCount].Equals(":"))
                            {
                                if (innerCount + 9 < count && nodesInPreOrder[innerCount + 3].Equals("[") && nodesInPreOrder[innerCount + 4].Equals("]") && nodesInPreOrder[innerCount + 5].Equals("[") && nodesInPreOrder[innerCount + 6].Equals("]"))
                                {
                                    funcReturnType = nodesInPreOrder[innerCount + 1] + "[][]";
                                }
                                else if (innerCount + 4 < count && nodesInPreOrder[innerCount + 3].Equals("[") && nodesInPreOrder[innerCount + 4].Equals("]"))
                                {
                                    funcReturnType = nodesInPreOrder[innerCount + 1] + "[]";
                                }
                                else
                                {
                                    funcReturnType = nodesInPreOrder[innerCount + 1];
                                }
                            }

                            if (nodesInPreOrder[innerCount + 1].Equals("}"))
                            {
                                classTable.Insert(new SymbolValue(name: funcName, kind: "Function", type: funcReturnType + ": " + funcParams));
                            }

                            innerCount++;
                        }
                    }
                }
                else if (nodesInPreOrder[index].Equals("integer") || nodesInPreOrder[index].Equals("float"))
                {
                    var offsetValueForVariable = nodesInPreOrder[index].Equals("integer") ? 4 : 8;

                    // Parse variable names and types, then insert into class table.
                    if (index + 9 < count && (nodesInPreOrder[index + 3]?.Equals("[") ?? false) && ((nodesInPreOrder[index + 4]?.Equals("]") ?? false) || (nodesInPreOrder[index + 5]?.Equals("]") ?? false)) && ((nodesInPreOrder[index + 6]?.Equals("[") ?? false) || (nodesInPreOrder[index + 7]?.Equals("[") ?? false)) && ((nodesInPreOrder[index + 7]?.Equals("]") ?? false) || (nodesInPreOrder[index + 8]?.Equals("]") ?? false) || (nodesInPreOrder[index + 9]?.Equals("]") ?? false)))
                    {
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[][]"));
                    }
                    else if (index + 5 < count && (nodesInPreOrder[index + 3]?.Equals("[") ?? false) && ((nodesInPreOrder[index + 4]?.Equals("]") ?? false) || (nodesInPreOrder[index + 5]?.Equals("]") ?? false)))
                    {
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[]", offset: (offsetValueForVariable * int.Parse(nodesInPreOrder[index + 4])), scopedOffset: classTable.TableOffset + (offsetValueForVariable * int.Parse(nodesInPreOrder[index + 4]))));
                        classTable.TableOffset = classTable.TableOffset + (offsetValueForVariable * int.Parse(nodesInPreOrder[index + 4]));
                    }
                    else
                    {
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index], offset: offsetValueForVariable, scopedOffset: classTable.TableOffset + offsetValueForVariable));
                        classTable.TableOffset = classTable.TableOffset + offsetValueForVariable;
                    }
                }
                else if (allClassNames.Any(c => c.Equals(nodesInPreOrder[index]) && !nodesInPreOrder[3].Equals(nodesInPreOrder[index])))
                {
                    // Parse variable names and types, then insert into function table.
                    if (nodesInPreOrder[index + 4].Equals("[") && (nodesInPreOrder[index + 5].Equals("]") || nodesInPreOrder[index + 6].Equals("]")) && (nodesInPreOrder[index + 7].Equals("[") || nodesInPreOrder[index + 8].Equals("[")) && (nodesInPreOrder[index + 8].Equals("]") || nodesInPreOrder[index + 9].Equals("]") || nodesInPreOrder[index + 10].Equals("]")))
                    {
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[][]"));
                    }
                    else if (nodesInPreOrder[index + 4].Equals("[") && (nodesInPreOrder[index + 5].Equals("]") || nodesInPreOrder[index + 6].Equals("]")))
                    {
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[]"));
                    }
                    else
                    {
                        var classVarTable = currentTable.TableEntries.Find(c => c.Name.Equals(nodesInPreOrder[index]) && c.Kind.Equals("Class"))?.Link;
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index], offset: classVarTable.TableOffset, scopedOffset: classTable.TableOffset + classVarTable.TableOffset));
                        classTable.TableOffset = classTable.TableOffset + classVarTable.TableOffset;
                    }
                }
				else if (Char.IsUpper(nodesInPreOrder[index].ToCharArray()[0]) && !nodesInPreOrder[index+1].Equals("{") && !nodesInPreOrder[index + 1].Equals(":"))
				{
					classTable.Insert(new SymbolValue(name: nodesInPreOrder[index+2], kind: "Variable", type: nodesInPreOrder[index]));
				}

                index++;
            }

            return classTable;
        }

        public static SymbolTable BuildTableFromClassNode(List<string> nodesInPreOrder, SymbolTable currentTable)
        {
            var classTable = new SymbolTable(name: nodesInPreOrder[3]);
            var count = nodesInPreOrder.Count;
            var index = 3;
            var isaFunc = false;

            var classSymbol = new SymbolValue(name: nodesInPreOrder[3], kind: "Class", link: classTable);
            allClassNames.Add(nodesInPreOrder[3]);

            if (nodesInPreOrder[4].Equals("inherits"))
            {
                classSymbol.Link.IsInherited = true;
                classSymbol.Link.InheritedTables.Add(nodesInPreOrder[6]);

                if (nodesInPreOrder[7].Equals("inherits"))
                {
                    classSymbol.Link.InheritedTables.Add(nodesInPreOrder[9]);

                    if (nodesInPreOrder[10].Equals("inherits"))
                    {
                        classSymbol.Link.InheritedTables.Add(nodesInPreOrder[12]);
                        if (nodesInPreOrder[13].Equals("inherits"))
                        {
                            classSymbol.Link.InheritedTables.Add(nodesInPreOrder[15]);
                            if (nodesInPreOrder[16].Equals("inherits"))
                            {
                                classSymbol.Link.InheritedTables.Add(nodesInPreOrder[18]);
                            }
                        }
                    }
                }
            }

            currentTable.Insert(classSymbol);

            while (index != count - 1)
            {
                // Todo: Parse class variables from classes?

                if (isaFunc || (nodesInPreOrder[index].Equals("id") && nodesInPreOrder[index + 2].Equals("(")))
                {
                    isaFunc = true;
                    // Don't operate on nodes until func is clear
                    if (nodesInPreOrder[index].Equals("}"))
                    {
                        isaFunc = false;
                    }
                    else if (index + 2 < count && nodesInPreOrder[index + 2].Equals("("))
                    {
                        var funcName = nodesInPreOrder[index + 1];
                        var innerCount = index + 2;
                        var funcParams = "";
                        var funcReturnType = "";

                        while (!nodesInPreOrder[innerCount].Equals("}"))
                        {
                            // Get function parmeters
                            if (nodesInPreOrder[innerCount].Equals("("))
                            {
                                if (nodesInPreOrder[innerCount + 4].Equals("[") && (nodesInPreOrder[innerCount + 5].Equals("]") || nodesInPreOrder[innerCount + 6].Equals("]")) && (nodesInPreOrder[innerCount + 6].Equals("[") || nodesInPreOrder[innerCount + 7].Equals("[")) && (nodesInPreOrder[innerCount + 7].Equals("]") || nodesInPreOrder[innerCount + 8].Equals("]") || nodesInPreOrder[innerCount + 9].Equals("]")))
                                {
                                    funcParams = nodesInPreOrder[innerCount + 1] + "[][]";
                                }
                                else if (nodesInPreOrder[innerCount + 4].Equals("[") && (nodesInPreOrder[innerCount + 5].Equals("]") || nodesInPreOrder[innerCount + 6].Equals("]")))
                                {
                                    funcParams = nodesInPreOrder[innerCount + 1] + "[]";
                                }
                                else
                                {
                                    funcParams = nodesInPreOrder[innerCount + 1];
                                }
                            }
                            else if (nodesInPreOrder[innerCount].Equals(","))
                            {
                                if (nodesInPreOrder[innerCount + 3].Equals("[") && (nodesInPreOrder[innerCount + 4].Equals("]") || nodesInPreOrder[innerCount + 5].Equals("]")) && (nodesInPreOrder[innerCount + 5].Equals("[") || nodesInPreOrder[innerCount + 6].Equals("[")) && (nodesInPreOrder[innerCount + 6].Equals("]") || nodesInPreOrder[innerCount + 7].Equals("]") || nodesInPreOrder[innerCount + 8].Equals("]")))
                                {
                                    funcParams += ", " + nodesInPreOrder[innerCount + 1] + "[][]";
                                }
                                else if (nodesInPreOrder[innerCount + 3].Equals("[") && (nodesInPreOrder[innerCount + 4].Equals("]") || nodesInPreOrder[innerCount + 5].Equals("]")))
                                {
                                    funcParams += ", " + nodesInPreOrder[innerCount + 1] + "[]";
                                }
                                else
                                {
                                    funcParams += ", " + nodesInPreOrder[innerCount + 1];
                                }
                            }

                            // Get function return type
                            if (nodesInPreOrder[innerCount].Equals(":"))
                            {
                                if (innerCount + 9 < count && nodesInPreOrder[innerCount + 3].Equals("[") && nodesInPreOrder[innerCount + 4].Equals("]") && nodesInPreOrder[innerCount + 5].Equals("[") && nodesInPreOrder[innerCount + 6].Equals("]"))
                                {
                                    funcReturnType = nodesInPreOrder[innerCount + 1] + "[][]";
                                }
                                else if (innerCount + 4 < count && nodesInPreOrder[innerCount + 3].Equals("[") && nodesInPreOrder[innerCount + 4].Equals("]"))
                                {
                                    funcReturnType = nodesInPreOrder[innerCount + 1] + "[]";
                                }
                                else
                                {
                                    funcReturnType = nodesInPreOrder[innerCount + 1];
                                }
                            }

                            if (nodesInPreOrder[innerCount + 1].Equals("}"))
                            {
                                classTable.Insert(new SymbolValue(name: funcName, kind: "Function", type: funcReturnType + ": " + funcParams));
                            }

                            innerCount++;
                        }
                    }
                }
                else if (nodesInPreOrder[index].Equals("integer") || nodesInPreOrder[index].Equals("float"))
                {
                    var offsetValueForVariable = nodesInPreOrder[index].Equals("integer") ? 4 : 8;

                    // Parse variable names and types, then insert into class table.
                    if (index + 9 < count && (nodesInPreOrder[index + 3]?.Equals("[") ?? false) && ((nodesInPreOrder[index + 4]?.Equals("]") ?? false) || (nodesInPreOrder[index + 5]?.Equals("]") ?? false)) && ((nodesInPreOrder[index + 6]?.Equals("[") ?? false) || (nodesInPreOrder[index + 7]?.Equals("[") ?? false)) && ((nodesInPreOrder[index + 7]?.Equals("]") ?? false) || (nodesInPreOrder[index + 8]?.Equals("]") ?? false) || (nodesInPreOrder[index + 9]?.Equals("]") ?? false)))
                    {
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[][]"));
                    }
                    else if (index + 5 < count && (nodesInPreOrder[index + 3]?.Equals("[") ?? false) && ((nodesInPreOrder[index + 4]?.Equals("]") ?? false) || (nodesInPreOrder[index + 5]?.Equals("]") ?? false)))
                    {
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[]", offset: offsetValueForVariable, scopedOffset: classTable.TableOffset + (offsetValueForVariable * int.Parse(nodesInPreOrder[index + 4]))));
                        classTable.TableOffset = classTable.TableOffset + (offsetValueForVariable * int.Parse(nodesInPreOrder[index + 4]));
                    }
                    else
                    {
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index], offset: offsetValueForVariable, scopedOffset: classTable.TableOffset + offsetValueForVariable));
                        classTable.TableOffset = classTable.TableOffset + offsetValueForVariable;
                    }
                }
                else if (allClassNames.Any(c => c.Equals(nodesInPreOrder[index]) && !nodesInPreOrder[3].Equals(nodesInPreOrder[index])) && !nodesInPreOrder[index-2].Equals("inherits"))
                {
                    // Parse variable names and types, then insert into function table.
                    if (nodesInPreOrder.Count() > index + 9 && nodesInPreOrder[index + 4].Equals("[") && (nodesInPreOrder[index + 5].Equals("]") || nodesInPreOrder[index + 6].Equals("]")) && (nodesInPreOrder[index + 7].Equals("[") || nodesInPreOrder[index + 8].Equals("[")) && (nodesInPreOrder[index + 8].Equals("]") || nodesInPreOrder[index + 9].Equals("]") || nodesInPreOrder[index + 10].Equals("]")))
                    {
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[][]"));
                    }
                    else if (nodesInPreOrder.Count() > index + 5 && nodesInPreOrder[index + 4].Equals("[") && (nodesInPreOrder[index + 5].Equals("]") || nodesInPreOrder[index + 6].Equals("]")))
                    {
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[]"));
                    }
                    else
                    {
                        var classVarTable = currentTable.TableEntries.Find(c => c.Name.Equals(nodesInPreOrder[index]) && c.Kind.Equals("Class"))?.Link;
                        classTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index], offset: classVarTable.TableOffset, scopedOffset: classTable.TableOffset + classVarTable.TableOffset));
                        classTable.TableOffset = classTable.TableOffset + classVarTable.TableOffset;
                    }
                }

                index++;
            }

            return classTable;
        }

        public static void SeparateFunctionDeclarations(TreeNode<string> currentNode, SymbolTable currentTable)
        {
            var nextNodes = Tree<string>.NextSubNodes(currentNode);

            // Separate each function node
            foreach (var node in nextNodes)
            {
                if (node.Value.Equals(""))
                {
                    return;
                }

                var nodesInPreOrder = Tree<string>.PreOrderingOfSubNodes(node, new List<string>());

                if (nodesInPreOrder.Where(c => c.Equals("Function:")).Count() > 1)
                {
                    var lastIndex = nodesInPreOrder.FindLastIndex(c => c.Equals("Function:"));
                    var firstList = nodesInPreOrder.GetRange(0, lastIndex);
                    var secondList = nodesInPreOrder.GetRange(lastIndex, nodesInPreOrder.Count - firstList.Count);

                    BuildTableFromFunctionNode(firstList, currentTable);
                    BuildTableFromFunctionNode(secondList, currentTable);
                }
                else
                {
                    BuildTableFromFunctionNode(node, currentTable);
                }
            }
        }

        public static void BuildTableFromFunctionNode(List<string> nodesInPreOrder, SymbolTable currentTable)
        {
            var isMemberFunc = nodesInPreOrder.Count() > 4 && nodesInPreOrder[3].Equals(":") && nodesInPreOrder[4].Equals(":");

            var funcTable = isMemberFunc ? new SymbolTable(name: nodesInPreOrder[2] +"_"+ nodesInPreOrder[6]) : new SymbolTable(name: nodesInPreOrder[2]);
            funcTable.IsAFunctionTable = true;
            var count = nodesInPreOrder.Count;
            var indexForFunction = isMemberFunc ? 7 : 3;
            var index = isMemberFunc ? 7 : 3;
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
                    if (nodesInPreOrder[index - 1].Equals(":"))
                    {
                        //this is a returned variable from a function decl
                    }
                    else if (nodesInPreOrder[index + 3].Equals("[") && (nodesInPreOrder[index + 4].Equals("]") || nodesInPreOrder[index + 5].Equals("]")) && (nodesInPreOrder[index + 6].Equals("[") || nodesInPreOrder[index + 7].Equals("[")) && (nodesInPreOrder[index + 7].Equals("]") || nodesInPreOrder[index + 8].Equals("]") || nodesInPreOrder[index + 9].Equals("]")))
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[][]"));
                    }
                    else if (nodesInPreOrder[index + 3].Equals("[") && (nodesInPreOrder[index + 4].Equals("]") || nodesInPreOrder[index + 5].Equals("]")))
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[]"));
                    }
                    else
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index]));
                    }
                }
                else if (allClassNames.Any(c => c.Equals(nodesInPreOrder[index])))
                {
                    // Parse variable names and types, then insert into function table.
                    if (index + 10 < count && nodesInPreOrder[index + 4].Equals("[") && (nodesInPreOrder[index + 5].Equals("]") || nodesInPreOrder[index + 6].Equals("]")) && (nodesInPreOrder[index + 7].Equals("[") || nodesInPreOrder[index + 8].Equals("[")) && (nodesInPreOrder[index + 8].Equals("]") || nodesInPreOrder[index + 9].Equals("]") || nodesInPreOrder[index + 10].Equals("]")))
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[][]"));
                    }
                    else if (index + 6 < count && nodesInPreOrder[index + 4].Equals("[") && (nodesInPreOrder[index + 5].Equals("]") || nodesInPreOrder[index + 6].Equals("]")))
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[]"));
                    }
                    else
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index]));
                    }
                }

                index++;
            }
        }

        public static void BuildTableFromFunctionNode(TreeNode<string> currentNode, SymbolTable currentTable)
        {
            var nodesInPreOrder = Tree<string>.PreOrderingOfSubNodes(currentNode, new List<string>());
            var isMemberFunc = nodesInPreOrder.Count() > 4 && nodesInPreOrder[3].Equals(":") && nodesInPreOrder[4].Equals(":");

            var funcTable = isMemberFunc ? new SymbolTable(name: nodesInPreOrder[2] +"_"+ nodesInPreOrder[6]) : new SymbolTable(name: nodesInPreOrder[2]);
            funcTable.IsAFunctionTable = true;
            var count = nodesInPreOrder.Count;
            var indexForFunction = isMemberFunc ? 7 : 3;
            var index = isMemberFunc ? 7 : 3;
            var funcParams = "";
            var funcReturnType = "";

            while (indexForFunction != count - 1)
            {
                // Get function parmeters
                if (nodesInPreOrder[indexForFunction].Equals("("))
                {
                    if (nodesInPreOrder.Count() > 8 && nodesInPreOrder[index + 4].Equals("[") && (nodesInPreOrder[index + 5].Equals("]") || nodesInPreOrder[index + 6].Equals("]")) && (nodesInPreOrder[index + 6].Equals("[") || nodesInPreOrder[index + 7].Equals("[")) && (nodesInPreOrder[index + 7].Equals("]") || nodesInPreOrder[index + 8].Equals("]") || nodesInPreOrder[index + 9].Equals("]")))
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
                    var offsetValueForVariable = nodesInPreOrder[index].Equals("integer") ? 4 : 8;

                    // Parse variable names and types, then insert into function table.
                    if (nodesInPreOrder[index - 1].Equals(":"))
                    {
                        //this is a returned variable from a function decl
                    }
                    else if (nodesInPreOrder[index + 3].Equals("[") && (nodesInPreOrder[index + 4].Equals("]") || nodesInPreOrder[index + 5].Equals("]")) && (nodesInPreOrder[index + 6].Equals("[") || nodesInPreOrder[index + 7].Equals("[")) && (nodesInPreOrder[index + 7].Equals("]") || nodesInPreOrder[index + 8].Equals("]") || nodesInPreOrder[index + 9].Equals("]")))
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[][]"));
                    }
                    else if (nodesInPreOrder[index + 3].Equals("[") && (nodesInPreOrder[index + 4].Equals("]") || nodesInPreOrder[index + 5].Equals("]")))
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[]"));
                    }
                    else
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index], offset: offsetValueForVariable, scopedOffset: funcTable.TableOffset + offsetValueForVariable));
                        funcTable.TableOffset = funcTable.TableOffset + offsetValueForVariable;
                    }
                }
                else if (allClassNames.Any(c => c.Equals(nodesInPreOrder[index])))
                {
                    // Parse variable names and types, then insert into function table.
                    if (index + 10 < count && nodesInPreOrder[index + 4].Equals("[") && (nodesInPreOrder[index + 5].Equals("]") || nodesInPreOrder[index + 6].Equals("]")) && (nodesInPreOrder[index + 7].Equals("[") || nodesInPreOrder[index + 8].Equals("[")) && (nodesInPreOrder[index + 8].Equals("]") || nodesInPreOrder[index + 9].Equals("]") || nodesInPreOrder[index + 10].Equals("]")))
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[][]"));
                    }
                    else if (index + 6 < count && nodesInPreOrder[index + 4].Equals("[") && (nodesInPreOrder[index + 5].Equals("]") || nodesInPreOrder[index + 6].Equals("]")))
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[]"));
                    }
                    else
                    {
                        funcTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index]));
                    }
                }

                index++;
            }

            currentTable.TableEntries.Find(c => c.Name.Equals(nodesInPreOrder[2]) && c.Kind.Equals("Function")).Offset = funcTable.TableOffset;
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
                    var offsetValueForVariable = nodesInPreOrder[index].Equals("integer") ? 4 : 8;

                    // Parse variable names and types, then insert into main table.
                    if (nodesInPreOrder[index + 3].Equals("[") && (nodesInPreOrder[index + 4].Equals("]") || nodesInPreOrder[index + 5].Equals("]")) && (nodesInPreOrder[index + 6].Equals("[") || nodesInPreOrder[index + 7].Equals("[")) && (nodesInPreOrder[index + 7].Equals("]") || nodesInPreOrder[index + 8].Equals("]") || nodesInPreOrder[index + 9].Equals("]")))
                    {
                        mainTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[][]"));
                    }
                    else if (nodesInPreOrder[index + 3].Equals("[") && nodesInPreOrder[index + 5].Equals("]"))
                    {
                        mainTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[]"));
                    }
                    else
                    {
                        mainTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index], offset: offsetValueForVariable, scopedOffset: mainTable.TableOffset + offsetValueForVariable));
                        mainTable.TableOffset = mainTable.TableOffset + offsetValueForVariable;
                    }
                }
                else if (allClassNames.Any(c => c.Equals(nodesInPreOrder[index])))
                {
                    // Parse variable names and types, then insert into function table.
                    if (nodesInPreOrder[index + 3].Equals("[") && (nodesInPreOrder[index + 4].Equals("]") || nodesInPreOrder[index + 5].Equals("]")) && (nodesInPreOrder[index + 6].Equals("[") || nodesInPreOrder[index + 7].Equals("[")) && (nodesInPreOrder[index + 7].Equals("]") || nodesInPreOrder[index + 8].Equals("]") || nodesInPreOrder[index + 9].Equals("]")))
                    {
                        mainTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[][]"));
                    }
                    else if (nodesInPreOrder[index + 3].Equals("[") && (nodesInPreOrder[index + 4].Equals("]") || nodesInPreOrder[index + 5].Equals("]")))
                    {
                        var classTable = currentTable.TableEntries.Find(c => c.Name.Equals(nodesInPreOrder[index]) && c.Kind.Equals("Class"))?.Link;
                        
                        mainTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index] + "[]", offset: classTable.TableOffset * int.Parse(nodesInPreOrder[index + 4]), scopedOffset: mainTable.TableOffset + (classTable.TableOffset * int.Parse(nodesInPreOrder[index + 4]))));
                        mainTable.TableOffset = mainTable.TableOffset + (classTable.TableOffset * int.Parse(nodesInPreOrder[index + 4]));
                    }
                    else
                    {
                        var classTable = currentTable.TableEntries.Find(c => c.Name.Equals(nodesInPreOrder[index]) && c.Kind.Equals("Class"))?.Link;
                        mainTable.Insert(new SymbolValue(name: nodesInPreOrder[index + 2], kind: "Variable", type: nodesInPreOrder[index], offset: classTable.TableOffset, scopedOffset: mainTable.TableOffset + classTable.TableOffset));
                        mainTable.TableOffset = mainTable.TableOffset + classTable.TableOffset;
                    }
                }

                index++;
            }

            currentTable.TableEntries.Find(c => c.Name.Equals("main") && c.Kind.Equals("Function")).Offset = mainTable.TableOffset;
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
                        errorList.Add("Error(5,8): multiple variables with same name in the same scope, var: " + variableSymbol.Name + " in Scope: " + table.Name);
                    }
                }

                foreach (var classSymbol in allClassesSymbols)
                {
                    if (allClassesSymbols.Where(c => c.Name.Equals(classSymbol.Name)).Count() > 1)
                    {
                        //Semantic Error: Multiples of a named class in the same scope error
                        errorList.Add("Error(5,8): multiple classes with same name, class name: " + classSymbol.Name);
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
                                errorList.Add("Error(5): function with same name and same type, function: " + functionSymbols.Name + " with Type: " + functionSymbols.Type);
                            }
                        }

                        //Semantic Warning: Overloading of functions should return warning
                        errorList.Add("Warning(9): function overloaded, function: " + functionSymbols.Name + " in class: " + table.Name);
                    }
                }
            }

            var classTables = symbolTable.GetClassTables();
            foreach (var table in classTables)
            {
                if (table.IsInherited)
                {
                    foreach (var inheritedTable in table.InheritedTables)
                    {
                        var parentTable = classTables.Find(t => t.Name.Equals(inheritedTable));
                        foreach (var parentSymbols in parentTable.TableEntries)
                        {
                            if (table.TableEntries.Where(t => t.Name.Equals(parentSymbols.Name) && t.Kind.Equals(parentSymbols.Kind) && t.Type.Equals(parentSymbols.Type)).Count() > 0)
                            {
                                //Warning for shadowed inherited members (classes)
                                errorList.Add("Warning(5): shadowed inherited class member: " + parentSymbols.Name);
                            }
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
                            errorList.Add("Warning(14): Circular dependencies for classes: " + table.Name + " and " + getClassTable.Name);
                        }
                    }
                }
            }

            return errorList;
        }

        public static (List<(List<string>, SymbolTable)> subScopeList, List<string> errorList) SemanticAnalysis(Tree<string> tree, SymbolTable symbolTable, List<string> errorList)
        {
            TreeNode<string> root = tree.Root;
            var listOfSubScopes = new List<(List<string>, SymbolTable)>();
            var progStart = root.GetChild(0);

            var classes = progStart.GetChild(0);
            var nextClassNodes = Tree<string>.NextSubNodes(classes);
            var allCalledFunctions = new List<(string fName, string fParams)>();
            var allFunctions = symbolTable.GetFunctions();
			var functionHasReturnType = new Dictionary<SymbolValue, bool>();

			foreach (var funk in allFunctions)
			{
				functionHasReturnType.Add(funk, false);
			}

            // Separate each class node
            foreach (var classNode in nextClassNodes)
            {
                var subscopeNodes = Tree<string>.PreOrderingOfSubNodes(classNode, new List<string>());
                var classTable = subscopeNodes.Count <=3 ? null : symbolTable.TableEntries.FirstOrDefault(c => c.Name.Equals(subscopeNodes[3]))?.Link;

                if (classTable != null)
                {
                    var subScopeEntry = (subscopeNodes, classTable);
                    listOfSubScopes.Add(subScopeEntry);
                }
            }

            var functions = progStart.GetChild(1);
            var nextFunctionNodes = Tree<string>.NextSubNodes(functions);

            // Separate each class node
            foreach (var functionNode in nextFunctionNodes)
            {
                if (!functionNode.Value.Equals(""))
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
                var passedReturnType = false;

                while (index != count - 1)
                {
                    // if func have we passed its return type
                    if (isaFunc)
                    {
                        if (list[index].Equals(":"))
                        {
                            passedReturnType = true;
                        }
                    }

                    // Id must by defined in scope
                    if (list[index].Equals("id") && !isaFunc)
                    {
                        var idName = list[index + 1];

                        if (!idName.Equals("integer") && !idName.Equals("float") && !idName.Equals("do") && !idName.Equals("["))
                        {
                            // Check tables
                            var checkGlobalTable = symbolTable.TableEntries.Where(c => c.Name.Equals(idName));
                            var checkLocalTable = subScope.Item2.TableEntries.Where(c => c.Name.Equals(idName));

                            if (checkGlobalTable.Count() == 0 && checkLocalTable.Count() == 0)
                            {
                                errorList.Add("Error(11): use of undeclared local variable, class, or function, named: " + idName + " in class/function: " + subScope.Item2.Name);
                            }
                        }

						if (list[index + 2].Equals("["))
						{
							if (list[index + 5].Equals("[") || list[index + 6].Equals("["))
							{
								var checkLocalTable = subScope.Item2.TableEntries.Where(c => c.Name.Equals(idName) && c.Type.Contains("[][]"));

								if (checkLocalTable.Count() == 0)
								{
									errorList.Add("Error(13): array dimensions improper use, for array: " + idName);
								}
							}
							else
							{
								var typeError = subScope.Item2.TableEntries.Where(c => c.Name.Equals(idName) && c.Type.Contains("[][]"));

								var checkLocalTable = subScope.Item2.TableEntries.Where(c => c.Name.Equals(idName) && c.Type.Contains("[]"));

								if (typeError.Count() == 1 || checkLocalTable.Count() == 0)
								{
									errorList.Add("Error(13): array dimensions improper use, for array: " + idName);
								}
							}

						}
                    }
                    else if (list[index].Equals("id") && isaFunc && passedReturnType)
                    {
                        var idName = list[index + 1];

                        if (!idName.Equals("integer") && !idName.Equals("float") && !idName.Equals("do") && !idName.Equals("["))
                        {
                            // Check tables
                            var checkGlobalTable = symbolTable.TableEntries.Where(c => c.Name.Equals(idName));
                            var checkLocalTable = subScope.Item2.TableEntries.Where(c => c.Name.Equals(idName));

                            if (checkGlobalTable.Count() == 0 && checkLocalTable.Count() == 0)
                            {
                                errorList.Add("Error(11): use of undeclared local variable or function, name: " + idName + " in class/function: " + subScope.Item2.Name);
                            }
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
                                            errorList.Add("Error(13): array index not an integer, array: " + firstOperand);
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
                                            errorList.Add("Error(13): array index not an integer, array: " + firstOperand);
                                        }
                                    }
                                    else
                                    {
                                        firstOperand = list[index - 9];

                                        var arrayVar1 = list[index - 2];
                                        var arrayVar2 = list[index - 6];
                                        if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar1)).Type.Equals("integer") || !subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar2)).Type.Equals("integer"))
                                        {
                                            errorList.Add("Error(13): array index not an integer, array: " + firstOperand);
                                        }
                                    }
                                }
                                else
                                {
                                    firstOperand = list[index - 5];

                                    var arrayVar = list[index - 2];
                                    if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar)).Type.Equals("integer"))
                                    {
                                        errorList.Add("Error(13): array index not an integer, array: " + firstOperand);
                                    }
                                }
                            }

                            firstOperandType = subScope.Item2.TableEntries.FirstOrDefault(c => c.Name.Equals(firstOperand))?.Type;
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
                            secondOperandType = subScope.Item2.TableEntries.FirstOrDefault(c => c.Name.Equals(firstOperand))?.Type;

                            // array index type checking
                            if (int.TryParse(list[index + 4], out _))
                            {
                                if (list[index + 6].Equals("["))
                                {
                                    if (int.TryParse(list[index + 7], out _))
                                    {
                                    }
									else
									{
										var arrayVar = list[index + 8];
										if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar)).Type.Equals("integer"))
										{
											errorList.Add("Error(13): array index not an integer, array: " + secondOperand);
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
                                            errorList.Add("Error(13): array index not an integer, array: " + secondOperand);
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
                            secondOperandType = subScope.Item2.TableEntries.FirstOrDefault(c => c.Name.Equals(secondOperand))?.Type;

                            if (secondOperand != null && secondOperandType == null)
                            {
                                var returnEntry = symbolTable.TableEntries.FirstOrDefault(c => c.Name.Equals(secondOperand))?.Type;
                                if (returnEntry.Contains(":"))
                                {
                                    secondOperandType = returnEntry.Split(':')[0];
                                }
                                else
                                {
                                    secondOperandType = returnEntry;
                                }
                            }
                        }

                        if (firstOperandType == null || secondOperandType == null)
                        {
                            if (firstOperandType != secondOperandType)
                            {
                                errorList.Add("Error(10): expression types are incompatible, first operand: " + firstOperand + ", second operand: " + secondOperand);
                            }
                        }
                        else if (!firstOperandType.Contains(secondOperandType))
                        {
                            errorList.Add("Error(10): expression types are incompatible, first operand: " + firstOperand + ", second operand: " + secondOperand);
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
                                            errorList.Add("Error(13): array index not an integer, array: " + firstOperand);
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
                                            errorList.Add("Error(13): array index not an integer, array: " + firstOperand);
                                        }
                                    }
                                    else
                                    {
                                        firstOperand = list[index - 10];

                                        var arrayVar1 = list[index - 3];
                                        var arrayVar2 = list[index - 7];
                                        if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar1)).Type.Equals("integer") || !subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar2)).Type.Equals("integer"))
                                        {
                                            errorList.Add("Error(13): array index not an integer, array: " + firstOperand);
                                        }
                                    }
                                }
                                else
                                {
                                    firstOperand = list[index - 6];

                                    var arrayVar = list[index - 3];
                                    if (!subScope.Item2.TableEntries.First(c => c.Name.Equals(arrayVar)).Type.Equals("integer"))
                                    {
                                        errorList.Add("Error(13): array index not an integer, array: " + firstOperand);
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
                                            errorList.Add("Error(13): array index not an integer, array: " + secondOperand);
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
                                            errorList.Add("Error(13): array index not an integer, array: " + secondOperand);
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
                            secondOperandType = subScope.Item2.TableEntries.FirstOrDefault(c => c.Name.Equals(firstOperand))?.Type;
                        }

                        if (!(firstOperandType?.Contains(secondOperandType ?? string.Empty) ?? true) || (firstOperandType != secondOperandType))
                        {
                            errorList.Add("Error(10): expression types are incompatible, first operand: " + firstOperand + ", second operand: " + secondOperand);
                        }
                    }

                    //Type checking function return types
                    if (list[index].Equals("return") && isaFunc)
                    {
                        var returnVariable = "";
                        var returnType = "";

                        if (list[index + 4].Equals("["))
                        {
                            returnVariable = list[index + 3];
                            returnType = subScope.Item2.TableEntries.First(c => c.Name.Equals(returnVariable)).Type;

                            // array index type checking
                            if (int.TryParse(list[index + 5], out _))
                            {
                                if (list[index + 7].Equals("["))
                                {
                                    if (int.TryParse(list[index + 8], out _))
                                    {
                                        var arrayVar = list[index + 9];
                                        if (!(subScope.Item2.TableEntries.FirstOrDefault(c => c.Name.Equals(arrayVar))?.Type.Equals("integer") ?? false))
                                        {
                                            errorList.Add("Error(13): array index not an integer, array: " + returnVariable);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (list[index + 8].Equals("["))
                                {
                                    if (int.TryParse(list[index + 9], out _))
                                    {
                                        var arrayVar1 = list[index + 6];
                                        var arrayVar2 = list[index + 10];
                                        if (!(subScope.Item2.TableEntries.FirstOrDefault(c => c.Name.Equals(arrayVar1))?.Type.Equals("integer") ?? false) || !(subScope.Item2.TableEntries.FirstOrDefault(c => c.Name.Equals(arrayVar2))?.Type.Equals("integer") ?? false))
                                        {
                                            errorList.Add("Error(13): array index not an integer, array: " + returnVariable);
                                        }
                                    }
                                }
                            }
                        }
                        else if (int.TryParse(list[index + 2], out _))
                        {
                            returnVariable = list[index + 2];
                            returnType = "integer";
                        }
                        else if (float.TryParse(list[index + 2], out _))
                        {
                            returnVariable = list[index + 2];
                            returnType = "float";
                        }
                        else
                        {
                            returnVariable = list[index + 3];
                            returnType = subScope.Item2.TableEntries.FirstOrDefault(c => c.Name.Equals(returnVariable))?.Type;
                        }

                        var functionName = list[2];
                        var tableReturnType = symbolTable.TableEntries.FirstOrDefault(c => c.Name.Equals(functionName) && c.Kind.Equals("Function") && c.Type.Contains(returnType));

                        if (tableReturnType == null)
                        {
                            errorList.Add("Error(10): returned value type does not match function return type, function: " + functionName);
                        }
						else
						{
							functionHasReturnType[tableReturnType] = true;
						}
                    }

                    //'.' dot operator used properly
                    if (list[index].Equals("."))
                    {
                        var parentVarName = list[index - 1];
                        var functionOrVariableCalled = list[index + 2];

                        var classNameContaining = subScope.Item2.TableEntries.Find(c => c.Name.Equals(parentVarName))?.Type;

                        if (classNameContaining == null)
                        {
                            // Error using dot operator
                            errorList.Add("Error(15): Dot Operator error, class not found, for variable: " + parentVarName);
                        }

                        // find class in global table
                        var classEntry = symbolTable.TableEntries.Where(c => c.Name.Equals(classNameContaining) && c.Kind.Equals("Class"));

                        if (classEntry.Count() == 1)
                        {
                            var classTable = classEntry.First().Link;
                            var countCorrect = classTable.TableEntries.Where(c => c.Name.Equals(functionOrVariableCalled));

                            if (countCorrect.Count() == 0)
                            {
                                // Error using dot operator
                                errorList.Add("Error(15): Dot Operator error, variable/function not found in class: " + classNameContaining + " for variable/function: " + functionOrVariableCalled);
                            }

                        }
                        else
                        {
                            // Error using dot operator
                            errorList.Add("Error(15): Dot Operator error, class not found, for class: " + classNameContaining + " of variable: " + parentVarName);
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
                                    var varType = subScope.Item2.TableEntries.FirstOrDefault(c => c.Name.Equals(varName) && c.Kind.Equals("Variable"))?.Type;
                                    paramType = varType;
                                    parameters.Add(paramType);
                                }
                                else if (int.TryParse(list[index + paramIndex], out _))
                                {
                                    parameters.Add("integer");
                                }
                                else if (float.TryParse(list[index + paramIndex], out _))
                                {
                                    parameters.Add("float");
                                }

                                newParam = false;
                            }

                            if (list[index + paramIndex].Equals(","))
                            {
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

                        if (!symbolTable.GetFunctions().Any(c => c.Name.Equals(functionName) && c.Type.Split(':')[1].Trim().Equals(concatenatedParams)))
                        {
                            // Error function parameters do not match function definition
                            errorList.Add("Error(12): function call parameters do not match function definition, function: " + functionName);
                        }
                    }

                    // Every called function has a definition
                    foreach (var functionThatWasCalled in functionsCalled)
                    {
                        if (!allFunctions.Any(c => c.Name.Equals(functionThatWasCalled.fName) && c.Type.Contains(functionThatWasCalled.fParams)))
                        {
                            errorList.Add("Error(6): function called does not contain definition, function: " + functionThatWasCalled.fName + ", with parameters: " + functionThatWasCalled.fParams);
                        }
                    }

                    index++;
                }
            }

			foreach (var funk in functionHasReturnType)
			{
				if (!funk.Value && !funk.Key.Name.Equals("main"))
				{
					errorList.Add("Error(10): function does not contain a return statement, function: " + funk.Key.Name);
				}
			}

            // Every function definition has a function call
            foreach (var aFunc in allFunctions)
            {
                if (!allCalledFunctions.Any(c => c.fName.Equals(aFunc.Name) && aFunc.Type.Contains(c.fParams)) && !aFunc.Name.Equals("main"))
                {
                    errorList.Add("Warning(6): function definition is not used, function: " + aFunc.Name + ", with return and parameters; " + aFunc.Type);
                }
            }

            return (listOfSubScopes, errorList);
        }
    }
}
