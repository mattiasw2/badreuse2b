module SegmentExtraction

open System
open System.Text.RegularExpressions

/// Replaces multi-line strings (enclosed in """) with empty strings ("").
let removeMultilineStrings (content: string) : string =
    let pattern = "\"{3}([^\"]|\"{1,2})*?\"{3}"
    Regex.Replace(content, pattern, "\"\"", RegexOptions.Singleline)

/// Removes multi-line comments (enclosed in (* and *)).
let removeMultilineComments (content: string) : string =
    Regex.Replace(content, "\\(\\*[\\s\\S]*?\\*\\)", "")

/// Extracts segments from a given file content.
/// Multi-line strings are replaced with empty strings ("").
/// Multi-line comments are removed.
/// A new segment begins when an empty line is followed by text in the first position.
let extractSegments (fileContent: string) : string list =
    let processedContent = 
        fileContent
        |> removeMultilineComments
        |> removeMultilineStrings
        |> fun s -> s.Replace("\r\n", "\n")

    let lines = processedContent.Split('\n') |> Array.toList
    let rec loop acc currentSegment = function
        | [] -> 
            match currentSegment with
            | [] -> List.rev acc
            | _ -> List.rev ((String.concat "\n" (List.rev currentSegment)) :: acc)
        | "" :: rest when List.isEmpty currentSegment -> 
            loop acc [] rest
        | "" :: next :: rest when not (next.StartsWith(" ")) && not (next.StartsWith("\t")) ->
            match currentSegment with
            | [] -> loop acc [next] rest
            | _ -> loop ((String.concat "\n" (List.rev currentSegment)) :: acc) [next] rest
        | line :: rest ->
            loop acc (line :: currentSegment) rest

    loop [] [] lines
    |> List.filter (not << String.IsNullOrWhiteSpace)
