using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using System;

public abstract class SerializableDictionaryBase
{
    public abstract class Storage { }

    protected class Dictionary<TKey, TValue> : System.Collections.Generic.Dictionary<TKey, TValue>
    {
        public Dictionary() { }
        public Dictionary(IDictionary<TKey, TValue> dict) : base(dict){ }
        public Dictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

[System.Serializable]
public abstract class SerializableDictionaryBase<TKey, TValue, TValueStorage> : SerializableDictionaryBase, IDictionary<TKey, TValue>, IDictionary, 
    ISerializationCallbackReceiver, IDeserializationCallback, ISerializable
{
    Dictionary<TKey, TValue> m_Dict;

    [SerializeField]
    TKey[] m_Keys;

    [SerializeField]
    TValueStorage[] m_Values;

    public SerializableDictionaryBase()
    {
        m_Dict = new Dictionary<TKey, TValue>();
    }

    public SerializableDictionaryBase(IDictionary<TKey, TValue> dict)
    {
        m_Dict = new Dictionary<TKey, TValue>(dict);
    }

    protected abstract void SetValue(TValueStorage[] storage, int i, TValue value);
    protected abstract TValue GetValue(TValueStorage[] storage,int i);

    public void CopyFrom(IDictionary<TKey, TValue> dict)
    {
        m_Dict.Clear();
        foreach(var kvp in dict)
        {
            m_Dict[kvp.Key] = kvp.Value;
        }
    }

    public void OnAfterDeserialize()
    {
        if(m_Keys != null && m_Values != null && m_Keys.Length == m_Values.Length)
        {
            m_Dict.Clear();
            int n = m_Keys.Length;
            for(int i = 0; i < n; ++i)
            {
                m_Dict[m_Keys[i]] = GetValue(m_Values, i);
            }

            m_Keys = null;
            m_Values = null;
        }
    }

    public void OnBeforeSerialize()
    {
        int n = m_Dict.Count;
        m_Keys = new TKey[n];
        m_Values = new TValueStorage[n];

        int i = 0;
        foreach(var kvp in m_Dict)
        {
            m_Keys[i] = kvp.Key;
            SetValue(m_Values, i, kvp.Value);
            ++i;
        }
    }

    IDictionary<TKey, TValue> GetAsInterface() { return (IDictionary<TKey, TValue>)m_Dict; }

    IDictionary GetAsIDictionary() {  return (IDictionary)m_Dict; }

    #region IDictionary<TKey, TValue>
    public ICollection<TKey> Keys { get { return GetAsInterface().Keys; } }
    public ICollection<TValue> Values { get { return GetAsInterface().Values; } }

    public int Count { get { return GetAsInterface().Count; } }
    public bool IsReadOnly { get { return GetAsInterface().IsReadOnly; } }

    public TValue this[TKey key]
    {
        get { return GetAsInterface()[key]; }
        set { GetAsInterface()[key] = value; }
    }

    public void Add(TKey key, TValue value)
    {
        GetAsInterface().Add(key, value);
    }

    public bool ContainsKey(TKey key)
    {
        return GetAsInterface().ContainsKey(key);
    }
    public bool Remove(TKey key)
    {
        return GetAsInterface().Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return GetAsInterface().TryGetValue(key, out value);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        GetAsInterface().Add(item);
    }

    public void Clear()
    {
        GetAsInterface().Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return GetAsInterface().Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        GetAsInterface().CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return GetAsInterface().Remove(item);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return GetAsInterface().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetAsInterface().GetEnumerator();
    }

    #endregion

    #region IDictionary
    public bool IsFixedSize { get { return GetAsIDictionary().IsFixedSize; } }
    ICollection IDictionary.Keys { get { return GetAsIDictionary().Keys; } }
    ICollection IDictionary.Values { get { return GetAsIDictionary().Values; } }

    public bool IsSynchronized { get { return GetAsIDictionary().IsSynchronized; } }

    public object SyncRoot { get { return GetAsIDictionary().SyncRoot; } }

    public object this[object key]
    {
        get { return GetAsIDictionary()[key]; }
        set { GetAsIDictionary()[key] = value; }
    }

    public void Add(object key, object value)
    {
        GetAsIDictionary().Add(key, value);
    }

    public bool Contains(object key)
    {
        return (GetAsIDictionary().Contains(key));
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return GetAsIDictionary().GetEnumerator();
    }

    public void Remove(object key)
    {
        GetAsIDictionary().Remove(key);
    }

    public void CopyTo(Array array, int index)
    {
        GetAsIDictionary().CopyTo(array, index);
    }

    #endregion

    #region IDeserializationCallback

    public void OnDeserialization(object sender)
    {
        ((IDeserializationCallback)m_Dict).OnDeserialization(sender);
    }
    #endregion

    #region ISerializable

    protected SerializableDictionaryBase(SerializationInfo info, StreamingContext context)
    {
        m_Dict = new Dictionary<TKey, TValue>(info, context);
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        ((ISerializable)m_Dict).GetObjectData(info, context);
    }

    #endregion
}

public static class SerializableDictionary
{
    public class Storage<T> : SerializableDictionaryBase.Storage
    {
        public T m_Data;
    }
 }

[System.Serializable]
public class SerializableDictionary< TKey, TValue> : SerializableDictionaryBase<TKey, TValue, TValue>
{
    public SerializableDictionary() { }
    public SerializableDictionary(IDictionary<TKey, TValue> dict) : base(dict) { }

    public SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

    protected override TValue GetValue(TValue[] storage, int i)
    {
        return storage[i];
    }

    protected override void SetValue(TValue[] storage, int i, TValue value)
    {
        storage[i] = value;
    }
}

[System.Serializable]
public class SerializableDictionary<TKey, TValue, TValueStorage> : SerializableDictionaryBase<TKey, TValue, TValueStorage>
    where TValueStorage : SerializableDictionary.Storage<TValue>, new()
{
    public SerializableDictionary() { }
    public SerializableDictionary(IDictionary<TKey, TValue> dict) : base(dict) { }

    public SerializableDictionary(SerializationInfo info, StreamingContext context) : base (info, context) { }

    protected override TValue GetValue(TValueStorage[] storage, int i)
    {
        return storage[i].m_Data;
    }

    protected override void SetValue(TValueStorage[] storage, int i, TValue value)
    {
        storage[i] = new TValueStorage();
        storage[i].m_Data = value;
    }
}
