using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using NUnit.Framework;
using TestHelper;

namespace Clave.MementoMori.Test
{
    [TestFixture]
    public class ObsoleteAfterTests : CodeFixVerifier
    {
        [Test]
        public void TestNothing()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [Test]
        public void TestInTheFuture()
        {
            var test = @"
                using System;
                using System.Collections.Generic;
                using System.Linq;
                using System.Text;
                using System.Threading.Tasks;
                using System.Diagnostics;

                namespace ConsoleApplication1
                {
                    [Obsolete(""after 2029-01-13: Remove this"")]
                    class TypeName
                    {
                    }
                }";
            var expected = new DiagnosticResult
            {
                Id = "ObsoleteAfterInfo",
                Message = "This will become obsolete after 2029-01-13.",
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 11, 22)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);

            VerifyNoCSharpFix(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [Test]
        public void TestInThePast()
        {
            var test = @"
                using System;
                using System.Collections.Generic;
                using System.Linq;
                using System.Text;
                using System.Threading.Tasks;
                using System.Diagnostics;

                namespace ConsoleApplication1
                {
                    [Obsolete(""After 2019-01-13: Remove this"")]
                    class TypeName
                    {
                    }
                }";
            var expected = new DiagnosticResult
            {
                Id = "ObsoleteAfterError",
                Message = "This became obsolete on 2019-01-13.",
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 22)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = $@"
                using System;
                using System.Collections.Generic;
                using System.Linq;
                using System.Text;
                using System.Threading.Tasks;
                using System.Diagnostics;

                namespace ConsoleApplication1
                {{
                    [Obsolete(""After {DateTime.Now.AddDays(1):yyy-MM-dd}: Remove this"")]
                    class TypeName
                    {{
                    }}
                }}";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new ObsoleteAfterCodeFixProvider();

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ObsoleteAfterAnalyzer();
    }
}
