using AllanMilne.Ardkit;
using System.Collections.Generic;

namespace CMP409_Compilers {
    public class PALParser : RecoveringRdParser {
        private PALSemantics semantics;

        public PALParser()
        : base(new PALScanner()) {
            semantics = new PALSemantics(this);
        }

        protected override void recStarter() {
            recProgram();
        }

        protected void recProgram() {
            Scope.OpenScope();
            mustBe("PROGRAM");
            mustBe(Token.IdentifierToken);
            mustBe("WITH");
            recVarDecls();
            mustBe("IN");
            recStatement();
            recCheckStatements();
            mustBe("END");
            mustBe(Token.EndOfFile);
            Scope.CloseScope();
        }

        protected void recVarDecls() {
            while (have(Token.IdentifierToken)) {
                var IdentifierList = recIdentList();
                mustBe("AS");
                int VariableType = recType();

                foreach (var id in IdentifierList) {
                    semantics.DeclareIdentifier(id, VariableType);
                }
            }
        }

        protected List<IToken> recIdentList() {
            List<IToken> Identifiers = new List<IToken>();
            Identifiers.Add(scanner.CurrentToken);
            mustBe(Token.IdentifierToken);

            while (have(",")) {
                mustBe(",");
                Identifiers.Add(scanner.CurrentToken);
                mustBe(Token.IdentifierToken);
            }
            return Identifiers;
        }

        protected int recType() {
            int VariableType = LanguageType.Undefined;

            if (have("REAL")) {
                VariableType = LanguageType.Real;
                mustBe("REAL");
            }
            else if (have("INTEGER")) {
                VariableType = LanguageType.Integer;
                mustBe("INTEGER");
            }
            else
                syntaxError("Type");
            return VariableType;
        }

        protected void recStatement() {
            if (have(Token.IdentifierToken))
                recAssignment();
            else if (have("UNTIL")) {
                recLoop();
            }
            else if (have("IF")) {
                recConditional();
            }
            else if (have("INPUT") || have("OUTPUT")) {
                recIO();
            }
            else
                syntaxError("<StatementError>");
        }

        protected void recCheckStatements() {
            while (have(Token.IdentifierToken) || have("UNTIL") || have("IF")
                    || have("INPUT") || have("OUTPUT")) {
                recStatement();
            }
        }

        protected void recAssignment() {
            // Selected variable having token assigned to it
            IToken LHS = scanner.CurrentToken;
            mustBe(Token.IdentifierToken);

            bool AssignmentCheck = have("=");
            mustBe("=");

            // Keep token for later use in error reporting
            IToken RHSToken = scanner.CurrentToken;
            int RHS = recExpression();

            // Check semantics upon valid assignment syntax
            if (AssignmentCheck) {
                // Check if variable being assigned to is within scope
                int variable = semantics.CheckVariableScope(LHS);

                // After confirming variable is in scope check assignment type compatibility
                if (variable != LanguageType.Undefined)
                    semantics.CheckTypeConflict(RHSToken, variable, RHS);
            }
        }

        protected int recTerm() {
            int VariableType = recFactor();
            while (have("*") || have("/")) {
                if (have("*"))
                    mustBe("*");
                else
                    mustBe("/");
                // Keep token for later use in error reporting
                IToken RHSToken = scanner.CurrentToken;

                int RHS = recFactor();
                VariableType = semantics.CheckExpression(RHSToken, VariableType, RHS);
            }
            return VariableType;
        }

        protected int recExpression() {
            int VariableType = recTerm();
            while (have("+") || have("-")) {
                if (have("+"))
                    mustBe("+");
                else
                    mustBe("-");
                // Keep token for later use in error reporting
                IToken RHSToken = scanner.CurrentToken;
                int RHS = recTerm();
                VariableType = semantics.CheckExpression(RHSToken, VariableType, RHS);
            }
            return VariableType;
        }

        protected int recFactor() {
            if (have("+"))
                mustBe("+");
            else if (have("-"))
                mustBe("-");
            if (have("(")) {
                mustBe("(");
                int VariableType = recExpression();
                mustBe(")");
                return VariableType;
            }
            else
                return recValue();
        }

        protected int recValue() {
            int VariableType = LanguageType.Undefined;
            IToken token = scanner.CurrentToken;
            if (have(Token.IdentifierToken)) {
                mustBe(Token.IdentifierToken);
                VariableType = semantics.CheckVariableScope(token);
            }
            else if (have(Token.IntegerToken)) {
                mustBe(Token.IntegerToken);
                VariableType = semantics.CheckVariableType(token);
            }
            else if (have(Token.RealToken)) {
                mustBe(Token.RealToken);
                VariableType = semantics.CheckVariableType(token);
            }
            else
                syntaxError("Value");
            return VariableType;
        }

        protected void recLoop() {
            mustBe("UNTIL");
            recBooleanExpr();
            mustBe("REPEAT");
            recCheckStatements();
            mustBe("ENDLOOP");
        }

        protected void recBooleanExpr() {
            int LHS = recExpression();
            if (have("<"))
                mustBe("<");
            else if (have("="))
                mustBe("=");
            else if (have(">"))
                mustBe(">");
            else
                syntaxError("Boolean Expression");
            // Keep token for later use in error reporting
            IToken RHSToken = scanner.CurrentToken;
            int RHS = recExpression();
            semantics.CheckTypeConflict(RHSToken, LHS, RHS);
        }

        protected void recConditional() {
            mustBe("IF");
            recBooleanExpr();
            mustBe("THEN");
            recCheckStatements();
            if (have("ELSE")) {
                mustBe("ELSE");
                recCheckStatements();
            }
            mustBe("ENDIF");
        }

        protected void recIO() {
            if (have("INPUT")) {
                mustBe("INPUT");
                List<IToken> Identifiers = recIdentList();
                // Check if variable is within scope
                foreach (var id in Identifiers) {
                    semantics.CheckVariableScope(id);
                }
            }
            else if (have("OUTPUT")) {
                mustBe("OUTPUT");
                recExpression();
                while (have(",")) {
                    mustBe(",");
                    recExpression();
                }
            }
            else
                syntaxError("Input/Output");
        }
    }
}