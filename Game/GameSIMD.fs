module GameSIMD

open System
open System.Numerics

open Common

module Board =
    let boardSize size = 2 + (size * size)
    let emptyCells size =
        let cells = Array.zeroCreate (boardSize size)
        cells.[0] <- -1s
        cells.[17] <- -1s
        cells

    let empty (size:int) =
        {
            Cells = emptyCells size
            Size = size
            Score = 0
            RNG = Random()
            RNGSeed = None
        } 
    
    let emptyWithSeed size seed =
        { empty size with
            RNG = Random(seed)
            RNGSeed = Some seed }

    let addRandomCell (board:Board<int16[]>) =
        let rec addRec pos value (board:Board<int16[]>) =
            let i = 1 + GameArray.Board.posToIndex board.Size pos
            match board.Cells.[i] with
            | v when v = 0s -> 
                board.Cells.[i] <- value
                board
            | _ -> 
                addRec (GameArray.Board.randomPos board) value board
        let value = GameArray.Board.randomValue board
        let pos = GameArray.Board.randomPos board
        addRec pos value board

    let init board =
        board
        |> addRandomCell
        |> addRandomCell

    let create = empty >> init

    let fromList (cells:int list list) =
        let empty = emptyCells 4
        let cells' =
            cells
            |> List.collect id
            |> List.toArray
            |> Array.map int16
        Array.blit cells' 0 empty 1 (cells'.Length)
        empty

    let toString (board:Board<int16[]>) =
        GameArray.Board.toString 
            { board with 
                Cells = board.Cells
                    |> Array.skip 1
                    |> Array.take 16 }

let vIgnoreMatchIndicies = Vector([|
    Int16.MaxValue
    Int16.MaxValue
    Int16.MaxValue
    0s
    Int16.MaxValue
    Int16.MaxValue
    Int16.MaxValue
    0s
    Int16.MaxValue
    Int16.MaxValue
    Int16.MaxValue
    0s
    Int16.MaxValue
    Int16.MaxValue
    Int16.MaxValue
    0s |])
let swipeSIMD (cells:int16[]) =
    let vOrig = Vector(cells, 1)
    let vLShift = Vector(cells, 2)
    // todo: remove last index of each row from matches
    let vMatches = Vector.BitwiseAnd(vIgnoreMatchIndicies, Vector.Equals(vOrig, vLShift))
    let vOrigMatches = Vector.ConditionalSelect(vMatches, vOrig, Vector<int16>.Zero)
    let vDoubleOrigMatches = Vector.Multiply(Vector(2s), vOrigMatches)
    let vRShift = Vector(cells)
    let vRShiftEqualsOrig = Vector.Equals(vOrig, vRShift)
    let vRShiftMatches = Vector.Xor(vRShiftEqualsOrig, Vector<int16>.One)
    let vZeroedOrig = Vector.Multiply(vOrig, vRShiftMatches)
    let vResult = Vector.Max(vDoubleOrigMatches, vZeroedOrig)
    vResult.CopyTo(cells, 1)
    let scores = Array.zeroCreate Vector<int16>.Count
    vDoubleOrigMatches.CopyTo(scores)
    let mutable score = 0s
    for i = 0 to scores.Length - 1 do
        if scores.[i] > 0s then
            score <- score + scores.[i]
    
    score

let pack (cells:int16[]) =
    // Indicies account for vector padding
    let mutable i = 1
    let mutable j = 2
    while j < cells.Length - 1 do
        if (j % 4) = 1 then
            i <- j
        elif cells.[i] > 0s then
            i <- i + 1
        elif cells.[i] = 0s && cells.[j] > 0s then
            cells.[i] <- cells.[j]
            cells.[j] <- 0s
            i <- i + 1

        j <- j + 1
        
    cells

let rotate (cells:int16[]) (map:int[]) =
    let cellsCopy = Array.copy cells
    for i = 0 to 15 do
        cells.[map.[i] + 1] <- cellsCopy.[i + 1]

let rotateCopy (cells:int16[]) (map:int[]) =
    let cellsCopy = Array.copy cells
    for i = 0 to 15 do
        cellsCopy.[map.[i] + 1] <- cells.[i + 1]
    
    cellsCopy

let rotateDirection (cells:int16[]) direction =
    match direction with
    | Left -> ()
    | Right -> rotate cells GameArray.flipTransposeMap
    | Up -> rotate cells GameArray.clockwiseTransposeMap
    | Down -> rotate cells GameArray.anticlockwiseTransposeMap

let rotateOppositeDirection (cells:int16[]) direction =
    match direction with
    | Left -> ()
    | Right -> rotate cells GameArray.flipTransposeMap
    | Up -> rotate cells GameArray.anticlockwiseTransposeMap
    | Down -> rotate cells GameArray.clockwiseTransposeMap

let swipe (board:Board<int16[]>) direction =
    rotateDirection board.Cells direction
    let score =
        pack board.Cells
        |> swipeSIMD
    pack board.Cells |> ignore
    rotateOppositeDirection board.Cells direction
    { board with Score = board.Score + int score }

let arraysEqual (cells:int16[]) (b:int16[]) =
    Vector.EqualsAll(Vector(cells, 1), Vector(b, 1))

let trySwipe (board:Board<int16[]>) direction =
    let origCells = Array.copy board.Cells
    let board' = swipe board direction
    if arraysEqual origCells board.Cells then
        board
    else
        Board.addRandomCell board'

let canSwipe board =
    let hRows = ReadOnlySpan(board.Cells, 1, 16)
    let canSwipeHorizontal = GameArray.canSwipeRows board.Size &hRows (board.Size - 1)
    let rotated = rotateCopy board.Cells GameArray.clockwiseTransposeMap
    let vRows = ReadOnlySpan(rotated)
    let canSwipeVertical = GameArray.canSwipeRows board.Size &vRows (board.Size - 1)
    canSwipeHorizontal || canSwipeVertical

let boardContext = {
    TrySwipe = trySwipe
    Clone = GameArray.Board.clone
    Create = Board.create
    CreateWithSeed = fun i j -> Board.create i
    CanSwipe = canSwipe
    ToString = Board.toString
}