class Token:
    def __init__(self, token_type, value=None):
        self.type = token_type
        self.value = value

class TokenType:
    KeyWord = "KeyWord"
    OPERATOR = "Operator"
    STRING = "String"

class PythonLexer:
    def __init__(self, source):
        self.source = source
        self.position = 0

    def next_token(self):
        pass

def main():
    source_code = input("Enter code: ")
    lexer = PythonLexer(source_code)
    
    while True:
        token = lexer.next_token()
        if token is None:
            break
        print(f"Type: {token.type}, value: {token.value}")

if __name__ == "__main__":
    main()
