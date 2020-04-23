using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SynSemDriver
{
    /// <summary>Represents a tree data structure</summary>
    /// <typeparam name="T">the type of the values in the
    /// tree</typeparam>
    public class Tree<T>
    {
        // The root of the tree
        private TreeNode<T> root;

        /// <summary>Constructs the tree</summary>
        /// <param name="value">the value of the node</param>
        public Tree(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Cannot insert null value!");
            }

            this.root = new TreeNode<T>(value);
        }

        /// <summary>Constructs the tree</summary>
        /// <param name="value">the value of the root node</param>
        /// <param name="children">the children of the root
        /// node</param>
        public Tree(T value, params Tree<T>[] children) : this(value)
        {
            foreach (Tree<T> child in children)
            {
                this.root.AddChild(child.root);
            }
        }

        /// <summary>
        /// The root node or null if the tree is empty
        /// </summary>
        public TreeNode<T> Root
        {
            get
            {
                return this.root;
            }
        }

        /// <summary>Traverses and prints tree in
        /// Depth-First Search (DFS) manner</summary>
        /// <param name="root">the root of the tree to be
        /// traversed</param>
        /// <param name="spaces">the spaces used for
        /// representation of the parent-child relation</param>
        private void PrintDFS(TreeNode<T> root, string spaces)
        {
            if (this.root == null)
            {
                return;
            }

            //Console.WriteLine(spaces + root.Value);

            TreeNode<T> child = null;

            for (int i = 0; i < root.ChildrenCount; i++)
            {
                child = root.GetChild(i);
                PrintDFS(child, spaces + "   ");
            }
        }

        /// <summary>Traverses and prints the tree in
        /// Depth-First Search (DFS) manner</summary>
        public void TraverseDFS()
        {
            this.PrintDFS(this.root, string.Empty);
        }

        private void PrintDFSToFile(TreeNode<T> root, string spaces, StreamWriter sw)
        {
            if (this.root == null)
            {
                return;
            }

            sw.WriteLine(spaces + root.Value);

            TreeNode<T> child = null;

            for (int i = 0; i < root.ChildrenCount; i++)
            {
                child = root.GetChild(i);
                PrintDFSToFile(child, spaces + "   ", sw);
            }
        }

        /// <summary>Traverses and prints the tree in
        /// Depth-First Search (DFS) manner</summary>
        public void TraverseDFSInFile(StreamWriter sw)
        {
            this.PrintDFSToFile(this.root, string.Empty, sw);
        }

        private void TrimTree(TreeNode<T> root, List<string> nonTerminals, int levelsToTrim)
        {
            if (this.root == null || levelsToTrim == 0)
            {
                return;
            }

            TreeNode<T> child = null;
            TreeNode<T> grandChild = null;

            for (int i = 0; i < root.ChildrenCount; i++)
            {
                child = root.GetChild(i);

                if (nonTerminals.Contains(child.Value as string))
                {
                    for (int j = 0; j < child.ChildrenCount; j++)
                    {
                        grandChild = child.GetChild(j);
                        root.newParent(grandChild);
                    }

                    root.RemoveChild(child);
                }
                
                TrimTree(child, nonTerminals, levelsToTrim - 1);
            }
        }

        private void TreeModify(TreeNode<T> root, List<string> nonTerminals)
        {
            if (this.root == null)
            {
                return;
            }

            TreeNode<string> child = null;

            for (int i = 0; i < root.ChildrenCount; i++)
            {
                child = root.GetChild(i) as TreeNode<string>;

                if (child.Value.Equals("FuncDef"))
                {
                    child.Value = "Function:";
                }
                else if (child.Value.Equals("ClassDecl"))
                {
                    child.Value = "Class:";
                }
                else if (nonTerminals.Contains(child.Value))
                {
                    child.Value = "->";
                }
                else if (child.Value.Equals("EPSILON") || child.Value.Equals("intnum") || child.Value.Equals("floatnum") || child.Value.Equals("lt") || child.Value.Equals("gt") || child.Value.Equals("geq") || child.Value.Equals("leq"))
                {
                    child.Value = "";
                }

                TreeModify(child as TreeNode<T>, nonTerminals);
            }
        }

        private bool TrimRoot(TreeNode<T> root, List<string> nonTerminals)
        {
            if (this.root == null)
            {
                return true;
            }

            TreeNode<T> child = null;
            TreeNode<T> grandChild = null;
            bool repeat = false;

            for (int i = 0; i < root.ChildrenCount; i++)
            {
                child = root.GetChild(i);

                if (nonTerminals.Contains(child.Value as string))
                {
                    for (int j = 0; j < child.ChildrenCount; j++)
                    {
                        grandChild = child.GetChild(j);
                        root.newParent(grandChild);
                    }

                    root.RemoveChild(child);
                    repeat = true;
                }
            }
            return repeat;
        }

        private bool PerfectTrim(TreeNode<T> root, List<string> nonTerminals)
        {
            if (this.root == null)
            {
                return true;
            }

            TreeNode<T> child = null;
            TreeNode<T> grandChild = null;
            bool repeat = false;

            for (int i = root.ChildrenCount-1; i >= 0 ; i--)
            {
                child = root.GetChild(i);

                if (nonTerminals.Contains(child.Value as string))
                {
                    for (int j = 0; j < child.ChildrenCount; j++)
                    {
                        grandChild = child.GetChild(j);
                        root.newParent(grandChild);
                    }

                    root.RemoveChild(child);
                    repeat = true;
                }
            }
            return repeat;
        }

        public void TrimToAbstractTree(List<string> nonTerminals)
        {
            this.TreeModify(this.root, nonTerminals);
        }

        public static List<string> PreOrderingOfSubNodes(TreeNode<string> root, List<string> nodeValues)
        {
            if (!root.Value.Equals("") && !root.Value.Equals("->"))
            {
                nodeValues.Add(root.Value);
            }

            if (root != null && root.hasChildren)
            {
                for (int i = 0; i < root.ChildrenCount; i++)
                {
                    var child = root.GetChild(i);
                    PreOrderingOfSubNodes(child, nodeValues);
                }
            }

            return nodeValues;
        }

        public static List<TreeNode<string>> NextSubNodes(TreeNode<string> root)
        {
            List<TreeNode<string>> nodeValues = new List<TreeNode<string>>();

            if (root != null && root.hasChildren)
            {
                for (int i = 0; i < root.ChildrenCount; i++)
                {
                    var child = root.GetChild(i);
                    nodeValues.Add(child);
                }
            }

            return nodeValues;
        }

        public static List<string> LevelOrderOfSubNodes(TreeNode<string> root)
        {
            Queue<TreeNode<string>> queue = new Queue<TreeNode<string>>();
            List<string> nodeValues = new List<string>();

            queue.Enqueue(root);

            while (queue.Count != 0)
            {
                TreeNode<string> tempNode = queue.Dequeue();

                if (!tempNode.Value.Equals("") && !tempNode.Value.Equals("->"))
                {
                    nodeValues.Add(tempNode.Value);
                }

                if (tempNode != null && tempNode.hasChildren)
                {
                    for (int i = 0; i < tempNode.ChildrenCount; i++)
                    {
                        queue.Enqueue(tempNode.GetChild(i));
                    }
                }
            }

            return nodeValues;
        }


        public int MaxDepth(TreeNode<T> node, int depth)
        {
            if (node == null)
                return 0;
            else
            {
                TreeNode<T> child = null;
                List<int> depths = new List<int>();

                for (int i = 0; i < node.ChildrenCount; i++)
                {
                    child = node.GetChild(i);
                    var calculatedDepth = MaxDepth(child, depth + 1) + 1;
                    depths.Add(calculatedDepth);
                }

                int tempMax = 0;
                foreach(int possibility in depths)
                {
                    if (possibility > tempMax)
                    {
                        tempMax = possibility;
                    }
                }

                return tempMax;
            }
        }
    }
}
