export const env = {
  apiBaseUrl: process.env.NEXT_PUBLIC_API_BASE_URL ?? "",

  firebase: {
    apiKey: process.env.NEXT_PUBLIC_FIREBASE_API_KEY ?? "",
    authDomain: process.env.NEXT_PUBLIC_FIREBASE_AUTH_DOMAIN ?? "",
    projectId: process.env.NEXT_PUBLIC_FIREBASE_PROJECT_ID ?? "",
    storageBucket: process.env.NEXT_PUBLIC_FIREBASE_STORAGE_BUCKET ?? "",
    messagingSenderId: process.env.NEXT_PUBLIC_FIREBASE_MESSAGING_SENDER_ID ?? "",
    appId: process.env.NEXT_PUBLIC_FIREBASE_APP_ID ?? "",
  },
};

export function getMissingPublicEnvVars() {
  const requiredValues = {
    NEXT_PUBLIC_API_BASE_URL: env.apiBaseUrl,
    NEXT_PUBLIC_FIREBASE_API_KEY: env.firebase.apiKey,
    NEXT_PUBLIC_FIREBASE_PROJECT_ID: env.firebase.projectId,
    NEXT_PUBLIC_FIREBASE_AUTH_DOMAIN: env.firebase.authDomain,
    NEXT_PUBLIC_FIREBASE_STORAGE_BUCKET: env.firebase.storageBucket,
    NEXT_PUBLIC_FIREBASE_MESSAGING_SENDER_ID: env.firebase.messagingSenderId,
    NEXT_PUBLIC_FIREBASE_APP_ID: env.firebase.appId,
  };

  return Object.entries(requiredValues)
    .filter(([, value]) => !value || value === "REPLACE_ME")
    .map(([key]) => key);
}