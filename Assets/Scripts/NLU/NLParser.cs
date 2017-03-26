namespace Assets.Scripts.NLU
{
	public interface NLParser
	{
		string NLParse(string rawSent);

		void InitParserService(string address);
	}
}