using System;
using System.Collections.Generic;

namespace NOP.Data.Graphs
{
    public sealed class DijkstraPathBuilder<TVertex, TDistance>
    {
        public delegate bool TryGetDistance(TVertex startVertex, TDistance startDistance, TVertex endVertex, out TDistance endDistance);

        private readonly IReadOnlyDictionary<TVertex, IReadOnlyCollection<TVertex>> _graph;

        private readonly TryGetDistance _tryGetDistance;

        private readonly IEqualityComparer<TVertex> _vertexComparer;

        private readonly IComparer<TDistance> _distanceComparer;

        private readonly Dictionary<TVertex, TVertex> _verticesOrder = new();

        private readonly Dictionary<TVertex, TDistance> _verticesDistance = new();

        public IReadOnlyDictionary<TVertex, TVertex> VerticesOrder => _verticesOrder;

        public IReadOnlyDictionary<TVertex, TDistance> VerticesDistance => _verticesDistance;

        public DijkstraPathBuilder(
            IReadOnlyDictionary<TVertex, IReadOnlyCollection<TVertex>> graph,
            TryGetDistance tryGetDistance,
            IEqualityComparer<TVertex> vertexComparer = null,
            IComparer<TDistance> distanceComparer = null)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
            _tryGetDistance = tryGetDistance ?? throw new ArgumentNullException(nameof(tryGetDistance));
            _vertexComparer = vertexComparer ?? EqualityComparer<TVertex>.Default;
            _distanceComparer = distanceComparer ?? Comparer<TDistance>.Default;
        }

        public DijkstraPathBuilder<TVertex, TDistance> Build(TVertex startVertex)
        {
            _verticesOrder.Clear();
            _verticesDistance.Clear();

            var verticesQueue = new BinaryHeap<TVertex, TDistance>(_vertexComparer, _distanceComparer);
            verticesQueue.Enqueue(startVertex, default);

            while (verticesQueue.TryDequeue(out var nearestVertex, out var nearestVertexDistance))
            {
                if (!_graph.TryGetValue(nearestVertex, out var neighborVertices))
                {
                    continue;
                }

                foreach (var neighborVertex in neighborVertices)
                {
                    if (_vertexComparer.Equals(neighborVertex, startVertex))
                    {
                        continue;
                    }

                    if (!_tryGetDistance(nearestVertex, nearestVertexDistance, neighborVertex, out var neighborVertexDistance))
                    {
                        continue;
                    }

                    if (!_verticesDistance.TryGetValue(neighborVertex, out var neighborVertexDistancePrev))
                    {
                        _verticesDistance[neighborVertex] = neighborVertexDistance;
                        _verticesOrder[neighborVertex] = nearestVertex;
                        verticesQueue.Enqueue(neighborVertex, neighborVertexDistance);
                    }
                    else if (_distanceComparer.Compare(neighborVertexDistance, neighborVertexDistancePrev) < 0)
                    {
                        _verticesDistance[neighborVertex] = neighborVertexDistance;
                        _verticesOrder[neighborVertex] = nearestVertex;
                        verticesQueue.DecreasePriority(neighborVertex, neighborVertexDistance);
                    }
                }
            }

            return this;
        }
    }
}
