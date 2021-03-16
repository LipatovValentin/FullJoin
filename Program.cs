using System;
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
                new Person { Name = "Sergey", City = "Vladimir" }
            };
            var weathers = new List<Weather>
            {
                new Weather { Now = "Solar", City = "Moscow" },
                new Weather { Now = "Rainy", City = "Tallin" }
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
        public static IEnumerable<TResult> FullJoin<TFirst, TSecond, TKey, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TKey> firstKeySelector, Func<TSecond, TKey> secondKeySelector, Func<int, TFirst, TSecond, TResult> resultSelector)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (secondKeySelector == null) throw new ArgumentNullException(nameof(secondKeySelector));
            if (secondKeySelector == null) throw new ArgumentNullException(nameof(secondKeySelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

            var hashset = new HashSet<Pair<TFirst, TSecond>>();
            var firstLookup = first.ToLookup(firstKeySelector);
            var secondLookup = second.ToLookup(secondKeySelector);

            // INNER JOIN
            foreach (var firstGroup in firstLookup)
            {
                foreach (var firstItem in firstGroup)
                {
                    if (secondLookup.Contains(firstGroup.Key) == true)
                    {
                        foreach (var secondItem in secondLookup[firstGroup.Key])
                        {
                            hashset.Add(new Pair<TFirst, TSecond>(firstItem, secondItem));
                        }
                    }
                }
            }
            // LEFT JOIN with NULL
            foreach (var firstGroup in firstLookup)
            {
                foreach (var firstItem in firstGroup)
                {
                    if (secondLookup.Contains(firstGroup.Key) == false)
                    {
                        hashset.Add(new Pair<TFirst, TSecond>(firstItem, default(TSecond)));
                    }
                }
            }
            // RIGHT JOIN with NULL
            foreach (var secondGroup in secondLookup)
            {
                foreach (var secondItem in secondGroup)
                {
                    if (firstLookup.Contains(secondGroup.Key) == false)
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
