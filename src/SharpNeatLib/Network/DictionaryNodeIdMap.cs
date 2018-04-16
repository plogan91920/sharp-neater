﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SharpNeat.Network
{
    /// <summary>
    /// An INodeIdMap implemented with a dictionary keyed by node ID.
    /// </summary>
    public class DictionaryNodeIdMap : INodeIdMap
    {
        readonly int _inputOutputCount;
        readonly Dictionary<int,int> _hiddenNodeIdxById;

        #region Constructor

        public DictionaryNodeIdMap(
            int inputOutputCount, 
            Dictionary<int,int> hiddenNodeIdxById)
        {
            _inputOutputCount = inputOutputCount;
            _hiddenNodeIdxById = hiddenNodeIdxById;
        }

        #endregion

        #region INodeIdMap

        /// <summary>
        /// Gets the number of mapped node IDs.
        /// </summary>
        public int Count
        {
            get => _inputOutputCount + _hiddenNodeIdxById.Count;
        }

        /// <summary>
        /// Map a given node ID
        /// </summary>
        /// <param name="id">A node ID.</param>
        /// <returns>The mapped to ID.</returns>
        public int Map(int id)
        {
            // Input/output node IDs are fixed.
            if (id < _inputOutputCount)
            {
                return id;
            }
            // Hidden nodes have mappings stored in a dictionary.
            return _hiddenNodeIdxById[id];
        }

        #endregion

        #region Public Static Methods

        public void UpdateNodeIdMap(
            int[] hiddenNodeIdArr,
            int inputOutputCount,
            int[] nodeIdxMap)
        {
            // Reapply the dictionary building logic from DirectedGraphUtils.CompileNodeIdMap(),
            // but with an additional indirection applied to the allocated node indexes.
            for(int i=0, nodeIdx=inputOutputCount; i < hiddenNodeIdArr.Length; i++, nodeIdx++) {
                _hiddenNodeIdxById[hiddenNodeIdArr[i]] = nodeIdxMap[nodeIdx];
            }
        }

        #endregion
    }
}
