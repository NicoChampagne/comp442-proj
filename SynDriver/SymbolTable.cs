using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynDriver
{
    public class SymbolTable
    {
        public string Name { get; set; }

        // set when inherited
        public string InheritedTable { get; } = "";

        // set when inherited
        public bool IsInherited { get; } = false;

        public List<SymbolValue> TableEntries { get; } = new List<SymbolValue>();

        public SymbolTable(string name = "")
        {
            Name = name;
        }

        public SymbolTable CreateNewTable(string name = "")
        {
            var newTable = new SymbolTable();
            newTable.Name = name;

            return newTable;
        }

        public List<SymbolValue> GetVariables()
        {
            var allVariables = new List<SymbolValue>();

            foreach (var symbol in this.TableEntries)
            {
                if (symbol.Kind.Equals("Variable"))
                {
                    allVariables.Add(symbol);
                }
            }

            return allVariables;
        }

        public List<SymbolValue> GetFunctions()
        {
            var allFunctions = new List<SymbolValue>();

            foreach (var symbol in this.TableEntries)
            {
                if (symbol.Kind.Equals("Function"))
                {
                    allFunctions.Add(symbol);
                }
            }

            return allFunctions;
        }

        public List<SymbolTable> GetClassTables()
        {
            var allClasses = new List<SymbolTable>();

            foreach (var symbol in this.TableEntries)
            {
                if (symbol.Kind.Equals("Class"))
                {
                    allClasses.Add(symbol.Link);
                }
            }

            return allClasses;
        }

        public List<SymbolTable> GetTables()
        {
            var allTables = new List<SymbolTable>();
            allTables.Add(this);

            foreach (var symbol in this.TableEntries)
            {
                if (symbol.Link != null)
                {
                    allTables.Add(symbol.Link);

                    foreach (var innerSymbol in symbol.Link.TableEntries)
                    {
                        if (innerSymbol.Link != null)
                        {
                            allTables.Add(innerSymbol.Link);

                            foreach (var innerInnerSymbol in innerSymbol.Link.TableEntries)
                            {
                                if (innerInnerSymbol.Link != null)
                                {
                                    allTables.Add(innerInnerSymbol.Link);
                                }
                            }
                        }
                    }
                }
            }

            return allTables;
        }

        public void Search()
        {

        }

        public void Insert(SymbolValue newSymbol)
        {
            TableEntries.Add(newSymbol);
        }

        public void Print()
        {
            // Print table name
            // Print all symbols
            // if symbol contain tables print subtables
        }

        public void Delete(SymbolValue newSymbol)
        {
            TableEntries.Remove(newSymbol);
        }

        public string EntriesToString()
        {
            var entries = "";
            TableEntries.ForEach(c => entries += c.ToString() + "\n");
            return entries;
        }

        public override string ToString()
        {
            var subTables = "";
            TableEntries.ForEach(c =>
            {
                if (c.Link != null)
                {
                    subTables += c.Link.ToString();
                }
            });

            return "----------------------------------------------------------------------------\n" +
                "Table: " + Name + "\n" +
                EntriesToString() +
                "---------------------------------------------------------------------------\n" +
                subTables;
        }
    }
}
