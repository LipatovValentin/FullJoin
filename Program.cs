using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FullJoin
{
    class Program
    {
        static void Main(string[] args)
        {
            var persons = new List<Person>
            {
                new Person { Name = "Alexey", City = "Moscow" },
                new Person { Name = "Vladimir", City = "St. Peterburg" },
                new Person { Name = "Sergey", City = "Vladimir" },
            };
            var weathers = new List<Weather>
            {
                new Weather { Now = "Solar", City = "Moscow" },
                new Weather { Now = "Rainy", City = "Tallin" }

            };

            var join = persons.FullJoin(weathers, x => x.City, y => y.City, (first, second) => new { first, second });

            foreach (var j in join)
            {
                Console.WriteLine($"{ j.first?.Name ?? "NULL" } | { j.second?.Now ?? "NULL" }");
            }

            Console.ReadKey();
        }
    }
    public static class Helper
    {
        public static IEnumerable<TResult> FullJoin<TFirst, TSecond, TKey, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TKey> firstKeySelector, Func<TSecond, TKey> secondKeySelector, Func<TFirst, TSecond, TResult> resultSelector)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (secondKeySelector == null) throw new ArgumentNullException(nameof(secondKeySelector));
            if (secondKeySelector == null) throw new ArgumentNullException(nameof(secondKeySelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

            var hashset = new HashSet<TResult>();
            var firstLookup = first.ToLookup(firstKeySelector);
            var secondLookup = second.ToLookup(secondKeySelector);

            foreach (var firstGroup in firstLookup)
            {
                foreach (var firstItem in firstGroup)
                {
                    if (secondLookup.Contains(firstGroup.Key))
                    {
                        foreach (var secondItem in secondLookup[firstGroup.Key])
                        {
                            hashset.Add(resultSelector(firstItem, secondItem));
                        }
                    }
                    else
                    {
                        hashset.Add(resultSelector(firstItem, default(TSecond)));
                    }
                }
            }
            foreach (var secondGroup in secondLookup)
            {
                foreach (var secondItem in secondGroup)
                {
                    if (firstLookup.Contains(secondGroup.Key))
                    {
                        foreach (var firstItem in firstLookup[secondGroup.Key])
                        {
                            hashset.Add(resultSelector(firstItem, secondItem));
                        }
                    }
                    else
                    {
                        hashset.Add(resultSelector(default(TFirst), secondItem));
                    }
                }
            }
            return (IEnumerable<TResult>) hashset;
        }
    }
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
}
