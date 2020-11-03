open System

type Position = {
    X: int
    Y: int
}

type Cell =
| Value of int
| Empty

type Board = Map<Position, Cell>

type Direction =
| Up
| Down
| Left
| Right

let dumpBoard board =

    board
    |> Map.toSeq
    |> Seq.sortBy (fun (pos, _) -> pos.X, pos.Y)
    |> Seq.iter (fun (pos, cell) ->
        printf "%s" (if pos.Y = 0 then Environment.NewLine else String.Empty)
        match cell with
        | Empty -> "    -"
        | Value v -> sprintf "%5i" v
        |> printf "%s"
    ) 

let r = Random()
let randomPos max =
    { X = r.Next(max - 1); Y = r.Next(max - 1) }

let randomValue () =
    pown 2 (r.Next(1, 3))

let newBoard size =
    let randoms = set [(randomPos size); (randomPos size)]
    [ for x in [0..size-1] do
        for y in [0..size-1] do
            yield { X = x; Y = y} ]
    |> Seq.fold (fun board position -> 
        let cell = 
            if Set.contains position randoms 
            then Value (randomValue()) 
            else Empty
        Map.add position cell board
    ) Map.empty

[<EntryPoint>]
let main argv =
    let board = newBoard 4
    dumpBoard board
    0