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
        public static IEnumerable<TResult> FullOuterJoin<TFirst, TSecond, TKey, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TKey> firstKeySelector, Func<TSecond, TKey> secondKeySelector, Func<int, TFirst, TSecond, TResult> resultSelector)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            if (secondKeySelector == null) throw new ArgumentNullException(nameof(secondKeySelector));
            if (secondKeySelector == null) throw new ArgumentNullException(nameof(secondKeySelector));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));
            
            int counter = 0;
            var firstLookup = first.ToLookup(firstKeySelector);
            var secondLookup = second.ToLookup(secondKeySelector);

            foreach (var firstItem in first)
            {
                var secondItems = secondLookup[firstKeySelector(firstItem)].AsEnumerable<TSecond>().GetEnumerator();
                if (secondItems.MoveNext())
                {
                    do
                    {
                        yield return resultSelector(++counter, firstItem, secondItems.Current);
                    }
                    while (secondItems.MoveNext());
                }
                else
                {
                    yield return resultSelector(++counter, firstItem, default(TSecond));
                }
            }
            foreach (var secondItem in second)
            {
                if (firstLookup.Contains(secondKeySelector(secondItem)) == false)
                {
                    yield return resultSelector(++counter, default(TFirst), secondItem);
                }
            }
        }
    }
}
