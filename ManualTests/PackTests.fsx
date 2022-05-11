open System
open System.Collections
open System.Numerics
open System.Text

let inline collapse16 i =
    -(-i >>> 15)

let inline collapse32 i =
    -(-i >>> 31)

// Return the shuffle map needed to pack cells
let cells = [|
    1s; 2s; 0s; 4s;
    0s; 0s; 7s; 8s;
    0s; 0s; 0s; 0s;
    0s; 3s; 0s; 9s |]

// should produce
// 1 1 0 1, 0 0 1 1, 0 0 0 0, 0 0 0 1
// 1 1 1 0, 1 1 0 0, 0 0 0 0, 1 0 0 0

let temp () =

    let i1 = cells.[1] |> collapse16
    let v1 = Vector  (cells, 0)
    let v2 = Vector.GreaterThan (v1, Vector.Zero) |> Vector.Abs
    let cellBits = Array.zeroCreate<int16> 16
    v2.CopyTo (cellBits)


let buildIndex (cells: int16[]) =

    let mutable index = 0
    let i0 = cells.[0] |> collapse16 |> int
    index <- index ||| (i0 <<< 0)
    let i1 = cells.[1] |> collapse16 |> int
    index <- index ||| (i1 <<< 1)
    let i2 = cells.[2] |> collapse16 |> int
    index <- index ||| (i2 <<< 2)
    let i3 = cells.[3] |> collapse16 |> int
    index <- index ||| (i3 <<< 3)
    let i4 = cells[4]  |> collapse16 |> int
    index <- index ||| (i4 <<< 4)
    let i5 = cells[5]  |> collapse16 |> int
    index <- index ||| (i5 <<< 5)
    let i6 = cells[6]  |> collapse16 |> int
    index <- index ||| (i6 <<< 6)
    let i7 = cells[7]  |> collapse16 |> int
    index <- index ||| (i7 <<< 7)
    let i8 = cells[8]  |> collapse16 |> int
    index <- index ||| (i8 <<< 8)
    let i9 = cells[9]  |> collapse16 |> int
    index <- index ||| (i9 <<< 9)
    let i10 = cells[10] |> collapse16 |> int
    index <- index ||| (i10 <<< 10)
    let i11 = cells[11] |> collapse16 |> int
    index <- index ||| (i11 <<< 11)
    let i12 = cells[12] |> collapse16 |> int
    index <- index ||| (i12 <<< 12)
    let i13 = cells[13] |> collapse16 |> int
    index <- index ||| (i13 <<< 13)
    let i14 = cells[14] |> collapse16 |> int
    index <- index ||| (i14 <<< 14)
    let i15 = cells[15] |> collapse16 |> int
    index <- index ||| (i15 <<< 15)
    index

let index = buildIndex cells
printfn "%B" index

// 0 0 1 2
// 3 4 0 0

let buildMask (s : string) =
    let a = Array.zeroCreate 16
    let chars = s.ToCharArray()
    for k = 0 to 3 do
        for i = 0 to 3 do
            let baseIdx = (k * 4)
            let idx = baseIdx + i
            let mutable j = idx
            while chars[j] = '0' && j < baseIdx + 3 do
                j <- j + 1
                
            a[idx] <- j
            chars[idx] <- '0'
            chars[j] <- '0'
    
    a

let m = buildMask "0011000110000000"

let pack (cells:int16[]) =
    // Indicies account for vector padding
    let a = Array.zeroCreate 16
    let mutable i = 0
    for j = 1 to cells.Length - 1 do
        let vi = cells.[i]
        let vj = cells.[j]
        if (j % 4) = 1 then
            i <- j
        elif vi > 0s then
            i <- i + 1
        elif vj > 0s then
            cells.[i] <- vj
            cells.[j] <- 0s
            i <- i + 1
        
    cells


let t = buildMask "0010000000000000"
t

for i = 0 to 4 do //int UInt16.MaxValue do
    // get string
    let iString = 
        Convert.ToString (i, 2)
        |> fun s -> s.PadLeft (16, '0')
        |> fun s -> s.ToCharArray () |> Array.map string
        |> Array.map Int16.Parse
        |> buildIndex

    printfn "%i" iString