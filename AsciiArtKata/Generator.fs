module Generator

open System

let typography = 
    """
     ___   _____   _____   _____   _____   _____   _____   _   _   _       _   _   _    _           ___  ___   __   _   _____   _____   _____     _____    _____   _____   _   _   _     _   _          __ __    __ __    __  ______ 
    /   | |  _  \ /  ___| |  _  \ | ____| |  ___| /  ___| | | | | | |     | | | | / /  | |         /   |/   | |  \ | | /  _  \ |  _  \ /  _  \   |  _  \  /  ___/ |_   _| | | | | | |   / / | |        / / \ \  / / \ \  / / |___  / 
   / /| | | |_| | | |     | | | | | |__   | |__   | |     | |_| | | |     | | | |/ /   | |        / /|   /| | |   \| | | | | | | |_| | | | | |   | |_| |  | |___    | |   | | | | | |  / /  | |  __   / /   \ \/ /   \ \/ /     / /  
  / /_| | |  _  { | |     | | | | |  __|  |  __|  | |  _  |  _  | | |  _  | | | |\ \   | |       / / |__/ | | | |\   | | | | | |  ___/ | | | |   |  _  /  \___  \   | |   | | | | | | / /   | | /  | / /     }  {     \  /     / /   
 /  __  | | |_| | | |___  | |_| | | |___  | |     | |_| | | | | | | | | |_| | | | \ \  | |___   / /       | | | | \  | | |_| | | |     | |_| |_  | | \ \   ___| |   | |   | |_| | | |/ /    | |/   |/ /     / /\ \    / /     / /__  
/_/   |_| |_____/ \_____| |_____/ |_____| |_|     \_____/ |_| |_| |_| \_____/ |_|  \_\ |_____| /_/        |_| |_|  \_| \_____/ |_|     \_______| |_|  \_\ /_____/   |_|   \_____/ |___/     |___/|___/     /_/  \_\  /_/     /_____| 
"""

let lines = typography.Split([|'\r'; '\n'|], StringSplitOptions.RemoveEmptyEntries)

// step 1: compute max line length
let maxlineLength = lines |> Seq.map (fun l -> l.Length) |> Seq.max

// step 2: check if a line with different length exists
let linesContainsErrorOfLength = lines |> Seq.exists (fun l -> l.Length <> maxlineLength)

// step 3: find all spaces columns offsets between letters
let letters = 
  seq {
    for i in 0 .. maxlineLength-1 do
      let isSpace = lines |> Seq.exists(fun l -> l.[i] <> ' ') |> not
      if isSpace then yield i
  } |> Seq.toList

// ------------------
let numberOfLetters = letters.Length
let alphabet = ['a' .. 'z']

type LetterCoordinates =
  { From:int; To:int }
  member __.Length 
    with get () = __.To - __.From

type CoordinatesComputationState = 
  { CurrentOffset:int
    Items:(char * LetterCoordinates) list }
  static member Empty = 
    { CurrentOffset=0; Items=[] }

type LetterContent = string list
type LetterCoordinatesMap = System.Collections.Generic.IDictionary<char, LetterCoordinates>

// ------------------

// step 4: use map2 then fold, then dict

let accumulateCoordinates state (letter:char, index:int) =
  let items = (letter,{ From=state.CurrentOffset; To=index }) :: state.Items
  { state with Items=items; CurrentOffset=index }

let computeCoordinates() : LetterCoordinatesMap =
  let indexedLetters =
    letters
    |> List.map2 (fun k v -> k,v) alphabet
  indexedLetters
  |>List.fold accumulateCoordinates CoordinatesComputationState.Empty
  |> fun state -> dict state.Items


// step 5: build an optionnal

let getLetter (c:char) (coordinates:LetterCoordinatesMap) : LetterContent option =
  if coordinates.ContainsKey c
  then
    let letterCoordinates = coordinates.Item c
    lines
    |> Seq.map(fun l -> l.Substring(letterCoordinates.From, letterCoordinates.Length))
    |> Seq.toList
    |> Some
  else None

// Try:
// ------------------

// computeCoordinates() |> getLetter 'a'
// val it : string list option =
//   Some
//     ["     ___ "; "    /   |"; "   / /| |"; "  / /_| |"; " /  __  |";
//      "/_/   |_|"]

// computeCoordinates() |> getLetter '_'
// val it : string list option = None

// ------------------

// step 6: draw a letter

let drawLetters (letters:LetterContent list) =
  for i in 0 .. lines.Length-1 do
    for letter in letters do
      printf "%s" letter.[i]
    printfn ""

// step 7: use Seq.choose to draw found letters
let drawString coordinates (text:string) =
  let findLetter c = coordinates |> getLetter c
  text.ToLower().ToCharArray() 
  |> Seq.choose findLetter 
  |> Seq.toList
  |> drawLetters

// Try
// ------------------
// let coordinates = computeCoordinates()
// drawString "Fsharp"
// ------------------

// step 8: Calculate the average width of the letters
let getAvgLetterSize (coordinates:LetterCoordinatesMap) =
  coordinates.Values |> Seq.averageBy (fun v -> float v.Length) |> int


// ------------------
let createSpace width height = 
  seq {
    for i in [0..height] do
      yield {0..width} |> Seq.map(fun _ -> ' ') |> Seq.toArray |> String
  } |> Seq.toList
// ------------------

// step 9: drawString' must display a space whose size corresponds to the average size of the characters when it finds an unknown letter
let drawString' (coordinates:LetterCoordinatesMap) (text:string) =
  let findLetter c = coordinates |> getLetter c
  seq {
    for letter in text.ToLower().ToCharArray() do
      match findLetter letter with
      | Some content -> yield content
      | None -> 
        let avgLetterSize = getAvgLetterSize coordinates
        yield createSpace avgLetterSize lines.Length
  }
  |> Seq.toList
  |> drawLetters

// Try
// ------------------
//let coordinates = computeCoordinates()
//drawString' coordinates "Fsharp is cool"
// ------------------
