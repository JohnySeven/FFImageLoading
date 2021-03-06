using System.Collections.Generic;
using System.Linq;

namespace FFImageLoading.Cache
{
	public abstract class LRUCache<TKey, TValue> where TKey : class where TValue : class
	{
		protected class LRUCacheItem<K, V>
		{
			public K Key
			{
				get;
				private set;
			}

			public V Value
			{
				get;
				private set;
			}

			public LRUCacheItem(K k, V v)
			{
				Key = k;
				Value = v;
			}
		}

		private readonly object _lockObj = new object();

		private int _currentSize;

		private Dictionary<TKey, LinkedListNode<LRUCacheItem<TKey, TValue>>> _cacheMap = new Dictionary<TKey, LinkedListNode<LRUCacheItem<TKey, TValue>>>();

		protected LinkedList<LRUCacheItem<TKey, TValue>> _lruList = new LinkedList<LRUCacheItem<TKey, TValue>>();

		protected int _capacity;

		public IList<TKey> Keys
		{
			get
			{
				lock (_lockObj)
				{
					return _cacheMap.Keys.ToList();
				}
			}
		}

		public IList<TValue> Values
		{
			get
			{
				lock (_lockObj)
				{
					return _cacheMap.Values.Select((LinkedListNode<LRUCacheItem<TKey, TValue>> v) => v.Value.Value).ToList();
				}
			}
		}

		public LRUCache(int capacity)
		{
			_capacity = capacity;
		}

		public abstract int GetValueSize(TValue value);

		public bool ContainsKey(TKey key)
		{
			TValue value;
			return TryGetValue(key, out value);
		}

		public TValue Get(TKey key)
		{
			lock (_lockObj)
			{
				if (_cacheMap.TryGetValue(key, out var value))
				{
					TValue value2 = value.Value.Value;
					_lruList.Remove(value);
					_lruList.AddLast(value);
					return value2;
				}
				return null;
			}
		}

		public bool TryAdd(TKey key, TValue value)
		{
			lock (_lockObj)
			{
				CleanAbandonedItems();
				if (_cacheMap.ContainsKey(key))
				{
					return false;
				}
				CheckSize(key, value);
				LRUCacheItem<TKey, TValue> value2 = new LRUCacheItem<TKey, TValue>(key, value);
				LinkedListNode<LRUCacheItem<TKey, TValue>> linkedListNode = new LinkedListNode<LRUCacheItem<TKey, TValue>>(value2);
				_lruList.AddLast(linkedListNode);
				_cacheMap.Add(key, linkedListNode);
				return true;
			}
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			lock (_lockObj)
			{
				if (_cacheMap.TryGetValue(key, out var value2))
				{
					value = value2.Value.Value;
					if (value == null)
					{
						Remove(key);
						return false;
					}
					_lruList.Remove(value2);
					_lruList.AddLast(value2);
					return true;
				}
				value = null;
				return false;
			}
		}

		public void Clear()
		{
			lock (_lockObj)
			{
				_cacheMap.Clear();
				_lruList.Clear();
			}
		}

		private void CleanAbandonedItems()
		{
		}

		protected virtual bool CheckSize(TKey key, TValue value)
		{
			int valueSize = GetValueSize(value);
			_currentSize += valueSize;
			while (_currentSize > _capacity && _lruList.Count > 0)
			{
				RemoveFirst();
			}
			return true;
		}

		public void Remove(TKey key)
		{
			if (_cacheMap.TryGetValue(key, out var value))
			{
				_lruList.Remove(value);
			}
		}

		protected virtual void RemoveNode(LinkedListNode<LRUCacheItem<TKey, TValue>> node)
		{
			_lruList.Remove(node);
			_cacheMap.Remove(node.Value.Key);
			_currentSize -= GetValueSize(node.Value.Value);
		}

		protected void RemoveFirst()
		{
			LinkedListNode<LRUCacheItem<TKey, TValue>> first = _lruList.First;
			RemoveNode(first);
		}
	}
}
