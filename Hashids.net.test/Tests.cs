using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace HashidsNet.test {
	[TestFixture]
	public class Tests {
		Hashids hashids;
		private string salt;
		private string alphabet;

		[SetUp]
		public virtual void SetUp() {
			salt = "this is my salt";
			alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
			hashids = new Hashids(salt: salt, alphabet: alphabet);
		}

		[TearDown]
		public virtual void Dispose() {
			hashids = null;
		}

		[Test]
		public void it_requires_an_alphabet() {
			Assert.Throws<ArgumentNullException>(() => new Hashids(alphabet: ""));
		}

		[Test]
		public void it_fails_on_whitespace_in_alphabet() {
			Assert.Throws<ArgumentException>(() => new Hashids(alphabet: "ab cd"));
		}

		[Test]
		public void it_fails_on_short_alphabet() {
			Assert.Throws<ArgumentException>(() => new Hashids(alphabet: "abcd"));
		}

		[Test]
		public void it_fails_on_short_alphabet_with_duplicates() {
			Assert.Throws<ArgumentException>(() => new Hashids(alphabet: "aaabbbcccddd"));
		}

		[Test]
		public void ConsistentShuffle_shuffles_identical_to_the_nodejs_lib() {
			var shuffleResultsFromNodeLib = new Dictionary<string, string>() { 
				{"564123", "123456"},
				{"sinypushttimi", "thisismyinput"},
				{"sftoidoeuoousdhwrn", "tddsureisfunwoohoo"}
			};
			foreach (var kvp in shuffleResultsFromNodeLib) {
				Assert.AreEqual(kvp.Key, Hashids.ConsistentShuffle(input: kvp.Value, salt: salt));
			}
		}

		[Test]
		public void it_encrypts_longs_identical_to_the_nodejs_lib() {
			var resultsFromNodeLib = new Dictionary<string, long>() { 
				{"y2jl7rm5", 1234567890},
				{"q2dxzp4vq", 9876543210},
				{"77m", 123},
				{"43l3w7", 789456}
			};
			foreach (var kvp in resultsFromNodeLib) {
				Assert.AreEqual(kvp.Key, hashids.Encrypt(kvp.Value));
			}
		}

		[Test]
		public void it_encrypts_and_decrypts_large_numbers() {
			for (long i = 1000000000L; i < 1000001000L; i++)
				Assert.AreEqual(i, hashids.DecryptOne(hashids.Encrypt(i)));
		}

		[Test]
		public void it_consistently_encrypts_large_numbers() {
			var store = new Dictionary<long, string>();
			for (long i = 1000000000L; i < 1000000100L; i++)
				store.Add(i, hashids.Encrypt(i));

			for (long i = 0; i < 10000; i++) {
				foreach (var item in store)
					Assert.AreEqual(item.Value, hashids.Encrypt(item.Key));
			}
		}

		[Test]
		public void it_encrypts_and_decrypts_lists_of_large_numbers() {
			var input = new List<long>();

			for (long i = 1000000000L, j = 1; i < 1000001000L; i++, j++) {
				input.Add(i);
				if (j % 5 == 0) {
					Assert.AreEqual(input, hashids.Decrypt(hashids.Encrypt(input.ToArray())));
					input = new List<long>();
				}
			}
		}

		[Test]
		public void it_consistently_encrypts_lists_of_large_numbers() {
			var store = new Dictionary<long[], string>();
			var input = new List<long>();

			for (long i = 1000000000L, j = 1; i < 1000000100L; i++, j++) {
				input.Add(i);
				if (j % 5 == 0) {
					store.Add(input.ToArray(), hashids.Encrypt(input.ToArray()));
					input = new List<long>();
				}
			}

			for (long i = 0; i < 1000; i++) {
				foreach (var item in store)
					Assert.AreEqual(item.Value, hashids.Encrypt(item.Key));
			}
		}

		[Test]
		public void it_encrypts_and_decrypts_small_numbers() {
			for (long i = 0L; i < 1000L; i++)
				Assert.AreEqual(i, hashids.DecryptOne(hashids.Encrypt(i)));
		}

		[Test]
		public void it_consistently_encrypts_small_numbers() {
			var store = new Dictionary<long, string>();
			for (long i = 0L; i < 100L; i++)
				store.Add(i, hashids.Encrypt(i));

			for (long i = 0; i < 1000; i++) {
				foreach (var item in store)
					Assert.AreEqual(item.Value, hashids.Encrypt(item.Key));
			}
		}

		[Test]
		public void it_encrypts_and_decrypts_lists_of_small_numbers() {
			var input = new List<long>();

			for (long i = 0l, j = 1; i < 100L; i++, j++) {
				input.Add(i);
				if (j % 5 == 0) {
					Assert.AreEqual(input, hashids.Decrypt(hashids.Encrypt(input.ToArray())));
					input = new List<long>();
				}
			}
		}

		[Test]
		public void it_consistently_encrypts_lists_of_small_numbers() {
			var store = new Dictionary<long[], string>();
			var input = new List<long>();

			for (long i = 0l, j = 1; i < 100L; i++, j++) {
				input.Add(i);
				if (j % 5 == 0) {
					store.Add(input.ToArray(), hashids.Encrypt(input.ToArray()));
					input = new List<long>();
				}
			}

			for (long i = 0; i < 1000; i++) {
				foreach (var item in store)
					Assert.AreEqual(item.Value, hashids.Encrypt(item.Key));
			}
		}

		[Test]
		public void it_encrypts_and_decrypts_random_numbers() {
			var random = new Random();
			int r = 0;
			for (long i = 0; i < 1000; i++) {
				r = random.Next();
				Assert.AreEqual(r, hashids.DecryptOne(hashids.Encrypt(r)));
			}
		}

	}
}
