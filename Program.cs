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
                new Person { Name = "Max", City = "St. Peterburg" },
                new Person { Name = "Sergey", City = "Sochi" },
                new Person { Name = "Jon", City = "Irkutsk" },
                new Person { Name = "Masha", City = "Irkutsk" }
            };
            var weathers = new List<Weather>
            {
                new Weather { Now = "Solar", City = "Moscow" },
                new Weather { Now = "Solar-Cloudy", City = "Moscow" },
                new Weather { Now = "Rainy", City = "Tallin" },
                new Weather { Now = "Hot", City = "Sochi" },
                new Weather { Now = "Cold", City = "Irkutsk" }
            };
            var join = persons.FullOuterJoin(weathers, x => x.City, y => y.City, (id, first, second) => new { id, first, second });
            foreach (var j in join)
            {
                Console.WriteLine($"{ j.first?.Name ?? "NULL" } | { j.id } | { j.second?.Now ?? "NULL" }");
            }
            Console.ReadKey();
        }
    }

    public static class Helper
    {
        private struct Pair<TFirst, TSecond>
        {
            public TFirst First { get; set; }
            public TSecond Second { get; set; }
            public Pair(TFirst first, TSecond second)
            {
                First = first;
                Second = second;
            }
        }

        public static IEnumerable<TResult> FullOuterJoin<TFirst, TSecond, TKey, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TKey> firstKeySelector, Func<TSecond, TKey> secondKeySelector, Func<int, TFirst, TSecond, TResult> resultSelector)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (secondKeySelector == null) throw new ArgumentNullException(nameof(secondKeySelector));
            if (secondKeySelector == null) throw new ArgumentNullException(nameof(secondKeySelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));
            
            int counter = 0;
            var hashset = new HashSet<Pair<TFirst, TSecond>>();
            var firstLookup = first.ToLookup(firstKeySelector);
            var secondLookup = second.ToLookup(secondKeySelector);

            foreach (var secondItem in second)
            {
                foreach (var firstItem in first)
                {
                    if (firstLookup.Contains(secondKeySelector(secondItem)) == true)
                    {
                        if (firstKeySelector(firstItem).Equals(secondKeySelector(secondItem)))
                        {
                            hashset.Add(new Pair<TFirst, TSecond>(firstItem, secondItem));
                        }
                    }
                    else
                    {
                        if (secondLookup.Contains(firstKeySelector(firstItem)) == false)
                        {
                            hashset.Add(new Pair<TFirst, TSecond>(firstItem, default(TSecond)));
                        }
                        hashset.Add(new Pair<TFirst, TSecond>(default(TFirst), secondItem));
                    }
                }
            }

            return hashset.Select(x => resultSelector(++counter, x.First, x.Second));
        }
    }
}
