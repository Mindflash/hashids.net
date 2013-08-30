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
		public void it_consistently_shuffles() { // down the sidewalk
			Assert.AreEqual("564123", Hashids.ConsistentShuffle(input: "123456", salt: "this is my salt"));
			Assert.AreEqual("sinypushttimi", Hashids.ConsistentShuffle(input: "thisismyinput", salt: "this is my salt"));
			Assert.AreEqual("sftoidoeuoousdhwrn", Hashids.ConsistentShuffle(input: "tddsureisfunwoohoo", salt: "this is my salt"));
		}
		
		[Test]
		public void it_encrypts_longs() {
			Assert.AreEqual("y2jl7rm5", hashids.Encrypt(1234567890));
			Assert.AreEqual("q2dxzp4vq", hashids.Encrypt(9876543210));
			Assert.AreEqual("77m", hashids.Encrypt(123));
			Assert.AreEqual("43l3w7", hashids.Encrypt(789456));
		}

		[Test]
		public void it_decrypts_longs() {
			Assert.AreEqual(1234567890, hashids.Decrypt("y2jl7rm5").First());
			Assert.AreEqual(9876543210, hashids.Decrypt("q2dxzp4vq").First());
			Assert.AreEqual(123, hashids.Decrypt("77m").First());
			Assert.AreEqual(789456, hashids.Decrypt("43l3w7").First());
		}

	}
}
