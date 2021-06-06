using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PCBdesignCADSimuModeling.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged, IDataErrorInfo, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected virtual bool OnPropertyChanged<T>(ref T backingField, T value,
            [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }


        #region Валидация

        public DictionaryWrapper<string, string> ErrorCollection { get; } = new(new Dictionary<string, string>());

        public virtual bool IsValid => 
            Validator.TryValidateObject(this, new ValidationContext(this), null, true);

        public virtual string Error
        {
            get
            {
                var result = ErrorCollection
                    .Where(pair => pair.Value != null)
                    .Aggregate(String.Empty, (current, pair) => current + $"{pair.Key} - {pair.Value} \r\n");

                return (result == String.Empty) ? null : result;
            }
        }

        public virtual string this[string columnName]
        {
            get
            {
                string result = null;
                var validationResults = new List<ValidationResult>();

                var propertyValue = this.GetType().GetProperty(columnName)?.GetValue(this);
                var isValid = Validator.TryValidateProperty(
                    propertyValue, new ValidationContext(this) {MemberName = columnName}, validationResults);

                if (!isValid)
                    result = validationResults.First().ErrorMessage; // ToDo: replace with aggregate

                if (ErrorCollection.ContainsKey(columnName))
                {
                    if (ErrorCollection[columnName] != result)
                    {
                        ErrorCollection[columnName] = result;
                        OnPropertyChanged(nameof(ErrorCollection));
                        OnPropertyChanged(nameof(Error));
                    }
                }
                else if (result != null)
                {
                    ErrorCollection.Add(columnName, result);
                    OnPropertyChanged(nameof(ErrorCollection));
                    OnPropertyChanged(nameof(Error));
                }

                return result;
            }
        }

        #endregion


        public virtual void Dispose()
        {
        }
    }


    public class DictionaryWrapper<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _dictionary;


        public DictionaryWrapper(IDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
            _ = new Dictionary<TKey, TValue>();
        }


        public TValue this[TKey key]
        {
            get
            {
                _dictionary.TryGetValue(key, out var value);
                return value;
            }
            set => _dictionary[key] = value;
        }


        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public ICollection<TKey> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _dictionary.Values;

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _dictionary).GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Remove(item);
        }

        public int Count => _dictionary.Count;

        public bool IsReadOnly => _dictionary.IsReadOnly;
    }
}