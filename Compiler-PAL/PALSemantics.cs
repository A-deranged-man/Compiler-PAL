using AllanMilne.Ardkit;

namespace CMP409_Compilers {
    class PALSemantics : Semantics {
        public PALSemantics(IParser p) : base(p) { 
        }

        public void DeclareIdentifier(IToken id, int VariableType) {
            if (!id.Is(Token.IdentifierToken)) return;
            Scope symbols = Scope.CurrentScope;
            if (symbols.IsDefined(id.TokenValue)) {
                semanticError(new AlreadyDeclaredError(id, symbols.Get(id.TokenValue)));
            }
            else {
                symbols.Add(new VarSymbol(id, VariableType));
            }
        }

        public int CheckVariableScope(IToken id) {
            if (!Scope.CurrentScope.IsDefined(id.TokenValue)) {
                semanticError(new NotDeclaredError(id));
                return LanguageType.Undefined;
            }
            else return CheckVariableType(id);
        }

        public int CheckVariableType(IToken token) {
            int CurrentType = LanguageType.Undefined;

            if (token.Is(Token.IdentifierToken))
                CurrentType = Scope.CurrentScope.Get(token.TokenValue).Type;
            else if (token.Is(Token.IntegerToken))
                CurrentType = LanguageType.Integer;
            else if (token.Is(Token.RealToken))
                CurrentType = LanguageType.Real;

            return CurrentType;
        }

        public int CheckExpression(IToken expected, int LHS, int RHS) {
            if (LHS == LanguageType.Undefined && RHS == LanguageType.Undefined) {
                return LanguageType.Undefined;
            }
            if (CheckTypeConflict(expected, LHS, RHS))
                return LHS;
            else
                return LHS > RHS ? LHS : RHS;
        }

        public bool CheckTypeConflict(IToken token, int PreviousType, int UpdatedType) {
            if (PreviousType != UpdatedType) {
                semanticError(new TypeConflictError(token, UpdatedType, PreviousType));
                return false;
            }
            return true;
        }
    }
}
