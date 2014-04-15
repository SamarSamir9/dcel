using System;
using System.Collections.Generic;
using System.Linq;

namespace DCEL
{
    /// <summary>
    /// A node in a red-black tree can have one of the following two colors.
    /// </summary>
    internal enum RBTreeNodeColor : byte
    {
        Red,
        Black
    }

    /// <summary>
    /// Helps to avoid a lot of code repetition.
    /// </summary>
    internal enum RBTreeDirection : byte
    {
        Left,
        Right
    }

    public class RBTreeSetNode<TKey> : RBTreeBaseNode<TKey, RBTreeSetNode<TKey>>
    {
        public RBTreeSetNode()
            : this(default(TKey))
        {
        }

        public RBTreeSetNode(TKey key)
            : base(key)
        {
        }

        public override string ToString()
        {
            return Key.ToString();
        }
    }

    public class RBTreeMapNode<TKey, TValue> : RBTreeBaseNode<TKey, RBTreeMapNode<TKey, TValue>>
    {
        /// <summary>
        /// The value associated with this node.
        /// It does not affect the tree ordering.
        /// </summary>
        public TValue Value { get; set; }

        public RBTreeMapNode()
            : this(default(TKey), default(TValue))
        {
        }

        public RBTreeMapNode(TKey key, TValue value)
            : base(key)
        {
            Value = value;
        }

        public override string ToString()
        {
            return String.Format("({0}, {1})", Key, Value);
        }
    }

    public abstract class RBTreeBaseNode<TKey, TNode>
        where TNode : RBTreeBaseNode<TKey, TNode>
    {
        /// <summary>
        /// The key that determines the position of this node in the tree ordering.
        /// Note that changing the key so that this position should be changed will
        /// corrupt the state of the tree.
        /// </summary>
        public TKey Key { get; set; }

        /// <summary>
        /// Returns the node immediately before this node in the tree ordering, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        public TNode Predecessor
        {
            get
            {
                return (predecessor.IsSentinel ? null : predecessor);
            }
        }

        /// <summary>
        /// Returns the node immediately after this node in the tree ordering, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        public TNode Successor
        {
            get
            {
                return (successor.IsSentinel ? null : successor);
            }
        }

        internal RBTreeNodeColor color;
        internal TNode left;
        internal TNode right;
        internal TNode parent;
        internal TNode predecessor;
        internal TNode successor;

        internal RBTreeBaseNode()
            : this(default(TKey))
        {
        }

        internal RBTreeBaseNode(TKey key)
        {
            Key = key;
            ClearReferences();
        }

        internal void ClearReferences()
        {
            color = RBTreeNodeColor.Red;
            left = null;
            right = null;
            parent = null;
            predecessor = null;
            successor = null;
        }

        internal bool HasParent
        {
            get
            {
                return parent != null;
            }
        }

        internal bool IsSentinel
        {
            get
            {
                return this == left;
            }
        }

        internal bool IsLeftChild
        {
            get
            {
                return this == parent.left;
            }
        }

        internal bool IsRightChild
        {
            get
            {
                return this == parent.right;
            }
        }

        internal RBTreeDirection ChildDirection
        {
            get
            {
                return (IsLeftChild ? RBTreeDirection.Left : RBTreeDirection.Right);
            }
        }

        internal TNode Sibling
        {
            get
            {
                return (IsLeftChild ? parent.right : parent.left);
            }
        }

        internal TNode GetNeighbor(RBTreeDirection direction)
        {
            return (direction == RBTreeDirection.Left ? predecessor : successor);
        }

        internal TNode GetChild(RBTreeDirection direction)
        {
            return (direction == RBTreeDirection.Left ? left : right);
        }

        internal void SetChild(TNode child, RBTreeDirection direction)
        {
            if (direction == RBTreeDirection.Left)
                left = child;
            else
                right = child;
        }
    }

    ///<summary>
    ///A data structure for storing a set of sorted keys.
    ///Supports fast (logarithmic) operations for searching, adding, and removing.
    ///</summary>
    public class RBTreeSet<TKey> : RBTreeBase<TKey, RBTreeSetNode<TKey>>
    {
        /// <summary>
        /// Creates an empty tree that orders keys based on the given comparison delegate.
        /// </summary>
        public RBTreeSet(Comparison<TKey> comparison)
            : base(comparison)
        {
        }

        /// <summary>
        /// Adds the given key to the tree and returns true,
        /// if no node with equal key exists in the tree.
        /// Otherwise, returns false.
        /// </summary>
        public bool Add(TKey key)
        {
            return Add(new RBTreeSetNode<TKey>(key));
        }
    }

    ///<summary>
    ///A data structure for storing a set of key-value pairs sorted by key.
    ///Supports fast (logarithmic) operations for searching, adding, and removing.
    ///</summary>
    public class RBTreeMap<TKey, TValue> : RBTreeBase<TKey, RBTreeMapNode<TKey, TValue>>
    {
        /// <summary>
        /// Creates an empty tree that orders keys based on the given comparison delegate.
        /// </summary>
        public RBTreeMap(Comparison<TKey> comparison)
            : base(comparison)
        {
        }

        /// <summary>
        /// Enumerates in ascending order the values in the tree.
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get
            {
                return GetValues(true);
            }
        }

        /// <summary>
        /// Enumerates the values in the tree.
        /// </summary>
        public IEnumerable<TValue> GetValues(bool ascending)
        {
            return GetNodes(ascending).Select(node => node.Value);
        }

        /// <summary>
        /// The value of the node with minimum key in the tree.
        /// </summary>
        public TValue MinValue
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException("No MinValue exists because tree is empty.");
                return MinNode.Value;
            }
        }

        /// <summary>
        /// The value of the node with maximum key in the tree.
        /// </summary>
        public TValue MaxValue
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException("No MaxValue exists because tree is empty.");
                return MaxNode.Value;
            }
        }

        /// <summary>
        /// Adds the given key-value pair to the tree and returns true,
        /// if no node with equal key exists in the tree.
        /// Otherwise, returns false.
        /// </summary>
        public bool Add(TKey key, TValue value)
        {
            return Add(new RBTreeMapNode<TKey, TValue>(key, value));
        }
    }

    ///<summary>
    ///A data structure for storing a set of items sorted by some key.
    ///Supports fast (logarithmic) operations for searching, adding, and removing.
    ///</summary>
    public abstract class RBTreeBase<TKey, TNode>
        where TNode : RBTreeBaseNode<TKey, TNode>, new()
    {
        /// <summary>
        /// A KeyRangePredicate specifies a range of keys by returning
        ///  o a negative number for keys that are outside of the range and to the left,
        ///  o a positive number for keys that are outside of the range and to the right, and
        ///  o zero for keys that are in the range.
        /// </summary>
        public delegate int KeyRangePredicate(TKey key);

        /// <summary>
        /// The comparison which defines the key ordering.
        /// </summary>
        protected Comparison<TKey> Comparison { get; private set; }

        public int Count { get; private set; }

        public bool IsEmpty
        {
            get
            {
                return Count == 0;
            }
        }

        protected TNode Root { get; private set; }

        /// <summary>
        /// The node which serves as a special kind of "null" value.
        /// It makes many of the algorithms involved much simpler.
        /// </summary>
        protected TNode SentinelNode { get; private set; }

        /// <summary>
        /// Creates an empty tree that orders keys based on the given comparison delegate.
        /// </summary>
        internal RBTreeBase(Comparison<TKey> comparison)
        {
            Comparison = comparison;
            SentinelNode = new TNode();
            Clear();
        }

        /// <summary>
        /// Remove all items from this tree.
        /// </summary>
        public void Clear()
        {
            SentinelNode.color = RBTreeNodeColor.Black;
            SentinelNode.parent = null;
            SentinelNode.left = SentinelNode;
            SentinelNode.right = SentinelNode;
            SentinelNode.predecessor = SentinelNode;
            SentinelNode.successor = SentinelNode;

            Root = SentinelNode;
            Count = 0;
        }

        /// <summary>
        /// Enumerate in ascending order the nodes in the tree.
        /// </summary>
        public IEnumerable<TNode> Nodes
        {
            get
            {
                return GetNodes(true);
            }
        }

        /// <summary>
        /// Enumerate in ascending order the keys in the tree.
        /// </summary>
        public IEnumerable<TKey> Keys
        {
            get
            {
                return GetKeys(true);
            }
        }

        /// <summary>
        /// Enumerate the nodes in the tree.
        /// </summary>
        public IEnumerable<TNode> GetNodes(bool ascending)
        {
            RBTreeDirection direction = (ascending ? RBTreeDirection.Right : RBTreeDirection.Left);
            for (TNode node = SentinelNode.GetNeighbor(direction); node != SentinelNode; node = node.GetNeighbor(direction))
                yield return node;
        }

        /// <summary>
        /// Enumerate the keys in the tree.
        /// </summary>
        public IEnumerable<TKey> GetKeys(bool ascending)
        {
            return GetNodes(ascending).Select(node => node.Key);
        }

        /// <summary>
        /// The node with minimum key in the tree, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        public TNode MinNode
        {
            get
            {
                return SentinelNode.Successor;
            }
        }

        /// <summary>
        /// The node with maximum key in the tree, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        public TNode MaxNode
        {
            get
            {
                return SentinelNode.Predecessor;
            }
        }

        /// <summary>
        /// The minimum key in the tree.
        /// </summary>
        public TKey MinKey
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException("No MinKey exists because tree is empty.");
                return MinNode.Key;
            }
        }

        /// <summary>
        /// The maximum key in the tree.
        /// </summary>
        public TKey MaxKey
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException("No MaxKey exists because tree is empty.");
                return MaxNode.Key;
            }
        }

        /// <summary>
        /// Does the tree contain the given key?
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            return Find(key) != null;
        }

        /// <summary>
        /// Returns the node in the tree with the given key, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        public TNode this[TKey key]
        {
            get
            {
                return Find(key);
            }
        }

        /// <summary>
        /// Returns the node in the tree with the given key, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        public TNode Find(TKey key)
        {
            TNode node = Root;

            while (node != SentinelNode)
            {
                int result = Comparison(key, node.Key);
                if (result == 0)
                    return node;
                else if (result < 0)
                    node = node.left;
                else
                    node = node.right;
            }

            return null;
        }

        /// <summary>
        /// Of the nodes whose keys compare less than or equal to the given key,
        /// returns the one with maximum key, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        public TNode FindPredecessorInclusive(TKey key)
        {
            return FindRangeMax(k => Math.Max(0, Comparison(k, key)));
        }

        /// <summary>
        /// Of the nodes whose keys compare greater than or equal to the given key,
        /// returns the one with minimum key, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        public TNode FindSuccessorInclusive(TKey key)
        {
            return FindRangeMax(k => Math.Min(0, Comparison(k, key)));
        }

        /// <summary>
        /// Of the nodes whose keys compare strictly less than the given key,
        /// returns the one with maximum key, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        public TNode FindPredecessorExclusive(TKey key)
        {
            TNode node = FindPredecessorInclusive(key);
            return (node != null ? node.Predecessor : null);
        }

        /// <summary>
        /// Of the nodes whose keys compare strictly greater than the given key,
        /// returns the one with minimum key, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        public TNode FindSuccessorExclusive(TKey key)
        {
            TNode node = FindSuccessorInclusive(key);
            return (node != null ? node.Successor : null);
        }

        /// <summary>
        /// Of the nodes whose keys satisfy the given predicate,
        /// returns the one with maximum key, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        /// <param name="predicate">Must not conflict with the comparison delegate.</param>
        public TNode FindMin(Predicate<TKey> predicate)
        {
            return FindExtremum(predicate, RBTreeDirection.Left);
        }

        /// <summary>
        /// Of the nodes whose keys satisfy the given predicate,
        /// returns the one with minimum key, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        /// <param name="predicate">Must not conflict with the comparison delegate.</param>
        public TNode FindMax(Predicate<TKey> predicate)
        {
            return FindExtremum(predicate, RBTreeDirection.Right);
        }

        private TNode FindExtremum(Predicate<TKey> predicate, RBTreeDirection direction)
        {
            RBTreeDirection oppositeDirection = OppositeDirection(direction);

            TNode extremum = null;
            TNode node = Root;
            while (node != SentinelNode)
            {
                if (predicate(node.Key))
                {
                    extremum = node;
                    node = node.GetChild(direction);
                }
                else
                    node = node.GetChild(oppositeDirection);
            }
            return extremum;
        }

        /// <summary>
        /// Of the nodes whose keys satisfy the given range predicate,
        /// returns the one with minimum key, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        /// <param name="rangePredicate">Must not conflict with the comparison delegate.</param>
        public TNode FindRangeMin(KeyRangePredicate rangePredicate)
        {
            return FindRangeExtremum(rangePredicate, RBTreeDirection.Left);
        }

        /// <summary>
        /// Of the nodes whose keys satisfy the given range predicate,
        /// returns the one with maximum key, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        /// <param name="rangePredicate">Must not conflict with the comparison delegate.</param>
        public TNode FindRangeMax(KeyRangePredicate rangePredicate)
        {
            return FindRangeExtremum(rangePredicate, RBTreeDirection.Right);
        }

        private TNode FindRangeExtremum(KeyRangePredicate rangePredicate, RBTreeDirection direction)
        {
            TNode extremum = null;
            TNode node = Root;
            while (node != SentinelNode)
            {
                int result = rangePredicate(node.Key);
                if (result == 0)
                {
                    extremum = node;
                    node = node.GetChild(direction);
                }
                else if (result < 0)
                    node = node.right;
                else
                    node = node.left;
            }
            return extremum;
        }

        /// <summary>
        /// Enumerates the nodes whose keys satisfy the given range predicate.
        /// </summary>
        /// <param name="rangePredicate">Must not conflict with the comparison delegate.</param>
        public IEnumerable<TNode> FindRange(KeyRangePredicate rangePredicate)
        {
            TNode first = FindRangeMin(rangePredicate);
            if (first == null)
                yield break;
            TNode last = FindRangeMax(rangePredicate);
            last = last.successor;
            for (TNode node = first; node != last; node = node.successor)
                yield return node;
        }

        /// <summary>
        /// Adds the given node to the tree and returns true, if no node with equal key exists in the tree.
        /// Otherwise, returns false.
        /// </summary>
        public bool Add(TNode node)
        {
            TNode foundNode;
            return Add(node, out foundNode);
        }

        /// <summary>
        /// Adds the given node to the tree and returns true, if no node with equal key exists in the tree.
        /// Otherwise, sets node to the existing node with equal key and returns false.
        /// </summary>
        public bool Add(ref TNode node)
        {
            return Add(node, out node);
        }

        /// <summary>
        /// Adds the given node to the tree, sets foundNode to node, and returns true, if no node
        /// with equal key exists in the tree.
        /// Otherwise, sets foundNode to the existing node with equal key and returns false.
        /// </summary>
        public bool Add(TNode node, out TNode foundNode)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            TNode parent = null;
            {
                TNode curr = Root;
                while (curr != SentinelNode)
                {
                    int result = Comparison(node.Key, curr.Key);
                    if (result == 0)
                    {
                        foundNode = curr;
                        return false;
                    }

                    parent = curr;

                    if (result < 0)
                        curr = curr.left;
                    else
                        curr = curr.right;
                }
            }

            node.parent = parent;
            node.left = SentinelNode;
            node.right = SentinelNode;

            if (node.HasParent)
            {
                if (Comparison(node.Key, node.parent.Key) < 0)
                    node.parent.left = node;
                else
                    node.parent.right = node;
            }
            else
                Root = node;

            RestorePropertiesAfterAdd(node);
            Count++;

            TNode predecessor = TraverseToFindNeighbor(node, RBTreeDirection.Left);
            TNode successor = TraverseToFindNeighbor(node, RBTreeDirection.Right);
            predecessor.successor = node;
            node.predecessor = predecessor;
            node.successor = successor;
            successor.predecessor = node;

            foundNode = node;
            return true;
        }

        /// <summary>
        /// Removes the MinNode from the tree and returns it, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        public TNode RemoveMin()
        {
            TNode node = MinNode;
            if (node != null)
                Remove(node);
            return node;
        }

        /// <summary>
        /// Removes the MaxNode from the tree and returns it, if it exists.
        /// Otherwise, returns null.
        /// </summary>
        public TNode RemoveMax()
        {
            TNode node = MaxNode;
            if (node != null)
                Remove(node);
            return node;
        }

        /// <summary>
        /// Removes the node with given key from the tree and returns it, if such a node exists.
        /// Otherwise, returns null.
        /// </summary>
        public TNode Remove(TKey key)
        {
            TNode node = Root;
            while (node != SentinelNode)
            {
                int result = Comparison(key, node.Key);
                if (result == 0)
                    break;
                if (result < 0)
                    node = node.left;
                else
                    node = node.right;
            }

            if (node == SentinelNode)
                return null;

            Remove(node);

            return node;
        }

        /// <summary>
        /// Removes the given node from the tree.
        /// </summary>
        public void Remove(TNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            //replacement takes the place of node - or is node if node has no children
            TNode replacement;
            if (node.left == SentinelNode || node.right == SentinelNode)
                replacement = node;
            else
                replacement = node.successor;

            //replacementChild is either the single child of replacement or SentinelNode
            TNode replacementChild = (replacement.left != SentinelNode ? replacement.left : replacement.right);

            replacementChild.parent = replacement.parent;
            if (replacement.HasParent)
                replacement.parent.SetChild(replacementChild, replacement.ChildDirection);
            else
                Root = replacementChild;

            if (replacement != node)
            {
                node.successor = replacement.successor;
                replacement.successor.predecessor = node;
            }
            else
            {
                TNode predecessor = node.predecessor;
                TNode successor = node.successor;
                predecessor.successor = successor;
                successor.predecessor = predecessor;
            }

            if (replacement.color == RBTreeNodeColor.Black)
                RestorePropertiesAfterRemove(replacementChild);

            //Before, I was copying data from replacement to node.
            //Problem! External references to node become corrupted!
            //Now instead steal references from node to replacement.
            StealReferences(node, replacement);
            Count--;
        }

        private void StealReferences(TNode from, TNode to)
        {
            if (from == to)
                return;

            if (Root == from)
                Root = to;

            if (from.HasParent)
                from.parent.SetChild(to, from.ChildDirection);
            to.parent = from.parent;

            if (from.left.parent == from)
                from.left.parent = to;
            to.left = from.left;

            if (from.right.parent == from)
                from.right.parent = to;
            to.right = from.right;

            if (from.predecessor.successor == from)
                from.predecessor.successor = to;
            to.predecessor = from.predecessor;

            if (from.successor.predecessor == from)
                from.successor.predecessor = to;
            to.successor = from.successor;

            to.color = from.color;

            from.ClearReferences();
        }

        private void RestorePropertiesAfterAdd(TNode node)
        {
            while (node != Root && node.parent.color == RBTreeNodeColor.Red)
            {
                TNode uncle = node.parent.Sibling;
                if (uncle != null && uncle.color == RBTreeNodeColor.Red)
                {
                    TNode parent = node.parent;
                    TNode grandparent = parent.parent;

                    grandparent.color = RBTreeNodeColor.Red;
                    parent.color = RBTreeNodeColor.Black;
                    uncle.color = RBTreeNodeColor.Black;

                    node = grandparent;
                }
                else
                {
                    RBTreeDirection grandparentToParent = node.parent.ChildDirection;
                    RBTreeDirection parentToNode = node.ChildDirection;
                    if (grandparentToParent != parentToNode)
                    {
                        node = node.parent;
                        Rotate(node, grandparentToParent);
                    }

                    TNode parent = node.parent;
                    TNode grandparent = parent.parent;

                    grandparent.color = RBTreeNodeColor.Red;
                    parent.color = RBTreeNodeColor.Black;
                    Rotate(grandparent, OppositeDirection(grandparentToParent));
                }
            }

            Root.color = RBTreeNodeColor.Black;
        }

        private void RestorePropertiesAfterRemove(TNode node)
        {
            while (node != Root && node.color == RBTreeNodeColor.Black)
            {
                RBTreeDirection direction = node.ChildDirection;
                RBTreeDirection oppositeDirection = OppositeDirection(direction);

                TNode sibling = node.Sibling;
                if (sibling.color == RBTreeNodeColor.Red)
                {
                    sibling.color = RBTreeNodeColor.Black;
                    node.parent.color = RBTreeNodeColor.Red;
                    Rotate(node.parent, direction);
                    sibling = node.parent.GetChild(oppositeDirection);
                }

                if ((sibling.left.color == RBTreeNodeColor.Black)
                    && (sibling.right.color == RBTreeNodeColor.Black))
                {
                    sibling.color = RBTreeNodeColor.Red;
                    node = node.parent;
                }
                else
                {
                    if (sibling.GetChild(oppositeDirection).color == RBTreeNodeColor.Black)
                    {
                        sibling.GetChild(direction).color = RBTreeNodeColor.Black;
                        sibling.color = RBTreeNodeColor.Red;
                        Rotate(sibling, oppositeDirection);
                        sibling = node.parent.GetChild(oppositeDirection);
                    }

                    sibling.color = node.parent.color;
                    node.parent.color = RBTreeNodeColor.Black;
                    sibling.GetChild(oppositeDirection).color = RBTreeNodeColor.Black;
                    Rotate(node.parent, direction);
                    node = Root;
                }
            }

            node.color = RBTreeNodeColor.Black;
        }

        private void Rotate(TNode node, RBTreeDirection direction)
        {
            RBTreeDirection oppositeDirection = OppositeDirection(direction);

            //replacement will rotate to where node was, pushing node down
            TNode replacement = node.GetChild(oppositeDirection);
            node.SetChild(replacement.GetChild(direction), oppositeDirection);

            if (replacement.GetChild(direction) != SentinelNode)
                replacement.GetChild(direction).parent = node;

            if (replacement != SentinelNode)
                replacement.parent = node.parent;

            if (node.HasParent)
                node.parent.SetChild(replacement, node.ChildDirection);
            else
                Root = replacement;

            replacement.SetChild(node, direction);
            if (node != SentinelNode)
                node.parent = replacement;
        }

        private TNode TraverseToFindNeighbor(TNode node, RBTreeDirection direction)
        {
            if (node.GetChild(direction) == SentinelNode)
            {
                while (node.HasParent && node.ChildDirection == direction)
                    node = node.parent;
                return (node.HasParent ? node.parent : SentinelNode);
            }
            else
            {
                node = node.GetChild(direction);
                RBTreeDirection oppositeDirection = OppositeDirection(direction);
                while (node.GetChild(oppositeDirection) != SentinelNode)
                    node = node.GetChild(oppositeDirection);
                return node;
            }
        }

        private static RBTreeDirection OppositeDirection(RBTreeDirection dir)
        {
            return (dir == RBTreeDirection.Left ? RBTreeDirection.Right : RBTreeDirection.Left);
        }

        public override string ToString()
        {
            return String.Format("Count = {0}", Count);
        }
    }
}
