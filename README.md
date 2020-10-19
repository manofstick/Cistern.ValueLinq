# Cistern.ValueLinq

In the tradition of [Kevin Montrose's LinqAF](https://github.com/kevin-montrose/LinqAF), a version of Linq that has minimal allocations.

This rendition has some generic magic to make it _much_ more flexible, allowing additional containers and/or functions that are fully interoperable with by just adding two quite trivial structures.

But...

Yes, their had to be a but!

As it's playing along the edges of C#'s type system, you have to specify the types for the func arguments. Also there is compromise for avoiding allocations vs not specifying type arguments in some cases, where not specifying type arguments wins.

Anyway, this is a very early prototype, but I have implemented a few core aggregation functions `Count`, `ToList`, `Aggregate` as well as the core operations of `Where` and `Select` and so I think there are enough pieces on the board for an initial evaluation.

So here's an example (from the Benchmark project)...

```
List<(double, double, double)> _doubledoubledoubles = << some data >>;

_doubledoubledoubles
.Select((in (double x, double y, double z) d) => (d.x * d.x, d.y * d.y, d.z * d.z))
.Where((in (double x, double y, double z) d) => d.x > 0.25 && d.y > 0.25 && d.z > 0.25)
.Aggregate((0.0, 0.0, 0.0), ((double x, double y, double z) a, (double x, double y, double z) d) => (a.x + d.x, a.y + d.y, a.z + d.z));
```

|         Method |  Length |            Mean |         Error |        StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------- |-------- |----------------:|--------------:|--------------:|------:|--------:|-------:|------:|------:|----------:|
|           Linq |       0 |        176.7 ns |       1.16 ns |       1.09 ns |  1.00 |    0.00 | 0.0420 |     - |     - |     176 B |
|         LinqAF |       0 |      3,173.8 ns |      62.95 ns |     147.14 ns | 18.22 |    0.93 |      - |     - |     - |         - |
|      ValueLinq |       0 |        169.6 ns |       0.84 ns |       0.78 ns |  0.96 |    0.01 |      - |     - |     - |         - |
| ValueLinqByRef |       0 |        174.5 ns |       1.02 ns |       0.96 ns |  0.99 |    0.01 |      - |     - |     - |         - |
|                |         |                 |               |               |       |         |        |       |       |           |
|           Linq |       1 |        197.3 ns |       0.44 ns |       0.41 ns |  1.00 |    0.00 | 0.0420 |     - |     - |     176 B |
|         LinqAF |       1 |      3,477.8 ns |      68.78 ns |     114.92 ns | 17.74 |    0.60 |      - |     - |     - |         - |
|      ValueLinq |       1 |        197.1 ns |       1.30 ns |       1.22 ns |  1.00 |    0.01 |      - |     - |     - |         - |
| ValueLinqByRef |       1 |        182.0 ns |       0.04 ns |       0.04 ns |  0.92 |    0.00 |      - |     - |     - |         - |
|                |         |                 |               |               |       |         |        |       |       |           |
|           Linq |      10 |        520.9 ns |       3.77 ns |       3.53 ns |  1.00 |    0.00 | 0.0420 |     - |     - |     176 B |
|         LinqAF |      10 |      4,200.0 ns |      83.13 ns |      81.65 ns |  8.06 |    0.16 |      - |     - |     - |         - |
|      ValueLinq |      10 |        388.9 ns |       0.66 ns |       0.59 ns |  0.75 |    0.01 |      - |     - |     - |         - |
| ValueLinqByRef |      10 |        349.8 ns |       1.04 ns |       0.97 ns |  0.67 |    0.00 |      - |     - |     - |         - |
|                |         |                 |               |               |       |         |        |       |       |           |
|           Linq |     100 |      4,079.2 ns |       3.82 ns |       3.57 ns |  1.00 |    0.00 | 0.0381 |     - |     - |     176 B |
|         LinqAF |     100 |     10,691.7 ns |     200.36 ns |     156.43 ns |  2.62 |    0.04 |      - |     - |     - |         - |
|      ValueLinq |     100 |      2,697.5 ns |       2.86 ns |       2.24 ns |  0.66 |    0.00 |      - |     - |     - |         - |
| ValueLinqByRef |     100 |      2,219.2 ns |       9.05 ns |       7.07 ns |  0.54 |    0.00 |      - |     - |     - |         - |
|                |         |                 |               |               |       |         |        |       |       |           |
|           Linq |    1000 |     39,774.6 ns |      85.11 ns |      71.07 ns |  1.00 |    0.00 |      - |     - |     - |     176 B |
|         LinqAF |    1000 |     76,915.0 ns |     950.93 ns |   1,095.10 ns |  1.94 |    0.03 |      - |     - |     - |         - |
|      ValueLinq |    1000 |     26,667.3 ns |       3.01 ns |       2.52 ns |  0.67 |    0.00 |      - |     - |     - |         - |
| ValueLinqByRef |    1000 |     21,557.3 ns |     114.11 ns |     101.16 ns |  0.54 |    0.00 |      - |     - |     - |         - |
|                |         |                 |               |               |       |         |        |       |       |           |
|           Linq | 1000000 | 40,104,726.2 ns | 114,350.26 ns | 106,963.31 ns |  1.00 |    0.00 |      - |     - |     - |     176 B |
|         LinqAF | 1000000 | 37,430,000.0 ns |  37,817.23 ns |  35,374.26 ns |  0.93 |    0.00 |      - |     - |     - |         - |
|      ValueLinq | 1000000 | 27,331,066.0 ns | 533,837.31 ns | 675,133.27 ns |  0.69 |    0.02 |      - |     - |     - |         - |
| ValueLinqByRef | 1000000 | 21,943,806.5 ns |  76,914.40 ns |  71,945.78 ns |  0.55 |    0.00 |      - |     - |     - |         - |
