"use strict";

function inflateDirectoryEntry(directoryEntry) {
  const PLAYABLE_IMAGE_EXTS = ["jfif", "jpg", "jpeg", "png", "gif", "webp"];
  const IMAGE_EXTS = [].concat(PLAYABLE_IMAGE_EXTS, []);
  const PLAYABLE_VIDEO_EXTS = ["ogg", "ogm", "ogv", "m4v", "mkv", "mp4", "webm"];
  const VIDEO_EXTS = [].concat(PLAYABLE_VIDEO_EXTS, ["avi",  "wmv"]);
  const PLAYABLE_AUDIO_EXTS = ["aac", "oga", "opus", "m4a", "mka", "mp3", "wav"];
  const AUDIO_EXTS = [].concat(PLAYABLE_AUDIO_EXTS, ["flac", "wma"]);
  const PLAYABLE_TEXT_EXTS = ["txt", "md"];
  const TEXT_EXTS = [].concat(PLAYABLE_TEXT_EXTS, []);
  const PLAYABLE_DOCUMENT_EXTS = ["htm", "html", "pdf"];
  const DOCUMENT_EXTS = [].concat(PLAYABLE_DOCUMENT_EXTS, ["doc", "docx"]);
  const COMPRESSED_EXTS = ["bz2", "gz", "lz", "lz4", "lzma", "xz", "7z", "rar", "tgz", "txz", "rar", "zip"];

  function getIcon(entry) {
    if(entry.homeEntry) return "🏠";
    if(entry.topEntry) return "🔝";
    if(entry.parentEntry) return "⬆️";
    if(entry.directory) return "📁";
    if(entry.file) {
      if(VIDEO_EXTS.includes(entry.ext)) return "🎞️";
      if(IMAGE_EXTS.includes(entry.ext)) return "🖼️";
      if(AUDIO_EXTS.includes(entry.ext)) return "🔊";
      if(TEXT_EXTS.includes(entry.ext)) return "📖";
      if(DOCUMENT_EXTS.includes(entry.ext)) return "📄";
      if(COMPRESSED_EXTS.includes(entry.ext)) return "📦";
      return "📄";
    }

    return "";
  }

  function getType(entry) {
    if(entry.directory) return "Dir";
    return entry.name.slice((Math.max(0, entry.name.lastIndexOf(".")) || Infinity) + 1);
  }

  function getMediaType(entry) {
    if(entry.directory || !entry.ext) return null;

    if(PLAYABLE_IMAGE_EXTS.includes(entry.ext)) return "image";
    if(PLAYABLE_VIDEO_EXTS.includes(entry.ext)) return "video";
    if(PLAYABLE_AUDIO_EXTS.includes(entry.ext)) return "audio";
    if(PLAYABLE_TEXT_EXTS.includes(entry.ext)) return "text";
    if(PLAYABLE_DOCUMENT_EXTS.includes(entry.ext)) return "document";

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
      entry.ext = entry.type?.toLowerCase();
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