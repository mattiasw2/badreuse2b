# Duplicate Code Detection Tool

This tool detects duplicate or similar code segments by tokenizing source files, indexing segments, and comparing them against tokens from outstanding commits. It rebuilds the entire index every time it runs, which is ideal for projects where the entire codebase fits in memory.

## Overview

- **Purpose:**  
  Identify duplicate or similar code segments by comparing tokens extracted from source code segments and commit changes.

- **Approach:**  
  1. **Rebuild the index** by scanning all F\# source files.
  2. **Extract segments** based on specific delimiters (an empty line followed by a line starting at position 0).
  3. **Tokenize each segment** using a custom tokenizer that normalizes identifiers and string literals.
  4. **Store segment metadata** (file path, start/end rows, original segment, and token set) in an in-memory list.
  5. **For each outstanding commit**, tokenize the commit’s code changes.
  6. **Compare token sets** using set intersection. If the intersection contains at least _X_ tokens (default is 4), flag the commit along with the matching segment.

## Components

### 1. Tokenizer

- **Function:** `tokensOfCodeLine`
- **Responsibilities:**
  - **Token Extraction:**  
    - Extract string literals and process the rest of the code separately.
  - **Normalization:**  
    - **Lowercase:** Convert tokens to lowercase.
    - **CamelCase Splitting:** Use regex to split identifiers like `MyFunction` into separate tokens.
    - **Special Character Removal:** Remove non-alphanumeric characters.
    - **Trailing Digits Removal:** Remove numeric suffixes (e.g., converting `error404` to `error`).
  - **Filtering:**  
    - Discard tokens shorter than 6 characters.
    - Remove tokens that are reserved keywords.
  
### 2. Segment Extraction

- **Definition:**  
  A **segment** is defined as a block of code that starts when an empty line is followed by a line with text starting at the first position.

- **Process:**
  1. **File Selection:**  
     - Loop over all files matching the pattern `*.[.]fs`.
  2. **Segment Delimitation:**  
     - A segment begins when you find an empty line, and the next line starts at position 0 (i.e., no indentation).
     - Remove multiline comments (enclosed by `(*` and `*)`) 
     - Replacemultiline strings (enclosed by `"""`) by ""
  3. **Tokenization per Segment:**  
     - For every segment, tokenize each line using the `tokensOfCodeLine` function.
     - Combine tokens from all lines in the segment into a `Set<string>` to eliminate duplicates.

- **Data Stored per Segment:**
  - **File Location:** File name/path.
  - **Start Row and End Row:** The line numbers indicating the segment boundaries.
  - **Segment Text:** The original code text for reference.
  - **Token Set:** The normalized set of tokens for set intersection comparisons.

### 3. Indexing

- **Structure:**  
  A list of segment records, each containing the file location, start and end rows, the segment’s text, and its corresponding token set.

- **Purpose:**  
  This in-memory index allows for efficient scanning and comparison of code segments during commit analysis.

### 4. Commit Analysis and Similarity Checking

- **Commit Tokenization:**
  - For every outstanding commit, extract the modified code.
  - Apply the same tokenization process to generate a set of tokens for the commit.

- **Similarity Check:**
  - **Comparison:**  
    - For each commit token set, iterate through all segment token sets in the index.
    - Compute the intersection (common tokens) between the commit tokens and a segment’s tokens.
  - **Threshold:**  
    - If the intersection contains **4 or more tokens** (configurable threshold), flag the commit as similar to the corresponding segment.
  - **Result:**  
    - Record the match along with the commit identifier and segment details (file, start row, end row, segment text).

### 5. User Notification

- **Output:**  
  - Present the user with a report showing:
    - The commit(s) that have token overlaps with an existing segment.
    - The detailed information of the matching segment.
    - The number of matching tokens.
  
- **Purpose:**  
  Allow the developer to review potential duplicate code and take corrective actions before committing.

## Implementation Considerations

- **Full Rebuild:**  
  The index is completely rebuilt each time, which simplifies the process and ensures consistency, given that the codebase fits into memory.

- **Normalization:**  
  The tokenizer applies robust normalization (lowercasing, removing plurals, splitting identifiers, and removing trailing digits) to improve matching accuracy.

- **Threshold Tuning:**  
  The similarity threshold (currently 4 tokens) is configurable based on project needs.

- **Extensibility:**  
  The design is modular, making it straightforward to enhance tokenization, add new normalization rules, or incorporate additional similarity metrics in the future.

