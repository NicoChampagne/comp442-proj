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
                var swTokens = new StreamWriter(Path.GetDirectoryName(args[0]) + "\\" + fileName + ".outderivation");
                var swTree = new StreamWriter(Path.GetDirectoryName(args[0]) + "\\" + fileName + ".outast");
                var swErrors = new StreamWriter(Path.GetDirectoryName(args[0]) + "\\" + fileName + ".outsyntaxerrors");
                var tokens = new List<TokenData>();

                //Read the first line of file
                var line = sr.ReadLine();
                //Continue to read until you reach end of file
                while (line != null)
                {
                    // Analyze the line
                    var trimmedLine = Regex.Replace(line, @"\s+", "");
                    string[] separator = { "][" };
                    var words = trimmedLine.Split(separator, StringSplitOptions.None);

                    for (int i = 0; i < words.Length; i++)
                    {
                        if (words[i].Equals(string.Empty))
                        {
                            continue;
                        }

                        if (i == 0)
                        {
                            words[i] = words[i].Substring(1);
                        }

                        if (i == words.Length - 1)
                        {
                            words[i] = words[i].Substring(0, words[i].Length-1);
                        }

                        var splitToken = words[i].Trim().Split(',');

                        if (splitToken[0].Equals("inlinecmt") || splitToken[0].Equals("blockcmt"))
                        {
                            continue;
                        }

                        if (splitToken[0].Equals("comma"))
                        {
                            splitToken[2] = splitToken[3];
                            splitToken[3] = splitToken[4];
                            splitToken[1] = ",";
                        }


                        var token = new TokenData(splitToken[0], splitToken[1], int.Parse(splitToken[2]), int.Parse(splitToken[3]));
                        tokens.Add(token);
                    }

                    //Read the next line
                    line = sr.ReadLine();
                }

                var parsedOutput = SyntacticAnalyzer.Parse(tokens);

                foreach (string errorLine in parsedOutput.errorList)
                {
                    swErrors.WriteLine(errorLine);
                }

                parsedOutput.parseTree.TraverseDFSInFile(swTokens);

                var absTree = SyntacticAnalyzer.ParseTreeToAbstractSyntaxTree(parsedOutput.parseTree);
                absTree.TraverseDFSInFile(swTree);

                sr.Close();
                swTokens.Close();
                swTree.Close();
                swErrors.Close();

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
            if (!filePath.EndsWith(".outlextokens"))
            {
                Console.WriteLine("SynDriver needs a .outlextokens file as argument to continue analysis.");
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
