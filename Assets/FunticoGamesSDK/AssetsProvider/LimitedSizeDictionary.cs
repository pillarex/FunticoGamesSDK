using System;
using System.Collections;
using System.Collections.Generic;

namespace FunticoGamesSDK.AssetsProvider
{
    public class LimitedSizeDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>
    {
        private readonly Dictionary<TKey, TValue> _dictionary;
        private readonly Queue<TKey> _orderQueue;
        private readonly int _maxSize;
        public event Action<TKey, TValue> OnKeyEvicted;

        public LimitedSizeDictionary(int maxSize)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be greater than zero.");
            }

            _maxSize = maxSize;
            _dictionary = new Dictionary<TKey, TValue>();
            _orderQueue = new Queue<TKey>();
        }

        public TValue this[TKey key]
        {
            get
            {
                if (_dictionary.TryGetValue(key, out var value))
                {
                    return value;
                }

                throw new KeyNotFoundException($"The key '{key}' was not found in the collection.");
            }
            set
            {
                if (_dictionary.ContainsKey(key))
                {
                    _dictionary[key] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (_dictionary.ContainsKey(key))
            {
                throw new ArgumentException("An element with the same key already exists in the collection.");
            }

            if (_maxSize == 0) return;

            if (_dictionary.Count >= _maxSize)
            {
                TKey oldestKey = _orderQueue.Dequeue();
                OnKeyEvicted?.Invoke(oldestKey, _dictionary[oldestKey]);
                _dictionary.Remove(oldestKey);
            }

            _dictionary.Add(key, value);
            _orderQueue.Enqueue(key);
        }

        public bool Remove(TKey key)
        {
            if (!_dictionary.ContainsKey(key))
            {
                return false;
            }

            bool removed = _dictionary.Remove(key);
            if (removed)
            {
                // Rebuild the queue without the removed key
                var newQueue = new Queue<TKey>(_orderQueue.Count - 1);
                foreach (var k in _orderQueue)
                {
                    if (!k.Equals(key))
                    {
                        newQueue.Enqueue(k);
                    }
                }

                _orderQueue.Clear();
                foreach (var k in newQueue)
                {
                    _orderQueue.Enqueue(k);
                }
            }

            return removed;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public int Count => _dictionary.Count;

        public bool IsReadOnly => false;

        public ICollection<TKey> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _dictionary.Values;

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _dictionary.Clear();
            _orderQueue.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.ContainsKey(item.Key) &&
                   EqualityComparer<TValue>.Default.Equals(_dictionary[item.Key], item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (var pair in _dictionary)
            {
                array[arrayIndex++] = pair;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Contains(item) && Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}