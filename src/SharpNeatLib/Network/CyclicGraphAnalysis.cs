﻿using System.Collections.Generic;
using System.Diagnostics;
using Redzen;
using Redzen.Structures;

namespace SharpNeat.Network
{
    /// <summary>
    /// An algorithm for testing for the presence of at least one connectivity cycle within a network.
    /// 
    /// Method.
    /// =======
    /// 1) We loop over all nodes in the network and perform a depth-first traversal from each node. 
    /// (Note. the order that the nodes are traversed does not affect the correctness of the method)
    /// 
    /// 2) Each traversal keeps track of its ancestor nodes (the path to the current node) for each step
    /// in the traversal. Thus if the traversal encounters an ancestor node then a cycle has been detected.
    /// 
    /// 3) A set of visited nodes is maintained. This persists between traversals and allows each traversal 
    /// to avoid traversing into nodes that have already been traversed.
    /// 
    /// Note. We must traverse from each node rather then just e.g. the input nodes, because the network may 
    /// have connectivity dead ends or even isolated connectivity that therefore would not be traversed into 
    /// by following connectivity from the input nodes only, hence we perform a traversal from each node and
    /// attempt to maintain algorithmic efficiency by avoiding traversal into nodes that have already been 
    /// traversed into.
    /// </summary>
    public class CyclicGraphAnalysis
    {
        #region Instance Fields

        /// <summary>
        /// The directed graph being tested.
        /// </summary>
        DirectedGraph _digraph;

        /// <summary>
        /// A bitmap in which each bit represents a node in the graph. 
        /// The set bits represent the set of nodes that are ancestors of the current traversal node.
        /// </summary>
        BoolArray _ancestorNodeBitmap;

        /// <summary>
        /// A bitmap in which each bit represents a node in the graph. 
        /// The set bits represent the set of visited nodes on the current traversal path.
        /// 
        /// This is used to quickly determine if a given path should be traversed or not. 
        /// </summary>
        BoolArray _visitedNodeBitmap;

        #endregion

        #region Construction

        public CyclicGraphAnalysis()
        {
            const int defaultInitialNodeCapacity = 2048;
            _ancestorNodeBitmap = new BoolArray(defaultInitialNodeCapacity);
            _visitedNodeBitmap = new BoolArray(defaultInitialNodeCapacity);
        }

        public CyclicGraphAnalysis(int initialNodeCapacity)
        {
            _ancestorNodeBitmap = new BoolArray(initialNodeCapacity);
            _visitedNodeBitmap = new BoolArray(initialNodeCapacity);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns true if there is at least one connectivity cycle within the provided DirectedGraph.
        /// </summary>
        public bool IsCyclic(DirectedGraph digraph)
        {
            Debug.Assert(null == _digraph, "Re-entrant call on non re-entrant method.");
            _digraph = digraph;

            EnsureNodeCapacity(digraph.TotalNodeCount);

            try
            {
                // Loop over all nodes. Take each one in turn as a traversal root node.
                int nodeCount = _digraph.TotalNodeCount;
                for(int nodeIdx=0; nodeIdx < nodeCount; nodeIdx++)
                {
                    // Determine if the node has already been visited.
                    if(_visitedNodeBitmap[nodeIdx])
                    {   // Already traversed; Skip.
                        continue;
                    }

                    // Traverse into the node. 
                    if(TraverseNode(nodeIdx))
                    {   // Cycle detected.
                        return true;    
                    }
                }

                // No cycles detected.
                return false;
            }
            finally
            {
                Cleanup();
            }
        }

        #endregion

        #region Private Methods

        private void EnsureNodeCapacity(int capacity)
        {
            if(capacity > _ancestorNodeBitmap.Length)
            {
                // For the new capacity, select the lowest power of two that is above the required capacity.
                capacity = MathUtils.CeilingPowerOfTwo(capacity);

                // Allocate new bitmaps with the new capacity.
                _ancestorNodeBitmap = new BoolArray(capacity);
                _visitedNodeBitmap = new BoolArray(capacity);
            }
        }

        private bool TraverseNode(int nodeIdx)
        {
            // Is the node on the current stack of traversal ancestor nodes?
            if(_ancestorNodeBitmap[nodeIdx])
            {   // Connectivity cycle detected.
                return true;
            }

            // Have we already traversed this node?
            if(_visitedNodeBitmap[nodeIdx])
            {   // Already visited; Skip.
                return false;
            }

            // Traverse into the node's targets / children (if it has any)
            int connIdx = _digraph.GetFirstConnectionIndex(nodeIdx);
            if(-1 == connIdx) 
            {   // No cycles on this traversal path.
                return false;
            }

            // Add node to the set of traversal path nodes.
            _ancestorNodeBitmap[nodeIdx] = true;

            // Register the node as having been visited.
            _visitedNodeBitmap[nodeIdx] = true;

            // Traverse into targets.
            int[] srcIdxArr = _digraph.ConnectionIdArrays._sourceIdArr;

            for(; connIdx < srcIdxArr.Length && srcIdxArr[connIdx] == nodeIdx; connIdx++)
            {
                if(TraverseNode(_digraph.ConnectionIdArrays._targetIdArr[connIdx])) 
                {   // Cycle detected.
                    return true;
                }
            }
            
            // Remove node from set of traversal path nodes.
            _ancestorNodeBitmap[nodeIdx] = false;

            // No cycles were detected in the traversal paths from this node.
            return false;
        }

        private void Cleanup()
        {
            _digraph = null;
            _ancestorNodeBitmap.Reset(false);
            _visitedNodeBitmap.Reset(false);
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Returns true if there is at least one connectivity cycle within the provided DirectedGraph.
        /// </summary>
        /// <remarks>
        /// A static version of IsCyclic() that will create and cleanup its own memory allocations for the 
        /// analysis algorithm instead of re-using pre-allocated memory. I.e. this method is a slower version
        /// but is provided for scenarios where the convenience is preferable to speed.
        /// </remarks>
        public static bool IsCyclicStatic(DirectedGraph digraph)
        {
            var cyclicAnalysis = new CyclicGraphAnalysis(digraph.TotalNodeCount);
            return cyclicAnalysis.IsCyclic(digraph);
        }

        #endregion
    }
}
