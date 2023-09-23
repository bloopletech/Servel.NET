var ume = (function() {
  function normalise(text) {
    return text.replace(/\s+/g, " ").trim();
  }

  function escapeEntities(text) {
    return text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
  }

  const BOLD_ITALIC_REGEX = /(?:(\W|^)(\*)|(\*)(\W|$)|(\W|^)(_)|(_)(\W|$))/;

  function parseInlines(text) {
    const parts = text.split(BOLD_ITALIC_REGEX);

    if(parts.filter(function(part) { return part == "*"; }).length % 2 != 0) return text;
    if(parts.filter(function(part) { return part == "_"; }).length % 2 != 0) return text;

    var inBold = false;
    var inItalic = false;
    var html = [];

    for(const part of parts) {
      if(part == "*") {
        inBold = !inBold;
        html.push(inBold ? "<b>" : "</b>"); //Note that in_bold has been inverted, so this is inverted as well
      }
      else if(part == "_") {
        inItalic = !inItalic;
        html.push(inItalic ? "<i>" : "</i>"); //Note that in_italic has been inverted, so this is inverted as well
      }
      else {
        html.push(part);
      }
    }

    return html.join("");
  }

  function parseHeading(text) {
    const parts = text.match(/^(#+) (.*)$/);
    return "<h" + parts[1].length + ">" + parseInlines(parts[2]) + "</h" + parts[1].length + ">";
  }

  function parseParagraph(text) {
    return "<p>" + parseInlines(text) + "</p>";
  }

  return function(text) {
    const nodes = text.split(/(\r?\n){2,}/g);
    var html = [];

    for(const node of nodes) {
      const cleanedNode = escapeEntities(normalise(node));

      if(cleanedNode.startsWith("#")) html.push(parseHeading(cleanedNode));
      else html.push(parseParagraph(cleanedNode));
    }

    return html.join("");
  }
})();

if(typeof module === "object" && typeof module.exports === "object") module.exports = ume;
