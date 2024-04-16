using System;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace P42.Utils
{
    public class ObservableConcurrentCollection<T> : ObservableCollection<T>, IEnumerable
    {
        bool _allowDuplicates = true;
        public bool AllowDuplicates
        {
            get => _allowDuplicates;
            set
            {
                if (_allowDuplicates != value)
                {
                    _allowDuplicates = value;
                    if (!_allowDuplicates)
                    {
                        lock (_lock)
                        {
                            for (int i = 0; i < Count - 1; i++)
                            {
                                var item = this[i];
                                for (int j = Count - 1; j > i; j--)
                                {
                                    if (item.Equals(this[j]))
                                        base.RemoveItem(j);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected object _lock = new object();

        protected override void ClearItems()
        {
            lock (_lock)
            {
                if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
                {
                    while (Count > 0)
                        base.RemoveAt(Count - 1);
                }
                else
                    base.ClearItems();
                // Change made to address Xamarin.Forms.iOS ListView crash:
                // NSInternalInconsistencyException Reason: Invalid update: invalid number of rows in section X.
                // The number of rows contained in an existing section after the update (i) must be equal to the number
                // of rows contained in that section before the update (j), plus or minus the number of rows inserted or
                // deleted from that section (k inserted, l deleted) and plus or minus the number of rows moved into or
                // out of that section (0 moved in, 0 moved out).
            }
        }

        protected override void InsertItem(int index, T item)
        {
            if (!AllowDuplicates && Contains(item))
                return;
            lock (_lock)
            {
                if (index > Count)
                    index = Count;
                base.InsertItem(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            if (index >= Count)
                return;
            lock (_lock)
                base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            if (!AllowDuplicates && Contains(item))
                return;
            lock (_lock)
            {
                if (index > Count)
                    index = Count;
                base.SetItem(index, item);
            }
        }

        public T[] ToArray()
        {
            lock (_lock)
            {
                var result = new T[Count];
                CopyTo(result, 0);
                return result;
            }
        }

        public List<T> ToList()
        {
            lock (_lock)
            {
                return new List<T>(this);
            }
        }

        protected bool _editingRange;
        public virtual NotifyCollectionChangedEventArgs AddRange(IEnumerable<T> range)
        {
            if (!range?.Any() ?? true)
                return null;
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, range.ToList());
            lock (_lock)
            {
                _editingRange = true;
                AddRangeInner(range);
                _editingRange = false;
            }
            OnCollectionChanged(args);
            return args;
        }

        void AddRangeInner(IEnumerable<T> range)
        {
            var count = Count;
            foreach (var item in range)
            {
                if (!AllowDuplicates && Contains(item))
                    continue;
                base.InsertItem(count++, item);
            }
        }

        public virtual NotifyCollectionChangedEventArgs RemoveRange(IEnumerable<T> range)
        {
            if (!range?.Any() ?? true)
                return null;
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, range.ToList());
            lock (_lock)
            {
                _editingRange = true;
                RemoveRangeInner(range);
                _editingRange = false;
            }
            OnCollectionChanged(args);
            return args;
        }

        void RemoveRangeInner(IEnumerable<T> range)
        {
            int count = Count;
            foreach (var item in range)
            {
                var index = IndexOf(item);
                if (index >= 0 && index < Count)
                {
                    base.RemoveItem(index);
                    count--;
                }
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_editingRange)
                base.OnCollectionChanged(e);
        }


        public new IEnumerator<T> GetEnumerator()
            => ToList().GetEnumerator();

        public new int IndexOf(T item)
        {
            lock (_lock)
                return base.IndexOf(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            //return ((IEnumerable)items).GetEnumerator();
            return ToArray().GetEnumerator();
        }


        #region Linq

        #region Aggregate
        public T Aggregate(Func<T, T, T> func)
        {
            if (func == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.func, GetType());

            T result = default;
            bool touched = false;
            foreach (var item in ToArray())
            {
                touched = true;
                result = func(result, item);
            }
            if (!touched)
                ThrowHelper.ThrowNoElementsException(GetType());

            return result;

        }

        public TAccumulate Aggregate<TAccumulate>(TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
        {
            if (func == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.func, GetType());

            TAccumulate result = seed;
            foreach (T element in ToArray())
                result = func(result, element);

            return result;
        }

        public TResult Aggregate<TAccumulate, TResult>(TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        {
            if (func == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.func, GetType());

            if (resultSelector == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.resultSelector, GetType());
            }

            TAccumulate result = seed;
            foreach (T element in ToArray())
                result = func(result, element);

            return resultSelector(result);
        }
        #endregion


        #region Any/All
        public bool Any()
            => Count > 0;

        public bool Any(Func<T, bool> predicate)
        {
            if (predicate == null)
                return Count > 0;
            foreach (var item in ToArray())
                if (predicate(item))
                    return true;
            return false;
        }

        public bool All(Func<T, bool> predicate)
        {
            if (predicate == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.predicate, GetType());

            foreach (T element in ToArray())
                if (!predicate(element))
                    return false;

            return true;
        }
        #endregion


        #region Append / Prepend
        public IEnumerable<T> Append(T element)
        {
            var result = ToList();
            result.Add(element);
            return result;
        }

        public IEnumerable<T> Prepend(T element)
        {
            var result = ToList();
            result.Insert(0, element);
            return result;
        }
        #endregion


        #region Average
        public double? Average()
        {
            var type = typeof(T);
            if (type == typeof(int))
                return ToArray().Cast<int>().Average();
            if (type == typeof(int?))
                return ToArray().Cast<int?>().Average();
            if (type == typeof(long))
                return ToArray().Cast<long>().Average();
            if (type == typeof(long?))
                return ToArray().Cast<long?>().Average();
            if (type == typeof(float))
                return ToArray().Cast<float>().Average();
            if (type == typeof(float?))
                return ToArray().Cast<float?>().Average();
            if (type == typeof(double))
                return ToArray().Cast<double>().Average();
            if (type == typeof(double?))
                return ToArray().Cast<double?>().Average();
            if (type == typeof(decimal))
                return (double?)ToArray().Cast<decimal>().Average();
            if (type == typeof(decimal?))
                return (double?)ToArray().Cast<decimal?>().Average();
            return null;
        }

        public double Average(Func<T, int> selector)
            => ToArray().Average(selector);

        public double? Average(Func<T, int?> selector)
            => ToArray().Average(selector);

        public double Average(Func<T, long> selector)
            => ToArray().Average(selector);

        public double? Average(Func<T, long?> selector)
            => ToArray().Average(selector);

        public double Average(Func<T, float> selector)
            => ToArray().Average(selector);

        public double? Average(Func<T, float?> selector)
            => ToArray().Average(selector);

        public double Average(Func<T, double> selector)
            => ToArray().Average(selector);

        public double? Average(Func<T, double?> selector)
            => ToArray().Average(selector);

        public decimal Average(Func<T, decimal> selector)
            => ToArray().Average(selector);

        public decimal? Average(Func<T, decimal?> selector)
            => ToArray().Average(selector);
        #endregion


        #region Cast
        public IEnumerable<TResult> OfType<TResult>()
        {
            foreach (var item in ToArray())
            {
                if (item is TResult result)
                {
                    yield return result;
                }
            }
        }

        public IEnumerable<TResult> Cast<TResult>()
            => OfType<TResult>();
        #endregion


        #region Concat
        public IEnumerable<T> Concat(IEnumerable<T> other)
        {
            var array = ToArray();
            return array.Concat(other);
        }
        #endregion


        #region Contains
        public new bool Contains(T value)
        {
            lock (_lock)
                return base.Contains(value);
        }

        public bool Contains(T value, IEqualityComparer<T> comparer)
        {
            if (comparer == null)
            {
                foreach (T element in ToArray())
                {
                    if (EqualityComparer<T>.Default.Equals(element, value)) // benefits from devirtualization and likely inlining
                    {
                        return true;
                    }
                }
            }
            else
            {
                foreach (T element in ToArray())
                {
                    if (comparer.Equals(element, value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion


        #region DefaultIfEmpty
        public IEnumerable<T> DefaultIfEmpty() =>
            DefaultIfEmpty(default);

        public IEnumerable<T> DefaultIfEmpty(T defaultValue)
        {
            return Any()
                ? ToArray()
                : new T[] { defaultValue };
        }
        #endregion


        #region Distinct
        public IEnumerable<T> Distinct() => Distinct(null);

        public IEnumerable<T> Distinct(IEqualityComparer<T> comparer)
            => ToArray().Distinct(comparer);
        #endregion


        #region Element At
        public T ElementAt(int index)
            => this[index];

        public T ElementAtOrDefault(int index)
        {
            if (index >= 0)
            {
                if (index < Count)
                {
                    return this[index];
                }
            }
            return default;
        }
        #endregion


        #region Enumerable
        public IEnumerable<T> AsEnumerable() => ToArray();
        #endregion


        #region Except
        public IEnumerable<T> Except(IEnumerable<T> second)
            => ToArray().Except(second);

        public IEnumerable<T> Except(IEnumerable<T> second, IEqualityComparer<T> comparer)
            => ToArray().Except(second, comparer);
        #endregion


        #region First
        public T First()
            => ToArray().First();

        public T First(Func<T, bool> predicate)
            => ToArray().First(predicate);

        public T FirstOrDefault()
            => ToArray().FirstOrDefault();

        public T FirstOrDefault(Func<T, bool> predicate)
            => ToArray().FirstOrDefault(predicate);
        #endregion


        #region Group Join
        public IEnumerable<TResult> GroupJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner, Func<T, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<T, IEnumerable<TInner>, TResult> resultSelector)
            => ToArray().GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector);

        public IEnumerable<TResult> GroupJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner, Func<T, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<T, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
            => ToArray().GroupJoin(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        #endregion


        #region Grouping
        public IEnumerable<IGrouping<TKey, T>> GroupBy<TKey>(Func<T, TKey> keySelector) =>
            ToArray().GroupBy(keySelector);

        public IEnumerable<IGrouping<TKey, T>> GroupBy<TKey>(Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer) =>
            ToArray().GroupBy(keySelector, comparer);

        public IEnumerable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> elementSelector) =>
            ToArray().GroupBy(keySelector, elementSelector);

        public IEnumerable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> elementSelector, IEqualityComparer<TKey> comparer) =>
            ToArray().GroupBy(keySelector, elementSelector, comparer);

        public IEnumerable<TResult> GroupBy<TKey, TResult>(Func<T, TKey> keySelector, Func<TKey, IEnumerable<T>, TResult> resultSelector) =>
            ToArray().GroupBy(keySelector, resultSelector, null);

        public IEnumerable<TResult> GroupBy<TKey, TElement, TResult>(Func<T, TKey> keySelector, Func<T, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector) =>
            ToArray().GroupBy(keySelector, elementSelector, resultSelector, null);

        public IEnumerable<TResult> GroupBy<TKey, TResult>(Func<T, TKey> keySelector, Func<TKey, IEnumerable<T>, TResult> resultSelector, IEqualityComparer<TKey> comparer) =>
            ToArray().GroupBy(keySelector, resultSelector, comparer);

        public IEnumerable<TResult> GroupBy<TKey, TElement, TResult>(Func<T, TKey> keySelector, Func<T, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer) =>
            ToArray().GroupBy(keySelector, elementSelector, resultSelector, comparer);
        #endregion


        #region Intersect
        public IEnumerable<T> Intersect(IEnumerable<T> second)
            => ToArray().Intersect(second);

        public IEnumerable<T> Intersect(IEnumerable<T> second, IEqualityComparer<T> comparer)
             => ToArray().Intersect(second, comparer);
        #endregion


        #region Join
        public IEnumerable<TResult> Join<TInner, TKey, TResult>(IEnumerable<TInner> inner, Func<T, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<T, TInner, TResult> resultSelector)
            => ToArray().Join(inner, outerKeySelector, innerKeySelector, resultSelector);

        public IEnumerable<TResult> Join<TInner, TKey, TResult>(IEnumerable<TInner> inner, Func<T, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<T, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
            => ToArray().Join(inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        #endregion


        #region Last
        public T Last()
            => ToArray().Last();

        public T Last(Func<T, bool> predicate)
            => ToArray().Last(predicate);
        #endregion


        #region Lookup
        public ILookup<TKey, T> ToLookup<TKey>(Func<T, TKey> keySelector)
            => ToArray().ToLookup(keySelector);

        public ILookup<TKey, T> ToLookup<TKey>(Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
            => ToArray().ToLookup(keySelector, comparer);

        public ILookup<TKey, TElement> ToLookup<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> elementSelector)
            => ToArray().ToLookup(keySelector, elementSelector);

        public ILookup<TKey, TElement> ToLookup<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> elementSelector, IEqualityComparer<TKey> comparer)
            => ToArray().ToLookup(keySelector, elementSelector, comparer);
        #endregion


        #region Max
        public double? Max()
        {
            var type = typeof(T);
            if (type == typeof(int))
                return ToArray().Cast<int>().Max();
            if (type == typeof(int?))
                return ToArray().Cast<int?>().Max();
            if (type == typeof(long))
                return ToArray().Cast<long>().Max();
            if (type == typeof(long?))
                return ToArray().Cast<long?>().Max();
            if (type == typeof(float))
                return ToArray().Cast<float>().Max();
            if (type == typeof(float?))
                return ToArray().Cast<float?>().Max();
            if (type == typeof(double))
                return ToArray().Cast<double>().Max();
            if (type == typeof(double?))
                return ToArray().Cast<double?>().Max();
            if (type == typeof(decimal))
                return (double?)ToArray().Cast<decimal>().Max();
            if (type == typeof(decimal?))
                return (double?)ToArray().Cast<decimal?>().Max();
            return null;
        }

        public int Max(Func<T, int> selector)
            => ToArray().Max(selector);

        public int? Max(Func<T, int?> selector)
            => ToArray().Max(selector);

        public long Max(Func<T, long> selector)
            => ToArray().Max(selector);

        public long? Max(Func<T, long?> selector)
            => ToArray().Max(selector);

        public float Max(Func<T, float> selector)
            => ToArray().Max(selector);

        public float? Max(Func<T, float?> selector)
            => ToArray().Max(selector);

        public double Max(Func<T, double> selector)
            => ToArray().Max(selector);

        public double? Max(Func<T, double?> selector)
            => ToArray().Max(selector);

        public decimal Max(Func<T, decimal> selector)
            => ToArray().Max(selector);

        public decimal? Max(Func<T, decimal?> selector)
            => ToArray().Max(selector);
        #endregion


        #region Min
        public double? Min()
        {
            var type = typeof(T);
            if (type == typeof(int))
                return ToArray().Cast<int>().Min();
            if (type == typeof(int?))
                return ToArray().Cast<int?>().Min();
            if (type == typeof(long))
                return ToArray().Cast<long>().Min();
            if (type == typeof(long?))
                return ToArray().Cast<long?>().Min();
            if (type == typeof(float))
                return ToArray().Cast<float>().Min();
            if (type == typeof(float?))
                return ToArray().Cast<float?>().Min();
            if (type == typeof(double))
                return ToArray().Cast<double>().Min();
            if (type == typeof(double?))
                return ToArray().Cast<double?>().Min();
            if (type == typeof(decimal))
                return (double?)ToArray().Cast<decimal>().Min();
            if (type == typeof(decimal?))
                return (double?)ToArray().Cast<decimal?>().Min();
            return null;
        }

        public int Min(Func<T, int> selector)
            => ToArray().Min(selector);

        public int? Min(Func<T, int?> selector)
            => ToArray().Min(selector);

        public long Min(Func<T, long> selector)
            => ToArray().Min(selector);

        public long? Min(Func<T, long?> selector)
            => ToArray().Min(selector);

        public float Min(Func<T, float> selector)
            => ToArray().Min(selector);

        public float? Min(Func<T, float?> selector)
            => ToArray().Min(selector);

        public double Min(Func<T, double> selector)
            => ToArray().Min(selector);

        public double? Min(Func<T, double?> selector)
            => ToArray().Min(selector);

        public decimal Min(Func<T, decimal> selector)
            => ToArray().Min(selector);

        public decimal? Min(Func<T, decimal?> selector)
            => ToArray().Min(selector);
        #endregion


        #region OrderBy
        public IOrderedEnumerable<T> OrderBy<TKey>(Func<T, TKey> keySelector)
            => ToArray().OrderBy(keySelector);

        public IOrderedEnumerable<T> OrderBy<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
            => ToArray().OrderBy(keySelector, comparer);

        public IOrderedEnumerable<T> OrderByDescending<TKey>(Func<T, TKey> keySelector)
            => ToArray().OrderByDescending(keySelector);

        public IOrderedEnumerable<T> OrderByDescending<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
            => ToArray().OrderByDescending(keySelector, comparer);
        #endregion


        #region Reverse
        public IEnumerable<T> Reverse()
            => ToArray().Reverse();
        #endregion


        #region Select
        public IEnumerable<TResult> Select<TResult>(Func<T, TResult> selector)
            => ToArray().Select(selector);

        public IEnumerable<TResult> Select<TResult>(Func<T, int, TResult> selector)
            => ToArray().Select(selector);
        #endregion


        #region SelectMany
        public IEnumerable<TResult> SelectMany<TResult>(Func<T, IEnumerable<TResult>> selector)
            => ToArray().SelectMany(selector);

        public IEnumerable<TResult> SelectMany<TResult>(Func<T, int, IEnumerable<TResult>> selector)
            => ToArray().SelectMany(selector);

        public IEnumerable<TResult> SelectMany<TCollection, TResult>(Func<T, int, IEnumerable<TCollection>> collectionSelector, Func<T, TCollection, TResult> resultSelector)
            => ToArray().SelectMany(collectionSelector, resultSelector);

        public IEnumerable<TResult> SelectMany<TCollection, TResult>(Func<T, IEnumerable<TCollection>> collectionSelector, Func<T, TCollection, TResult> resultSelector)
            => ToArray().SelectMany(collectionSelector, resultSelector);
        #endregion


        #region Sequence Equal
        public bool SequenceEqual(IEnumerable<T> second)
             => ToArray().SequenceEqual(second);

        public bool SequenceEqual(IEnumerable<T> second, IEqualityComparer<T> comparer)
            => ToArray().SequenceEqual(second, comparer);
        #endregion


        #region Single
        public T Single()
            => ToArray().Single();

        public T Single(Func<T, bool> predicate)
            => ToArray().Single(predicate);

        public T SingleOrDefault()
            => ToArray().SingleOrDefault();

        public T SingleOrDefault(Func<T, bool> predicate)
            => ToArray().SingleOrDefault(predicate);
        #endregion


        #region Skip
        public IEnumerable<T> Skip(int count)
            => ToArray().Skip(count);

        public IEnumerable<T> SkipWhile(Func<T, bool> predicate)
            => ToArray().SkipWhile(predicate);

        public IEnumerable<T> SkipWhile(Func<T, int, bool> predicate)
            => ToArray().SkipWhile(predicate);

        //public IEnumerable<T> SkipLast(int count)
        //    => ToArray().SkipL
        #endregion


        #region Sum
        public double? Sum()
        {
            var type = typeof(T);
            if (type == typeof(int))
                return ToArray().Cast<int>().Sum();
            if (type == typeof(int?))
                return ToArray().Cast<int?>().Sum();
            if (type == typeof(long))
                return ToArray().Cast<long>().Sum();
            if (type == typeof(long?))
                return ToArray().Cast<long?>().Sum();
            if (type == typeof(float))
                return ToArray().Cast<float>().Sum();
            if (type == typeof(float?))
                return ToArray().Cast<float?>().Sum();
            if (type == typeof(double))
                return ToArray().Cast<double>().Sum();
            if (type == typeof(double?))
                return ToArray().Cast<double?>().Sum();
            if (type == typeof(decimal))
                return (double?)ToArray().Cast<decimal>().Sum();
            if (type == typeof(decimal?))
                return (double?)ToArray().Cast<decimal?>().Sum();
            return null;
        }

        public int Sum(Func<T, int> selector)
            => ToArray().Sum(selector);

        public int? Sum(Func<T, int?> selector)
            => ToArray().Sum(selector);

        public long Sum(Func<T, long> selector)
            => ToArray().Sum(selector);

        public long? Sum(Func<T, long?> selector)
            => ToArray().Sum(selector);

        public float Sum(Func<T, float> selector)
            => ToArray().Sum(selector);

        public float? Sum(Func<T, float?> selector)
            => ToArray().Sum(selector);

        public double Sum(Func<T, double> selector)
            => ToArray().Sum(selector);

        public double? Sum(Func<T, double?> selector)
            => ToArray().Sum(selector);

        public decimal Sum(Func<T, decimal> selector)
            => ToArray().Sum(selector);

        public decimal? Sum(Func<T, decimal?> selector)
            => ToArray().Sum(selector);
        #endregion


        #region Take
        public IEnumerable<T> Take(int count)
            => ToArray().Take(count);

        public IEnumerable<T> TakeWhile(Func<T, bool> predicate)
            => ToArray().TakeWhile(predicate);

        public IEnumerable<T> TakeWhile(Func<T, int, bool> predicate)
            => ToArray().TakeWhile(predicate);

        //public IEnumerable<T> TakeLast(int count)
        //{ }
        #endregion



        #region Collection
        public Dictionary<TKey, T> ToDictionary<TKey>(Func<T, TKey> keySelector)
            => ToArray().ToDictionary(keySelector);

        public Dictionary<TKey, T> ToDictionary<TKey>(Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
            => ToArray().ToDictionary(keySelector, comparer);

        public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> elementSelector)
            => ToArray().ToDictionary(keySelector, elementSelector);

        public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> elementSelector, IEqualityComparer<TKey> comparer)
            => ToArray().ToDictionary(keySelector, elementSelector, comparer);

        //public HashSet<T> ToHashSet() => source.ToHashSet(comparer: null);

        //public HashSet<T> ToHashSet(IEqualityComparer<T>? comparer) { }
        #endregion


        #region Union
        public IEnumerable<T> Union(IEnumerable<T> second)
            => ToArray().Union(second);

        public IEnumerable<T> Union(IEnumerable<T> second, IEqualityComparer<T> comparer)
            => ToArray().Union(second, comparer);
        #endregion


        #region Where
        public IEnumerable<T> Where(Func<T, bool> predicate)
            => ToArray().Where(predicate);

        public IEnumerable<T> Where(Func<T, int, bool> predicate)
            => ToArray().Where(predicate);

        #endregion


        #region Zip
        //public IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector) { }

        //public IEnumerable<(TFirst First, TSecond Second)> Zip<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second) { }
        #endregion

        #endregion

    }
}