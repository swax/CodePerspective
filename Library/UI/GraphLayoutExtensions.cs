using System;
using System.Collections.Generic;
using System.Linq;

namespace XLibrary
{
    /// <summary>
    /// Extension methods to help with performant graph layout operations
    /// </summary>
    public static class GraphLayoutExtensions
    {
        /// <summary>
        /// Updates the graph layout incrementally based on node changes
        /// </summary>
        public static void UpdateLayoutForChangedNodes(this Graph graph, IEnumerable<int> changedNodeIds)
        {
            // Mark nodes as dirty to trigger incremental layout
            GraphLayout.MarkNodesForUpdate(changedNodeIds);
            
            // Apply layout with incremental updates
            GraphLayout.ApplySugiyamaLayout(graph);
        }
        
        /// <summary>
        /// Adapts layout complexity based on available frame time
        /// </summary>
        public static void AdaptLayoutComplexity(this Graph graph, float availableMs)
        {
            // Adjust layout complexity based on available processing time
            if (availableMs < 5) // Very limited time
            {
                GraphLayout.Config.MaxSweepCount = 2;
                GraphLayout.Config.MinSweepCount = 1;
                GraphLayout.Config.UseParallelProcessing = true;
                GraphLayout.Config.EnableEarlyTermination = true;
            }
            else if (availableMs < 15) // Some time available
            {
                GraphLayout.Config.MaxSweepCount = 4;
                GraphLayout.Config.MinSweepCount = 2;
                GraphLayout.Config.UseParallelProcessing = true;
                GraphLayout.Config.EnableEarlyTermination = true;
            }
            else // Plenty of time
            {
                GraphLayout.Config.MaxSweepCount = 8;
                GraphLayout.Config.MinSweepCount = 3;
                GraphLayout.Config.UseParallelProcessing = (graph.NodeMap.Count > 50);
                GraphLayout.Config.EnableEarlyTermination = true;
            }
        }
        
        /// <summary>
        /// Helper method to clear cached data when the graph structure changes significantly
        /// </summary>
        public static void ResetLayoutCache(this Graph graph)
        {
            GraphLayout.ClearCache();
        }
    }
}
