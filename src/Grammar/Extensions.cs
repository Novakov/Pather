using Antlr4.Runtime;

namespace Grammar
{
    public static class Extensions
    {
        public static TContext As<TContext>(this ParserRuleContext @this)
            where TContext : ParserRuleContext
        {
            return (TContext)@this;
        }
    }
}
