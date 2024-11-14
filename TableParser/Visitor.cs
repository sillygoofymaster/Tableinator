using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TableParser
{
    public class LabCalculatorVisitor : TableParserBaseVisitor<double>
    {

        public override double VisitCompileUnit(TableParserParser.CompileUnitContext context) // some questionable naming choices going on
        {
            return Visit(context.expression());
        }

        public override double VisitNumberExpr(TableParserParser.NumberExprContext context)
        {
            var result = double.Parse(context.GetText());
            Debug.WriteLine(result);

            return result;
        }

        //IdentifierExpr
        public override double VisitIdentifierExpr(TableParserParser.IdentifierExprContext context)
        {
            var result = context.GetText();
            Cell cell;

            if (!Calculator.TableIndex.TableIdentifier.ContainsKey(result.ToString()))
            {
                throw new KeyNotFoundException($"Tаблиця не містить клітинки {result}.");
            }

            if (!Calculator.TableIndex.TableIdentifier.TryGetValue(result.ToString(), out cell))
            {
                return 0.0;
            }

            var resultCell = Calculator.TableIndex.TableIdentifier[result];
            Calculator.TableIndex.TableIdentifier[Calculator.TableIndex.CurrentCellName].Dependencies.Add(result);
            //видобути значення змінної з таблиці
            if (Calculator.TableIndex.HasCyclicDependency(result))
            {
                throw new KeyNotFoundException($"Клітинка {result} не повинна посилатися на себе ж.");
            }

            if (!resultCell.OwnDependents.Contains(Calculator.TableIndex.CurrentCellName))
            {
                resultCell.OwnDependents.Add(Calculator.TableIndex.CurrentCellName);
            }

            return Calculator.Evaluate(cell.Expression);
        }

        public override double VisitParenthesizedExpr(TableParserParser.ParenthesizedExprContext context)
        {
            return Visit(context.expression());
        }

        public override double VisitExponentialExpr(TableParserParser.ExponentialExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            Debug.WriteLine("{0} ^ {1}", left, right);
            return System.Math.Pow(left, right);
        }

        public override double VisitAdditiveExpr(TableParserParser.AdditiveExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == TableParserParser.ADD)
            {
                Debug.WriteLine("{0} + {1}", left, right);
                return left + right;
            }
            else //LabCalculatorLexer.SUBTRACT
            {
                Debug.WriteLine("{0} - {1}", left, right);
                return left - right;
            }
        }

        public override double VisitMultiplicativeExpr(TableParserParser.MultiplicativeExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == TableParserParser.MULTIPLY)
            {
                Debug.WriteLine("{0} * {1}", left, right);
                return left * right;
            }
            else //LabCalculatorLexer.DIVIDE
            {
                Debug.WriteLine("{0} / {1}", left, right);
                return left / right;
            }
        }

        public override double VisitModDivExpr(TableParserParser.ModDivExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);

            if (context.operatorToken.Type == TableParserParser.MOD)
                {
                    Debug.WriteLine("{0} mod {1}", left, right);
                    return left % right;
                }
                else //LabCalculatorLexer.DIV
                {
                    Debug.WriteLine("({0} div {1}", left, right);
                    return (int)left / (int)right;
                }
        }

        public override double VisitMinExpr(TableParserParser.MinExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            Debug.WriteLine("min({0}, {1})", left, right);
            return Math.Min(left, right);
        }

        public override double VisitMaxExpr(TableParserParser.MaxExprContext context)
        {
            var left = WalkLeft(context);
            var right = WalkRight(context);
            Debug.WriteLine("max({0}, {1})", left, right);
            return Math.Max(left, right);
        }

        private double WalkLeft(TableParserParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<TableParserParser.ExpressionContext>(0));
        }

        private double WalkRight(TableParserParser.ExpressionContext context)
        {
            return Visit(context.GetRuleContext<TableParserParser.ExpressionContext>(1));
        }
    }
}
