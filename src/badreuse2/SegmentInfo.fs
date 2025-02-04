namespace badreuse2

type SegmentInfo = {
    FilePath: string
    StartLine: int
    EndLine: int
    SegmentText: string
    Tokens: Set<string>
}
