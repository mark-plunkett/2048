open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86

let x = 40

printfn "%B" x

let firstOfEachPairMask = Vector256.Create(
    0uy,
    2uy,
    4uy,
    6uy,
    8uy,
    10uy,
    12uy,
    14uy,
    16uy,
    18uy,
    20uy,
    22uy,
    24uy,
    26uy,
    28uy,
    30uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy)

let cells1 = Vector256.Create(
    0s,
    1s,
    2s,
    3s,
    4s,
    5s,
    6s,
    7s,
    0s,
    0s,
    0s,
    0s,
    0s,
    0s,
    0s,
    0s)

printfn "cells %A" cells1

let cells2 = Vector256.Create(
    8s,
    9s,
    10s,
    11s,
    12s,
    13s,
    14s,
    15s,
    0s,
    0s,
    0s,
    0s,
    0s,
    0s,
    0s,
    0s)

printfn "cells2 %A" cells2

let cells1Bytes = cells1.AsByte()

printfn "cells1Bytes %A" cells1Bytes

let cells1Packed = Avx2.Shuffle (cells1Bytes, firstOfEachPairMask)

printfn "cells1Packed %A" cells1Packed

let cells2Bytes = cells2.AsByte()
let cells2Packed = Avx2.Shuffle (cells2Bytes, firstOfEachPairMask)

printfn "cells2Packed %A" cells2Packed

let combined = Avx2.BlendVariable (
    cells1Packed,
    cells2Packed,
    Vector256.Create(
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        System.Byte.MaxValue,
        System.Byte.MaxValue,
        System.Byte.MaxValue,
        System.Byte.MaxValue,
        System.Byte.MaxValue,
        System.Byte.MaxValue,
        System.Byte.MaxValue,
        System.Byte.MaxValue,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy,
        0uy))

printfn "combined %A" combined

let final128 = combined.GetLower()

let antiClockwiseMask = Vector128.Create(
    3uy,
    7uy,
    11uy,
    15uy,
    2uy,
    6uy,
    10uy,
    14uy,
    1uy,
    5uy,
    9uy,
    13uy,
    0uy,
    4uy,
    8uy,
    12uy)

let res = Avx2.Shuffle (final128, antiClockwiseMask)

printfn "res %A" res
