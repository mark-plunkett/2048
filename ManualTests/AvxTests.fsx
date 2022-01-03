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
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
    0uy,
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
    0uy)

let cells = Vector256.Create(
    1s,
    2s,
    3s,
    4s,
    5s,
    6s,
    7s,
    8s,
    9s,
    10s,
    11s,
    12s,
    13s,
    14s,
    15s,
    16s)

printfn "cells %A" cells

let cellsBytes = cells.AsByte()

printfn "cellsBytes %A" cellsBytes

let cellsPacked = Avx2.Shuffle (cellsBytes, firstOfEachPairMask)

printfn "cellsPacked %A" cellsPacked

let cells128 = cellsPacked.GetLower().WithUpper(cellsPacked.GetUpper().GetLower())

printfn "cells128 %A" cells128

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

let res = Ssse3.Shuffle (cells128, antiClockwiseMask)

printfn "res %A" res

let res256 = res.ToVector256()

printfn "res256 %A" res256

let unpackedLow = Avx2.UnpackLow (res256, Vector256.Zero)
let unpackedHigh = Avx2.UnpackHigh (res256, Vector256.Zero)
let unpacked = unpackedLow.WithUpper (unpackedHigh.GetLower())

printfn "unpacked %A" unpacked

let unpacked16s = unpacked.AsInt16 ()

printfn "unpacked16s %A" unpacked16s
