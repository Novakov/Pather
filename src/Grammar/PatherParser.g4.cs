using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Grammar
{
    public enum BinaryOperator
    {
        Plus,
        Pipe
    }

    partial class PatherParser
    {
        partial class ValueReferenceExpressionContext
        {
            public string Scope => this.scope.Text;
            public string ValueName => this.valueName.Text.Trim('\'');
        }

        partial class FunctionCallContext : IExpression
        {
            public string Name => this.ID().GetText();

            public IExpression[] Arguments => this.expressionList()?.simpleExpression() ?? new IExpression[0];
        }


        partial class SimpleExpressionContext : IExpression
        {
        }

        partial class ExpressionContext : IExpression
        {
        }

        partial class BinaryExpressionContext
        {
            public BinaryOperator Operator
            {
                get {
                    switch (this.OP().GetText())
                    {
                        case "+":
                            return BinaryOperator.Plus;
                        case "|":
                            return BinaryOperator.Pipe;
                        default:
                            throw new InvalidOperationException("Invalid binary operator " + this.OP().GetText());
                    }
                }
            }
        }

        partial class StringConstantContext 
        {
            public string Value => this.STRING().GetText().Trim('\'');
        }

        partial class NumberConstantContext
        {
            public double Value => double.Parse(this.NUMBER().GetText(), CultureInfo.InvariantCulture);
        }
    }
}
