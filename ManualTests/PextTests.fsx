
open System
open System.Numerics
open System.Runtime.Intrinsics
open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86

module Constants =
    let firstBit = 0b11111111uy

let buildIndexBranchless (cells : Span<int16>) =

    let vCells = Vector<int16> (cells)
    let vGreaterThanZero = Vector.GreaterThan (vCells, Vector.Zero)

    printfn "vGreater %A" vGreaterThanZero

    let vBytes = Vector.Narrow (vGreaterThanZero, vGreaterThanZero)

    printfn "vBytes %A" vBytes

    let vLower = vBytes.AsVector128 ()

    let index = Sse2.MoveMask (vLower)

    index

let cells = [|
    1s; 2s; 0s; 4s;
    0s; 0s; 7s; 8s;
    0s; 0s; 0s; 0s;
    0s; 3s; 0s; 9s |]

let index = buildIndexBranchless cells
printfn "index %B" index