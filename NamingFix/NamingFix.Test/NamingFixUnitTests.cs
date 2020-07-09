using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using NamingFix;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.MSTest.AnalyzerVerifier<NamingFix.PublicConstAnalyzer>;
using System.IO;
using Microsoft.CodeAnalysis.Testing;

namespace NamingFix.Test
{
    [TestClass]
    public class UnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = @"";

            await Verify.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = File.ReadAllText("./TestData/MIC001/Warning.cs");
            var fixtest = File.ReadAllText("./TestData/MIC001/Fixed.cs");


            DiagnosticResult expected = Verify.Diagnostic(PublicConstAnalyzer.DiagnosticId).WithSpan(9, 26, 9, 31).WithArguments("value");
            await Verify.VerifyAnalyzerAsync(test, expected);
            //await Verify.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
