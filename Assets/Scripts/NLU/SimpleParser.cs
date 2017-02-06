using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Assets.Scripts.NLU
{
	public class SimpleParser : NLParser {

		private List<string> _events = new List<string>(new[]
		{
			"grasp",
			"hold",
			"touch",
			"move",
			"turn",
			"roll",
			"spin",
			"stack",
			"put",
			"lean on",
			"lean against",
			"flip on edge",
			"flip at center",
			"flip",
			"close",
			"open",
			"lift",
			"drop",
			"reach",
			"slide"});

		private List<string> _objects = new List<string>(new[]
		{
			"block",
			"ball",
			"plate",
			"cup",
			"cup1",
			"cup2",
			"cup3",
			"cups",
			"disc",
			"spoon",
			"book",
			"blackboard",
			"bottle",
			"grape",
			"apple",
			"banana",
			"table",
			"bowl",
			"knife",
			"pencil",
			"paper sheet",
			"hand",
			"arm",
			"mug",
			"block1",
			"block2",
			"block3",
			"block4",
			"block5",
			"block6",
			"blocks",
			"lid",
			"stack",
			"staircase",
			"pyramid"
		});


		private List<string> _relations = new List<string>(new[]
		{
			"touching",
			"in",
			"on",
			"behind",
			"in front of",
			"near",
			"left of",
			"right of",
			"center of",
			"under"
		});

		private List<string> _attribs = new List<string>(new[]
		{
			"brown",
			"blue",
			"black",
			"green",
			"yellow",
			"red",
			"leftmost",
			"middle",
			"rightmost"
		});

		private List<string> _determiners = new List<string>(new[]
		{
			"a",
			"two"
		});

		private List<string> _exclude = new List<string>(new[]
		{
			"the"
		});

		private string[] SentSplit(string sent)
		{
			var tokens = new List<string>(Regex.Split(sent.ToLower(), " +"));
			return tokens.Where(token => !_exclude.Contains(token)).ToArray();
		}

		public string NLParse(string rawSent)
		{
			var tokens = SentSplit(rawSent);
			var form = tokens[0] + "(";
			var cur = 1;
			var end = tokens.Length;
			while (cur < end)
			{
				if (cur + 2 < end &&
					tokens[cur] == "in" && tokens[cur + 1] == "front" && tokens[cur + 2] == "of")
				{
					form += ",in_front(";
					cur += 3;
				}
				else if (cur + 1 < end &&
						 tokens[cur] == "left" && tokens[cur + 1] == "of")
				{
					form += ",left(";
					cur += 2;
				}
				else if (cur + 1 < end &&
						 tokens[cur] == "right" && tokens[cur + 1] == "of")
				{
					form += ",right(";
					cur += 2;
				}
				else if (cur + 1 < end &&
						 tokens[cur] == "center" && tokens[cur + 1] == "of")
				{
					form += ",center(";
					cur += 2;
				}
				else if (cur + 1 < end &&
						 tokens[cur] == "paper" && tokens[cur + 1] == "sheet")
				{
					form += "paper_sheet";
					cur += 2;
				}
				else if (_relations.Contains(tokens[cur]))
				{
					if (form.EndsWith("("))
					{
						form += tokens[cur] + "(";
					}
					else
					{
						form += "," + tokens[cur] + "(";
					}
					cur += 1;
				}
				else if (_attribs.Contains(tokens[cur]) ||
						 _determiners.Contains(tokens[cur]))
				{
					form += tokens[cur] + "(" + tokens[cur + 1] + ")";
					cur += 2;
				}
				else if (_objects.Contains(tokens[cur]))
				{
					form += tokens[cur];
				    cur++;
				}
				else
				{
				    cur++;
				}
			}
			var opens = form.Count(c => c == '(');
			var closes = form.Count(c => c == ')');
		    for (int i = closes; i < opens; i++)
		    {
		        form += ")";
		    }
//			form += string.Concat(Enumerable.Repeat(")", opens - closes));
			return form;
		}

		public void InitParserService(string address) {
			// do nothing
		}
	}
}