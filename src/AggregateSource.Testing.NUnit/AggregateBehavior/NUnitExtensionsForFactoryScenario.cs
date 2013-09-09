﻿using System;
using System.IO;
using System.Linq;

namespace AggregateSource.Testing.AggregateBehavior
{
    /// <summary>
    /// NUnit specific extension methods for asserting aggregate factory behavior.
    /// </summary>
    public static class NUnitExtensionsForFactoryScenario
    {
        /// <summary>
        /// Asserts that the specification is met.
        /// </summary>
        /// <param name="builder">The specification builder.</param>
        /// <param name="comparer">The event comparer.</param>
        public static void Assert(this IEventCentricAggregateFactoryTestSpecificationBuilder builder,
            IEventComparer comparer)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (comparer == null) throw new ArgumentNullException("comparer");
            var specification = builder.Build();
            var runner = new EventCentricAggregateFactoryTestRunner(comparer);
            var result = runner.Run(specification);
            if (result.Failed)
            {
                if (result.ButException.HasValue)
                {
                    using (var writer = new StringWriter())
                    {
                        writer.WriteLine("  Expected: {0} events,", result.Specification.Thens.Length);
                        writer.WriteLine("  But was:  {0}", result.ButException.Value);

                        throw new NUnit.Framework.AssertionException(writer.ToString());
                    }
                }
                if (result.ButEvents.HasValue)
                {
                    if (result.ButEvents.Value.Length != result.Specification.Thens.Length)
                    {
                        using (var writer = new StringWriter())
                        {
                            writer.WriteLine("  Expected: {0} events ({1}),",
                                result.Specification.Thens.Length,
                                String.Join(",", result.Specification.Thens.Select(_ => _.GetType().Name).ToArray()));
                            writer.WriteLine("  But was:  {0} events ({1})",
                                result.ButEvents.Value.Length,
                                String.Join(",", result.ButEvents.Value.Select(_ => _.GetType().Name).ToArray()));

                            throw new NUnit.Framework.AssertionException(writer.ToString());
                        }
                    }
                    using (var writer = new StringWriter())
                    {
                        writer.WriteLine("  Expected: {0} events ({1}),",
                                result.Specification.Thens.Length,
                                String.Join(",", result.Specification.Thens.Select(_ => _.GetType().Name).ToArray()));
                        writer.WriteLine("  But found the following differences:");
                        foreach (var difference in
                            result.Specification.Thens.
                            Zip(result.ButEvents.Value,
                                (expected, actual) => new Tuple<object, object>(expected, actual)).
                            SelectMany(_ => comparer.Compare(_.Item1, _.Item2)))
                        {
                            writer.WriteLine("    {0}", difference.Message);
                        }

                        throw new NUnit.Framework.AssertionException(writer.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Asserts that the specification is met.
        /// </summary>
        /// <param name="builder">The specification builder.</param>
        /// <param name="comparer">The exception comparer.</param>
        public static void Assert(this IExceptionCentricAggregateFactoryTestSpecificationBuilder builder,
            IExceptionComparer comparer)
        {
            if (builder == null) throw new ArgumentNullException("builder");
            if (comparer == null) throw new ArgumentNullException("comparer");
            var specification = builder.Build();
            var runner = new ExceptionCentricAggregateFactoryTestRunner(comparer);
            var result = runner.Run(specification);
            if (result.Failed)
            {
                if (result.ButException.HasValue)
                {
                    using (var writer = new StringWriter())
                    {
                        writer.WriteLine("  Expected: {0},", result.Specification.Throws);
                        writer.WriteLine("  But was:  {0}", result.ButException.Value);

                        throw new NUnit.Framework.AssertionException(writer.ToString());
                    }
                }
                if (result.ButEvents.HasValue)
                {
                    using (var writer = new StringWriter())
                    {
                        writer.WriteLine("  Expected: {0},", result.Specification.Throws);
                        writer.WriteLine("  But was:  {0} events ({1})",
                            result.ButEvents.Value.Length,
                            String.Join(",", result.ButEvents.Value.Select(_ => _.GetType().Name).ToArray()));

                        throw new NUnit.Framework.AssertionException(writer.ToString());
                    }
                }
            }
        }
    }
}