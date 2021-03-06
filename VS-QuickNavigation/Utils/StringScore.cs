﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VS_QuickNavigation
{
	internal class StringScore
	{
		private static int CharScore(char a, char b)
		{
			if ( a == b )
			{
				return 2;
			}else if (char.ToUpperInvariant(a) == char.ToUpperInvariant(b))
			{
				return 1;
			}
			return 0;
		}

		
		public static int Search(string query, string content, List<Tuple<int,int>> matchIndexOut = null, int doubleScoreStart = 0)
		{
			if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(content))
			{
				return 0;
			}

			int queryIndex = query.Length - 1;
			int contentIndex = content.Length - 1;
			int totalScore = 0;
			int combo = 1;
			int? currentMatch = null;
			
			while (contentIndex >= 0)
			{
				//int bestScore = 0;
				//int bestScoreLastCharPos = 0;

				int charScore = CharScore(content[contentIndex], query[queryIndex]);
				if (charScore > 0)
				{
					if (!currentMatch.HasValue)
					{
						currentMatch = contentIndex;
					}

					int multScore = contentIndex >= doubleScoreStart ? 2 : 1; // Double score
					totalScore += charScore * combo * multScore;


					//if (charScore == 2) //To test : ignore
					{
						--queryIndex;
						++combo;

						if (queryIndex < 0)
						{
							--contentIndex;
							break;
						}
					}
				}
				else
				{
					combo = 1;
					if (null != matchIndexOut && currentMatch.HasValue)
					{
						Debug.Assert((contentIndex - currentMatch.Value) < 0);
						matchIndexOut.Insert(0, new Tuple<int, int>(contentIndex+1, currentMatch.Value - contentIndex));
						currentMatch = null;
					}
				}

				--contentIndex;
			}

			if (null != matchIndexOut && currentMatch.HasValue)
			{
				Debug.Assert((contentIndex - currentMatch.Value) < 0);
				matchIndexOut.Insert(0, new Tuple<int, int>(contentIndex+1, currentMatch.Value - contentIndex));
			}

			return totalScore;
		}

		/*void SearchBestToken(string query, string inText, int inTextStart, int queryStart, out int bestScore, out int startBest, out int len)
		{
			int tokenIndex = queryStart;
			int stringIndex = inTextStart;
			int totalScore = 0;
			int combo = 1;
			int? currentMatch = null;

			//int bestScore = 0;
			//int bestScoreLastCharPos = 0;

			while (stringIndex < inText.Length)
			{
				

				int charScore = CharScore(inText[stringIndex], query[tokenIndex]);
				if (charScore > 0)
				{
					if (!currentMatch.HasValue)
					{
						currentMatch = stringIndex;
					}

					int multScore = stringIndex >= doubleScoreStart ? 2 : 1; // Double score
					totalScore += charScore * combo * multScore;


					//if (charScore == 2) //To test : ignore
					{
						++tokenIndex;
						++combo;

						if (tokenIndex >= query.Length)
						{
							++stringIndex;
							break;
						}
					}
				}
				else
				{
					combo = 1;
					if (null != matchIndexOut && currentMatch.HasValue)
					{
						Debug.Assert((stringIndex - currentMatch.Value) > 0);
						matchIndexOut.Add(new Tuple<int, int>(currentMatch.Value, stringIndex - currentMatch.Value));
						currentMatch = null;
					}
				}

				++stringIndex;
			}
		}*/

		public static void FormatMatches(string sString, List<Tuple<int, int>> matches, List<Tuple<string, bool>> formatted, int start = 0/*, int end = -1*/)
		{
			formatted.Clear();
			if (matches.Count > 0)
			{
				int previousIndex = start;
				foreach (var match in matches)
				{
					if (match.Item1 < start && (match.Item1 + match.Item2) < start)
					{
						continue;
					}
					if (match.Item1 > start)
					{
						formatted.Add(Tuple.Create(sString.Substring(previousIndex, match.Item1 - previousIndex), false));
					}
					formatted.Add(Tuple.Create(sString.Substring(match.Item1, match.Item2), true));

					previousIndex = match.Item1 + match.Item2;
				}

				Tuple<int, int> lastMatch = matches[matches.Count - 1];
				if ((lastMatch.Item1 + lastMatch.Item2) < sString.Length)
				{
					formatted.Add(Tuple.Create(sString.Substring(Math.Max(start,lastMatch.Item1 + lastMatch.Item2)), false));
				}
			}
			else
			{
				formatted.Add(Tuple.Create(sString.Substring(start), false));
			}
		}

		public static List<string> Search(
			string search,
			string words,
			double fuzzyness)
		{
			IEnumerable<string> wordList = words.Split(
				'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
				'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
				'U', 'V', 'W', 'X', 'Y', 'Z', ' ', '-', '_');
			return Search(search, wordList, fuzzyness);
		}

		public static List<string> Search(
			string word,
			IEnumerable<string> wordList,
			double fuzzyness)
		{
			List<string> foundWords = new List<string>();

			foreach (string s in wordList)
			{
				// Calculate the Levenshtein-distance:
				int levenshteinDistance =
					LevenshteinDistance(word, s);

				// Length of the longer string:
				int length = Math.Max(word.Length, s.Length);

				// Calculate the score:
				double score = 1.0 - (double)levenshteinDistance / length;

				// Match?
				if (score > fuzzyness)
					foundWords.Add(s);
			}
			return foundWords;
		}

		public static int LevenshteinDistance(string a, string b)
		{
			if (String.IsNullOrEmpty(a) || String.IsNullOrEmpty(b)) return 0;

			int lengthA = a.Length;
			int lengthB = b.Length;
			var distances = new int[lengthA + 1, lengthB + 1];
			for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
			for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

			for (int i = 1; i <= lengthA; i++)
				for (int j = 1; j <= lengthB; j++)
				{
					int cost = b[j - 1] == a[i - 1] ? 0 : 1;
					distances[i, j] = Math.Min
						(
						Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
						distances[i - 1, j - 1] + cost
						);
				}
			return distances[lengthA, lengthB];
		}
	}
}