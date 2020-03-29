using System;
using System.Collections.Generic;
using System.Text;

namespace SynDriver
{
    /// <summary>Represents a tree node</summary>
    /// <typeparam name="T">the type of the values in nodes
    /// </typeparam>
    public class TreeNode<T>
    {
        // Contains the value of the node
        private T value;
        // Shows whether the current node has a parent or not
        private bool hasParent;
        private TreeNode<T> parent;
        // Contains the children of the node (zero or more)
        private List<TreeNode<T>> children;
        public bool hasChildren => ChildrenCount > 0;
        public int getSiblingsCount => parent.ChildrenCount - 1;

        /// <summary>Constructs a tree node</summary>
        /// <param name="value">the value of the node</param>
        public TreeNode(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Cannot insert null value!");
            }

            this.value = value;
            this.children = new List<TreeNode<T>>();
        }

        /// <summary>The value of the node</summary>
        public T Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.value = value;
            }
        }
        
        /// <summary>The number of node's children</summary>
        public int ChildrenCount
        {
            get
            {
                return this.children.Count;
            }
        }

        /// <summary>Adds child to the node</summary>
        /// <param name="child">the child to be added</param>
        public void AddChild(TreeNode<T> child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("Cannot insert null value!");
            }
            
            if (child.hasParent)
            {
                throw new ArgumentException("The node already has a parent!");
            }

            child.hasParent = true;
            child.parent = this;
            this.children.Add(child);
        }

        public void AddChildAtIndex(TreeNode<T> child, int index)
        {
            if (child == null)
            {
                throw new ArgumentNullException("Cannot insert null value!");
            }

            if (child.hasParent)
            {
                throw new ArgumentException("The node already has a parent!");
            }

            child.hasParent = true;
            child.parent = this;
            this.children.Insert(index, child);
        }

        public void newParent(TreeNode<T> child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("Cannot insert null value!");
            }

            child.parent = this;
            this.children.Add(child);
        }

        public void newParentWithChildIndex(TreeNode<T> child, int index)
        {
            if (child == null)
            {
                throw new ArgumentNullException("Cannot insert null value!");
            }

            child.parent = this;
            this.children.Insert(index, child);
        }

        public void RemoveChild(TreeNode<T> child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("Cannot insert null value!");
            }

            if (!child.hasParent)
            {
                throw new ArgumentException("The node doesnt have a parent!");
            }

            child.parent.children.Remove(child);
            child.parent = null;
            child.hasParent = false;
        }

        /// <summary>
        /// Gets the child of the node at given index
        /// </summary>
        /// <param name="index">the index of the desired child</param>
        /// <returns>the child on the given position</returns>
        public TreeNode<T> GetChild(int index)
        {
            return this.children[index];
        }

        public TreeNode<T> GetParent()
        {
            return this.parent;
        }
    }
}
