lexer stage：
1.SyntaxTrivia
    todo： " " "\t" "\n" "\r" 
        -FullWidth：Span
        -Language
        -RawKind：Whitespace,EndOfLine
        Span：文本中起始到结束位置，显示形式 [start,end)
        -Text：文本内容 " " "\n"(LinqPad中没有显示Text属性，是因为使用的是外部的syntax类) 
        SpanStart：文本中起始位置
        Token：
    
2.SyntaxToken
    todo：
        -FullWidth：包括trivia在内的Span
        -Language
        HasLeadingTrivia：是否有LeadingTrivia
        HasTrailingTrivia：是否有TrailingTrivia
        LeadingTrivia：获取LeadingTrivia
        TrailingTrivia：获取TrailingTrivia
        -RawKind：NumericLiteralToken
        Span：文本中起始到结束位置，显示形式 [start,end)
        -Text
        SpanStart
        -Value
        ValueText
        
todo :
【ok】 plus token
【ok】 minus token
【ok】理解SlidingTextWindow object pool机制
【ok】理解SlidingTextWindow StringTable的机制
【ok】LexerCache 实现机制
    LookupTrivia
    TextKeyedCache<SyntaxTrivia> _triviaMap 类似StringTable
    TextKeyedCache<SyntaxToken> _tokenMap
    CachingIdentityFactory<string, SyntaxKind> _keywordKindMap 计算密集型缓存，常用于一些提前预埋的数据缓存
【ok】理解SyntaxListBuilder cache机制
ToListNode方法中SyntaxList.List方法的两个重载方法WithTwoChildren和WithThreeChildren
使用了SyntaxNodeCache.TryGetNode方法和SyntaxNodeCache.AddNode方法实现缓存机制
IsCacheable
【ok】实现HasLeadingTrivia,HasTrailingTrivia
【ok】实现Width，对于SyntaxToken表示不包含trivia的文本长度，对于SyntaxTrivia表示trivia本身的文本长度
【ok】实现pratt parsing

【ok】初步实现 parser 
    能解析NumericLiteralExpression 
        如1
    能解析BinaryExpression
        如-1
    能解析BinaryExpression
        如1 + 2表达式      

【ok】实现GreenNode创建红色节点
【todo】了解红色节点封装了GreenNode的哪些属性，直接看LinqPad的Syntax Visualizer
    FullSpan
    -HasLeadingTrivia
    -HasTrailingTrivia
    Language
    Left
    Parent
    RawKind
    Right
    Span
    SpanStart
    -SyntaxTree
    Token

【todo】红色节点的HasLeadingTrivia、HasTrailingTrivia、SyntaxTree属性的实现
关键方法实现
GetTrailingTrivia
GetLastToken
this.Navigator.GetLastToken
GetPredicateFunction
GetStepIntoFunction
ChildNodesAndTokens
Reverse
AsToken
AsNode
新增测试用例
【todo】打印对象的时候显示文本内容
Console.WriteLine(expr.Left);// output:1+ 2
【todo】这是红色节点的属性。实现FullSpan，表示不包含trivia的文本跨度
【todo】这是红色节点的属性。实现Span，表示不包含trivia的文本跨度
        
        