# Firebase Authentication Status

## App

Virtual Advocate PI

## Phase

Phase 11 - Android and iOS app MVP

## Task

Integrate Firebase Authentication

## Status

Completed.

## Approach

MAUI MVP uses Firebase Authentication REST email/password sign-in.

Firebase ID token and refresh token are stored in SecureStorage.

Sign-out clears stored Firebase authentication values.

## Files created

maui/VirtualAdvocatePI.Mobile/Services/Auth/FirebaseAuthSessionService.cs.

maui/VirtualAdvocatePI.Mobile/Pages/LoginPage.xaml.

maui/VirtualAdvocatePI.Mobile/Pages/LoginPage.xaml.cs.

## Files updated

maui/VirtualAdvocatePI.Mobile/Configuration/MobileAppSettings.cs.

maui/VirtualAdvocatePI.Mobile/Models/Auth/AuthState.cs.

maui/VirtualAdvocatePI.Mobile/Services/Auth/IAuthSessionService.cs.

maui/VirtualAdvocatePI.Mobile/Services/Auth/MockAuthSessionService.cs.

maui/VirtualAdvocatePI.Mobile/MauiProgram.cs.

maui/VirtualAdvocatePI.Mobile/AppShell.xaml.

maui/VirtualAdvocatePI.Mobile/Pages/HomePage.xaml.

maui/VirtualAdvocatePI.Mobile/Pages/HomePage.xaml.cs.

## Behaviour

App starts on LoginPage.

Users can sign in with Firebase email/password.

HomePage shows signed-in email.

Users can sign out.

Firebase Web API key is configured through MobileAppSettings.

## Build checks

Windows build passed.

Android build passed.

## Next task

Build authenticated API client and token flow.
