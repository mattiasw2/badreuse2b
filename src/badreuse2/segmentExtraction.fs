module badreuse2.SegmentExtraction

open System
open System.IO
open System.Text.RegularExpressions
open System.Collections.Generic

/// Replaces multi-line strings (enclosed in """) with empty strings ("").
let removeMultilineStrings (content: string) : string =
    // smart: matches the first `"""` and stops at the next `"""`, even if there are one or two double quotes in between.
    let pattern = "\"{3}([^\"]|\"{1,2})*?\"{3}"
    Regex.Replace(content, pattern, "\"\"", RegexOptions.Singleline)

/// Removes multi-line comments (enclosed in (* and *)).
let removeMultilineComments (content: string) : string =
    Regex.Replace(content, "\\(\\*[\\s\\S]*?\\*\\)", "")

let tokensOfCodeLine (line: string) : string list =
    line.Split([|' '; '\t'; '.'; ','; ';'; '('; ')'; '{'; '}'|], 
                StringSplitOptions.RemoveEmptyEntries)
    |> Array.map (fun s -> s.Trim().ToLower())
    |> Array.filter (fun s -> s.Length > 1)
    |> Array.toList

let createSegmentInfo filePath startLine lines =
    let endLine = startLine + List.length lines - 1
    let text = lines |> List.rev |> String.concat "\n"
    let tokens = 
        lines
        |> List.collect tokensOfCodeLine
        |> Set.ofList
    {
        FilePath = filePath
        StartLine = startLine
        EndLine = endLine
        SegmentText = text
        Tokens = tokens
    }

/// Extracts segments from a given file content.
/// Multi-line strings are replaced with empty strings ("").
/// Multi-line comments are removed.
/// A new segment begins when an empty line is followed by text in the first position.
let extractSegments (filePath: string) (fileContent: string) : SegmentInfo list =
    let processedContent = 
        fileContent
        |> removeMultilineComments
        |> removeMultilineStrings
        |> fun s -> s.Replace("\r\n", "\n")

    let lines = processedContent.Split('\n') |> Array.mapi (fun i line -> (i + 1, line))
    
    let rec collectSegments acc currentSegment startLine lines = 
        match lines with
        | [] -> 
            match currentSegment with
            | [] -> List.rev acc
            | lines -> 
                let segment = createSegmentInfo filePath startLine lines
                List.rev (segment :: acc)
        | (lineNum, "") :: rest when List.isEmpty currentSegment ->
            collectSegments acc currentSegment lineNum rest
        | (lineNum, "") :: (nextLineNum, nextLine) :: rest
            when not (nextLine.StartsWith(" ")) && not (nextLine.StartsWith("\t")) ->
            let newAcc = 
                match currentSegment with
                | [] -> acc
                | lines -> 
                    let segment = createSegmentInfo filePath startLine lines
                    segment :: acc
            collectSegments newAcc [nextLine] nextLineNum rest
        | (lineNum, line) :: rest ->
            collectSegments acc (line :: currentSegment) startLine rest

    collectSegments [] [] 1 (Array.toList lines)

// Removed SegmentInfo definition - now in separate file

let processFiles (rootDir: string) =
    let fsFiles = 
        Directory.EnumerateFiles(rootDir, "*.fs", SearchOption.AllDirectories)
        |> Seq.toList

    fsFiles
    |> List.collect (fun filePath ->
        try
            File.ReadAllText(filePath)
            |> extractSegments filePath
        with ex ->
            printfn "Error processing %s: %s" filePath ex.Message
            [])
