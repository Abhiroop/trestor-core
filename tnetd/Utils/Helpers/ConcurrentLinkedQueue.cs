using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace TNetD.Helpers
{
    /*
     * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
     *
     * This code is free software; you can redistribute it and/or modify it
     * under the terms of the GNU General Public License version 2 only, as
     * published by the Free Software Foundation.  Sun designates this
     * particular file as subject to the "Classpath" exception as provided
     * by Sun in the LICENSE file that accompanied this code.
     *
     * This code is distributed in the hope that it will be useful, but WITHOUT
     * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
     * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
     * version 2 for more details (a copy is included in the LICENSE file that
     * accompanied this code).
     *
     * You should have received a copy of the GNU General Public License version
     * 2 along with this work; if not, write to the Free Software Foundation,
     * Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA.
     *
     * Please contact Sun Microsystems, Inc., 4150 Network Circle, Santa Clara,
     * CA 95054 USA or visit www.sun.com if you need additional information or
     * have any questions.

     * This file is available under and governed by the GNU General Public
     * License version 2 only, as published by the Free Software Foundation.
     * However, the following notice accompanied the original version of this
     * file:
     *
     * Written by Doug Lea with assistance from members of JCP JSR-166
     * Expert Group and released to the public domain, as explained at
     * http://creativecommons.org/licenses/publicdomain
     * 
     * Ported from Java by Aizikovich Evgeni
     * http://www.evgeni.name
     */


    /// <summary>
    /// This is a straight adaptation of Michael and Scott algorithm.
    /// For explanation, read the paper.  The only (minor) algorithmic
    /// difference is that this version supports lazy deletion of
    /// internal nodes (method remove(Object)) -- remove CAS'es item
    /// fields to null. The normal queue operations unlink but then
    /// pass over nodes with null item fields. Similarly, iteration
    /// methods ignore those with nulls.
    /// </summary>
    /// <typeparam name="T">The type of the queue items.</typeparam>
    [Serializable]
    public sealed class ConcurrentLinkedQueue<T> : ISerializable where T : class
    {
        private class Node
        {
            private volatile T item;
            private volatile Node next;

            public Node(T x)
            {
                item = x;
            }

            public Node(T x, Node n)
            {
                item = x;
                next = n;
            }

            public bool casNext(Node cmp, Node val)
            {
                return (Interlocked.CompareExchange<Node>(ref next, val, cmp) != next);
            }

            public bool casItem(T cmp, T val)
            {
                return (Interlocked.CompareExchange<T>(ref item, val, cmp) != item);
            }


            /// <summary>
            /// Gets or sets the item.
            /// </summary>
            /// <value>The item.</value>
            public T Item
            {
                get
                {
                    return item;
                }
                set
                {
                    Interlocked.Exchange<T>(ref item, value);
                }
            }

            public Node Next
            {
                get
                {
                    return next;
                }
                set
                {
                    Interlocked.Exchange<Node>(ref next, value);
                }
            }
        }


        private volatile Node head = null;
        private volatile Node tail = null;

        /// <summary>
        /// Creates a <see cref="ConcurrentQueue"/> that is initially empty.
        /// </summary>
        public ConcurrentLinkedQueue()
        {
            head = new Node(null, null);
            tail = head;
        }

        /// <summary>
        /// Creates <see cref="ConcurrentQueue"/> and fills it by items from privoded collection.
        /// </summary>
        /// <param name="c">Collection with items to populate <see cref="ConcurrentQueue"/></param>
        public ConcurrentLinkedQueue(List<T> c)
            : this()
        {
            List<T>.Enumerator enumerator = c.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Add(enumerator.Current);
            }
        }

        private bool casTail(Node cmp, Node val)
        {
            return (Interlocked.CompareExchange<Node>(ref tail, val, cmp) != tail);
        }

        private bool casHead(Node cmp, Node val)
        {
            return (Interlocked.CompareExchange<Node>(ref head, val, cmp) != head);
        }


        /// <summary>
        /// Adds the specified element to the tail of this queue.
        /// </summary>
        /// <param name="o">Item to add.</param>
        /// <returns>True when operetion succeed.</returns>
        /// <exception cref="NullReferenceException">Thrown when o is null.</exception>
        public bool Add(T o)
        {
            if (o == null)
                throw new NullReferenceException();

            Node n = new Node(o, null);
            for (;;)
            {
                Node t = tail;
                Node s = t.Next;
                if (t == tail)
                {
                    if (s == null)
                    {
                        if (t.casNext(s, n))
                        {
                            casTail(t, n);
                            return true;
                        }
                    }
                    else
                    {
                        casTail(t, s);
                    }
                }
            }
        }


        /// <summary>
        /// Gets the first item from the top of queue, removing it from queue.
        /// </summary>
        /// <returns>Item from the top or null if queue is empty.</returns>
        public T Poll()
        {
            for (;;)
            {
                Node h = head;
                Node t = tail;
                Node first = h.Next;
                if (h == head)
                {
                    if (h == t)
                    {
                        if (first == null)
                            return null;
                        else
                            casTail(t, first);
                    }
                    else
                        if (casHead(h, first))
                    {
                        T item = first.Item;
                        if (item != null)
                        {
                            first.Item = null;
                            return item;
                        }
                        // else skip over deleted item, continue loop,
                    }
                }
            }
        }

        /// <summary>
        /// Gets the first item from the top of queue, without removing it from queue.
        /// </summary>
        /// <returns></returns>
        public T Peek()
        { // same as poll except don't remove item
            for (;;)
            {
                Node h = head;
                Node t = tail;
                Node first = h.Next;
                if (h == head)
                {
                    if (h == t)
                    {
                        if (first == null)
                            return null;
                        else
                            casTail(t, first);
                    }
                    else
                    {
                        T item = first.Item;
                        if (item != null)
                            return item;
                        else // remove deleted node and continue
                            casHead(h, first);
                    }
                }
            }
        }

        /**
         * Returns the first actual (non-header) node on list.  This is yet
         * another variant of poll/peek; here returning out the first
         * node, not element (so we cannot collapse with peek() without
         * introducing race.)
         */
        Node first()
        {
            for (;;)
            {
                Node h = head;
                Node t = tail;
                Node first = h.Next;
                if (h == head)
                {
                    if (h == t)
                    {
                        if (first == null)
                            return null;
                        else
                            casTail(t, first);
                    }
                    else
                    {
                        if (first.Item != null)
                            return first;
                        else // remove deleted node and continue
                            casHead(h, first);
                    }
                }
            }
        }

        /// <summary>
        /// Gets indication if queue is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return first() == null;
            }
        }


        /// <summary>
        /// Returns the number of elements in this queue.  If this queue
        /// contains more than <tt>Integer.MAX_VALUE</tt> elements, returns int.MaxValue.
        /// Beware that, unlike in most collections, this method is
        /// <em>NOT</em> a constant-time operation. Because of the
        /// asynchronous nature of these queues, determining the current
        /// number of elements requires an O(n) traversal.
        /// </summary>
        /// <returns>Returns the number of elements in this queue.</returns>
        public int Size()
        {
            int count = 0;
            for (Node p = first(); p != null; p = p.Next)
            {
                if (p.Item != null)
                {
                    // Collections.size() spec says to max out
                    if (++count == int.MaxValue)
                        break;
                }
            }
            return count;
        }

        /// <summary>
        /// Determines if specified item is in the queue.
        /// </summary>
        /// <param name="o">The item to look for.</param>
        /// <returns>True if specified item is in the queue, otherwise false.</returns>
        public bool Contains(T o)
        {
            if (o == null)
                return false;

            for (Node p = first(); p != null; p = p.Next)
            {
                T item = p.Item;
                if (item != null && o.Equals(item))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Removes specified item from queue.
        /// </summary>
        /// <param name="o">Item to remove.</param>
        /// <returns>Returns true if operation succeeded, otherwise false</returns>
        public bool Remove(T o)
        {
            if (o == null)
                return false;

            for (Node p = first(); p != null; p = p.Next)
            {
                T item = p.Item;
                if (item != null && o.Equals(item) && p.casItem(item, null))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Creates new empty array, and populates it by queue items.
        /// </summary>
        /// <returns>Array populated by items from queue.</returns>
        public T[] ToArray()
        {
            // Use ArrayList to deal with resizing.
            List<T> list = new List<T>();
            for (Node p = first(); p != null; p = p.Next)
            {
                T item = p.Item;
                if (item != null)
                    list.Add(item);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Populates specified array by items from the queue. If there is not enough space in provided array, new array is created.
        /// </summary>
        /// <param name="a">The array to populate with queue items.</param>
        /// <returns>Array populated by items from queue.</returns>
        public T[] ToArray(T[] a)
        {
            // try to use sent-in array
            int k = 0;
            Node p;
            for (p = first(); p != null && k < a.Length; p = p.Next)
            {
                T item = p.Item;
                if (item != null)
                    a[k++] = item;
            }
            if (p == null)
            {
                if (k < a.Length)
                    a[k] = null;
                return a;
            }

            // If won't fit, use ArrayList version
            List<T> al = new List<T>();
            for (Node q = first(); q != null; q = q.Next)
            {
                T item = q.Item;
                if (item != null)
                    al.Add(item);
            }
            return al.ToArray();
        }

        /**
         * Returns an iterator over the elements in this queue in proper sequence.
         * The returned iterator is a "weakly consistent" iterator that
         * will never throw {@link java.util.ConcurrentModificationException},
         * and guarantees to traverse elements as they existed upon
         * construction of the iterator, and may (but is not guaranteed to)
         * reflect any modifications subsequent to construction.
         *
         * @return an iterator over the elements in this queue in proper sequence.
         */
        ////public Iterator<E> iterator()
        ////{
        ////    return new Itr();
        ////}

        private class Itr : IEnumerator<T>
        {
            /**
             * Next node to return item for.
             */
            private Node nextNode = null;

            /**
             * nextItem holds on to item fields because once we claim
             * that an element exists in hasNext(), we must return it in
             * the following next() call even if it was in the process of
             * being removed when hasNext() was called.
             **/
            private T nextItem;

            /**
             * Node of the last returned item, to support remove.
             */
            private Node lastRet;

            private Node _firstNode = null;

            public Itr(Node firstNode)
            {
                _firstNode = firstNode;

                advance();
            }

            /**
             * Moves to next valid node and returns item to return for
             * next(), or null if no such.
             */
            private T advance()
            {
                lastRet = nextNode;
                T x = nextItem;

                Node p = (nextNode == null) ? _firstNode : nextNode.Next;
                for (;;)
                {
                    if (p == null)
                    {
                        nextNode = null;
                        nextItem = null;
                        return x;
                    }
                    T item = p.Item;
                    if (item != null)
                    {
                        nextNode = p;
                        nextItem = item;
                        return x;
                    }
                    else // skip over nulls
                        p = p.Next;
                }
            }

            public bool hasNext()
            {
                return nextNode != null;
            }

            public T next()
            {
                if (nextNode == null)
                    throw new IndexOutOfRangeException();
                return advance();
            }

            public void remove()
            {
                Node l = lastRet;
                if (l == null)
                    throw new InvalidOperationException();
                // rely on a future traversal to relink.
                l.Item = null;
                lastRet = null;
            }

            #region IEnumerator<T> Members

            public T Current
            {
                get
                {
                    return lastRet.Item;
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (object)lastRet;
                }
            }

            public bool MoveNext()
            {
                if (!hasNext())
                    return false;

                advance();
                return true;
            }

            public void Reset()
            {
                nextNode = null;
                advance();
            }

            #endregion
        }

        #region ISerializable Members

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            int itemIndex = 0; //used to provide unique name in stream

            for (Node p = first(); p != null; p = p.Next)
            {
                T item = p.Item;
                if (item != null)
                    info.AddValue(itemIndex.ToString(), item, typeof(T));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentLinkedQueue&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        private ConcurrentLinkedQueue(SerializationInfo info, StreamingContext context)
            : this()
        {
            if (info.MemberCount == 0)
                return;

            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                T item = (T)enumerator.Value;

                if (item == null)
                    break;
                else
                    Add(item);
            }
        }

        #endregion
    }
}
