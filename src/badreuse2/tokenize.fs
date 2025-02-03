module badreuse2.tokenize

open System.Text.RegularExpressions

// A sample normalization function
let reservedKeywordsOfFsharp = Set.ofList [
    // Basic keywords
    "abstract"; "and"; "as"; "assert"; "base"; "begin"; "class"; "default"; "delegate"
    "do"; "done"; "downcast"; "downto"; "elif"; "else"; "end"; "exception"; "extern"
    "false"; "finally"; "fixed"; "for"; "fun"; "function"; "global"; "if"; "in"; "inherit"
    "inline"; "interface"; "internal"; "lazy"; "let"; "match"; "member"; "module"; "mutable"
    "namespace"; "new"; "not"; "null"; "of"; "open"; "or"; "override"; "private"; "protected"
    "public"; "rec"; "return"; "select"; "static"; "struct"; "then"; "to"; "true"; "try"
    "type"; "upcast"; "use"; "val"; "void"; "when"; "while"; "with"; "yield"
    // Composite keywords
    "and!"; "let!"; "match!"; "return!"; "use!"
    // Special operators as words
    "atomic"; "break"; "checked"; "component"; "const"; "constraint"; "constructor"
    "continue"; "eager"; "event"; "external"; "functor"; "include"; "method"
    "mixin"; "object"; "parallel"; "process"; "protected"; "pure"; "sealed"; "tailcall"
    "trait"; "virtual"; "volatile"
]

// Split the code line into tokens, get rid of short ones, generic ones like reserved keywords and short variable names
let tokensOfCodeLine (token: string) : string array =
    // Extract string literals and preserve them
    let stringLiterals = Regex.Matches(token, "\"[^\"]*\"")
    let nonStringParts = Regex.Split(token, "\"[^\"]*\"")

    // Process non-string parts
    let processedTokens = 
        nonStringParts
        |> Array.collect (fun part ->
            part
            |> fun t -> Regex.Split(t, "(?<!^)(?=[A-Z])")  // Split camelCase
            |> Array.collect (fun part ->
                part.ToLowerInvariant()
                |> fun t -> Regex.Replace(t, "[^a-z0-9]", " ")  // Remove special chars including URL paths
                |> fun t -> t.Split([|' '|], System.StringSplitOptions.RemoveEmptyEntries)
                |> Array.filter (fun t -> not (t.StartsWith("/")))  // Filter out URL paths
            )
        )

    // Combine string literals and processed tokens
    let stringTokens = 
        stringLiterals
        |> Seq.map (fun m -> m.Value.Trim('"'))
        |> Seq.filter (fun s -> s.Length > 5)  // Only filter by length for string literals
        |> Seq.toArray

    Array.append processedTokens stringTokens
    |> Array.map (fun part -> Regex.Replace(part, @"\d+$", ""))  // Remove trailing digits
    |> Array.filter (fun part -> part.Length > 5 && not (Set.contains part reservedKeywordsOfFsharp))

// Split the code into tokens, get rid of short ones, generic ones like reserved keywords and short variable names
let tokensOfCode (code: string) : string array =
    // First remove all comments (both line and inline)
    let cleanedCode = Regex.Replace(code, @"//.*?$|//.*?(?=\n)|/\*.*?\*/", "", RegexOptions.Multiline ||| RegexOptions.Singleline)
    
    // Split code into tokens based on whitespace and common delimiters,
    // then normalize each token.
    cleanedCode.Split([|' '; '\n'; '\r'; '\t'|], System.StringSplitOptions.RemoveEmptyEntries)
    |> Array.collect tokensOfCodeLine
    |> Array.sort
    |> Array.distinct
