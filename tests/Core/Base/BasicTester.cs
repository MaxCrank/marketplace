// File: BasicTester.cs
// Copyright (c) 2018-2019 Maksym Shnurenok
// License: MIT
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Marketplace.Core.Tests.Base
{
    /// <summary>
    /// Base test class for running fixtures in parallel.
    /// </summary>
    [TestFixture]
    [Parallelizable(ParallelScope.Fixtures)]
    [ExcludeFromCodeCoverage]
    public abstract class BasicTester
    {
        #region Fields

        /// <summary>
        /// The running fixtures count
        /// </summary>
        private static readonly ConcurrentDictionary<string, int> RunningFixturesCount = 
            new ConcurrentDictionary<string, int>();

        /// <summary>
        /// The stopwatch
        /// </summary>
        private readonly Stopwatch stopwatch = new Stopwatch();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the fixture runner. Meant to be a base class name to be used for child fixtures counting.
        /// </summary>
        /// <value>
        /// The fixture runner.
        /// </value>
        protected virtual string FixtureRunner => this.GetType().BaseType.Name;

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        protected virtual string Password => "testCorePassword";

        /// <summary>
        /// Gets the application identifier.
        /// </summary>
        /// <value>
        /// The application identifier.
        /// </value>
        protected virtual string AppId => "TestApp";

        /// <summary>
        /// Gets the current running fixtures count for the particular runner text fixture belongs to.
        /// </summary>
        /// <value>
        /// The current running fixtures count.
        /// </value>
        protected int CurrentRunningFixturesCount => RunningFixturesCount[this.FixtureRunner];

        #endregion

        #region Configuration

        /// <summary>
        /// Initializes the tests.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [OneTimeSetUp]
        public async Task InitTests()
        {
            if (RunningFixturesCount.ContainsKey(this.FixtureRunner))
            {
                RunningFixturesCount[this.FixtureRunner]++;
            }
            else
            {
                RunningFixturesCount[this.FixtureRunner] = 1;
            }

            await this.InitFixtureResources();
        }

        /// <summary>
        /// Initializes the fixture resources.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected abstract Task InitFixtureResources();

        /// <summary>
        /// Finalizes the tests.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [OneTimeTearDown]
        public async Task FinalizeTests()
        {
            RunningFixturesCount[this.FixtureRunner]--;
            await this.ReleaseFixtureResources();
        }

        /// <summary>
        /// Releases the fixture resources.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected abstract Task ReleaseFixtureResources();

        /// <summary>
        /// Initializes the test.
        /// </summary>
        [SetUp]
        public void InitTest()
        {
            stopwatch.Restart();
        }

        /// <summary>
        /// Finalizes the test.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [TearDown]
        public async Task FinalizeTest()
        {
            await this.ReleaseTestResources();
            stopwatch.Stop();
            var test = TestContext.CurrentContext.Test;
            string argString = string.Empty;
            foreach (var arg in test.Arguments)
            {
                argString += arg + ", ";
            }

            if (!string.IsNullOrEmpty(argString))
            {
                argString = argString.Remove(argString.Length - 1, 1);
            }

            string newLine =
                $"{test.MethodName}({argString}) - {stopwatch.ElapsedMilliseconds} ms{Environment.NewLine}";
            Console.WriteLine(newLine);
        }

        /// <summary>
        /// Releases the test resources.
        /// </summary>
        /// <returns></returns>
        protected abstract Task ReleaseTestResources();

        #endregion

        #region Helper Methods

        /// <summary>
        /// Performs the action with all objects.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="objects">The objects.</param>
        protected void PerformWithAllObjects<T>(Action<T> action, IEnumerable<T> objects)
        {
            foreach (T obj in objects)
            {
                action.Invoke(obj);
            }
        }

        /// <summary>
        /// Performs the action with all objects.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="objects">The objects.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected async Task PerformWithAllObjectsAsync<T>(Func<T, Task> action, IEnumerable<T> objects)
        {
            await Task.WhenAll(objects.Select(action.Invoke));
        }

        #endregion
    }
}
