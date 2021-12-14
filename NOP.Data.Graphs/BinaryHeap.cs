using System;
using System.Collections.Generic;

namespace NOP.Data.Graphs
{
    public sealed class BinaryHeap<TItem, TPrioriry>
    {
        private readonly IEqualityComparer<TItem> _itemComparer;

        private readonly IComparer<TPrioriry> _priorityComparer;

        private readonly List<(TItem Item, TPrioriry Priority)> _heap = new();

        public BinaryHeap(IEqualityComparer<TItem> itemComparer = null, IComparer<TPrioriry> priorityComparer = null)
        {
            _itemComparer = itemComparer ?? EqualityComparer<TItem>.Default;
            _priorityComparer = priorityComparer ?? Comparer<TPrioriry>.Default;
        }

        public void Enqueue(TItem item, TPrioriry priority)
        {
            _heap.Add((item, priority));

            if (_heap.Count > 1)
            {
                ShiftUp(_heap.Count - 1);
            }
        }

        public bool TryDequeue(out TItem item, out TPrioriry priority)
        {
            if (_heap.Count == 0)
            {
                item = default;
                priority = default;

                return false;
            }
            else if (_heap.Count == 1)
            {
                (item, priority) = _heap[0];
                _heap.Clear();

                return true;
            }

            (item, priority) = _heap[0];
            _heap[0] = _heap[^1];
            _heap.RemoveAt(_heap.Count - 1);

            ShiftDown(0);

            return true;
        }

        public bool DecreasePriority(TItem item, TPrioriry priority)
        {
            var i = _heap.FindIndex(x => _itemComparer.Equals(x.Item, item) && _priorityComparer.Compare(x.Priority, priority) > 0);
            if (i == -1)
            {
                return false;
            }

            _heap[i] = (item, priority);
            ShiftUp(i);

            return true;
        }

        public void RemoveWhere(Predicate<TItem> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            for (int i = _heap.Count - 1; i >= 0; i--)
            {
                if (match(_heap[i].Item))
                {
                    if (i == _heap.Count - 1)
                    {
                        _heap.RemoveAt(_heap.Count - 1);
                    }
                    else
                    {
                        _heap[i] = _heap[^1];
                        _heap.RemoveAt(_heap.Count - 1);

                        if (i == 0 || _priorityComparer.Compare(_heap[i / 2].Priority, _heap[i].Priority) < 0)
                        {
                            ShiftDown(i);
                        }
                        else
                        {
                            ShiftUp(i);
                        }
                    }
                }
            }
        }

        private void ShiftUp(int i)
        {
            while (_priorityComparer.Compare(_heap[i].Priority, _heap[(i - 1) / 2].Priority) < 0)
            {
                Swap(i, (i - 1) / 2);

                i = (i - 1) / 2;
            }
        }

        private void ShiftDown(int i)
        {
            while ((2 * i) + 1 < _heap.Count)
            {
                var left = (2 * i) + 1;
                var right = (2 * i) + 2;

                var j = left;
                if (right < _heap.Count && _priorityComparer.Compare(_heap[right].Priority, _heap[left].Priority) < 0)
                {
                    j = right;
                }

                if (_priorityComparer.Compare(_heap[i].Priority, _heap[j].Priority) <= 0)
                {
                    break;
                }

                Swap(i, j);
                i = j;
            }
        }

        private void Swap(int i, int j)
        {
            var temp = _heap[i];

            _heap[i] = _heap[j];
            _heap[j] = temp;
        }
    }
}
