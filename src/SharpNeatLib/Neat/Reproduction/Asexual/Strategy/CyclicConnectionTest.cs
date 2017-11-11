﻿using System.Collections.Generic;
using Redzen.Collections;
using SharpNeat.Neat.Genome;
using SharpNeat.Network;

namespace SharpNeat.Neat.Reproduction.Asexual.Strategy
{
    /// <summary>
    /// For testing if a proposed new connection on a directed acyclic graph would form a cycle.
    /// </summary>
    /// <remarks>
    /// Each instance of this class allocates a stack and a hashset for use by the traversal algorithm, and these
    /// are cleared and re-used for each call to IsConnectionCyclic(). This avoids memory re-allocation and garbage
    /// collection overhead, but the side effect is that IsConnectionCyclic() is not thread safe. However, A thread 
    /// safe static method IsCyclicStatic() is provided for convenience, but this will have the additional memory
    /// and GC overhead associated with each call to it.
    /// 
    /// This class is optimized for speed and efficiency and as such is tightly coupled with the connection gene 
    /// array data structure, and is perhaps not as easy to read/understand as a traditional depth first traversal 
    /// using function recursion. However this is essentially a depth first algorithm but with its own stack instead 
    /// of using the call stack, and a each stack frame is just an index into the connection array.
    /// 
    /// The idea is that an entry on the stack represent both a node that is being traversed (given by the connection's
    /// source node) and an iterator over that node's target nodes (given by the connection index, which works because 
    /// connections are sorted by sourceId).
    /// 
    /// The main optimizations then are:
    /// 1) No method call overhead from recursive method calls.
    /// 2) Each stack frame is a single int32, which keeps the max size of the stack for any given traversal at a minimum.
    /// 3) The stack and a visitedNodes hashmap are allocated for each class instance and are cleared and re-used for each 
    /// call to IsConnectionCyclic(), therefore avoiding memory allocation and garbage collection overhead.
    /// 
    /// Using our own stack also avoids any potential for a stack overflow on very deep graphs, which could occur if using 
    /// method call recursion.
    /// </remarks>
    /// <typeparam name="T">Connection weight type.</typeparam>
    public class CyclicConnectionTest<T> where T : struct
    {
        #region Instance Fields

        /// <summary>
        /// The graph traversal stack, as required by a depth first graph traversal algorithm.
        /// Each stack entry is an index into a connection array, representing iteration over the connections 
        /// for one source node.
        /// </summary>
        IntStack _traversalStack = new IntStack(16);    
        /// <summary>
        /// Maintain a set of nodes that have been visited, this allows us to avoid unnecessary
        /// re-traversal of nodes.
        /// </summary>
        HashSet<int> _visitedNodes = new HashSet<int>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Tests if the proposed new connection newConn would form a cycle if added to the existing directed
        /// acyclic graph connArr.
        /// </summary>
        /// <param name="connArr">A set of connections that describe a directed acyclic graph.</param>
        /// <param name="newConn">A proposed new connection to add to the graph.</param>
        public bool IsConnectionCyclic(ConnectionGene<T>[] connArr, DirectedConnection newConn)
        {
            // Ensure cleanup occurs before we return so that we can guarantee the class instance is ready for 
            // re-use on the next call.
            try {
                return IsConnectionCyclicInner(connArr, newConn);
            }
            finally 
            {
                _traversalStack.Clear();
                _visitedNodes.Clear();
            }
        }

        #endregion

        #region Private Methods


        /// <summary>
        /// Tests if the proposed new connection newConn would form a cycle if added to the existing directed
        /// acyclic graph connArr.
        /// </summary>
        /// <param name="connArr">A set of connections that describe a directed acyclic graph.</param>
        /// <param name="newConn">A proposed new connection to add to the graph.</param>
        /// <remarks>
        /// This search uses an explicitly created stack instead of using function recursion, the reasons for this are:
        /// 1) Avoids the possibility of a call stack overflow when handling very deep graphs.
        /// 2) Avoids method call overhead.
        /// 3) Allows for an optimal/compact stack frame (a single integer i.e. 4 bytes).
        /// </remarks>
        private bool IsConnectionCyclicInner(ConnectionGene<T>[] connArr, DirectedConnection newConn)
        {
            // Test if the new connection is pointing to itself.
            if(newConn.SourceId == newConn.TargetId) {
                return true;
            }

            // Initialise traversal.
            // Notes. 
            // We traverse forwards starting at the new connection's target node. If the new connection's source node is encountered
            // during traversal then the new connection would form a cycle in the graph as a whole.
            //
            // However, we already asserted that newConn.SourceId != newConn.TargetId (above), therefore we add newConn.TargetId to
            // visitedNodes to indicate that it has already been visited, and place its target nodes into the stack of nodes to traverse
            // (traversalStack). Essentially we're starting the traversal one level on to avoid a redundant test (i.e. for slightly 
            // improved performance).

            // The 'terminal' node ID, i.e. if traversal reaches this node then newConn would form a cycle and we stop/terminate traversal.
            int terminalNodeId = newConn.SourceId;

            // Init the current traversal node, starting at the newConn.TargetId.
            int currNodeId = newConn.TargetId;
            
            // Search outgoing connections from currNodeId.
            int connStartIdx = ConnectionGeneUtils.GetConnectionIndexBySourceNodeId(connArr, currNodeId);
            if(connStartIdx < 0)
            {   // The current node has no outgoing connections, therefore newConn does not form a cycle.
                return false;
            }

            // Push connIdx onto the stack.
            _traversalStack.Push(connStartIdx);

            // Add the current node to the set of visited nodes.
            _visitedNodes.Add(currNodeId);

            // While there are entries on the stack.
            while(0 != _traversalStack.Count)
            {
                // Get the connection at the top of the stack.
                int connIdx = _traversalStack.Peek();
                currNodeId = connArr[connIdx].SourceId;

                // Find the next connection from the current node that we can traverse down, if any.
                int nextConnIdx = -1;
                for(int i = connIdx+1; i < connArr.Length && connArr[i].SourceId == currNodeId; i++)
                {
                    if(!_visitedNodes.Contains(connArr[i].TargetId))
                    {
                        // We have found the next connection to traverse.
                        nextConnIdx = i;
                        break;
                    }
                }

                // Move/iterate to the next connection for the current node, if any.
                if(-1 != nextConnIdx) 
                {   // We have the next connection to traverse down for the current node; update the current node's
                    // entry on the top of the stack to point to it.
                    _traversalStack.Poke(nextConnIdx);
                }
                else
                {   // No more connections for the current node; remove its entry from the top of the stack.
                    _traversalStack.Pop();
                }

                // Test if the next traversal child node has already been visited.
                int childNodeId = connArr[connIdx].TargetId;
                if(_visitedNodes.Contains(childNodeId)) {
                    continue;
                }

                // Test if the connection target is the terminal node.
                if(childNodeId == terminalNodeId) {
                    return true;
                }

                // We're about to traverse into childNodeId, so mark it as visited to prevent re-traversal.
                _visitedNodes.Add(childNodeId);

                // Search for outgoing connections from childNodeId.
                connStartIdx = ConnectionGeneUtils.GetConnectionIndexBySourceNodeId(connArr, childNodeId);
                if(connStartIdx >= 0)
                {   // childNodeId has outgoing connections from it. Push the first connection onto the stack.
                    _traversalStack.Push(connStartIdx);    
                }                
            }

            // Traversal has completed without visiting the terminal node, therefore the new connection
            // does not form a cycle in the graph.
            return false;
        }

        #endregion
    }
}
