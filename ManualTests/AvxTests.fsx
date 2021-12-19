open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86

let x = 40

printfn "%B" x

let v1 = Vector256.Create(
    0s,
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
    15s)

printfn "v1 %A" v1

let v1Bytes = v1.AsSByte()

printfn "v1b %A" v1Bytes

(*
    [0 1 2 3]
    [0 1]
    [2 3]

    rotate anti-clockwise
    [1 3 0 2]
    [1 3]
    [0 2]

    [1 3 0 2]

    [0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15]
    [0 1 2 3]
    [4 5 6 7]
    [8 9 10 11]
    [12 13 14 15]

    rotate anti-clockwise
    [ 3 7 11 15 2 6 10 14 1 5 9 13 0 4 8 12 ]

*)

let antiClockwiseMask = Vector256.Create(
    6y, 7y,
    14y, 15y,
    22y, 23y,
    30y, 31y,
    4y, 5y,
    12y, 13y,
    20y, 21y,
    28y, 29y,
    2y, 3y,
    10y, 11y,
    18y, 19y,
    26y, 27y,
    0y, 1y,
    8y, 9y,
    16y, 17y,
    24y, 25y)

let antiClockwiseMaskBytes = antiClockwiseMask.AsSByte()
printfn "mask bytes %A" antiClockwiseMaskBytes

let res = Avx2.Shuffle (v1Bytes, antiClockwiseMaskBytes) |> Vector256.AsUInt16

printfn "res %A" res
