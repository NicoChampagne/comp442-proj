using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using LexDriver;

namespace SynSemDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            SystemChecks(args[0]);
            var fileName = Path.GetFileNameWithoutExtension(args[0]);

            try
            {
                var sr = new StreamReader(args[0]);
                var tokens = new List<TokenData>();
                var swTokens = new StreamWriter(Path.GetDirectoryName(args[0]) + "\\" + fileName + ".outlextokens");
                var swErrors = new StreamWriter(Path.GetDirectoryName(args[0]) + "\\" + fileName + ".outlexerrors");

                //Read the first line of file
                var line = sr.ReadLine();
                var inlineLexeme = "";
                int inlineLine = 0;
                int lineNumber = 1;
                bool inline = false;
                bool firstTokenLine = true;

                //Continue to read until you reach end of file
                while (line != null)
                {
                    var modifiedLine = line;
                    // Analyze the line
                    foreach (string key in TokenData.ReservedWordDictionary.Keys)
                    {
                        if (key.Equals("*") || key.Equals("/") || key.Equals("or") || key.Equals("and"))
                        {
                            continue;
                        }

                        if (modifiedLine.Contains(key))
                        {
                            modifiedLine = modifiedLine.Replace($"{key}", $" {key} ");
                        }
                    }

                    var trimmedLine = Regex.Replace(modifiedLine, @"\s+", " ");

                    var words = trimmedLine.Split(' ');

                    bool firstToken = true;

                    for (int i = 0; i < words.Length; i++)
                    {
                        if (words[i].Equals(string.Empty))
                        {
                            continue;
                        }

                        var token = LexicalAnalyzer.Analyze(words[i].Trim(), lineNumber, i);

                        if (token.Type.StartsWith("Invalid"))
                        {
                            if (!inline)
                            {
                                swErrors.WriteLine(token.ToErrorString());
                            }
                        }
                        else
                        {
                            if (!inline)
                            {
                                if (firstToken && !firstTokenLine)
                                {
                                    swTokens.WriteLine();
                                }

                                if (token.Type.Equals("blockcmt"))
                                {
                                    var lexeme = line.Substring(line.IndexOf("//"));
                                    token.Lexeme = lexeme;
                                    swTokens.Write(token);
                                    break;
                                }

                                if (token.Type.Equals("inlinecmt"))
                                {
                                    inline = !inline;
                                    inlineLexeme += token.Lexeme + " ";
                                    inlineLine = token.Line;
                                    continue;
                                }

                                swTokens.Write(token);
                                tokens.Add(token);
                                firstToken = false;
                                firstTokenLine = false;
                            }

                            if (inline && !token.Type.Equals("inlinecmt"))
                            {
                                inlineLexeme += token.Lexeme + " ";
                            }

                            if (inline && token.Type.Equals("inlinecmt"))
                            {
                                inline = !inline;
                                inlineLexeme += token.Lexeme;
                                token.Lexeme = inlineLexeme;
                                token.Line = inlineLine;
                                swTokens.Write(token);
                                inlineLexeme = "";
                            }
                        }
                    }

                    //Read the next line
                    line = sr.ReadLine();
                    ++lineNumber;
                }

                sr.Close();
                swTokens.Close();
                swErrors.Close();

                ////Read the first line of file
                //var line = sr.ReadLine();
                ////Continue to read until you reach end of file
                //while (line != null)
                //{
                //    // Analyze the line
                //    var trimmedLine = Regex.Replace(line, @"\s+", "");
                //    string[] separator = { "][" };
                //    var words = trimmedLine.Split(separator, StringSplitOptions.None);

                //    for (int i = 0; i < words.Length; i++)
                //    {
                //        if (words[i].Equals(string.Empty))
                //        {
                //            continue;
                //        }

                //        if (i == 0)
                //        {
                //            words[i] = words[i].Substring(1);
                //        }

                //        if (i == words.Length - 1)
                //        {
                //            words[i] = words[i].Substring(0, words[i].Length-1);
                //        }

                //        var splitToken = words[i].Trim().Split(',');

                //        if (splitToken[0].Equals("inlinecmt") || splitToken[0].Equals("blockcmt"))
                //        {
                //            continue;
                //        }

                //        if (splitToken[0].Equals("comma"))
                //        {
                //            splitToken[2] = splitToken[3];
                //            splitToken[3] = splitToken[4];
                //            splitToken[1] = ",";
                //        }


                //        var token = new TokenData(splitToken[0], splitToken[1], int.Parse(splitToken[2]), int.Parse(splitToken[3]));
                //        tokens.Add(token);
                //    }

                //    //Read the next line
                //    line = sr.ReadLine();
                //}

                var swDev = new StreamWriter(Path.GetDirectoryName(args[0]) + "\\" + fileName + ".outderivation");
                var swTree = new StreamWriter(Path.GetDirectoryName(args[0]) + "\\" + fileName + ".outast");
                var swSynErrors = new StreamWriter(Path.GetDirectoryName(args[0]) + "\\" + fileName + ".outsyntaxerrors");

                var parsedOutput = SyntacticAnalyzer.Parse(tokens);

                foreach (string errorLine in parsedOutput.errorList)
                {
                    swSynErrors.WriteLine(errorLine);
                }

                parsedOutput.parseTree.TraverseDFSInFile(swDev);

                var absTree = SyntacticAnalyzer.ParseTreeToAbstractSyntaxTree(parsedOutput.parseTree);
                absTree.TraverseDFSInFile(swTree);

                sr.Close();
                swDev.Close();
                swTree.Close();
                swSynErrors.Close();

                Console.WriteLine("Done reading and writing derivation files");

                var swTable = new StreamWriter(Path.GetDirectoryName(args[0]) + "\\" + fileName + ".outsymboltables");
                var swSemErrors = new StreamWriter(Path.GetDirectoryName(args[0]) + "\\" + fileName + ".outsemanticerrors");


                //Start semantic parsing

                // Traverse Ast and create symbol tables.
                var symbolTableResult = SemanticAnalyzer.CreateSymbolTable(absTree);
                swTable.WriteLine(symbolTableResult.symbolTable);

                // Traverse Ast and ensure semantics are good.
                var semResult = SemanticAnalyzer.SemanticAnalysis(absTree, symbolTableResult.symbolTable, symbolTableResult.errorList);

                foreach(var error in symbolTableResult.errorList)
                {
                    swSemErrors.WriteLine(error);
                }

                swTable.Close();
                swSemErrors.Close();

                var moon = new StreamWriter(Path.GetDirectoryName(args[0]) + "\\" + fileName + ".moon");

                var moonCode = CodeGenerator.GenerateMoonCode(semResult.subScopeList, symbolTableResult.symbolTable);

                foreach (var moonLine in moonCode)
                {
                    moon.WriteLine(moonLine);
                }

                moon.Close();

                Console.WriteLine("Done reading and writing semantic files");
                System.Environment.Exit(9000);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                System.Environment.Exit(2);
            }
        }

        public static void SystemChecks(string filePath = "")
        {
            if (!filePath.EndsWith(".src"))
            {
                Console.WriteLine("Compilers needs a .src file as argument to continue analysis.");
                Console.WriteLine("Ending program");
                System.Environment.Exit(0);
            }

            if (!File.Exists(filePath))
            {
                Console.WriteLine("Error: File " + filePath + " does not exist on this system.");
                Console.WriteLine("Ending program");
                System.Environment.Exit(1);
            }
        }
    }
}
