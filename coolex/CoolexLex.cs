﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace coolex
{
    class CoolexType
    {
        public List<CoolexType> Block;
        public CoolexLex.Type Type;
        public string Value;

        public CoolexType(CoolexLex.Type type, string value)
        {
            Type = type;
            Value = value;
            Block = null;
        }
    }

    class CoolexLex
    {
        public enum Type
        {
            BLOCK,
            UNKNOWN,
            OPERATOR,
            STRING_LITERAL,

            NUMERIC_LITERAL,
            PLUS,
            MINUS,
            DIVIDE,
            MULTIPLY
        }

        private string[] OPERATORS = new[] { "+", "-", "/", "*", "{", "}", " " };
        private char[] STRING_LITERAL = new[] { '"', '\'' };

        private string[] BLOCK_OPEN = new[] { "{" };
        private string[] BLOCK_CLOSE = new[] { "}" };

        private Dictionary<Type, string[]> Pieces = new Dictionary<Type, string[]>
        {
            { Type.BLOCK, new[] { "{", "}" } },
            { Type.NUMERIC_LITERAL, new[] { "/[-0-9]+/" } },
            { Type.PLUS, new[] { "+" } },
            { Type.MINUS, new[] { "-" } },
            { Type.DIVIDE, new[] { "/" } },
            { Type.MULTIPLY, new[] { "*" } },
        };

        private CoolexType TokenList = new CoolexType(Type.BLOCK, "");
        public List<CoolexType> GetTokens() => TokenList.Block;

        private Boolean InString = false;
        private string token = "";
        private int cIndex = 0;
        private bool IsOperator = false;
        public void Lex(string Text)
        {
            TokenList.Block = new List<CoolexType>();
            while (cIndex < Text.Length)
            {
                IsOperator = false;
                if (InString == false)
                {
                    foreach (string Operator in OPERATORS)
                    {
                        if (cIndex + Operator.Length > Text.Length) continue;
                        if (Text.Substring(cIndex, Operator.Length) == Operator)
                        {
                            //Sort the old token before adding the operator
                            WorkToken();

                            //Insert new token (operator token)
                            token = Text.Substring(cIndex, Operator.Length);
                            WorkToken();

                            cIndex += Operator.Length;
                            IsOperator = true;
                            break;
                        }
                    }
                }

                if (IsOperator == false)
                {
                    char c = Text.Substring(cIndex, 1).ToCharArray()[0];

                    if (STRING_LITERAL.Contains(c))
                    {
                        if (Text.Substring(cIndex - 1, 1) == "\\")
                            token += c;
                        else
                        {
                            //This means it's end of STRING_LITERAL, and must be added to token list
                            WorkToken(InString);
                            InString = !InString;
                        }
                    }
                    else
                        token += c;


                    cIndex++;
                }
            }

            WorkToken();
        }

        private int BlockIndex = 0;
        private List<CoolexType> GetLastToken(int Direction = 0)
        {
            List<CoolexType> Result = TokenList.Block;

            BlockIndex += Direction;

            for (int levels = 0; levels < BlockIndex; levels++)
            {
                if (Result.Count() > 0)
                {
                    if (Result[Result.Count - 1].Block == null)
                        Result[Result.Count - 1].Block = new List<CoolexType>();

                    Result = Result[Result.Count - 1].Block;
                }
            }
            
            return Result;
        }

        public void WorkToken(Boolean stringToken = false)
        {
            string piece = token;
            token = "";

            if (piece != "")
            {
                if (stringToken == false)
                {
                    foreach (var Piece in Pieces)
                    {
                        foreach (string Value in Piece.Value)
                        {
                            if (Value.Length > 1 && Value.StartsWith("/") && Value.EndsWith("/") && !OPERATORS.Contains(piece))
                            {
                                if (System.Text.RegularExpressions.Regex.IsMatch(piece, Value.Trim('/')))
                                {
                                    GetLastToken().Add(new CoolexType(Piece.Key, piece));
                                    return;
                                }
                            }
                            else
                            {
                                if (Value == piece)
                                {
                                    if (BLOCK_OPEN.Contains(piece))
                                    {
                                        GetLastToken(1);
                                    }
                                    else if (BLOCK_CLOSE.Contains(piece))
                                    {
                                        GetLastToken(-1);
                                    }
                                    else
                                    {
                                        GetLastToken().Add(new CoolexType(Piece.Key, piece));
                                    }
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    GetLastToken().Add(new CoolexType(Type.STRING_LITERAL, piece));
                }
            }

        }
    }
}
