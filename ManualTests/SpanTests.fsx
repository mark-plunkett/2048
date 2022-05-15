open System
open System.Numerics

let cells = [| 
    0s
    0s; 0s; 0s; 0s
    0s; 0s; 0s; 2s
    0s; 0s; 0s; 0s
    0s; 0s; 0s; 2s
|]

let process () =
    let span = Span<int16> (cells, 1, 16)
    Vector<int16> (span)

let vec = process ()
Vector<int16>.Count