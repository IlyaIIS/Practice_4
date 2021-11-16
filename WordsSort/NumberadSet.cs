using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace WordsSort
{
    /// <summary> Сет содержащий пары значение-количество, где нет 2-х пар с одинаковым значением. </summary>
    public class NumberedSet<T> : ICollection<T>, IEnumerable<ValueCountPair<T>>
    {
        private List<ValueCountPair<T>> elements = new List<ValueCountPair<T>>();

        public NumberedSet() { }
        public NumberedSet(ICollection<T> collection)
        {
            foreach (T item in collection)
            {
                Add(item);
            }
        }

        public int Count { get { return elements.Count; } }

        public bool IsReadOnly => throw new NotImplementedException();

        ///<summary> Добавляет item в сет если в нём нет эквивалентого для item элемента, в ином случае связанное с эквивалентным элементом число увеличивается на 1. </summary>
        public void Add(T item)
        {
            ValueCountPair<T> newItem = new ValueCountPair<T>(item);
            if (!elements.Contains(newItem))
            {
                elements.Add(newItem);
            }
            else
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Equals(newItem))
                    {
                        elements[i].Count++;
                        break;
                    }
                }
            }
        }
        ///<summary> Если в сете содержиться элемент эквивалентный item, то связанное с ним число уменьшается на 1. Если после уменьшения число стало равно 0, то элемент удаляется из сета. </summary>
        public void Subtract(T item)
        {
            ValueCountPair<T> newItem = new ValueCountPair<T>(item);
            if (elements.Contains(newItem))
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Equals(newItem))
                    {
                        elements[i].Count--;
                        if (elements[i].Count == 0)
                            elements.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public void Clear()
        {
            elements = new List<ValueCountPair<T>>();
        }

        public bool Contains(T item)
        {
            return elements.Contains(new ValueCountPair<T>(item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            return elements.Remove(new ValueCountPair<T>(item));
        }

        public IEnumerator<ValueCountPair<T>> GetEnumerator()
        {
            var current = elements[0];
            for (int i = 0; i < elements.Count; i++)
            {
                yield return elements[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ValueCountPair<T> this[int index]
        {
            get { return elements[index]; }
        }

        public void Sort()
        {
            elements.OrderBy(element => element.Value);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary> Пара значение-количество. </summary>

    public class ValueCountPair<T>
    {
        public T Value { get; }
        public int Count { get; set; }
        public ValueCountPair(T item)
        {
            Value = item;
            Count = 1;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        /// <summary> Сравнение игнорирует количество. </summary>
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(ValueCountPair<T>))
                    return ((ValueCountPair<T>)obj).Value.Equals(Value);
            else
                return false;
        }
    }
}
