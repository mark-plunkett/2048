open System.Numerics
open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86

module Constants =

    let firstOfEachPairMask = Vector256.Create(
        0uy,
        2uy,
        4uy,
        6uy,
        8uy,
        10uy,
        12uy,
        14uy,
        0uy, 0uy, 0uy, 0uy, 0uy, 0uy, 0uy, 0uy,
        16uy,
        18uy,
        20uy,
        22uy,
        24uy,
        26uy,
        28uy,
        30uy,
        0uy, 0uy, 0uy, 0uy, 0uy, 0uy, 0uy, 0uy)

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

    let antiClockwiseMasku = Vector128.Create(
        3y,
        7y,
        11y,
        15y,
        2y,
        6y,
        10y,
        14y,
        1y,
        5y,
        9y,
        13y,
        0y,
        4y,
        8y,
        12y)

let cellsArray = [|
    25s
    1s;
    2s;
    3s;
    4s;
    5s;
    6s;
    7s;
    8s;
    9s;
    10s;
    11s;
    12s;
    13s;
    14s;
    15s;
    |]

let get () =
    use ptr = fixed cellsArray
    Avx.LoadVector256 (ptr)
    
let cells = get ()

let c1 = Vector (cellsArray)
let c2 = Vector.Narrow (c1, c1)
let c3 = c2.AsVector128 ()
let c4 = Ssse3.Shuffle (c3, Constants.antiClockwiseMasku)
let c5 = c4.AsVector ()
let mutable v1 = Vector ()
let mutable discard = Vector ()
let c6 = Vector.Widen (c5, &v1, &discard)
v1

let cellsBytes = cells.AsByte ()
let cellsPacked = Avx2.Shuffle (cellsBytes, Constants.firstOfEachPairMask)
let cells128 = cellsPacked.GetLower().WithUpper(cellsPacked.GetUpper().GetLower())
let res = Ssse3.Shuffle (cells128, Constants.antiClockwiseMask)
let res256 = res.ToVector256 ()
let unpackedLow = Avx2.UnpackLow (res256, Vector256.Zero)
let unpackedHigh = Avx2.UnpackHigh (res256, Vector256.Zero)
let unpacked = unpackedLow.WithUpper (unpackedHigh.GetLower())
unpacked.AsInt16 ()
