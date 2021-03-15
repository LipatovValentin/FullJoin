using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FullJoin
{
    public class Person
    {
        public string Name { get; set; }
        public string City { get; set; }
    }
    public class Weather
    {
        public string Now { get; set; }
        public string City { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var persons = new List<Person>
            {
                new Person { Name = "Alexey", City = "Moscow" },
                new Person { Name = "Vladimir", City = "St. Peterburg" },
                new Person { Name = "Sergey", City = "Vladimir" },
                new Person { Name = "Valentin", City = "Sochi" }
            };
            var weathers = new List<Weather>
            {
                new Weather { Now = "Solar", City = "Moscow" },
                new Weather { Now = "Rainy", City = "Tallin" },
                new Weather { Now = "Hot", City = "Sochi" }
            };
            var join = persons.FullJoin(weathers, x => x.City, y => y.City, (id, first, second) => new { id, first, second });
            foreach (var j in join)
            {
                Console.WriteLine($"{ j.first?.Name ?? "NULL" } | { j.id } | { j.second?.Now ?? "NULL" }");
            }
            Console.ReadKey();
        }
    }

    public static class Helper
    {
        private class Pair<TFirst, TSecond> : IEqualityComparer<Pair<TFirst, TSecond>>
        {
            public TFirst First { get; set; }
            public TSecond Second { get; set; }
            public Pair() : this (default(TFirst), default(TSecond)) { }
            public Pair(TFirst first, TSecond second)
            {
                First = first;
                Second = second;
            }
            public bool Equals(Pair<TFirst, TSecond> x, Pair<TFirst, TSecond> y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                if ((x.First == null && y.First == null) && 
                    (x.Second == null && y.Second == null)) return true;
                if (x.First == null || y.First == null) return false;
                if (x.Second == null || y.Second == null) return false;
                if (x.First.Equals(y.First) && x.Second.Equals(y.Second))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public int GetHashCode(Pair<TFirst, TSecond> obj)
            {
                return base.GetHashCode();
            }
        }
        public static IEnumerable<TResult> FullJoin<TFirst, TSecond, TKey, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TKey> firstKeySelector, Func<TSecond, TKey> secondKeySelector, Func<int, TFirst, TSecond, TResult> resultSelector)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (secondKeySelector == null) throw new ArgumentNullException(nameof(secondKeySelector));
            if (secondKeySelector == null) throw new ArgumentNullException(nameof(secondKeySelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

            var hashset = new HashSet<Pair<TFirst, TSecond>>(new Pair<TFirst, TSecond>());
            var firstLookup = first.ToLookup(firstKeySelector);
            var secondLookup = second.ToLookup(secondKeySelector);

            // LEFT JOIN
            foreach (var firstGroup in firstLookup)
            {
                foreach (var firstItem in firstGroup)
                {
                    if (secondLookup.Contains(firstGroup.Key))
                    {
                        foreach (var secondItem in secondLookup[firstGroup.Key])
                        {
                            hashset.Add(new Pair<TFirst, TSecond>(firstItem, secondItem));
                        }
                    }
                    else
                    {
                        hashset.Add(new Pair<TFirst, TSecond>(firstItem, default(TSecond)));
                    }
                }
            }
            // RIGHT JOIN
            foreach (var secondGroup in secondLookup)
            {
                foreach (var secondItem in secondGroup)
                {
                    if (firstLookup.Contains(secondGroup.Key))
                    {
                        foreach (var firstItem in firstLookup[secondGroup.Key])
                        {
                            hashset.Add(new Pair<TFirst, TSecond>(firstItem, secondItem));
                        }
                    }
                    else
                    {
                        hashset.Add(new Pair<TFirst, TSecond>(default(TFirst), secondItem));
                    }
                }
            }

            int counter = 0;
            foreach (var item in hashset)
            {
                yield return resultSelector(++counter, item.First, item.Second);
            }
        }
    }
}
