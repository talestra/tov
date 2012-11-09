using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TalesOfVesperiaTranslationEngine;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;

namespace Tests.TalesOfVesperiaTranslationEngine
{
    [TestClass]
    public class ProgressHandlerTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            var Lines = new Queue<string>();
            var ProgressHandler = new ProgressHandler();
            ProgressHandler.OnProgressUpdated += () =>
            {
                var Line = String.Format(
                    "{0}:{1:0.0}, {2}:{3:0.0}",
                    ProgressHandler.GetLevelDescription(0),
                    ProgressHandler.GetProcessedLevelProgress(0),
                    ProgressHandler.GetLevelDescription(1),
                    ProgressHandler.GetProcessedLevelProgress(1)
                );
                Lines.Enqueue(Line);
                Console.WriteLine(Line);
            };
            ProgressHandler.AddProgressLevel("Test1", 3, () =>
            {
                ProgressHandler.IncrementLevelProgress();
                ProgressHandler.AddProgressLevel("Test2", 2, () =>
                {
                    ProgressHandler.IncrementLevelProgress();
                    ProgressHandler.IncrementLevelProgress();
                });
                ProgressHandler.IncrementLevelProgress();
                ProgressHandler.IncrementLevelProgress();
            });

            Assert.AreEqual("Test1:0.0, :0.0", Lines.Dequeue());
            Assert.AreEqual("Test1:0.3, :0.0", Lines.Dequeue());
            Assert.AreEqual("Test1:0.3, Test2:0.0", Lines.Dequeue());
            Assert.AreEqual("Test1:0.5, Test2:0.5", Lines.Dequeue());
            Assert.AreEqual("Test1:0.7, Test2:1.0", Lines.Dequeue());
            Assert.AreEqual("Test1:0.7, :0.0", Lines.Dequeue());
            Assert.AreEqual("Test1:1.0, :0.0", Lines.Dequeue());

        }
    }
}

