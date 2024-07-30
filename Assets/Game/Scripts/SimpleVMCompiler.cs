using System;
using System.Collections.Generic;

public class SimpleVMCompiler {

	public static byte[] Compile(string code) {

		List<Token> tokens = Tokenize(code);
		ProgramNode programNode = Parse(tokens);
		List<byte> program = Generate(programNode);

		return program.ToArray();
	}

#region Tokenizer

	private struct Token {
		public TokenType TokenType;
		public string Value;
		public int Line;

		public Token(TokenType type, string value, int line) {
			TokenType = type;
			Value = value;
			Line = line;
		}
	}
	
	private enum TokenType {
		Keyword,
		Identifier,
		Number,
		Operator,
		Delimiter
	}

	private static readonly HashSet<string> Keywords = new() {
		"num", "array", "if", "for", "in", "to"
	};
	
	private static readonly HashSet<char> Specials = new() {
		'=', '!', '<', '>', '+', '-', '*', '/', '%', '^', '&', '|', '~',
		';', '(', ')', '{', '}', '[', ']', ',',
	};

	private static List<Token> Tokenize(string code) {
		List<Token> tokens = new();
		int i = 0;
		int line = 0;

		while (i < code.Length) {
			if (code[i] == '\n') {
				line++;
				i++;
				continue;
			}
			if (char.IsWhiteSpace(code[i])) {
				i++;
				continue;
			}

			if (char.IsLetter(code[i])) {
				int start = i;
				while (i < code.Length && (char.IsLetterOrDigit(code[i]) || code[i] == '_')) {
					i++;
				}
				string value = code[start..i];
				TokenType type = Keywords.Contains(value) ? TokenType.Keyword : TokenType.Identifier;
				tokens.Add(new Token(type, value, line));
				continue;
			}

			if (char.IsDigit(code[i])) {
				int start = i;
				while (i < code.Length && char.IsDigit(code[i])) {
					i++;
				}
				string value = code[start..i];
				tokens.Add(new Token(TokenType.Number, value, line));
				continue;
			}

			{
				int start = i;
				while (i < code.Length && Specials.Contains(code[i])) {
					i++;
				}

				string value = code[start..i];

				switch (value) {
					case "=":
					case "!":
					case "==":
					case "!=":
					case "<":
					case ">":
					case "<=":
					case ">=":
					case "+":
					case "-":
					case "*":
					case "/":
					case "%":
					case "^":
					case "&":
					case "|":
					case "~":
						tokens.Add(new Token(TokenType.Operator, value, line));
						break;
					case ";":
					case "(":
					case ")":
					case "{":
					case "}":
					case "[":
					case "]":
					case ",":
						tokens.Add(new Token(TokenType.Delimiter, value, line));
						break;
					default:
						throw new Exception($"Unexpected string: \"{value}\". at line: {line}");
				}
			}
			i++;
		}

		return tokens;
	}
	
#endregion

#region Parser

	private abstract class AstNode { };
	
	private class ProgramNode : AstNode {
		public List<FunctionNode> Functions = new();
	}

	private class FunctionNode : AstNode {
		public string Name;
		public List<AstNode> Body = new();
	}
	
	private class VarDeclNode : AstNode {
		public string Name;
		public string Type;
	}

	private class AssignmentNode : AstNode {
		public string Name;
		public AstNode Expression;
	}

	private class ExpressionNode : AstNode { }

	private class LiteralNode : ExpressionNode {
		public string Value;
	}

	private class IdentifierNode : ExpressionNode {
		public string Name;
	}

	private class BinaryOpNode : ExpressionNode {
		public string Operator;
		public ExpressionNode Left;
		public ExpressionNode Right;
	}

	private class FunctionCallNode : ExpressionNode {
		public string Name;
		public List<ExpressionNode> Arguments = new();
	}

    private static ProgramNode Parse(List<Token> tokens) {
        int pos = 0;
        
        var program = new ProgramNode();
        while (pos < tokens.Count) {
	        program.Functions.Add(ParseFunction());
        }
        return program;

        FunctionNode ParseFunction() {
            var function = new FunctionNode();
            
            if (tokens[pos].TokenType == TokenType.Keyword) {
                function.Name = tokens[pos].Value;
                pos++;
                Expect(TokenType.Identifier); // function name
                Expect(TokenType.Delimiter);  // opening parenthesis
                Expect(TokenType.Delimiter);  // closing parenthesis
                Expect(TokenType.Delimiter);  // opening brace

                while (tokens[pos].Value != "}") {
                    function.Body.Add(ParseStatement());
                }

                Expect(TokenType.Delimiter);  // closing brace
            } else {
                throw new Exception("Expected function definition.");
            }

            return function;
        }

        AstNode ParseStatement() {
            if (tokens[pos].TokenType == TokenType.Keyword && tokens[pos].Value == "num") {
                return ParseVarDeclaration();
            }
            if (tokens[pos].TokenType == TokenType.Identifier && tokens[pos + 1].Value == "=") {
                return ParseAssignment();
            }
            if (tokens[pos].TokenType == TokenType.Identifier && tokens[pos + 1].TokenType == TokenType.Delimiter && tokens[pos + 1].Value == "(") {
                return ParseFunctionCall();
            }
            throw new Exception($"Unexpected token: {tokens[pos].Value}");
        }

        VarDeclNode ParseVarDeclaration() {
            var varDecl = new VarDeclNode {
                Name = tokens[pos + 1].Value,
                Type = tokens[pos].Value
            };
            pos += 2;
            Expect(TokenType.Delimiter); // semicolon
            return varDecl;
        }

        AssignmentNode ParseAssignment() {
            var assignment = new AssignmentNode {
                Name = tokens[pos].Value
            };
            pos += 2; // skip identifier and '='
            assignment.Expression = ParseExpression();
            Expect(TokenType.Delimiter); // semicolon
            return assignment;
        }

        FunctionCallNode ParseFunctionCall() {
            var functionCall = new FunctionCallNode {
                Name = tokens[pos].Value
            };
            pos++; // skip function name
            Expect(TokenType.Delimiter); // opening parenthesis

            while (tokens[pos].TokenType != TokenType.Delimiter || tokens[pos].Value != ")") {
                functionCall.Arguments.Add(ParseExpression());
                if (tokens[pos].TokenType == TokenType.Delimiter && tokens[pos].Value == ",") {
                    pos++; // skip comma
                }
            }

            Expect(TokenType.Delimiter); // closing parenthesis
            return functionCall;
        }

        ExpressionNode ParseExpression() {
            var expr = ParseTerm();

            while (tokens[pos].TokenType == TokenType.Operator &&
                   (tokens[pos].Value == "+" || tokens[pos].Value == "-")) {
                var op = tokens[pos].Value;
                pos++;
                var right = ParseTerm();
                expr = new BinaryOpNode {
                    Operator = op,
                    Left = expr,
                    Right = right
                };
            }

            return expr;
        }

        ExpressionNode ParseTerm() {
            var term = ParseFactor();

            while (tokens[pos].TokenType == TokenType.Operator &&
                   (tokens[pos].Value == "*" || tokens[pos].Value == "/")) {
                var op = tokens[pos].Value;
                pos++;
                var right = ParseFactor();
                term = new BinaryOpNode {
                    Operator = op,
                    Left = term,
                    Right = right
                };
            }

            return term;
        }

        ExpressionNode ParseFactor() {
            if (tokens[pos].TokenType == TokenType.Number) {
                var literal = new LiteralNode { Value = tokens[pos].Value };
                pos++;
                return literal;
            }

            if (tokens[pos].TokenType == TokenType.Identifier) {
                var identifier = new IdentifierNode { Name = tokens[pos].Value };
                pos++;
                return identifier;
            }

            if (tokens[pos].TokenType == TokenType.Delimiter && tokens[pos].Value == "(") {
                pos++;
                var expr = ParseExpression();
                Expect(TokenType.Delimiter); // closing parenthesis
                return expr;
            }

            throw new Exception($"Unexpected token: {tokens[pos].Value}");
        }

        void Expect(TokenType type) {
            if (tokens[pos].TokenType != type) {
                throw new Exception($"Expected token {type} but got {tokens[pos].TokenType}");
            }
            pos++;
        }
    }

#endregion

#region Generator

	private static List<byte> Generate(ProgramNode programNode) {

		return new();
	}

#endregion

}