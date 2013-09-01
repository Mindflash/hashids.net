#hashids.net

A .NET version of Ivan Akimov's NodeJS hashids library. 

[![Build Status](https://travis-ci.org/Mindflash/hashids.net.png?branch=master)](https://travis-ci.org/Mindflash/hashids.net)

### Usage:
```csharp
// all params are optional (though you really should use a salt)
var hashids = new Hashids(
  salt: "this is my salt", 
  alphabet: "abcdefghijklmnopqrstuvwxyz0123456789", 
  minHashLength: 0
);

// encrypting 

// a simple long
var encrypted = hashids.Encrypt(1234567890); // result: "y2jl7rm5"

// encrypting N longs
encrypted = hashids.Encrypt(1234567890,9876543210,654987321,456123789)); 
// result: "jxypk9w2frmlyvk19cqjr8jmeapj34ry7"

// decrypting

var decryptedLong = hashids.Decrypt("y2jl7rm5"); // result: List<long>(){ 1234567890 }
// if you're only fetching 1 value, you can always call Decrypt(input)..FirstOrDefault();

var decryptedListOfLongs = hashids.Decrypt("jxypk9w2frmlyvk19cqjr8jmeapj34ry7"); 
// result: List<long>(){ 1234567890,9876543210,654987321,456123789 }
```
