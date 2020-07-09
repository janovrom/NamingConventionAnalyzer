using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;
using NamingFix;
using System.IO;
using TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NamingFix.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = File.ReadAllText("./TestData/MIC001/Warning.cs");
            var fixtest = File.ReadAllText("./TestData/MIC001/Fixed.cs");

            var expected = new DiagnosticResult
            {
                Id = PublicConstAnalyzer.DiagnosticId,
                Message = string.Format(PublicConstAnalyzer.MessageFormat.ToString(), "value"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 9, 26)
                        }
            };
            //DiagnosticResult expected = Verify.Diagnostic(PublicConstAnalyzer.DiagnosticId).WithSpan(9, 26, 9, 31).WithArguments("value");
            VerifyCSharpDiagnostic(test, expected);
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new PublicConstCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new PublicConstAnalyzer();
        }
    }
}
