import { describe, expect, it } from "vitest";
import {
  formatEvidenceFileSize,
  maxEvidenceUploadBytes,
  validateEvidenceUploadFile,
} from "./evidenceUploadValidation";

function makeFile(name: string, sizeBytes: number, type: string) {
  const file = new File([new Uint8Array(Math.max(sizeBytes, 0))], name, { type });
  Object.defineProperty(file, "size", { value: sizeBytes });
  return file;
}

describe("validateEvidenceUploadFile", () => {
  it("rejects a missing file", () => {
    expect(validateEvidenceUploadFile(null)).toBe(
      "Choose a file before uploading.",
    );
  });

  it("rejects an empty file", () => {
    const file = makeFile("report.pdf", 0, "application/pdf");
    expect(validateEvidenceUploadFile(file)).toMatch(/empty/i);
  });

  it("rejects a file larger than the max upload size", () => {
    const file = makeFile("report.pdf", maxEvidenceUploadBytes + 1, "application/pdf");
    expect(validateEvidenceUploadFile(file)).toMatch(/too large/i);
  });

  it("rejects an unsupported extension", () => {
    const file = makeFile("report.exe", 1024, "application/octet-stream");
    expect(validateEvidenceUploadFile(file)).toMatch(/not supported/i);
  });

  it("accepts a supported PDF within the size limit", () => {
    const file = makeFile("report.pdf", 1024, "application/pdf");
    expect(validateEvidenceUploadFile(file)).toBeNull();
  });
});

describe("formatEvidenceFileSize", () => {
  it("formats sizes under 1MB in KB, rounded up", () => {
    expect(formatEvidenceFileSize(1500)).toBe("2 KB");
  });

  it("formats sizes at or over 1MB in MB with one decimal", () => {
    expect(formatEvidenceFileSize(2.5 * 1024 * 1024)).toBe("2.5 MB");
  });
});
