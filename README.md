# Cistern.ValueLinq

In the tradition of [Kevin Montrose's LinqAF](https://github.com/kevin-montrose/LinqAF), a version of Linq that has minimal allocations. (Another rendition on this there is [@reegeek's StructLinq](https://github.com/reegeek/StructLinq)).

So given that there area already a couple of version of a value based linq, what does this one bring to the table?

- Easy conversion from `System.Linq` (just change the using declaration)
- Different performance profile (there are winners and losers)
- Different memory usage profile (but zero allocations on many streams)
- Performance remains on execution even when stream is returned to an `IEnumerable<>` (but obviously this creates an allocation)
- Fast across all source lengths (i.e. no heavy creation phase)
- If you want to get ugly, performance can be close to hand written loops (and actually beat it sometimes. Huh? Say what? Run the example! Depends on input data, so I'm assuming just "luck" of branch prediction going in my favour, but would really love someone with skills to analyse assembly to debunk or confirm this)

Anyway, here is simple example:


```csharp
        [Benchmark]
        public double Flatout()
        {
            var total = 0.0;
            foreach (var x in _ints)
            {
                if ((x & 1) == 0)
                    total += x * 2;
            }
            return total;
        }
```

OK; Pretty simple, the linq transform of this is:

```csharp
        [Benchmark(Baseline = true)]
        public double Linq() =>
            _ints
            .Where(x => (x & 1) == 0)
            .Select(x => x * 2)
            .Sum();
```

And, the Cistern.ValueLinq looks exactly the same:

```csharp
        [Benchmark]
        public double CisternValueLinq_normal() =>
            _ints
            .Where(x => (x & 1) == 0)
            .Select(x => x * 2)
            .Sum();
```

Now `_ints` here is an `IEnumerable<int>` which means that the poor `Flatout` benchmark doesn't get any loop goodness, so I'll give it a helping hand by having another version with a cast to the underlying array that it actually is, but obviously the "normal" code now has to have this additional knowledge that the Linqs all have wrapped up under the hood:

```csharp
        [Benchmark]
        public double Flatout_cast_to_array()
        {
            var total = 0.0;
            foreach (var x in (int[])_ints)
            {
                if ((x & 1) == 0)
                    total += x * 2;
            }
            return total;
        }
```

OK, and here is an example of the 'ugly' where we use value-type lambdas to perform the actions. These are pure functions. They could copy some state around, but lead to some perverse outcomes.

```csharp
        struct DoubleAnInt : IFunc<int, int> { public int Invoke(int t) => t * 2; } 
        struct FilterEvenInts : IFunc<int, bool> { public bool Invoke(int t) => (t & 1) == 0; }
```

And then we can use them in my new Linq as follows

```csharp
        public double CisternValueLinq_struct() =>
            _ints
            .Where(new FilterEvenInts()) // ug, sugar please
            .Select(new DoubleAnInt(), default(int)) // ug, sugar please + better type inference...
            .Sum();
```

Notice that we have to supply the output type for the where (via `default(int)`) due to the way that c# handles type inference of generic arguments with constraints.

Now, the following doesn't exist, but I'm imagining you could have an additional syntax something like the following, where a >=> would create a value type IFunc used in the above example, and allow a layer of type-inference to do some magic. But hey, one can but dream:

```
        [Benchmark]
        public double CisternValueLinq_struct_in_dream_land() =>
            _ints
            .Where(t >=> t * 2)
            .Select(t >=> (t & 1) == 0)
            .Sum();
```

And finally a "nothing up my sleave" version that seemly switches between the value type representation and the `IEnumerable<>` for an example of the trivial interop with existing code. It also shows that segments created in different parts can all be just put together with an Aggregating function, and hence all the magic that ties together everything at runtime.

```csharp
        [Benchmark]
        public double CisternValueLinq_struct_nothing_up_my_sleve()
        {
            IEnumerable<int> collection = GetCollection();
            IEnumerable<int> withWhere = AddWhere(collection);
            IEnumerable<int> andSelect = AddSelect(withWhere);
            
            var result = andSelect.Sum();

            return result;

            IEnumerable<int> GetCollection() => _ints;
            IEnumerable<int> AddWhere(IEnumerable<int> stuff) => stuff.Where(new FilterEvenInts());
            IEnumerable<int> AddSelect(IEnumerable<int> stuff) => stuff.Select(new DoubleAnInt(), default(int));
        }
```

|                  Method |  Length |             Mean | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------ |-------- |-----------------:|------:|--------:|-------:|------:|------:|----------:|
| CisternValueLinq_normal |       1 |        93.307 ns |  0.76 |    0.01 |      - |     - |     - |         - |
| **CisternValueLinq_struct** |       **1** |        **95.048 ns** |  **0.78** |    0.01 |      - |     - |     - |         - |
| ..._nothing_up_my_sleve |       1 |       156.451 ns |  1.28 |    0.02 | 0.0153 |     - |     - |      64 B |
|                 Flatout |       1 |        24.405 ns |  0.20 |    0.00 | 0.0076 |     - |     - |      32 B |
|   **Flatout_cast_to_array** |       **1** |         **8.534 ns** |  **0.07** |    0.00 |      - |     - |     - |         - |
|                    Linq |       1 |       122.235 ns |  1.00 |    0.00 | 0.0248 |     - |     - |     104 B |
|                         |         |                  |       |         |        |       |       |           |
| CisternValueLinq_normal |     100 |       841.937 ns |  0.95 |    0.03 |      - |     - |     - |         - |
| **CisternValueLinq_struct** |     **100** |       **230.915 ns** |  **0.26** |    **0.01** |      - |     - |     - |         - |
| ..._nothing_up_my_sleve |     100 |       293.547 ns |  0.33 |    0.01 | 0.0153 |     - |     - |      64 B |
|                 Flatout |     100 |       704.422 ns |  0.79 |    0.03 | 0.0076 |     - |     - |      32 B |
|   **Flatout_cast_to_array** |     **100** |       **132.444 ns** |  **0.15** |    **0.00** |      - |     - |     - |         - |
|                    Linq |     100 |       886.122 ns |  1.00 |    0.00 | 0.0248 |     - |     - |     104 B |
|                         |         |                  |       |         |        |       |       |           |
| CisternValueLinq_normal | 1000000 | 7,717,081.944 ns |  1.03 |    0.04 |      - |     - |     - |         - |
| **CisternValueLinq_struct** | **1000000** | **1,421,075.820 ns** |  **0.19** |    0.01 |      - |     - |     - |         - |
| ..._nothing_up_my_sleve | 1000000 | 1,413,621.875 ns |  0.19 |    0.00 |      - |     - |     - |      68 B |
|                 Flatout | 1000000 | 6,875,806.944 ns |  0.92 |    0.04 |      - |     - |     - |      32 B |
|   **Flatout_cast_to_array** | **1000000** | **1,253,509.401 ns** |  **0.17** |    0.00 |      - |     - |     - |       3 B |
|                    Linq | 1000000 | 7,469,367.057 ns |  1.00 |    0.00 |      - |     - |     - |     104 B |

