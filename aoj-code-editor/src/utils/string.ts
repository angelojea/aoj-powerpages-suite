export function getStringBetweenTags(str: string, startTag: string, endTag: string) {
    // Find the start of the startTag
    const startIndex = str.indexOf(startTag);
    if (startIndex === -1) return ''; // Start tag not found

    // Find the end of the startTag
    const start = startIndex + startTag.length;

    // Find the start of the endTag after the startTag
    const endIndex = str.indexOf(endTag, start);
    if (endIndex === -1) return ''; // End tag not found

    // Extract the section between the start and end tags
    return str.substring(start, endIndex);
}