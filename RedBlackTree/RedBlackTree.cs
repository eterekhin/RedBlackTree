using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RedBlackTree.Tests")]
namespace RedBlackTreeStructure
{
    public static class NodeExtensions
    {
        public static bool IsRightChild(this RedBlackTree.Node nd) => nd.Parent.Right == nd;
        public static bool IsLeftChild(this RedBlackTree.Node nd) => nd.Parent.Left == nd;

        public static RedBlackTree.Node Uncle(this RedBlackTree.Node nd)
        {
            if (IsLeftChild(nd.Parent))
                return nd.Parent.Parent.Right;

            if (IsRightChild(nd.Parent))
                return nd.Parent.Parent.Left;

            throw new Exception("Invalid state");
        }

        public static bool HasRedUncle(this RedBlackTree.Node nd)
        {
            if (nd?.Parent?.Parent == null)
            {
                return false;
            }

            if (nd.Parent.Parent.Left == nd.Parent)
            {
                return nd.Parent.Parent.Right?.Color == RedBlackTree.Color.Red;
            }

            if (nd.Parent.Parent.Right == nd.Parent)
            {
                return nd.Parent.Parent.Left?.Color == RedBlackTree.Color.Red;
            }

            throw new ArgumentException("state invalid, parent not parent for " + nameof(nd));
        }

        public static bool HasBlackUncleOrHasntUncle(this RedBlackTree.Node nd)
        {
            if (nd.Parent?.Parent == null)
                return false;


            if (nd.Parent.Parent.Left == nd.Parent)
            {
                return nd.Parent.Parent.Right == null || nd.Parent.Parent.Right.Color == RedBlackTree.Color.Black;
            }

            if (nd.Parent.Parent.Right == nd.Parent)
            {
                return nd.Parent.Parent.Left == null || nd.Parent.Parent.Left.Color == RedBlackTree.Color.Black;
            }

            throw new ArgumentException("state invalid, parent not parent for " + nameof(nd));
        }

        public static void LeftRotate(this RedBlackTree.Node d)
        {
            if (d == null)
                throw new ArgumentException("node doesn't equal null");
            var b = d.Parent;
            if (b == null)
                throw new ArgumentException("parent node doesn't equal null");
            if (d.Parent.Right != d)
                throw new ArgumentException("If you use left rotate node must contain right child");

            var high = d.Parent.Parent;
            var c = d.Left;
            b.SetRight(c);
            if (high?.Left == b)
                high.SetLeft(d);
            else if (high?.Right == b)
                high.SetRight(d);

            d.SetLeft(b);
        }

        public static void RightRotate(this RedBlackTree.Node d)
        {
            if (d == null)
                throw new ArgumentException("node doesn't equal null");
            var b = d.Parent;
            if (b == null)
                throw new ArgumentException("parent node doesn't equal null");
            if (b.Left != d)
                throw new ArgumentException("If you use right rotate node must contain left child");

            var high = b.Parent;

            var e = d.Right;
            b.SetLeft(e);

            if (high?.Left == b)
                high.SetLeft(d);
            else if (high?.Right == b)
                high.SetRight(d);

            d.SetRight(b);
        }

        public static RedBlackTree.Node Sibling(this RedBlackTree.Node nd)
        {
            if (nd.Parent == null)
            {
                throw new InvalidOperationException("parent is null");
            }

            if (nd.Parent.Left == nd)
                return nd.Parent.Right;
            if (nd.Parent.Right == nd)
                return nd.Parent.Left;

            throw new ArgumentException("invalid state");
        }
    }

    public class RedBlackTree
    {
        public enum Color
        {
            Red,
            Black
        }

        public class NodeEqualityComparer : IEqualityComparer<Node>
        {
            public static IEqualityComparer<Node> Comparer = new NodeEqualityComparer();

            public bool Equals(Node x, Node y)
            {
                if (x == null && y == null)
                    return true;

                if (x == null || y == null)
                    return false;
                return x.Color == y.Color && x.Value.Equals(y.Value) && Equals(x.Left, y.Left) &&
                       Equals(x.Right, y.Right);
            }

            public int GetHashCode(Node obj)
            {
                var left = obj.Left?.GetHashCode();
                var right = obj.Right?.GetHashCode();
                if (left != null && right != null)
                {
                    return left.Value ^ right.Value;
                }

                if (right != null)
                {
                    return right.Value;
                }

                if (left != null)
                {
                    return left.Value;
                }

                return obj.Value ^ (int) obj.Color;
            }
        }

        public class Node
        {
            private Node Nil => new Node(this);

            public Node()
            {
                _right = _left = Nil;
            }

            // nil creating
            private Node(Node parent)
            {
                Value = int.MinValue;
                Parent = parent;
                Color = Color.Black;
            }

            public void SetLeft(Node nd)
            {
                if (Left?.Parent != null)
                    Left.Parent = null;
                _left = nd;
                if (nd != null)
                    nd.Parent = this;
            }

            public void SetRight(Node nd)
            {
                if (Right?.Parent != null)
                    Right.Parent = null;
                _right = nd;
                if (nd != null)
                    nd.Parent = this;
            }

            public (Node removedNode, Node childRemovedNode) Remove()
            {
                if (Left.IsNil() && Right.IsNil())
                {
                    if (Parent?.Left == this)
                        Parent.SetLeft(Nil);
                    else if (Parent?.Right == this)
                        Parent.SetRight(Nil);
                    return (this, Nil);
                }

                if (Left.IsNil() && !Right.IsNil())
                {
                    if (Parent?.Left == this)
                    {
                        Parent.SetLeft(Left);
                        if (Color == Color.Black && Left.Color == Color.Red)
                        {
                            Left.Color = Color.Black;
                        }
                    }
                    else if (Parent?.Right == this)
                    {
                        Parent.SetRight(Right);
                        if (Color == Color.Black && Right.Color == Color.Red)
                        {
                            Right.Color = Color.Black;
                        }
                    }

                    return (this, Right);
                }

                if (!Left.IsNil() && Right.IsNil())
                {
                    if (Parent?.Left == this)
                    {
                        Parent.SetLeft(Left);
                        if (Color == Color.Black && Right.Color == Color.Red)
                        {
                            Right.Color = Color.Black;
                        }
                    }
                    else if (Parent?.Right == this)
                    {
                        Parent.SetRight(Left);
                        if (Color == Color.Black && Left.Color == Color.Red)
                        {
                            Left.Color = Color.Black;
                        }
                    }

                    return (this, Left);
                }

                var firstNodeWhichMore = Right;
                while (!firstNodeWhichMore.Left.IsNil())
                {
                    firstNodeWhichMore = firstNodeWhichMore.Left;
                }

                // ReSharper disable once PossibleNullReferenceException
                Value = firstNodeWhichMore.Value;
                return firstNodeWhichMore.Remove();
            }

            public bool IsNil() => Value == int.MinValue;

            private Node _left;
            private Node _right;

            public Node Left
            {
                get => _left;
                internal set => SetLeft(value);
            }

            public Node Right
            {
                get => _right;
                internal set => SetRight(value);
            }

            public Node Parent { get; private set; }
            public Color Color;
            public int Value;
        }

        internal Node _root;

        public RedBlackTree(Node root = default) => _root = root;

        private (Node, bool isRightSon ) GetParentForNewNode(int value)
        {
            var parent = _root;
            if (parent == null)
                throw new AggregateException();
            while (true)
            {
                if (parent.Value < value)
                {
                    if (parent.Right.IsNil())
                        return (parent, true);
                    parent = parent.Right;
                }
                else
                {
                    if (parent.Left.IsNil())
                        return (parent, false);
                    parent = parent.Left;
                }
            }
        }

        private Node GetNode(int value)
        {
            var parent = _root;
            if (parent == null)
                throw new AggregateException();
            if (parent.Value == value)
                return parent;
            while (true)
            {
                if (parent.Value < value)
                {
                    if (parent.Right.IsNil())
                        throw new ArgumentException($"Node with key = {value} doesn't exit");
                    if (parent.Right.Value == value)
                        return parent.Right;
                    parent = parent.Right;
                }
                else
                {
                    if (parent.Left.IsNil())
                        throw new ArgumentException($"Node with key = {value} doesn't exit");

                    if (parent.Left.Value == value)
                        return parent.Left;
                    parent = parent.Left;
                }
            }
        }

        public void Insert(int n)
        {
            var newNode = new Node() {Value = n};
            if (_root == null)
            {
                _root = new Node()
                {
                    Color = Color.Black,
                    Value = n
                };
                return;
            }

            var (parent, isRight) = GetParentForNewNode(n);


            newNode.Color = Color.Red;
            newNode.Value = n;
            if (isRight)
            {
                parent.SetRight(newNode);
            }
            else
            {
                parent.SetLeft(newNode);
            }

            if (parent.Color == Color.Black)
            {
                return;
            }

            Balance(newNode);
        }

        public bool Contains(int value)
        {
            var node = _root;
            while (node != null)
            {
                if (node.Value > value)
                    node = node.Left;
                else if (node.Value < value) node = node.Right;
                else return true;
            }

            return false;
        }

        private void Balance(Node nd)
        {
            if (nd == _root)
            {
                nd.Color = Color.Black;
            }

            else if (nd.HasRedUncle() && nd.Parent.Color == Color.Red)
            {
                nd.Parent.Color = Color.Black;
                nd.Parent.Parent.Color = Color.Red;
                nd.Uncle().Color = Color.Black;
                if (nd.Parent.Parent.Parent == null || nd.Parent.Parent.Parent.Color == Color.Red)
                {
                    Balance(nd.Parent.Parent);
                }
            }
            else if (nd.HasBlackUncleOrHasntUncle() && nd.Parent.Color == Color.Red)
            {
                if (nd.IsLeftChild() && nd.Parent.IsLeftChild())
                {
                    var highParent = nd.Parent.Parent.Parent;
                    var b = nd.Parent.Parent;
                    var a = nd.Parent;
                    var bIsRoot = highParent == null;

                    a.RightRotate();

                    a.Color = Color.Black;
                    b.Color = Color.Red;

                    if (bIsRoot)
                        _root = a;
                }
                else if (nd.IsRightChild() && nd.Parent.IsLeftChild())
                {
                    nd.LeftRotate();
                    Balance(nd.Left);
                }
                else if (nd.IsLeftChild() && nd.Parent.IsRightChild())
                {
                    nd.RightRotate();
                    Balance(nd.Right);
                }
                else if (nd.IsRightChild() && nd.Parent.IsRightChild())
                {
                    var highParent = nd.Parent.Parent.Parent;
                    var a = nd.Parent;
                    var b = nd.Parent.Parent;
                    var bIsRoot = highParent == null;

                    a.LeftRotate();
                    a.Color = Color.Black;
                    b.Color = Color.Red;
                    if (bIsRoot)
                        _root = a;
                }
            }
        }

        public void Remove(int value)
        {
            var deleted = GetNode(value);
            var (removed, child) = deleted.Remove();

            if (removed.Color == Color.Black)
            {
                BalanceAfterRemoveNode(child);
            }
        }

        public void BalanceAfterRemoveNode(Node childRemovedNode)
        {
            var siblingDeletedNode = childRemovedNode.Sibling();

            //1
            if (siblingDeletedNode?.Color == Color.Red && childRemovedNode.IsLeftChild())
            {
                var a = childRemovedNode.Parent;
                var b = siblingDeletedNode;
                a.LeftRotate();
                a.Color = Color.Red;
                b.Color = Color.Black;
                BalanceAfterRemoveNode(childRemovedNode);
            }
            //2
            else if (siblingDeletedNode?.Color == Color.Red && childRemovedNode.IsRightChild())
            {
                var a = childRemovedNode.Parent;
                var b = siblingDeletedNode;
                a.RightRotate();
                a.Color = Color.Red;
                b.Color = Color.Black;
                BalanceAfterRemoveNode(childRemovedNode);
            }
            //5
            else if (childRemovedNode.IsLeftChild() && siblingDeletedNode?.Color == Color.Black &&
                     siblingDeletedNode.Left.Color == Color.Red)
            {
                var needRecursiveForParent = childRemovedNode.Parent.Color == Color.Black;
                var b = siblingDeletedNode;
                var c = b.Left;
                c.LeftRotate();
                c.Color = Color.Black;
                b.Color = Color.Red;
                if (needRecursiveForParent)
                {
                    BalanceAfterRemoveNode(childRemovedNode.Parent);
                }
            }
            //6
            else if (childRemovedNode.IsRightChild() && siblingDeletedNode?.Color == Color.Black &&
                     siblingDeletedNode.Right.Color == Color.Red)
            {
                var needRecursiveForParent = childRemovedNode.Parent.Color == Color.Black;
                var b = siblingDeletedNode;
                var c = b.Right;
                c.RightRotate();
                c.Color = Color.Black;
                b.Color = Color.Red;
                if (needRecursiveForParent)
                {
                    BalanceAfterRemoveNode(childRemovedNode.Parent);
                }
            }
            //3
            else if (childRemovedNode.IsLeftChild() && siblingDeletedNode.Color == Color.Black &&
                     siblingDeletedNode.Left.Color == Color.Black && siblingDeletedNode.Right.Color == Color.Black)
            {
                var a = childRemovedNode.Parent;
                var b = a.Right;
                b.Color = Color.Red;
                a.Color = Color.Black;
            }

            //4
            else if (childRemovedNode.IsRightChild() && siblingDeletedNode.Color == Color.Black &&
                     siblingDeletedNode.Left.Color == Color.Black && siblingDeletedNode.Right.Color == Color.Black)
            {
                var a = childRemovedNode.Parent;
                var b = a.Left;
                b.Color = Color.Red;
                a.Color = Color.Black;
            }

            //7
            else if (childRemovedNode.IsLeftChild() && siblingDeletedNode.Color == Color.Black &&
                     siblingDeletedNode.Left.Color == Color.Black && siblingDeletedNode.Right.Color == Color.Red)
            {
                var a = childRemovedNode.Parent;
                var b = a.Right;
                var d = b.Right;
                a.LeftRotate();
                b.Color = a.Color;
                a.Color = Color.Black;
                d.Color = Color.Black;
            }

            //8
            else if (childRemovedNode.IsRightChild() && siblingDeletedNode.Color == Color.Black &&
                     siblingDeletedNode.Left.Color == Color.Red && siblingDeletedNode.Right.Color == Color.Black)
            {
                var a = childRemovedNode.Parent;
                var b = a.Left;
                var d = b.Left;
                a.RightRotate();
                b.Color = a.Color;
                a.Color = Color.Black;
                d.Color = Color.Black;
            }
        }
    }
}