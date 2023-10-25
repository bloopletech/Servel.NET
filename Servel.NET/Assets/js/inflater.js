"use strict";

function inflateDirectoryEntry(directoryEntry) {
  const IMAGE_EXTS = ["jpg", "jpeg", "png", "gif"];
  const VIDEO_EXTS = ["webm", "mp4", "mkv"];
  const AUDIO_EXTS = ["mp3", "m4a", "wav"];
  const TEXT_EXTS = ["txt"];

  function getIcon(entry) {
    if(entry.homeEntry) return "🏠";
    if(entry.topEntry) return "🔝";
    if(entry.parentEntry) return "⬆️";
    if(entry.directory) return "📁";
    if(entry.file) {
      if(entry.mediaType == "video") return "🎞️";
      if(entry.mediaType == "image") return "🖼️";
      if(entry.mediaType == "audio") return "🔊";
      return "📝";
    }

    return "";
  }

  function getType(entry) {
    if(entry.directory) return "Dir";
    return entry.name.slice((Math.max(0, entry.name.lastIndexOf(".")) || Infinity) + 1);
  }

  function getMediaType(entry) {
    if(entry.directory || !entry.type) return null;

    if(IMAGE_EXTS.includes(entry.type)) return "image";
    if(VIDEO_EXTS.includes(entry.type)) return "video";
    if(AUDIO_EXTS.includes(entry.type)) return "audio";
    if(TEXT_EXTS.includes(entry.type)) return "text";

    return null;
  }

  function getClass(entry) {
    const classes = [];
    if(entry.directory) classes.push("directory");
    if(entry.file) classes.push("file");
    if(entry.homeEntry) classes.push("home");
    if(entry.topEntry) classes.push("top");
    if(entry.parentEntry) classes.push("parent");
    if(entry.mediaType) classes.push("media", entry.mediaType);
    return classes.join(" ");
  }

  function inflateEntries(entries) {
    for(const entry of entries) {
      entry.type = getType(entry);
      entry.mediaType = getMediaType(entry);
      entry.icon = getIcon(entry);
      entry.sizeText = entry.file ? filesize(entry.size || 0, { base: 2, standard: "jedec" }) : "-";
      entry.mtimeText = entry.mtime != null ? f(new Date(entry.mtime)) : "-";
      entry.class = getClass(entry);
    }
  }

  for(const entry of directoryEntry.specialEntries) entry.directory = true;
  for(const entry of directoryEntry.directories) entry.directory = true;
  for(const entry of directoryEntry.files) entry.file = true;
  inflateEntries(directoryEntry.specialEntries);
  inflateEntries(directoryEntry.directories);
  inflateEntries(directoryEntry.files);
}