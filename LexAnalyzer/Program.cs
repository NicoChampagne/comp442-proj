using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LexDriver
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

                Console.WriteLine("Done reading and writing files");
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
                Console.WriteLine("LexDriver needs a .src file as argument to continue analysis.");
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
