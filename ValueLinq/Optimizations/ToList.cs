using System;

namespace Cistern.ValueLinq.Optimizations
{
    struct ToList_XXX {}

    struct ToList_Select_XXX<T, U>
    {
        public readonly Func<T, U> Map;
        public ToList_Select_XXX(Func<T, U> map) => Map = map;
    }

    struct ToList_Select_Where_XXX<T, U>
    {
        public readonly Func<T, bool> Filter;
        public readonly Func<T, U> Map;

        public ToList_Select_Where_XXX(Func<T, bool> filter, Func<T, U> map) => (Filter, Map) = (filter, map);
    }

    struct ToList_Where_XXX<T>
    {
        public readonly Func<T, bool> Filter;
        public ToList_Where_XXX(Func<T, bool> filter) => Filter = filter;
    }

    struct ToList_Where_Select_XXX<T, U>
    {
        public readonly Func<T, U> Map;
        public readonly Func<U, bool> Filter;

        public ToList_Where_Select_XXX(Func<T, U> map, Func<U, bool> filter) => (Map, Filter) = (map, filter);
    }

}
