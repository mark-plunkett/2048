module GameSIMDBranchless

#nowarn "9"

open Microsoft.FSharp.NativeInterop
open System
open System.Collections.Generic
open System.Numerics
open System.Runtime.CompilerServices
open System.Runtime.Intrinsics
open System.Runtime.Intrinsics.X86

open Common

type ArrayPool<'t>(aFactory:_ -> 't) =

    let unrented = Stack<'t>(Array.map aFactory [|1..100|])
    
    member _.Rent() =
        unrented.Pop()

    member _.Return(a) =
        unrented.Push(a)

type Board = 
    val mutable Score: int
    val Size: int
    val Cells: int16[]
    val RNG: Random
    val RNGSeed: int option
    new (size:int, cells:int16[], rng:Random, rngSeed:int option) = {
        Score= 0
        Size = size
        Cells = cells
        RNG = rng
        RNGSeed = rngSeed
    }

    member this.SetScore(score) =
        this.Score <- score

//type Board(size, cells:int16[], rng, rngSeed) = 
//    member val Score = 0 with get, set
//    member _.Size = size
//    member _.Cells = cells
//    member _.RNG = rng
//    member _.RNGSeed = rngSeed

module Board =

    type PosGenerator() =
        static let ps = [|3;7;15;8;14;11;0;9;12;1;4;13;10;6;5;2;11|]
        static let mutable i = 0

        static member Next() =
            i <- i + 1
            if i >= ps.Length then i <- 0
            ps.[i]

    let boardSize size = 2 + (size * size)
    let emptyCells size =
        let cells = Array.zeroCreate (boardSize size)
        cells.[0] <- -1s
        cells.[17] <- -1s
        cells

    let empty (size:int) =
        Board(size, emptyCells size, Random(), None)
            
    let emptyWithSeed size seed =
        Board(size, emptyCells size, Random(seed), Some seed)

    let randomValue (board:Board) =
        int16 <| if board.RNG.NextDouble() > 0.9 then 2 else 1

    let addRandomCell (board:inref<Board>) =
        let rec addRec value (board:Board) =
            let i = 1 + PosGenerator.Next()
            if board.Cells.[i] = 0s then
                board.Cells.[i] <- value
                board
            else
                addRec value board
        let value = randomValue board
        addRec value board

    let init (board:Board) =
        addRandomCell &board |> ignore
        addRandomCell &board |> ignore
        board

    let create = empty >> init

    let clone (board:Board) =
        let random =
            match board.RNGSeed with
            | Some s -> Random(s)
            | None -> Random()
        Board(board.Size, Array.copy board.Cells, random, board.RNGSeed)

    let toString (board:Board) =
        let newLine = Environment.NewLine
        board.Cells
        |> Array.skip 1
        |> Array.take 16
        |> Array.indexed
        |> Array.fold (fun acc (i, v) ->
            acc
                + if i % board.Size = 0 then newLine + newLine else String.Empty
                + match v with
                    | 0s -> "    -"
                    | v -> sprintf "%5i" <| int (2.0 ** float v)
        ) newLine

    let fromList (cells:int list list) =
        let empty = emptyCells 4
        let cells' =
            cells
            |> List.collect id
            |> List.toArray
            |> Array.map int16
        Array.blit cells' 0 empty 1 (cells'.Length)
        empty

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
        
    let antiClockwiseTransposeMap = Vector128.Create(
        3uy, 7uy, 11uy, 15uy,
        2uy, 6uy, 10uy, 14uy,
        1uy, 5uy, 9uy, 13uy,
        0uy, 4uy, 8uy, 12uy)

    let clockwiseTransposeMap = Vector128.Create(
        12uy, 8uy, 4uy, 0uy,
        13uy, 9uy, 5uy, 1uy,
        14uy, 10uy, 6uy, 2uy,
        15uy, 11uy, 7uy, 3uy)

    let flipTransposeMap = Vector128.Create(
        3uy, 2uy, 1uy, 0uy,
        7uy, 6uy, 5uy, 4uy,
        11uy, 10uy, 9uy, 8uy,
        15uy, 14uy, 13uy, 12uy)

    let rotateRightMask = Vector128.Create(
        15uy, 0uy, 1uy, 2uy,
        3uy, 4uy, 5uy, 6uy,
        7uy, 8uy, 9uy, 10uy,
        11uy, 12uy, 13uy, 14uy)

    let vIgnoreRowStartIndicies = Vector([|
        0s
        -1s
        -1s
        -1s
        0s
        -1s
        -1s
        -1s
        0s
        -1s
        -1s
        -1s
        0s
        -1s
        -1s
        -1s |])

    let vIgnoreRowEndIndicies = Vector([|
        -1s
        -1s
        -1s
        0s
        -1s
        -1s
        -1s
        0s
        -1s
        -1s
        -1s
        0s
        -1s
        -1s
        -1s
        0s |])

let dump msg (o : Vector<int16>) = 
    // let o' = [4..7] |> List.map (fun i -> sprintf "%i " o.[i]) |> fun s -> String.Join ("", s)
    printfn "%30s: %A" msg o
    o

let boardPool = ArrayPool(fun _ -> GameSIMD.Board.emptyCells 4)
let board16Pool = ArrayPool(fun _ -> Array.zeroCreate<int16> 16)

let inline shuffleVec (cells : Vector256<int16>) mask =

    let cellsBytes = cells.AsByte ()
    let cellsPacked = Avx2.Shuffle (cellsBytes, Constants.firstOfEachPairMask)
    let cells128 = cellsPacked.GetLower().WithUpper(cellsPacked.GetUpper().GetLower())
    let res = Ssse3.Shuffle (cells128, mask)
    let res256 = res.ToVector256 ()
    let unpackedLow = Avx2.UnpackLow (res256, Vector256.Zero)
    let unpackedHigh = Avx2.UnpackHigh (res256, Vector256.Zero)
    let unpacked = unpackedLow.WithUpper (unpackedHigh.GetLower())
    unpacked.AsInt16 ()

let inline shuffle (cellsArray:int16[]) (mask : Vector128<byte>) =

    let ptr = fixed cellsArray
    let cells = Avx.LoadVector256 (NativePtr.add ptr 1)
    let unpacked = shuffleVec cells mask
    unpacked.AsVector().CopyTo(cellsArray, 1)

let swipeSIMD (cells:int16[]) =

    let vOrig = Vector (cells, 1) |> dump "vOrig"
    let vOrigLShift = Vector (cells, 2) |> dump "vOrigLShift"
    let vOrigRShift = shuffleVec (vOrig.AsVector256()) Constants.rotateRightMask |> Vector256.AsVector |> dump "vOrigRShift"
    let vOrigLShiftZeroed = Vector.BitwiseAnd (Constants.vIgnoreRowEndIndicies, vOrigLShift) |> dump "vOrigLShiftZeroed"
    let vOrigRShiftZeroed = Vector.BitwiseAnd (Constants.vIgnoreRowStartIndicies, vOrigRShift) |> dump "vOrigRShiftZeroed"
    let vBothMatches = Vector.Equals (vOrigLShiftZeroed, vOrigRShiftZeroed) |> dump "vBothMatches"
    let vBothMatchesNonZero = Vector.ConditionalSelect (Vector.GreaterThan (vOrigLShiftZeroed, Vector.Zero), vBothMatches, Vector.Zero) |> dump "vBothMatchesNonZero"

    printfn ""

    let vMatches = Vector.BitwiseAnd(GameSIMD.vIgnoreMatchIndicies, Vector.Equals(vOrig, vOrigLShift))
    let vOrigMatches = Vector.ConditionalSelect(vMatches, vOrig, Vector.Zero)
    let vDoubleOrigMatches = Vector.Multiply(Vector(2s), vOrigMatches)
    let vRShift = Vector(cells)
    let vRShiftEqualsOrig = Vector.Equals(vOrig, vRShift)
    let vRShiftMatches = Vector.Xor(vRShiftEqualsOrig, Vector.One)
    let vZeroedOrig = Vector.Multiply(vOrig, vRShiftMatches)
    let vIncremented = Vector.Max(vDoubleOrigMatches, vZeroedOrig) |> dump "vIncremented"
    let vResult = Vector.ConditionalSelect (vBothMatchesNonZero, vOrig, vIncremented) |> dump "vResult"

    vResult.CopyTo(cells, 1)
    let mem = NativePtr.stackalloc<int16>(16)
    let scores = Span<int16>(NativePtr.toVoidPtr mem, 16)
    let mutable scoreA = 0s
    let mutable scoreB = 0s
    let mutable scoreC = 0s
    let mutable scoreD = 0s
    for i = 0 to 3 do
        scoreA <- scoreA + scores.[i]
        scoreB <- scoreB + scores.[i]
        scoreC <- scoreC + scores.[i]
        scoreD <- scoreD + scores.[i]
    
    (scoreA + scoreB) + (scoreC + scoreD)
    
let inline rotateCopy (cells:int16[]) (map:int[]) =
    let cellsCopy = Array.copy cells
    for i = 0 to 15 do
        cellsCopy.[map.[i] + 1] <- cells.[i + 1]
    
    cellsCopy

let inline rotateDirection (cells:int16[]) direction =
    match direction with
    | Left -> ()
    | Right -> shuffle cells Constants.flipTransposeMap
    | Up -> shuffle cells Constants.antiClockwiseTransposeMap
    | Down -> shuffle cells Constants.clockwiseTransposeMap

let inline rotateOppositeDirection (cells:int16[]) direction =
    match direction with
    | Left -> ()
    | Right -> shuffle cells Constants.flipTransposeMap
    | Up -> shuffle cells Constants.clockwiseTransposeMap
    | Down -> shuffle cells Constants.antiClockwiseTransposeMap

let inline pack (cells:int16[]) =
    // Indicies account for vector padding
    let mutable i = 1
    for j = 2 to cells.Length - 2 do
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

let inline swipe (board:inref<Board>) direction =
    rotateDirection board.Cells direction
    pack board.Cells |> ignore
    let score = swipeSIMD board.Cells
    pack board.Cells |> ignore
    rotateOppositeDirection board.Cells direction
    board.SetScore (board.Score + int score)

let trySwipe (board:Board) direction =
    let origCells = boardPool.Rent()
    try
        Array.blit board.Cells 1 origCells 1 16
        swipe &board direction
        if GameSIMD.arraysEqual origCells board.Cells then board
        else Board.addRandomCell &board
    finally
        boardPool.Return(origCells)

let canSwipe (board:Board) =
    let hRows = ReadOnlySpan(board.Cells, 1, 16)
    let canSwipeHorizontal = GameArray.canSwipeRows board.Size &hRows (board.Size - 1)
    let rotated = rotateCopy board.Cells GameArray.clockwiseTransposeMap
    let vRows = ReadOnlySpan(rotated, 1, 16)
    let canSwipeVertical = GameArray.canSwipeRows board.Size &vRows (board.Size - 1)
    canSwipeHorizontal || canSwipeVertical

let boardContext = {
    TrySwipe = trySwipe
    Clone = Board.clone
    Create = Board.create
    CreateWithSeed = fun i j -> Board.emptyWithSeed i j |> Board.init
    CanSwipe = canSwipe
    ToString = Board.toString
    Score = fun board -> board.Score
    RNG = fun board -> board.RNG
}
