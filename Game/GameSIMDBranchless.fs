module GameSIMDBranchless

#nowarn "9"

open Microsoft.FSharp.NativeInterop
open System
open System.Collections.Generic
open System.Numerics
open System.Runtime.CompilerServices

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

let dump msg o = 
    printfn "%s: %A" msg o
    o

let boardPool = ArrayPool(fun _ -> GameSIMD.Board.emptyCells 4)
let board16Pool = ArrayPool(fun _ -> Array.zeroCreate<int16> 16)
let swipeSIMD (cells:int16[]) =
    let vOrig = Vector(cells, 1)
    let vOrigZeroesMask = Vector.Equals(vOrig, Vector<int16>.Zero)
    let vOrigInc = Vector.Add(Vector<int16>.One, vOrig)
    let vOrigIncZeroed = Vector.ConditionalSelect(vOrigZeroesMask, Vector<int16>.Zero, vOrigInc)
    let vLShift = Vector(cells, 2)
    let vMatches = Vector.BitwiseAnd(GameSIMD.vIgnoreMatchIndicies, Vector.Equals(vOrig, vLShift))
    let vOrigMatchesInc = Vector.ConditionalSelect(vMatches, vOrigIncZeroed, Vector<int16>.Zero)
    let vRShift = Vector(cells)
    let vRShiftEqualsOrig = Vector.Equals(vOrig, vRShift)
    let vRShiftMatches = Vector.Xor(vRShiftEqualsOrig, Vector<int16>.One)
    let vZeroedOrig = Vector.Multiply(vOrig, vRShiftMatches)
    let vResult = Vector.Max(vOrigMatchesInc, vZeroedOrig)
    vResult.CopyTo(cells, 1)
    let mem = NativePtr.stackalloc<int16>(16)
    let scores = Span<int16>(NativePtr.toVoidPtr mem, 16)
    vOrigMatchesInc.CopyTo(scores)
    let mutable score = 0s
    for i = 0 to 15 do
        if scores.[i] > 0s then
            score <- score + int16 (2.0 ** float scores.[i])
    
    score

let inline rotate (cells:int16[]) (map:int[]) =
    let cellsCopy = boardPool.Rent()
    try
        Array.blit cells 0 cellsCopy 0 cells.Length
        cells.[map.[0] + 1] <- cellsCopy.[1]
        cells.[map.[1] + 1] <- cellsCopy.[2]
        cells.[map.[2] + 1] <- cellsCopy.[3]
        cells.[map.[3] + 1] <- cellsCopy.[4]
        cells.[map.[4] + 1] <- cellsCopy.[5]
        cells.[map.[5] + 1] <- cellsCopy.[6]
        cells.[map.[6] + 1] <- cellsCopy.[7]
        cells.[map.[7] + 1] <- cellsCopy.[8]
        cells.[map.[8] + 1] <- cellsCopy.[9]
        cells.[map.[9] + 1] <- cellsCopy.[10]
        cells.[map.[10] + 1] <- cellsCopy.[11]
        cells.[map.[11] + 1] <- cellsCopy.[12]
        cells.[map.[12] + 1] <- cellsCopy.[13]
        cells.[map.[13] + 1] <- cellsCopy.[14]
        cells.[map.[14] + 1] <- cellsCopy.[15]
        cells.[map.[15] + 1] <- cellsCopy.[16]

    finally
        boardPool.Return(cellsCopy)

let inline rotateCopy (cells:int16[]) (map:int[]) =
    let cellsCopy = Array.copy cells
    for i = 0 to 15 do
        cellsCopy.[map.[i] + 1] <- cells.[i + 1]
    
    cellsCopy

let inline rotateDirection (cells:int16[]) direction =
    match direction with
    | Left -> ()
    | Right -> rotate cells GameArray.flipTransposeMap
    | Up -> rotate cells GameArray.clockwiseTransposeMap
    | Down -> rotate cells GameArray.anticlockwiseTransposeMap

let inline rotateOppositeDirection (cells:int16[]) direction =
    match direction with
    | Left -> ()
    | Right -> rotate cells GameArray.flipTransposeMap
    | Up -> rotate cells GameArray.anticlockwiseTransposeMap
    | Down -> rotate cells GameArray.clockwiseTransposeMap

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
