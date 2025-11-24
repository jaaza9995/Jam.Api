// Normalizes ASP.NET ValidationProblemDetails / ModelState errors into a flat
// Record with camelCased keys so the frontend can read them consistently.
export function parseBackendErrors(resBody: any): Record<string, string> {
  const result: Record<string, string> = {};

  if (!resBody || typeof resBody !== "object") return result;

  // ASP.NET wraps validation errors in an "errors" property; fall back to root object.
  const errorObj = resBody.errors ?? resBody;

  for (const rawKey of Object.keys(errorObj)) {
    const messages = errorObj[rawKey];
    if (!Array.isArray(messages) || messages.length === 0) continue;

    // Example keys: "Title", "QuestionScenes[0].StoryText"
    const normalizedKey = rawKey
      .split(".")
      .map((part) => {
        const match = part.match(/^([A-Za-z_]+)(.*)$/);
        if (!match) return part;
        const [, name, rest] = match;
        return name.charAt(0).toLowerCase() + name.slice(1) + rest;
      })
      .join(".");

    const firstMessage = messages[0];

    result[normalizedKey] = firstMessage; // first message per key

    // Also expose the leaf key (e.g., "storyText") so screens that don't parse paths still work.
    const leaf = normalizedKey.split(".").pop();
    if (leaf && !result[leaf]) {
      result[leaf] = firstMessage;
    }
  }

  return result;
}
