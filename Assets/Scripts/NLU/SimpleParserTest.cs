#if !UNITY_EDITOR && !UNITY_STANDALONE
using System;
using NUnit.Framework;

namespace Assets.Scripts.NLU
{
    [TestFixture]
    public class SimpleParserTest
    {
		NLParser parser;

		[SetUp]
		protected void SetUp()
		{
			parser = new SimpleParser();
		}

        [Test]
        public void ShowParse()
        {
			Console.WriteLine(parser.NLParse("put apple on the red cup"));
			Console.WriteLine(parser.NLParse("put the red block behind the blue block"));
			Console.WriteLine(parser.NLParse("put the red block on the blue block"));
			Console.WriteLine(parser.NLParse("put the red block in front of the blue block"));
			Console.WriteLine(parser.NLParse("put the red leftmost block in front of the blue block"));
        }
    }

}
#endif
