﻿using System.Collections.Generic;
using SharpNeat.Graphs;
using SharpNeat.Graphs.Acyclic;
using Xunit;
using static SharpNeat.Tests.Graph.NetworkUtils;

namespace SharpNeat.Tests.Graph
{
    public class WeightedDirectedGraphAcyclicBuilderTests
    {
        #region Test Methods

        [Fact]
        public void SimpleAcyclic()
        {
            // Simple acyclic graph.
            var connList = new List<WeightedDirectedConnection<double>>
            {
                new WeightedDirectedConnection<double>(0, 3, 0.0),
                new WeightedDirectedConnection<double>(1, 3, 1.0),
                new WeightedDirectedConnection<double>(2, 3, 2.0),
                new WeightedDirectedConnection<double>(2, 4, 3.0)
            };

            // Create graph.
            var digraph = WeightedDirectedGraphAcyclicBuilder<double>.Create(connList, 3, 2);

            // The graph should be unchanged from the input connections.
            CompareConnectionLists(connList, digraph.ConnectionIdArrays, digraph.WeightArray);

            // Check the node count.
            Assert.Equal(5, digraph.TotalNodeCount);
        }

        [Fact]
        public void DepthNodeReorderTest()
        {
            // Define graph connections.
            var connList = new List<WeightedDirectedConnection<double>>
            {
                new WeightedDirectedConnection<double>(0, 4, 0.0),
                new WeightedDirectedConnection<double>(4, 5, 1.0),
                new WeightedDirectedConnection<double>(5, 2, 2.0),
                new WeightedDirectedConnection<double>(1, 2, 3.0),
                new WeightedDirectedConnection<double>(2, 3, 4.0)
            };

            // Create graph.
            connList.Sort(WeightedDirectedConnectionComparer<double>.Default);
            var digraph = WeightedDirectedGraphAcyclicBuilder<double>.Create(connList, 2, 2);

            // The nodes should have IDs allocated based on depth, i.e. the layer they are in.
            // And connections should be ordered by source node ID.
            var connListExpected = new List<WeightedDirectedConnection<double>>
            {
                new WeightedDirectedConnection<double>(0, 2, 0.0),
                new WeightedDirectedConnection<double>(1, 4, 3.0),
                new WeightedDirectedConnection<double>(2, 3, 1.0),
                new WeightedDirectedConnection<double>(3, 4, 2.0),
                new WeightedDirectedConnection<double>(4, 5, 4.0)
            };

            // Compare actual and expected connections.
            CompareConnectionLists(connListExpected, digraph.ConnectionIdArrays, digraph.WeightArray);

            // Test layer info.
            LayerInfo[] layerArrExpected = new LayerInfo[5];
            layerArrExpected[0] = new LayerInfo(2, 2);
            layerArrExpected[1] = new LayerInfo(3, 3);
            layerArrExpected[2] = new LayerInfo(4, 4);
            layerArrExpected[3] = new LayerInfo(5, 5);
            layerArrExpected[4] = new LayerInfo(6, 5);
            Assert.Equal(5, digraph.LayerArray.Length);

            // Check the node count.
            Assert.Equal(6, digraph.TotalNodeCount);
        }

        #endregion
    }
}
