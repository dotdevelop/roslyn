﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.UnsealClass;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Diagnostics;
using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.UnsealClass
{
    [Trait(Traits.Feature, Traits.Features.CodeActionsUnsealClass)]
    public sealed class UnsealClassTests : AbstractCSharpDiagnosticProviderBasedUserDiagnosticTest
    {
        internal override (DiagnosticAnalyzer, CodeFixProvider) CreateDiagnosticProviderAndFixer(Workspace workspace)
            => (null, new CSharpUnsealClassCodeFixProvider());

        [Fact]
        public async Task RemovedFromSealedClass()
        {
            await TestInRegularAndScriptAsync(@"
sealed class C
{
}
class D : [|C|]
{
}", @"
class C
{
}
class D : C
{
}");
        }

        [Fact(Skip = "SyntaxGenerator doesn't understand unsafe")]
        public async Task RemovedFromSealedClassWithOtherModifiersPreserved()
        {
            await TestInRegularAndScriptAsync(@"
public sealed unsafe class C
{
}
class D : [|C|]
{
}", @"
public unsafe class C
{
}
class D : C
{
}");
        }

        [Fact]
        public async Task RemovedFromSealedClassWithConstructedGeneric()
        {
            await TestInRegularAndScriptAsync(@"
sealed class C<T>
{
}
class D : [|C<int>|]
{
}", @"
class C<T>
{
}
class D : C<int>
{
}");
        }

        [Fact]
        public async Task NotOfferedForNonSealedClass()
        {
            await TestMissingInRegularAndScriptAsync(@"
class C
{
}
class D : [|C|]
{
}");
        }

        [Fact]
        public async Task NotOfferedForStaticClass()
        {
            await TestMissingInRegularAndScriptAsync(@"
static class C
{
}
class D : [|C|]
{
}");
        }

        [Fact]
        public async Task NotOfferedForStruct()
        {
            await TestMissingInRegularAndScriptAsync(@"
struct S
{
}
class D : [|S|]
{
}");
        }

        [Fact]
        public async Task NotOfferedForDelegate()
        {
            await TestMissingInRegularAndScriptAsync(@"
delegate void F();
{
}
class D : [|F|]
{
}");
        }

        [Fact]
        public async Task NotOfferedForSealedClassFromMetadata1()
        {
            await TestMissingInRegularAndScriptAsync(@"
class D : [|string|]
{
}");
        }

        [Fact]
        public async Task NotOfferedForSealedClassFromMetadata2()
        {
            await TestMissingInRegularAndScriptAsync(@"
class D : [|System.ApplicationId|]
{
}");
        }

        [Fact]
        public async Task RemovedFromAllPartialClassDeclarationsInSameFile()
        {
            await TestInRegularAndScriptAsync(@"
public sealed partial class C
{
}
partial class C
{
}
sealed partial class C
{
}
class D : [|C|]
{
}", @"
public partial class C
{
}
partial class C
{
}
partial class C
{
}
class D : C
{
}");
        }

        [Fact]
        public async Task RemovedFromAllPartialClassDeclarationsAcrossFiles()
        {
            await TestInRegularAndScriptAsync(@"
<Workspace>
    <Project Language=""C#"">
        <Document>
public sealed partial class C
{
}
        </Document>
        <Document>
partial class C
{
}
sealed partial class C
{
}
        </Document>
        <Document>
class D : [|C|]
{
}
        </Document>
    </Project>
</Workspace>", @"
<Workspace>
    <Project Language=""C#"">
        <Document>
public partial class C
{
}
        </Document>
        <Document>
partial class C
{
}
partial class C
{
}
        </Document>
        <Document>
class D : C
{
}
        </Document>
    </Project>
</Workspace>");
        }

        [Fact]
        public async Task RemovedFromClassInVisualBasicProject()
        {
            await TestInRegularAndScriptAsync(@"
<Workspace>
    <Project Language=""C#"" CommonReferences=""true"" AssemblyName=""Project1"">
        <ProjectReference>Project2</ProjectReference>
        <Document>
class D : [|C|]
{
}
        </Document>
    </Project>
    <Project Language=""Visual Basic"" CommonReferences=""true"" AssemblyName=""Project2"">
        <Document>
public notinheritable class C
end class
        </Document>
    </Project>
</Workspace>", @"
<Workspace>
    <Project Language=""C#"" CommonReferences=""true"" AssemblyName=""Project1"">
        <ProjectReference>Project2</ProjectReference>
        <Document>
class D : C
{
}
        </Document>
    </Project>
    <Project Language=""Visual Basic"" CommonReferences=""true"" AssemblyName=""Project2"">
        <Document>
public class C
end class
        </Document>
    </Project>
</Workspace>");
        }
    }
}
