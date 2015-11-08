﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static LanguageExt.Prelude;

namespace LanguageExt
{
    /// <summary>
    /// Writer monad
    /// </summary>
    /// <typeparam name="Out">Writer output</typeparam>
    /// <typeparam name="T">Wrapped type</typeparam>
    public delegate WriterResult<Out, T> Writer<Out, T>();

    public struct WriterResult<Out, T>
    {
        public readonly T Value;
        public readonly IEnumerable<Out> Output;
        public readonly bool IsBottom;

        internal WriterResult(T value, IEnumerable<Out> output, bool isBottom = false)
        {
            if (isnull(output)) throw new ArgumentNullException(nameof(output));
            Value = value;
            Output = output;
            IsBottom = isBottom;
        }

        public static implicit operator WriterResult<Out, T>(T value) =>
           new WriterResult<Out, T>(value, new Out[0]);        // TODO:  Not a good idea

        public static implicit operator T(WriterResult<Out, T> value) =>
           value.Value;

    }

    internal static class WriterResult
    {
        internal static WriterResult<Out, T> Bottom<Out, T>(IEnumerable<Out> output) =>
            new WriterResult<Out, T>(default(T), output, true);

        internal static WriterResult<Out, T> Bottom<Out, T>(Out output) =>
            new WriterResult<Out, T>(default(T), new Out[1] { output }, true);

        internal static WriterResult<Out, T> Bottom<Out, T>(params Out[] output) =>
            new WriterResult<Out, T>(default(T), output, true);

        internal static WriterResult<Out, T> Return<Out, T>(T value, IEnumerable<Out> output) =>
            new WriterResult<Out, T>(value, output, false);

        internal static WriterResult<Out, T> Return<Out, T>(T value, Out output) =>
            new WriterResult<Out, T>(value, new Out[1] { output }, false);

        internal static WriterResult<Out, T> Return<Out, T>(T value, params Out[] output) =>
            new WriterResult<Out, T>(value, output, false);
    }

    /// <summary>
    /// Writer extension methods
    /// </summary>
    public static class WriterExt
    {
        public static IEnumerable<T> AsEnumerable<Out, T>(this Writer<Out, T> self)
        {
            var res = self();
            if (!res.IsBottom)
            {
                yield return self().Value;
            }
        }

        public static Writer<Out,Unit> Iter<Out, T>(this Writer<Out, T> self, Action<T> action)
        {
            return () =>
            {
                var res = self();
                if (!res.IsBottom)
                {
                    action(res.Value);
                }
                return WriterResult.Return(unit,res.Output);
            };
        }

        public static Writer<Out,int> Count<Out, T>(this Writer<Out, T> self) => () =>
            bmap(self(), x => 1);

        public static Writer<Out, bool> ForAll<Out, T>(this Writer<Out, T> self, Func<T, bool> pred) => () =>
            bmap(self(), x => pred(x));

        public static Writer<Out,bool> Exists<Out, T>(this Writer<Out, T> self, Func<T, bool> pred) => () =>
            bmap(self(), x => pred(x));

        public static Writer<Out, S> Fold<Out, S, T>(this Writer<Out, T> self, S state, Func<S, T, S> folder) => () =>
            bmap(self(), x => folder(state, x));

        public static Writer<Out, R> Map<Out, T, R>(this Writer<Out, T> self, Func<T, R> mapper) =>
            self.Select(mapper);

        public static Writer<Out, R> Bind<Out, T, R>(this Writer<Out, T> self, Func<T, Writer<Out, R>> binder)
        {
            return () =>
            {
                var t = self();
                if (t.IsBottom) return WriterResult.Bottom<Out, R>(t.Output);
                var u = binder(t.Value)();
                return WriterResult.Return(u.Value, t.Output.Concat(u.Output));
            };
        }

        /// <summary>
        /// Select
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Writer<W, U> Select<W, T, U>(this Writer<W, T> self, Func<T, U> select)
        {
            if (isnull(select)) throw new ArgumentNullException(nameof(select));
            return () =>
            {
                var resT = self();
                if (resT.IsBottom) return WriterResult.Bottom<W, U>(resT.Output);
                var resU = select(resT.Value);
                return WriterResult.Return(resU, resT.Output);
            };
        }

        /// <summary>
        /// Select Many
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Writer<W, V> SelectMany<W, T, U, V>(
            this Writer<W, T> self,
            Func<T, Writer<W, U>> bind,
            Func<T, U, V> project
        )
        {
            if (isnull(bind)) throw new ArgumentNullException(nameof(bind));
            if (isnull(project)) throw new ArgumentNullException(nameof(project));

            return () =>
            {
                var resT = self();
                if (resT.IsBottom) return WriterResult.Bottom<W, V>(resT.Output);
                var resU = bind(resT.Value).Invoke();
                if (resT.IsBottom) return WriterResult.Bottom<W, V>(resU.Output);
                var resV = project(resT.Value, resU.Value);
                return WriterResult.Return(resV, resT.Output.Concat(resU.Output));
            };
        }

        public static Writer<W, T> Filter<W, T>(this Writer<W, T> self, Func<T, bool> pred) =>
            self.Where(pred);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Writer<W, T> Where<W, T>(this Writer<W, T> self, Func<T, bool> pred)
        {
            return () =>
            {
                var res = self();
                return pred(res.Value)
                    ? WriterResult.Return(res.Value, res.Output)
                    : WriterResult.Bottom<W, T>(res.Output);
            };
        }

        public static Writer<W, int> Sum<W>(this Writer<W, int> self) =>
            () => bmap(self(), x => x);

        private static WriterResult<W, R> bmap<W, T, R>(WriterResult<W, T> r, Func<T, R> f) =>
            r.IsBottom
                ? WriterResult.Bottom<W, R>(r.Output)
                : WriterResult.Return(f(r.Value), r.Output);

        private static WriterResult<W, Unit> bmap<W, T>(WriterResult<W, T> r, Action<T> f)
        {
            if (r.IsBottom)
            {
                return WriterResult.Bottom<W, Unit>(r.Output);
            }
            else
            {
                f(r.Value);
                return WriterResult.Return<W, Unit>(unit, r.Output);
            }
        }

        public static Writer<Out, Reader<Env, V>> foldT<Out, Env, T, V>(Writer<Out, Reader<Env, T>> self, V state, Func<V, T, V> fold) =>
            self.FoldT(state, fold);

        public static Writer<Out, V> foldT<Out, T, V>(Writer<Out, Writer<Out, T>> self, V state, Func<V, T, V> fold) =>
            self.FoldT(state, fold);

        public static Writer<Out, State<S, V>> foldT<Out, S, T, V>(Writer<Out, State<S, T>> self, V state, Func<V, T, V> fold) =>
            self.FoldT(state, fold);

        public static Writer<Out, Reader<Env,V>> FoldT<Out, Env, T, V>(this Writer<Out, Reader<Env, T>> self, V state, Func<V, T, V> fold)
        {
            return () =>
            {
                var inner = self();
                if (inner.IsBottom) return WriterResult.Bottom<Out, Reader<Env, V>>(inner.Output);

                return WriterResult.Return<Out, Reader<Env, V>>(env =>
                   inner.Value.Fold(state, fold)(env),
                   inner.Output
                );
            };
        }

        public static Writer<Out, V> FoldT<Out, T, V>(this Writer<Out, Writer<Out, T>> self, V state, Func<V, T, V> fold)
        {
            return () =>
            {
                var inner = self();
                if (inner.IsBottom) return WriterResult.Bottom<Out, V>(inner.Output);
                var res = inner.Value.Fold(state, fold)();
                return WriterResult.Return<Out, V>(res.Value, inner.Output.Concat(res.Output));
            };
        }

        public static Writer<Out, State<S, V>> FoldT<Out, S, T, V>(this Writer<Out, State<S, T>> self, V state, Func<V, T, V> fold)
        {
            return () =>
            {
                var inner = self();
                if (inner.IsBottom) return WriterResult.Bottom<Out, State<S, V>>(inner.Output);

                return WriterResult.Return<Out, State<S, V>>(s =>
                   inner.Value.Fold(state, fold)(s),
                   inner.Output
                );
            };
        }

        /// <summary>
        /// Select Many
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Writer<Out, Reader<E, V>> SelectMany<Out, E, T, U, V>(
            this Writer<Out, T> self,
            Func<T, Reader<E, U>> bind,
            Func<T, U, V> project
            )
        {
            if (isnull(bind)) throw new ArgumentNullException(nameof(bind));
            if (isnull(project)) throw new ArgumentNullException(nameof(project));
            return () =>
            {
                var resT = self();
                if (resT.IsBottom) return WriterResult.Bottom<Out, Reader<E, V>>(resT.Output);
                return WriterResult.Return<Out, Reader<E, V>>(env =>
                {
                    var resU = bind(resT.Value)(env);
                    if (resU.IsBottom) return ReaderResult.Bottom<V>();
                    return ReaderResult.Return(project(resT.Value, resU.Value));
                },resT.Output);
            };
        }

        /// <summary>
        /// Select Many
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Writer<Out, State<S, V>> SelectMany<Out, S, T, U, V>(
            this Writer<Out, T> self,
            Func<T, State<S, U>> bind,
            Func<T, U, V> project
            )
        {
            if (isnull(bind)) throw new ArgumentNullException(nameof(bind));
            if (isnull(project)) throw new ArgumentNullException(nameof(project));
            return () =>
            {
                var resT = self();
                if (resT.IsBottom) return WriterResult.Bottom<Out, State<S, V>>(resT.Output);
                return WriterResult.Return<Out, State<S, V>>(state =>
                {
                    var resU = bind(resT.Value)(state);
                    if (resU.IsBottom) return StateResult.Bottom<S, V>(state);
                    return StateResult.Return(resU.State, project(resT.Value, resU.Value));
                },resT.Output);
            };
        }
    }
}
