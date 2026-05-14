export const maxEvidenceUploadBytes = 25 * 1024 * 1024;

export const allowedEvidenceUploadExtensions = [
  ".pdf",
  ".jpg",
  ".jpeg",
  ".png",
  ".webp",
  ".heic",
  ".heif",
  ".doc",
  ".docx",
  ".txt",
  ".rtf",
];

export const allowedEvidenceUploadMimeTypes = [
  "application/pdf",
  "image/jpeg",
  "image/png",
  "image/webp",
  "image/heic",
  "image/heif",
  "application/msword",
  "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
  "text/plain",
  "application/rtf",
  "text/rtf",
];

export const evidenceUploadAcceptValue = [
  ...allowedEvidenceUploadMimeTypes,
  ...allowedEvidenceUploadExtensions,
].join(",");

export function formatEvidenceFileSize(bytes: number) {
  if (bytes >= 1024 * 1024) {
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  return `${Math.max(1, Math.ceil(bytes / 1024))} KB`;
}

export function validateEvidenceUploadFile(file: File | null) {
  if (!file) {
    return "Choose a file before uploading.";
  }

  if (file.size <= 0) {
    return "The selected file appears to be empty. Choose a file with content.";
  }

  if (file.size > maxEvidenceUploadBytes) {
    return `The selected file is too large. Maximum file size is ${formatEvidenceFileSize(maxEvidenceUploadBytes)}.`;
  }

  const lowerName = file.name.toLowerCase();
  const hasAllowedExtension = allowedEvidenceUploadExtensions.some((extension) =>
    lowerName.endsWith(extension),
  );

  if (!hasAllowedExtension) {
    return "This file type is not supported. Upload PDF, image, Word, text or RTF files only.";
  }

  if (file.type && !allowedEvidenceUploadMimeTypes.includes(file.type)) {
    return "This file content type is not supported. Upload PDF, image, Word, text or RTF files only.";
  }

  return null;
}