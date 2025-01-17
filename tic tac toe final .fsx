//Jose david zapata jaramillo
//Michel augusto velasquez ibarra

open System 

type Letter = | X | O

type Value =
    | Unspecified 
    | Letter of Letter

type OneThroughThree = | One | Two | Three 

type Row = Value * Value * Value

type Board = Row * Row * Row

let emptyBoard =  
    (Unspecified, Unspecified, Unspecified),
    (Unspecified, Unspecified, Unspecified),
    (Unspecified, Unspecified, Unspecified)

type Position = {
    Column: OneThroughThree
    Row: OneThroughThree
}

type Move = {
    At: Position
    Place: Letter
}

let select (board: Board) (position: Position) = 
    match board, position with 
        | ((x, _, _), _, _), {Row = One; Column = One} -> x
        | ((_, x, _), _, _), {Row = One; Column = Two} -> x
        | ((_, _, x), _, _), {Row = One; Column = Three} -> x
        | (_, (x, _, _), _), {Row = Two; Column = One} -> x
        | (_, (_, x, _), _), {Row = Two; Column = Two} -> x
        | (_, (_, _, x), _), {Row = Two; Column = Three} -> x
        | (_, _, (x, _, _)), {Row = Three; Column = One} -> x
        | (_, _, (_, x, _)), {Row = Three; Column = Two} -> x
        | (_, _, (_, _, x)), {Row = Three; Column = Three} -> x

let set value (board: Board) (position: Position) = 
    match board, position with 
        | ((_, v2, v3), r2, r3), { Row = One; Column = One} -> (value, v2, v3), r2, r3 
        | ((v1, _, v3), r2, r3), { Row = One; Column = Two} -> (v1, value, v3), r2, r3 
        | ((v1, v2, _), r2, r3), { Row = One; Column = Three} -> (v1, v2, value), r2, r3 
        | (r1, (_, v2, v3), r3), { Row = Two; Column = One} -> r1, (value, v2, v3), r3
        | (r1, (v1, _, v3), r3), { Row = Two; Column = Two} -> r1, (v1, value, v3), r3
        | (r1, (v1, v2, _), r3), { Row = Two; Column = Three} -> r1, (v1, v2, value), r3
        | (r1, r2, (_, v2, v3)), { Row = Three; Column = One} -> r1, r2, (value, v2, v3)
        | (r1, r2, (v1, _, v3)), { Row = Three; Column = Two} -> r1, r2, (v1, value, v3)
        | (r1, r2, (v1, v2, _)), { Row = Three; Column = Three} -> r1, r2, (v1, v2, value)

let modify f (board: Board) (position: Position) = 
    set (f (select board position)) board position

let placePieceIfCan piece = modify (function | Unspecified -> Letter piece | x -> x)

let makeMove (board: Board) (move: Move) =
    if select board move.At = Unspecified
        then Some (placePieceIfCan move.Place board move.At)
        else None 

let wayToWin =
    [
        ({ Row = One; Column = One },{ Row = One; Column = Two },{ Row = One; Column = Three });
        ({ Row = Two; Column = One },{ Row = Two; Column = Two },{ Row = Two; Column = Three });
        ({ Row = Three; Column = One },{ Row = Three; Column = Two },{ Row = Three; Column = Three });
        ({ Row = One; Column = One },{ Row = Two; Column = One },{ Row = Three; Column = One });
        ({ Row = One; Column = Two },{ Row = Two; Column = Two },{ Row = Three; Column = Two });
        ({ Row = One; Column = Three },{ Row = Two; Column = Three },{ Row = Three; Column = Three });
        ({ Row = One; Column = One },{ Row = Two; Column = Two },{ Row = Three; Column = Three });
        ({ Row = One; Column = Three },{ Row = Two; Column = Two },{ Row = Three; Column = One })
    ]

let cells =
    List.ofSeq <| seq {
        for row in [One; Two; Three] do 
            for column in [One; Two; Three] do
                yield { Row = row; Column = column}
    }

let map3 f (a, b, c) = f a, f b, f c

let winner (board: Board) =
    let winPaths = List.map (map3 (select board)) wayToWin
    if List.contains (Letter X, Letter X, Letter X) winPaths then Some X
    elif List.contains (Letter O, Letter O, Letter O) winPaths then Some O
    else None 

let slotsRemaining (board: Board) =
    List.exists ((=) Unspecified << select board) cells

type Outcome =
    | NoneYet 
    | Winner of Letter
    | Draw

let outcome (board: Board) =
    match winner board, slotsRemaining board with 
    | Some winningLetter, _ -> Winner winningLetter
    | None, false -> Draw 
    | _ -> NoneYet

let renderValue = function 
    | Unspecified -> " "
    | Letter X -> "X"
    | Letter O -> "O"

let otherPlayer = function
    | X -> O
    | O -> X 

let render ((a, b, c), (d, e, f), (g, h, i)) =
    sprintf "%s | %s | %s\n_ _ _ _ _\n%s | %s | %s\n_ _ _ _ _\n%s | %s | %s"
        (renderValue a) (renderValue b) (renderValue c)
        (renderValue d) (renderValue e) (renderValue f)
        (renderValue g) (renderValue h) (renderValue i)

type GameState = { Board: Board; WhoseTurn: Letter }

let initialGameState = { Board = emptyBoard; WhoseTurn = X }

let parseOneTwoThree = function
    | "1" -> Some One
    | "2" -> Some Two
    | "3" -> Some Three
    | _ -> None

let parseMove (raw: string) : Position option =
    match raw.Split [|' '|] with 
    | [| r; c |] ->
        match parseOneTwoThree r, parseOneTwoThree c with
        | Some row, Some column -> Some { Row = row; Column = column }
        | _ -> None
    | _ -> None 

let rec readMoveIo letter =
    printf "Row: "
    let firstInput = System.Console.ReadLine()
    printf "\nColumn: "
    let secondInput = System.Console.ReadLine()
    match parseMove (String.Concat(firstInput, " ", secondInput)) with
    | Some position -> { At = position; Place = letter }
    | None -> 
        printf "Wrong move! Please input row and column numbers\n"
        readMoveIo letter

let rec nextMoveIo board letter =
    match makeMove board (readMoveIo letter) with
    | Some newBoard -> newBoard
    | _ -> 
        printf "Wrong move! Position is occupied.\n"
        nextMoveIo board letter 

let rec playIo { Board = board; WhoseTurn = currentPlayer } =
    printf "%A's turn\n" currentPlayer
    printf "%s\n" (render board)

    let newBoard = nextMoveIo board currentPlayer

    match outcome newBoard with
    | Winner letter -> 
        printf "%A wins!!\n" letter
        printf "%s\n" (render newBoard)
    | Draw -> 
        printf "It's a draw\n"
    | NoneYet -> 
        playIo { Board = newBoard; WhoseTurn = otherPlayer currentPlayer }

// Inicia el juego
playIo initialGameState

 
 