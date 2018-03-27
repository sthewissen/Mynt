using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mynt.Common.Framework.Singleton
{
    /// <summary>
    /// Stores singleton instances (regroups managers)
    /// </summary>
    public class SingletonManager
    {
        public static readonly SingletonManager Instance = new SingletonManager();

        private readonly Dictionary<Type, ISingletonManageable> _contentDictionary = new Dictionary<Type, ISingletonManageable>();

        private readonly object _locker = new object();

        protected SingletonManager()
        {
        }

        /// <summary>
        /// Adds the specified new element.
        /// </summary>
        /// <param name="newElement">The new element.</param>
        public void Add(ISingletonManageable newElement)
        {
            _contentDictionary.Add(newElement.GetType(), newElement);
        }

        /// <summary>
        /// Removes the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        public void Remove(ISingletonManageable element)
        {
            _contentDictionary.Remove(element.GetType());
        }

        public bool Contains(Type elementType)
        {
            return _contentDictionary.ContainsKey(elementType);
        }

        public void Clear()
        {
            _contentDictionary.Clear();
        }

        public T Get<T>() where T : ISingletonManageable, new()
        {
            lock (_locker)
            {
                if (!_contentDictionary.ContainsKey(typeof(T)))
                    Add(new T());
            }
            return (T)_contentDictionary[typeof(T)];
        }

        public IList<T> GetByInterface<T>(Type elementInterface)
        {
            IList<T> resultList = new List<T>();

            foreach (var currentDicoEntry in _contentDictionary)
            {
                Type valueType = currentDicoEntry.Key;
                if (elementInterface.IsAssignableFrom(valueType))
                    resultList.Add((T)currentDicoEntry.Value);
            }

            return resultList;
        }

        /// <summary>
        /// Returns the parameter type cast as a T-type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elementInterface"></param>
        /// <returns></returns>
        /// <example>Return an ICatalogManager from type CatalogServerManager</example>
        public T GetByInterfaceFirstItem<T>(Type elementInterface)
        {
            foreach (var currentDicoEntry in _contentDictionary)
            {
                Type valueType = currentDicoEntry.Key;
                if (elementInterface.IsAssignableFrom(valueType))
                    return (T)currentDicoEntry.Value;
            }

            throw new Exception(string.Format("SingletonManager contained no entry with Interface {0}", elementInterface));
        }

        public bool ContainsElementImplementingInterface(Type elementInterface)
        {
            foreach (var currentDicoEntry in _contentDictionary)
            {
                Type valueType = currentDicoEntry.Key;
                if (elementInterface.IsAssignableFrom(valueType))
                    return true;
            }

            return false;
        }

        public void RemoveByInterface(Type elementInterface)
        {
            lock (_locker)
            {
                var toDeleteList = new List<ISingletonManageable>();
                foreach (var currentDicoEntry in _contentDictionary)
                {
                    Type valueType = currentDicoEntry.Key;
                    if (elementInterface.IsAssignableFrom(valueType))
                        toDeleteList.Add(currentDicoEntry.Value);
                }

                foreach (ISingletonManageable item in toDeleteList)
                {
                    Remove(item);
                }
            }
        }
    }
}
