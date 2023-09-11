using System;

namespace CodeAnalysis
{
    public class SlidingTextWindow
    {
        public const char InvalidCharacter = char.MaxValue;// 表示没有更多字符可读
        public const int DefaultWindowLength = 8;// 滑动窗口默认容量

        private readonly SourceText _text;// Source of text to parse.  
        private int _basis;// 相对于源代码文本起始位置的偏移，重用已经解析过的token的缓冲区后，_basis = token缓冲区长度
        private int _offset;// 相对于滑动窗口起始位置的偏移，指向[即将]读取的字符位置
        private readonly int _textLength;// 文本长度
        private char[] _characterWindow;// 滑动窗口缓冲区，默认容量为8
        private int _characterWindowCount;// 滑动窗口中有效字符数（已装填的字符），也表示下一次拷贝文本的起始位置

        private int _lexemeStart;// 单词开始位置。用法：在解析一个token前，使用Start方法记录_lexemeStart为当前_offset，解析完后，通过GetText获取_lexemeStart到当前_offset的文本，即为单词文本

        public SlidingTextWindow(SourceText text)
        {
            _text = text; 
            _basis = 0;
            _offset = 0;
            _textLength = text.Length;
            _lexemeStart = 0;
            _characterWindow = new char[DefaultWindowLength];
        }
        
        public SourceText Text
        {
            get
            {
                return _text;
            }
        }
        
        /// <summary>
        /// 滑动窗口缓冲区，默认容量为8
        /// </summary>
        public char[] CharacterWindow
        {
            get
            {
                return _characterWindow;
            }
        }
        /// <summary>
        /// 滑动窗口中有效字符数（已装填的字符）
        /// </summary>
        public int CharacterWindowCount
        {
            get
            {
                return _characterWindowCount;
            }
        }
        
        
        
        /// <summary>
        /// lexer指针在文本中的位置
        /// </summary>
        public int Position
        {
            get
            {
                return _basis + _offset;
            }
        }
        
        /// <summary>
        /// lexer指针在滑动窗口中的位置
        /// </summary>
        public int Offset
        {
            get
            {
                return _offset;
            }
        }
        
        /// <summary>
        /// token相对于滑动窗口的起始位置
        /// </summary>
        public int LexemeRelativeStart
        {
            get
            {
                return _lexemeStart;
            }
        }
        
        /// <summary>
        /// token相对于文本的起始位置
        /// </summary>
        public int LexemeStartPosition
        {
            get
            {
                return _basis + _lexemeStart;
            }
        }
        
        /// <summary>
        /// token 宽度
        /// </summary>
        public int Width
        {
            get
            {
                return _offset - _lexemeStart;
            }
        }

        bool MoveChars()
        {
            // 第一次读取时，滑动窗口为空，需要装填
            // 第n次读取时，如果没有可读字符返回false
            if (_offset >= _characterWindowCount)
            {
                // 为了避免滑动窗口无限制增大（减少非必要的扩容），我们需要重复利用_lexemeStart之前的字符（已经读取，不再使用了）空间
                // 当_lexemeStart占滑动窗口大小的1/4时，将_lexemeStart之后的字符（长度为_characterWindowCount - _lexemeStart）移动到滑动窗口的起始位置，
                // 此时我们还需要调整基准位置，确保索引正确
                if (_lexemeStart > (_characterWindowCount / 4))
                {
                    Array.Copy(_characterWindow,
                        _lexemeStart,
                        _characterWindow,
                        0,
                        _characterWindowCount - _lexemeStart);
                    // todo 会影响到什么？影响到的地方需要使用basis调整正确
                    // 1._lexemeStart置0
                    // 2._offset减去_lexemeStart
                    // 3._characterWindowCount减去_lexemeStart
                    _characterWindowCount -= _lexemeStart;
                    _offset -= _lexemeStart;
                    _basis += _lexemeStart;
                    _lexemeStart = 0;
                }
                
                if (_characterWindowCount >= _characterWindow.Length)
                {
                    // 滑动窗口缓冲区已满，需要扩容
                    // 扩容后，滑动窗口缓冲区容量为原来的2倍
                    Array.Resize(ref _characterWindow, _characterWindow.Length * 2);
                }

                // 第一次读取时,装填的字符数量为min(_textLength,_characterWindow.Length)
                // 第n次读取时，装填的字符数量为min(_textLength - _characterWindowCount,_characterWindow.Length-_characterWindowCount)
                // min(滑动窗口中剩余可存放数量，文本中剩余可读数量)
                int amountToRead = Math.Min(_textLength - (_basis + _characterWindowCount), _characterWindow.Length - _characterWindowCount);
                // 从文本中读取字符到滑动窗口：从文本的0位置开始读取
                // 拷贝到_characterWindow的_characterWindowCount的位置，拷贝的字符数为amountToRead
                // amountToRead为0时，不拷贝
                _text.CopyTo(_basis + _characterWindowCount, _characterWindow, _characterWindowCount, amountToRead);
                // 更新滑动窗口中有效字符数
                _characterWindowCount += amountToRead;
                return amountToRead > 0;
            }

            return true;
        }
        
        public bool IsReallyAtEnd()
        {
            // 先比较_offset >= _characterWindowCount更cheap
            return _offset >= _characterWindowCount && Position >= _textLength;
        }

        public char PeekChar()
        {
            // 第一次读取时，滑动窗口为空，需要装填
            if (_offset >= _characterWindowCount && !MoveChars())
            {
                return InvalidCharacter;
            }
            return _characterWindow[_offset];
        }

        public char PeekChar(int delta)
        {
            // 记录position
            int position = Position;
            // 向前移动delta个位置
            AdvanceChar(delta);
            // 查看当前位置的字符
            char ch = PeekChar();// todo: perf 手动内联
            // 重置position
            Reset(position);
            return ch;
        }
        
        /// <summary>
        /// Advance the current position by one. No guarantee that this
        /// position is valid.不保证当前位置有效，使用前先使用PeekChar将文本加载到缓冲区
        /// </summary>
        public void AdvanceChar()
        {
            _offset++;
        }
        
        public void AdvanceChar(int n)
        {
            _offset += n;
        }

        /// <summary>
        /// 检查当前位置的字符是否与预期的字符匹配，如果匹配则向前移动一位。
        /// </summary>
        /// <param name="expected"></param>
        /// <returns></returns>
        public bool Match(char expected)
        {
            if (IsReallyAtEnd())
            {
                return false;
            }

            if (expected != PeekChar())
            {
                return false;
            }
            
            AdvanceChar();
            return true;
        }

        /// <summary>
        /// 获取下一个字符。
        /// 如果当前位置有效，将当前位置向前移动一位；如果当前位置无效，返回无效字符，位置不变
        /// </summary>
        /// <returns></returns>
        public char NextChar()
        {
            char ch = PeekChar();
            if (ch != InvalidCharacter)
            {
                AdvanceChar();
            }
            return ch;
        }

        /// <summary>
        /// 记录单词的开始位置
        /// </summary>
        public void Start()
        {
            _lexemeStart = _offset;
        }
        
        // 实现Reset
        public void Reset(int position)
        {
            // 如果position在滑动窗口中，直接设置_offset
            int relative = position - _basis;
            if (relative >= 0 && relative < _characterWindowCount)
            {
                _offset = relative;
            }
            else // 外部做操作导致的，目前不可能
            {
                
            }
        }
        
        
        public string GetText(bool intern)
        {
            return GetText();
        }
        
        /// <summary>
        /// 获取从单词开始位置到当前位置的文本
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            return GetText(LexemeStartPosition, Width);
        }
        
        /// <summary>
        /// 从指定位置获取指定长度的文本
        /// </summary>
        /// <param name="position"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GetText(int position, int length)
        {
            int offset = position - _basis;
            return new string(_characterWindow, offset, length);
        }
        
        
        
    }
}