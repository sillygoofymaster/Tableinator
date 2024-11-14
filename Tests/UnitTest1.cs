using NUnit.Framework;
using TableParser;
using Calculator = TableParser.Calculator;

namespace UnitTests
{
    public class CalculatorTests
    {
        [Test]
        public void EvaluateTestBinaryAddition()
        {
            Assert.That(Calculator.Evaluate("1+4"), Is.EqualTo(5));
        }

        [Test]
        public void EvaluateTestBinarySubtraction()
        {
            Assert.That(Calculator.Evaluate("8-6"), Is.EqualTo(2));
        }

        [Test]
        public void EvaluateTestBinaryMultiplication()
        {
            Assert.That(Calculator.Evaluate("13*13"), Is.EqualTo(169));
        }

        [Test]
        public void EvaluateTestBinaryDivision()
        {
            Assert.That(Calculator.Evaluate("27/9"), Is.EqualTo(3));
        }

        [Test]
        public void EvaluateTestMod()
        {
            Assert.That(Calculator.Evaluate("mod(9, 5)"), Is.EqualTo(4));
        }

        [Test]
        public void EvaluateTestDiv()
        {
            Assert.That(Calculator.Evaluate("div(18, 7)"), Is.EqualTo(2));
        }

        [Test]
        public void EvaluateTestExponent()
        {
            Assert.That(Calculator.Evaluate("4^2"), Is.EqualTo(16));
            Assert.That(Calculator.Evaluate("2^(2^2)"), Is.EqualTo(16));
        }

        [Test]
        public void EvaluateTestMin()
        {
            Assert.That(Calculator.Evaluate("min(-3, 6)"), Is.EqualTo(-3));
        }

        [Test]
        public void EvaluateTestMax()
        {
            Assert.That(Calculator.Evaluate("max(-3, 6)"), Is.EqualTo(6));
        }
    }
}