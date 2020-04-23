using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynSemDriver
{
	public class CodeGenerator
	{
		public static Stack<string> registerPool = new Stack<string>();
		public static List<string> moonCode = new List<string>();
		public static List<string> moonExecCode = new List<string>()
			{
				"\n\tentry"
			};
		public static List<string> moonDataCode = new List<string>();
		public static List<string> moonFunctionCode = new List<string>();
		public static List<string> calledFunctionsParams = new List<string>();
		public static List<string> declaredClassVariables = new List<string>();
		public static Dictionary<string, int> declaredArraySizes = new Dictionary<string, int>();

		// Counters for executable code
		public static int literalCount = 0;
		public static int tempCount = 0;
		public static int ifCount = 0;
		public static int loopCount = 0;

		// Counters for function code
		public static int literalCountFunc = 0;
		public static int tempCountFunc = 0;
		public static int ifCountFunc = 0;
		public static int loopCountFunc = 0;
		public static int funcParamCount = 0;
		public static int funcReturnCount = 0;

		public static List<string> GenerateMoonCode(List<(List<string>, SymbolTable)> subScopeList, SymbolTable symbolTable)
		{
			StartRegisters();

			// Generate Function code
			var functions = subScopeList.Where(l => l.Item2.IsAFunctionTable);

			foreach (var function in functions)
			{
				GenerateFunctionCode(function, symbolTable);
			}

			// Generate main method code - No functions yet.
			var mainMethod = subScopeList.Find(l => l.Item2.Name.Equals("main"));
			var inDeclarations = false;
			var inStatements = false;
			var inLoop = false;
			var inIf = false;
			var statementStack = new Stack<string>();
			var ifCountStack = new Stack<int>();
			var loopCountStack = new Stack<int>();
			var startingIfOrLoop = false;


			var lineOfCodeToInterpret = new List<string>();
			foreach (var items in mainMethod.Item1)
			{
				if (items.Equals("local"))
				{
					inDeclarations = true;
					items.Remove(items.IndexOf("local"));
				}
				else if (items.Equals("do"))
				{
					inDeclarations = false;
					inStatements = true;
					statementStack.Push("do");
				}
				else if (inStatements && items.Equals("if"))
				{
					// store what you are in 
					statementStack.Push("if");
					inIf = true;
					startingIfOrLoop = true;
				}
				else if (inStatements && items.Equals("while"))
				{
					// store what you are in 
					statementStack.Push("while");
					inLoop = true;
					startingIfOrLoop = true;
				}
				else if (inStatements && items.Equals("end"))
				{
					// pop where you are 
					var popped = statementStack.Pop();
					inIf = statementStack.Contains("if");
					inLoop = statementStack.Contains("while");
				}

				//Console.WriteLine(items);
				if (items.Equals(";") || (items.Equals(")") && inLoop && startingIfOrLoop) || (items.Equals(")") && inIf && startingIfOrLoop))
				{
					// interpret the line of code
					startingIfOrLoop = false;

					if ((lineOfCodeToInterpret.Count() == 1 || lineOfCodeToInterpret.Count() == 0) && items.Equals(";") && (inLoop || inIf))
					{
						var ending = statementStack.Pop();
						inIf = statementStack.Contains("if");
						inLoop = statementStack.Contains("while");

						// write out while loop ending 
						if (ending.Equals("while"))
						{
							var currentloopCount = loopCountStack.Peek();
							var startWhileNumber = "gowhile" + currentloopCount;
							var endWhileNumber = "endwhile" + currentloopCount;
							moonExecCode.Add("\tj " + startWhileNumber);
							moonExecCode.Add(endWhileNumber);

							loopCountStack.Pop();
						}
						else if (ending.Equals("if"))
						{
							var currentIfCount = ifCountStack.Peek();
							// write out else beginning 
							var endIfNumber = "endif" + currentIfCount;
							moonExecCode.Add("\n\t%- End if statement -%");
							moonExecCode.Add(endIfNumber);

							ifCountStack.Pop();
						}
					}


					if (lineOfCodeToInterpret.Contains("else") && inStatements && inIf)
					{
						var currentIfCount = ifCountStack.Peek();
						// write out else beginning 
						var endElseNumber = "else" + currentIfCount;
						var endIfNumber = "endif" + currentIfCount;

						moonExecCode.Add("\n\t%- Start branch for else statement -%");
						moonExecCode.Add("\tj " + endIfNumber);
						moonExecCode.Add(endElseNumber);
					}

					if (lineOfCodeToInterpret.Contains("while") && inStatements && inLoop)
					{
						loopCountStack.Push(++loopCount);

						// write out while loop beginning 
						var startWhileNumber = "gowhile" + loopCount;
						var endWhileNumber = "endwhile" + loopCount;
						moonExecCode.Add(startWhileNumber);
						var expressionResult = ComputeExpression(lineOfCodeToInterpret, symbolTable: mainMethod.Item2);

						var register = registerPool.Pop();
						moonExecCode.Add("\n\t%- Branch to end while loop -%");
						moonExecCode.Add("\tlw " + register + "," + expressionResult + "(R0)");
						moonExecCode.Add("\tbz " + register + "," + endWhileNumber);
						registerPool.Push(register);

					}
					else if (lineOfCodeToInterpret.Contains("if") && inStatements && inIf)
					{
						// write out while loop beginning 
						ifCountStack.Push(++ifCount);
						var startElseNumber = "else" + ifCount;
						var expressionResult = ComputeExpression(lineOfCodeToInterpret, symbolTable: mainMethod.Item2);

						var register = registerPool.Pop();
						moonExecCode.Add("\n\t%- Start branch for if statement -%");
						moonExecCode.Add("\tlw " + register + "," + expressionResult + "(R0)");
						moonExecCode.Add("\tbz " + register + "," + startElseNumber);
						registerPool.Push(register);
					}
					else if (lineOfCodeToInterpret.Contains("write") && inStatements)
					{
						//assignment of variable
						var ids = lineOfCodeToInterpret.Where(c => c.Equals("id"));
						var containsExpression = ContainsExpression(lineOfCodeToInterpret);

						if (ids.Count() == 0 && !containsExpression)
						{
							var parenthesisIndex = lineOfCodeToInterpret.IndexOf("(");
							int theInt;
							// not sure how to print float
							if (int.TryParse(lineOfCodeToInterpret[parenthesisIndex + 1], out theInt))
							{
								moonExecCode.Add("\n\t%- Printing int " + theInt + " -%");
								moonExecCode.Add("\taddi R1,R0," + theInt);
								moonExecCode.Add("\tjl R15,putint");

								moonExecCode.Add("\n\t%- Printing a space -%");
								moonExecCode.Add("\tputc R0");
							}
						}
						else if (ids.Count() == 1 && !containsExpression)
						{
							var idIndex = lineOfCodeToInterpret.IndexOf("id");
							var value = lineOfCodeToInterpret[idIndex + 1];

							if ((idIndex + 2) < lineOfCodeToInterpret.Count() && lineOfCodeToInterpret[idIndex + 2].Equals("["))
							{
								var assignVarSymbol = mainMethod.Item2.TableEntries.Find(c => c.Name.Equals(value) && c.Kind.Equals("Variable"));
								var offsetMultiplier = 0;

								if (assignVarSymbol.Type.Contains("integer"))
								{
									offsetMultiplier = 4;
								}
								else if (assignVarSymbol.Type.Contains("float"))
								{
									offsetMultiplier = 8;
								}
								else
								{
									//compute class multiplier
								}

								moonExecCode.Add("\n\t%- Printing value of " + value + " at " + lineOfCodeToInterpret[idIndex + 3] + " -%");
								moonExecCode.Add("\taddi R3,R0," + int.Parse(lineOfCodeToInterpret[idIndex + 3]) * offsetMultiplier);
								moonExecCode.Add("\tlw R2," + value + "(R3)");
								moonExecCode.Add("\tadd R1,R0,R2");
								moonExecCode.Add("\tjl R15,putint");

								moonExecCode.Add("\n\t%- Printing a space -%");
								moonExecCode.Add("\tputc R0");
							}
							else
							{
								moonExecCode.Add("\n\t%- Printing value of " + value + " -%");
								moonExecCode.Add("\tlw R2," + value + "(R0)");
								moonExecCode.Add("\tadd R1,R0,R2");
								moonExecCode.Add("\tjl R15,putint");

								moonExecCode.Add("\n\t%- Printing a space -%");
								moonExecCode.Add("\tputc R0");
							}
						}
						else if (ids.Count() == 2 && lineOfCodeToInterpret.Contains("[") && !containsExpression)
						{
							var idIndex = lineOfCodeToInterpret.IndexOf("id");
							if (lineOfCodeToInterpret[idIndex + 2].Equals("["))
							{
								var varValue = lineOfCodeToInterpret[idIndex + 1];
								var offsetVar = lineOfCodeToInterpret[idIndex + 4];

								moonExecCode.Add("\n\t%- Printing value of " + varValue + " -%");
								moonExecCode.Add("\tlw R3," + offsetVar + "(R0)");
								moonExecCode.Add("\taddi R4,R3,4");
								moonExecCode.Add("\tlw R2," + varValue + "(R4)");
								moonExecCode.Add("\tadd R1,R0,R2");
								moonExecCode.Add("\tjl R15,putint");

								moonExecCode.Add("\n\t%- Printing a space -%");
								moonExecCode.Add("\tputc R0");
							}
							else
							{
								if (lineOfCodeToInterpret.Contains("."))
								{
									var value = lineOfCodeToInterpret[idIndex + 1];
									var dotIndex = lineOfCodeToInterpret.IndexOf(".");
									var innerVariable = lineOfCodeToInterpret[dotIndex + 2];
									var bracketIndex = lineOfCodeToInterpret.IndexOf("[");
									var offset = lineOfCodeToInterpret[bracketIndex + 1];

									moonExecCode.Add("\n\t%- Printing value of " + value + "_" + innerVariable + "[" + offset + "] -%");
									moonExecCode.Add("\taddi R3,R0," + (int.Parse(offset) * 4));
									moonExecCode.Add("\tlw R2," + value + "_" + innerVariable + "(R3)");
									moonExecCode.Add("\tadd R1,R0,R2");
									moonExecCode.Add("\tjl R15,putint");

									moonExecCode.Add("\n\t%- Printing a space -%");
									moonExecCode.Add("\tputc R0");
								}
							}
						}
						else if (containsExpression)
						{ }
						else
						{
							// write class members
							if (lineOfCodeToInterpret.Contains("."))
							{
								var idIndex = lineOfCodeToInterpret.IndexOf("id");
								var value = lineOfCodeToInterpret[idIndex + 1];
								var dotIndex = lineOfCodeToInterpret.IndexOf(".");
								var innerVariable = lineOfCodeToInterpret[dotIndex + 2];

								moonExecCode.Add("\n\t%- Printing value of " + value + "_" + innerVariable + " -%");
								moonExecCode.Add("\tlw R2," + value + "_" + innerVariable + "(R0)");
								moonExecCode.Add("\tadd R1,R0,R2");
								moonExecCode.Add("\tjl R15,putint");

								moonExecCode.Add("\n\t%- Printing a space -%");
								moonExecCode.Add("\tputc R0");
							}
						}
					}
					else if (lineOfCodeToInterpret.Contains("read") && inStatements)
					{
						//assignment of variable
						var ids = lineOfCodeToInterpret.Where(c => c.Equals("id"));
						var containsExpression = ContainsExpression(lineOfCodeToInterpret);

						if (ids.Count() == 0 && !containsExpression)
						{
							// can not have this scenario, no variable to store read int
						}
						else if (ids.Count() == 1 && !containsExpression)
						{
							var idIndex = lineOfCodeToInterpret.IndexOf("id");
							var value = lineOfCodeToInterpret[idIndex + 1];

							if ((idIndex + 2) < lineOfCodeToInterpret.Count() && lineOfCodeToInterpret[idIndex + 2].Equals("["))
							{
								var assignVarSymbol = mainMethod.Item2.TableEntries.Find(c => c.Name.Equals(value) && c.Kind.Equals("Variable"));
								var offsetMultiplier = 0;

								if (assignVarSymbol.Type.Contains("integer"))
								{
									offsetMultiplier = 4;
								}
								else if (assignVarSymbol.Type.Contains("float"))
								{
									offsetMultiplier = 8;
								}
								else
								{
									//compute class multiplier
								}

								moonExecCode.Add("\n\t%- Reading value from console and saving it into " + value + "[" + int.Parse(lineOfCodeToInterpret[idIndex + 3]) + "] -%");
								moonExecCode.Add("\tjl R15,getint");
								moonExecCode.Add("\taddi R2,R0," + int.Parse(lineOfCodeToInterpret[idIndex + 3]) * offsetMultiplier);
								moonExecCode.Add("\tsw " + value + "(R2),R1");
							}
							else
							{
								moonExecCode.Add("\n\t%- Reading value from console and saving it into " + value + " -%");
								moonExecCode.Add("\tjl R15,getint");
								moonExecCode.Add("\tsw " + value + "(R0),R1");
							}
						}
						else if (ids.Count() == 2 && lineOfCodeToInterpret.Contains("[") && !containsExpression)
						{
							var idIndex = lineOfCodeToInterpret.IndexOf("id");
							if (lineOfCodeToInterpret[idIndex + 2].Equals("["))
							{
								var varValue = lineOfCodeToInterpret[idIndex + 1];
								var offsetVar = lineOfCodeToInterpret[idIndex + 4];

								moonExecCode.Add("\n\t%- Reading value from console and saving it into " + varValue + "[" + offsetVar + "] -%");
								moonExecCode.Add("\tjl R15,getint");
								moonExecCode.Add("\tlw R2," + offsetVar + "(R0)");
								moonExecCode.Add("\tmuli R3,R2,4");
								moonExecCode.Add("\tsw " + varValue + "(R3),R1");
							}
							else
							{
								//wrong in form a.b[3]
								//if (lineOfCodeToInterpret.Contains("."))
								//{
								//    var idIndex = lineOfCodeToInterpret.IndexOf("id");
								//    var value = lineOfCodeToInterpret[idIndex + 1];
								//    var dotIndex = lineOfCodeToInterpret.IndexOf(".");
								//    var innerVariable = lineOfCodeToInterpret[dotIndex + 2];

								//    moonExecCode.Add("\n\t%- Printing value of " + value + "_" + innerVariable + " -%");
								//    moonExecCode.Add("\tlw R2," + value + "_" + innerVariable + "(R0)");
								//    moonExecCode.Add("\tadd R1,R0,R2");
								//    moonExecCode.Add("\tjl R15,putint");
								//}

								//var value = lineOfCodeToInterpret[idIndex + 1];
								//moonExecCode.Add("\n\t%- Printing value of " + value + " -%");
								//moonExecCode.Add("\taddi R1,R0," + value);
								//moonExecCode.Add("\tjl R15,putint");
							}
						}
						else if (containsExpression)
						{ }
						else
						{
							// write class members
							if (lineOfCodeToInterpret.Contains("."))
							{
								var idIndex = lineOfCodeToInterpret.IndexOf("id");
								var value = lineOfCodeToInterpret[idIndex + 1];
								var dotIndex = lineOfCodeToInterpret.IndexOf(".");
								var innerVariable = lineOfCodeToInterpret[dotIndex + 2];

								moonExecCode.Add("\n\t%- Reading value from console and saving it into " + value + "." + innerVariable + " -%");
								moonExecCode.Add("\tjl R15,getint");
								moonExecCode.Add("\tsw " + value + "_" + innerVariable + "(R0),R1");
							}
						}

					}
					else if (lineOfCodeToInterpret.Contains("(") && inStatements)
					{
						var parenthesisIndex = lineOfCodeToInterpret.IndexOf("(");
						var functionCallName = lineOfCodeToInterpret[parenthesisIndex - 1];
						var commaCount = lineOfCodeToInterpret.Where(c => c.Equals(",")).Count();
						var parameterCount = commaCount + 1;
						var paramListString = "";
						var paramList = new List<string>();
						var funcParamCounter = 0;
						var firstparam = true;

						if (lineOfCodeToInterpret[parenthesisIndex + 1].Equals(")"))
						{
							parameterCount = 0;
						}

						if (parameterCount > 0)
						{
							paramList = lineOfCodeToInterpret.GetRange(parenthesisIndex + 1, lineOfCodeToInterpret.Count() - (parenthesisIndex + 1));
						}

						// code for parameters... need to expand
						for (var i = 0; i < parameterCount; i++)// (parameterCount == 1)
						{
							//quick one param function 
							if (int.TryParse(paramList[0], out _))
							{
								if (firstparam)
								{
									paramListString = "integer";
									firstparam = false;
								}
								else
								{
									paramListString = ", integer";
								}

								var paramName = functionCallName + "_param" + ++funcParamCounter;

								if (!calledFunctionsParams.Contains(paramName))
								{
									moonDataCode.Add(paramName + " res 4");
									calledFunctionsParams.Add(paramName);
								}

								var register = registerPool.Pop();
								moonExecCode.Add("\n\t%- Assigning function parameter " + paramName + " -%");
								moonExecCode.Add("\taddi " + register + ",R0," + paramList[0]);
								moonExecCode.Add("\tsw " + paramName + "(R0)," + register);
								registerPool.Push(register);

								paramList.RemoveAt(0);
								paramList.RemoveAt(0);
							}
							else if (float.TryParse(paramList[0], out _))
							{
								if (firstparam)
								{
									paramListString = "float";
									firstparam = false;
								}
								else
								{
									paramListString = ", float";
								}

								// cant handle floats yet

								paramList.RemoveAt(0);
								paramList.RemoveAt(0);
							}
							else if (paramList[0].Equals("id") && !lineOfCodeToInterpret.Contains("["))
							{
								var variName = paramList[1];
								var paramSymbol = mainMethod.Item2.TableEntries.Find(c => c.Name.Equals(variName));
								var paramType = paramSymbol?.Type;

								if (firstparam)
								{
									paramListString = paramType;
									firstparam = false;
								}
								else
								{
									paramListString += ", " + paramType;
								}

								var paramName = functionCallName + "_param" + ++funcParamCounter;

								var register = registerPool.Pop();
								if (!calledFunctionsParams.Contains(paramName))
								{
									if (paramType.Contains("["))
									{
										var varSize = declaredArraySizes[variName];
										moonDataCode.Add(paramName + " res " + 4 * varSize);
										calledFunctionsParams.Add(paramName);
									}
									else
									{
										moonDataCode.Add(paramName + " res 4");
										calledFunctionsParams.Add(paramName);
									}
								}
								moonExecCode.Add("\n\t%- Assigning function parameter " + paramName + " -%");
								moonExecCode.Add("\tlw " + register + "," + variName + "(R0)");
								moonExecCode.Add("\tsw " + paramName + "(R0)," + register);
								registerPool.Push(register);

								paramList.RemoveAt(0);
								paramList.RemoveAt(0);
								paramList.RemoveAt(0);
							}
						}

						var functionInfo = symbolTable.TableEntries.Find(c => c.Name.Equals(functionCallName) && c.Kind.Equals("Function") && c.Type.Contains(paramListString));

						var returnEnd = functionInfo.Type.IndexOf(":");
						var funcReturnType = functionInfo.Type.Substring(0, returnEnd);

						// code for result
						if (funcReturnType.Contains("void"))
						{

						}
						else if (funcReturnType.Contains("integer"))
						{
							moonDataCode.Add(functionCallName + "_return res 4");
							//not array yet                           
						}
						else if (funcReturnType.Contains("float"))
						{
							// not yet
						}
						else
						{
							// class
						}

						// code to function call
						moonExecCode.Add("\n\t%- Jump to function " + functionCallName + " -%");
						moonExecCode.Add("\tjl R15," + functionCallName);

						// code saving function result
						if (funcReturnType.Contains("void"))
						{

						}
						else if (funcReturnType.Contains("integer"))
						{
							//not array yet
							var idIndex = lineOfCodeToInterpret.IndexOf("id");

							if (lineOfCodeToInterpret[idIndex + 2].Equals("["))
							{

							}
							else
							{
								var register = registerPool.Pop();
								moonExecCode.Add("\n\t%- Assigning return variable to " + lineOfCodeToInterpret[idIndex + 1] + " -%");
								moonExecCode.Add("\tlw " + register + "," + functionCallName + "_return(R0)");
								moonExecCode.Add("\tsw " + lineOfCodeToInterpret[idIndex + 1] + "(R0)," + register);
								registerPool.Push(register);

							}

						}
						else if (funcReturnType.Contains("float"))
						{
							// not yet
						}
						else
						{
							// class
						}

					}
					else if (lineOfCodeToInterpret.Contains("=") && inStatements)
					{
						//assignment of variable
						var countOfItems = lineOfCodeToInterpret.Count();
						var indexOfEquals = lineOfCodeToInterpret.IndexOf("=");
						var leftSide = lineOfCodeToInterpret.GetRange(0, indexOfEquals);
						var rightSide = lineOfCodeToInterpret.GetRange(indexOfEquals + 1, countOfItems - leftSide.Count() - 1);


						var idIndexLeft = leftSide.IndexOf("id");
						var varNameToAssign = leftSide[idIndexLeft + 1];
						var idIndexRight = 0;
						var varName = "";

						if (ContainsExpression(rightSide))
						{
							var returnOfExpression = ComputeExpression(rightSide, symbolTable: mainMethod.Item2);

							// todo
							if (leftSide.Contains("["))
							{
								var arrayIndex = leftSide.IndexOf("[");
								var offsetRegister = "R0";

								if (leftSide[arrayIndex + 1].Equals("id"))
								{
									// array value with index from an id
									// not done yet
								}
								else if (leftSide.Contains("."))
								{
									var litValue = leftSide[arrayIndex + 1];
									var dotIndex = leftSide.IndexOf(".");
									var innerVariable = leftSide[dotIndex + 2];
									varNameToAssign = varNameToAssign + "_" + innerVariable;
									offsetRegister = registerPool.Pop();
									moonExecCode.Add("\n\t%- Assigning array variable " + varNameToAssign + "[" + litValue + "] -%");
									moonExecCode.Add("\taddi " + offsetRegister + ",R0," + int.Parse(litValue) * 4);
								}
								else
								{
									var litValue = leftSide[arrayIndex + 1];
									var assignVarSymbol = mainMethod.Item2.TableEntries.Find(c => c.Name.Equals(varNameToAssign) && c.Kind.Equals("Variable"));
									var offsetMultiplier = 0;

									if (assignVarSymbol.Type.Contains("integer"))
									{
										offsetMultiplier = 4;
									}
									else if (assignVarSymbol.Type.Contains("float"))
									{
										offsetMultiplier = 8;
									}
									else
									{
										//compute class multiplier
									}

									offsetRegister = registerPool.Pop();
									moonExecCode.Add("\n\t%- Assigning array variable " + varNameToAssign + "[" + litValue + "] -%");
									moonExecCode.Add("\taddi " + offsetRegister + ",R0," + int.Parse(litValue) * offsetMultiplier);
								}

								var register = registerPool.Pop();
								moonExecCode.Add("\tlw " + register + ",R0," + returnOfExpression);
								moonExecCode.Add("\tsw " + varNameToAssign + "(" + offsetRegister + ")," + register);
								registerPool.Push(offsetRegister);
								registerPool.Push(register);
							}
							else
							{
								if (leftSide.Contains("."))
								{
									var dotIndex = leftSide.IndexOf(".");
									var innerVariable = leftSide[dotIndex + 2];

									var register = registerPool.Pop();
									moonExecCode.Add("\n\t%- Storing result of " + returnOfExpression + " in " + varNameToAssign + "_" + innerVariable + " -%");
									moonExecCode.Add("\tlw " + register + "," + returnOfExpression + "(R0)");
									moonExecCode.Add("\tsw " + varNameToAssign + "_" + innerVariable + "(R0)," + register);
									registerPool.Push(register);
								}
								else
								{
									var freeRegister = registerPool.Pop();
									moonExecCode.Add("\n\t%- Storing result of " + returnOfExpression + " in " + varNameToAssign + " -%");
									moonExecCode.Add("\tlw " + freeRegister + "," + returnOfExpression + "(R0)");
									moonExecCode.Add("\tsw " + varNameToAssign + "(R0)," + freeRegister);
									registerPool.Push(freeRegister);
								}
							}
						}
						else
						{
							if (rightSide.Count == 1)
							{
								var literal = rightSide[0];
								varName = "lit_" + literalCount++;

								// store literal
								var freeRegister = registerPool.Pop();
								moonDataCode.Add(varName + " res 4");
								moonExecCode.Add("\n\t%- Storing literal " + varName + " -%");
								moonExecCode.Add("\taddi " + freeRegister + ",R0," + literal);
								moonExecCode.Add("\tsw " + varName + "(R0)," + freeRegister);
								registerPool.Push(freeRegister);
							}
							else
							{
								idIndexRight = rightSide.IndexOf("id");
								varName = rightSide[idIndexRight + 1];
							}

							if (leftSide.Contains("["))
							{
								var arrayIndex = leftSide.IndexOf("[");
								var offsetRegister = "R0";

								if (leftSide[arrayIndex + 1].Equals("id"))
								{
									// array value with index from an id
									// not done yet
								}
								else if (leftSide.Contains("."))
								{
									var litValue = leftSide[arrayIndex + 1];
									var dotIndex = leftSide.IndexOf(".");
									var innerVariable = leftSide[dotIndex + 2];

									if (dotIndex < arrayIndex)
									{
										varNameToAssign = varNameToAssign + "_" + innerVariable;
										offsetRegister = registerPool.Pop();
										moonExecCode.Add("\n\t%- Assigning array variable " + varNameToAssign + "[" + litValue + "] -%");
										moonExecCode.Add("\taddi " + offsetRegister + ",R0," + int.Parse(litValue) * 4);
									}
									else
									{
										varNameToAssign = varNameToAssign + litValue + "_" + innerVariable;
										offsetRegister = registerPool.Pop();
										moonExecCode.Add("\n\t%- Assigning inner variable " + varNameToAssign + " -%");
										offsetRegister = "R0";
									}

								}
								else
								{
									var litValue = leftSide[arrayIndex + 1];
									var assignVarSymbol = mainMethod.Item2.TableEntries.Find(c => c.Name.Equals(varNameToAssign) && c.Kind.Equals("Variable"));
									var offsetMultiplier = 0;

									if (assignVarSymbol.Type.Contains("integer"))
									{
										offsetMultiplier = 4;
									}
									else if (assignVarSymbol.Type.Contains("float"))
									{
										offsetMultiplier = 8;
									}
									else
									{
										//compute class multiplier
									}


									offsetRegister = registerPool.Pop();
									moonExecCode.Add("\n\t%- Assigning array variable " + varNameToAssign + "[" + litValue + "] -%");
									moonExecCode.Add("\taddi " + offsetRegister + ",R0," + int.Parse(litValue) * offsetMultiplier);
								}

								var register = registerPool.Pop();
								moonExecCode.Add("\tlw " + register + "," + varName + "(R0)");
								moonExecCode.Add("\tsw " + varNameToAssign + "(" + offsetRegister + ")," + register);
								registerPool.Push(offsetRegister);
								registerPool.Push(register);
							}
							else
							{
								if (leftSide.Contains("."))
								{
									var dotIndex = leftSide.IndexOf(".");
									var innerVariable = leftSide[dotIndex + 2];

									if (dotIndex + 3 < leftSide.Count() && leftSide[dotIndex + 3].Equals("."))
									{
										var innerInnerVariable = leftSide[dotIndex + 5];
										varNameToAssign = varNameToAssign + "_" + innerVariable + "_" + innerInnerVariable;
									}
									else
									{
										varNameToAssign = varNameToAssign + "_" + innerVariable;
									}
									//lineOfCodeToInterpret[index].Equals("id") && symbolTable.TableEntries.Any(c => c.Name.Equals(lineOfCodeToInterpret[index + 1]) && c.Kind.Equals("Class"))
									var register = registerPool.Pop();
									moonExecCode.Add("\n\t%- Assigning variable " + varNameToAssign + " -%");
									moonExecCode.Add("\tlw " + register + "," + varName + "(R0)");
									moonExecCode.Add("\tsw " + varNameToAssign + "(R0)," + register);
									registerPool.Push(register);

								}
								else
								{
									var register = registerPool.Pop();
									moonExecCode.Add("\n\t%- Assigning variable " + varNameToAssign + " -%");
									moonExecCode.Add("\tlw " + register + "," + varName + "(R0)");
									moonExecCode.Add("\tsw " + varNameToAssign + "(R0)," + register);
									registerPool.Push(register);
								}
							}
						}

					}
					else if (inDeclarations)
					{
						//declaration of variable
						var index = 0;
						var count = lineOfCodeToInterpret.Count();

						while (index != count - 1)
						{
							if (lineOfCodeToInterpret[index].Equals("integer"))
							{
								// array impl
								if (index + 3 < count && lineOfCodeToInterpret[index + 3].Equals("["))
								{
									if (index + 6 < count && lineOfCodeToInterpret[index + 6].Equals("["))
									{
										if (index + 9 < count && lineOfCodeToInterpret[index + 9].Equals("["))
										{
											moonDataCode.Add(lineOfCodeToInterpret[index + 2] + "\tres " + 4 * int.Parse(lineOfCodeToInterpret[index + 4]) * int.Parse(lineOfCodeToInterpret[index + 7]) * int.Parse(lineOfCodeToInterpret[index + 10]));
										}
										else
										{
											moonDataCode.Add(lineOfCodeToInterpret[index + 2] + "\tres " + 4 * int.Parse(lineOfCodeToInterpret[index + 4]) * int.Parse(lineOfCodeToInterpret[index + 7]));
										}
									}
									else
									{
										moonDataCode.Add(lineOfCodeToInterpret[index + 2] + "\tres " + 4 * int.Parse(lineOfCodeToInterpret[index + 4]));
										declaredArraySizes.Add(lineOfCodeToInterpret[index + 2], int.Parse(lineOfCodeToInterpret[index + 4]));
									}
								}
								else
								{
									// reserve space for an int
									moonDataCode.Add(lineOfCodeToInterpret[index + 2] + "\tres 4");
								}
							}
							else if (lineOfCodeToInterpret[index].Equals("float"))
							{
								if (index + 3 < count && lineOfCodeToInterpret[index + 3].Equals("["))
								{
									if (index + 6 < count && lineOfCodeToInterpret[index + 6].Equals("["))
									{
										if (index + 9 < count && lineOfCodeToInterpret[index + 9].Equals("["))
										{
											moonDataCode.Add(lineOfCodeToInterpret[index + 2] + "res " + 8 * int.Parse(lineOfCodeToInterpret[index + 4]) * int.Parse(lineOfCodeToInterpret[index + 7]) * int.Parse(lineOfCodeToInterpret[index + 10]));
										}
										else
										{
											moonDataCode.Add(lineOfCodeToInterpret[index + 2] + " res " + 8 * int.Parse(lineOfCodeToInterpret[index + 4]) * int.Parse(lineOfCodeToInterpret[index + 7]));
										}
									}
									else
									{
										moonDataCode.Add(lineOfCodeToInterpret[index + 2] + " res " + 8 * int.Parse(lineOfCodeToInterpret[index + 4]));

										declaredArraySizes.Add(lineOfCodeToInterpret[index + 2], int.Parse(lineOfCodeToInterpret[index + 4]));
									}
								}
								else
								{
									// reserve space for a float
									moonDataCode.Add(lineOfCodeToInterpret[index + 2] + " res 8");
								}
							}
							else
							{
								// check for class variable declarations
								if (lineOfCodeToInterpret[index].Equals("id") && symbolTable.TableEntries.Any(c => c.Name.Equals(lineOfCodeToInterpret[index + 1]) && c.Kind.Equals("Class")))
								{
									var classOffset = symbolTable.TableEntries.Find(c => c.Name.Equals(lineOfCodeToInterpret[index + 1]) && c.Kind.Equals("Class"))?.Link.TableOffset;

									if (index + 4 < count && lineOfCodeToInterpret[index + 4].Equals("["))
									{
										if (index + 7 < count && lineOfCodeToInterpret[index + 7].Equals("["))
										{
											if (index + 10 < count && lineOfCodeToInterpret[index + 10].Equals("["))
											{
												moonDataCode.Add(lineOfCodeToInterpret[index + 3] + " res " + classOffset * int.Parse(lineOfCodeToInterpret[index + 5]) * int.Parse(lineOfCodeToInterpret[index + 8]) * int.Parse(lineOfCodeToInterpret[index + 11]));
											}
											else
											{
												moonDataCode.Add(lineOfCodeToInterpret[index + 3] + " res " + classOffset * int.Parse(lineOfCodeToInterpret[index + 5]) * int.Parse(lineOfCodeToInterpret[index + 8]));
											}
										}
										else
										{
											moonDataCode.Add(lineOfCodeToInterpret[index + 3] + " res " + classOffset * int.Parse(lineOfCodeToInterpret[index + 5]));
											declaredArraySizes.Add(lineOfCodeToInterpret[index + 3], int.Parse(lineOfCodeToInterpret[index + 5]));
										}
									}
									else
									{
										// reserve space for a class
										moonDataCode.Add(lineOfCodeToInterpret[index + 3] + " res " + classOffset);

										ReserveClassVariables(lineOfCodeToInterpret[index + 1], lineOfCodeToInterpret[index + 3], symbolTable);
									}
								}
							}

							index++;
						}
					}

					lineOfCodeToInterpret = new List<string>();
				}
				else
				{
					lineOfCodeToInterpret.Add(items);
				}
			}

			moonCode = moonFunctionCode.Concat(moonExecCode.Append("\n\thlt\n").Concat(moonDataCode)).ToList();
			return moonCode;
		}

		public static void GenerateFunctionCode((List<string>, SymbolTable) functionInfo, SymbolTable symbolTable)
		{
			var functionName = functionInfo.Item2.Name;
			var inDeclarations = false;
			var inStatements = false;
			var inLoop = false;
			var inIf = false;
			var statementStack = new Stack<string>();
			var startingIfOrLoop = false;
			var gotParams = false;
			var funcParamCounter = 0;
			var ifCountStack = new Stack<int>();
			var loopCountStack = new Stack<int>();

			moonFunctionCode.Add("\n\t%- Function definition for " + functionName + " -%");
			moonFunctionCode.Add(functionName);

			var lineOfCodeToInterpret = new List<string>();
			foreach (var items in functionInfo.Item1)
			{
				if (items.Equals("local"))
				{
					inDeclarations = true;
					items.Remove(items.IndexOf("local"));
				}
				else if (items.Equals("do"))
				{
					inDeclarations = false;
					inStatements = true;
					statementStack.Push("do");
				}
				else if (inStatements && items.Equals("if"))
				{
					// store what you are in 
					statementStack.Push("if");
					inIf = true;
					startingIfOrLoop = true;
				}
				else if (inStatements && items.Equals("while"))
				{
					// store what you are in 
					statementStack.Push("while");
					inLoop = true;
					startingIfOrLoop = true;
				}
				else if (inStatements && items.Equals("end"))
				{
					// pop where you are 
					var popped = statementStack.Pop();
					inIf = statementStack.Contains("if");
					inLoop = statementStack.Contains("while");
				}

				//Console.WriteLine(items);
				if (items.Equals(";") || (items.Equals(")") && inLoop && startingIfOrLoop) || (items.Equals(")") && inIf && startingIfOrLoop) || items.Equals(":"))
				{
					// interpret the line of code
					startingIfOrLoop = false;

					if (lineOfCodeToInterpret.Contains("else") && inStatements)
					{
						var currentIfCount = ifCountStack.Peek();
						// write out else beginning 
						var endElseNumber = functionName + "_else" + currentIfCount;
						var endIfNumber = functionName + "_endif" + currentIfCount;

						moonFunctionCode.Add("\n\t%- Start branch for else statement -%");
						moonFunctionCode.Add("\tj " + endIfNumber);
						moonFunctionCode.Add(endElseNumber);
					}

					if ((lineOfCodeToInterpret.Count() == 2 || lineOfCodeToInterpret.Count() == 1 || lineOfCodeToInterpret.Count() == 0) && items.Equals(";") && (inLoop || inIf))
					{
						var ending = statementStack.Pop();
						inIf = statementStack.Contains("if");
						inLoop = statementStack.Contains("while");

						// write out while loop ending 
						if (ending.Equals("while"))
						{
							var currentloopCount = loopCountStack.Peek();
							var startWhileNumber = functionName + "_gowhile" + currentloopCount;
							var endWhileNumber = functionName + "_endwhile" + currentloopCount;
							moonFunctionCode.Add("\tj " + startWhileNumber);
							moonFunctionCode.Add(endWhileNumber);

							loopCountStack.Pop();
						}
						else if (ending.Equals("if"))
						{
							var currentIfCount = ifCountStack.Peek();
							// write out else beginning 
							var endIfNumber = functionName + "_endif" + currentIfCount;
							moonFunctionCode.Add("\n\t%- End if statement -%");
							moonFunctionCode.Add(endIfNumber);

							ifCountStack.Pop();
						}
					}

					if (!gotParams)
					{
						// write out while loop beginning 
						inDeclarations = true;
						gotParams = true;

						var parenthesisStartIndex = lineOfCodeToInterpret.IndexOf("(");
						var parameters = lineOfCodeToInterpret.GetRange(parenthesisStartIndex, lineOfCodeToInterpret.Count() - parenthesisStartIndex);

						for (var i = 0; i < parameters.Count(); i++)
						{
							if (parameters[i].Equals("id") && parameters[i - 1].Equals("integer"))
							{
								var varParamName = functionName + "_" + parameters[i + 1];
								var paramName = functionName + "_param" + ++funcParamCounter;

								if (!calledFunctionsParams.Contains(paramName))
								{
									if (parameters[i+2].Equals("["))
									{
									}
									else
									{
										moonDataCode.Add(paramName + " res 4");
										calledFunctionsParams.Add(paramName);
									}
								}

								var register = registerPool.Pop();
								moonFunctionCode.Add("\n\t%- Assigning inner function parameter " + varParamName + " -%");
								moonFunctionCode.Add("\tlw " + register + "," + paramName + "(R0)");
								moonFunctionCode.Add("\tsw " + varParamName + "(R0)," + register);
								registerPool.Push(register);
							}
						}

					}

					if (lineOfCodeToInterpret.Contains("while") && inStatements && inLoop)
					{
						loopCountStack.Push(++loopCountFunc);
						// write out while loop beginning 
						var startWhileNumber = functionName + "_gowhile" + loopCountFunc;
						var endWhileNumber = functionName + "_endwhile" + loopCountFunc;
						moonFunctionCode.Add(startWhileNumber);
						var expressionResult = ComputeExpression(lineOfCodeToInterpret, true, functionName, functionInfo.Item2);

						var register = registerPool.Pop();
						moonFunctionCode.Add("\n\t%- Branch to end while loop -%");
						moonFunctionCode.Add("\tlw " + register + "," + expressionResult + "(R0)");
						moonFunctionCode.Add("\tbz " + register + "," + endWhileNumber);
						registerPool.Push(register);

					}
					else if (lineOfCodeToInterpret.Contains("if") && inStatements && inIf)
					{
						ifCountStack.Push(++ifCountFunc);
						// write out while loop beginning 
						var startElseNumber = functionName + "_else" + ifCountFunc;
						var expressionResult = ComputeExpression(lineOfCodeToInterpret, isaFunc: true, funcName: functionName, symbolTable: functionInfo.Item2);

						var register = registerPool.Pop();
						moonFunctionCode.Add("\n\t%- Start branch for if statement -%");
						moonFunctionCode.Add("\tlw " + register + "," + expressionResult + "(R0)");
						moonFunctionCode.Add("\tbz " + register + "," + startElseNumber);
						registerPool.Push(register);
					}
					else if (lineOfCodeToInterpret.Contains("return") && inStatements)
					{
						//assignment of variable
						var ids = lineOfCodeToInterpret.Where(c => c.Equals("id"));
						var containsExpression = ContainsExpression(lineOfCodeToInterpret);

						if (ids.Count() == 1 && !containsExpression)
						{
							var parenthesisIndex = lineOfCodeToInterpret.IndexOf("(");
							int theInt;
							// not sure how to return float
							if (int.TryParse(lineOfCodeToInterpret[parenthesisIndex + 1], out theInt))
							{
								var returnName = functionName + "_return";

								var register = registerPool.Pop();
								moonFunctionCode.Add("\n\t%- Returning int " + theInt + " -%");
								moonFunctionCode.Add("\taddi " + register + ",R0," + theInt);
								moonFunctionCode.Add("\tsw " + returnName + "(R0)," + register);
								registerPool.Push(register);
							}
							else if (ids.Count() == 1 && !containsExpression)
							{
								var idIndex = lineOfCodeToInterpret.IndexOf("id");
								var value = lineOfCodeToInterpret[idIndex + 1];

								if ((idIndex + 2) < lineOfCodeToInterpret.Count() && lineOfCodeToInterpret[idIndex + 2].Equals("["))
								{
									var assignVarSymbol = functionInfo.Item2.TableEntries.Find(c => c.Name.Equals(value) && c.Kind.Equals("Variable"));
									var offsetMultiplier = 0;

									if (assignVarSymbol.Type.Contains("integer"))
									{
										offsetMultiplier = 4;
									}
									else if (assignVarSymbol.Type.Contains("float"))
									{
										offsetMultiplier = 8;
									}
									else
									{
										//compute class multiplier
									}

									var returnName = functionName + "_return";

									var register = registerPool.Pop();
									var register2 = registerPool.Pop();
									moonFunctionCode.Add("\n\t%- Returning value of " + functionName + "_" + value + " at " + lineOfCodeToInterpret[idIndex + 3] + " -%");
									moonFunctionCode.Add("\taddi " + register + ",R0," + int.Parse(lineOfCodeToInterpret[idIndex + 3]) * offsetMultiplier);
									moonFunctionCode.Add("\tlw " + register2 + "," + functionName + "_" + value + "(" + register + ")");
									moonFunctionCode.Add("\tsw " + returnName + "(R0)," + register2);
									registerPool.Push(register2);
									registerPool.Push(register);
								}
								else
								{
									var returnName = functionName + "_return";

									var valueOfId = lineOfCodeToInterpret[idIndex + 1];
									var register = registerPool.Pop();
									moonFunctionCode.Add("\n\t%- Returning value of " + functionName + "_" + valueOfId + " -%");
									moonFunctionCode.Add("\tlw " + register + "," + functionName + "_" + valueOfId + "(R0)");
									moonFunctionCode.Add("\tsw " + returnName + "(R0)," + register);
									registerPool.Push(register);
								}
							}
							else if (ids.Count() == 2 && lineOfCodeToInterpret.Contains("[") && !containsExpression)
							{
								var idIndex = lineOfCodeToInterpret.IndexOf("id");
								if (lineOfCodeToInterpret[idIndex + 2].Equals("["))
								{
									var varValue = lineOfCodeToInterpret[idIndex + 1];
									var offsetVar = functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[idIndex + 4];

									moonFunctionCode.Add("\n\t%- Printing value of " + functionInfo.Item2.Name + "_" + varValue + " -%");
									moonFunctionCode.Add("\tlw R3," + offsetVar + "(R0)");
									moonFunctionCode.Add("\tlw R2," + functionInfo.Item2.Name + "_" + varValue + "(R3)");
									moonFunctionCode.Add("\tadd R1,R0,R2");
									moonFunctionCode.Add("\tjl R15,putint");
								}
								else
								{
									var value = lineOfCodeToInterpret[idIndex + 1];
									moonFunctionCode.Add("\n\t%- Printing value of " + functionInfo.Item2.Name + "_" + value + " -%");
									moonFunctionCode.Add("\taddi R1,R0," + functionInfo.Item2.Name + "_" + value);
									moonFunctionCode.Add("\tjl R15,putint");
								}
							}
							else if (containsExpression)
							{ }
							else
							{
								// class members
							}
						}
					}
					else if (lineOfCodeToInterpret.Contains("write") && inStatements)
					{
						//assignment of variable
						var ids = lineOfCodeToInterpret.Where(c => c.Equals("id"));
						var containsExpression = ContainsExpression(lineOfCodeToInterpret);

						if (ids.Count() == 0 && !containsExpression)
						{
							var parenthesisIndex = lineOfCodeToInterpret.IndexOf("(");
							int theInt;
							// not sure how to print float
							if (int.TryParse(lineOfCodeToInterpret[parenthesisIndex + 1], out theInt))
							{
								var tempStorage = "temp_" + ++tempCount;

								moonDataCode.Add(tempStorage + " res 4");
								moonFunctionCode.Add("\n\t%- Save R15 contents -%");
								moonFunctionCode.Add("\tadd R1,R0,R15");
								moonFunctionCode.Add("\tsw " + tempStorage + "(R0),R1");

								moonFunctionCode.Add("\n\t%- Printing int " + theInt + " -%");
								moonFunctionCode.Add("\taddi R1,R0," + theInt);
								moonFunctionCode.Add("\tjl R15,putint");
								moonFunctionCode.Add("\n\t%- Printing a space -%");
								moonFunctionCode.Add("\tputc R0");

								moonFunctionCode.Add("\n\t%- Unload R15 contents -%");
								moonFunctionCode.Add("\tlw R1," + tempStorage + "(R0)");
								moonFunctionCode.Add("\tadd R15,R0,R1");
							}
						}
						else if (ids.Count() == 1 && !containsExpression)
						{
							var idIndex = lineOfCodeToInterpret.IndexOf("id");
							var value = lineOfCodeToInterpret[idIndex + 1];

							if ((idIndex + 2) < lineOfCodeToInterpret.Count() && lineOfCodeToInterpret[idIndex + 2].Equals("["))
							{
								var assignVarSymbol = functionInfo.Item2.TableEntries.Find(c => c.Name.Equals(value) && c.Kind.Equals("Variable"));
								var offsetMultiplier = 0;

								if (assignVarSymbol.Type.Contains("integer"))
								{
									offsetMultiplier = 4;
								}
								else if (assignVarSymbol.Type.Contains("float"))
								{
									offsetMultiplier = 8;
								}
								else
								{
									//compute class multiplier
								}

								var tempStorage = "temp_" + ++tempCount;

								moonDataCode.Add(tempStorage + " res 4");
								moonFunctionCode.Add("\n\t%- Save R15 contents -%");
								moonFunctionCode.Add("\tadd R1,R0,R15");
								moonFunctionCode.Add("\tsw " + tempStorage + "(R0),R1");

								moonFunctionCode.Add("\n\t%- Printing value of " + functionInfo.Item2.Name + "_" + value + " at " + lineOfCodeToInterpret[idIndex + 3] + " -%");
								moonFunctionCode.Add("\taddi R3,R0," + int.Parse(lineOfCodeToInterpret[idIndex + 3]) * offsetMultiplier);
								moonFunctionCode.Add("\tlw R2," + functionInfo.Item2.Name + "_" + value + "(R3)");
								moonFunctionCode.Add("\tadd R1,R0,R2");
								moonFunctionCode.Add("\tjl R15,putint");
								moonFunctionCode.Add("\n\t%- Printing a space -%");
								moonFunctionCode.Add("\tputc R0");

								moonFunctionCode.Add("\n\t%- Unload R15 contents -%");
								moonFunctionCode.Add("\tlw R1," + tempStorage + "(R0)");
								moonFunctionCode.Add("\tadd R15,R0,R1");
							}
							else
							{
								var tempStorage = "temp_" + ++tempCount;

								moonDataCode.Add(tempStorage + " res 4");
								moonFunctionCode.Add("\n\t%- Save R15 contents -%");
								moonFunctionCode.Add("\tadd R1,R0,R15");
								moonFunctionCode.Add("\tsw " + tempStorage + "(R0),R1");

								var valueOfId = lineOfCodeToInterpret[idIndex + 1];
								moonFunctionCode.Add("\n\t%- Printing value of " + functionInfo.Item2.Name + "_" + valueOfId + " -%");
								moonFunctionCode.Add("\tlw R2," + functionInfo.Item2.Name + "_" + valueOfId + "(R0)");
								moonFunctionCode.Add("\tadd R1,R0,R2");
								moonFunctionCode.Add("\tjl R15,putint");
								moonFunctionCode.Add("\n\t%- Printing a space -%");
								moonFunctionCode.Add("\tputc R0");

								moonFunctionCode.Add("\n\t%- Unload R15 contents -%");
								moonFunctionCode.Add("\tlw R1," + tempStorage + "(R0)");
								moonFunctionCode.Add("\tadd R15,R0,R1");
							}
						}
						else if (ids.Count() == 2 && lineOfCodeToInterpret.Contains("[") && !containsExpression)
						{
							var idIndex = lineOfCodeToInterpret.IndexOf("id");
							if (lineOfCodeToInterpret[idIndex + 2].Equals("["))
							{
								var varValue = lineOfCodeToInterpret[idIndex + 1];
								var offsetVar = functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[idIndex + 4];

								moonFunctionCode.Add("\n\t%- Printing value of " + functionInfo.Item2.Name + "_" + varValue + " -%");
								moonFunctionCode.Add("\tlw R3," + offsetVar + "(R0)");
								moonFunctionCode.Add("\tlw R2," + functionInfo.Item2.Name + "_" + varValue + "(R3)");
								moonFunctionCode.Add("\tadd R1,R0,R2");
								moonFunctionCode.Add("\tjl R15,putint");

								moonFunctionCode.Add("\n\t%- Printing a space -%");
								moonFunctionCode.Add("\tputc R0");
							}
							else
							{
								var value = lineOfCodeToInterpret[idIndex + 1];
								moonFunctionCode.Add("\n\t%- Printing value of " + functionInfo.Item2.Name + "_" + value + " -%");
								moonFunctionCode.Add("\taddi R1,R0," + functionInfo.Item2.Name + "_" + value);
								moonFunctionCode.Add("\tjl R15,putint");

								moonFunctionCode.Add("\n\t%- Printing a space -%");
								moonFunctionCode.Add("\tputc R0");
							}
						}
						else if (containsExpression)
						{ }
						else
						{
							// class members
						}
					}
					else if (lineOfCodeToInterpret.Contains("read") && inStatements)
					{
						//assignment of variable
						var idIndexes = lineOfCodeToInterpret.Where(c => c.Equals("id"));

						foreach (var idIndex in idIndexes)
						{

						}


					}
					else if (lineOfCodeToInterpret.Contains("=") && inStatements)
					{
						//assignment of variable
						var countOfItems = lineOfCodeToInterpret.Count();
						var indexOfEquals = lineOfCodeToInterpret.IndexOf("=");
						var leftSide = lineOfCodeToInterpret.GetRange(0, indexOfEquals);
						var rightSide = lineOfCodeToInterpret.GetRange(indexOfEquals + 1, countOfItems - leftSide.Count() - 1);


						var idIndexLeft = leftSide.IndexOf("id");
						var varNameToAssign = leftSide[idIndexLeft + 1];
						var idIndexRight = 0;
						var varName = "";

						if (ContainsExpression(rightSide))
						{
							var returnOfExpression = ComputeExpression(rightSide, true, functionName, symbolTable: functionInfo.Item2);

							// todo
							if (leftSide.Contains("["))
							{
								var arrayIndex = leftSide.IndexOf("[");
								var offsetRegister = "R0";

								if (leftSide[arrayIndex + 1].Equals("id"))
								{
									// array value with index from an id
									// not done yet
								}
								else
								{
									var litValue = leftSide[arrayIndex + 1];
									var assignVarSymbol = functionInfo.Item2.TableEntries.Find(c => c.Name.Equals(varNameToAssign) && c.Kind.Equals("Variable"));
									var offsetMultiplier = 0;

									if (assignVarSymbol.Type.Contains("integer"))
									{
										offsetMultiplier = 4;
									}
									else if (assignVarSymbol.Type.Contains("float"))
									{
										offsetMultiplier = 8;
									}
									else
									{
										//compute class multiplier
									}

									offsetRegister = registerPool.Pop();
									moonFunctionCode.Add("\n\t%- Assigning array variable " + functionName + "_" + varNameToAssign + "[" + litValue + "] -%");
									moonFunctionCode.Add("\taddi " + offsetRegister + ",R0," + int.Parse(litValue) * offsetMultiplier);
								}

								var register = registerPool.Pop();
								moonFunctionCode.Add("\tlw " + register + "," + returnOfExpression + "(R0)");
								moonFunctionCode.Add("\tsw " + functionName + "_" + varNameToAssign + "(" + offsetRegister + ")," + register);
								registerPool.Push(offsetRegister);
								registerPool.Push(register);
							}
							else
							{
								var freeRegister = registerPool.Pop();
								moonFunctionCode.Add("\n\t%- Storing result of " + returnOfExpression + " in " + functionName + "_" + varNameToAssign + " -%");
								moonFunctionCode.Add("\tlw " + freeRegister + "," + returnOfExpression + "(R0)");
								moonFunctionCode.Add("\tsw " + functionName + "_" + varNameToAssign + "(R0)," + freeRegister);
								registerPool.Push(freeRegister);
							}

						}
						else
						{
							if (rightSide.Count == 1)
							{
								var literal = rightSide[0];
								varName = "lit_" + literalCount++;

								// store literal
								var freeRegister = registerPool.Pop();
								moonDataCode.Add(varName + " res 4");
								moonFunctionCode.Add("\n\t%- Storing literal " + varName + " -%");
								moonFunctionCode.Add("\taddi " + freeRegister + ",R0," + literal);
								moonFunctionCode.Add("\tsw " + varName + "(R0)," + freeRegister);
								registerPool.Push(freeRegister);
							}
							else
							{
								idIndexRight = rightSide.IndexOf("id");
								varName = rightSide[idIndexRight + 1];
							}

							if (leftSide.Contains("["))
							{
								var arrayIndex = leftSide.IndexOf("[");
								var offsetRegister = "R0";

								if (leftSide[arrayIndex + 1].Equals("id"))
								{
									// array value with index from an id
									// not done yet
								}
								else
								{
									var litValue = leftSide[arrayIndex + 1];
									var assignVarSymbol = functionInfo.Item2.TableEntries.Find(c => c.Name.Equals(varNameToAssign) && c.Kind.Equals("Variable"));
									var offsetMultiplier = 0;

									if (assignVarSymbol.Type.Contains("integer"))
									{
										offsetMultiplier = 4;
									}
									else if (assignVarSymbol.Type.Contains("float"))
									{
										offsetMultiplier = 8;
									}
									else
									{
										//compute class multiplier
									}


									offsetRegister = registerPool.Pop();
									moonFunctionCode.Add("\n\t%- Assigning array variable " + functionName + "_" + varNameToAssign + "[" + litValue + "] -%");
									moonFunctionCode.Add("\taddi " + offsetRegister + ",R0," + int.Parse(litValue) * offsetMultiplier);
								}

								var register = registerPool.Pop();
								moonFunctionCode.Add("\tlw " + register + "," + functionName + "_" + varName + "(R0)");
								moonFunctionCode.Add("\tsw " + functionName + "_" + varNameToAssign + "(" + offsetRegister + ")," + register);
								registerPool.Push(offsetRegister);
								registerPool.Push(register);
							}
							else
							{
								var register = registerPool.Pop();
								varName = !varName.Contains("lit_") ? functionName + "_" + varName : varName;
								moonFunctionCode.Add("\n\t%- Assigning variable " + functionName + "_" + varNameToAssign + " -%");
								moonFunctionCode.Add("\tlw " + register + "," + varName + "(R0)");
								moonFunctionCode.Add("\tsw " + functionName + "_" + varNameToAssign + "(R0)," + register);
								registerPool.Push(register);
							}
						}

					}
					else if (lineOfCodeToInterpret.Contains("(") && inStatements)
					{
						//assignment of variable
						var idIndexes = lineOfCodeToInterpret.Where(c => c.Equals("id"));

						foreach (var idIndex in idIndexes)
						{

						}


					}
					else if (inDeclarations)
					{
						//declaration of variable
						var index = 0;
						var count = lineOfCodeToInterpret.Count();

						while (index != count - 1)
						{
							if (lineOfCodeToInterpret[index].Equals("integer"))
							{
								// array impl
								if (index + 3 < count && lineOfCodeToInterpret[index + 3].Equals("["))
								{
									if (index + 6 < count && lineOfCodeToInterpret[index + 6].Equals("["))
									{
										if (index + 9 < count && lineOfCodeToInterpret[index + 9].Equals("["))
										{
											moonDataCode.Add(functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[index + 2] + "\tres " + 4 * int.Parse(lineOfCodeToInterpret[index + 4]) * int.Parse(lineOfCodeToInterpret[index + 7]) * int.Parse(lineOfCodeToInterpret[index + 10]));
										}
										else
										{
											moonDataCode.Add(functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[index + 2] + "\tres " + 4 * int.Parse(lineOfCodeToInterpret[index + 4]) * int.Parse(lineOfCodeToInterpret[index + 7]));
										}
									}
									else
									{
										if (int.TryParse(lineOfCodeToInterpret[index + 4], out _))
										{
											moonDataCode.Add(functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[index + 2] + "\tres " + 4 * int.Parse(lineOfCodeToInterpret[index + 4]));
										}
										else
										{
											moonDataCode.Add(functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[index + 2] + "\tres " + 4);
										}
									}
								}
								else
								{
									// reserve space for an int
									moonDataCode.Add(functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[index + 2] + "\tres 4");
								}
							}
							else if (lineOfCodeToInterpret[index].Equals("float"))
							{
								if (index + 3 < count && lineOfCodeToInterpret[index + 3].Equals("["))
								{
									if (index + 6 < count && lineOfCodeToInterpret[index + 6].Equals("["))
									{
										if (index + 9 < count && lineOfCodeToInterpret[index + 9].Equals("["))
										{
											moonDataCode.Add(functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[index + 2] + "res " + 8 * int.Parse(lineOfCodeToInterpret[index + 4]) * int.Parse(lineOfCodeToInterpret[index + 7]) * int.Parse(lineOfCodeToInterpret[index + 10]));
										}
										else
										{
											moonDataCode.Add(functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[index + 2] + " res " + 8 * int.Parse(lineOfCodeToInterpret[index + 4]) * int.Parse(lineOfCodeToInterpret[index + 7]));
										}
									}
									else
									{
										moonDataCode.Add(functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[index + 2] + " res " + 8 * int.Parse(lineOfCodeToInterpret[index + 4]));
									}
								}
								else
								{
									// reserve space for a float
									moonDataCode.Add(functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[index + 2] + " res 8");
								}
							}
							else
							{
								// check for class variable declarations
								if (lineOfCodeToInterpret[index].Equals("id") && symbolTable.TableEntries.Any(c => c.Name.Equals(lineOfCodeToInterpret[index + 1]) && c.Kind.Equals("class")))
								{
									var classOffset = symbolTable.TableEntries.Find(c => c.Name.Equals(lineOfCodeToInterpret[index + 1]) && c.Kind.Equals("class"))?.Offset;

									if (index + 4 < count && lineOfCodeToInterpret[index + 4].Equals("["))
									{
										if (index + 7 < count && lineOfCodeToInterpret[index + 7].Equals("["))
										{
											if (index + 10 < count && lineOfCodeToInterpret[index + 10].Equals("["))
											{
												moonDataCode.Add(functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[index + 3] + " res " + classOffset * int.Parse(lineOfCodeToInterpret[index + 5]) * int.Parse(lineOfCodeToInterpret[index + 8]) * int.Parse(lineOfCodeToInterpret[index + 11]));
											}
											else
											{
												moonDataCode.Add(functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[index + 3] + " res " + classOffset * int.Parse(lineOfCodeToInterpret[index + 5]) * int.Parse(lineOfCodeToInterpret[index + 8]));
											}
										}
										else
										{
											moonDataCode.Add(functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[index + 3] + " res " + classOffset * int.Parse(lineOfCodeToInterpret[index + 5]));
										}
									}
									else
									{
										// reserve space for a class variable

										moonDataCode.Add(functionInfo.Item2.Name + "_" + lineOfCodeToInterpret[index + 3] + " res " + classOffset);
									}
								}
							}

							index++;
						}
					}

					lineOfCodeToInterpret = new List<string>();
				}
				else
				{
					lineOfCodeToInterpret.Add(items);
				}
			}

			moonFunctionCode.Add("\n\tjr R15"); ;
		}

		public static void ReserveClassVariables(string className, string classVarName, SymbolTable symbolTable)
		{
			var classTable = symbolTable.TableEntries.Find(c => c.Name.Equals(className) && c.Kind.Equals("Class")).Link;
			var variables = classTable.TableEntries.Where(c => c.Kind.Equals("Variable"));

			foreach (var classVar in variables)
			{
				if (classVar.Type.Contains("integer"))
				{
					if (classVar.Type.Contains("["))
					{
						moonDataCode.Add(classVarName + "_" + classVar.Name + "\tres " + classVar.Offset);
					}
					else
					{
						moonDataCode.Add(classVarName + "_" + classVar.Name + "\tres 4");
					}
				}
				else if (classVar.Type.Contains("float"))
				{
					if (classVar.Type.Contains("["))
					{
						moonDataCode.Add(classVarName + "_" + classVar.Name + "\tres " + classVar.Offset);
					}
					else
					{
						moonDataCode.Add(classVarName + "_" + classVar.Name + "\tres 8");
					}
				}
				else
				{
					//classes in classes
					if (classVar.Type.Contains("["))
					{
						var theClass = symbolTable.TableEntries.Find(c => c.Name.Equals(classVar.Type))?.Link;
						var classOffset = theClass.TableOffset;

						moonDataCode.Add(classVarName + "_" + classVar.Name + "\tres " + classVar.Offset);
					}
					else
					{
						var theClass = symbolTable.TableEntries.Find(c => c.Name.Equals(classVar.Type))?.Link;
						var classOffset = theClass.TableOffset;
						moonDataCode.Add(classVarName + "_" + classVar.Name + "\tres " + classOffset);

						ReserveClassVariables(theClass.Name, classVarName + "_" + classVar.Name, symbolTable);
					}
				}
			}
		}

		public static bool ContainsExpression(List<string> items)
		{
			return items.Contains("+") || items.Contains("-") || items.Contains("*") || items.Contains("/");
		}

		public static string ExecuteCalculationCode(string operation, string firstOperand, string firstOperandType, string lastOperand, string lastOperandType, bool isaFunc = false, string funcName = "")
		{
			var listToOutput = isaFunc ? moonFunctionCode : moonExecCode;
			var varName = "temp_" + ++tempCount;
			moonDataCode.Add(varName + " res 4");

			if (firstOperandType.Equals("literal") && lastOperandType.Equals("literal"))
			{
				var register1 = registerPool.Pop();
				var register2 = registerPool.Pop();
				listToOutput.Add("\n\t%- Computation " + firstOperand + " " + operation + " " + lastOperand + " -%");
				listToOutput.Add("\taddi " + register1 + ",R0," + firstOperand);
				listToOutput.Add("\t" + operation + "i " + register2 + "," + register1 + "," + lastOperand);
				listToOutput.Add("\tsw " + varName + "(R0)," + register2);
				registerPool.Push(register1);
				registerPool.Push(register2);
			}
			else if (firstOperandType.Equals("variable") && lastOperandType.Equals("literal"))
			{
				var register1 = registerPool.Pop();
				var register2 = registerPool.Pop();
				listToOutput.Add("\n\t%- Computation " + firstOperand + " " + operation + " " + lastOperand + " -%");
				listToOutput.Add("\tlw " + register1 + "," + firstOperand + "(R0)");
				listToOutput.Add("\t" + operation + "i " + register2 + "," + register1 + "," + lastOperand);
				listToOutput.Add("\tsw " + varName + "(R0)," + register2);
				registerPool.Push(register1);
				registerPool.Push(register2);
			}
			else if (firstOperandType.Equals("literal") && lastOperandType.Equals("variable"))
			{
				var register1 = registerPool.Pop();
				var register2 = registerPool.Pop();
				moonExecCode.Add("\n\t%- Computation " + firstOperand + " " + operation + " " + lastOperand + " -%");
				moonExecCode.Add("\tlw " + register1 + "," + lastOperand + "(R0)");
				moonExecCode.Add("\t" + operation + "i " + register2 + "," + register1 + "," + firstOperand);
				moonExecCode.Add("\tsw " + varName + "(R0)," + register2);
				registerPool.Push(register1);
				registerPool.Push(register2);
			}
			else if (firstOperandType.Equals("variable") && lastOperandType.Equals("variable"))
			{
				var register1 = registerPool.Pop();
				var register2 = registerPool.Pop();
				var register3 = registerPool.Pop();
				listToOutput.Add("\n\t%- Computation " + firstOperand + " " + operation + " " + lastOperand + " -%");
				listToOutput.Add("\tlw " + register1 + "," + firstOperand + "(R0)");
				listToOutput.Add("\tlw " + register2 + "," + lastOperand + "(R0)");
				listToOutput.Add("\t" + operation + " " + register3 + "," + register1 + "," + register2);
				listToOutput.Add("\tsw " + varName + "(R0)," + register3);
				registerPool.Push(register1);
				registerPool.Push(register2);
				registerPool.Push(register3);
			}

			return varName;
		}

		public static string ComputeExpression(List<string> items, bool isaFunc = false, string funcName = "", SymbolTable symbolTable = null)
		{
			var lastOperand = "";
			var lastOperandType = "";
			var firstOperand = "";
			var firstOperandType = "";

			var count = items.Count();

			if (items[count - 1].Equals(";") || items[count - 1].Equals(")"))
			{
				items.RemoveAt(count - 1);
			}

			if (int.TryParse(items[count - 1], out _))
			{
				lastOperand = items[count - 1];
				lastOperandType = "literal";
				items.RemoveAt(count - 1);
			}
			else if (float.TryParse(items[count - 1], out _))
			{
				//do float ops
			}
			else if (items[count - 1].Equals("]"))
			{
				//do array ops
				var offset = 0;
				if (int.TryParse(items[count - 2], out offset) && items[count - 3].Equals("["))
				{
					var varName = "temp_" + ++tempCount;
					moonDataCode.Add(varName + " res 4");

					var inputName = items[count - 4];
					var listToOutput = isaFunc ? moonFunctionCode : moonExecCode;
					var symbolValue = symbolTable.TableEntries.Find(c => c.Name.Equals(items[count - 4]) && c.Type.Contains("["));
					var offsetCount = 0;

					if (symbolValue.Type.Contains("integer"))
					{
						offsetCount = 4 * offset;
					}
					else if (symbolValue.Type.Contains("float"))
					{
						// dont use this yet
						offsetCount = 8 * offset;
					}
					else
					{
						//some class offset
					}

					//set varname
					var register1 = registerPool.Pop();
					var register2 = registerPool.Pop();
					listToOutput.Add("\n\t%- Setting " + inputName + "[" + offset + "] to temp variable " + varName + " -%");
					listToOutput.Add("\taddi " + register1 + ",R0," + offsetCount);
					listToOutput.Add("\tlw " + register2 + "," + inputName + "(" + register1 + ")");
					listToOutput.Add("\tsw " + varName + "(R0)," + register2);
					registerPool.Push(register2);
					registerPool.Push(register1);

					lastOperand = varName;
					lastOperandType = "variable";
					items.RemoveAt(count - 1);
					items.RemoveAt(count - 2);
					items.RemoveAt(count - 3);
					items.RemoveAt(count - 4);
					items.RemoveAt(count - 5);
				}
				else if (items[count - 4].Equals("["))
				{
					var listToOutput = isaFunc ? moonFunctionCode : moonExecCode;

					var varName = "temp_" + ++tempCount;
					moonDataCode.Add(varName + " res 4");

					var offsetTempVarName1 = "temp_" + ++tempCount;
					moonDataCode.Add(offsetTempVarName1 + " res 4");
					var offsetTempVarName2 = "temp_" + ++tempCount;
					moonDataCode.Add(offsetTempVarName2 + " res 4");

					var offsetVar = isaFunc && !items[count - 2].Contains("temp_") ? funcName + "_" + items[count - 2] : items[count - 2];

					var register = registerPool.Pop();
					listToOutput.Add("\n\t%- Retriving value of " + offsetVar + " -%");
					listToOutput.Add("\tlw " + register + "," + offsetVar + "(R0)");
					listToOutput.Add("\tsw " + offsetTempVarName1 + "(R0)," + register);
					registerPool.Push(register);

					var register1 = registerPool.Pop();
					var register2 = registerPool.Pop();
					listToOutput.Add("\n\t%- Mult value of  " + offsetTempVarName1 + " by int size -%");
					listToOutput.Add("\tlw " + register1 + "," + offsetTempVarName1 + "(R0)");
					listToOutput.Add("\tmuli " + register2 + "," + register1 + ",4");
					listToOutput.Add("\tsw " + offsetTempVarName2 + "(R0)," + register2);
					registerPool.Push(register2);
					registerPool.Push(register1);

					//set varname
					var register3 = registerPool.Pop();
					var register4 = registerPool.Pop();
					listToOutput.Add("\n\t%- Setting " + items[count - 5] + "[" + offset + "] to temp variable " + varName + " -%");
					listToOutput.Add("\tlw " + register3 + "," + offsetTempVarName2 + "(R0)");
					listToOutput.Add("\tlw " + register4 + "," + items[count - 5] + "(" + register3 + ")");
					listToOutput.Add("\tsw " + varName + "(R0)," + register4);
					registerPool.Push(register4);
					registerPool.Push(register3);

					lastOperand = varName;
					lastOperandType = "variable";
					items.RemoveAt(count - 1);
					items.RemoveAt(count - 2);
					items.RemoveAt(count - 3);
					items.RemoveAt(count - 4);
					items.RemoveAt(count - 5);
					items.RemoveAt(count - 6);
				}
				else
				{
					var listToOutput = isaFunc ? moonFunctionCode : moonExecCode;

					// check if contains expression
					var bracketIndex = items.LastIndexOf("[");
					var insideBrackets = items.GetRange(bracketIndex + 1, items.Count() - (bracketIndex + 2));
					var insideCount = insideBrackets.Count();
					if (ContainsExpression(insideBrackets))
					{
						var result = ComputeExpression(insideBrackets, isaFunc, funcName, symbolTable);

						var varName = "temp_" + ++tempCount;
						moonDataCode.Add(varName + " res 4");

						var offsetTempVarName1 = "temp_" + ++tempCount;
						moonDataCode.Add(offsetTempVarName1 + " res 4");

						//var offsetVar = items[count - 2];

						var register1 = registerPool.Pop();
						var register2 = registerPool.Pop();
						listToOutput.Add("\n\t%- Mult value of " + result + " by int size -%");
						listToOutput.Add("\tlw " + register1 + "," + result + "(R0)");
						listToOutput.Add("\tmuli " + register2 + "," + register1 + ",4");
						listToOutput.Add("\tsw " + offsetTempVarName1 + "(R0)," + register2);
						registerPool.Push(register2);
						registerPool.Push(register1);

						//set varname
						var register3 = registerPool.Pop();
						var register4 = registerPool.Pop();
						listToOutput.Add("\n\t%- Setting " + items[bracketIndex - 1] + "[" + result + "] to temp variable " + varName + " -%");
						listToOutput.Add("\tlw " + register3 + "," + offsetTempVarName1 + "(R0)");
						listToOutput.Add("\tlw " + register4 + "," + items[bracketIndex - 1] + "(" + register3 + ")");
						listToOutput.Add("\tsw " + varName + "(R0)," + register4);
						registerPool.Push(register4);
						registerPool.Push(register3);

						lastOperand = varName;
						lastOperandType = "variable";
						for (var i = 1; i <= insideCount + 4; i++)
						{
							items.RemoveAt(count - i);
						}
					}
				}
			}
			else if (items[count - 2].Equals("id"))
			{
				if (items[count - 3].Equals("."))
				{
					var classVar = items[count - 4];

					if (items[count - 6].Equals("."))
					{
						classVar = items[count - 7] + "_" + items[count - 4];
					}

					lastOperand = isaFunc ? funcName + "_" + classVar + "_" + items[count - 1] : classVar + "_" + items[count - 1];
					lastOperandType = "variable";
					items.RemoveAt(count - 1);
					items.RemoveAt(count - 2);
					items.RemoveAt(count - 3);
					items.RemoveAt(count - 4);
					items.RemoveAt(count - 5);

					if (items[count - 6].Equals("."))
					{
						items.RemoveAt(count - 6);
						items.RemoveAt(count - 7);
						items.RemoveAt(count - 8);
					}
				}
				else
				{
					lastOperand = isaFunc && !items[count - 1].Contains("temp_") ? funcName + "_" + items[count - 1] : items[count - 1];
					lastOperandType = "variable";
					items.RemoveAt(count - 1);
					items.RemoveAt(count - 2);
				}
			}

			count = items.Count();
			if (items.Count() == 0)
			{
				return lastOperand;
			}
			var operand = items[count - 1];
			items.RemoveAt(count - 1);
			count = items.Count();

			if (int.TryParse(items[count - 1], out _))
			{
				firstOperand = items[count - 1];
				firstOperandType = "literal";
				items.RemoveAt(count - 1);
			}
			else if (float.TryParse(items[count - 1], out _))
			{
				//do float ops
			}
			else if (items[count - 1].Equals("]"))
			{
				//do array ops
				var offset = 0;
				if (int.TryParse(items[count - 2], out offset) && items[count - 3].Equals("["))
				{
					var varName = "temp_" + ++tempCount;
					moonDataCode.Add(varName + " res 4");

					var inputName = items[count - 4];
					var listToOutput = isaFunc ? moonFunctionCode : moonExecCode;
					var symbolValue = symbolTable.TableEntries.Find(c => c.Name.Equals(items[count - 4]) && c.Type.Contains("["));
					var offsetCount = 0;

					if (symbolValue.Type.Contains("integer"))
					{
						offsetCount = 4 * offset;
					}
					else if (symbolValue.Type.Contains("float"))
					{
						// dont use this yet
						offsetCount = 8 * offset;
					}
					else
					{
						//some class offset
					}

					//set varname
					var register1 = registerPool.Pop();
					var register2 = registerPool.Pop();
					listToOutput.Add("\n\t%- Setting " + inputName + "[" + offset + "] to temp variable " + varName + " -%");
					listToOutput.Add("\taddi " + register1 + ",R0," + offsetCount);
					listToOutput.Add("\tlw " + register2 + "," + inputName + "(" + register1 + ")");
					listToOutput.Add("\tsw " + varName + "(R0)," + register2);
					registerPool.Push(register2);
					registerPool.Push(register1);

					firstOperand = varName;
					firstOperandType = "variable";
					items.RemoveAt(count - 1);
					items.RemoveAt(count - 2);
					items.RemoveAt(count - 3);
					items.RemoveAt(count - 4);
					items.RemoveAt(count - 5);
				}
				else if (items[count - 4].Equals("["))
				{
					var listToOutput = isaFunc ? moonFunctionCode : moonExecCode;

					var varName = "temp_" + ++tempCount;
					moonDataCode.Add(varName + " res 4");

					var offsetTempVarName1 = "temp_" + ++tempCount;
					moonDataCode.Add(offsetTempVarName1 + " res 4");
					var offsetTempVarName2 = "temp_" + ++tempCount;
					moonDataCode.Add(offsetTempVarName2 + " res 4");

					var offsetVar = isaFunc && !items[count - 2].Contains("temp_") ? funcName + "_" + items[count - 2] : items[count - 2];

					var register = registerPool.Pop();
					listToOutput.Add("\n\t%- Retriving value of " + offsetVar + " -%");
					listToOutput.Add("\tlw " + register + "," + offsetVar + "(R0)");
					listToOutput.Add("\tsw " + offsetTempVarName1 + "(R0)," + register);
					registerPool.Push(register);

					var register1 = registerPool.Pop();
					var register2 = registerPool.Pop();
					listToOutput.Add("\n\t%- Mult value of " + offsetTempVarName1 + " by int size -%");
					listToOutput.Add("\tlw " + register1 + "," + offsetTempVarName1 + "(R0)");
					listToOutput.Add("\tmuli " + register2 + "," + register1 + ",4");
					listToOutput.Add("\tsw " + offsetTempVarName2 + "(R0)," + register2);
					registerPool.Push(register2);
					registerPool.Push(register1);

					//set varname
					var register3 = registerPool.Pop();
					var register4 = registerPool.Pop();
					listToOutput.Add("\n\t%- Setting " + items[count - 5] + "[" + offset + "] to temp variable " + varName + " -%");
					listToOutput.Add("\tlw " + register3 + "," + offsetTempVarName2 + "(R0)");
					listToOutput.Add("\tlw " + register4 + "," + items[count - 5] + "(" + register3 + ")");
					listToOutput.Add("\tsw " + varName + "(R0)," + register4);
					registerPool.Push(register4);
					registerPool.Push(register3);

					firstOperand = varName;
					firstOperandType = "variable";
					items.RemoveAt(count - 1);
					items.RemoveAt(count - 2);
					items.RemoveAt(count - 3);
					items.RemoveAt(count - 4);
					items.RemoveAt(count - 5);
					items.RemoveAt(count - 6);
				}
				else
				{
					var listToOutput = isaFunc ? moonFunctionCode : moonExecCode;

					// check if contains expression
					var bracketIndex = items.LastIndexOf("[");
					var insideBrackets = items.GetRange(bracketIndex + 1, items.Count() - (bracketIndex + 2));
					var insideCount = insideBrackets.Count();

					if (ContainsExpression(insideBrackets))
					{
						var result = ComputeExpression(insideBrackets, isaFunc, funcName, symbolTable);

						var varName = "temp_" + ++tempCount;
						moonDataCode.Add(varName + " res 4");

						var offsetTempVarName1 = "temp_" + ++tempCount;
						moonDataCode.Add(offsetTempVarName1 + " res 4");

						//var offsetVar = items[count - 2];

						var register1 = registerPool.Pop();
						var register2 = registerPool.Pop();
						listToOutput.Add("\n\t%- Mult value of " + result + " by int size -%");
						listToOutput.Add("\tlw " + register1 + "," + result + "(R0)");
						listToOutput.Add("\tmuli " + register2 + "," + register1 + ",4");
						listToOutput.Add("\tsw " + offsetTempVarName1 + "(R0)," + register2);
						registerPool.Push(register2);
						registerPool.Push(register1);

						//set varname
						var register3 = registerPool.Pop();
						var register4 = registerPool.Pop();
						listToOutput.Add("\n\t%- Setting " + items[bracketIndex - 1] + "[" + offsetTempVarName1 + "] to temp variable " + varName + " -%");
						listToOutput.Add("\tlw " + register3 + "," + offsetTempVarName1 + "(R0)");
						listToOutput.Add("\tlw " + register4 + "," + items[bracketIndex - 1] + "(" + register3 + ")");
						listToOutput.Add("\tsw " + varName + "(R0)," + register4);
						registerPool.Push(register4);
						registerPool.Push(register3);

						firstOperand = varName;
						firstOperandType = "variable";
						for (var i = 1; i <= insideCount + 4; i++)
						{
							items.RemoveAt(count - i);
						}
					}
				}
			}
			else if (items[count - 2].Equals("id"))
			{
				if (count - 3 > items.Count() && items[count - 3].Equals("."))
				{
					var classVar = items[count - 4];

					if (items[count - 6].Equals("."))
					{
						classVar = items[count - 7] + "_" + items[count - 4];
					}

					firstOperand = isaFunc ? funcName + "_" + classVar + "_" + items[count - 1] : classVar + "_" + items[count - 1];
					firstOperandType = "variable";
					items.RemoveAt(count - 1);
					items.RemoveAt(count - 2);
					items.RemoveAt(count - 3);
					items.RemoveAt(count - 4);
					items.RemoveAt(count - 5);

					if (items[count - 6].Equals("."))
					{
						items.RemoveAt(count - 6);
						items.RemoveAt(count - 7);
						items.RemoveAt(count - 8);
					}
				}
				else
				{
					firstOperand = isaFunc && !items[count - 1].Contains("temp_") ? funcName + "_" + items[count - 1] : items[count - 1];
					firstOperandType = "variable";
					items.RemoveAt(count - 1);
					items.RemoveAt(count - 2);
				}
			}

			var returnLiteral = "";
			var operation = "";

			if (operand.Equals("+"))
			{
				operation = "add";
			}
			else if (operand.Equals("-"))
			{
				operation = "sub";
			}
			else if (operand.Equals("*"))
			{
				operation = "mul";
			}
			else if (operand.Equals("/"))
			{
				operation = "div";
			}
			else if (operand.Equals("%"))
			{
				operation = "mod";
			}
			else if (operand.Equals(">"))
			{
				operation = "cgt";
			}
			else if (operand.Equals("<"))
			{
				operation = "clt";
			}
			else if (operand.Equals(">="))
			{
				operation = "gle";
			}
			else if (operand.Equals("<="))
			{
				operation = "cle";
			}
			else if (operand.Equals("and"))
			{
				operation = "and";
			}
			else if (operand.Equals("or"))
			{
				operation = "or";
			}
			else if (operand.Equals("not"))
			{
				operation = "not";
			}
			else if (operand.Equals("=="))
			{
				operation = "ceq";
			}
			else if (operand.Equals("!="))
			{
				operation = "cne";
			}

			if (!operation.Equals(""))
			{
				returnLiteral = ExecuteCalculationCode(operation, firstOperand, firstOperandType, lastOperand, lastOperandType, isaFunc, funcName);
				items.Add("id");
				items.Add(returnLiteral);
			}

			if (ContainsExpression(items))
			{
				return ComputeExpression(items, isaFunc, funcName, symbolTable);
			}

			return returnLiteral;
		}

		public static void StartRegisters()
		{
			registerPool.Push("R13");
			registerPool.Push("R12");
			registerPool.Push("R11");
			registerPool.Push("R10");
			registerPool.Push("R9");
			registerPool.Push("R8");
			registerPool.Push("R7");
			registerPool.Push("R6");
			registerPool.Push("R5");
			registerPool.Push("R4");
			registerPool.Push("R3");
			registerPool.Push("R2");
			registerPool.Push("R1");
		}
	}
}
