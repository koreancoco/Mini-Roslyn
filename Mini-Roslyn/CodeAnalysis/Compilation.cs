using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using MiniRoslyn.CodeAnalysis.Binding;
using MiniRoslyn.CodeAnalysis.Diagnostics;
using MiniRoslyn.CodeAnalysis.Lowering;
using MiniRoslyn.CodeAnalysis.Scripting;
using MiniRoslyn.CodeAnalysis.Symbols;
using MiniRoslyn.CodeAnalysis.Syntax;

namespace MiniRoslyn.CodeAnalysis
{
    public sealed class Compilation
    {
        public SyntaxTree SyntaxTree { get; }

        public Compilation(SyntaxTree syntaxTree)
            :this(null,syntaxTree)
        {
        }
        
        private Compilation(Compilation previous,SyntaxTree syntaxTree)
        {
            Previous = previous;
            SyntaxTree = syntaxTree;
        }

        public Compilation Previous { get; }

        private BoundGlobalScope _globalScope;

        // 当前submission的全局作用域继承自上一个submission的全局作用域
        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    var globalScope = Binder.BoundGlobalScope(Previous?.GlobalScope, SyntaxTree.Root);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }

                return _globalScope;
            }
        }

        public Compilation ContinueWith(SyntaxTree syntaxTree)
        {
            return new Compilation(this, syntaxTree);
        }
        
        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var diagnostics = SyntaxTree.Diagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
            {
                return new EvaluationResult(diagnostics, null);
            }

            var program = Binder.BindProgram(GlobalScope);
            var evaluator = new Evaluator(program,variables);
            var result = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, result);
        }
        

        public void EmitTree(TextWriter textWriter)
        {
            var program = Binder.BindProgram(GlobalScope);
            BoundNodePrinter.WriteTo(program.Statement, textWriter);

        }
    }
}