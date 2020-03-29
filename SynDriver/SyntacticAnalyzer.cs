using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LexDriver;

namespace SynDriver
{
    public class SyntacticAnalyzer
    {
        private static Stack<(string, TreeNode<string>)> synStack = new Stack<(string, TreeNode<string>)>();
        private static Tree<string> parseTree;
        private static List<string> errorList = new List<string>();

        public static (bool containsErrors, Tree<string> parseTree, List<string> errorList) Parse(List<TokenData> tokensToParse)
        {
            var index = 0;
            var currentToken = default(TokenData);
            string tempString = "";
            string tokenSymbol = "";
            bool errorOccurred = false;

            var toStopStack = ("$", default(TreeNode<string>));
            synStack.Push(toStopStack);

            parseTree = new Tree<string>("START");
            TreeNode<string> currentNode = parseTree.Root;
            var toStartStack = ("START", currentNode);
            synStack.Push(toStartStack);

            currentToken = tokensToParse[index];
            tokenSymbol = SymbolToken(currentToken);

            while (synStack.Peek().Item1 != "$")
            {
                tempString = synStack.Peek().Item1;

                // If the top of stack is a terminal symbol
                if (ParseTable.TerminalSymbolsSet.Contains(tempString)) 
                {
                    if (tempString == tokenSymbol)
                    {
                        if (tempString != currentToken.Lexeme)
                        {
                            currentNode.AddChild(new TreeNode<string>(currentToken.Lexeme));
                        }

                        synStack.Pop();
                        if (index != tokensToParse.Count-1)
                        {
                            currentToken = tokensToParse[++index];
                        }
                        else
                        {
                            return (false, parseTree, errorList);
                        }

                        tokenSymbol = SymbolToken(currentToken);
                    }
                    else
                    {
                        //skipErrors
                        errorList.Add("Syntactic error on line (" + currentToken.Line + "," + currentToken.Index + ") Skipping token: " + currentToken.Lexeme + " with production: " + synStack.Peek().Item1);

                        if (index != tokensToParse.Count-1)
                        {
                            currentToken = tokensToParse[++index];
                        }
                        else
                        {
                            return (false, parseTree, errorList);
                        }

                        tokenSymbol = SymbolToken(currentToken);
                        errorOccurred = true;
                    }
                }
                else // A non-terminal symbol on top of the stack
                {
                    // If there is a table production for this combination
                    if (ParseTable.ProductionExists(tempString, tokenSymbol))
                    {
                        currentNode = synStack.Peek().Item2;
                        synStack.Pop();
                        var key = (tempString, tokenSymbol);
                        var production = ParseTable.NonTerminalSymbolToProductionDictionary[key];
                        var productions = production.Split(' ');
                        var reverseProductions = productions.Reverse().ToList();
                        List<TreeNode<string>> nodes = new List<TreeNode<string>>();

                        foreach (string symbol in productions)
                        {
                            // Push productions to tree
                            if (currentNode.Value.Contains("Rep") && productions.Count() == 1)
                            {
                                currentNode.Value = symbol;
                            }
                            else
                            {
                                var node = new TreeNode<string>(symbol);
                                currentNode.AddChild(node);
                                nodes.Add(node);
                            }
                        }

                        if (production.Equals("EPSILON"))
                        {
                            continue;
                        }

                        currentNode = currentNode.GetChild(0);

                        for (var i = 0; i < reverseProductions.Count() ; i++)
                        {
                            // Push productions to stack
                            var nextProduction = (reverseProductions[i], nodes[reverseProductions.Count() - i - 1]);
                            synStack.Push(nextProduction);
                        }
                    }
                    else
                    {
                        //skipErrors
                        errorList.Add("Syntactic error on line (" + currentToken.Line + "," + currentToken.Index + ") Skipping token: " + currentToken.Lexeme + " with production: " + synStack.Peek().Item1);

                        if (index != tokensToParse.Count-1)
                        {
                            currentToken = tokensToParse[++index];
                        }
                        else
                        {
                            return (false, parseTree, errorList);
                        }

                        tokenSymbol = SymbolToken(currentToken);
                        errorOccurred = true;
                    }
                }
            }

            if ((currentToken.Lexeme != "$") || (errorOccurred == true))
            {
                return (false, parseTree, errorList);
            }
            else
            {
                return (true, parseTree, errorList);
            }
        }

        public static string SymbolToken(TokenData token)
        {
            if (ParseTable.TerminalSymbolsSet.Contains(token.Type))
            {
                return token.Type;
            }
            else
            {
                return token.Lexeme;
            }
        }

        public static Tree<string> ParseTreeToAbstractSyntaxTree(Tree<string> parseTree)
        {
            //for every node but first node see if we can cut it out
            parseTree.TrimToAbstractTree(ParseTable.NonTerminalSymbolsSet.ToList());

            return parseTree;
        }
    }
}
