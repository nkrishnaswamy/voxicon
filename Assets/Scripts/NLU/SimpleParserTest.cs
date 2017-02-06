#if !(UNITY_EDITOR)
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
			Console.WriteLine(parser.NLParse("put apple on the cup"));
        }
    }

}
#endif
