# Libaec

Libaec is a .NET interop wrapper for the native [libaec](https://github.com/MathisRosenhauer/libaec) C implementation. It provides a managed interface to the library, allowing .NET developers to easily incorporate AEC (Adaptive Entropy Coding) decoding functionality into their projects. (managed API for encoding not yet implemented)

## Usage

Here's a basic example of how to use Libaec:

```csharp
using Libaec;

int sourceLength = ...
byte[] source = ...

var decoder = new AecDecoder(
	bitsPerSample: 32,
	flags: AecDataFlags.AEC_DATA_SIGNED | AecDataFlags.AEC_DATA_PREPROCESS,
	blockSize: 16,
	rsi: 128);

var dest = decoder.Decode(source, sourceLength, nbSamples);
```

For direct access to the libaec API, you can use the `Libaec.Interop` class with the `AecStream` struct.

## License

This project is licensed under the BSD 2-Clause "Simplified" License. See the LICENSE file for details.

## Acknowledgements

This project is a interop of the [libaec](https://github.com/MathisRosenhauer/libaec) library. Many thanks to the original authors and contributors of libaec : Mathis Rosenhauer, Moritz Hanke, Joerg Behrens, Luis Kornblueh.
