using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HashidsNet {
	public class Hashids {
		public const string DEFAULT_ALPHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
		public const string DEFAULT_SEPARATORS = "cfhistuCFHISTU";
		public const string VERSION = "0.3.0";
		public const int MIN_ALPHABET_LENGTH = 16;
		public const int MIN_HASH_LENGTH = 0;

		public string Salt { get; private set; }
		public int MinHashLength { get; private set; }
		public string Alphabet { get; private set; }
		public string Separators { get; private set; }
		public string Guards { get; private set; }

		public Hashids(string salt = "", int minHashLength = MIN_HASH_LENGTH, string alphabet = DEFAULT_ALPHABET) {
			if (string.IsNullOrWhiteSpace(alphabet))
				throw new ArgumentNullException("alphabet");

			if (alphabet.Contains(' '))
				throw new ArgumentException("alphabet cannot contain spaces", "alphabet");

			Alphabet = new String(alphabet.Distinct().ToArray());

			Salt = salt;
			MinHashLength = minHashLength;

			// separators should be in the alphabet
			Separators = DEFAULT_SEPARATORS;
			Separators = new String(Separators.Where(x => Alphabet.Contains(x)).ToArray());

			if (string.IsNullOrWhiteSpace(Separators))
				throw new ArgumentException("alphabet does not contain separators", "separators");

			Separators = ConsistentShuffle(input: Separators, salt: Salt);

			// remove separator characters from the alphabet
			Alphabet = new String(Alphabet.Where(x => !Separators.Contains(x)).ToArray());

			if (string.IsNullOrWhiteSpace(Alphabet) || Alphabet.Length < MIN_ALPHABET_LENGTH)
				throw new ArgumentException("alphabet must contain atleast " + MIN_ALPHABET_LENGTH + " unique, non-separator characters.", "alphabet");

			double sepDivisor = 3.5;
			if (Separators.Length == 0 || (Alphabet.Length / Separators.Length) > sepDivisor) {
				int sepsLength = (int)Math.Ceiling(Alphabet.Length / sepDivisor);

				if (sepsLength == 1)
					sepsLength++;

				if (sepsLength > Separators.Length) {
					int diff = sepsLength - Separators.Length;
					Separators += Alphabet.Substring(0, diff);
					Alphabet = Alphabet.Substring(diff);
				} else {
					Separators = Separators.Substring(0, sepsLength);
				}
			}

			double guardDivisor = 12.0;
			Alphabet = ConsistentShuffle(input: Alphabet, salt: Salt);
			int guardCount = (int)Math.Ceiling(Alphabet.Length / guardDivisor);

			if (Alphabet.Length < 3) {
				Guards = Separators.Substring(0, guardCount);
				Separators = Separators.Substring(guardCount);
			} else {
				Guards = Alphabet.Substring(0, guardCount);
				Alphabet = Alphabet.Substring(guardCount);
			}
		}

		public string Encrypt(List<long> input) {
			return Encrypt(input: input.ToArray());
		}

		public string Encrypt(params long[] input) {
			StringBuilder ret = new StringBuilder();
			int numbersSize = input.Length;
			int numbersHashInt = 0;
			string alphabet = Alphabet;

			for (int i = 0; i < numbersSize; i++) {
				numbersHashInt += (int)(input[i] % (i + 100));
			}

			char lottery = alphabet[numbersHashInt % alphabet.Length];
			ret.Append(lottery);

			for (int i = 0; i < numbersSize; i++) {
				long number = input[i];
				string buffer = lottery + Salt + alphabet;

				alphabet = ConsistentShuffle(alphabet, buffer.Substring(0, alphabet.Length));

				StringBuilder last = Hash(number, alphabet);
				ret.Append(last);

				if (i + 1 < input.Length) {
					number %= (int)(last[0]) + i;
					int sepsIndex = (int)(number % Separators.Length);
					ret.Append(Separators[sepsIndex]);
				}
			}

			if (ret.Length < MinHashLength) {
				int guardIndex = (numbersHashInt + (int)(ret[0])) % Guards.Length;
				char guard = Guards[guardIndex];
				ret.Insert(0, guard);

				if (ret.Length < MinHashLength) {
					guardIndex = (numbersHashInt + (int)(ret[2])) % Guards.Length;
					guard = Guards[guardIndex];
					ret.Append(guard);
				}
			}

			int halfLength = (int)(alphabet.Length / 2);
			while (ret.Length < MinHashLength) {
				alphabet = ConsistentShuffle(alphabet, alphabet);

				ret.Insert(0, alphabet.Substring(halfLength));
				ret.Append(alphabet.Substring(0, halfLength));

				int excess = ret.Length - MinHashLength;
				if (excess > 0) {
					ret.Remove(0, excess / 2);
					ret.Remove(MinHashLength, ret.Length - MinHashLength);
				}
			}

			return ret.ToString();
		}

		public long DecryptOne(string input) {
			return Decrypt(input: input).FirstOrDefault();
		}

		public List<long> Decrypt(string input) {
			List<long> ret = new List<long>();
			string alphabet = Alphabet;

			Regex guardsRegex = new Regex("[" + Guards + "]");
			string[] hashArray = guardsRegex.Split(input);

			int i = 0;
			if (hashArray.Length == 3 || hashArray.Length == 2)
				i = 1;

			string hashBreakdown = hashArray[i];
			// TODO - add loads of error checking
			char lottery = hashBreakdown[0];
			hashBreakdown = hashBreakdown.Substring(1);

			hashArray = new Regex("[" + Separators + "]").Split(hashBreakdown);

			for (i = 0; i < hashArray.Length; i++) {
				string subHash = hashArray[i];
				string buffer = lottery + Salt + alphabet;

				alphabet = ConsistentShuffle(alphabet, buffer.Substring(0, alphabet.Length));
				ret.Add(Unhash(subHash, alphabet));
			}

			if (Encrypt(ret.ToArray()) != input) {
				ret = new List<long>();
			}

			return ret;
		}

		private StringBuilder Hash(long input, string alphabet) {
			StringBuilder hash = new StringBuilder();

			do {
				hash.Insert(0, alphabet[(int)(input % alphabet.Length)]);
				input = (int)(input / alphabet.Length);
			} while (input != 0);

			return hash;
		}

		private long Unhash(string input, string alphabet) {
			long ret = 0;
			int pos;

			for (int i = 0; i < input.Length; i++) {
				pos = alphabet.IndexOf(input[i]);
				ret += (long)(pos * Math.Pow(alphabet.Length, input.Length - i - 1));
			}

			return ret;
		}

		public static string ConsistentShuffle(string input, string salt) {
			if (String.IsNullOrEmpty(salt))
				return input;

			int v = 0, p = 0, j = 0, val = 0;
			int saltLength = salt.Length;
			var alphabetArray = input.ToCharArray();

			for (int i = alphabetArray.Length - 1; i > 0; i--, v++) {
				v %= saltLength;
				p += val = (int)salt[v];
				j = (val + v + p) % i;

				char char0 = alphabetArray[j];
				char char1 = alphabetArray[i];
				alphabetArray[j] = char1;
				alphabetArray[i] = char0;
			}

			return new String(alphabetArray);
		}

	}
}
