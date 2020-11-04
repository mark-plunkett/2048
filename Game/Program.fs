open System

type Position = {
    X: int
    Y: int
}

type CellValue = int

type Board = Map<Position, CellValue>

type Direction =
| Up
| Down
| Left
| Right

let dumpBoard board =
    board
    |> Map.toSeq
    |> Seq.sortBy (fun (pos, _) -> pos.Y, pos.X)
    |> Seq.iter (fun (pos, cell) ->
        printf "%s" (if pos.X = 1 then Environment.NewLine else String.Empty)
        match cell with
        | 0 -> "    -"
        | v -> sprintf "%5i" v
        |> printf "%s"
    )
    printfn ""
    board

let r = Random()
let randomPos max =
    { X = r.Next(1, max); Y = r.Next(1, max) }

let randomValue () =
    //pown 2 (r.Next(1, 3))
    2

let newBoard size =
    let randoms = set [{X = 1; Y = 2;};{X = 1; Y = 3}]
    //let randoms = set [(randomPos size); (randomPos size)]
    [ for x in [1..size] do
        for y in [1..size] do
            yield { X = x; Y = y} ]
    |> Seq.fold (fun board position -> 
        let cell = 
            if Set.contains position randoms 
            then randomValue() 
            else 0
        Map.add position cell board
    ) Map.empty

let newBoardFromList board =
    [ for y, row in List.indexed board do
        for x, cell in List.indexed row do
            yield ({ X = x + 1; Y = y + 1}, cell) ]
    |> List.fold (fun board (pos, cell) ->
        Map.add pos cell board
    ) Map.empty

let flatten cells =
    let rec flattenRec acc cells =
        match cells with
        | [] -> acc
        | [x] -> x::acc
        | first::second::rest ->
            if first = second then flattenRec ((first + second)::acc) rest
            else flattenRec (first::acc) (second::rest)

    flattenRec [] cells
    |> List.rev

let padList n v list =
    list@([1..n-List.length list] |> List.map (fun _ -> v))

let swipe direction board =
    match direction with
    | Up -> 
        let cellFolder x cells (i, newVal) =
            Map.add {X=x;Y=i+1} newVal cells
        let rowFolder board currentX =
            board
            |> Map.toList
            |> List.where (fun (_, v) -> v > 0)
            |> List.where (fun ({ X = x; Y = _ }, _) -> x = currentX)
            |> List.map (fun (pos, _) -> board.[pos])
            |> flatten
            |> padList 4 0
            |> List.toArray
            |> Array.indexed
            |> Array.fold (cellFolder currentX) board
        [1..4]
        |> List.fold rowFolder board

let test () =
    newBoardFromList [
        [ 0; 0; 2; 4 ]
        [ 2; 0; 2; 4 ]
        [ 2; 0; 2; 0 ]
        [ 0; 2; 8; 16 ]
    ] 

[<EntryPoint>]
let main argv =
    //let board = newBoard 4
    //dumpBoard board

    //printfn ""
    ////flatten [2;2;4;4]
    ////|> Seq.iter (printfn "%i")

    //swipe Up board
    //|> dumpBoard

    test ()
    |> dumpBoard
    |> swipe Up
    |> dumpBoard

    0